using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Xml;
using Diary.HTML;
using HarmonyLib;
using RimWorld;
using RTFExporter;
using Verse;

namespace Diary
{
    public class DiaryService : GameComponent
    {
        private Dictionary<string, string> entries;
        private Dictionary<string, List<DiaryImageEntry>> imagesPerDay;
        private List<DiaryImageEntry> allImages;

        public DiaryService(Game game)
        {
            entries = new Dictionary<string, string>();
            imagesPerDay = new Dictionary<string, List<DiaryImageEntry>>();
            allImages = new List<DiaryImageEntry>();
        }

        private string[] GetTextEntriesToExport()
        {
            int currentItem = 0;
            string[] entriesToExport = new string[entries.Count];

            foreach (KeyValuePair<string, string> item in entries)
            {
                string[] strings = item.Key.Split('-');
                int day = int.Parse(strings[0]);
                Quadrum quadrum = (Quadrum)Quadrum.Parse(typeof(Quadrum), strings[1]);
                int year = int.Parse(strings[2]);

                entriesToExport[currentItem] =
                    $"{Find.ActiveLanguageWorker.OrdinalNumber(day + 1)} {QuadrumUtility.Label(quadrum)} {year}\n\n{item.Value}\n";

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
            using (RTFDocument doc = new RTFDocument(filePath))
            {
                var p = doc.AppendParagraph();

                p.style.alignment = Alignment.Center;
                p.style.spaceAfter = 600;
                p.style.indent = new Indent(0, 0, 0);

                var title = p.AppendText($"{Faction.OfPlayer.Name}");

                title.style.bold = true;
                title.style.fontFamily = "Times New Roman";
                title.style.fontSize = 18;

                foreach (KeyValuePair<string, string> item in entries)
                {
                    string[] strings = item.Key.Split('-');
                    int day = int.Parse(strings[0]);
                    Quadrum quadrum = (Quadrum)Quadrum.Parse(typeof(Quadrum), strings[1]);
                    int year = int.Parse(strings[2]);

                    var entryParagraph = doc.AppendParagraph();

                    entryParagraph.style.alignment = Alignment.Left;
                    entryParagraph.style.spaceAfter = 600;
                    entryParagraph.style.indent = new Indent(0, 0, 0);

                    var t = entryParagraph.AppendText(
                        $"{Find.ActiveLanguageWorker.OrdinalNumber(day + 1)} {QuadrumUtility.Label(quadrum)} {year}"
                    );

                    t.style.bold = true;
                    t.style.fontFamily = "Times New Roman";
                    t.style.fontSize = 16;

                    var t2 = entryParagraph.AppendText($"\n\n{item.Value}");

                    t2.style.fontFamily = "Times New Roman";
                    t2.style.fontSize = 12;
                }
            }
        }

        private void ExportToHTML(string outFolderPath)
        {
            Log.Message(
                $"{AppDomain.CurrentDomain.BaseDirectory} ////// {LoadedModManager.GetMod<Diary>().Content.RootDir} //// {LoadedModManager.GetMod<Diary>().Content.RootDir} ////// {Assembly.GetExecutingAssembly().FullName}"
            );
            var projectPath = LoadedModManager.GetMod<Diary>().Content.RootDir;
            string templateFolder = Path.Combine(projectPath, "Assemblies", "HTML", "DiaryExport");
            var builder = new HTMLBuilder(templateFolder, outFolderPath);

            builder.SetTitle(Faction.OfPlayer.Name);

            foreach (KeyValuePair<string, string> item in entries)
            {
                string[] strings = item.Key.Split('-');
                int day = int.Parse(strings[0]);
                Quadrum quadrum = (Quadrum)Quadrum.Parse(typeof(Quadrum), strings[1]);
                int year = int.Parse(strings[2]);

                builder.AddH2(
                    $"{Find.ActiveLanguageWorker.OrdinalNumber(day + 1)} {QuadrumUtility.Label(quadrum)} {year}"
                );

                builder.AddParagraph($"{item.Value}");

                var imagesForThisDay = imagesPerDay.TryGetValue(item.Key);

                if (imagesForThisDay != null && imagesForThisDay.Count > 0)
                {
                    foreach (var entry in imagesForThisDay)
                    {
                        builder.AddImage(entry.Path);
                    }
                }
            }

            builder.Build();
        }

        private string GetDictionaryKey(int day, Quadrum quadrum, int year)
        {
            return day.ToString() + '-' + quadrum.ToString() + '-' + year.ToString();
        }

        public string ReadEntry(int day, Quadrum quadrum, int year)
        {
            DefaultMessage defaultMessageSetting = LoadedModManager
                .GetMod<Diary>()
                .GetSettings<DiarySettings>()
                .DefaultMessage;

            string defaultMessage = "";

            if (defaultMessageSetting == DefaultMessage.NoEntryFound)
            {
                defaultMessage = "Diary_No_Entry_Found".Translate();
            }

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
            string key = GetDictionaryKey(day, quadrum, year);
            string currentEntry = entries.TryGetValue(key);

            if (writeCurrentHour)
            {
                data = $"[{TimeTools.GetCurrentHour()}{"LetterHour".Translate()}] {data}";
            }
            if (onNewLine)
            {
                data = $"\n{data}";
            }
            if (currentEntry != null)
            {
                data = $"{currentEntry}{data}";
            }

            entries.SetOrAdd(key, data);
        }

        public void AppendEntryNow(string data, bool onNewLine = true, bool writeCurrentHour = true)
        {
            string key = GetDictionaryKey(
                TimeTools.GetCurrentDay(),
                TimeTools.GetCurrentQuadrum(),
                TimeTools.GetCurrentYear()
            );
            string currentEntry = entries.TryGetValue(key);

            if (writeCurrentHour)
            {
                data = $"[{TimeTools.GetCurrentHour()}{"LetterHour".Translate()}] {data}";
            }
            if (onNewLine)
            {
                data = $"\n{data}";
            }
            if (currentEntry != null)
            {
                data = $"{currentEntry}{data}";
            }

            entries.SetOrAdd(key, data);
        }

        public List<DiaryImageEntry> GetAllImages()
        {
            return allImages;
        }

        public int GetIndexForAllImages(int day, Quadrum quadrum, int year)
        {
            for (int i = 0; i < allImages.Count; i++)
            {
                DiaryImageEntry entry = allImages[i];

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
                {
                    return i;
                }
            }

            return -1;
        }

        public List<DiaryImageEntry> ReadImages(int day, Quadrum quadrum, int year)
        {
            return imagesPerDay.TryGetValue(GetDictionaryKey(day, quadrum, year));
        }

        public void AddImageNow(string path)
        {
            string key = GetDictionaryKey(
                TimeTools.GetCurrentDay(),
                TimeTools.GetCurrentQuadrum(),
                TimeTools.GetCurrentYear()
            );
            List<DiaryImageEntry> currentImages = imagesPerDay.TryGetValue(key);

            if (currentImages == null)
            {
                currentImages = new List<DiaryImageEntry>();
            }

            DiaryImageEntry entryToAdd = new DiaryImageEntry(
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

        public void Export(bool silent = false)
        {
            string folder = Path.GetFullPath(
                LoadedModManager.GetMod<Diary>().GetSettings<DiarySettings>().FolderPath
            );
            ExportFormat format = LoadedModManager
                .GetMod<Diary>()
                .GetSettings<DiarySettings>()
                .ExportFormat;

            //Log.Message($"Export Diary in {folder} - Format : {format}");

            string savename = Faction.OfPlayer.Name;
            string savePath = Path.Combine(
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
                {
                    Find.WindowStack.Add(
                        new Dialog_ExportDiary("Diary_Export_Successful".Translate(), savePath)
                    );
                }
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

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (entries == null)
                {
                    entries = new Dictionary<string, string>();
                }
                if (allImages == null)
                {
                    allImages = new List<DiaryImageEntry>();
                    imagesPerDay = new Dictionary<string, List<DiaryImageEntry>>();
                }

                foreach (DiaryImageEntry entry in allImages)
                {
                    string key = GetDictionaryKey(entry.Days, entry.Quadrum, entry.Year);

                    List<DiaryImageEntry> currentImages = imagesPerDay.TryGetValue(key);

                    if (currentImages == null)
                    {
                        currentImages = new List<DiaryImageEntry>();
                    }

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
