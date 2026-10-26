using System.Reflection;
using Diary.Core;
using Diary.HTML;
using Diary.Settings;
using Diary.UI;
using RimWorld;
using RTFExporter;
using Verse;

// Don't change the namespace for compatibility with old versions
namespace Diary;

public class DiaryService : GameComponent
{
    private List<DiaryImageEntry> _allImages;
    private Dictionary<string, string> _entries;
    private Dictionary<string, List<DiaryImageEntry>> _imagesPerDay;

    public DiaryService(Game game)
    {
        _entries = new Dictionary<string, string>();
        _imagesPerDay = new Dictionary<string, List<DiaryImageEntry>>();
        _allImages = new List<DiaryImageEntry>();
    }

    private List<(string, int, Quadrum, int)> GetEntriesKeySorted()
    {
        var sortedKeys = new List<(string, int, Quadrum, int)>();

        foreach (var item in _entries)
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
        var entriesToExport = new string[_entries.Count];

        var keysSortedByDates = GetEntriesKeySorted();

        foreach (var keyObject in keysSortedByDates)
        {
            var (key, day, quadrum, year) = keyObject;

            entriesToExport[currentItem] =
                $"{Find.ActiveLanguageWorker.OrdinalNumber(day + 1)} {quadrum.Label()} {year}\n\n{_entries[key]}\n";

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
        using var doc = new RTFDocument(filePath);
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

            var t2 = entryParagraph.AppendText($"\n\n{_entries[key]}");

            t2.style.fontFamily = "Times New Roman";
            t2.style.fontSize = 12;
        }
    }

