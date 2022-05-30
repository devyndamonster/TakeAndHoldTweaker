﻿using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LegacyCharacterLoader.Utilities
{
    public static class LegacyLogger
    {
        public static ManualLogSource BepLog;

        public static bool AllowLogging = true;
        public static bool LogCharacter = true;
        public static bool LogLoading = true;
        public static bool LogTNH = true;


        public enum LogType
        {
            General,
            Character,
            Loading,
            TNH
        }

        public static void Init()
        {
            BepLog = BepInEx.Logging.Logger.CreateLogSource("TNHTweakerLegacy");
        }

        public static void Log(string log, LogType type)
        {
            if (AllowLogging)
            {
                if(type == LogType.General)
                {
                    BepLog.LogInfo(log);
                }
                else if(type == LogType.Character && LogCharacter)
                {
                    BepLog.LogInfo(log);
                }
                else if (type == LogType.Loading && LogLoading)
                {
                    BepLog.LogInfo(log);
                }
                else if (type == LogType.TNH && LogTNH)
                {
                    BepLog.LogInfo(log);
                }
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