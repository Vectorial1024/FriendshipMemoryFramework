using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace FriendshipMemoryFramework
{
    public class FriendshipMemoryTracker : IExposable
    {
        Pawn subject;
        List<FriendshipMemory> friendshipMemories;

        public Pawn Subject => subject;

        public FriendshipMemoryTracker()
        {
            // load from savefile
            friendshipMemories = new List<FriendshipMemory>();
        }

        internal FriendshipMemoryTracker(Pawn subject): this()
        {
            // instantiate for new pawn
            this.subject = subject;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref subject, "subject");
            Scribe_Collections.Look(ref friendshipMemories, "friendshipMemories", LookMode.Deep, subject);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                // set the cached-field of subject
                foreach (FriendshipMemory memory in friendshipMemories)
                {
                    memory.Subject = Subject;
                }
            }
        }

        public FriendshipMemory GetFriendshipMemoryForOther(Pawn other, bool ensureExists = false)
        {
            if (other == null)
            {
                return null;
            }
            // will give null if not found
            FriendshipMemory queriedMemory = friendshipMemories.Where((queryFM) => queryFM.Other == other).FirstOrDefault();
            if (ensureExists && queriedMemory == null)
            {
                // FriendshipMemoryFramework_Main.LogError("Creating new FriendshipMemory for " + Subject.Name?.ToString() + " -> " + other.Name?.ToString());
                queriedMemory = new FriendshipMemory(Subject);
                queriedMemory.Other = other;
                friendshipMemories.Add(queriedMemory);
            }
            return queriedMemory;
        }

        public void Notify_TickOnce(int tickInterval)
        {
            foreach (FriendshipMemory memory in friendshipMemories)
            {
                memory.Tick(tickInterval);
            }
        }

        public void Notify_SubjectPhysicallySeenAnotherPawn(Pawn other)
        {
            if (CanAcceptOtherPawn(other))
            {
                GetFriendshipMemoryForOther(other, true).Notify_PhysicallySeenRecently();
            }
        }

        public void Notify_SubjectPhysicallySeenCorpse(Pawn other)
        {
            if (CanAcceptOtherPawn(other))
            {
                GetFriendshipMemoryForOther(other, true).Notify_PhysicallySeenCorpse();
            }
        }

        public void Notify_SubjectSociallyInteractedWithAnotherPawn(Pawn other, InteractionDef intDef)
        {
            if (CanAcceptOtherPawn(other))
            {
                GetFriendshipMemoryForOther(other, true).Notify_SociallyInteractedRecently(intDef);
            }
        }

        public void OutputFriendshipMemoryToLog()
        {
            FriendshipMemoryFramework_Main.LogError("Outputting friendships of " + Subject?.Name?.ToString());
            foreach (FriendshipMemory memory in friendshipMemories)
            {
                FriendshipMemoryFramework_Main.LogError(memory.ReadableDetails);
            }
        }

        public bool CanAcceptOtherPawn(Pawn other)
        {
            if (Subject == other)
            {
                return false;
            }
            if (Subject.relations.RelatedPawns.Contains(other))
            {
                // bonded, father, etc.
                return true;
            }
            Faction selfFaction = Subject.Faction;
            if (other.IsCapableOfThought())
            {
                return true;
            }
            return false;
        }
    }
}
