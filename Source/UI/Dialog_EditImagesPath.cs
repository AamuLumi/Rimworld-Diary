using Diary.Core;
using UnityEngine;
using Verse;

namespace Diary.UI;

public class Dialog_EditImagesPath : Window
{
    private string _currentPath;

    public Dialog_EditImagesPath(string currentPath)
    {
        _currentPath = currentPath;
        doCloseButton = false;
        doCloseX = true;
    }

    public override void OnAcceptKeyPressed()
    {
        var service = Current.Game.GetComponent<DiaryService>();

        service.ChangeImagesFolder(_currentPath);

        base.OnAcceptKeyPressed();
    }

    public override void DoWindowContents(Rect inRect)
    {
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);

        listingStandard.Label("Diary_Edit_Screenshot_Folder_Explanation".Translate());

        _currentPath = listingStandard.TextEntry(_currentPath);

        listingStandard.End();

        if (!Widgets.ButtonText(
                new Rect(15f, (float)(inRect.height - 35.0 - 15.0), (float)(inRect.width - 15.0 - 15.0), 35f),
                "OK"))
            return;

        OnAcceptKeyPressed();
    }
}