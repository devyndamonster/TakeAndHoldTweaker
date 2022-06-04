using FistVR;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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



        /// <summary>
        /// Patches SetPhase_Take method to use the SupplyChallenges min and max boxes values <br/><br/>
        /// Related Features: <br/>
        /// - <see href="https://github.com/devyndamonster/TakeAndHoldTweaker/issues/106"> Allow you to set min and max boxes spawned at supply points </see><br/>
        /// </summary>
        [HarmonyPatch(typeof(TNH_Manager), "SetPhase_Take")]
        [HarmonyILManipulator]
        public static void BeginAnalyzingNoTargetsPatch(ILContext ctx, MethodBase orig)
        {
            ILCursor cursor = new ILCursor(ctx);

            //Remove the two constant value for box counts
            cursor.GotoNext(MoveType.After,
                i => i.MatchLdcI4(1),
                i => i.MatchLdcI4(1),
                i => i.MatchLdcI4(1),
                i => i.MatchLdloc(16)
                );
            cursor.RemoveRange(2);

            //Now add calls to get min and max supply point boxes
            cursor.Emit(OpCodes.Ldsfld, AccessTools.Field(typeof(TNHManagerStateWrapper), "Instance"));
            cursor.Emit(OpCodes.Callvirt, AccessTools.Method(typeof(TNHManagerStateWrapper), "GetCurrentSupplyChallenge"));
            cursor.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(SupplyChallenge), "MinBoxesSpawned"));

            cursor.Emit(OpCodes.Ldsfld, AccessTools.Field(typeof(TNHManagerStateWrapper), "Instance"));
            cursor.Emit(OpCodes.Callvirt, AccessTools.Method(typeof(TNHManagerStateWrapper), "GetCurrentSupplyChallenge"));
            cursor.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(SupplyChallenge), "MaxBoxesSpawned"));
        }

    }
}
