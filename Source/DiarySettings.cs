using System;
using System.Collections.Generic;

using UnityEngine;
using RimWorld;
using Verse;

namespace Diary
{
    public class DiarySettings : ModSettings
    {
        private string folderPath;
        private ExportFormat exportFormat;
        private DefaultMessage defaultMessage;
        private LogWriterFilter logWriterFilter;
        private LogFilter defaultLogFilter;

        public bool ConnectedToProgressRenderer;

        public string FolderPath
        {
            get { return folderPath; }
        }

        public ExportFormat ExportFormat
        {
            get { return exportFormat; }
        }

        public DefaultMessage DefaultMessage
        {
            get { return defaultMessage; }
        }

        public LogWriterFilter LogWriterFilter
        {
            get { return logWriterFilter; }
        }

        public LogFilter DefaultLogFilter
        {
            get { return defaultLogFilter; }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            ConnectedToProgressRenderer = false;

            Scribe_Values.Look(ref folderPath, "folderPath", Application.dataPath);
            Scribe_Values.Look(ref exportFormat, "exportFormat", ExportFormat.Text);
            Scribe_Values.Look(ref defaultMessage, "defaultMessage", DefaultMessage.Empty);
            Scribe_Values.Look(ref logWriterFilter, "logWriterFilter", LogWriterFilter.None);
            Scribe_Values.Look(ref defaultLogFilter, "defaultLogFilter", LogFilter.Events);
        }

        public void SetFormat(ExportFormat f)
        {
            exportFormat = f;
        }

        public void SetDefaultMessage(DefaultMessage m)
        {
            defaultMessage = m;
        }
        public void SetDefaultLogFilter(LogFilter f)
        {
            defaultLogFilter = f;
        }

        public void SetLogWriterFilter(LogWriterFilter f)
        {
            logWriterFilter = f;
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.Label("Diary_Export_Folder_Path".Translate());
            folderPath = listingStandard.TextEntry(folderPath);

            if (listingStandard.ButtonText("Diary_Reset_To_Default_Path".Translate()))
            {
                folderPath = Application.dataPath;
            }

            listingStandard.Label("Diary_Export_Format".Translate());
            if (listingStandard.ButtonText(DiaryTypeTools.GetFormatName(exportFormat)))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (int i in Enum.GetValues(typeof(ExportFormat)))
                {
                    int current = i;
                    list.Add(new FloatMenuOption(DiaryTypeTools.GetFormatName((ExportFormat)i), delegate
                    {
                        SetFormat((ExportFormat)current);
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }

            listingStandard.Label("Diary_Default_Log_Filter".Translate());
            if (listingStandard.ButtonText(DiaryTypeTools.GetLogFilterName(defaultLogFilter)))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (int i in Enum.GetValues(typeof(LogFilter)))
                {
                    int current = i;
                    list.Add(new FloatMenuOption(DiaryTypeTools.GetLogFilterName((LogFilter)i), delegate
                    {
                        SetDefaultLogFilter((LogFilter)current);
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }

            listingStandard.Label("Diary_Log_Writer_Filter_Explanation".Translate());

            listingStandard.Label("Diary_Log_Writer_Filter".Translate());
            if (listingStandard.ButtonText(DiaryTypeTools.GetLogWriterFilterName(logWriterFilter)))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (int i in Enum.GetValues(typeof(LogWriterFilter)))
                {
                    int current = i;
                    list.Add(new FloatMenuOption(DiaryTypeTools.GetLogWriterFilterName((LogWriterFilter)i), delegate
                    {
                        SetLogWriterFilter((LogWriterFilter)current);
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }

            if (ConnectedToProgressRenderer)
            {
                listingStandard.Label("Diary_Connected_To_Progress_Renderer".Translate());
            }
            else
            {
                listingStandard.Label("Diary_Not_Connected_To_Progress_Renderer".Translate());
            }

            listingStandard.End();
        }
    }
}
