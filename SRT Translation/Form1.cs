﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SRT_Translation
{
    public partial class TranslationForm : Form
    {
        public string[] SrtFiles;

        public TranslationForm() => InitializeComponent();

        private async void TranslationForm_Load(object sender, EventArgs e)
        {
            FileTranslationControl ftc;
            string output;
            foreach (string s in SrtFiles)
            {
                if (EndsWith(s, "en"))
                    output = s.Substring(0, s.Length - 6) + "ar.srt";
                else if (EndsWith(s, "english"))
                    output = s.Substring(0, s.Length - 11) + "Arabic.srt";
                else if (!EndsWith(s, "ar") && !EndsWith(s, "arabic"))
                    output = s.Substring(0, s.Length - 4) + "_ar.srt";
                else
                    continue;

                ftc = new FileTranslationControl(s, Path.GetFileName(s), output, GetColor(flowLayoutPanel1));
                flowLayoutPanel1.Controls.Add(ftc);
            }

            await TranslateFile();
            Close();
        }

        private readonly char[] chars = { ' ', '_', '-', '.' };
        private bool EndsWith(string s, string lang)
        {
            foreach (char c in chars)
                if (s.EndsWith($"{c}{lang}.srt", StringComparison.CurrentCultureIgnoreCase))
                    return true;
            
            return false;
        }

        public async Task TranslateFile()
        {
            string[] lines;
            FileTranslationControl ftc;
            while (flowLayoutPanel1.Controls.Count > 0)
            {
                ftc = (FileTranslationControl)flowLayoutPanel1.Controls[0];
                if (File.Exists(ftc.FilePath))
                {
                    lines = File.ReadAllLines(ftc.FilePath);
                    await TranslateAsync(lines, "en", "ar", ftc);
                    File.WriteAllLines(ftc.OutputFilePath, lines);
                    flowLayoutPanel1.Controls.Remove(ftc);
                }
            }
        }

        static readonly List<int> indexes = new List<int>();
        static readonly StringBuilder collection = new StringBuilder();
        public static async Task TranslateAsync(string[] text, string sourceLang, string targetLang, FileTranslationControl ftc = null)
        {
            string line; string[] lines;
            ftc.LinesCountText = text.Length;
            for (int i = 0; i < text.Length; i++)
            {
                line = text[i];
                if (line.Trim() != "" && !int.TryParse(line, out _) && (!line.Contains(" --> ") || !line.Contains(":") || !line.Contains(",") && !line.Contains(".")))
                {
                    indexes.Add(i);
                    collection.Append(line).Append('\n');
                }
                if (collection.Length > 3000 || i == text.Length - 1)
                {
                    lines = await Translator.TranslateAsync(collection.ToString(), sourceLang, targetLang);
                    if (lines.Length >= indexes.Count)
                        for (int j = 0; j < indexes.Count; j++)
                            text[indexes[j]] = lines[j];
                    
                    indexes.Clear(); collection.Clear();
                    if (ftc != null) ftc.PrecentText = i;
                }
            }
        }

        private Color GetColor(Control control)
        {
            switch (control.Controls.Count % 8)
            {
                case 1: return Color.FromArgb(250, 200, 200);
                case 2: return Color.FromArgb(250, 250, 200);
                case 3: return Color.FromArgb(200, 250, 200);
                case 4: return Color.FromArgb(200, 200, 200);
                case 5: return Color.FromArgb(200, 250, 250);
                case 6: return Color.FromArgb(200, 200, 250);
                case 7: return Color.FromArgb(250, 200, 250);
                default: return Color.FromArgb(250, 250, 250);
            }
        }
    }
}
