// File: transcribe.io/Services/IMicrophoneService_iOS.cs

using System;
using System.Threading;
using System.Threading.Tasks;
using AVFoundation;
using Foundation;
using transcribe.io.Services;

namespace transcribe.io.Platforms.iOS
{
    public class IMicrophoneService_iOS : IMicrophoneService
    {
        public event EventHandler<byte[]>? AudioBufferAvailable;

        private AVAudioEngine? engine;
        private bool isRecording;

        public bool IsRecording => isRecording;

        public async Task StartCaptureAsync(CancellationToken cancellationToken = default)
        {
            if (isRecording)
                return;

            var session = AVAudioSession.SharedInstance();
            session.SetCategory(AVAudioSessionCategory.Record);
            session.SetActive(true, out _);

            engine = new AVAudioEngine();
            var input = engine.InputNode;
            var format = input.GetBusOutputFormat(0); // Use hardware format (likely 48kHz, 1ch)

            int bufferCount = 0;
            input.InstallTapOnBus(0, 1600, format, (buffer, when) =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    StopCaptureAsync().Wait();
                    return;
                }

                var audioBuffer = buffer.AudioBufferList[0];
                int bytesPerSample = (int)(format.StreamDescription.BytesPerFrame / format.ChannelCount);
                var data = new byte[buffer.FrameLength * bytesPerSample * format.ChannelCount];
                System.Runtime.InteropServices.Marshal.Copy(audioBuffer.Data, data, 0, data.Length);

                bufferCount++;
                if (bufferCount % 10 == 0)
                    Console.WriteLine($"[Microphone] Buffer #{bufferCount}, Length: {data.Length} bytes, SampleRate: {format.SampleRate}");

                if (data.Length > 0)
                    AudioBufferAvailable?.Invoke(this, data);
            });

            engine.Prepare();
            NSError? error;
            engine.StartAndReturnError(out error);
            if (error != null)
                throw new Exception(error.LocalizedDescription);

            isRecording = true;
        }

        public Task StopCaptureAsync()
        {
            if (!isRecording)
                return Task.CompletedTask;

            engine?.InputNode.RemoveTapOnBus(0);
            engine?.Stop();
            AVAudioSession.SharedInstance().SetActive(false, out _);

            isRecording = false;
            return Task.CompletedTask;
        }
    }
}