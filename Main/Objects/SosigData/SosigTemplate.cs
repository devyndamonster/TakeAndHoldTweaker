using BepInEx.Logging;
using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.Objects.LootPools;
using TNHTweaker.Utilities;
using UnityEngine;

namespace TNHTweaker.Objects.SosigData
{
    [CreateAssetMenu(menuName = "TNHTweaker/SosigTemplate", fileName = "NewSosigTemplate")]
    public class SosigTemplate : ScriptableObject
    {
        public string DisplayName;
        public SosigEnemyCategory SosigEnemyCategory;
        public SosigEnemyID SosigEnemyID;
        public List<string> SosigPrefabs;
        public List<SosigConfigTemplate> Configs;
        public List<SosigConfigTemplate> ConfigsEasy;
        public List<OutfitConfig> OutfitConfigs;
        public List<string> WeaponOptions;
        public List<string> WeaponOptionsSecondary;
        public List<string> WeaponOptionsTertiary;
        public float SecondaryChance;
        public float TertiaryChance;
		public float DroppedLootChance;
        public EquipmentGroup DroppedLootPool;
    }
}
