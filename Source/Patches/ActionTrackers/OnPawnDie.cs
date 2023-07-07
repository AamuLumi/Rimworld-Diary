using Diary.Core.Events;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Diary
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.SetDead))]
    public static class AT_PawnDie
    {
        private static void Postfix(ref Pawn_HealthTracker __instance)
        {
            var currentInstance = __instance;
            var pa = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists.Find(
                p => p.health == currentInstance
            );

            if (pa == null) return;

            var e = new Event_OnPawnDie(pa);
            e.CommitEntry();
        }
    }
}