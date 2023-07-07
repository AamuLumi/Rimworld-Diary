using Diary.Core.Events;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Diary
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.TakeDamage))]
    public static class AT_OnPawnHitEnemy
    {
        private static void Postfix(ref Pawn __instance)
        {
            if (__instance.Faction !== ) return;

            var e = new Event_OnPawnDie(pa);
            e.CommitEntry();
        }
    }
}