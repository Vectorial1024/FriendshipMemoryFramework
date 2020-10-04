using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace FriendshipMemoryFramework
{
    public class FriendshipMemory : IExposable
    {
        Pawn subject;
        Pawn other;
        int approxTicksSinceLastSeen;
        int approxTicksSinceLastInteracted;
        int approxTicksSinceLastStateUpdate;
        InteractionDef lastKnownInteraction;
        float lastKnownOpinion;
        PawnLifeState lastKnownPawnState;

        public static readonly int TicksPerDay = 60000;

        public Pawn Subject
        {
            get
            {
                return subject;
            }
            internal set
            {
                subject = value;
            }
        }

        public Pawn Other 
        {
            get
            {
                return other;
            }
            internal set
            {
                other = value;
            }
        }

        /// <summary>
        /// How many ticks ago (approx) did the subject pawn last seen the other pawn.
        /// <para/>
        /// Seeing the corpse also counts as "last seen". Caravan members are always seeing each other during travels.
        /// </summary>
        public int ApproxTicksSinceLastSeen => approxTicksSinceLastSeen;

        /// <summary>
        /// How many ticks ago (approx) did the subject pawn last socially-interacted with the other pawn.
        /// <para/>
        /// Will not be updated in caravans due to game limitations. Only counts in-map social interaction, but then, if interaction occured, usually both pawns are close to each other.
        /// </summary>
        public int ApproxTicksSinceLastInteracted => approxTicksSinceLastInteracted;

        /// <summary>
        /// How many ticks ago (approx) was the LastKnownLifeState updated for this memory.
        /// </summary>
        public int ApproxTicksSinceLastStateUpdate => approxTicksSinceLastStateUpdate;

        /// <summary>
        /// Whether, from the programming point of view, it is no longer possible to determine who the other pawn is.
        /// </summary>
        public bool MemoryIsArchivable => other == null || other.Dead;

        /// <summary>
        /// The last known state of the other pawn (UNKNOWN, ALIVE, or DEAD) that the subject pawn knows. Use with ApproxTicksSinceLastStateUpdate to check rough history.
        /// <para/>
        /// Will enter UNKNOWN state from ALIVE state if haven't seen/interacted woth other pawn for 3 days (default, may be overriden by other mods).
        /// </summary>
        public PawnLifeState LastKnownLifeState
        {
            get
            {
                return lastKnownPawnState;
            }
            internal set
            {
                lastKnownPawnState = value;
                approxTicksSinceLastStateUpdate = 0;
            }
        }

        public FriendshipMemory(Pawn subject)
        {
            this.subject = subject;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref other, "other");
            Scribe_Values.Look(ref approxTicksSinceLastSeen, "approxTicksSinceLastSeen");
            Scribe_Values.Look(ref approxTicksSinceLastInteracted, "approxTicksSinceLastInteracted");
            Scribe_Values.Look(ref approxTicksSinceLastStateUpdate, "approxTicksSinceLastStateUpdate");
            Scribe_Defs.Look(ref lastKnownInteraction, "lastKnownInteraction");
            Scribe_Values.Look(ref lastKnownOpinion, "lastKnownOpinion");
            Scribe_Values.Look(ref lastKnownPawnState, "lastKnownPawnState");
        }

        public string ReadableDetails
        {
            get
            {
                return "Memory of " + Subject?.Name?.ToString() + " -> " + Other?.Name?.ToString() + " " +
                    "last seen: " + approxTicksSinceLastSeen + ", last interacted: " + approxTicksSinceLastInteracted + ", " +
                    "last state update: " + approxTicksSinceLastStateUpdate + ", " +
                    "last opinion: " + lastKnownOpinion + ", last state: " + lastKnownPawnState;
            }
        }

        /// <summary>
        /// Notify that the subject pawn has just seen the living body of the other pawn.
        /// <para/>
        /// Not to confuse with Notify_PhysicallySeenCorpse().
        /// </summary>
        public void Notify_PhysicallySeenRecently()
        {
            approxTicksSinceLastSeen = 0;
            UpdateLastKnownOpinion();
            LastKnownLifeState = PawnLifeState.ALIVE;
        }

        /// <summary>
        /// Notify that the subject pawn has just seen the corpse of the other pawn.
        /// <para/>
        /// Not to confuse with Notify_PhysicallySeenRecently().
        /// </summary>
        public void Notify_PhysicallySeenCorpse()
        {
            approxTicksSinceLastSeen = 0;
            UpdateLastKnownOpinion();
            LastKnownLifeState = PawnLifeState.DEAD;
        }

        /// <summary>
        /// Notify that the subject pawn has just socially interacted with the other pawn.
        /// <para/>
        /// Not to confuse with Notify...
        /// </summary>
        /// <param name="interactionDef"></param>
        public void Notify_SociallyInteractedRecently(InteractionDef interactionDef)
        {
            approxTicksSinceLastInteracted = 0;
            lastKnownInteraction = interactionDef;
            UpdateLastKnownOpinion();
            LastKnownLifeState = PawnLifeState.ALIVE;
        }

        /// <summary>
        /// Notify that the other pawn entered the UNKNOWN state.
        /// <para/>
        /// You should be sure of what you want to do before you set it to UNKNOWN.
        /// </summary>
        public void Notify_UnknownState()
        {
            // dont use this unless you want to do it!
            UpdateLastKnownLifeState(PawnLifeState.UNKNOWN);
        }

        /// <summary>
        /// Notify that the other pawn died.
        /// </summary>
        public void Notify_Died()
        {
            UpdateLastKnownLifeState(PawnLifeState.DEAD);
        }

        public void Tick(int currentInterval)
        {
            approxTicksSinceLastInteracted += currentInterval;
            approxTicksSinceLastSeen += currentInterval;
            // determine if is too long no see
            if (lastKnownPawnState == PawnLifeState.ALIVE && approxTicksSinceLastSeen >= TicksPerDay * 3 && approxTicksSinceLastInteracted >= TicksPerDay * 3)
            {
                // too long.
                Notify_UnknownState();
            }
        }

        public void UpdateLastKnownLifeState(PawnLifeState newState)
        {
            lastKnownPawnState = newState;
            approxTicksSinceLastStateUpdate = 0;
        }

        public void UpdateLastKnownOpinion()
        {
            if (MemoryIsArchivable)
            {
                // undefined; we should not taint something when the other pawn is dead
                return;
            }
            lastKnownOpinion = Subject.relations.OpinionOf(Other);
        }
    }
}
