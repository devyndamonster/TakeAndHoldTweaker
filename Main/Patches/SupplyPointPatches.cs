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
using TNHTweaker.Objects.CharacterData;
using TNHTweaker.Objects.LootPools;
using TNHTweaker.ObjectWrappers;
using TNHTweaker.Utilities;
using UnityEngine;

namespace TNHTweaker.Patches
{
    public static class SupplyPointPatches
    {

        /// <summary>
        /// Replaces entire call that spawns supply point boxes <br/><br/>
        /// Related Features: <br/>
        /// - <see href="https://github.com/devyndamonster/TakeAndHoldTweaker/issues/106"> Allow you to set min and max boxes spawned at supply points </see><br/>
        /// - <see href="https://github.com/devyndamonster/TakeAndHoldTweaker/issues/107"> Allow you to set min and max tokens per supply point </see><br/>
        /// - <see href="https://github.com/devyndamonster/TakeAndHoldTweaker/issues/108"> Allow you to set min and max health drops per supply point </see><br/>
        /// </summary>
        [HarmonyPatch(typeof(TNH_SupplyPoint), "SpawnBoxes")]
        [HarmonyPrefix]
        public static bool SpawnBoxesReplacementPatch(TNH_SupplyPoint __instance, int min, int max, bool SpawnToken)
        {
            __instance.SpawnPoints_Boxes.Shuffle();

             SupplyChallenge supplyChallenge = TNHManagerStateWrapper.Instance.GetCurrentLevel().SupplyChallenge;

            int tokensSpawned = 0;
            int healthSpawned = 0;
            int boxesToSpawn = UnityEngine.Random.Range(min, max + 1);
            for(int boxIndex = 0; boxIndex < boxesToSpawn; boxIndex++)
            {
                GameObject spawnedBox = SpawnSupplyBox(__instance);
                __instance.m_spawnBoxes.Add(spawnedBox);

                if (ShouldBoxContainToken(supplyChallenge, tokensSpawned, SpawnToken))
                {
                    spawnedBox.GetComponent<TNH_ShatterableCrate>().SetHoldingToken(__instance.M);
                    tokensSpawned += 1;
                }

                else if (ShouldBoxContainHealth(supplyChallenge, healthSpawned))
                {
                    spawnedBox.GetComponent<TNH_ShatterableCrate>().SetHoldingHealth(__instance.M);
                    healthSpawned += 1;
                }
            }

            return false;
        }

        private static GameObject SpawnSupplyBox(TNH_SupplyPoint __instance)
        {
            Transform spawnTransform = __instance.SpawnPoints_Boxes.GetRandom();

            Vector3 yOffset = Vector3.up * 0.1f;
            Vector3 xOffset = Vector3.right * UnityEngine.Random.Range(-0.5f, 0.5f);
            Vector3 zOffset = Vector3.forward * UnityEngine.Random.Range(-0.5f, 0.5f);
            Vector3 spawnPosition = spawnTransform.position + yOffset + xOffset + zOffset;
            Quaternion spawnRotation = Quaternion.Slerp(spawnTransform.rotation, UnityEngine.Random.rotation, 0.1f);

            GameObject spawnedBox = UnityEngine.Object.Instantiate(__instance.M.Prefabs_ShatterableCrates.GetRandom(), spawnPosition, spawnRotation);

            return spawnedBox;
        }


        private static bool ShouldBoxContainToken(SupplyChallenge supplyChallenge, int tokensSpawned, bool spawnToken)
        {
            if (tokensSpawned < supplyChallenge.MinTokensPerSupply) return true;
            else if (tokensSpawned >= supplyChallenge.MaxTokensPerSupply) return false;
            else return spawnToken && UnityEngine.Random.Range(0f, 1f) <= supplyChallenge.BoxTokenChance;
        }


        private static bool ShouldBoxContainHealth(SupplyChallenge supplyChallenge, int healthSpawned)
        {
            float spawnChance = GetHealthSpawnChance(healthSpawned);
            if (healthSpawned < supplyChallenge.MinHealthDropsPerSupply) return true;
            else if (healthSpawned >= supplyChallenge.MaxHealthDropsPerSupply) return false;
            return UnityEngine.Random.Range(0f, 1f) <= spawnChance;
        }


        private static float GetHealthSpawnChance(int healthSpawned)
        {
            switch (healthSpawned)
            {
                case 0:
                    return 0.9f;
                case 1:
                    return 0.6f;
                default:
                    return 0.2f;
            }
        }


