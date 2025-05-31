// <copyright file="WhisperModelDownloadViewModel.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using Drastic.ViewModels;
using transcribe.io.Models;
using transcribe.io.Services;
using Microsoft.Maui.Adapters;

namespace transcribe.io.ViewModels;

public class WhisperModelDownloadViewModel
    : BaseViewModel
{
    private WhisperModelService modelService;
    private List<WhisperDownload> downloads;
    private IList<WhisperDownloadSection> sections = new List<WhisperDownloadSection>();
    public WhisperModelDownloadViewModel(IServiceProvider services)
        : base(services)
    {
        this.modelService = services.GetService(typeof(WhisperModelService)) as WhisperModelService ??
                            throw new NullReferenceException(nameof(WhisperModelService));
        this.downloads = this.modelService.AllModels
            .Select(n => new WhisperDownload(n, this.modelService, this.Dispatcher)).ToList();
        this.Downloads = new VirtualListViewAdapter<WhisperDownload>(this.downloads);
        this.SectionedDownloads = new SectionedWhisperDownloadAdapter(this.sections);
        this.SectionedDownloads.AddItems(this.downloads);
    }

    public WhisperModelService ModelService => this.modelService;

    public VirtualListViewAdapter<WhisperDownload> Downloads { get; }
    
    public SectionedWhisperDownloadAdapter SectionedDownloads { get; }
}