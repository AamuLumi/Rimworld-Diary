using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Diary
{
    public class MainTabWindow_Diary : MainTabWindow
    {
        private readonly List<DiaryImageEntry> allImages;
        private readonly GUIDraggableTexture draggableImage;
        private readonly List<string> fastHourStrings;
        private readonly DiarySettings settings;
        private readonly Dictionary<string, string> truncationCache = new Dictionary<string, string>();
        private TextEditor currentTextEditor;
        private int day;
        private List<DiaryImageEntry> dayImages;
        private int displayedMessageIndex = -1;
        private bool imageDisplayMode;
        private LogFilter logFilter;
        private float messageLastHeight;
        private Vector2 messagesScrollPos;
        private bool moveCursorToEndAtNextFrame;
        private Quadrum quadrum = Quadrum.Aprimay;
        private int selectedAllImagesIndex;
        private int year;

        //private void CaptureScreenshotWithoutUI(int width, int height)
        //{
        //    //var cgo = UnityEngine.Object.Instantiate(Find.CameraDriver.gameObject);
        //    Camera camera = Find.CameraDriver.gameObject.GetComponent<Camera>();

        //    Log.Message(camera.transform.position.x.ToString() + ' ' + camera.transform.position.y.ToString() + ' ' + camera.transform.position.z.ToString());

        //    RenderTexture screenTexture = new RenderTexture(Screen.width, Screen.height, 16);
        //    camera.targetTexture = screenTexture;
        //    RenderTexture.active = screenTexture;

        //    Texture2D screenshot = null;

        //    try
        //    {
        //        camera.Render();

        //        screenshot = new Texture2D(width, height);
        //        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);

        //        File.WriteAllBytes(Path.Combine(Application.dataPath, "test123.jpg"), ImageConversion.EncodeToJPG(screenshot));
        //    }
        //    finally
        //    {
        //        RenderTexture.active = null;

        //        if (screenshot != null)
        //        {
        //            UnityEngine.Object.Destroy(screenshot);
        //        }
        //    }
        //}

        public MainTabWindow_Diary()
        {
            day = TimeTools.GetCurrentDay();
            quadrum = TimeTools.GetCurrentQuadrum();
            year = TimeTools.GetCurrentYear();

            fastHourStrings = new List<string>();

            for (var i = 0; i < 24; i++) fastHourStrings.Add(i + "LetterHour".Translate());

            closeOnAccept = false;

            settings = LoadedModManager.GetMod<Diary>().GetSettings<DiarySettings>();
            imageDisplayMode = false;
            logFilter = settings.DefaultLogFilter;
            draggableImage = new GUIDraggableTexture();
            selectedAllImagesIndex = -1;
            allImages = Current.Game.GetComponent<DiaryService>().GetAllImages();
            moveCursorToEndAtNextFrame = false;
        }

        public bool CanAccessToPreviousDay()
        {
            return day != TimeTools.GetMinimumDay(quadrum, year) || quadrum != TimeTools.GetMinimumQuadrum(year) ||
                   year != TimeTools.GetMinimumYear();
        }

        public bool CanAccessToPreviousEntry()
        {
            return allImages != null && selectedAllImagesIndex > 0;
        }

        public bool CanAccessToNextDay()
        {
            return day != TimeTools.GetMaximumDay(quadrum, year) || quadrum != TimeTools.GetMaximumQuadrum(year) ||
                   year != TimeTools.GetMaximumYear();
        }

        public bool CanAccessToNextEntry()
        {
            return allImages != null && selectedAllImagesIndex < allImages.Count - 1;
        }

        public void SetCurrentDateToPreviousDay()
        {
            if (!CanAccessToPreviousDay()) return;

            displayedMessageIndex = -1;

            if (day > TimeTools.GetMinimumDay(quadrum, year))
            {
                day--;

                return;
            }

            day = GenDate.DaysPerQuadrum - 1;

            if (quadrum > Quadrum.Aprimay)
            {
                quadrum--;

                return;
            }

            year--;
            quadrum = Quadrum.Decembary;
        }

        public void SetCurrentDateToNextDay()
        {
            if (!CanAccessToNextDay()) return;

            displayedMessageIndex = -1;

            if (day < GenDate.DaysPerQuadrum - 1)
            {
                day++;

                return;
            }

            day = 0;

            if (quadrum < Quadrum.Decembary)
            {
                quadrum++;

                return;
            }

            year++;
            quadrum = Quadrum.Aprimay;
        }

        public void SetCurrentDay(int day)
        {
            this.day = day;

            displayedMessageIndex = -1;
        }

        public void SetCurrentQuadrum(Quadrum q)
        {
            quadrum = q;

            if (day > TimeTools.GetMaximumDay(quadrum, year))
                day = TimeTools.GetMaximumDay(quadrum, year);
            else if (day < TimeTools.GetMinimumDay(quadrum, year)) day = TimeTools.GetMinimumDay(quadrum, year);

            displayedMessageIndex = -1;
        }

        public void SetCurrentYear(int year)
        {
            this.year = year;

            if (quadrum > TimeTools.GetMaximumQuadrum(year))
                quadrum = TimeTools.GetMaximumQuadrum(year);
            else if (quadrum < TimeTools.GetMinimumQuadrum(year)) quadrum = TimeTools.GetMinimumQuadrum(year);

            if (day > TimeTools.GetMaximumDay(quadrum, year))
                day = TimeTools.GetMaximumDay(quadrum, year);
            else if (day < TimeTools.GetMinimumDay(quadrum, year)) day = TimeTools.GetMinimumDay(quadrum, year);

            displayedMessageIndex = -1;
        }

        public bool IsCurrentDate(int ticks, bool convertToAbs = false)
        {
            if (convertToAbs) ticks = GenDate.TickGameToAbs(ticks);

            return day == GenDate.DayOfQuadrum(ticks, TimeTools.GetCurrentLocation().x) &&
                   quadrum == GenDate.Quadrum(ticks, TimeTools.GetCurrentLocation().x) &&
                   year == GenDate.Year(ticks, TimeTools.GetCurrentLocation().x);
        }

        private List<object> GetLogsToDisplay()
        {
            var logs = new List<object>();

            if (logFilter == LogFilter.Events || logFilter == LogFilter.All)
                logs = logs.Concat(Find.Archive.ArchivablesListForReading).ToList();

            if (logFilter == LogFilter.Chats || logFilter == LogFilter.All)
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
            if (allImages == null || index >= allImages.Count) return;

            var previousSelectedAllImagesIndex = selectedAllImagesIndex;

            selectedAllImagesIndex = index;

            if (previousSelectedAllImagesIndex == -1)
                dayImages = Current.Game.GetComponent<DiaryService>().ReadImages(day, quadrum, year);

            if (selectedAllImagesIndex != -1)
            {
                draggableImage.LoadTexture(allImages[selectedAllImagesIndex].Path);

                var entry = allImages[selectedAllImagesIndex];

                var dateChanged = false;

                if (day != entry.Days)
                {
                    day = entry.Days;
                    dateChanged = true;
                }

                if (quadrum != entry.Quadrum)
                {
                    quadrum = entry.Quadrum;
                    dateChanged = true;
                }

                if (year != entry.Year)
                {
                    year = entry.Year;
                    dateChanged = true;
                }

                if (dateChanged) dayImages = Current.Game.GetComponent<DiaryService>().ReadImages(day, quadrum, year);
            }
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
                label: fastHourStrings[
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
                var entryToAdd = archivable.ArchivedLabel.StripTags();

                if (settings.AreDescriptionExportedWithEvents)
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

                Current.Game.GetComponent<DiaryService>().AppendEntry(entryToAdd, day, quadrum, year);
                GUI.FocusControl("DiaryTextArea");
                // We need to wait the refresh of the area with the new text to move the cursor
                moveCursorToEndAtNextFrame = true;
            }

            if (Mouse.IsOver(rect4)) displayedMessageIndex = index;
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
                label: fastHourStrings[GenDate.HourOfDay(logEntry.Timestamp, TimeTools.GetCurrentLocation().x)],
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

                Current.Game.GetComponent<DiaryService>().AppendEntry(entryToAdd, day, quadrum, year);
                GUI.FocusControl("DiaryTextArea");
                // We need to wait the refresh of the area with the new text to move the cursor
                moveCursorToEndAtNextFrame = true;
            }

            if (Mouse.IsOver(rect4)) displayedMessageIndex = index;
            if (!Widgets.ButtonInvisible(rect4)) return;
        }

        public void DoImageDisplayContents(Rect inRect)
        {
            if (draggableImage.HasImageLoaded()) draggableImage.Draw(inRect);

            Widgets.EndGroup();
        }

        public override void DoWindowContents(Rect inRect)
        {
            var dateRect = new Rect(0f, 0f, inRect.width, 40f);
            var widthPerButton = inRect.width / 5;

            Text.Font = GameFont.Small;
            Widgets.BeginGroup(inRect);

            if (allImages.Count > 0 &&
                Widgets.ButtonText(new Rect(0.0f, dateRect.yMin, widthPerButton / 2, dateRect.yMax),
                    imageDisplayMode ? "Diary".Translate() : "Diary_Images".Translate()))
            {
                imageDisplayMode = !imageDisplayMode;

                SetSelectedAllImagesIndex(Current.Game.GetComponent<DiaryService>()
                    .GetIndexForAllImages(day, quadrum, year));
            }

            if (imageDisplayMode)
            {
                if (CanAccessToPreviousEntry())
                    if (Widgets.ButtonText(
                            new Rect(widthPerButton * 0.5f, dateRect.yMin, widthPerButton / 2, dateRect.yMax), "<"))
                        SetSelectedAllImagesIndex(selectedAllImagesIndex - 1);
            }
            else
            {
                if (CanAccessToPreviousDay())
                    if (Widgets.ButtonText(
                            new Rect(widthPerButton * 0.5f, dateRect.yMin, widthPerButton / 2, dateRect.yMax), "<"))
                        SetCurrentDateToPreviousDay();
            }

            if (Widgets.ButtonText(
                    new Rect(imageDisplayMode ? widthPerButton * 1.5f : widthPerButton * 1f, dateRect.yMin,
                        imageDisplayMode ? widthPerButton / 2 : widthPerButton, dateRect.yMax),
                    Find.ActiveLanguageWorker.OrdinalNumber(day + 1)))
            {
                var list = new List<FloatMenuOption>();
                for (var i = TimeTools.GetMinimumDay(quadrum, year); i <= TimeTools.GetMaximumDay(quadrum, year); i++)
                {
                    var current = i;
                    list.Add(new FloatMenuOption(Find.ActiveLanguageWorker.OrdinalNumber(current + 1),
                        delegate { SetCurrentDay(current); }));
                }

                Find.WindowStack.Add(new FloatMenu(list));
            }

            if (Widgets.ButtonText(new Rect(widthPerButton * 2, dateRect.yMin, widthPerButton, dateRect.yMax),
                    quadrum.Label()))
            {
                var list = new List<FloatMenuOption>();
                for (var q = TimeTools.GetMinimumQuadrum(year); q <= TimeTools.GetMaximumQuadrum(year); q++)
                {
                    var current = q;
                    list.Add(new FloatMenuOption(current.Label(), delegate { SetCurrentQuadrum(current); }));
                }

                Find.WindowStack.Add(new FloatMenu(list));
            }


            if (Widgets.ButtonText(new Rect(widthPerButton * 3, dateRect.yMin, widthPerButton, dateRect.yMax),
                    this.year.ToString()))
            {
                var list = new List<FloatMenuOption>();
                for (var year = TimeTools.GetMinimumYear(); year <= TimeTools.GetMaximumYear(); year++)
                {
                    var current = year;
                    list.Add(new FloatMenuOption(current.ToString(), delegate { SetCurrentYear(current); }));
                }

                Find.WindowStack.Add(new FloatMenu(list));
            }

            if (imageDisplayMode)
            {
                if (CanAccessToNextEntry())
                    if (Widgets.ButtonText(
                            new Rect(widthPerButton * 4f, dateRect.yMin, widthPerButton / 2, dateRect.yMax), ">"))
                        SetSelectedAllImagesIndex(selectedAllImagesIndex + 1);
            }
            else
            {
                if (CanAccessToNextDay())
                    if (Widgets.ButtonText(
                            new Rect(widthPerButton * 4f, dateRect.yMin, widthPerButton / 2, dateRect.yMax), ">"))
                        SetCurrentDateToNextDay();
            }


            if (imageDisplayMode)
            {
                if (dayImages != null && dayImages.Count > 0)
                {
                    var d = allImages[selectedAllImagesIndex];

                    if (d != null &&
                        Widgets.ButtonText(
                            new Rect(widthPerButton * 1f, dateRect.yMin, widthPerButton / 2, dateRect.yMax),
                            fastHourStrings[d.Hours]))
                    {
                        var list = new List<FloatMenuOption>();
                        for (var i = 0; i < dayImages.Count; i++)
                        {
                            var current = i;
                            list.Add(new FloatMenuOption(fastHourStrings[dayImages[current].Hours], delegate
                            {
                                var newIndex = allImages.IndexOf(dayImages[current]);

                                if (newIndex != -1)
                                {
                                    selectedAllImagesIndex = newIndex;
                                    draggableImage.LoadTexture(allImages[newIndex].Path);
                                }
                            }));
                        }

                        Find.WindowStack.Add(new FloatMenu(list));
                    }
                }

                DoImageDisplayContents(new Rect(0f, dateRect.yMax + 10f, inRect.width, inRect.height - dateRect.yMax));

                return;
            }

            var entryWritingRect = new Rect(0f, dateRect.yMax + 10f, inRect.width, 300f);
            GUI.SetNextControlName("DiaryTextArea");
            var newEntry = Widgets.TextArea(entryWritingRect,
                Current.Game.GetComponent<DiaryService>().ReadEntry(day, quadrum, this.year));
            Current.Game.GetComponent<DiaryService>().WriteEntry(newEntry, day, quadrum, this.year);
            var controlID = GUIUtility.GetControlID(entryWritingRect.GetHashCode(), FocusType.Keyboard);
            currentTextEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), controlID - 1);

            if (moveCursorToEndAtNextFrame)
            {
                if (currentTextEditor != null)
                {
                    currentTextEditor.MoveTextEnd();
                    var e = new Event();
                    e.type = EventType.KeyDown;
                    currentTextEditor.UpdateScrollOffsetIfNeeded(e);
                }

                moveCursorToEndAtNextFrame = false;
            }

            var actionsRect = new Rect(0f, entryWritingRect.yMax + 10f, inRect.width, 30f);

            if (Widgets.ButtonText(new Rect(0, actionsRect.yMin, widthPerButton, 30f),
                    DiaryTypeTools.GetLogFilterName(logFilter)))
            {
                var list = new List<FloatMenuOption>();
                foreach (int i in Enum.GetValues(typeof(LogFilter)))
                {
                    var current = i;
                    list.Add(new FloatMenuOption(DiaryTypeTools.GetLogFilterName((LogFilter)i),
                        delegate { logFilter = (LogFilter)current; }));
                }

                Find.WindowStack.Add(new FloatMenu(list));
            }

            if (Widgets.ButtonText(new Rect(widthPerButton * 4f, actionsRect.yMin, widthPerButton, 30f),
                    "Diary_Export".Translate())) Current.Game.GetComponent<DiaryService>().Export();

            var logToDisplay = GetLogsToDisplay();
            var messagesRect = new Rect(0f, 0f, inRect.width / 2 - 25f, messageLastHeight);
            var fullMessagesRect = new Rect(0f, actionsRect.yMax + 10f, inRect.width / 2 - 10f,
                inRect.height - actionsRect.yMax - 50f);
            var messageDetailsRect = new Rect(fullMessagesRect.xMax + 20f, actionsRect.yMax + 10f,
                inRect.width / 2 - 10f, inRect.height - actionsRect.yMax - 50f);
            var num = 0f;

            Widgets.BeginScrollView(fullMessagesRect, ref messagesScrollPos, messagesRect);

            for (var num2 = logToDisplay.Count - 1; num2 >= 0; num2--)
            {
                var message = logToDisplay[num2];

                if (message is IArchivable)
                {
                    var archivable = (IArchivable)message;

                    if (settings.ArchivableShouldBeIgnored(archivable)) continue;

                    if (!IsCurrentDate(archivable.CreatedTicksGame, true)) continue;

                    if (num2 > displayedMessageIndex && displayedMessageIndex == -1) displayedMessageIndex = num2;

                    if (num + 30f >= messagesScrollPos.y && num <= messagesScrollPos.y + inRect.height)
                        DoArchivableRow(new Rect(0f, num, messagesRect.width - 5f, 30f), archivable, num2);

                    num += 30f;
                }
                else if (message is LogEntry)
                {
                    var logEntry = (LogEntry)message;
                    if (!IsCurrentDate(logEntry.Timestamp)) continue;

                    if (num2 > displayedMessageIndex && displayedMessageIndex == -1) displayedMessageIndex = num2;

                    if (num + 30f >= messagesScrollPos.y && num <= messagesScrollPos.y + inRect.height)
                        DoLogEntryRow(new Rect(0f, num, messagesRect.width - 5f, 30f), logEntry, num2);

                    num += 30f;
                }
            }

            messageLastHeight = num;
            Widgets.EndScrollView();

            if (displayedMessageIndex >= 0 && logToDisplay.Count > 0)
            {
                if (logToDisplay[displayedMessageIndex] is IArchivable)
                {
                    var archivable = (IArchivable)logToDisplay[displayedMessageIndex];
                    TaggedString label = archivable.ArchivedTooltip.TruncateHeight(messageDetailsRect.width - 10f,
                        messageDetailsRect.height - 10f, truncationCache);

                    if (archivable is ChoiceLetter)
                    {
                        var letter = (ChoiceLetter)archivable;

                        if (letter.quest != null) label = letter.quest.description;
                    }

                    Widgets.Label(messageDetailsRect.ContractedBy(5f), label);
                }
                else if (logToDisplay[displayedMessageIndex] is LogEntry)
                {
                    var logEntry = (LogEntry)logToDisplay[displayedMessageIndex];
                    TaggedString label = logEntry.ToGameStringFromPOV(logEntry.GetConcerns().First())
                        .TruncateHeight(messageDetailsRect.width - 10f, messageDetailsRect.height - 10f,
                            truncationCache);
                    Widgets.Label(messageDetailsRect.ContractedBy(5f), label);
                }
            }
            else
            {
                Widgets.NoneLabel(messageDetailsRect.yMin + 3f, messageDetailsRect.width,
                    "(" + "NoMessages".Translate() + ")");
            }

            Widgets.EndGroup();
        }
    }
}