using UnityEngine;
using Verse;

namespace DiaryMod
{
    public class Dialog_EditImagesPath : Window
    {
        private string currentPath;

        public Dialog_EditImagesPath(string currentPath)
        {
            this.currentPath = currentPath;
            doCloseButton = false;
            doCloseX = true;
        }

        public override void OnAcceptKeyPressed()
        {
            var service = Current.Game.GetComponent<DiaryService>();

            service.ChangeImagesFolder(currentPath);

            base.OnAcceptKeyPressed();
        }

        public override void DoWindowContents(Rect inRect)
        {
            var listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.Label("Diary_Edit_Screenshot_Folder_Explanation".Translate());

            currentPath = listingStandard.TextEntry(currentPath);

            listingStandard.End();

            if (!Widgets.ButtonText(
                    new Rect(15f, (float)(inRect.height - 35.0 - 15.0), (float)(inRect.width - 15.0 - 15.0), 35f),
                    "OK"))
                return;

            OnAcceptKeyPressed();
        }
    }
}