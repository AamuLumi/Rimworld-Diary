using HarmonyLib;
using Verse;

namespace Diary.Patches;

public static class ListenProgressRenderer_CreateFilePath
{
    public static void Postfix(ref string __result)
    {
        Current.Game.GetComponent<DiaryService>().AddImageNow(__result);
    }

    public static void OnCaptureSaved<T>(T info)
    {
        var path = Traverse.Create(info).Property("FilePath").GetValue<string>();
        Current.Game?.GetComponent<DiaryService>()?.AddImageNow(path);
    }
}