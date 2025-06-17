#if ANDROID

using Android.Media;
using transcribe.io.Services;
using System.Runtime.InteropServices;

namespace transcribe.io.Platforms.Android.Services
{
    public class MicrophoneService_Android : IMicrophoneService
    {
        public event EventHandler<byte[]>? AudioBufferAvailable;

        private AudioRecord? audioRecord;
        private bool isRecording;
        private int channels = 1;
        private int sampleRate = 48000;
        private int bitsPerSample = 16;
        private List<byte> accumulatedAudio = new();
        private CancellationTokenSource? recordingCts;

        public bool IsRecording => isRecording;
        public int Channels => channels;
        public int SampleRate => sampleRate;
        public int BitsPerSample => bitsPerSample;

        public async Task StartCaptureAsync(CancellationToken cancellationToken = default)
        {
            if (isRecording)
                return;

            accumulatedAudio.Clear();

            sampleRate = 48000;
            channels = 1;
            bitsPerSample = 16;

            var channelConfig = ChannelIn.Mono;
            var audioFormat = Encoding.Pcm16bit;
            int minBufferSize = AudioRecord.GetMinBufferSize(sampleRate, channelConfig, audioFormat);
            if (minBufferSize <= 0)
                throw new Exception("Invalid buffer size for AudioRecord");

            audioRecord = new AudioRecord(
                AudioSource.Mic,
                sampleRate,
                channelConfig,
                audioFormat,
                minBufferSize * 2);

            audioRecord.StartRecording();
            isRecording = true;
            recordingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            await Task.Run(() =>
            {
                var buffer = new byte[minBufferSize];
                try
                {
                    while (!recordingCts.Token.IsCancellationRequested)
                    {
                        int read = audioRecord.Read(buffer, 0, buffer.Length);
                        if (read > 0)
                        {
                            var data = new byte[read];
                            Array.Copy(buffer, data, read);
                            accumulatedAudio.AddRange(data);
                            AudioBufferAvailable?.Invoke(this, data);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[MicrophoneService_Android] Exception: {ex}");
                }
            }, recordingCts.Token);
        }

        public Task StopCaptureAsync()
        {
            if (!isRecording)
                return Task.CompletedTask;

            recordingCts?.Cancel();
            recordingCts?.Dispose();
            recordingCts = null;

            audioRecord?.Stop();
            audioRecord?.Release();
            audioRecord = null;
            isRecording = false;

            // You can access the full audio data via accumulatedAudio.ToArray() if needed

            return Task.CompletedTask;
        }
    }
}
#endif