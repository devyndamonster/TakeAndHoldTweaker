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

namespace TNHTweaker
{
    public class TNHTweaker : BaseUnityPlugin
    {
        public static Dictionary<TNH_CharacterDef, Character> CharacterDict = new Dictionary<TNH_CharacterDef, Character>();
        public static Dictionary<SosigEnemyTemplate, SosigTemplate> SosigDict = new Dictionary<SosigEnemyTemplate, SosigTemplate>();

        private void Awake()
        {
            TNHTweakerLogger.Init();
            TNHTweakerLogger.Log("Hello World (from TNH Tweaker)", TNHTweakerLogger.LogType.General);

            TNHTweakerConfig.LoadConfigFile(this);

            Harmony.CreateAndPatchAll(typeof(TNHTweaker));
        }

    }
}
