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

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref folderPath, "folderPath", Application.dataPath);
            Scribe_Values.Look(ref exportFormat, "exportFormat", ExportFormat.Text);
            Scribe_Values.Look(ref defaultMessage, "defaultMessage", DefaultMessage.Empty);
        }

        public string GetFormatName(ExportFormat format)
        {
            switch (format)
            {
                case ExportFormat.Text: return "Diary_Format_Text".Translate();
                case ExportFormat.RTF: return "Diary_Format_RTF".Translate();
                default: return "";
            }
        }

        public string GetDefaultMessageName(DefaultMessage m)
        {
            switch (m)
            {
                case DefaultMessage.Empty: return "Diary_Empty".Translate();
                case DefaultMessage.NoEntryFound: return "Diary_No_Entry_Found_Message".Translate();
                default: return "";
            }
        }

        public void SetFormat(ExportFormat f)
        {
            exportFormat = f;
        }

        public void SetDefaultMessage(DefaultMessage m)
        {
            defaultMessage = m;
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
            if (listingStandard.ButtonText(GetFormatName(exportFormat)))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (int i in Enum.GetValues(typeof(ExportFormat)))
                {
                    int current = i;
                    list.Add(new FloatMenuOption(GetFormatName((ExportFormat)i), delegate
                    {
                        SetFormat((ExportFormat)current);
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }

            listingStandard.Label("Diary_Default_Entry_Message".Translate());
            if (listingStandard.ButtonText(GetDefaultMessageName(defaultMessage)))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (int i in Enum.GetValues(typeof(DefaultMessage)))
                {
                    int current = i;
                    list.Add(new FloatMenuOption(GetDefaultMessageName((DefaultMessage)i), delegate
                    {
                        SetDefaultMessage((DefaultMessage)current);
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }

            listingStandard.End();
        }
    }
}
