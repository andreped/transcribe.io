// <copyright file="DrasticWhisperFileExtensions.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

namespace transcribe.io.Tools;

public static class DrasticWhisperFileExtensions
{
    public static List<string> MFSupported = new List<string>()
        {
            ".avi",
            ".wmv",
            ".mov",
            ".mp4",
            ".mp3",
            ".wma",
            ".m4a",
            ".flac",
            ".mkv",
        };

    public static List<string> Supported = new List<string>()
        {
            ".3g2",
            ".3gp",
            ".3gp2",
            ".3gpp",
            ".amv",
            ".asf",
            ".avi",
            ".divx",
            ".drc",
            ".dv",
            ".f4v",
            ".flv",
            ".gvi",
            ".gxf",
            ".ismv",
            ".iso",
            ".m1v",
            ".m2v",
            ".m2t",
            ".m2ts",
            ".m3u8",
            ".mkv",
            ".mov",
            ".m4a",
            ".mp2",
            ".mp2v",
            ".mp4",
            ".m4v",
            ".mp4v",
            ".mpe",
            ".mpeg",
            ".mpeg1",
            ".mpeg2",
            ".mpeg4",
            ".mpg",
            ".mpv2",
            ".mts",
            ".mtv",
            ".mxf",
            ".mxg",
            ".nsv",
            ".nut",
            ".nuv",
            ".ogm",
            ".ogv",
            ".ogx",
            ".ps",
            ".rec",
            ".rm",
            ".rmvb",
            ".tob",
            ".ts",
            ".tts",
            ".vro",
            ".webm",
            ".wm",
            ".wmv",
            ".wtv",
            ".xesc",
            ".mp3",
            ".ogg",
            ".opus",
            ".aac",
            ".wma",
            ".wav",
            ".flac",
            ".mlp",
        };

    public static string[] VideoExtensions =
    {
                ".3g2", ".3gp", ".3gp2", ".3gpp", ".amv", ".asf", ".avi", ".divx", ".drc", ".dv",

                ".f4v", ".flv", ".gvi", ".gxf", ".ismv", ".iso", ".m1v", ".m2v", ".m2t", ".m2ts",

                ".m4v", ".mkv", ".mov", ".mp2", ".mp2v", ".mp4", ".mp4v", ".mpe", ".mpeg",

                ".mpeg1", ".mpeg2", ".mpeg4", ".mpg", ".mpv2", ".mts", ".mtv", ".mxf", ".mxg",

                ".nsv", ".nut", ".nuv", ".ogm", ".ogv", ".ogx", ".ps", ".rec", ".rm", ".rmvb",

                ".tod", ".ts", ".tts", ".vro", ".webm", ".wm", ".wmv", ".wtv", ".xesc",
    };

    public static string[] AudioExtensions =
    {
                ".3ga", ".a52", ".aac", ".ac3", ".adt", ".adts", ".aif", ".aifc", ".aiff", ".amr",

                ".aob", ".ape", ".awb", ".caf", ".dts", ".flac", ".it", ".m4a", ".m4b", ".m4p",

                ".mid", ".mka", ".mlp", ".mod", ".mpa", ".mp1", ".mp2", ".mp3", ".mpc", ".mpga",

                ".oga", ".ogg", ".opus", ".oma", ".opus", ".ra", ".ram", ".rmi", ".s3m", ".spx", ".tta",

                ".voc", ".vqf", ".w64", ".wav", ".wma", ".wv", ".xa", ".xm",
    };

    public static string[] SubtitleExtensions =
    {
            ".srt", ".ass", ".ssa",
    };

    public enum DAFileType
    {
        Audio,
        Video,
        Subtitle,
        Other,
    }

    public static DAFileType FileTypeHelper(string ext)
    {
        if (VideoExtensions.Contains(ext))
        {
            return DAFileType.Video;
        }
        else if (AudioExtensions.Contains(ext))
        {
            return DAFileType.Audio;
        }
        else if (SubtitleExtensions.Contains(ext))
        {
            return DAFileType.Subtitle;
        }
        else
        {
            return DAFileType.Other;
        }
    }
}