// <copyright file="FFMpegTranscodeService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Xabe.FFmpeg;

namespace transcribe.io.Services;

public class FFMpegTranscodeService : ITranscodeService
{
    private string basePath;
    private string? generatedFilename;

    public FFMpegTranscodeService(string? basePath = default, string? generatedFilename = default)
    {
        this.basePath = basePath ?? Path.GetTempPath();
        this.generatedFilename = generatedFilename;
    }

    /// <inheritdoc/>
    public string BasePath => this.basePath;

    /// <inheritdoc/>
    public async Task<string> ProcessFile(string file)
    {
        var mediaInfo = await FFmpeg.GetMediaInfo(file);
        var audioStream = mediaInfo.AudioStreams.FirstOrDefault();
        if (audioStream is null)
        {
            return string.Empty;
        }

        if (audioStream.SampleRate != 16000)
        {
            var outputfile = Path.Combine(this.basePath, $"{this.generatedFilename ?? Path.GetRandomFileName()}.wav");
            var result = await FFmpeg.Conversions.New()
                .AddStream(audioStream)
                .AddParameter("-c pcm_s16le -ar 16000")
                .SetOutput(outputfile)
                .SetOverwriteOutput(true)
                .Start();

            if (result is null)
            {
                return string.Empty;
            }

            return outputfile;
        }

        return file;
    }
}