﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiaryMod.Core.Diary;
using DiaryMod.HTML;
using RimWorld;
using RTFExporter;
using Verse;

namespace DiaryMod
{
    public class DiaryService : GameComponent
    {
        private static string COLONY_DIARY = "colony";

        private List<DiaryImageEntry> allImages;
        private Dictionary<string, Diary> diaries;

        [Obsolete("entries is only kept for compatibility with old versions of the mod")]
        private Dictionary<string, string> entries;

        private Dictionary<string, List<DiaryImageEntry>> imagesPerDay;

        public DiaryService(Game game)
        {
            entries = new Dictionary<string, string>();
            imagesPerDay = new Dictionary<string, List<DiaryImageEntry>>();
            allImages = new List<DiaryImageEntry>();
            diaries = new Dictionary<string, Diary>();
        }

        private List<(string, int, Quadrum, int)> GetEntriesKeySorted()
        {
            var sortedKeys = new List<(string, int, Quadrum, int)>();

            foreach (var item in entries)
            {
                var strings = item.Key.Split('-');
                var day = int.Parse(strings[0]);
                var quadrum = (Quadrum)Enum.Parse(typeof(Quadrum), strings[1]);
                var year = int.Parse(strings[2]);

                var entryToAdd = (item.Key, day, quadrum, year);
                var isEntryAdded = false;

                for (var i = 0; i < sortedKeys.Count; i++)
                {
                    var currentEntry = sortedKeys[i];

                    if (entryToAdd.Item4 <= currentEntry.Item4 && entryToAdd.Item3 <= currentEntry.Item3 &&
                        entryToAdd.Item2 < currentEntry.Item2)
                    {
                        sortedKeys.Insert(i, entryToAdd);
                        isEntryAdded = true;

                        break;
                    }
                }

                if (!isEntryAdded) sortedKeys.Add(entryToAdd);
            }

            return sortedKeys;
        }

        private string[] GetTextEntriesToExport()
        {
            var currentItem = 0;
            var entriesToExport = new string[entries.Count];

            var keysSortedByDates = GetEntriesKeySorted();

            foreach (var keyObject in keysSortedByDates)
            {
                var (key, day, quadrum, year) = keyObject;

                entriesToExport[currentItem] =
                    $"{Find.ActiveLanguageWorker.OrdinalNumber(day + 1)} {quadrum.Label()} {year}\n\n{entries[key]}\n";

                currentItem++;
            }

            return entriesToExport;
        }

        private void ExportToText(string filePath)
        {
            File.WriteAllLines(filePath, GetTextEntriesToExport());
        }

        private void ExportToRTF(string filePath)
        {
            using (var doc = new RTFDocument(filePath))
            {
                var p = doc.AppendParagraph();

                p.style.alignment = Alignment.Center;
                p.style.spaceAfter = 600;
                p.style.indent = new Indent(0, 0, 0);

                var title = p.AppendText($"{Faction.OfPlayer.Name}");

                title.style.bold = true;
                title.style.fontFamily = "Times New Roman";
                title.style.fontSize = 18;

                var keysSortedByDates = GetEntriesKeySorted();

                foreach (var keyObject in keysSortedByDates)
                {
                    var (key, day, quadrum, year) = keyObject;

                    var entryParagraph = doc.AppendParagraph();

                    entryParagraph.style.alignment = Alignment.Left;
                    entryParagraph.style.spaceAfter = 600;
                    entryParagraph.style.indent = new Indent(0, 0, 0);

                    var t = entryParagraph.AppendText(
                        $"{Find.ActiveLanguageWorker.OrdinalNumber(day + 1)} {quadrum.Label()} {year}"
                    );

                    t.style.bold = true;
                    t.style.fontFamily = "Times New Roman";
                    t.style.fontSize = 16;

                    var t2 = entryParagraph.AppendText($"\n\n{entries[key]}");

                    t2.style.fontFamily = "Times New Roman";
                    t2.style.fontSize = 12;
                }
            }
        }

        private void ExportToHTML(string outFolderPath)
        {
            var projectPath = LoadedModManager.GetMod<DiaryMod>().Content.RootDir;
            var templateFolder = Path.Combine(projectPath, "Assemblies", "HTML", "DiaryExport");
            var builder = new HTMLBuilder(templateFolder, outFolderPath);

            builder.SetTitle(Faction.OfPlayer.Name);

            var keysSortedByDates = GetEntriesKeySorted();

            foreach (var keyObject in keysSortedByDates)
            {
                var (key, day, quadrum, year) = keyObject;
                builder.AddH2(
                    $"{Find.ActiveLanguageWorker.OrdinalNumber(day + 1)} {quadrum.Label()} {year}"
                );

                builder.AddParagraph($"{entries[key]}");

                var imagesForThisDay = imagesPerDay.TryGetValue(key);

                if (imagesForThisDay != null && imagesForThisDay.Count > 0)
                    foreach (var entry in imagesForThisDay)
                        builder.AddImage(entry.Path);
            }

            builder.Build();
        }

        private string GetDictionaryKey(int day, Quadrum quadrum, int year)
        {
        }

