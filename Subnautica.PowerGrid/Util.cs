using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;

namespace Subnautica.PowerGrid
{
    internal static class Util
    {
        public static bool TRACE = true;

        public static void Log(string message)
        {
            if (TRACE) FileLog.Log(message);
        }

        public static void Log(string message, params object[] args)
        {
            if (TRACE) FileLog.Log(string.Format(message, args));
        }

        public static void LogError(string message, Exception ex)
        {
            if (TRACE) FileLog.Log(string.Format("{0}\n{1}\n{2}", message, ex.Message, ex.StackTrace));
        }
    }
}
