using Diary.Core.Events;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Diary
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.CheckForStateChange))]
    public static class AT_PawnDowned
    {
        private static void Prefix(out PawnStatus __state, ref Pawn_HealthTracker __instance)
        {
            __state = new PawnStatus(__instance.Downed, __instance.Dead);
        }

        private static void Postfix(PawnStatus __state, ref Pawn_HealthTracker __instance)
        {
            var currentInstance = __instance;
            var pa = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists.Find(
                p => p.health == currentInstance
            );

            if (pa == null) return;

            if (__state.Downed != __instance.Downed)
            {
                if (__instance.Downed)
                    new Event_OnPawnDowned(pa).CommitEntry();
                else
                    new Event_OnPawnDie(pa).CommitEntry();
            }
        }
    }
}