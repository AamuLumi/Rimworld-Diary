using System.Linq;
using HarmonyLib;
using Verse;

namespace Diary
{
    [HarmonyPatch(typeof(PlayLog), nameof(PlayLog.Add))]
    public static class ListenPlayLog_Add
    {
        private static void Prefix(LogEntry entry)
        {
            var currentFilter = LoadedModManager.GetMod<Diary>().GetSettings<DiarySettings>().LogWriterFilter;

            if (currentFilter != LogWriterFilter.All && currentFilter != LogWriterFilter.Chats) return;

            var stringToWrite = entry.ToGameStringFromPOV(entry.GetConcerns().First()).StripTags();

            Current.Game.GetComponent<DiaryService>().AppendEntryNow(stringToWrite);
        }
    }
}