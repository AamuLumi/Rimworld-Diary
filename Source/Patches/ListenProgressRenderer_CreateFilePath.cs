using Verse;

namespace DiaryMod
{
    public static class ListenProgressRenderer_CreateFilePath
    {
        private static void Postfix(ref string __result)
        {
            Current.Game.GetComponent<DiaryService>().AddImageNow(__result);
        }
    }
}