        public string ReadEntry(int day, Quadrum quadrum, int year)
        {
        }

        public void WriteEntry(string data, int day, Quadrum quadrum, int year)
        {
        }

        public void WriteEntryNow(string data)
        {
        }

        public void AppendEntry(
            string data,
            int day,
            Quadrum quadrum,
            int year,
            bool onNewLine = true,
            bool writeCurrentHour = true
        )
        {
        }

        public void AppendEntryNow(string data, bool onNewLine = true, bool writeCurrentHour = true)
        {
        }

        public List<DiaryImageEntry> GetAllImages()
        {
            return allImages;
        }

        public int GetIndexForAllImages(int day, Quadrum quadrum, int year)
        {
            for (var i = 0; i < allImages.Count; i++)
            {
                var entry = allImages[i];

                if (
                    entry.Year > year
                    || (
                        entry.Year == year
                        && (
                            entry.Quadrum > quadrum
                            || (entry.Quadrum == quadrum && entry.Days >= day)
                        )
                    )
                )
                    return i;
            }

            return -1;
        }

        public List<DiaryImageEntry> ReadImages(int day, Quadrum quadrum, int year)
        {
            return imagesPerDay.TryGetValue(GetDictionaryKey(day, quadrum, year));
        }

        public void AddImageNow(string path)
        {
            var key = GetDictionaryKey(
                TimeTools.GetCurrentDay(),
                TimeTools.GetCurrentQuadrum(),
                TimeTools.GetCurrentYear()
            );
            var currentImages = imagesPerDay.TryGetValue(key);

            if (currentImages == null) currentImages = new List<DiaryImageEntry>();

            var entryToAdd = new DiaryImageEntry(
                path,
                TimeTools.GetCurrentHour(),
                TimeTools.GetCurrentDay(),
                TimeTools.GetCurrentQuadrum(),
                TimeTools.GetCurrentYear()
            );

            currentImages.Add(entryToAdd);

            imagesPerDay.SetOrAdd(key, currentImages);
            allImages.Add(entryToAdd);
        }

        public void ChangeImagesFolder(string newFolderPath)
        {
            if (!Directory.Exists(newFolderPath)) Directory.CreateDirectory(newFolderPath);

            foreach (var currentImage in allImages)
            {
                var newFilePath = Path.Combine(newFolderPath, Path.GetFileName(currentImage.Path));

                if (File.Exists(currentImage.Path)) File.Move(currentImage.Path, newFilePath);

                currentImage.Path = newFilePath;
            }
        }

        public string GetCurrentImageFolderPath()
        {
            if (allImages.Count > 0) return Path.GetDirectoryName(allImages[0].Path);

            return "";
        }

        public void Export(bool silent = false)
        {
            var folder = Path.GetFullPath(
                LoadedModManager.GetMod<DiaryMod>().GetSettings<DiarySettings>().FolderPath
            );
            var format = LoadedModManager
                .GetMod<DiaryMod>()
                .GetSettings<DiarySettings>()
                .ExportFormat;

            //Log.Message($"Export Diary in {folder} - Format : {format}");

            var savename = Faction.OfPlayer.Name;
            var savePath = Path.Combine(
                folder,
                "diary-"
                + string.Concat(savename.Where(c => !char.IsWhiteSpace(c)))
                + "-"
                + DateTime.Now.Ticks
            );

            try
            {
                switch (format)
                {
                    case ExportFormat.Text:
                        savePath += ".txt";
                        ExportToText(savePath);
                        break;
                    case ExportFormat.RTF:
                        savePath += ".rtf";
                        ExportToRTF(savePath);
                        break;
                    case ExportFormat.HTML:
                        ExportToHTML(savePath);
                        break;
                }

                if (!silent)
                    Find.WindowStack.Add(
                        new Dialog_ExportDiary("Diary_Export_Successful".Translate(), savePath)
                    );
            }
            catch (DirectoryNotFoundException e)
            {
                Log.Message($"{e}");
                Find.WindowStack.Add(
                    new Dialog_ExportDiary("Diary_Directory_Not_Found".Translate())
                );
            }
            catch (Exception e)
            {
                Log.Message($"{e}");

                Find.WindowStack.Add(
                    new Dialog_ExportDiary("Diary_Error_During_Export".Translate())
                );
            }
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref entries, "entries", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref allImages, "allImages", LookMode.Deep);
            Scribe_Collections.Look(ref diaries, "diaries", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (entries == null) entries = new Dictionary<string, string>();
                if (allImages == null)
                {
                    allImages = new List<DiaryImageEntry>();
                    imagesPerDay = new Dictionary<string, List<DiaryImageEntry>>();
                }

                foreach (var entry in allImages)
                {
                    var key = GetDictionaryKey(entry.Days, entry.Quadrum, entry.Year);

                    var currentImages = imagesPerDay.TryGetValue(key);

                    if (currentImages == null) currentImages = new List<DiaryImageEntry>();

                    currentImages.Add(entry);
                    imagesPerDay.SetOrAdd(key, currentImages);
                }
            }
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
        }
    }
}