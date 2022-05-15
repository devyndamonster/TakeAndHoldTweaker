using LegacyCharacterLoader.Objects.LootPools;
using MagazinePatcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Valve.Newtonsoft.Json;
using Valve.Newtonsoft.Json.Converters;

namespace LegacyCharacterLoader.Objects.CharacterData
{
    public class LegacyCharacter
    {
        public string DisplayName;
        public string Description;
        public int CharacterGroup;
        public string TableID;
        public int StartingTokens;
        public bool ForceAllAgentWeapons;
        public bool ForceDisableOutfitFunctionality;
        public bool UsesPurchasePriceIncrement;
        public bool DisableCleanupSosigDrops;
        public bool HasPrimaryWeapon;
        public bool HasSecondaryWeapon;
        public bool HasTertiaryWeapon;
        public bool HasPrimaryItem;
        public bool HasSecondaryItem;
        public bool HasTertiaryItem;
        public bool HasShield;
        public List<TagEra> ValidAmmoEras;
        public List<TagSet> ValidAmmoSets;
        public List<string> GlobalAmmoBlacklist;
        public List<MagazineBlacklistEntry> MagazineBlacklist;
        public LegacyEquipmentGroup RequireSightTable;
        public LegacyLoadoutEntry PrimaryWeapon;
        public LegacyLoadoutEntry SecondaryWeapon;
        public LegacyLoadoutEntry TertiaryWeapon;
        public LegacyLoadoutEntry PrimaryItem;
        public LegacyLoadoutEntry SecondaryItem;
        public LegacyLoadoutEntry TertiaryItem;
        public LegacyLoadoutEntry Shield;
        public List<LegacyEquipmentPool> EquipmentPools;
        public List<LegacyLevel> Levels;
        public List<LegacyLevel> LevelsEndless;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented, new StringEnumConverter());
        }
    }

    
}
