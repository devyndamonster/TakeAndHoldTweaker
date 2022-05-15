using FistVR;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Reflection;
using TNHTweaker.ObjectWrappers;
using UnityEngine;

namespace TNHTweaker.Patches
{
    public class HoldPointPatches
    {

        [HarmonyPatch(typeof(TNH_HoldPoint), "SpawnTargetGroup")]
        [HarmonyPrefix]
        public static bool SpawnTargetGroupPatch(TNH_HoldPoint __instance)
        {
            __instance.DeleteAllActiveWarpIns();
            SetNumEncryptionsToSpawn(__instance);
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

        private static void SetNumEncryptionsToSpawn(TNH_HoldPoint __instance)
        {
            if(GM.TNH_Manager.EquipmentMode == TNHSetting_EquipmentMode.LimitedAmmo)
            {
                __instance.m_numTargsToSpawn = 1;
            }
            else
            {
                __instance.m_numTargsToSpawn = Mathf.Clamp(__instance.m_numTargsToSpawn, 1, 3);
            }
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


        /* This was an attempt at making an IL patch, but couldn't get around a Label Not Marked error :(
        [HarmonyPatch(typeof(TNH_HoldPoint), "SpawnTargetGroup")]
        [HarmonyILManipulator]
        private static void SpawnTargetGroupPatch(ILContext ctx, MethodBase orig)
        {
            ILCursor c = new ILCursor(ctx);

            //Remove call to instantiate a new encryption
            c.GotoNext(
                i => i.MatchLdloc(1),
                i => i.MatchCallvirt(AccessTools.Method(typeof(AnvilAsset), "GetGameObject"))
            );
            c.RemoveRange(14);
            
            //Replace removed lines with our own call to spawn encryptions
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc_2);
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(HoldPointPatches), "SpawnEncryption"));
            c.Emit(OpCodes.Stloc_3);
        }
        */
    }
}
