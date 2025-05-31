// <copyright file="MauiProgram.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Drastic.Services;
using transcribe.io.Services;
using transcribe.io.ViewModels;
using Microsoft.Extensions.Logging;

namespace transcribe.io;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
#if MACCATALYST
        DrasticForbiddenControls.CatalystControls.AllowsUnsupportedMacIdiomBehavior();
        Microsoft.Maui.Handlers.ButtonHandler.Mapper.AppendToMapping("ButtonChange", (handler, view) =>
        {
            handler.PlatformView.PreferredBehavioralStyle = UIKit.UIBehavioralStyle.Pad;
            handler.PlatformView.Layer.CornerRadius = 5;
            handler.PlatformView.ClipsToBounds = true;
        });
#endif
        var builder = MauiApp.CreateBuilder();

        builder.Services.AddSingleton<IAppDispatcher, MauiAppDispatcher>();
        builder.Services.AddSingleton<IErrorHandlerService, DebugErrorHandler>();
        builder.Services.AddSingleton<IWhisperService, DefaultWhisperService>();
#if IOS || ANDROID
		builder.Services.AddSingleton<ITranscodeService, VlcTranscodeService>();
#else
        builder.Services.AddSingleton<ITranscodeService, FFMpegTranscodeService>();
#endif
        builder.Services.AddSingleton<YouTubeService>();
        builder.Services.AddSingleton<WhisperModelService>();
        builder.Services.AddSingleton<WhisperModelDownloadViewModel>();
        builder.Services.AddSingleton<TranscriptionViewModel>();
        builder
            .UseMauiApp<App>()
            .UseVirtualListView()
            .ConfigureFonts(fonts =>
            {
#if ANDROID || WINDOWS
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
#endif
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}