// <copyright file="IWhisperService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using transcribe.io.Models;

namespace transcribe.io.Services;

public interface IWhisperService
{
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