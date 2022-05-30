using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.Utilities;

namespace TNHTweaker
{
    public static class TNHTweakerConfig
    {
        public static ConfigEntry<bool> UnlimitedTokens;
        public static ConfigEntry<bool> AllowLogging;
        public static ConfigEntry<bool> LogLoading;
        public static ConfigEntry<bool> LogTNH;
        public static ConfigEntry<bool> EnableDebugTools;

        public static void LoadConfigFile(BaseUnityPlugin plugin)
        {
            UnlimitedTokens = plugin.Config.Bind(
                "Debug",
                "EnableUnlimitedTokens",
                false,
                "If true, you will spawn with 999999 tokens for any character in TNH (useful for testing loot pools)"
                );

            AllowLogging = plugin.Config.Bind(
                "Debug",
                "AllowLogging",
                true,
                "When false, nothing will be logged by TNH Tweaker"
                );

            LogLoading = plugin.Config.Bind(
                "Debug",
                "LogLoading",
                false,
                "When true, logs the process of loading characters and other files"
                );

            LogTNH = plugin.Config.Bind(
                "Debug",
                "LogTNH",
                false,
                "When true, logs events that happen during TNH gameplay"
                );

            EnableDebugTools = plugin.Config.Bind(
                "Debug",
                "EnableDebugTools",
                false,
                "When true, tools for debugging characters will be enabled"
                );

            TNHTweakerLogger.AllowLogging = AllowLogging.Value;
            TNHTweakerLogger.LogLoading = LogLoading.Value;
            TNHTweakerLogger.LogTNH = LogTNH.Value;

            if (EnableDebugTools.Value)
            {
                DebugPanel.AddWristMenuButton();
            }
        }

    }
}
