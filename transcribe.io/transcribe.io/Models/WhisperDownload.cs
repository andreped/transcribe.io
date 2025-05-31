// <copyright file="WhisperDownload.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Downloader;
using Drastic.Services;
using Drastic.Tools;
using transcribe.io.Services;

namespace transcribe.io.Models;

public class WhisperDownload : INotifyPropertyChanged, IDisposable, IErrorHandlerService
{
    private DownloadService download;
    private CancellationTokenSource source;
    private bool downloadStarted;
    private double precent;
    private bool disposedValue;
    private IAppDispatcher dispatcher;
    private WhisperModelService service;

    public WhisperDownload(WhisperModel model, WhisperModelService service, IAppDispatcher dispatcher)
    {
        this.service = service;
        this.dispatcher = dispatcher;
        this.Model = model;

        this.download = new DownloadService(new DownloadConfiguration()
        {
            ChunkCount = 8,
            ParallelDownload = true,
        });

        this.download.DownloadStarted += this.Download_DownloadStarted;
        this.download.DownloadFileCompleted += this.Download_DownloadFileCompleted;
        this.download.DownloadProgressChanged += this.Download_DownloadProgressChanged;

        this.source = new CancellationTokenSource();

        this.DownloadCommand = new AsyncCommand(this.DownloadAsync, () => !string.IsNullOrEmpty(this.Model.DownloadUrl),
            this.dispatcher, this);
        this.CancelCommand = new AsyncCommand(this.CancelAsync, null, this.dispatcher, this);
        this.DeleteCommand = new AsyncCommand(this.DeleteAsync, null, this.dispatcher, this);
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    public WhisperModel Model { get; private set; }

    public DownloadService DownloadService => this.download;

    public double Precent
    {
        get { return this.precent; }
        set { this.SetProperty(ref this.precent, value); }
    }

    public bool DownloadStarted
    {
        get { return this.downloadStarted; }
        set { this.SetProperty(ref this.downloadStarted, value); }
    }

    public bool ShowDownloadButton => !this.Model.Exists && !this.DownloadStarted;

    public bool ShowCancelButton => this.DownloadStarted;

    public bool ShowDeleteButton => this.Model.Exists && !this.DownloadStarted;

    public AsyncCommand DownloadCommand { get; }

    public AsyncCommand CancelCommand { get; }

    public AsyncCommand DeleteCommand { get; }

    /// <inheritdoc/>
    void IDisposable.Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void UpdateButtons()
    {
        this.OnPropertyChanged(nameof(this.ShowDownloadButton));
        this.OnPropertyChanged(nameof(this.ShowCancelButton));
        this.OnPropertyChanged(nameof(this.ShowDeleteButton));

        this.DownloadCommand.RaiseCanExecuteChanged();
        this.CancelCommand.RaiseCanExecuteChanged();
        this.DeleteCommand.RaiseCanExecuteChanged();

        this.service.UpdateAvailableModels();
    }

    private void Download_DownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
    {
        this.Precent = e.ProgressPercentage / 100;
    }

    private void Download_DownloadFileCompleted(object? sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        this.DownloadStarted = false;
        if (e.Cancelled && e.UserState is Downloader.DownloadPackage package)
        {
            this.DeleteAsync().FireAndForgetSafeAsync();
        }

        this.UpdateButtons();
    }

    /// <inheritdoc/>
    public void HandleError(Exception ex)
    {
    }

    /// <summary>
    /// On Property Changed.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        this.dispatcher?.Dispatch(() =>
        {
            var changed = this.PropertyChanged;
            if (changed == null)
            {
                return;
            }

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        });
    }

#pragma warning disable SA1600 // Elements should be documented
    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "",
        Action? onChanged = null)
#pragma warning restore SA1600 // Elements should be documented
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
        {
            return false;
        }

        backingStore = value;
        onChanged?.Invoke();
        this.OnPropertyChanged(propertyName);
        return true;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                this.source?.Cancel();
                this.download.DownloadStarted -= this.Download_DownloadStarted;
                this.download.DownloadFileCompleted -= this.Download_DownloadFileCompleted;
                this.download.DownloadProgressChanged -= this.Download_DownloadProgressChanged;
                this.download.Dispose();
            }

            this.disposedValue = true;
        }
    }

    private void Download_DownloadStarted(object? sender, DownloadStartedEventArgs e)
    {
        this.DownloadStarted = true;
        this.UpdateButtons();
    }

    private async Task DownloadAsync()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(this.Model.FileLocation)!);
        await this.download.DownloadFileTaskAsync(this.Model.DownloadUrl, this.Model.FileLocation, this.source.Token);
    }

    private async Task CancelAsync()
    {
        this.download.CancelAsync();
        this.UpdateButtons();
    }

    private async Task DeleteAsync()
    {
        if (File.Exists(this.Model.FileLocation))
        {
            File.Delete(this.Model.FileLocation);
        }

        this.OnPropertyChanged(nameof(this.Model.Exists));
        this.UpdateButtons();
    }
}