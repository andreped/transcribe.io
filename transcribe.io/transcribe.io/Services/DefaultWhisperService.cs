// <copyright file="DefaultWhisperService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using transcribe.io.Models;
using Whisper.net;

namespace transcribe.io.Services;

public class DefaultWhisperService : IWhisperService, IDisposable
{
    private const int AudioSampleLengthS = 1;
    private const int TotalBufferLength = 30 / AudioSampleLengthS;
    private bool disposedValue;
    private WhisperFactory? factory;
    private WhisperProcessor? processor;
    private List<float[]> slidingBuffer = new(TotalBufferLength + 1);

    /// <inheritdoc/>
    public event EventHandler<OnNewSegmentEventArgs>? OnNewWhisperSegment;

    /// <inheritdoc/>
    public bool IsInitialized => this.processor is not null;

    /// <inheritdoc/>
    public bool IsIndeterminate => true;

    /// <inheritdoc/>
    public Action<double>? OnProgress { get; set; }

    /// <inheritdoc/>
    void IDisposable.Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public void InitModel(string path, WhisperLanguage language)
    {
        this.processor?.Dispose();
        this.factory = WhisperFactory.FromPath(path);
        this.processor = this.SetupProcessor(this.factory, language);
    }

    /// <inheritdoc/>
    public void InitModel(byte[] buffer, WhisperLanguage language)
    {
        this.factory = WhisperFactory.FromBuffer(buffer);
        this.processor = this.SetupProcessor(this.factory, language);
    }

    /// <inheritdoc/>
    public Task ProcessAsync(string filePath, CancellationToken? cancellationToken = null)
    {
        ArgumentNullException.ThrowIfNull(this.processor);

        return Task.Run(
            () =>
            {
                using var fileStream = File.OpenRead(filePath);
                this.processor.Process(fileStream);
            },
            cancellationToken ?? CancellationToken.None);
    }

    /// <inheritdoc/>
    public Task ProcessAsync(byte[] buffer, CancellationToken? cancellationToken = null)
    {
        return this.ProcessAsync(new MemoryStream(buffer), cancellationToken);
    }

    /// <inheritdoc/>
    public Task ProcessAsync(Stream stream, CancellationToken? cancellationToken = null)
    {
        ArgumentNullException.ThrowIfNull(this.processor);

        return Task.Run(
            () => { this.processor.Process(stream); },
            cancellationToken ?? CancellationToken.None);
    }

    /// <inheritdoc/>
    public Task ProcessBytes(byte[] e, CancellationToken? cancellationToken = null)
    {
        ArgumentNullException.ThrowIfNull(this.processor);

        var values = new short[e.Length / 2];
        Buffer.BlockCopy(e, 0, values, 0, e.Length);
        var samples = values.Select(x => x / (short.MaxValue + 1f)).ToArray();

        var silenceCount = samples.Count(x => IsSilence(x, -40));

        if (silenceCount < values.Length - (values.Length / 12))
        {
            this.slidingBuffer.Add(samples);

            if (this.slidingBuffer.Count > TotalBufferLength)
            {
                this.slidingBuffer.RemoveAt(0);
            }

            this.processor.Process(this.slidingBuffer.SelectMany(x => x).ToArray());
        }

        return Task.CompletedTask;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                this.factory?.Dispose();
                this.processor?.Dispose();
            }

            this.disposedValue = true;
        }
    }

    private static bool IsSilence(float amplitude, sbyte threshold)
        => GetDecibelsFromAmplitude(amplitude) < threshold;

    private WhisperProcessor SetupProcessor(WhisperFactory factory, WhisperLanguage language)
    {
        int max_threads = Math.Min(8, Environment.ProcessorCount);
        var builder = factory.CreateBuilder();
        if (language.IsAutomatic)
        {
            builder.WithLanguage("auto");
        }
        else
        {
            builder.WithLanguage(language.LanguageCode);
        }

        return builder.WithSegmentEventHandler(this.OnNewSegment).WithThreads(max_threads)
            .Build();
    }

    private void OnNewSegment(SegmentData e)
    {
        this.OnNewWhisperSegment?.Invoke(
            this,
            new OnNewSegmentEventArgs(new Models.WhisperSegmentData(e.Text, e.Start, e.End, e.MinProbability,
                e.MaxProbability, e.Probability, e.Language)));
    }

    private static double GetDecibelsFromAmplitude(float amplitude) => 20 * Math.Log10(Math.Abs(amplitude));
}