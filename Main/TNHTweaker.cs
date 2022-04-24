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
using TNHTweaker.Objects.CharacterData;
using TNHTweaker.Objects.SosigData;
using TNHTweaker.Patches;

namespace TNHTweaker
{
    public class TNHTweaker : BaseUnityPlugin
    {
        public static Dictionary<TNH_CharacterDef, Character> CustomCharacterDict = new Dictionary<TNH_CharacterDef, Character>();
        public static Dictionary<SosigEnemyTemplate, SosigTemplate> CustomSosigDict = new Dictionary<SosigEnemyTemplate, SosigTemplate>();

        public static Dictionary<Character, TNH_CharacterDef> BaseCharacterDict = new Dictionary<Character, TNH_CharacterDef>();
        public static Dictionary<SosigTemplate, SosigEnemyTemplate> BaseSosigDict = new Dictionary<SosigTemplate, SosigEnemyTemplate>();

        private void Awake()
        {
            TNHTweakerLogger.Init();
            TNHTweakerLogger.Log("Hello World (from TNH Tweaker)", TNHTweakerLogger.LogType.General);

            TNHTweakerConfig.LoadConfigFile(this);

            Harmony.CreateAndPatchAll(typeof(TNHMenuPatches));
        }

    }
}
