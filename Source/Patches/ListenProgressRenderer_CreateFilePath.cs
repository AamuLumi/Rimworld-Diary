using Diary.Core;
using Verse;

namespace Diary.Patches;

public static class ListenProgressRenderer_CreateFilePath
{
    private static void Postfix(ref string __result)
    {
        Current.Game.GetComponent<DiaryService>().AddImageNow(__result);
    }
}