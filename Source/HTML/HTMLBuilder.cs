﻿using System.Collections.Generic;
using System.IO;
using System.Text;
using Verse;

namespace Diary.HTML
{
    internal class HTMLBuilder
    {
        private readonly StringBuilder _contentBuilder;
        private readonly Dictionary<string, string> _images;
        private readonly string _outFolder;
        private readonly string _templateFolder;
        private string _title;

        public HTMLBuilder(string templateFolder, string outFolder)
        {
            _templateFolder = templateFolder;
            _outFolder = outFolder;
            _contentBuilder = new StringBuilder();
            _images = new Dictionary<string, string>();
        }

        public void Build()
        {
            Log.Message(_templateFolder);
            FileTools.CopyFilesRecursively(_templateFolder, _outFolder);

            var htmlContent = File.ReadAllText($"{_outFolder}/index.html");

            htmlContent = htmlContent.Replace("{{title}}", _title);
            htmlContent = htmlContent.Replace("{{content}}", _contentBuilder.ToString());

            File.WriteAllText($"{_outFolder}/index.html", htmlContent);

            foreach (var entry in _images) File.Copy(entry.Value, $"{_outFolder}/{entry.Key}", true);
        }

        public void SetTitle(string title)
        {
            _title = title;
        }

        public void AddImage(string path)
        {
            var ext = path.Substring(path.LastIndexOf('.'));
            var key = $"img-{_images.Count}{ext}";

            _images.Add(key, path);

            _contentBuilder.Append($"<img src=\"./{key}\"/><br />");
        }

        public void AddH1(string title)
        {
            _contentBuilder.Append($"<h1>{title}</h1>");
        }

        public void AddH2(string title)
        {
            _contentBuilder.Append($"<h2>{title}</h2>");
        }

        public void AddH3(string title)
        {
            _contentBuilder.Append($"<h3>{title}</h3>");
        }

        public void AddParagraph(string content)
        {
            _contentBuilder.Append($"<p>{content.Replace("\n", "<br />")}</p>");
        }
    }
}