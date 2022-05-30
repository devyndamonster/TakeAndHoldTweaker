using FistVR;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.Objects.CharacterData;
using TNHTweaker.Objects.LootPools;
using TNHTweaker.ObjectWrappers;

namespace TNHTweaker.Patches
{
    public static class TNHManagerPatches
    {

        /// <summary>
        /// Before initializing the base character classes tables, initialize the extended character classes tables <br/><br/>
        /// Related Features: <br/>
        /// - <see href="https://github.com/devyndamonster/TakeAndHoldTweaker/issues/101"> Have starting equipment use our own EquipmentGroup loot system </see><br/>
        /// - <see href="https://github.com/devyndamonster/TakeAndHoldTweaker/issues/105"> Have object constructor use our own EquipmentGroup loot system </see><br/>
        /// </summary>
        [HarmonyPatch(typeof(TNH_Manager), "InitTables")]
        [HarmonyPrefix]
        public static bool InitTablesPatch(TNH_Manager __instance)
        {
            TNHManagerStateWrapper.Instance.GetCurrentCharacter().GenerateTables();

            return true;
        }

    }
}
