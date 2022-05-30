﻿using FistVR;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Reflection;
using TNHTweaker.ObjectWrappers;
using TNHTweaker.Utilities;
using UnityEngine;

namespace TNHTweaker.Patches
{
    public class HoldPointPatches
    {

        /// <summary>
        /// Overrides logic that sets the number of encryptions that will spawn during a hold <br/><br/>
        /// Related Features: <br/>
        /// - <see href="https://github.com/devyndamonster/TakeAndHoldTweaker/issues/99"> Allow for min and max encryptions to be set for limited ammo mode </see><br/>
        /// </summary>
        [HarmonyPatch(typeof(TNH_HoldPoint), "SpawnWarpInMarkers")]
        [HarmonyILManipulator]
        public static void SpawnWarpInMarkersPatch(ILContext ctx, MethodBase orig)
        {
            ILCursor cursor = new ILCursor(ctx);

            //Go to right after we set m_numTargsToSpawn based on min and max targets
            cursor.GotoNext(
                i => i.MatchLdarg(0),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(AccessTools.Field(typeof(TNH_HoldPoint), "m_numTargsToSpawn")),
                i => i.MatchLdarg(0)
                );

            //Set m_numTargsToSpawn from our own call
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, ((Func<int>)GetNumEncryptionsToSpawn).Method);
            cursor.Emit(OpCodes.Stfld, AccessTools.Field(typeof(TNH_HoldPoint), "m_numTargsToSpawn"));
        }


        /// <summary>
        /// Replaces entire call that spawns in encryptions with our own <br/><br/>
        /// Related Features: <br/>
        /// - <see href="https://github.com/devyndamonster/TakeAndHoldTweaker/issues/99"> Allow for min and max encryptions to be set for limited ammo mode </see><br/>
        /// - <see href="https://github.com/devyndamonster/TakeAndHoldTweaker/issues/100"> Allow for mixed encryption types to spawn in during hold phases </see><br/>
        /// </summary>
        [HarmonyPatch(typeof(TNH_HoldPoint), "SpawnTargetGroup")]
        [HarmonyPrefix]
        public static bool SpawnTargetGroupPatch(TNH_HoldPoint __instance)
        {
            __instance.DeleteAllActiveWarpIns();
            ShuffleSpawnsIfStealth(__instance);

            for(int encryptionIndex = 0; encryptionIndex < __instance.m_numTargsToSpawn; encryptionIndex++)
            {
                TNH_EncryptionType selectedEncryptionType = GetEncryptionTypeToSpawn(encryptionIndex, __instance);
                SpawnEncryption(__instance, encryptionIndex, selectedEncryptionType);
            }

            return false;
        }

        private static void ShuffleSpawnsIfStealth(TNH_HoldPoint __instance)
        {
            if(__instance.m_curPhase.Encryption == TNH_EncryptionType.Stealth)
            {
                __instance.m_validSpawnPoints.Shuffle();
            }
        }

        public static int GetNumEncryptionsToSpawn()
        {
            return TNHManagerStateWrapper.Instance.GetCurrentHoldPhase().GetNumTargetsToSpawn(GM.TNH_Manager.EquipmentMode);
        }

        private static TNH_EncryptionType GetEncryptionTypeToSpawn(int encryptionIndex, TNH_HoldPoint __instance)
        {
            if(GM.TNH_Manager.TargetMode == TNHSetting_TargetMode.Simple)
            {
                return TNH_EncryptionType.Static;
            }

            else
            {
                return TNHManagerStateWrapper.Instance.GetCurrentHoldPhase().GetEncryptionFromIndex(encryptionIndex);
            }
        }


        public static GameObject SpawnEncryption(TNH_HoldPoint holdPoint, int encryptionIndex, TNH_EncryptionType encryptionType)
        {
            FVRObject encryptionPrefab = holdPoint.M.GetEncryptionPrefab(encryptionType);
            Transform selectedSpawnPoint = holdPoint.m_validSpawnPoints[encryptionIndex];
            return GameObject.Instantiate(encryptionPrefab.GetGameObject(), selectedSpawnPoint.position, selectedSpawnPoint.rotation);
        }
    }
}