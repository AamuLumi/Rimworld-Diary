using System.Linq;

using HarmonyLib;
using Verse;

namespace Diary
{
    [HarmonyPatch(typeof(PlayLog), nameof(PlayLog.Add))]
    public static class ListenPlayLog_Add
    {
        static void Prefix(LogEntry entry)
        {
            LogWriterFilter currentFilter = LoadedModManager.GetMod<Diary>().GetSettings<DiarySettings>().LogWriterFilter;

            if (currentFilter != LogWriterFilter.All && currentFilter != LogWriterFilter.Chats)
            {
                return;
            }

            string stringToWrite = ColoredText.StripTags(entry.ToGameStringFromPOV(entry.GetConcerns().First()));

            Current.Game.GetComponent<DiaryService>().AppendEntryNow(stringToWrite);
        }
    }
}
