using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;

namespace FriendshipMemoryFramework
{
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("Kill", MethodType.Normal)]
    public class PostFix_Pawn_Kill
    {
        [HarmonyPostfix]
        public static void Event_PawnDied(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit)
        {
            HandleOawnDied(__instance, dinfo, exactCulprit);
        }

        public static void HandleOawnDied(Pawn deadPawn, DamageInfo? dinfo, Hediff exactCulprit)
        {
            // override this from desync if needed to block news etc.
            // need to do with ?. so in edge cases where the world is not ready, we do not die.
            Find.World.GetComponent<FriendshipMemoryGlobalTracker>()?.Notify_PawnDied(deadPawn);
        }
    }
}
