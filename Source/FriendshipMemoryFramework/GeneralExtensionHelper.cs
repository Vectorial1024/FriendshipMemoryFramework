using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FriendshipMemoryFramework
{
    public static class GeneralExtensionHelper
    {
        /// <summary>
        /// Extension method. Returns true if this pawn can have Thoughts. Is NullReference-safe.
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns>Whether the mood/thoughts handler of this pawn is non-null</returns>
        public static bool IsCapableOfThought(this Pawn pawn)
        {
            return pawn?.needs?.mood?.thoughts != null;
        }
    }
}
