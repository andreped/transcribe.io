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
            var format = new AVAudioFormat(16000, 1);

            input.InstallTapOnBus(0, 1600, format, (buffer, when) =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    StopCaptureAsync().Wait();
                    return;
                }

                var audioBuffer = buffer.AudioBufferList[0];
                var data = new byte[buffer.FrameLength * 2];
                System.Runtime.InteropServices.Marshal.Copy(audioBuffer.Data, data, 0, data.Length);

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