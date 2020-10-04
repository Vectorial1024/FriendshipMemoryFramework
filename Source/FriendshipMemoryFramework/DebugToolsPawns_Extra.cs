using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FriendshipMemoryFramework
{
    public class DebugToolsPawns_Extra
    {
        [DebugAction(DebugActionCategories.Pawns, null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void OutputFriendshipMemoryToLog(Pawn p)
        {
            Find.World.GetComponent<FriendshipMemoryGlobalTracker>().GetFriendshipMemoryTrackerForSubject(p, true).OutputFriendshipMemoryToLog();
        }

    }
}
