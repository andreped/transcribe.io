// <copyright file="ITranscodeService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

namespace transcribe.io.Services;

/// <summary>
/// Transcode Service.
/// </summary>
public interface ITranscodeService
{
    string BasePath { get; }

    /// <summary>
    /// Process file.
    /// </summary>
    /// <param name="filePath">File Path.</param>
    /// <returns>Path to transcoded file.</returns>
    Task<string> ProcessFile(string filePath);
}