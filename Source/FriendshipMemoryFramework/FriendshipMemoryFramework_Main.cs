using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HugsLib;
using Verse;

namespace FriendshipMemoryFramework
{
    public class FriendshipMemoryFramework_Main : ModBase
    {
        public static string MODID => "com.vectorial1024.rimworld.fmf";

        /// <summary>
        /// Already includes a space character.
        /// </summary>
        public static string MODPREFIX => "[V1024-FMF] ";

        public override string ModIdentifier => MODID;

        public static void LogError(string message, bool ignoreLogLimit = false)
        {
            Log.Error(MODPREFIX + message, ignoreLogLimit);
        }

        public static void LogWarning(string message, bool ignoreLogLimit = false)
        {
            Log.Warning(MODPREFIX + message, ignoreLogLimit);
        }
    }
}
