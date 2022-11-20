using System.Linq;

using HarmonyLib;
using Verse;

namespace Diary
{
    [HarmonyPatch(typeof(ProgressRenderer.MapComponent_RenderManager), "CreateFilePath")]
    public static class ListenProgressRenderer_CreateFilePath
    {
        static void Postfix(ref string __result)
        {
            Log.Message("Image created");
            Current.Game.GetComponent<DiaryService>().AddImageNow(__result);
        }
    }
}
