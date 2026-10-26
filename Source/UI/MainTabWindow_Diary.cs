using System.Diagnostics;
using Diary.Core;
using Diary.Settings;
using RimWorld;
using UnityEngine;
using Verse;

namespace Diary.UI;

public class MainTabWindow_Diary : MainTabWindow
{
    private readonly List<DiaryImageEntry> _allImages;
    private readonly GUIDraggableTexture _draggableImage;
    private readonly List<string> _fastHourStrings;
    private readonly DiarySettings _settings;
    private readonly string _textAreaName = "DiaryTextArea";
    private readonly Dictionary<string, string> _truncationCache = new();
    private int _day;
    private List<DiaryImageEntry> _dayImages;
    private int _displayedMessageIndex = -1;
    private float _entryScrollY;
    private bool _imageDisplayMode;
    private float _lastWindowWidth;
    private LogFilter _logFilter;
    private float _messageLastHeight;
    private Vector2 _messagesScrollPos;
    private Quadrum _quadrum;
    private int _selectedAllImagesIndex;
    private float _widthPerButton;
    private int _year;

    public MainTabWindow_Diary()
    {
        _day = TimeTools.GetCurrentDay();
        _quadrum = TimeTools.GetCurrentQuadrum();
        _year = TimeTools.GetCurrentYear();

        _fastHourStrings = new List<string>();

        for (var i = 0; i < 24; i++) _fastHourStrings.Add(i + "LetterHour".Translate());

        closeOnAccept = false;

        _settings = LoadedModManager.GetMod<Diary>().GetSettings<DiarySettings>();
        _imageDisplayMode = false;
        _logFilter = _settings.DefaultLogFilter;
        _draggableImage = new GUIDraggableTexture();
        _selectedAllImagesIndex = -1;
        _allImages = Current.Game.GetComponent<DiaryService>().GetAllImages();
        _lastWindowWidth = 0;
        _entryScrollY = 0;
    }

    public bool CanAccessToPreviousDay()
    {
        return _day != TimeTools.GetMinimumDay(_quadrum, _year) || _quadrum != TimeTools.GetMinimumQuadrum(_year) ||
               _year != TimeTools.GetMinimumYear();
    }

    public bool CanAccessToPreviousEntry()
    {
        return _allImages != null && _selectedAllImagesIndex > 0;
    }

    public bool CanAccessToNextDay()
    {
        return _day != TimeTools.GetMaximumDay(_quadrum, _year) || _quadrum != TimeTools.GetMaximumQuadrum(_year) ||
               _year != TimeTools.GetMaximumYear();
    }

    public bool CanAccessToNextEntry()
    {
        return _allImages != null && _selectedAllImagesIndex < _allImages.Count - 1;
    }

    public void SetCurrentDateToPreviousDay()
    {
        if (!CanAccessToPreviousDay()) return;

        _displayedMessageIndex = -1;

        if (_day > TimeTools.GetMinimumDay(_quadrum, _year))
        {
            _day--;

            return;
        }

        _day = GenDate.DaysPerQuadrum - 1;

        if (_quadrum > Quadrum.Aprimay)
        {
            _quadrum--;

            return;
        }

        _year--;
        _quadrum = Quadrum.Decembary;
    }

    public void SetCurrentDateToNextDay()
    {
        if (!CanAccessToNextDay()) return;

        _displayedMessageIndex = -1;

        if (_day < GenDate.DaysPerQuadrum - 1)
        {
            _day++;

            return;
        }

        _day = 0;

        if (_quadrum < Quadrum.Decembary)
        {
            _quadrum++;

            return;
        }

        _year++;
        _quadrum = Quadrum.Aprimay;
    }

    public void SetCurrentDay(int day)
    {
        _day = day;

        _displayedMessageIndex = -1;
    }

    public void SetCurrentQuadrum(Quadrum q)
    {
        _quadrum = q;

        if (_day > TimeTools.GetMaximumDay(_quadrum, _year))
            _day = TimeTools.GetMaximumDay(_quadrum, _year);
        else if (_day < TimeTools.GetMinimumDay(_quadrum, _year)) _day = TimeTools.GetMinimumDay(_quadrum, _year);

        _displayedMessageIndex = -1;
    }

