using BepInEx.Configuration;
using FistVR;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TNHTweaker.ObjectTemplates;
using TNHTweaker.Utilities;
using UnityEngine;
using BepInEx;

namespace TNHTweaker
{
    public class TNHTweaker : BaseUnityPlugin
    {
        
        public static ConfigEntry<bool> UnlimitedTokens;

        private void Awake()
        {
            TNHTweakerLogger.Init();
            TNHTweakerLogger.Log("Hello World (from TNH Tweaker)", TNHTweakerLogger.LogType.General);

            LoadConfigFile();

            Harmony.CreateAndPatchAll(typeof(TNHTweaker));
        }

        private void LoadConfigFile()
        {
            TNHTweakerLogger.Log("TNHTweaker -- Getting config file", TNHTweakerLogger.LogType.File);

            UnlimitedTokens = Config.Bind(
                "Debug",
                "EnableUnlimitedTokens",
                false,
                "If true, you will spawn with 999999 tokens for any character in TNH (useful for testing loot pools)"
                );

        }

    }
}
