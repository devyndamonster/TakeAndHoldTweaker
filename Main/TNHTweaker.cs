using ADepIn;
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

        public static string OutputFilePath;


        /// <summary>
        /// First method that gets called
        /// </summary>
        private void Awake()
        {
            TNHTweakerLogger.Init();
            TNHTweakerLogger.Log("Hello World (from TNH Tweaker)", TNHTweakerLogger.LogType.General);

            Harmony.CreateAndPatchAll(typeof(TNHTweaker));
            Harmony.CreateAndPatchAll(typeof(TNHPatches));
            //Harmony.CreateAndPatchAll(typeof(DebugPatches));

            Stages.Setup += OnSetup;
        }


        /// <summary>
        /// Performs initial setup for TNH Tweaker
        /// </summary>
        /// <param name="stage"></param>
        private void OnSetup(SetupStage stage)
        {
            LoadConfigFile();
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

            file = Source.Resources.GetFile("token_icon.png");
            result = TNHTweakerUtils.LoadSprite(file);
            MagazinePanel.buttonIcon = result;

            /*
            file = Source.Resources.GetFile("full_auto.png");
            result = TNHTweakerUtils.LoadSprite(file);
            LoadedTemplateManager.PanelSprites.Add(PanelType.AddFullAuto, result);

            file = Source.Resources.GetFile("ammo_purchase.png");
            result = TNHTweakerUtils.LoadSprite(file);
            LoadedTemplateManager.PanelSprites.Add(PanelType.AmmoPurchase, result);

            file = Source.Resources.GetFile("mag_purchase.png");
            result = TNHTweakerUtils.LoadSprite(file);
            LoadedTemplateManager.PanelSprites.Add(PanelType.MagPurchase, result);

            file = Source.Resources.GetFile("gas_up.png");
            result = TNHTweakerUtils.LoadSprite(file);
            LoadedTemplateManager.PanelSprites.Add(PanelType.FireRateUp, result);

            file = Source.Resources.GetFile("gas_down.png");
            result = TNHTweakerUtils.LoadSprite(file);
            LoadedTemplateManager.PanelSprites.Add(PanelType.FireRateDown, result);
            */
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



        /// <summary>
        /// Every time an asset bundle is loaded asyncronously, the callback is added to a global list which can be monitored to see when loading is complete
        /// </summary>
        /// <param name="__result"></param>
        [HarmonyPatch(typeof(AnvilManager), "GetBundleAsync")]
        [HarmonyPostfix]
        public static void AddMonitoredAnvilCallback(AnvilCallback<AssetBundle> __result)
        {
            AsyncLoadMonitor.CallbackList.Add(__result);
            TNHTweakerLogger.Log("TNHTweaker -- Added AssetBundle anvil callback to monitored callbacks!", TNHTweakerLogger.LogType.File);
        }

    }
}
