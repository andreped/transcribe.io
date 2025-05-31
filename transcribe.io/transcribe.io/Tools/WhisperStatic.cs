// <copyright file="WhisperStatic.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Whisper.net.Ggml;

namespace transcribe.io.Tools;

public static class WhisperStatic
{
    public static string DefaultPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Drastic.Whisper");

    public static string ToDownloadUrl(this GgmlType type, QuantizationType quantizationType)
    {
        var subdirectory = GetQuantizationSubdirectory(quantizationType);
        var modelName = type.ToFilename();
        return $"https://huggingface.co/sandrohanea/whisper.net/resolve/v1/{subdirectory}/{modelName}";
    }

    public static string GetModelPath(GgmlType type, QuantizationType quantizationType)
        => Path.Combine(DefaultPath, GetQuantizationSubdirectory(quantizationType), type.ToFilename());

    public static string ToFilename(this GgmlType type) => type switch
    {
        GgmlType.Tiny => "ggml-tiny.bin",
        GgmlType.TinyEn => "ggml-tiny.en.bin",
        GgmlType.Base => "ggml-base.bin",
        GgmlType.BaseEn => "ggml-base.en.bin",
        GgmlType.Small => "ggml-small.bin",
        GgmlType.SmallEn => "ggml-small.en.bin",
        GgmlType.Medium => "ggml-medium.bin",
        GgmlType.MediumEn => "ggml-medium.en.bin",
        GgmlType.LargeV1 => "ggml-large-v1.bin",
        GgmlType.Large => "ggml-large.bin",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };

    public static string ToSha(this GgmlType type) => type switch
    {
        GgmlType.Tiny => "bd577a113a864445d4c299885e0cb97d4ba92b5f",
        GgmlType.TinyEn => "c78c86eb1a8faa21b369bcd33207cc90d64ae9df",
        GgmlType.Base => "465707469ff3a37a2b9b8d8f89f2f99de7299dac",
        GgmlType.BaseEn => "137c40403d78fd54d454da0f9bd998f78703390c",
        GgmlType.Small => "55356645c2b361a969dfd0ef2c5a50d530afd8d5",
        GgmlType.SmallEn => "db8a495a91d927739e50b3fc1cc4c6b8f6c2d022",
        GgmlType.Medium => "fd9727b6e1217c2f614f9b698455c4ffd82463b4",
        GgmlType.MediumEn => "8c30f0e44ce9560643ebd10bbe50cd20eafd3723",
        GgmlType.LargeV1 => "b1caaf735c4cc1429223d5a74f0f4d0b9b59a299",
        GgmlType.Large => "0f4c8e34f21cf1a914c59d8b3ce882345ad349d6",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };

    private static string GetQuantizationSubdirectory(QuantizationType quantization)
    {
        return quantization switch
        {
            QuantizationType.NoQuantization => "classic",
            QuantizationType.Q4_0 => "q4_0",
            QuantizationType.Q4_1 => "q4_1",
            QuantizationType.Q4_2 => "q4_2",
            QuantizationType.Q5_0 => "q5_0",
            QuantizationType.Q5_1 => "q5_1",
            QuantizationType.Q8_0 => "q8_0",
            _ => throw new ArgumentOutOfRangeException(nameof(quantization), quantization, null),
        };
    }
}