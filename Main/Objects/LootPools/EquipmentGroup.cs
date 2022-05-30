using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.ObjectWrappers;
using UnityEngine;

namespace TNHTweaker.Objects.LootPools
{

    [CreateAssetMenu(menuName = "TNHTweaker/EquipmentGroup", fileName = "NewEquipmentGroup")]
    public class EquipmentGroup : ScriptableObject
    {
        [Tooltip("Cummulative chance of this group being spawned from. Larger values means more likely. Default value is 1")]
        public float Rarity = 1;

        [Tooltip("Number of items from this group that will be selected to spawn (randomly selected)")]
        public int ItemsToSpawn = 1;

        [Tooltip("Number of magazines that will spawn with the item")]
        public int NumMagsSpawned = 3;

        [Tooltip("Number of clips that will spawn with the item, if it has no magazines")]
        public int NumClipsSpawned = 3;

        [Tooltip("Number of loose rounds that will spawn with the item, if it has no magazines or clips")]
        public int NumRoundsSpawned = 8;

        [Tooltip("When true, a single mag will be spawned with the item, as well as [NumClipsSpawned] clips")]
        public bool SpawnMagAndClip = false;

        [Tooltip("Chance of a bespoke attachment spawning with an item")]
        public float BespokeAttachmentChance = 0.5f;

        [Tooltip("When true, if this group is spawned from, all subgroups of this group will also spawn an item")]
        public bool ForceSpawnAllSubGroups = false;

        [Tooltip("The table of items that can spawn for this group")]
        public ObjectTable ObjectTable;

        [Tooltip("A list of tags that determine whether this table is spawnable from")]
        public List<string> RequiredTags = new List<string>();

        [Tooltip("A list of groups which could also be spawned from instead of spawning an item from this group. Based on the rarity of the subgroups")]
        public List<EquipmentGroup> SubGroups = new List<EquipmentGroup>();


        public bool CanSpawnFromPool()
        {
            foreach(string tag in RequiredTags)
            {
                if (!TNHManagerStateWrapper.Instance.IsTagActive(tag))
                {
                    return false;
                }
            }
            return ObjectTable.HasItems() || SubGroups.Any(o => o.CanSpawnFromPool());
        }

        public void GenerateTables()
        {
            ObjectTable.GenerateTable();

            foreach(EquipmentGroup group in SubGroups)
            {
                group.GenerateTables();
            }
        }

        public List<EquipmentGroup> GetEquipmentGroupsToSpawnFrom()
        {
            if (ForceSpawnAllSubGroups)
            {
                return GetGroupsForForcedSpawnAll();
            }

            return GetRandomGroupsToSpawnFrom();
        }

        private List<EquipmentGroup> GetGroupsForForcedSpawnAll()
        {
            List<EquipmentGroup> selectedGroups = new List<EquipmentGroup>();

            selectedGroups.Add(this);
            foreach(EquipmentGroup group in SubGroups.Where(o => o.CanSpawnFromPool()))
            {
                selectedGroups.AddRange(group.GetEquipmentGroupsToSpawnFrom());
            }

            return selectedGroups;
        }

        private List<EquipmentGroup> GetRandomGroupsToSpawnFrom()
        {
            List<EquipmentGroup> validSubGroups = SubGroups.Where(o => o.CanSpawnFromPool()).ToList();
            float totalChanceRange = ObjectTable.GeneratedObjects.Count + validSubGroups.Sum(o => o.Rarity);
            float randomValueSelection = UnityEngine.Random.Range(0, totalChanceRange);

            if (randomValueSelection < ObjectTable.GeneratedObjects.Count)
            {
                return new List<EquipmentGroup>() { this };
            }
            else
            {
                float summedChanceValue = ObjectTable.GeneratedObjects.Count;

                foreach (EquipmentGroup group in validSubGroups)
                {
                    if (summedChanceValue >= randomValueSelection)
                    {
                        return group.GetEquipmentGroupsToSpawnFrom();
                    }
                    summedChanceValue += group.Rarity;
                }
            }

            return new List<EquipmentGroup>();
        }

        public override string ToString()
        {
            return ObjectTable.ToString();
        }
    }
}
