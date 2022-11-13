using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Diary
{
    public class DiarySettings : ModSettings
    {
        private string folderPath;

        public string FolderPath
        {
            get { return folderPath; }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref folderPath, "folderPath", Application.dataPath);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.Label("Diary_Export_Folder_Path".Translate());
            folderPath = listingStandard.TextEntry(folderPath);

            listingStandard.End();
        }
    }
}
