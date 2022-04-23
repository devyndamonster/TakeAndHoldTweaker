using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TNHTweaker.Objects.LootPools
{
    public class EquipmentGroup : ScriptableObject
    {
        [Tooltip("Cummulative chance of this group being spawned from. Larger values means more likely. Default value is 1")]
        public float Rarity = 1;

        [Tooltip("Quest required for this group to be spawnable from. Considered unlocked if value is null or empty")]
        public string RequiredQuest = "";

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

        [Tooltip("A list of groups which could also be spawned from instead of spawning an item from this group. Based on the rarity of the subgroups")]
        public List<EquipmentGroup> SubGroups = new List<EquipmentGroup>();


        
        public static EquipmentGroup SelectGroupFromList(List<EquipmentGroup> groups)
        {
            //TODO function that will select a group based on cummulative chance
            return null;
        }


    }
}
