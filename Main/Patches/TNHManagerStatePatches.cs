using FistVR;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.ObjectWrappers;

namespace TNHTweaker.Patches
{
    public static class TNHManagerStatePatches
    {

        [HarmonyPatch(typeof(TNH_Manager), "Start")]
        [HarmonyPrefix]
        public static bool AddStateWrapperPatch(TNH_Manager __instance)
        {
            __instance.gameObject.AddComponent<TNHManagerStateWrapper>();
            return true;
        }

    }
}
