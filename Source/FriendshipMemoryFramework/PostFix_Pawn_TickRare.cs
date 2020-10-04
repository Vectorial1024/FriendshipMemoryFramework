using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FriendshipMemoryFramework
{
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("TickRare", MethodType.Normal)]
    public class PostFix_Pawn_TickRare
    {
		static readonly int TickIntervalVision = 4;

        [HarmonyPostfix]
        public static void TickRare(Pawn __instance)
        {
			if (!__instance.Suspended && Find.TickManager.TicksGame % TickIntervalVision == 0)
            {
				CheckVision(__instance);
			}
		}

		public static void CheckVision(Pawn pawn)
        {
			// Basically a copy of the function in PawnObserver
			if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight) || pawn.needs.mood == null)
			{
				return;
			}
			// caravans have some different management
			Caravan caravan = pawn.GetCaravan();
			if (caravan != null)
            {
				// inside caravan
				// see other
				foreach (Pawn potentialOther in caravan.PawnsListForReading)
                {
					Find.World.GetComponent<FriendshipMemoryGlobalTracker>().Notify_PhysicalSightOccured(pawn, potentialOther);
				}
				// see dead
				foreach (Thing thing in CaravanInventoryUtility.AllInventoryItems(caravan))
                {
					Corpse potentialCorpse = thing as Corpse;
					if (potentialCorpse != null)
                    {
						Pawn deadPawn = potentialCorpse.InnerPawn;
						Find.World.GetComponent<FriendshipMemoryGlobalTracker>().Notify_PhysicalSightOfCorpseOccured(pawn, deadPawn);
					}
                }
				return;
            }
			Map map = pawn.Map;
			if (map == null)
			{
				return;
			}
			MapPawns pawns = pawn.Map.mapPawns;
			IntVec3 selfPosition = pawn.Position;
			// seen alive
			foreach (Pawn potentialOther in pawns.AllPawnsSpawned)
			{
				if (GenSight.LineOfSight(selfPosition, potentialOther.Position, map, skipFirstCell: true))
				{
					// can see!
					float distanceSq = selfPosition.DistanceToSquared(potentialOther.Position);
					if (distanceSq > 25)
					{
						// distance radius > 5, excluded
						continue;
					}
					Find.World.GetComponent<FriendshipMemoryGlobalTracker>().Notify_PhysicalSightOccured(pawn, potentialOther);
				}
			}
			// seen corpse
			for (int i = 0; (float)i < 100f; i++)
			{
				IntVec3 intVec = pawn.Position + GenRadial.RadialPattern[i];
				if (!intVec.InBounds(map) || !GenSight.LineOfSight(intVec, pawn.Position, map, skipFirstCell: true))
				{
					continue;
				}
				List<Thing> thingList = intVec.GetThingList(map);
				for (int j = 0; j < thingList.Count; j++)
				{
					Corpse potentialCorpse = thingList[j] as Corpse;
					if (potentialCorpse != null)
					{
						Pawn deadPawn = potentialCorpse.InnerPawn;
						Find.World.GetComponent<FriendshipMemoryGlobalTracker>().Notify_PhysicalSightOfCorpseOccured(pawn, deadPawn);
					}
				}
			}
			// tick the stuff
			Find.World.GetComponent<FriendshipMemoryGlobalTracker>().GetFriendshipMemoryTrackerForSubject(pawn)?.Notify_TickOnce(TickIntervalVision);
		}
    }
}
