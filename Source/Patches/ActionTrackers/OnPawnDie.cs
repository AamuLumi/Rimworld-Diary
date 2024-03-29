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
            Log.Message("Dead detected");
            var currentInstance = __instance;
            var pa = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists.Find(
                p => p.health == currentInstance
            );

            Log.Message($"{pa} - ${currentInstance}");

            if (pa == null) return;

            var e = new Event_OnPawnDie(pa);

            Log.Message($"Commit event = ${e}");
            e.CommitEntry();
        }
    }
}