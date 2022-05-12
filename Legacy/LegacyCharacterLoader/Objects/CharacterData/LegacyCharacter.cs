using LegacyCharacterLoader.Enums;
using LegacyCharacterLoader.Objects.LootPools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public LoadoutEntry PrimaryWeapon;
        public LoadoutEntry SecondaryWeapon;
        public LoadoutEntry TertiaryWeapon;
        public LoadoutEntry PrimaryItem;
        public LoadoutEntry SecondaryItem;
        public LoadoutEntry TertiaryItem;
        public LoadoutEntry Shield;
        public List<EquipmentPool> EquipmentPools;
        public List<Level> Levels;
        public List<Level> LevelsEndless;
    }
}
