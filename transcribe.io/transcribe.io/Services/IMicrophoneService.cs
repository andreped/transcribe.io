// File: transcribe.io/Services/IMicrophoneService.cs

using System;
using System.Threading;
using System.Threading.Tasks;

namespace transcribe.io.Services
{
    public interface IMicrophoneService
    {
        /// <summary>
        /// Raised when a new audio buffer is available.
        /// </summary>
        event EventHandler<byte[]> AudioBufferAvailable;

        /// <summary>
        /// Starts capturing audio from the microphone.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        Task StartCaptureAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stops capturing audio from the microphone.
        /// </summary>
        Task StopCaptureAsync();
        
        /// <summary>
        /// Indicates if the microphone is currently recording.
        /// </summary>
        bool IsRecording { get; }
    }
}