    private void ExportToHTML(string outFolderPath)
    {
        var projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var templateFolder = Path.Combine(projectPath, "HTML", "DiaryExport");
        var builder = new HTMLBuilder(templateFolder, outFolderPath);

        builder.SetTitle(Faction.OfPlayer.Name);

        var keysSortedByDates = GetEntriesKeySorted();

        foreach (var keyObject in keysSortedByDates)
        {
            var (key, day, quadrum, year) = keyObject;
            builder.AddH2(
                $"{Find.ActiveLanguageWorker.OrdinalNumber(day + 1)} {quadrum.Label()} {year}"
            );

            builder.AddParagraph($"{_entries[key]}");

            var imagesForThisDay = _imagesPerDay.TryGetValue(key);

            if (imagesForThisDay != null && imagesForThisDay.Count > 0)
                foreach (var entry in imagesForThisDay)
                    builder.AddImage(entry.path);
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
            .GetMod<Diary>()
            .GetSettings<DiarySettings>()
            .DefaultMessage;

        var defaultMessage = "";

        if (defaultMessageSetting == DefaultMessage.NoEntryFound)
            defaultMessage = "Diary_No_Entry_Found".Translate();

        return _entries.TryGetValue(GetDictionaryKey(day, quadrum, year), defaultMessage);
    }

    public void WriteEntry(string data, int day, Quadrum quadrum, int year)
    {
        _entries.SetOrAdd(GetDictionaryKey(day, quadrum, year), data);
    }

    public void WriteEntryNow(string data)
    {
        _entries.SetOrAdd(
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
        bool writeCurrentHour = true,
        int replacementHour = 0
    )
    {
        var key = GetDictionaryKey(day, quadrum, year);
        var currentEntry = _entries.TryGetValue(key);
        var hourToUse = LoadedModManager
            .GetMod<Diary>()
            .GetSettings<DiarySettings>().IsEventAddedWithCurrentHour
            ? TimeTools.GetCurrentHour()
            : replacementHour;

        if (writeCurrentHour) data = $"[{hourToUse}{"LetterHour".Translate()}] {data}";
        if (onNewLine) data = $"\n{data}";
        if (currentEntry != null) data = $"{currentEntry}{data}";

        _entries.SetOrAdd(key, data);
    }

    public void AppendEntryNow(string data, bool onNewLine = true, bool writeCurrentHour = true)
    {
        var key = GetDictionaryKey(
            TimeTools.GetCurrentDay(),
            TimeTools.GetCurrentQuadrum(),
            TimeTools.GetCurrentYear()
        );
        var currentEntry = _entries.TryGetValue(key);

        if (writeCurrentHour) data = $"[{TimeTools.GetCurrentHour()}{"LetterHour".Translate()}] {data}";
        if (onNewLine) data = $"\n{data}";
        if (currentEntry != null) data = $"{currentEntry}{data}";

        _entries.SetOrAdd(key, data);
    }

    public List<DiaryImageEntry> GetAllImages()
    {
        return _allImages;
    }

    public int GetIndexForAllImages(int day, Quadrum quadrum, int year)
    {
        for (var i = 0; i < _allImages.Count; i++)
        {
            var entry = _allImages[i];

            if (
                entry.year > year
                || (
                    entry.year == year
                    && (
                        entry.quadrum > quadrum
                        || (entry.quadrum == quadrum && entry.days >= day)
                    )
                )
            )
                return i;
        }

        return -1;
    }

    public List<DiaryImageEntry> ReadImages(int day, Quadrum quadrum, int year)
    {
        return _imagesPerDay.TryGetValue(GetDictionaryKey(day, quadrum, year));
    }

    public void AddImageNow(string path)
    {
        var key = GetDictionaryKey(
            TimeTools.GetCurrentDay(),
            TimeTools.GetCurrentQuadrum(),
            TimeTools.GetCurrentYear()
        );
        var currentImages = _imagesPerDay.TryGetValue(key);

        if (currentImages == null) currentImages = new List<DiaryImageEntry>();

        var entryToAdd = new DiaryImageEntry(
            path,
            TimeTools.GetCurrentHour(),
            TimeTools.GetCurrentDay(),
            TimeTools.GetCurrentQuadrum(),
            TimeTools.GetCurrentYear()
        );

        currentImages.Add(entryToAdd);

        _imagesPerDay.SetOrAdd(key, currentImages);
        _allImages.Add(entryToAdd);
    }

    public void ChangeImagesFolder(string newFolderPath)
    {
        if (!Directory.Exists(newFolderPath)) Directory.CreateDirectory(newFolderPath);

        foreach (var currentImage in _allImages)
        {
            var newFilePath = Path.Combine(newFolderPath, Path.GetFileName(currentImage.path));

            if (File.Exists(currentImage.path)) File.Move(currentImage.path, newFilePath);

            currentImage.path = newFilePath;
        }
    }

    public string GetCurrentImageFolderPath()
    {
        if (_allImages.Count > 0) return Path.GetDirectoryName(_allImages[0].path);

        return "";
    }

    public void Export(bool silent = false)
    {
        var folder = Path.GetFullPath(
            LoadedModManager.GetMod<Diary>().GetSettings<DiarySettings>().FolderPath
        );
        var format = LoadedModManager
            .GetMod<Diary>()
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

        Scribe_Collections.Look(ref _entries, "entries", LookMode.Value, LookMode.Value);
        Scribe_Collections.Look(ref _allImages, "allImages", LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            if (_entries == null) _entries = new Dictionary<string, string>();
            if (_allImages == null)
            {
                _allImages = new List<DiaryImageEntry>();
                _imagesPerDay = new Dictionary<string, List<DiaryImageEntry>>();
            }

            foreach (var entry in _allImages)
            {
                var key = GetDictionaryKey(entry.days, entry.quadrum, entry.year);

                var currentImages = _imagesPerDay.TryGetValue(key);

                if (currentImages == null) currentImages = new List<DiaryImageEntry>();

                currentImages.Add(entry);
                _imagesPerDay.SetOrAdd(key, currentImages);
            }
        }
    }

    public override void FinalizeInit()
    {
        base.FinalizeInit();
    }
}