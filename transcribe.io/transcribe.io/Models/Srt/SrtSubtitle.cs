// <copyright file="SrtSubtitle.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace transcribe.io.Models
{
    public class SrtSubtitle : ISubtitle
    {
        public SrtSubtitle()
        {
        }

        public SrtSubtitle(string subtitle)
        {
            string[] subtitleLines = Regex.Split(subtitle, @"^\s*$", RegexOptions.Multiline);

            foreach (string subtitleLine in subtitleLines)
            {
                string subtitleLineTrimmed = subtitleLine.Trim();
                SrtSubtitleLine block = new SrtSubtitleLine(subtitleLineTrimmed);
                this.Lines.Add(block);
            }
        }

        /// <inheritdoc/>
        public List<ISubtitleLine> Lines { get; set; } = new List<ISubtitleLine>();

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < this.Lines.Count; i++)
            {
                sb.Append(this.Lines[i].ToString());
                if (i + 1 < this.Lines.Count)
                {
                    sb.AppendLine();
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
    }
}
