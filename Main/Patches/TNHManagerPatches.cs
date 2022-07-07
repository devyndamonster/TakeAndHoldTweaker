using FistVR;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Sodalite.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TNHTweaker.ObjectConverters;
using TNHTweaker.Objects.CharacterData;
using TNHTweaker.Objects.LootPools;
using TNHTweaker.ObjectWrappers;
using TNHTweaker.Utilities;
using UnityEngine;

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
        public static void SetPhaseTakeBoxCountPatch(ILContext ctx, MethodBase orig)
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


        /// <summary>
        /// Patches GenerateValidPatrol method to call our own GeneratePatrol method <br/><br/>
        /// Related Features: <br/>
        /// - <see href="https://github.com/devyndamonster/TakeAndHoldTweaker/issues/111"> Allow multiple types of sosigs to spawn in patrols </see><br/>
        /// </summary>
        [HarmonyPatch(typeof(TNH_Manager), "GenerateValidPatrol")]
        [HarmonyILManipulator]
        public static void GenerateValidPatrolPatch(ILContext ctx, MethodBase orig)
        {
            ILCursor cursor = new ILCursor(ctx);

            //Go to where SosigEnemyID is accessed, and remove it
            cursor.GotoNext(
                i => i.MatchLdloc(0),
                i => i.MatchLdfld(AccessTools.Field(typeof(TNH_PatrolChallenge.Patrol), "EType"))
                );
            cursor.Index += 1;
            cursor.RemoveRange(1);

            //Replace SosigEnemyID with a call to access a random one
            cursor.Emit(OpCodes.Call, ((Func<TNH_PatrolChallenge.Patrol, SosigEnemyID>)GetRandomEnemyFromPatrol).Method);
        }


        /// <summary>
        /// Patches GenerateSentryPatrol method to spawn multiple types of sosigs <br/><br/>
        /// Related Features: <br/>
        /// - <see href="https://github.com/devyndamonster/TakeAndHoldTweaker/issues/111"> Allow multiple types of sosigs to spawn in patrols </see><br/>
        /// </summary>
        [HarmonyPatch(typeof(TNH_Manager), "GenerateSentryPatrol")]
        [HarmonyILManipulator]
        public static void GenerateSentryPatrolPatch(ILContext ctx, MethodBase orig)
        {
            ILCursor cursor = new ILCursor(ctx);

            //Go to where SosigEnemyID is accessed, and remove it
            cursor.GotoNext(
                i => i.MatchLdarg(1),
                i => i.MatchLdfld(AccessTools.Field(typeof(TNH_PatrolChallenge.Patrol), "EType"))
                );
            cursor.Index += 1;
            cursor.RemoveRange(1);

            //Replace SosigEnemyID with a call to access a random one
            cursor.Emit(OpCodes.Call, ((Func<TNH_PatrolChallenge.Patrol, SosigEnemyID>)GetRandomEnemyFromPatrol).Method);
        }


        /// <summary>
        /// Patches UpdatePatrols method to not run if there are no valid patrols to spawn <br/><br/>
        /// Related Features: <br/>
        /// - <see href="https://github.com/devyndamonster/TakeAndHoldTweaker/issues/113"> Allow for boss patrols that can only spawn once </see><br/>
        /// </summary>
        [HarmonyPatch(typeof(TNH_Manager), "UpdatePatrols")]
        [HarmonyPrefix]
        public static bool UpdatePatrolsPatch(TNH_Manager __instance)
        {
            if(__instance.m_timeTilPatrolCanSpawn <= 0f)
            {
                return TNHManagerStateWrapper.Instance.GetCurrentLevel().Patrols.Any(o => TNHManagerStateWrapper.Instance.CanPatrolSpawn(o));
            }

            return true;
        }



        public static SosigEnemyID GetRandomEnemyFromPatrol(TNH_PatrolChallenge.Patrol patrol)
        {
            return PatrolConverter.PatrolFromVanilla[patrol].EnemyTypes.GetRandom();
        }

    }
}
