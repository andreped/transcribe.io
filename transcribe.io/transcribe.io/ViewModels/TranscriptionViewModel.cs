// <copyright file="TranscriptionViewModel.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
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
        get
        {
            return this.selectedLanguage;
        }

        set
        {
            this.SetProperty(ref this.selectedLanguage, value);
            this.RaiseCanExecuteChanged();
        }
    }

    public string? UrlField
    {
        get
        {
            return this.urlField;
        }

        set
        {
            this.SetProperty(ref this.urlField, value);
            this.RaiseCanExecuteChanged();
        }
    }

    public double Progress
    {
        get { return this.progress; }

        set { this.SetProperty(ref this.progress, value); }
    }

    /// <summary>
    /// Gets the subtitles.
    /// </summary>
    public VirtualListViewAdapter<ISubtitleLine> Subtitles { get; }

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
            }

            this.disposedValue = true;
        }
    }

    private async Task StartAsync()
    {
        this.subtitles.Clear();

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

    private void OnNewWhisperSegment(object? sender, OnNewSegmentEventArgs segment)
    {
        var e = segment.Segment;
        this.diagLogger?.LogDebug($"CSSS {e.Start} ==> {e.End} : {e.Text}");

        var item = new SrtSubtitleLine()
        { Start = e.Start, End = e.End, Text = e.Text.Trim(), LineNumber = this.subtitles.Count() + 1 };

        this.Dispatcher.Dispatch(() =>
        {
            this.subtitles.Add(item);
            this.Subtitles.InvalidateData();
        });
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
}