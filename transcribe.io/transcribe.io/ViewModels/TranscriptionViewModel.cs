using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    private readonly object audioBufferLock = new();
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
    private bool isProcessing;
    private List<byte> rawAudioBuffer = new List<byte>();
    private List<byte> processedAudioBuffer = new List<byte>();

    private string liveTranscriptionText = string.Empty;
    public string LiveTranscriptionText
    {
        get => liveTranscriptionText;
        set => SetProperty(ref liveTranscriptionText, value);
    }

    public bool IsProcessing
    {
        get => isProcessing;
        set => SetProperty(ref isProcessing, value);
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
        this.ResetTranscriptionCommand = new Command(ResetTranscription);

        this.microphoneService = services.GetService(typeof(IMicrophoneService)) as IMicrophoneService;
        if (this.microphoneService != null)
        {
            this.microphoneService.AudioBufferAvailable += this.OnAudioBufferAvailable;
        }

        this.ToggleRecordingCommand = new Command(async () => await ToggleRecordingAndTranscribeAsync(), () => !IsProcessing);
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

    public VirtualListViewAdapter<ISubtitleLine> Subtitles { get; }

    public bool IsRecording
    {
        get => this.isRecording;
        set => this.SetProperty(ref this.isRecording, value);
    }

    public ICommand ToggleRecordingCommand { get; }
    
    public ICommand ResetTranscriptionCommand { get; }

    public void OnProgress(double progress)
        => this.Progress = progress;

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
    
    private void ResetTranscription()
    {
        LiveTranscriptionText = string.Empty;
        subtitles.Clear();
        allSegments.Clear();
        committedUntil = TimeSpan.Zero;
        rawAudioBuffer.Clear();
        processedAudioBuffer.Clear();
        OnPropertyChanged(nameof(Subtitles));
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
        if (!File.Exists(filepath))
            return;

        if (!DrasticWhisperFileExtensions.Supported.Contains(Path.GetExtension(filepath)))
            return;

        await this.ParseAsync(filepath, token);
    }

    private async Task ParseAsync(string filepath, CancellationToken token)
    {
        var audioFile = await this.transcodeService.ProcessFile(filepath);
        if (string.IsNullOrEmpty(audioFile) || !File.Exists(audioFile))
            return;

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

    private readonly List<WhisperSegmentData> allSegments = new();
    private TimeSpan committedUntil = TimeSpan.Zero;

    private void OnNewWhisperSegment(object? sender, OnNewSegmentEventArgs segment)
    {
        var e = segment.Segment;
        if (string.IsNullOrWhiteSpace(e.Text))
            return;

        var idx = allSegments.FindIndex(s => s.Start == e.Start && s.End == e.End);
        if (idx >= 0)
            allSegments[idx] = e;
        else
            allSegments.Add(e);

        var ordered = allSegments.OrderBy(s => s.Start).ToList();

        var finalized = ordered.Take(ordered.Count - 1)
            .Where(s => s.End > committedUntil && s.Start >= committedUntil)
            .ToList();

        if (finalized.Count > 0)
            committedUntil = finalized.Max(s => s.End);

        var committedText = string.Join(" ", ordered.Where(s => s.End <= committedUntil).Select(s => s.Text.Trim()));
        var hypothesisText = string.Join(" ", ordered.Where(s => s.End > committedUntil).Select(s => s.Text.Trim()));
        var liveText = string.IsNullOrEmpty(committedText) ? hypothesisText : $"{committedText} {hypothesisText}";

        this.Dispatcher.Dispatch(() =>
        {
            LiveTranscriptionText = liveText;
        });

        //allSegments.RemoveAll(s => s.End <= committedUntil - TimeSpan.FromSeconds(30));
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
            return;

        var subtitle = new SrtSubtitle();
        foreach (var item in this.subtitles)
            subtitle.Lines.Add(item);

        await File.WriteAllTextAsync(filePath, subtitle.ToString());
    }

    // --- Microphone integration and live transcription ---

    private async Task ToggleRecordingAndTranscribeAsync()
    {
        if (IsProcessing)
            return;

        if (!IsRecording)
        {
            await StartRecordingAsync();
        }
        else
        {
            await StopRecordingAndTranscribeAsync();
        }
    }

    public async Task StartRecordingAsync(CancellationToken cancellationToken = default)
    {
        if (this.microphoneService != null && !this.microphoneService.IsRecording)
        {
            if (IsRealTimeTranscription)
            {
                if (!this.whisper.IsInitialized)
                {
                    this.whisper.InitModel(this.modelService.SelectedModel!.FileLocation, this.SelectedLanguage);
                }
                await this.whisper.StartLiveTranscriptionAsync(this.SelectedLanguage, cancellationToken);
            }
            IsRecording = true; // Set before awaiting
            try
            {
                await this.microphoneService.StartCaptureAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                IsRecording = false; // Revert if failed
                this.diagLogger?.LogError(ex, "Failed to start microphone capture");
            }
        }
    }

    private async Task StopRecordingAndTranscribeAsync()
    {
        IsRecording = false;
        IsProcessing = true;

        if (this.microphoneService != null && this.microphoneService.IsRecording)
        {
            await this.microphoneService.StopCaptureAsync();
            await this.whisper.StopLiveTranscriptionAsync();

            // Save raw buffer to WAV file (16-bit PCM, 48kHz, stereo as captured)
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "debug_microphone.wav");
            SaveWavFile(filePath, rawAudioBuffer.ToArray(), 48000, 2);

            // Save processed buffer to WAV file (16kHz, mono)
            string processedPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "debug_microphone_processed.wav");
            SaveWavFile(processedPath, processedAudioBuffer.ToArray(), 16000, 1);

            Console.WriteLine($"[Microphone] Saved audio files to directory: {Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}");

            // If not real-time, transcribe the processed buffer
            if (!IsRealTimeTranscription && processedAudioBuffer.Count > 0)
            {
                string tempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "recorded.wav");
                SaveWavFile(tempPath, processedAudioBuffer.ToArray(), 16000, 1);
                await this.ParseAsync(tempPath, CancellationToken.None);
            }

            rawAudioBuffer.Clear();
            processedAudioBuffer.Clear();
        }

        IsProcessing = false;
    }

    public bool IsRealTimeTranscription
    {
        get => isRealTimeTranscription;
        set => SetProperty(ref isRealTimeTranscription, value);
    }
    private bool isRealTimeTranscription;

    private void SaveWavFile(string filePath, byte[] audioData, int sampleRate, int channels)
    {
        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        int byteRate = sampleRate * channels * 2;
        int blockAlign = channels * 2;
        int subChunk2Size = audioData.Length;
        int chunkSize = 36 + subChunk2Size;

        fs.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        fs.Write(BitConverter.GetBytes(chunkSize), 0, 4);
        fs.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

        fs.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
        fs.Write(BitConverter.GetBytes(16), 0, 4);
        fs.Write(BitConverter.GetBytes((short)1), 0, 2);
        fs.Write(BitConverter.GetBytes((short)channels), 0, 2);
        fs.Write(BitConverter.GetBytes(sampleRate), 0, 4);
        fs.Write(BitConverter.GetBytes(byteRate), 0, 4);
        fs.Write(BitConverter.GetBytes((short)blockAlign), 0, 2);
        fs.Write(BitConverter.GetBytes((short)16), 0, 2);

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

        short[] outputShorts = output.Select(f => (short)(Math.Clamp(f, -1f, 1f) * 32767)).ToArray();
        byte[] outputBuffer = new byte[outputShorts.Length * bytesPerSample];
        Buffer.BlockCopy(outputShorts, 0, outputBuffer, 0, outputBuffer.Length);

        return outputBuffer;
    }

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
        int numSamples = inputBuffer.Length / bytesPerSample / 2;
        short[] monoSamples = new short[numSamples];
        for (int i = 0; i < numSamples; i++)
        {
            short left = ReadInt16LE(inputBuffer, i * 4);
            short right = ReadInt16LE(inputBuffer, i * 4 + 2);
            monoSamples[i] = (short)((left + right) / 2);
        }
        byte[] monoBuffer = new byte[numSamples * bytesPerSample];
        Buffer.BlockCopy(monoSamples, 0, monoBuffer, 0, monoBuffer.Length);

        return monoBuffer;
    }

    private void OnAudioBufferAvailable(object? sender, byte[] buffer)
    {
        int inputChannels = this.microphoneService?.Channels ?? 1;
        int inputSampleRate = this.microphoneService?.SampleRate ?? 48000;
        int bytesPerSample = 2;
        int frameSize = inputChannels * bytesPerSample;

        if (buffer.Length % frameSize != 0)
        {
            this.diagLogger?.LogWarning("Audio buffer misaligned: length {Length}, expected multiple of {FrameSize}", buffer.Length, frameSize);
            return;
        }

        lock (audioBufferLock)
        {
            rawAudioBuffer.AddRange(buffer);

            byte[] monoBuffer = buffer;
            if (inputChannels == 2)
                monoBuffer = DownmixToMono(buffer);

            var resampled = ResampleTo16kHz(monoBuffer, inputSampleRate, 16000);
            processedAudioBuffer.AddRange(resampled);

            _ = Task.Run(async () =>
            {
                try
                {
                    await this.whisper.ProcessAudioBufferAsync(resampled);
                }
                catch (Exception ex)
                {
                    this.diagLogger?.LogError(ex, "Error processing audio buffer");
                }
            });
        }
    }
}