        /// <summary>
        /// Replaces entire call that configures the player beginning equipment when the game starts <br/><br/>
        /// Related Features: <br/>
        /// - <see href="https://github.com/devyndamonster/TakeAndHoldTweaker/issues/101"> Have starting equipment use our own EquipmentGroup loot system </see><br/>
        /// </summary>
        [HarmonyPatch(typeof(TNH_SupplyPoint), "ConfigureAtBeginning")]
        [HarmonyPrefix]
        public static bool ConfigureAtBeginningPatch(TNH_SupplyPoint __instance, TNH_CharacterDef c)
        {
            __instance.m_trackedObjects.Clear();
            MoveSpawnerIfEnabled(__instance);
            SpawnStartingTables(__instance);

            Character character = TNHTweaker.CustomCharacterDict[c];
            if (character.Has_Weapon_Primary) SpawnStartingWeaponCrate(character.Weapon_Primary, GM.TNH_Manager.Prefab_WeaponCaseLarge, __instance.SpawnPoint_CaseLarge, __instance);
            if (character.Has_Weapon_Secondary) SpawnStartingWeaponCrate(character.Weapon_Secondary, GM.TNH_Manager.Prefab_WeaponCaseSmall, __instance.SpawnPoint_CaseSmall, __instance);
            if (character.Has_Weapon_Tertiary) SpawnStartingLooseEquipment(character.Weapon_Tertiary, __instance.SpawnPoint_Melee);
            if (character.Has_Item_Primary) SpawnStartingLooseEquipment(character.Item_Primary, __instance.SpawnPoints_SmallItem[0]);
            if (character.Has_Item_Secondary) SpawnStartingLooseEquipment(character.Item_Secondary, __instance.SpawnPoints_SmallItem[1]);
            if (character.Has_Item_Tertiary) SpawnStartingLooseEquipment(character.Item_Tertiary, __instance.SpawnPoints_SmallItem[2]);
            if (character.Has_Item_Shield) SpawnStartingLooseEquipment(character.Item_Shield, __instance.SpawnPoint_Shield);

            return false;
        }

        private static void MoveSpawnerIfEnabled(TNH_SupplyPoint __instance)
        {
            if(GM.TNH_Manager.ItemSpawnerMode == TNH_ItemSpawnerMode.On)
            {
                GM.TNH_Manager.ItemSpawner.transform.position = __instance.SpawnPoints_Panels[0].position + Vector3.up * 0.8f;
                GM.TNH_Manager.ItemSpawner.transform.rotation = __instance.SpawnPoints_Panels[0].rotation;
                GM.TNH_Manager.ItemSpawner.SetActive(true);
            }
        }

        private static void SpawnStartingTables(TNH_SupplyPoint __instance)
        {
            foreach(Transform tablePoint in __instance.SpawnPoint_Tables)
            {
                GameObject table = GameObject.Instantiate(GM.TNH_Manager.Prefab_MetalTable, tablePoint.position, tablePoint.rotation);
                __instance.m_trackedObjects.Add(table);
            }
        }

        private static void SpawnStartingWeaponCrate(LoadoutEntry loadoutEntry, GameObject casePrefab, Transform spawnPoint, TNH_SupplyPoint __instance)
        {
            TNHTweakerLogger.Log(loadoutEntry.ToString, TNHTweakerLogger.LogType.TNH);

            EquipmentGroup selectedGroup = loadoutEntry.GetStartingEquipmentGroups().GetRandom();
            FVRObject selectedItem = selectedGroup.ObjectTable.GeneratedObjects.GetRandom();

            GameObject weaponCase = GM.TNH_Manager.SpawnWeaponCase
            ( 
                casePrefab,
                spawnPoint.position,
                spawnPoint.forward,
                selectedItem,
                Mathf.Clamp(selectedGroup.NumMagsSpawned, 0, 3),
                Mathf.Clamp(selectedGroup.NumRoundsSpawned, 0, 6),
                selectedGroup.ObjectTable.MinAmmoCapacity,
                selectedGroup.ObjectTable.MaxAmmoCapacity
            );

            __instance.m_trackedObjects.Add(weaponCase);
            TNH_WeaponCrate weaponCaseComp = weaponCase.GetComponent<TNH_WeaponCrate>();
            weaponCaseComp.M = GM.TNH_Manager;
        }

        private static void SpawnStartingLooseEquipment(LoadoutEntry loadoutEntry, Transform spawnPoint)
        {
            EquipmentGroup selectedGroup = loadoutEntry.GetStartingEquipmentGroups().GetRandom();
            FVRObject selectedItem = selectedGroup.ObjectTable.GeneratedObjects.GetRandom();

            GameObject spawnedItem = GameObject.Instantiate(selectedItem.GetGameObject(), spawnPoint.position, spawnPoint.rotation);
            GM.TNH_Manager.AddObjectToTrackedList(spawnedItem);
        }


        /// <summary>
        /// Patches SpawnTakeEnemyGroup method to use the SupplyChallenges random sosig IDs <br/><br/>
        /// Related Features: <br/>
        /// - <see href="https://github.com/devyndamonster/TakeAndHoldTweaker/issues/109"> Allow multiple types of sosigs to spawn at supply points </see><br/>
        /// </summary>
        [HarmonyPatch(typeof(TNH_SupplyPoint), "SpawnTakeEnemyGroup")]
        [HarmonyILManipulator]
        public static void SpawnTakeEnemyGroupPatch(ILContext ctx, MethodBase orig)
        {
            ILCursor cursor = new ILCursor(ctx);

            //Go to where SosigEnemyID is accessed, and remove it
            cursor.GotoNext(
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(AccessTools.Field(typeof(TNH_SupplyPoint), "T")),
                i => i.MatchLdfld(AccessTools.Field(typeof(TNH_TakeChallenge), "GID"))
                );
            cursor.RemoveRange(3);

            //Replace SosigEnemyID with a call to access a random one
            cursor.Emit(OpCodes.Ldsfld, AccessTools.Field(typeof(TNHManagerStateWrapper), "Instance"));
            cursor.Emit(OpCodes.Callvirt, AccessTools.Method(typeof(TNHManagerStateWrapper), "GetCurrentSupplyChallenge"));
            cursor.Emit(OpCodes.Callvirt, AccessTools.Method(typeof(SupplyChallenge), "GetSosigEnemyIdToSpawn"));
        }
    }
}
