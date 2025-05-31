// <copyright file="IWhisperService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using transcribe.io.Models;

namespace transcribe.io.Services;

public interface IWhisperService
{
    // Start a live transcription session
    Task StartLiveTranscriptionAsync(WhisperLanguage language, CancellationToken? cancellationToken = default);

    // Process a single audio buffer (e.g., from microphone)
    Task ProcessAudioBufferAsync(byte[] buffer, CancellationToken? cancellationToken = default);

    // Stop the live transcription session
    Task StopLiveTranscriptionAsync();
    
    event EventHandler<OnNewSegmentEventArgs>? OnNewWhisperSegment;

    public Action<double>? OnProgress { get; set; }

    bool IsInitialized { get; }

    bool IsIndeterminate { get; }

    Task ProcessAsync(string filePath, CancellationToken? cancellationToken = default);

    Task ProcessAsync(byte[] buffer, CancellationToken? cancellationToken = default);

    Task ProcessAsync(Stream stream, CancellationToken? cancellationToken = default);

    Task ProcessBytes(byte[] bytes, CancellationToken? cancellationToken = default);

    void InitModel(string path, WhisperLanguage lang);

    void InitModel(byte[] buffer, WhisperLanguage lang);
}

public class OnNewSegmentEventArgs : EventArgs
{
    public OnNewSegmentEventArgs(WhisperSegmentData segmentData)
    {
        this.Segment = segmentData;
    }

    public WhisperSegmentData Segment { get; }
}