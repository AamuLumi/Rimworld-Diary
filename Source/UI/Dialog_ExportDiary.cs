using UnityEngine;
using Verse;

namespace DiaryMod
{
    public class Dialog_ExportDiary : Window
    {
        private readonly string message;
        private readonly string pathCreated;

        public Dialog_ExportDiary(string message)
        {
            doCloseButton = true;
            this.message = message;
        }

        public Dialog_ExportDiary(string message, string pathCreated)
        {
            doCloseButton = true;
            this.message = message;
            this.pathCreated = pathCreated;
        }

        public override void DoWindowContents(Rect inRect)
        {
            var messageToDisplay = message;

            if (pathCreated != null)
                messageToDisplay += $"\n\n{"Diary_File_Created_Available_At".Translate()} : \n\n{pathCreated}";

            var listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.Label(messageToDisplay);

            listingStandard.End();
        }
    }
}