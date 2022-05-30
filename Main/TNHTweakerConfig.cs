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

        public static void LoadConfigFile(BaseUnityPlugin plugin)
        {
            UnlimitedTokens = plugin.Config.Bind(
                "Debug",
                "EnableUnlimitedTokens",
                false,
                "If true, you will spawn with 999999 tokens for any character in TNH (useful for testing loot pools)"
                );


            TNHTweakerLogger.AllowLogging = true;
            TNHTweakerLogger.LogLoading = true;
            TNHTweakerLogger.LogTNH = true;

        }

    }
}
