using System.Linq;

using RimWorld;
using HarmonyLib;
using Verse;

namespace Diary
{
    [HarmonyPatch(typeof(Archive), nameof(Archive.Add))]
    public static class ListenArchive_Add
    {
        static void Prefix(IArchivable archivable)
        {
            LogWriterFilter currentFilter = LoadedModManager.GetMod<Diary>().GetSettings<DiarySettings>().LogWriterFilter;

            if (currentFilter != LogWriterFilter.All && currentFilter != LogWriterFilter.Events)
            {
                return;
            }

            string stringToWrite = ColoredText.StripTags(archivable.ArchivedLabel);

            Current.Game.GetComponent<DiaryService>().AppendEntryNow(stringToWrite);
        }
    }
}
