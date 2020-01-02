//#define DEBUG_ON : You Can Enable Debug Mode with complier Setting.
// In Unity, You simply can enable Debug from playerSetting -> Scripting Define Symbol

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

namespace ProvisGames.Core.AssetDependency.Utility
{
    static class Debugger
    {
        [Conditional("DEBUG_ON")]
        public static void Log(string message)
        {
            Debug.Log(message);
        }
        [Conditional("DEBUG_ON")]
        public static void Log(object message)
        {
            Debug.Log(message);
        }

        [Conditional("DEBUG_ON")]
        public static void LogWarning(string message)
        {
            Debug.LogWarning(message);
        }
        [Conditional("DEBUG_ON")]
        public static void LogWarning(object message)
        {
            Debug.LogWarning(message);
        }

        [Conditional("DEBUG_ON")]
        public static void LogError(string message)
        {
            Debug.LogError(message);
        }
        [Conditional("DEBUG_ON")]
        public static void LogError(object message)
        {
            Debug.LogError(message);
        }
    }
}