using System.Collections.ObjectModel;
using System.Windows.Input;
using Drastic.Tools;
using Drastic.ViewModels;
using transcribe.io.Models;
using transcribe.io.Services;
using transcribe.io.Tools;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Adapters;

namespace transcribe.io.ViewModels;

public class TranscriptionViewModel : BaseViewModel, IDisposable
{
    private WhisperModelService modelService;
    private IWhisperService whisper;
    private double progress;
    private ILogger? diagLogger;
    private bool disposedValue;
    private ITranscodeService transcodeService;
    private CancellationTokenSource cts;
    private WhisperLanguage selectedLanguage;
    private string? urlField;
    private bool canStart = true;
    private List<ISubtitleLine> subtitles = new List<ISubtitleLine>();
    private YouTubeService youTubeService;
    private IMicrophoneService? microphoneService;
    private bool isRecording;
    private List<byte> rawAudioBuffer = new List<byte>();
    private List<byte> processedAudioBuffer = new List<byte>();

    // --- Live transcription text property ---
    private string liveTranscriptionText = string.Empty;
    public string LiveTranscriptionText
    {
        get => liveTranscriptionText;
        set => SetProperty(ref liveTranscriptionText, value);
    }

    public TranscriptionViewModel(IServiceProvider services)
        : base(services)
    {
        this.youTubeService = services.GetRequiredService<YouTubeService>();
        this.diagLogger = services.GetService<ILogger>();
        this.modelService = services.GetService(typeof(WhisperModelService)) as WhisperModelService ??
                            throw new NullReferenceException(nameof(WhisperModelService));
        this.whisper = this.Services.GetRequiredService<IWhisperService>()!;
        this.transcodeService = this.Services.GetRequiredService<ITranscodeService>();
        this.whisper.OnProgress = this.OnProgress;
        this.whisper.OnNewWhisperSegment += this.OnNewWhisperSegment;
        this.modelService.OnUpdatedSelectedModel += this.ModelServiceOnUpdatedSelectedModel;
        this.modelService.OnAvailableModelsUpdate += this.ModelServiceOnAvailableModelsUpdate;
        this.WhisperLanguages = WhisperLanguage.GenerateWhisperLangauages();
        this.selectedLanguage = this.WhisperLanguages[0];
        this.cts = new CancellationTokenSource();
        this.StartCommand = new AsyncCommand(this.StartAsync, () => this.canStart, this.Dispatcher, this.ErrorHandler);
        this.ExportCommand = new AsyncCommand<string>(this.ExportAsync, null, this.ErrorHandler);
        this.Subtitles = new VirtualListViewAdapter<ISubtitleLine>(this.subtitles);

        // Inject IMicrophoneService if available
        this.microphoneService = services.GetService(typeof(IMicrophoneService)) as IMicrophoneService;
        if (this.microphoneService != null)
        {
            this.microphoneService.AudioBufferAvailable += this.OnAudioBufferAvailable;
        }

        this.ToggleRecordingCommand = new Command(async () => await ToggleRecordingAsync());
    }

    public TranscriptionViewModel(string srtText, IServiceProvider services)
        : this(services)
    {
        var subtitle = new SrtSubtitle(srtText);
        this.subtitles.Clear();
        this.subtitles.AddRange(subtitle.Lines);
    }

    public IWhisperService Whisper => this.whisper;

    public WhisperModelService ModelService => this.modelService;

    public AsyncCommand StartCommand { get; }

    public AsyncCommand<string> ExportCommand { get; }

    public IReadOnlyList<WhisperLanguage> WhisperLanguages { get; }

    public WhisperLanguage SelectedLanguage
    {
        get => this.selectedLanguage;
        set
        {
            this.SetProperty(ref this.selectedLanguage, value);
            this.RaiseCanExecuteChanged();
        }
    }

    public string? UrlField
    {
        get => this.urlField;
        set
        {
            this.SetProperty(ref this.urlField, value);
            this.RaiseCanExecuteChanged();
        }
    }

    public double Progress
    {
        get => this.progress;
        set => this.SetProperty(ref this.progress, value);
    }

