// <copyright file="SrtSubtitleLine.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

#nullable disable

using System;
using System.IO;
using System.Text;

namespace transcribe.io.Models
{
    public class SrtSubtitleLine : ISubtitleLine
    {
        public SrtSubtitleLine()
        {
        }

        public SrtSubtitleLine(string subtitleText)
        {
            subtitleText = subtitleText.Trim();
            using (StringReader data = new StringReader(subtitleText))
            {
                this.LineNumber = int.Parse(data.ReadLine().Trim());

                string secondLine = data.ReadLine();
                this.Start = TimeSpan.ParseExact(secondLine.Substring(0, 12), @"hh\:mm\:ss\,fff", null);
                this.End = TimeSpan.ParseExact(secondLine.Substring(17, 12), @"hh\:mm\:ss\,fff", null);

                this.Text = data.ReadToEnd().Trim();
            }
        }

        /// <inheritdoc/>
        public TimeSpan Start { get; set; }

        /// <inheritdoc/>
        public TimeSpan End { get; set; }

        public string Time => $"{this.Start} -> {this.End}";

        /// <inheritdoc/>
        public string Text { get; set; }

        public int LineNumber { get; set; }

        public byte[] Image { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(this.LineNumber.ToString());

            sb.Append(this.Start.ToString(@"hh\:mm\:ss\,fff"));
            sb.Append(" --> ");
            sb.Append(this.End.ToString(@"hh\:mm\:ss\,fff"));
            sb.AppendLine();

            sb.Append(this.Text);

            return sb.ToString();
        }
    }
}
