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
            {
                if (archivable is ChoiceLetter)
                {
                    var letter = (ChoiceLetter)archivable;

                    if (letter.quest != null) stringToWrite += $"\n{letter.quest.description.ToString().StripTags()}\n";
                    else
                        stringToWrite += $"\n{archivable.ArchivedTooltip.StripTags()}\n";
                }
                else
                {
                    stringToWrite += $"\n{archivable.ArchivedTooltip.StripTags()}\n";
                }
            }

            Current.Game.GetComponent<DiaryService>().AppendEntryNow(stringToWrite);
        }
    }
}