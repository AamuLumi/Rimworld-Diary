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
            var settings = LoadedModManager.GetMod<Diary>().GetSettings<DiarySettings>();
            var currentFilter = settings.LogWriterFilter;

            if (currentFilter != LogWriterFilter.All && currentFilter != LogWriterFilter.Events) return;

            if (settings.ArchivableShouldBeIgnored(archivable)) return;

            var stringToWrite = archivable.ArchivedLabel.StripTags();

            if (settings.AreDescriptionExportedWithEvents)
                stringToWrite += $"\n\n{archivable.ArchivedTooltip.StripTags()}\n";

            Current.Game.GetComponent<DiaryService>().AppendEntryNow(stringToWrite);
        }
    }
}