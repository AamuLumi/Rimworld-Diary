using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Diary
{
    public class DiarySettings : ModSettings
    {
        private bool _previousAutomaticExportEnabled;
        private bool automaticExportEnabled;
        private AutomaticExportPeriod automaticExportPeriod;

        public bool ConnectedToProgressRenderer;
        private LogFilter defaultLogFilter;
        private DefaultMessage defaultMessage;
        private ExportFormat exportFormat;
        private string folderPath;
        private LogWriterFilter logWriterFilter;

        public string FolderPath => folderPath;

        public ExportFormat ExportFormat => exportFormat;

        public DefaultMessage DefaultMessage => defaultMessage;

        public LogWriterFilter LogWriterFilter => logWriterFilter;

        public LogFilter DefaultLogFilter => defaultLogFilter;

        public bool AutomaticExportEnabled => automaticExportEnabled;

        public AutomaticExportPeriod AutomaticExportPeriod => automaticExportPeriod;

        public override void ExposeData()
        {
            base.ExposeData();

            ConnectedToProgressRenderer = false;

            Scribe_Values.Look(ref folderPath, "folderPath", Application.dataPath);
            Scribe_Values.Look(ref exportFormat, "exportFormat");
            Scribe_Values.Look(ref defaultMessage, "defaultMessage", DefaultMessage.Empty);
            Scribe_Values.Look(ref logWriterFilter, "logWriterFilter", LogWriterFilter.None);
            Scribe_Values.Look(ref defaultLogFilter, "defaultLogFilter", LogFilter.Events);
            Scribe_Values.Look(ref automaticExportEnabled, "automaticExportEnabled");
            Scribe_Values.Look(ref automaticExportPeriod, "automaticExportPeriod");
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

        public void SetAutomaticExportEnabled(bool b)
        {
            automaticExportEnabled = b;

            UpdateGameComponent();
        }

        private void UpdateGameComponent()
        {
            try
            {
                var gameComponent = Current.Game.GetComponent<GameComponent_AutomaticExport>();


                gameComponent?.OnSettingsUpdate(automaticExportEnabled, automaticExportPeriod);
            }
            catch (NullReferenceException)
            {
                // Do nothing, gameComponent is not loaded
            }
        }

        public void SetAutomaticExportPeriod(AutomaticExportPeriod p)
        {
            automaticExportPeriod = p;

            UpdateGameComponent();
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            if (automaticExportEnabled != _previousAutomaticExportEnabled) UpdateGameComponent();

            _previousAutomaticExportEnabled = automaticExportEnabled;


            var listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.Label("Diary_Export_Folder_Path".Translate());
            folderPath = listingStandard.TextEntry(folderPath);

            if (listingStandard.ButtonText("Diary_Reset_To_Default_Path".Translate()))
                folderPath = Application.dataPath;

            listingStandard.GapLine();

            listingStandard.Label("Diary_Export_Format".Translate());
            if (listingStandard.ButtonText(DiaryTypeTools.GetFormatName(exportFormat)))
            {
                var list = new List<FloatMenuOption>();
                foreach (int i in Enum.GetValues(typeof(ExportFormat)))
                {
                    var current = i;
                    list.Add(new FloatMenuOption(DiaryTypeTools.GetFormatName((ExportFormat)i),
                        delegate { SetFormat((ExportFormat)current); }));
                }

                Find.WindowStack.Add(new FloatMenu(list));
            }

            listingStandard.GapLine();

            listingStandard.Label("Diary_Automatic_Export".Translate());

            listingStandard.CheckboxLabeled("Diary_Automatic_Export_Enabled".Translate(), ref automaticExportEnabled);

            listingStandard.Label("Diary_Automatic_Export_Period".Translate());

            if (listingStandard.ButtonText(DiaryTypeTools.GetAutomaticExportPeriodName(automaticExportPeriod)))
            {
                var list = new List<FloatMenuOption>();
                foreach (int i in Enum.GetValues(typeof(AutomaticExportPeriod)))
                {
                    var current = i;
                    list.Add(new FloatMenuOption(DiaryTypeTools.GetAutomaticExportPeriodName((AutomaticExportPeriod)i),
                        delegate { SetAutomaticExportPeriod((AutomaticExportPeriod)current); }));
                }

                Find.WindowStack.Add(new FloatMenu(list));
            }

            listingStandard.GapLine();

            listingStandard.Label("Diary_Default_Log_Filter".Translate());
            if (listingStandard.ButtonText(DiaryTypeTools.GetLogFilterName(defaultLogFilter)))
            {
                var list = new List<FloatMenuOption>();
                foreach (int i in Enum.GetValues(typeof(LogFilter)))
                {
                    var current = i;
                    list.Add(new FloatMenuOption(DiaryTypeTools.GetLogFilterName((LogFilter)i),
                        delegate { SetDefaultLogFilter((LogFilter)current); }));
                }

                Find.WindowStack.Add(new FloatMenu(list));
            }

            listingStandard.Gap();

            listingStandard.Label("Diary_Log_Writer_Filter".Translate());
            listingStandard.Label("Diary_Log_Writer_Filter_Explanation".Translate());
            if (listingStandard.ButtonText(DiaryTypeTools.GetLogWriterFilterName(logWriterFilter)))
            {
                var list = new List<FloatMenuOption>();
                foreach (int i in Enum.GetValues(typeof(LogWriterFilter)))
                {
                    var current = i;
                    list.Add(new FloatMenuOption(DiaryTypeTools.GetLogWriterFilterName((LogWriterFilter)i),
                        delegate { SetLogWriterFilter((LogWriterFilter)current); }));
                }

                Find.WindowStack.Add(new FloatMenu(list));
            }

            listingStandard.GapLine();

            if (ConnectedToProgressRenderer)
                listingStandard.Label("Diary_Connected_To_Progress_Renderer".Translate());
            else
                listingStandard.Label("Diary_Not_Connected_To_Progress_Renderer".Translate());

            listingStandard.End();
        }
    }
}