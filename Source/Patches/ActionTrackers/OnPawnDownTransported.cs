using System.Reflection;
using Diary.Core.Events;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Diary
{
    [HarmonyPatch]
    public static class AT_PawnDowned
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.FirstMethod(
                typeof(Pawn_HealthTracker),
                method => method.Name.Contains("MakeDowned")
            );
        }

        private static void Prefix(ref Pawn_HealthTracker __instance)
        {
            var currentInstance = __instance;
            var pa = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists.Find(
                (p) => p.health == currentInstance
            );

            if (pa == null)
            {
                return;
            }

            var e = new Event_OnPawnDowned(pa);
            e.CommitEntry();
        }
    }
}
