// <copyright file="WhisperSegmentData.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

namespace transcribe.io.Models;

public class WhisperSegmentData
{
    public WhisperSegmentData(string text, TimeSpan start, TimeSpan end, float minProbability, float maxProbability, float probability, string language)
    {
        this.Text = text;
        this.Start = start;
        this.End = end;
        this.MinProbability = minProbability;
        this.MaxProbability = maxProbability;
        this.Probability = probability;
        this.Language = language;
    }

    public string Text { get; }

    public TimeSpan Start { get; }

    public TimeSpan End { get; }

    public float MinProbability { get; }

    public float MaxProbability { get; }

    public float Probability { get; }

    public string Language { get; }
}