    public void SetCurrentYear(int year)
    {
        _year = year;

        if (_quadrum > TimeTools.GetMaximumQuadrum(year))
            _quadrum = TimeTools.GetMaximumQuadrum(year);
        else if (_quadrum < TimeTools.GetMinimumQuadrum(year)) _quadrum = TimeTools.GetMinimumQuadrum(year);

        if (_day > TimeTools.GetMaximumDay(_quadrum, year))
            _day = TimeTools.GetMaximumDay(_quadrum, year);
        else if (_day < TimeTools.GetMinimumDay(_quadrum, year)) _day = TimeTools.GetMinimumDay(_quadrum, year);

        _displayedMessageIndex = -1;
    }

    public bool IsCurrentDate(int ticks, bool convertToAbs = false)
    {
        if (convertToAbs) ticks = GenDate.TickGameToAbs(ticks);

        return _day == GenDate.DayOfQuadrum(ticks, TimeTools.GetCurrentLocation().x) &&
               _quadrum == GenDate.Quadrum(ticks, TimeTools.GetCurrentLocation().x) &&
               _year == GenDate.Year(ticks, TimeTools.GetCurrentLocation().x);
    }

    private List<object> GetLogsToDisplay()
    {
        var logs = new List<object>();

        if (_logFilter == LogFilter.Events || _logFilter == LogFilter.All)
            logs = logs.Concat(Find.Archive.ArchivablesListForReading).ToList();

        if (_logFilter == LogFilter.Chats || _logFilter == LogFilter.All)
            logs = logs.Concat(Find.PlayLog.AllEntries).ToList();

        logs.Sort(delegate(object a, object b)
        {
            var aTime = 0;
            var bTime = 0;

            if (a is IArchivable)
                aTime = GenDate.TickGameToAbs(((IArchivable)a).CreatedTicksGame);
            else
                aTime = ((LogEntry)a).Timestamp;


            if (b is IArchivable)
                bTime = GenDate.TickGameToAbs(((IArchivable)b).CreatedTicksGame);
            else
                bTime = ((LogEntry)b).Timestamp;

            return aTime - bTime;
        });

        return logs;
    }

    private void SetSelectedAllImagesIndex(int index)
    {
        if (_allImages == null || index >= _allImages.Count) return;

        var previousSelectedAllImagesIndex = _selectedAllImagesIndex;

        _selectedAllImagesIndex = index;

        if (previousSelectedAllImagesIndex == -1)
            _dayImages = Current.Game.GetComponent<DiaryService>().ReadImages(_day, _quadrum, _year);

        if (_selectedAllImagesIndex != -1)
        {
            _draggableImage.LoadTexture(_allImages[_selectedAllImagesIndex].path);

            var entry = _allImages[_selectedAllImagesIndex];

            var dateChanged = false;

            if (_day != entry.days)
            {
                _day = entry.days;
                dateChanged = true;
            }

            if (_quadrum != entry.quadrum)
            {
                _quadrum = entry.quadrum;
                dateChanged = true;
            }

            if (_year != entry.year)
            {
                _year = entry.year;
                dateChanged = true;
            }

            if (dateChanged)
                _dayImages = Current.Game.GetComponent<DiaryService>().ReadImages(_day, _quadrum, _year);
        }
    }

    public void ScrollToEntryEnd()
    {
        _entryScrollY = float.MaxValue;
    }

    private void DoArchivableRow(Rect rect, IArchivable archivable, int index)
    {
        if (index % 2 == 1) Widgets.DrawLightHighlight(rect);

        Widgets.DrawHighlightIfMouseover(rect);
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.MiddleLeft;
        Text.WordWrap = false;

        var rect2 = rect;
        rect2.xMin += 5f;
        GUI.color = Color.white;
        var rect4 = rect2;
        var outerRect = rect2;
        outerRect.width = 30f;
        rect2.xMin += 35f;
        var archivedIcon = archivable.ArchivedIcon;
        if (archivedIcon != null)
        {
            GUI.color = archivable.ArchivedIconColor;
            Widgets.DrawTextureFitted(outerRect, archivedIcon, 0.8f);
            GUI.color = Color.white;
        }

        var rect5 = rect2;
        rect5.width = 40f;
        rect2.xMin += 45f;
        var location = Find.CurrentMap != null ? Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile) : default;
        GUI.color = new Color(0.75f, 0.75f, 0.75f);
        Widgets.Label(
            label: _fastHourStrings[
                GenDate.HourOfDay(GenDate.TickGameToAbs(archivable.CreatedTicksGame),
                    TimeTools.GetCurrentLocation().x)], rect: rect5);
        GUI.color = Color.white;
        var rect6 = rect2;
        rect6.xMax -= 40f;
        Widgets.Label(rect6, archivable.ArchivedLabel.Truncate(rect6.width));
        GenUI.ResetLabelAlign();
        Text.WordWrap = true;
        GUI.color = Color.white;

