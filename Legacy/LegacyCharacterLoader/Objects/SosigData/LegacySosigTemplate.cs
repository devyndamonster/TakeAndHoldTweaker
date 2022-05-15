using FistVR;
using LegacyCharacterLoader.Objects.LootPools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Valve.Newtonsoft.Json;
using Valve.Newtonsoft.Json.Converters;

namespace LegacyCharacterLoader.Objects.SosigData
{
    public class LegacySosigTemplate
    {
        public string DisplayName;
        public SosigEnemyCategory SosigEnemyCategory;
        public string SosigEnemyID;
        public List<string> SosigPrefabs;
        public List<LegacySosigConfig> Configs;
        public List<LegacySosigConfig> ConfigsEasy;
        public List<LegacyOutfitConfig> OutfitConfigs;
        public List<string> WeaponOptions;
        public List<string> WeaponOptionsSecondary;
        public List<string> WeaponOptionsTertiary;
        public float SecondaryChance;
        public float TertiaryChance;
        public float DroppedLootChance;
        public LegacyEquipmentGroup DroppedObjectPool;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented, new StringEnumConverter());
        }
    }
}
