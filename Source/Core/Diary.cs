using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RimWorld;
using RTFExporter;
using Verse;

namespace DiaryMod.Core
{
    namespace Diary
    {
        public class Diary : GameComponent
        {
            private readonly Dictionary<string, string> entries = new Dictionary<string, string>();
            private Pawn owner;

            public Diary()
            {
            }

            public Diary(Dictionary<string, string> entries)
            {
                this.entries = entries;
            }

            public Diary(Pawn owner)
            {
                this.owner = owner;
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
                var projectPath = LoadedModManager.GetMod<Diary>().Content.RootDir;
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
                return day.ToString() + '-' + quadrum + '-' + year;
            }

            public string ReadEntry(int day, Quadrum quadrum, int year)
            {
                var defaultMessageSetting = LoadedModManager
                    .GetMod<DiaryMod>()
                    .GetSettings<DiarySettings>()
                    .DefaultMessage;

                var defaultMessage = "";

                if (defaultMessageSetting == DefaultMessage.NoEntryFound)
                    defaultMessage = "Diary_No_Entry_Found".Translate();

                return entries.TryGetValue(GetDictionaryKey(day, quadrum, year), defaultMessage);
            }

            public void WriteEntry(string data, int day, Quadrum quadrum, int year)
            {
                entries.SetOrAdd(GetDictionaryKey(day, quadrum, year), data);
            }

            public void WriteEntryNow(string data)
            {
                entries.SetOrAdd(
                    GetDictionaryKey(
                        TimeTools.GetCurrentDay(),
                        TimeTools.GetCurrentQuadrum(),
                        TimeTools.GetCurrentYear()
                    ),
                    data
                );
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
                var key = GetDictionaryKey(day, quadrum, year);
                var currentEntry = entries.TryGetValue(key);

                if (writeCurrentHour) data = $"[{TimeTools.GetCurrentHour()}{"LetterHour".Translate()}] {data}";
                if (onNewLine) data = $"\n{data}";
                if (currentEntry != null) data = $"{currentEntry}{data}";

                entries.SetOrAdd(key, data);
            }

            public void AppendEntryNow(string data, bool onNewLine = true, bool writeCurrentHour = true)
            {
                var key = GetDictionaryKey(
                    TimeTools.GetCurrentDay(),
                    TimeTools.GetCurrentQuadrum(),
                    TimeTools.GetCurrentYear()
                );
                var currentEntry = entries.TryGetValue(key);

                if (writeCurrentHour) data = $"[{TimeTools.GetCurrentHour()}{"LetterHour".Translate()}] {data}";
                if (onNewLine) data = $"\n{data}";
                if (currentEntry != null) data = $"{currentEntry}{data}";

                entries.SetOrAdd(key, data);
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
        }
    }
}