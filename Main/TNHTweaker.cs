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
using Sodalite;

namespace TNHTweaker
{
    [BepInPlugin("devyndamonster.tnhtweaker", "TNH Tweaker", "2.0.0")]
    [BepInDependency(SodaliteConstants.Guid, SodaliteConstants.Version)]
    [BepInDependency("h3vr.otherloader", "1.3.0")]
    public class TNHTweaker : BaseUnityPlugin
    {
        public static TNHTweaker Instance;

        public static Dictionary<TNH_CharacterDef, Character> CustomCharacterDict = new Dictionary<TNH_CharacterDef, Character>();
        public static Dictionary<SosigEnemyTemplate, SosigTemplate> CustomSosigDict = new Dictionary<SosigEnemyTemplate, SosigTemplate>();

        public static Dictionary<Character, TNH_CharacterDef> BaseCharacterDict = new Dictionary<Character, TNH_CharacterDef>();
        public static Dictionary<SosigTemplate, SosigEnemyTemplate> BaseSosigDict = new Dictionary<SosigTemplate, SosigEnemyTemplate>();

        private void Awake()
        {
            Instance = this;

            TNHTweakerConfig.LoadConfigFile(this);

            TNHTweakerLogger.Init();
            TNHTweakerLogger.Log("Hello World (from TNH Tweaker)", TNHTweakerLogger.LogType.General);

            Harmony.CreateAndPatchAll(typeof(TNHMenuPatches));
        }

    }
}