        var buttonRect = new Rect(rect2.xMax - 35f, rect2.y, 35f, rect2.height);
        if (Widgets.ButtonText(buttonRect, "+"))
        {
            var hour = archivable.CreatedTicksGame;
            var entryToAdd = archivable.ArchivedLabel.StripTags();

            if (_settings.AreDescriptionExportedWithEvents)
            {
                if (archivable is ChoiceLetter)
                {
                    var letter = (ChoiceLetter)archivable;

                    if (letter.quest != null)
                        entryToAdd += $"\n{letter.quest.description.ToString().StripTags()}\n";
                    else
                        entryToAdd += $"\n{archivable.ArchivedTooltip.StripTags()}\n";
                }
                else
                {
                    entryToAdd += $"\n{archivable.ArchivedTooltip.StripTags()}\n";
                }
            }

            Current.Game.GetComponent<DiaryService>()
                .AppendEntry(entryToAdd, _day, _quadrum, _year, true, true, TimeTools.GetHourFromTicks(hour, true));
            ScrollToEntryEnd();
        }

        if (Mouse.IsOver(rect4)) _displayedMessageIndex = index;
        if (!Widgets.ButtonInvisible(rect4)) return;
        if (Event.current.button == 1)
        {
            var lookTargets = archivable.LookTargets;
            if (CameraJumper.CanJump(lookTargets.TryGetPrimaryTarget()))
            {
                CameraJumper.TryJumpAndSelect(lookTargets.TryGetPrimaryTarget());
                Find.MainTabsRoot.EscapeCurrentTab();
            }
        }
        else
        {
            archivable.OpenArchived();
        }
    }

    private void DoLogEntryRow(Rect rect, LogEntry logEntry, int index)
    {
        if (index % 2 == 1) Widgets.DrawLightHighlight(rect);
        Widgets.DrawHighlightIfMouseover(rect);
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.MiddleLeft;
        Text.WordWrap = false;
        var rect2 = rect;
        rect2.xMin += 5f;
        GUI.color = Color.white;
        var rect4 = rect2;
        var outerRect = rect2;
        outerRect.width = 30f;
        rect2.xMin += 35f;

        Texture icon = logEntry.IconFromPOV(logEntry.GetConcerns().First());
        var iconColor = logEntry.IconColorFromPOV(logEntry.GetConcerns().First());
        if (iconColor != null) GUI.color = (Color)iconColor;
        Widgets.DrawTextureFitted(outerRect, icon, 0.8f);
        GUI.color = Color.white;

        var rect5 = rect2;
        rect5.width = 40f;
        rect2.xMin += 45f;
        var location = Find.CurrentMap != null ? Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile) : default;
        GUI.color = new Color(0.75f, 0.75f, 0.75f);
        Widgets.Label(
            label: _fastHourStrings[GenDate.HourOfDay(logEntry.Timestamp, TimeTools.GetCurrentLocation().x)],
            rect: rect5);
        GUI.color = Color.white;
        var rect6 = rect2;
        rect6.xMax -= 40f;

        Widgets.Label(rect6, logEntry.ToGameStringFromPOV(logEntry.GetConcerns().First()));

        GenUI.ResetLabelAlign();
        Text.WordWrap = true;
        GUI.color = Color.white;

        var buttonRect = new Rect(rect2.xMax - 35f, rect2.y, 35f, rect2.height);
        if (Widgets.ButtonText(buttonRect, "+"))
        {
            var entryToAdd = logEntry.ToGameStringFromPOV(logEntry.GetConcerns().First()).StripTags();

            Current.Game.GetComponent<DiaryService>().AppendEntry(entryToAdd, _day, _quadrum, _year, true, true,
                TimeTools.GetHourFromTicks(logEntry.Tick));
            ScrollToEntryEnd();
        }

        if (Mouse.IsOver(rect4)) _displayedMessageIndex = index;
        if (!Widgets.ButtonInvisible(rect4)) return;
    }

    public void DoImageDisplayContents(Rect inRect)
    {
        if (_draggableImage.HasImageLoaded()) _draggableImage.Draw(inRect);

        Widgets.EndGroup();
    }

    public override void DoWindowContents(Rect inRect)
    {
        if (!Mathf.Approximately(_lastWindowWidth, inRect.width))
        {
            _widthPerButton = inRect.width / 5;
            _lastWindowWidth = inRect.width;
        }

        var dateRect = new Rect(0f, 0f, inRect.width, 40f);
        var entryWritingRect = new Rect(0f, dateRect.yMax + 10f, inRect.width, 300f);
        var actionsRect = new Rect(0f, entryWritingRect.yMax + 10f, inRect.width, 30f);
        Text.Font = GameFont.Small;

        Widgets.BeginGroup(inRect);

        if (_settings.isConnectedToProgressRenderer && _allImages.Count > 0) DrawDiarySwitchModeButton(dateRect);

        DrawDateButtons(dateRect);

        if (_settings.isConnectedToProgressRenderer) DrawScreenshotMenuButton(dateRect);

        if (_imageDisplayMode)
        {
            DrawImageView(inRect, dateRect);

            return;
        }

        DrawEntryWritingView(entryWritingRect);
        DrawActionsView(inRect, actionsRect);
        DrawMessagesView(inRect, actionsRect);

        Widgets.EndGroup();
    }

    public void DrawDiarySwitchModeButton(Rect dateRect)
    {
        if (
            Widgets.ButtonText(new Rect(0.0f, dateRect.yMin, _widthPerButton / 2, dateRect.yMax),
                _imageDisplayMode ? "Diary".Translate() : "Diary_Images".Translate()))
        {
            _imageDisplayMode = !_imageDisplayMode;

            SetSelectedAllImagesIndex(Current.Game.GetComponent<DiaryService>()
                .GetIndexForAllImages(_day, _quadrum, _year));
        }
    }

    public void DrawDateButtons(Rect dateRect)
    {
        if (_imageDisplayMode)
        {
            if (CanAccessToPreviousEntry())
                if (Widgets.ButtonText(
                        new Rect(_widthPerButton * 0.5f, dateRect.yMin, _widthPerButton / 2, dateRect.yMax), "<"))
                    SetSelectedAllImagesIndex(_selectedAllImagesIndex - 1);
        }
        else
        {
            if (CanAccessToPreviousDay())
                if (Widgets.ButtonText(
                        new Rect(_widthPerButton * 0.5f, dateRect.yMin, _widthPerButton / 2, dateRect.yMax), "<"))
                    SetCurrentDateToPreviousDay();
        }

        if (Widgets.ButtonText(
                new Rect(_imageDisplayMode ? _widthPerButton * 1.5f : _widthPerButton * 1f, dateRect.yMin,
                    _imageDisplayMode ? _widthPerButton / 2 : _widthPerButton, dateRect.yMax),
                Find.ActiveLanguageWorker.OrdinalNumber(_day + 1)))
        {
            var list = new List<FloatMenuOption>();
            for (var i = TimeTools.GetMinimumDay(_quadrum, _year);
                 i <= TimeTools.GetMaximumDay(_quadrum, _year);
                 i++)
            {
                var current = i;
                list.Add(new FloatMenuOption(Find.ActiveLanguageWorker.OrdinalNumber(current + 1),
                    delegate { SetCurrentDay(current); }));
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }

        if (Widgets.ButtonText(new Rect(_widthPerButton * 2, dateRect.yMin, _widthPerButton, dateRect.yMax),
                _quadrum.Label()))
        {
            var list = new List<FloatMenuOption>();
            for (var q = TimeTools.GetMinimumQuadrum(_year); q <= TimeTools.GetMaximumQuadrum(_year); q++)
            {
                var current = q;
                list.Add(new FloatMenuOption(current.Label(), delegate { SetCurrentQuadrum(current); }));
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }


        if (Widgets.ButtonText(new Rect(_widthPerButton * 3, dateRect.yMin, _widthPerButton, dateRect.yMax),
                _year.ToString()))
        {
            var list = new List<FloatMenuOption>();
            for (var year = TimeTools.GetMinimumYear(); year <= TimeTools.GetMaximumYear(); year++)
            {
                var current = year;
                list.Add(new FloatMenuOption(current.ToString(), delegate { SetCurrentYear(current); }));
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }

        if (_imageDisplayMode)
        {
            if (CanAccessToNextEntry())
                if (Widgets.ButtonText(
                        new Rect(_widthPerButton * 4f, dateRect.yMin, _widthPerButton / 2, dateRect.yMax), ">"))
                    SetSelectedAllImagesIndex(_selectedAllImagesIndex + 1);
        }
        else
        {
            if (CanAccessToNextDay())
                if (Widgets.ButtonText(
                        new Rect(_widthPerButton * 4f, dateRect.yMin, _widthPerButton / 2, dateRect.yMax), ">"))
                    SetCurrentDateToNextDay();
        }
    }

    public void DrawScreenshotMenuButton(Rect dateRect)
    {
        if (Widgets.ButtonImage(new Rect(
                _widthPerButton * 5f - dateRect.yMax, dateRect.yMin, dateRect.yMax,
                dateRect.yMax), ContentFinder<Texture2D>.Get("UI/Icons/Options/OptionsGeneral"), Color.white))
        {
            var list = new List<FloatMenuOption>();

            list.Add(new FloatMenuOption("Diary_Open_Screenshot_Folder".Translate(),
                delegate
                {
                    Process.Start(
                        $"file://{Current.Game.GetComponent<DiaryService>().GetCurrentImageFolderPath()}");
                }));

            list.Add(new FloatMenuOption("Diary_Edit_Screenshot_Folder".Translate(), delegate
            {
                Find.WindowStack.Add(
                    new Dialog_EditImagesPath(Current.Game.GetComponent<DiaryService>()
                        .GetCurrentImageFolderPath())
                );
            }));

            Find.WindowStack.Add(new FloatMenu(list));
        }
    }

    public void DrawImageView(Rect inRect, Rect dateRect)
    {
        if (_dayImages != null && _dayImages.Count > 0)
        {
            var d = _allImages[_selectedAllImagesIndex];

            if (d != null &&
                Widgets.ButtonText(
                    new Rect(_widthPerButton * 1f, dateRect.yMin, _widthPerButton / 2, dateRect.yMax),
                    _fastHourStrings[d.hours]))
            {
                var list = new List<FloatMenuOption>();
                for (var i = 0; i < _dayImages.Count; i++)
                {
                    var current = i;
                    list.Add(new FloatMenuOption(_fastHourStrings[_dayImages[current].hours], delegate
                    {
                        var newIndex = _allImages.IndexOf(_dayImages[current]);

                        if (newIndex != -1)
                        {
                            _selectedAllImagesIndex = newIndex;
                            _draggableImage.LoadTexture(_allImages[newIndex].path);
                        }
                    }));
                }

                Find.WindowStack.Add(new FloatMenu(list));
            }
        }

        DoImageDisplayContents(new Rect(0f, dateRect.yMax + 10f, inRect.width, inRect.height - dateRect.yMax));
    }

    public void DrawEntryWritingView(Rect entryWritingRect)
    {
        var oldEntry = Current.Game.GetComponent<DiaryService>().ReadEntry(_day, _quadrum, _year);
        var newEntry = GUIScrollableTextArea.Draw(entryWritingRect, _textAreaName, oldEntry, ref _entryScrollY,
            Text.CurTextAreaStyle);

        if (newEntry != oldEntry)
            Current.Game.GetComponent<DiaryService>().WriteEntry(newEntry, _day, _quadrum, _year);
    }

    public void DrawActionsView(Rect inRect, Rect actionsRect)
    {
        if (Widgets.ButtonText(new Rect(0, actionsRect.yMin, _widthPerButton, 30f),
                DiaryTypeTools.GetLogFilterName(_logFilter)))
        {
            var list = new List<FloatMenuOption>();
            foreach (int i in Enum.GetValues(typeof(LogFilter)))
            {
                var current = i;
                list.Add(new FloatMenuOption(DiaryTypeTools.GetLogFilterName((LogFilter)i),
                    delegate { _logFilter = (LogFilter)current; }));
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }

        if (Widgets.ButtonText(new Rect(_widthPerButton * 4f, actionsRect.yMin, _widthPerButton, 30f),
                "Diary_Export".Translate())) Current.Game.GetComponent<DiaryService>().Export();
    }

    public void DrawMessagesView(Rect inRect, Rect actionsRect)
    {
        var logToDisplay = GetLogsToDisplay();
        var messagesRect = new Rect(0f, 0f, inRect.width / 2 - 25f, _messageLastHeight);
        var fullMessagesRect = new Rect(0f, actionsRect.yMax + 10f, inRect.width / 2 - 10f,
            inRect.height - actionsRect.yMax - 50f);
        var messageDetailsRect = new Rect(fullMessagesRect.xMax + 20f, actionsRect.yMax + 10f,
            inRect.width / 2 - 10f, inRect.height - actionsRect.yMax - 50f);
        var num = 0f;

        Widgets.BeginScrollView(fullMessagesRect, ref _messagesScrollPos, messagesRect);

        for (var num2 = logToDisplay.Count - 1; num2 >= 0; num2--)
        {
            var message = logToDisplay[num2];

            if (message is IArchivable)
            {
                var archivable = (IArchivable)message;

                if (_settings.ArchivableShouldBeIgnored(archivable)) continue;

                if (!IsCurrentDate(archivable.CreatedTicksGame, true)) continue;

                if (num2 > _displayedMessageIndex && _displayedMessageIndex == -1) _displayedMessageIndex = num2;

                if (num + 30f >= _messagesScrollPos.y && num <= _messagesScrollPos.y + inRect.height)
                    DoArchivableRow(new Rect(0f, num, messagesRect.width - 5f, 30f), archivable, num2);

                num += 30f;
            }
            else if (message is LogEntry)
            {
                var logEntry = (LogEntry)message;
                if (!IsCurrentDate(logEntry.Timestamp)) continue;

                if (num2 > _displayedMessageIndex && _displayedMessageIndex == -1) _displayedMessageIndex = num2;

                if (num + 30f >= _messagesScrollPos.y && num <= _messagesScrollPos.y + inRect.height)
                    DoLogEntryRow(new Rect(0f, num, messagesRect.width - 5f, 30f), logEntry, num2);

                num += 30f;
            }
        }

        _messageLastHeight = num;
        Widgets.EndScrollView();

        if (_displayedMessageIndex >= 0 && logToDisplay.Count > 0)
        {
            if (logToDisplay[_displayedMessageIndex] is IArchivable)
            {
                var archivable = (IArchivable)logToDisplay[_displayedMessageIndex];
                TaggedString label = archivable.ArchivedTooltip.TruncateHeight(messageDetailsRect.width - 10f,
                    messageDetailsRect.height - 10f, _truncationCache);

                if (archivable is ChoiceLetter)
                {
                    var letter = (ChoiceLetter)archivable;

                    if (letter.quest != null) label = letter.quest.description;
                }

                Widgets.Label(messageDetailsRect.ContractedBy(5f), label);
            }
            else if (logToDisplay[_displayedMessageIndex] is LogEntry)
            {
                var logEntry = (LogEntry)logToDisplay[_displayedMessageIndex];
                TaggedString label = logEntry.ToGameStringFromPOV(logEntry.GetConcerns().First())
                    .TruncateHeight(messageDetailsRect.width - 10f, messageDetailsRect.height - 10f,
                        _truncationCache);
                Widgets.Label(messageDetailsRect.ContractedBy(5f), label);
            }
        }
        else
        {
            Widgets.NoneLabel(messageDetailsRect.yMin + 3f, messageDetailsRect.width,
                "(" + "NoMessages".Translate() + ")");
        }
    }
}