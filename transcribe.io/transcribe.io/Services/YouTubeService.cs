// <copyright file="YouTubeService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Text.RegularExpressions;
using YoutubeExplode;

namespace transcribe.io.Services;

public class YouTubeService : IOnlineVideoService
{
    private Regex youtubeRegex;
    private YoutubeClient client;

    public YouTubeService()
    {
        this.client = new YoutubeClient();
        this.youtubeRegex = new Regex(@"^(?:(?:https?:)?\/\/)?(?:www\.)?(?:m\.)?(?:youtube\.com\/(?:watch\?.*v=|embed\/|v\/|shorts\/)|youtu\.be\/)([^\s&?]+)");
    }

    /// <inheritdoc/>
    public async Task<string> GetAudioUrlAsync(string url)
    {
        var match = this.youtubeRegex.Match(url);
        if (!match.Success)
        {
            throw new ArgumentException(nameof(url));
        }

        var retry = 0;
        string? streamUrl = null;

        while (retry < 3)
        {
            try
            {
                var youtubeId = match.Groups[1].Value;
                var manifest = await this.client.Videos.Streams.GetManifestAsync(youtubeId);
                var audioStreams = manifest.GetAudioOnlyStreams();
                if (!audioStreams.Any())
                {
                    throw new ArgumentException(Translations.Common.NoAudioStreams);
                }

                streamUrl = audioStreams.First().Url;
                break;
            }
            catch (Exception ex)
            {
                await Task.Delay(1000);
                System.Diagnostics.Debug.WriteLine(ex);
            }

            retry++;
        }

        return streamUrl ?? throw new ArgumentException(Translations.Common.NoAudioStreams);
    }

    /// <inheritdoc/>
    public bool IsValidUrl(string url)
        => this.youtubeRegex.IsMatch(url);
}