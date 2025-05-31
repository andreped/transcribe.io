// <copyright file="WhisperDownloadModelPage.xaml.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using transcribe.io.ViewModels;

namespace transcribe.io;

public partial class WhisperDownloadModelPage : ContentPage
{
    private WhisperModelDownloadViewModel vm;

    public WhisperDownloadModelPage(IServiceProvider provider)
    {
        this.InitializeComponent();
        this.BindingContext = this.vm = provider.GetRequiredService<WhisperModelDownloadViewModel>();
    }

    private async void CloseModal(object sender, EventArgs e)
    {
        await this.Navigation.PopModalAsync();
    }
}