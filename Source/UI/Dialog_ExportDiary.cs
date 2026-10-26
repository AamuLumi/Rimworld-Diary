using UnityEngine;
using Verse;

namespace Diary.UI;

public class Dialog_ExportDiary : Window
{
    private readonly string _message;
    private readonly string _pathCreated;

    public Dialog_ExportDiary(string message)
    {
        doCloseButton = true;
        _message = message;
    }

    public Dialog_ExportDiary(string message, string pathCreated)
    {
        doCloseButton = true;
        _message = message;
        _pathCreated = pathCreated;
    }

    public override void DoWindowContents(Rect inRect)
    {
        var messageToDisplay = _message;

        if (_pathCreated != null)
            messageToDisplay += $"\n\n{"Diary_File_Created_Available_At".Translate()} : \n\n{_pathCreated}";

        var listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);

        listingStandard.Label(messageToDisplay);

        listingStandard.End();
    }
}