    /// <summary>
    /// Gets the subtitles.
    /// </summary>
    public VirtualListViewAdapter<ISubtitleLine> Subtitles { get; }

    public bool IsRecording
    {
        get => this.isRecording;
        set => this.SetProperty(ref this.isRecording, value);
    }

    public ICommand ToggleRecordingCommand { get; }

    public void OnProgress(double progress)
        => this.Progress = progress;

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                this.modelService.OnUpdatedSelectedModel -= this.ModelServiceOnUpdatedSelectedModel;
                this.whisper.OnNewWhisperSegment -= this.OnNewWhisperSegment;
                if (this.microphoneService != null)
                {
                    this.microphoneService.AudioBufferAvailable -= this.OnAudioBufferAvailable;
                }
            }

            this.disposedValue = true;
        }
    }

    private async Task StartAsync()
    {
        this.subtitles.Clear();
        this.LiveTranscriptionText = string.Empty;

        ArgumentNullException.ThrowIfNull(nameof(this.UrlField));
        ArgumentNullException.ThrowIfNull(nameof(this.modelService.SelectedModel));

        if (this.youTubeService.IsValidUrl(this.urlField ?? string.Empty))
        {
            await this.ParseAsync(await this.youTubeService.GetAudioUrlAsync(this.UrlField!), this.cts?.Token ?? CancellationToken.None);
        }
        else if (File.Exists(this.UrlField))
        {
            await this.LocalFileParseAsync(this.UrlField!, this.cts.Token);
        }
    }

    private async Task LocalFileParseAsync(string filepath, CancellationToken token)
    {
        string? audioFile = string.Empty;

        if (!File.Exists(filepath))
        {
            return;
        }

        if (!DrasticWhisperFileExtensions.Supported.Contains(Path.GetExtension(filepath)))
        {
            return;
        }

        await this.ParseAsync(filepath, token);
    }

    private async Task ParseAsync(string filepath, CancellationToken token)
    {
        string? audioFile = string.Empty;

        audioFile = await this.transcodeService.ProcessFile(filepath);
        if (string.IsNullOrEmpty(audioFile) || !File.Exists(audioFile))
        {
            return;
        }

        await this.PerformBusyAsyncTask(
            async () => { await this.GenerateCaptionsAsync(audioFile, token); },
            "Generating Subtitles");
    }

    private Task GenerateCaptionsAsync(string audioFile, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(nameof(this.modelService.SelectedModel));

        return this.PerformBusyAsyncTask(
            async () =>
            {
                this.whisper.InitModel(this.modelService.SelectedModel!.FileLocation, this.SelectedLanguage);

                await this.whisper.ProcessAsync(audioFile, token);
            },
            "Generating Subtitles");
    }

    // --- Update: accumulate live transcription text ---
    private void OnNewWhisperSegment(object? sender, OnNewSegmentEventArgs segment)
    {
        var e = segment.Segment;

        // Accumulate text with a space between chunks
        this.Dispatcher.Dispatch(() =>
        {
            if (!string.IsNullOrWhiteSpace(e.Text))
            {
                if (!string.IsNullOrEmpty(LiveTranscriptionText))
                    LiveTranscriptionText += " ";
                LiveTranscriptionText += e.Text.Trim();
            }
        });

        // Optionally, still add to subtitles if you want to keep the list:
        // var item = new SrtSubtitleLine()
        // { Start = e.Start, End = e.End, Text = e.Text.Trim(), LineNumber = this.subtitles.Count() + 1 };
        // this.Dispatcher.Dispatch(() =>
        // {
        //     this.subtitles.Add(item);
        //     this.Subtitles.InvalidateData();
        // });
    }

    private void ModelServiceOnUpdatedSelectedModel(object? sender, EventArgs e)
    {
        this.RaiseCanExecuteChanged();
    }

    private void ModelServiceOnAvailableModelsUpdate(object? sender, EventArgs e)
    {
        this.RaiseCanExecuteChanged();
    }

    private async Task ExportAsync(string filePath)
    {
        if (!this.subtitles.Any())
        {
            return;
        }

        var subtitle = new SrtSubtitle();
        foreach (var item in this.subtitles)
        {
            subtitle.Lines.Add(item);
        }

        await File.WriteAllTextAsync(filePath, subtitle.ToString());
    }

    // --- Microphone integration and live transcription ---

    private async Task ToggleRecordingAsync()
    {
        if (IsRecording)
        {
            await StopRecordingAsync();
        }
        else
        {
            await StartRecordingAsync();
        }
    }

    public async Task StartRecordingAsync(CancellationToken cancellationToken = default)
    {
        if (this.microphoneService != null && !this.microphoneService.IsRecording)
        {
            // Initialize model for live transcription if needed
            if (!this.whisper.IsInitialized)
            {
                this.whisper.InitModel(this.modelService.SelectedModel!.FileLocation, this.SelectedLanguage);
            }
            await this.whisper.StartLiveTranscriptionAsync(this.SelectedLanguage, cancellationToken);
            Console.WriteLine("[Microphone] StartLiveTranscriptionAsync completed");
            await this.microphoneService.StartCaptureAsync(cancellationToken);
            IsRecording = true;
        }
    }

    public async Task StopRecordingAsync()
    {
        if (this.microphoneService != null && this.microphoneService.IsRecording)
        {
            await this.microphoneService.StopCaptureAsync();
            await this.whisper.StopLiveTranscriptionAsync();
            IsRecording = false;

            // Log buffer stats
            Console.WriteLine($"[Microphone] Stopping recording. Raw buffer length: {rawAudioBuffer.Count} bytes, Processed buffer length: {processedAudioBuffer.Count} bytes");

            // Log first 32 bytes of raw buffer
            Console.WriteLine("[Microphone] First 32 bytes of raw buffer: " + BitConverter.ToString(rawAudioBuffer.Take(32).ToArray()));

            // Save raw buffer to WAV file (16-bit PCM, 48kHz, stereo as captured)
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "debug_microphone.wav");
            SaveWavFile(filePath, rawAudioBuffer.ToArray(), 48000, 2);
            Console.WriteLine($"[Microphone] Saved raw audio to {filePath}");

            // Save processed buffer to WAV file (16kHz, mono)
            string processedPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "debug_microphone_processed.wav");
            SaveWavFile(processedPath, processedAudioBuffer.ToArray(), 16000, 1);
            Console.WriteLine($"[Microphone] Saved processed audio to {processedPath}");

            rawAudioBuffer.Clear();
            processedAudioBuffer.Clear();
        }
    }

    private void SaveWavFile(string filePath, byte[] audioData, int sampleRate, int channels)
    {
        Console.WriteLine($"[WAV] Saving file: {filePath}, Length: {audioData.Length}, SampleRate: {sampleRate}, Channels: {channels}");
        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        int byteRate = sampleRate * channels * 2;
        int blockAlign = channels * 2;
        int subChunk2Size = audioData.Length;
        int chunkSize = 36 + subChunk2Size;

        // RIFF header
        fs.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        fs.Write(BitConverter.GetBytes(chunkSize), 0, 4);
        fs.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

        // fmt subchunk
        fs.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
        fs.Write(BitConverter.GetBytes(16), 0, 4); // Subchunk1Size
        fs.Write(BitConverter.GetBytes((short)1), 0, 2); // AudioFormat = PCM
        fs.Write(BitConverter.GetBytes((short)channels), 0, 2);
        fs.Write(BitConverter.GetBytes(sampleRate), 0, 4);
        fs.Write(BitConverter.GetBytes(byteRate), 0, 4);
        fs.Write(BitConverter.GetBytes((short)blockAlign), 0, 2);
        fs.Write(BitConverter.GetBytes((short)16), 0, 2); // BitsPerSample

        // data subchunk
        fs.Write(System.Text.Encoding.ASCII.GetBytes("data"));
        fs.Write(BitConverter.GetBytes(subChunk2Size), 0, 4);
        fs.Write(audioData, 0, audioData.Length);
    }

    private byte[] ResampleTo16kHz(byte[] inputBuffer, int inputSampleRate, int outputSampleRate)
    {
        int bytesPerSample = 2;
        int inputSamples = inputBuffer.Length / bytesPerSample;
        short[] inputShorts = new short[inputSamples];
        Buffer.BlockCopy(inputBuffer, 0, inputShorts, 0, inputBuffer.Length);

        float[] input = inputShorts.Select(s => s / 32768f).ToArray();

        int outputSamples = (int)((long)inputSamples * outputSampleRate / inputSampleRate);
        float[] output = new float[outputSamples];

        for (int i = 0; i < outputSamples; i++)
        {
            float srcIndex = (float)i * inputSampleRate / outputSampleRate;
            int idx = (int)srcIndex;
            float frac = srcIndex - idx;

            if (idx + 1 < inputSamples)
                output[i] = input[idx] * (1 - frac) + input[idx + 1] * frac;
            else
                output[i] = input[idx];
        }

        // Convert back to 16-bit PCM
        short[] outputShorts = output.Select(f => (short)(Math.Clamp(f, -1f, 1f) * 32767)).ToArray();
        byte[] outputBuffer = new byte[outputShorts.Length * bytesPerSample];
        Buffer.BlockCopy(outputShorts, 0, outputBuffer, 0, outputBuffer.Length);

        Console.WriteLine($"[Resample] Input samples: {inputSamples}, Output samples: {outputSamples}, Input length: {inputBuffer.Length}, Output length: {outputBuffer.Length}");
        Console.WriteLine("[Resample] First 16 output samples: " + string.Join(", ", outputShorts.Take(16)));

        return outputBuffer;
    }

    // Helper to always read 16-bit samples as little-endian
    private short ReadInt16LE(byte[] buffer, int offset)
    {
        if (BitConverter.IsLittleEndian)
            return BitConverter.ToInt16(buffer, offset);
        else
            return (short)((buffer[offset]) | (buffer[offset + 1] << 8));
    }

    private byte[] DownmixToMono(byte[] inputBuffer)
    {
        int bytesPerSample = 2;
        int numSamples = inputBuffer.Length / bytesPerSample / 2; // 2 channels
        short[] monoSamples = new short[numSamples];
        for (int i = 0; i < numSamples; i++)
        {
            short left = ReadInt16LE(inputBuffer, i * 4);
            short right = ReadInt16LE(inputBuffer, i * 4 + 2);
            monoSamples[i] = (short)((left + right) / 2);
        }
        byte[] monoBuffer = new byte[numSamples * bytesPerSample];
        Buffer.BlockCopy(monoSamples, 0, monoBuffer, 0, monoBuffer.Length);

        Console.WriteLine($"[Downmix] Stereo input length: {inputBuffer.Length}, Mono output length: {monoBuffer.Length}");
        Console.WriteLine("[Downmix] First 16 mono samples: " + string.Join(", ", monoSamples.Take(16)));

        return monoBuffer;
    }

    private async void OnAudioBufferAvailable(object? sender, byte[] buffer)
    {
        Console.WriteLine($"[Microphone] Received audio buffer of {buffer?.Length ?? 0} bytes");
        if (buffer != null && buffer.Length > 0)
        {
            Console.WriteLine("[Microphone] First 16 bytes: " + BitConverter.ToString(buffer.Take(16).ToArray()));
            for (int i = 0; i < Math.Min(8, buffer.Length / 2); i++)
            {
                short sample = ReadInt16LE(buffer, i * 2);
                Console.WriteLine($"[Microphone] Sample {i}: {sample}");
            }
        }

        rawAudioBuffer.AddRange(buffer);

        try
        {
            byte[] monoBuffer = buffer;
            if (buffer.Length % 4 == 0)
            {
                monoBuffer = DownmixToMono(buffer);
            }

            var resampled = ResampleTo16kHz(monoBuffer, 48000, 16000);
            processedAudioBuffer.AddRange(resampled);

            Console.WriteLine($"[Microphone] Processed buffer: raw {buffer.Length} bytes, mono {monoBuffer.Length} bytes, resampled {resampled.Length} bytes");

            await this.whisper.ProcessAudioBufferAsync(resampled);
        }
        catch (Exception ex)
        {
            this.diagLogger?.LogError(ex, "Error processing audio buffer");
            Console.WriteLine($"[Microphone] Exception: {ex}");
        }
    }
}