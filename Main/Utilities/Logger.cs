using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TNHTweaker.Utilities
{
    public static class TNHTweakerLogger
    {
        public static ManualLogSource BepLog;

        public static bool AllowLogging = false;
        public static bool LogCharacter = false;
        public static bool LogLoading = false;
        public static bool LogTNH = false;


        public enum LogType
        {
            General,
            Character,
            Loading,
            TNH
        }

        public static void Init()
        {
            BepLog = BepInEx.Logging.Logger.CreateLogSource("TNHTweaker");
        }

        public static bool ShouldLog(LogType type)
        {
            return 
                AllowLogging &&
                ((type == LogType.General) ||
                (type == LogType.Character && LogCharacter) ||
                (type == LogType.Loading && LogLoading) ||
                (type == LogType.TNH && LogTNH));
        }

        public static void Log(string log, LogType type)
        {
            if (ShouldLog(type))
            {
                BepLog.LogInfo(log);
            }
        }

        public static void Log(Func<string> delayedLog, LogType type)
        {
            if (ShouldLog(type))
            {
                BepLog.LogInfo(delayedLog);
            }
        }

        public static void LogWarning(string log)
        {
            BepLog.LogWarning(log);
        }

        public static void LogError(string log)
        {
            BepLog.LogError(log);
        }

    }
}
