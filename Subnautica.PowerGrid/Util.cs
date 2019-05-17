using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;

namespace Subnautica.PowerGrid
{
    internal static class Util
    {

        public static void Log(string message)
        {
#if DEBUG
            FileLog.Log(message);
#endif
        }

        public static void Log(string message, params object[] args)
        {
#if DEBUG
            FileLog.Log(string.Format(message, args));
#endif
        }

        public static void LogError(string message, Exception ex)
        {
#if DEBUG
            FileLog.Log(string.Format("{0}\n{1}\n{2}", message, ex.Message, ex.StackTrace));
#endif
        }
    }
}
