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
using Verse.Noise;

namespace Diary
{
    public class Dialog_ExportDiary : Window
    {
        private string message;
        private string pathCreated;

        public Dialog_ExportDiary(string message)
        {
            this.doCloseButton = true;
            this.message = message;
        }

        public Dialog_ExportDiary(string message, string pathCreated)
        {
            this.doCloseButton = true;
            this.message = message;
            this.pathCreated = pathCreated;
        }

        public override void DoWindowContents(Rect inRect)
        {
            string messageToDisplay = message;

            if (pathCreated != null)
            {
                messageToDisplay += $"\n\n{"Diary_File_Created_Available_At".Translate()} : \n\n{pathCreated}";
            }

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.Label(messageToDisplay);

            listingStandard.End();
        }
    }
}
