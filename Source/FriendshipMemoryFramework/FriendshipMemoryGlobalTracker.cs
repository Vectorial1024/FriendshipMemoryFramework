using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FriendshipMemoryFramework
{
    public class FriendshipMemoryGlobalTracker : WorldComponent
    {
        List<FriendshipMemoryTracker> allMemoryTrackers;

        public static readonly int TickInterval = 50;

        /// <summary>
        /// A shorthand to find the MemoryTracker for the currently-loaded world.
        /// </summary>
        public static FriendshipMemoryGlobalTracker LoadedInstance => Find.World.GetComponent<FriendshipMemoryGlobalTracker>();

        public FriendshipMemoryGlobalTracker(World world) : base(world)
        {
            allMemoryTrackers = new List<FriendshipMemoryTracker>();
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref allMemoryTrackers, "allMemoryTrackers", LookMode.Deep);
            base.ExposeData();
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
        }

        public override void WorldComponentTick()
        {
            // each pawn will tick their tracker in their patched tickraw() method
            base.WorldComponentTick();
        }

        public FriendshipMemoryTracker GetFriendshipMemoryTrackerForSubject(Pawn subject, bool ensureExists = false)
        {
            if (subject == null)
            {
                return null;
            }
            // will give null if not found
            FriendshipMemoryTracker queriedTracker = allMemoryTrackers.Where((queryFMT) => queryFMT.Subject == subject).FirstOrDefault();
            if (ensureExists && queriedTracker == null)
            {
                queriedTracker = new FriendshipMemoryTracker(subject);
                allMemoryTrackers.Add(queriedTracker);
            }
            return queriedTracker;
        }

        public void Notify_PhysicalSightOccured(Pawn observer, Pawn observed)
        {
            //FriendshipMemoryFramework_Main.LogError(observer.Name?.ToString() + " observed " + observed.Name?.ToString());
            GetFriendshipMemoryTrackerForSubject(observer, true).Notify_SubjectPhysicallySeenAnotherPawn(observed);
        }

        public void Notify_PhysicalSightOfCorpseOccured(Pawn observer, Pawn deadPawn)
        {
            //FriendshipMemoryFramework_Main.LogError(observer.Name?.ToString() + " observed corpse of " + deadPawn.Name?.ToString());
            GetFriendshipMemoryTrackerForSubject(observer, true).Notify_SubjectPhysicallySeenCorpse(deadPawn);
        }

        public void Notify_SocialInteractionOccured(Pawn initiator, Pawn recipient, InteractionDef intDef)
        {
            //FriendshipMemoryFramework_Main.LogError(initiator.Name?.ToString() + " interacted with " + recipient.Name?.ToString() + " using " + intDef.ToString());
            GetFriendshipMemoryTrackerForSubject(initiator, true).Notify_SubjectSociallyInteractedWithAnotherPawn(recipient, intDef);
        }

        public void Notify_PawnDied(Pawn deadPawn)
        {
            foreach (FriendshipMemoryTracker tracker in allMemoryTrackers)
            {
                tracker.GetFriendshipMemoryForOther(deadPawn)?.Notify_PhysicallySeenCorpse();
            }
        }
    }
}
