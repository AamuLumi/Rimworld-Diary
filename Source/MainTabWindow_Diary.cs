using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld.Planet;
using static HarmonyLib.Code;

namespace Diary
{
    public class MainTabWindow_Diary : MainTabWindow
    {
        private int day = 0;
        private Quadrum quadrum = Quadrum.Aprimay;
        private int year = 0;
        private Vector2 messagesScrollPos;
        private int displayedMessageIndex = -1;
        private float messageLastHeight = 0f;

        private readonly List<string> fastHourStrings;
        private Dictionary<string, string> truncationCache = new Dictionary<string, string>();

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
            this.day = GetCurrentDay();
            this.quadrum = GetCurrentQuadrum();
            this.year = GetCurrentYear();

            fastHourStrings = new List<string>();

            for (int i = 0; i < 24; i++)
            {
                fastHourStrings.Add(i + (string)"LetterHour".Translate());
            }

            this.closeOnAccept = false;
        }

        public Vector2 GetCurrentLocation()
        {
            Vector2 vector;

            if (WorldRendererUtility.WorldRenderedNow && Find.WorldSelector.selectedTile >= 0)
            {
                vector = Find.WorldGrid.LongLatOf(Find.WorldSelector.selectedTile);
            }
            else if (WorldRendererUtility.WorldRenderedNow && Find.WorldSelector.NumSelectedObjects > 0)
            {
                vector = Find.WorldGrid.LongLatOf(Find.WorldSelector.FirstSelectedObject.Tile);
            }
            else
            {
                if (Find.CurrentMap == null)
                {
                    return new Vector2();
                }
                vector = Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile);
            }

