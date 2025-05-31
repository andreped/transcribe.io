using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AVFoundation;
using Foundation;
using transcribe.io.Services;
using System.Runtime.InteropServices;

namespace transcribe.io.Platforms.iOS
{
    public class MicrophoneService_iOS : IMicrophoneService
    {
        public event EventHandler<byte[]>? AudioBufferAvailable;

        private AVAudioEngine? engine;
        private bool isRecording;
        private int channels = 1;
        private int sampleRate = 48000;
        private int bitsPerSample = 16;
        private List<byte> accumulatedAudio = new();

        public bool IsRecording => isRecording;
        public int Channels => channels;
        public int SampleRate => sampleRate;
        public int BitsPerSample => bitsPerSample;

        public async Task StartCaptureAsync(CancellationToken cancellationToken = default)
        {
            if (isRecording)
                return;

            accumulatedAudio.Clear();

            var session = AVAudioSession.SharedInstance();
            session.SetCategory(AVAudioSessionCategory.Record);
            session.SetActive(true, out _);

            engine = new AVAudioEngine();
            var input = engine.InputNode;
            var format = input.GetBusOutputFormat(0);

            channels = (int)format.ChannelCount;
            sampleRate = (int)format.SampleRate;
            bitsPerSample = 16; // Always output 16-bit PCM

            input.InstallTapOnBus(0, 1600, format, (buffer, when) =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    StopCaptureAsync().Wait();
                    return;
                }

                byte[] data;

                if (format.CommonFormat == AVAudioCommonFormat.PCMFloat32)
                {
                    unsafe
                    {
                        // Get pointer to the first channel's float data
                        var floatChannelDataPtr = buffer.FloatChannelData;
                        if (floatChannelDataPtr == IntPtr.Zero)
                        {
                            data = Array.Empty<byte>();
                        }
                        else
                        {
                            float* floatBuffer = (float*)Marshal.ReadIntPtr(floatChannelDataPtr);
                            int sampleCount = (int)buffer.FrameLength * (int)format.ChannelCount;
                            data = new byte[sampleCount * 2];
                            for (int i = 0; i < sampleCount; i++)
                            {
                                short s = (short)Math.Clamp(floatBuffer[i] * 32767f, -32768, 32767);
                                data[i * 2] = (byte)(s & 0xFF);
                                data[i * 2 + 1] = (byte)((s >> 8) & 0xFF);
                            }
                        }
                    }
                }
                else
                {
                    // Use raw PCM data
                    int frameLength = (int)buffer.FrameLength;
                    int bytesPerFrame = (int)format.StreamDescription.BytesPerFrame;
                    int dataLength = frameLength * bytesPerFrame;
                    data = new byte[dataLength];

                    if (buffer.AudioBufferList.Count > 0)
                    {
                        var audioBuffer = buffer.AudioBufferList[0];
                        Marshal.Copy(audioBuffer.Data, data, 0, data.Length);
                    }
                }

                if (data.Length > 0)
                {
                    accumulatedAudio.AddRange(data);
                    AudioBufferAvailable?.Invoke(this, data);
                }
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

            // You can access the full audio data via accumulatedAudio.ToArray() if needed

            return Task.CompletedTask;
        }
    }
}