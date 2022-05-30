using FistVR;
using HarmonyLib;
using Sodalite.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.Objects.CharacterData;
using TNHTweaker.Objects.LootPools;
using UnityEngine;

namespace TNHTweaker.Patches
{
    public class SupplyPointPatches
    {

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
            loadoutEntry.GenerateTables();
            EquipmentGroup selectedGroup = loadoutEntry.GetStartingEquipmentGroups().GetRandom();
            FVRObject selectedItem = selectedGroup.ObjectTable.GeneratedObjects.GetRandom();

            GameObject weaponCase = GM.TNH_Manager.SpawnWeaponCase
            ( 
                casePrefab,
                spawnPoint.position,
                spawnPoint.forward,
                selectedItem,
                selectedGroup.NumMagsSpawned,
                selectedGroup.NumRoundsSpawned,
                selectedGroup.ObjectTable.MinAmmoCapacity,
                selectedGroup.ObjectTable.MaxAmmoCapacity
            );

            __instance.m_trackedObjects.Add(weaponCase);
            weaponCase.GetComponent<TNH_WeaponCrate>().M = GM.TNH_Manager;
        }

        private static void SpawnStartingLooseEquipment(LoadoutEntry loadoutEntry, Transform spawnPoint)
        {
            loadoutEntry.GenerateTables();
            EquipmentGroup selectedGroup = loadoutEntry.GetStartingEquipmentGroups().GetRandom();
            FVRObject selectedItem = selectedGroup.ObjectTable.GeneratedObjects.GetRandom();

            GameObject spawnedItem = GameObject.Instantiate(selectedItem.GetGameObject(), spawnPoint.position, spawnPoint.rotation);
            GM.TNH_Manager.AddObjectToTrackedList(spawnedItem);
        }
    }
}
