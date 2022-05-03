using FistVR;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TNHTweaker.ObjectWrappers;
using UnityEngine;

namespace TNHTweaker.Patches
{
    public class HoldPointPatches
    {

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
            c.RemoveRange(13);

            //Replace removed lines with our own call to spawn encryptions
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc_2);
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(HoldPointPatches), "SpawnEncryption"));
        }


        public static GameObject SpawnEncryption(TNH_HoldPoint holdPoint, int encryptionIndex)
        {
            TNH_EncryptionType selectedEncryptionType = TNHManagerStateWrapper.Instance.GetCurrentHoldPhase().GetEncryptionFromIndex(encryptionIndex);
            FVRObject encryptionPrefab = holdPoint.M.GetEncryptionPrefab(selectedEncryptionType);
            Transform selectedSpawnPoint = holdPoint.m_validSpawnPoints[encryptionIndex];
            return GameObject.Instantiate(encryptionPrefab.GetGameObject(), selectedSpawnPoint.position, selectedSpawnPoint.rotation);
        }

    }
}
