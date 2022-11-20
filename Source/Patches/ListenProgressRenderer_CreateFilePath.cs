using System.Linq;

using HarmonyLib;
using Verse;

namespace Diary
{
    public static class ListenProgressRenderer_CreateFilePath
    {
        static void Postfix(ref string __result)
        {
            Current.Game.GetComponent<DiaryService>().AddImageNow(__result);
        }
    }
}
