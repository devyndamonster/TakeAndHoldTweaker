﻿using ADepIn;
using BepInEx.Configuration;
using Deli;
using Deli.Immediate;
using Deli.Setup;
using Deli.VFS;
using Deli.Runtime;
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
using UnityEngine.UI;
using Deli.Runtime.Yielding;
using Anvil;
using TNHTweaker.Patches;

namespace TNHTweaker
{
    public class TNHTweaker : DeliBehaviour
    {
        private static ConfigEntry<bool> printCharacters;
        private static ConfigEntry<bool> logTNH;
        private static ConfigEntry<bool> logFileReads;
        private static ConfigEntry<bool> allowLog;
        public static ConfigEntry<bool> BuildCharacterFiles;
        public static ConfigEntry<bool> UnlimitedTokens;
        public static ConfigEntry<bool> EnableDebugText;
        public static ConfigEntry<bool> EnableScoring;

        public static string OutputFilePath;

        //Variables used by various patches
        public static bool PreventOutfitFunctionality = false;
        public static List<int> SpawnedBossIndexes = new List<int>();
        public static List<int> SupplyPointIFFList = new List<int>();

        public static List<GameObject> SpawnedConstructors = new List<GameObject>();
        public static List<GameObject> SpawnedPanels = new List<GameObject>();
        public static List<EquipmentPoolDef.PoolEntry> SpawnedPools = new List<EquipmentPoolDef.PoolEntry>();

        public static List<List<string>> HoldActions = new List<List<string>>();
        public static List<HoldStats> HoldStats = new List<HoldStats>();

        public static int GunsRecycled;
        public static int ShotsFired;

        /// <summary>
        /// First method that gets called
        /// </summary>
        private void Awake()
        {
            TNHTweakerLogger.Init();
            TNHTweakerLogger.Log("Hello World (from TNH Tweaker)", TNHTweakerLogger.LogType.General);

            LoadConfigFile();

            Harmony.CreateAndPatchAll(typeof(TNHTweaker));
            Harmony.CreateAndPatchAll(typeof(TNHPatches));
            Harmony.CreateAndPatchAll(typeof(PatrolPatches));

            if (EnableScoring.Value) Harmony.CreateAndPatchAll(typeof(HighScorePatches));

            if (EnableDebugText.Value) Harmony.CreateAndPatchAll(typeof(DebugPatches));

            Stages.Setup += OnSetup;
        }


        /// <summary>
        /// Performs initial setup for TNH Tweaker
        /// </summary>
        /// <param name="stage"></param>
        private void OnSetup(SetupStage stage)
        {
            SetupOutputDirectory();
            LoadPanelSprites(stage);

            stage.SetupAssetLoaders[Source, "sosig"] = new SosigLoader().LoadAsset;
            stage.SetupAssetLoaders[Source, "vault_file"] = new VaultFileLoader().LoadAsset;
            stage.SetupAssetLoaders[Source, "character"] = new CharacterLoader().LoadAsset;
        }



        /// <summary>
        /// Loads the sprites used in secondary panels in TNH
        /// </summary>
        private void LoadPanelSprites(SetupStage stage)
        {
            IFileHandle file = Source.Resources.GetFile("mag_dupe_background.png");
            Sprite result = TNHTweakerUtils.LoadSprite(file);
            MagazinePanel.background = result;

            file = Source.Resources.GetFile("ammo_purchase_background.png");
            result = TNHTweakerUtils.LoadSprite(file);
            AmmoPurchasePanel.background = result;

            file = Source.Resources.GetFile("full_auto_background.png");
            result = TNHTweakerUtils.LoadSprite(file);
            FullAutoPanel.background = result;

            file = Source.Resources.GetFile("fire_rate_background.png");
            result = TNHTweakerUtils.LoadSprite(file);
            FireRatePanel.background = result;

            file = Source.Resources.GetFile("minus_icon.png");
            result = TNHTweakerUtils.LoadSprite(file);
            FireRatePanel.minusSprite = result;

            file = Source.Resources.GetFile("plus_icon.png");
            result = TNHTweakerUtils.LoadSprite(file);
            FireRatePanel.plusSprite = result;
        }




        /// <summary>
        /// Loads the bepinex config file, and applys those settings
        /// </summary>
        private void LoadConfigFile()
        {
            TNHTweakerLogger.Log("TNHTweaker -- Getting config file", TNHTweakerLogger.LogType.File);

            BuildCharacterFiles = Source.Config.Bind("General",
                                    "BuildCharacterFiles",
                                    false,
                                    "If true, files useful for character creation will be generated in TNHTweaker folder");

            EnableScoring = Source.Config.Bind("General",
                                    "EnableScoring",
                                    true,
                                    "If true, TNH scores will be uploaded to the TNH Dashboard (https://devyndamonster.github.io/TNHDashboard/index.html)");

            allowLog = Source.Config.Bind("Debug",
                                    "EnableLogging",
                                    true,
                                    "Set to true to enable logging");

            printCharacters = Source.Config.Bind("Debug",
                                         "LogCharacterInfo",
                                         false,
                                         "Decide if should print all character info");

            logTNH = Source.Config.Bind("Debug",
                                    "LogTNH",
                                    false,
                                    "If true, general TNH information will be logged");

            logFileReads = Source.Config.Bind("Debug",
                                    "LogFileReads",
                                    false,
                                    "If true, reading from a file will log the reading process");

            UnlimitedTokens = Source.Config.Bind("Debug",
                                    "EnableUnlimitedTokens",
                                    false,
                                    "If true, you will spawn with 999999 tokens for any character in TNH (useful for testing loot pools)");

            EnableDebugText = Source.Config.Bind("Debug",
                                    "EnableDebugText",
                                    false,
                                    "If true, some text will appear in TNH maps showing additional info");

            

            TNHTweakerLogger.AllowLogging = allowLog.Value;
            TNHTweakerLogger.LogCharacter = printCharacters.Value;
            TNHTweakerLogger.LogTNH = logTNH.Value;
            TNHTweakerLogger.LogFile = logFileReads.Value;
        }


        /// <summary>
        /// Creates the main TNH Tweaker file folder
        /// </summary>
        private void SetupOutputDirectory()
        {
            OutputFilePath = Application.dataPath.Replace("/h3vr_Data", "/TNH_Tweaker");

            if (!Directory.Exists(OutputFilePath))
            {
                Directory.CreateDirectory(OutputFilePath);
            }
        }



        [HarmonyPatch(typeof(TNH_ScoreDisplay), "SubmitScoreAndGoToBoard")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool PreventScoring(TNH_ScoreDisplay __instance, int score)
        {
            TNHTweakerLogger.Log("Preventing vanilla score submition", TNHTweakerLogger.LogType.TNH);

            GM.Omni.OmniFlags.AddScore(__instance.m_curSequenceID, score);

            __instance.m_hasCurrentScore = true;
            __instance.m_currentScore = score;

            if (EnableScoring.Value)
            {
                AnvilManager.Instance.StartCoroutine(HighScorePatches.SendScore(score));
            }

            //Draw local scores
            __instance.RedrawHighScoreDisplay(__instance.m_curSequenceID);

            GM.Omni.SaveToFile();

            return false;
        }

    }
}
