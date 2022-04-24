using FistVR;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.ObjectWrappers;
using TNHTweaker.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace TNHTweaker.Patches
{
    public class TNHMenuPatches
    {

        [HarmonyPatch(typeof(TNH_UIManager), "Start")]
        [HarmonyPrefix]
        public static bool InitTNH(TNH_UIManager __instance)
        {
            TNHTweakerLogger.Log("Start method of TNH_UIManager just got called!", TNHTweakerLogger.LogType.General);

            //Add menu wrapper to UI
            TNHMenuUIWrapper menuWrapper = __instance.gameObject.AddComponent<TNHMenuUIWrapper>();
            menuWrapper.InitTNHUI(__instance);

            return true;
        }
    }
}