            return vector;
        }

        public int GetCurrentDay()
        {
            return GenDate.DayOfQuadrum(Find.TickManager.TicksAbs, GetCurrentLocation().x);
        }

        public Quadrum GetCurrentQuadrum()
        {
            return GenDate.Quadrum(Find.TickManager.TicksAbs, GetCurrentLocation().x);
        }

        public int GetCurrentYear()
        {
            return GenDate.Year(Find.TickManager.TicksAbs, GetCurrentLocation().x);
        }

        public int GetMinimumDay()
        {
            if (this.quadrum != GenDate.Quadrum(Find.TickManager.gameStartAbsTick, GetCurrentLocation().x) || this.year != GenDate.Year(Find.TickManager.gameStartAbsTick, GetCurrentLocation().x))
            {
                return 0;
            }

            return GenDate.DayOfQuadrum(Find.TickManager.gameStartAbsTick, GetCurrentLocation().x);
        }

        public Quadrum GetMinimumQuadrum()
        {
            if (this.year != GenDate.Year(Find.TickManager.gameStartAbsTick, GetCurrentLocation().x))
            {
                return Quadrum.Aprimay;
            }

            return GenDate.Quadrum(Find.TickManager.gameStartAbsTick, GetCurrentLocation().x);
        }

        public int GetMinimumYear()
        {
            return GenDate.Year(Find.TickManager.gameStartAbsTick, GetCurrentLocation().x);
        }

        public int GetMaximumDay()
        {
            if (this.quadrum != GetCurrentQuadrum() || this.year != GetCurrentYear())
            {
                return GenDate.DaysPerQuadrum - 1;
            }

            return GetCurrentDay();
        }

        public Quadrum GetMaximumQuadrum()
        {
            if (this.year != GetCurrentYear())
            {
                return Quadrum.Undefined - 1;
            }

            return GetCurrentQuadrum();
        }

        public int GetMaximumYear()
        {
            return GetCurrentYear();
        }
        public bool CanAccessToPreviousDay()
        {
            return this.day != GetMinimumDay() || this.quadrum != GetMinimumQuadrum() || this.year != GetMinimumYear();
        }

        public bool CanAccessToNextDay()
        {
            return this.day != GetMaximumDay() || this.quadrum != GetMaximumQuadrum() || this.year != GetMaximumYear();
        }

        public void SetCurrentDateToPreviousDay()
        {
            if (!CanAccessToPreviousDay())
            {
                return;
            }

            displayedMessageIndex = -1;

            if (this.day > GetMinimumDay())
            {
                this.day--;

                return;
            }

            this.day = GenDate.DaysPerQuadrum - 1;

            if (this.quadrum > Quadrum.Aprimay)
            {
                this.quadrum--;

                return;
            }

            this.year--;
            this.quadrum = Quadrum.Decembary;
        }

        public void SetCurrentDateToNextDay()
        {
            if (!CanAccessToNextDay())
            {
                return;
            }

            displayedMessageIndex = -1;

            if (this.day < GenDate.DaysPerQuadrum - 1)
            {
                this.day++;

                return;
            }

            this.day = 0;

            if (this.quadrum < Quadrum.Decembary)
            {
                this.quadrum++;

                return;
            }

            this.year++;
            this.quadrum = Quadrum.Aprimay;
        }

        public void SetCurrentDay(int day)
        {
            this.day = day;

            displayedMessageIndex = -1;
        }

        public void SetCurrentQuadrum(Quadrum q)
        {
            this.quadrum = q;

            if (this.day > GetMaximumDay())
            {
                this.day = GetMaximumDay();
            }
            else if (this.day < GetMinimumDay())
            {
                this.day = GetMinimumDay();
            }

            displayedMessageIndex = -1;
        }

        public void SetCurrentYear(int year)
        {
            this.year = year;

            if (this.quadrum > GetMaximumQuadrum())
            {
                this.quadrum = GetMaximumQuadrum();
            }
            else if (this.quadrum < GetMinimumQuadrum())
            {
                this.quadrum = GetMinimumQuadrum();
            }

            if (this.day > GetMaximumDay())
            {
                this.day = GetMaximumDay();
            }
            else if (this.day < GetMinimumDay())
            {
                this.day = GetMinimumDay();
            }

            displayedMessageIndex = -1;
        }

        public bool IsCurrentDate(int ticks)
        {
            int absTicks = GenDate.TickGameToAbs(ticks);

            return this.day == GenDate.DayOfQuadrum(absTicks, GetCurrentLocation().x) && this.quadrum == GenDate.Quadrum(absTicks, GetCurrentLocation().x) && this.year == GenDate.Year(absTicks, GetCurrentLocation().x);
        }

        private void DoArchivableRow(Rect rect, IArchivable archivable, int index)
        {
            if (index % 2 == 1)
            {
                Widgets.DrawLightHighlight(rect);
            }
            Widgets.DrawHighlightIfMouseover(rect);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.WordWrap = false;
            Rect rect2 = rect;
            rect2.xMin += 35f;
            GUI.color = Color.white;
            Rect rect4 = rect2;
            Rect outerRect = rect2;
            outerRect.width = 30f;
            rect2.xMin += 35f;
            Texture archivedIcon = archivable.ArchivedIcon;
            if (archivedIcon != null)
            {
                GUI.color = archivable.ArchivedIconColor;
                Widgets.DrawTextureFitted(outerRect, archivedIcon, 0.8f);
                GUI.color = Color.white;
            }
            Rect rect5 = rect2;
            rect5.width = 40f;
            rect2.xMin += 45f;
            Vector2 location = ((Find.CurrentMap != null) ? Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile) : default(Vector2));
            GUI.color = new Color(0.75f, 0.75f, 0.75f);
            Widgets.Label(label: fastHourStrings[GenDate.HourOfDay(GenDate.TickGameToAbs(archivable.CreatedTicksGame), GetCurrentLocation().x)], rect: rect5);
            GUI.color = Color.white;
            Rect rect6 = rect2;
            Widgets.Label(rect6, archivable.ArchivedLabel.Truncate(rect6.width));
            GenUI.ResetLabelAlign();
            Text.WordWrap = true;
            GUI.color = Color.white;
            if (Mouse.IsOver(rect4))
            {
                displayedMessageIndex = index;
            }
            if (!Widgets.ButtonInvisible(rect4))
            {
                return;
            }
            if (Event.current.button == 1)
            {
                LookTargets lookTargets = archivable.LookTargets;
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

        public override void DoWindowContents(Rect inRect)
        {
            Rect dateRect = new Rect(0f, 0f, inRect.width, 40f);
            float widthPerButton = inRect.width / 5;

            Widgets.BeginGroup(inRect);

            if (CanAccessToPreviousDay())
            {
                if (Widgets.ButtonText(new Rect(widthPerButton * 0.5f, dateRect.yMin, widthPerButton / 2, dateRect.yMax), "<"))
                {
                    SetCurrentDateToPreviousDay();
                }
            }

            if (Widgets.ButtonText(new Rect(widthPerButton * 1, dateRect.yMin, widthPerButton, dateRect.yMax), Find.ActiveLanguageWorker.OrdinalNumber(this.day + 1)))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                for (int i = GetMinimumDay(); i <= GetMaximumDay(); i++)
                {
                    int current = i;
                    list.Add(new FloatMenuOption(Find.ActiveLanguageWorker.OrdinalNumber(current + 1), delegate
                    {
                        SetCurrentDay(current);
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }

            if (Widgets.ButtonText(new Rect(widthPerButton * 2, dateRect.yMin, widthPerButton, dateRect.yMax), QuadrumUtility.Label(this.quadrum)))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                for (Quadrum q = GetMinimumQuadrum(); q <= GetMaximumQuadrum(); q++)
                {
                    Quadrum current = q;
                    list.Add(new FloatMenuOption(QuadrumUtility.Label(current), delegate
                    {
                        SetCurrentQuadrum(current);
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }


            if (Widgets.ButtonText(new Rect(widthPerButton * 3, dateRect.yMin, widthPerButton, dateRect.yMax), this.year.ToString()))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                for (int year = GetMinimumYear(); year <= GetMaximumYear(); year++)
                {
                    int current = year;
                    list.Add(new FloatMenuOption(current.ToString(), delegate
                    {
                        SetCurrentYear(current);
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }

            if (CanAccessToNextDay())
            {
                if (Widgets.ButtonText(new Rect(widthPerButton * 4f, dateRect.yMin, widthPerButton / 2, dateRect.yMax), ">"))
                {
                    SetCurrentDateToNextDay();
                }
            }

            Rect entryWritingRect = new Rect(0f, dateRect.yMax + 10f, inRect.width, 300f);
            Current.Game.GetComponent<DiaryService>().WriteEntry(Widgets.TextArea(entryWritingRect, Current.Game.GetComponent<DiaryService>().ReadEntry(this.day, this.quadrum, this.year)), this.day, this.quadrum, this.year);

            List<IArchivable> archivablesListForReading = Find.Archive.ArchivablesListForReading;
            Rect messagesRect = new Rect(0f, 0f, inRect.width / 2 - 10f, messageLastHeight);
            Rect fullMessagesRect = new Rect(0f, entryWritingRect.yMax + 10f, inRect.width / 2 - 10f, inRect.height - entryWritingRect.yMax - 50f);
            Rect messageDetailsRect = new Rect(fullMessagesRect.xMax + 20f, entryWritingRect.yMax + 10f, inRect.width / 2 - 10f, inRect.height - entryWritingRect.yMax - 50f);
            float num = 0f;

            Widgets.BeginScrollView(fullMessagesRect, ref messagesScrollPos, messagesRect);

            for (int num2 = archivablesListForReading.Count - 1; num2 >= 0; num2--)
            {
                IArchivable message = archivablesListForReading[num2];

                if (IsCurrentDate(message.CreatedTicksGame))
                {
                    if (num2 > displayedMessageIndex && displayedMessageIndex == -1)
                    {
                        displayedMessageIndex = num2;
                    }

                    if (num + 30f >= messagesScrollPos.y && num <= messagesScrollPos.y + inRect.height)
                    {
                        DoArchivableRow(new Rect(0f, num, messagesRect.width - 5f, 30f), message, num2);
                    }

                    num += 30f;
                }
            }
            messageLastHeight = num;
            Widgets.EndScrollView();

            if (displayedMessageIndex >= 0 && archivablesListForReading.Count > 0)
            {
                TaggedString label = archivablesListForReading[displayedMessageIndex].ArchivedTooltip.TruncateHeight(messageDetailsRect.width - 10f, messageDetailsRect.height - 10f, truncationCache);
                Widgets.Label(messageDetailsRect.ContractedBy(5f), label);
            }
            else
            {
                Widgets.NoneLabel(messageDetailsRect.yMin + 3f, messageDetailsRect.width, "(" + "NoMessages".Translate() + ")");
            }

            Rect actionsRect = new Rect(0f, fullMessagesRect.yMax + 10f, inRect.width, 30f);

            if (Widgets.ButtonText(new Rect(widthPerButton * 4f, actionsRect.yMin, widthPerButton, 30f), "Diary_Export".Translate()))
            {
                Current.Game.GetComponent<DiaryService>().Export();
            }

            Widgets.EndGroup();
        }
    }
}
