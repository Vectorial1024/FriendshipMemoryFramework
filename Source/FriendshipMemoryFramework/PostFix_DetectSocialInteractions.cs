using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FriendshipMemoryFramework
{
    [HarmonyPatch(typeof(Pawn_InteractionsTracker))]
    [HarmonyPatch("TryInteractWith", MethodType.Normal)]
    public class PostFix_DetectSocialInteractions
    {
        [HarmonyPostfix]
        public static void TryInteractWith(bool __result, Pawn ___pawn, Pawn recipient, InteractionDef intDef)
        {
            if (__result)
            {
                // interaction success
                Find.World.GetComponent<FriendshipMemoryGlobalTracker>().Notify_SocialInteractionOccured(___pawn, recipient, intDef);
            }
        }
    }
}
