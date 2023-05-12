using HarmonyLib;
using RimWorld;
using Verse;

namespace Diary
{
    [HarmonyPatch(typeof(Archive), nameof(Archive.Add))]
    public static class ListenArchive_Add
    {
        private static void Prefix(IArchivable archivable)
        {
            var currentFilter = LoadedModManager.GetMod<Diary>().GetSettings<DiarySettings>().LogWriterFilter;

            if (currentFilter != LogWriterFilter.All && currentFilter != LogWriterFilter.Events) return;

            var stringToWrite = archivable.ArchivedLabel.StripTags();

            if (LoadedModManager.GetMod<Diary>().GetSettings<DiarySettings>().AreDescriptionExportedWithEvents)
                stringToWrite += $"\n\n{archivable.ArchivedTooltip.StripTags()}\n";

            Current.Game.GetComponent<DiaryService>().AppendEntryNow(stringToWrite);
        }
    }
}