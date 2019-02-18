using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueOfMixerClient.Core
{
    class Logger
    {
        public static void Info(string msg)
        {
            Log("[INFO]", msg);
        }


        public static void Error(string msg, Exception e = null)
        {
            Log("[Err]", $"{msg} Exception: {(e == null ? "None" : e.Message)}");
        }

        private static void Log(string type, string msg)
        {
            Console.WriteLine($"{type} {msg}");
        }
    }
}
