using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.Objects.CharacterData;
using TNHTweaker.Objects.LootPools;
using TNHTweaker.Utilities;
using UnityEngine;

namespace TNHTweaker.ObjectConverters
{
    public static class CharacterConverter
    {
        public static Character ConvertCharacterFromVanilla(TNH_CharacterDef from)
        {
			Character character = ScriptableObject.CreateInstance<Character>();

			character.DisplayName = from.DisplayName;
			character.CharacterID = from.CharacterID;
			character.Group = from.Group;
			character.TableID = from.TableID;
			character.Picture = from.Picture;
			character.Description = from.Description;
			character.StartingTokens = from.StartingTokens;
			character.ForceAllAgentWeapons = from.ForceAllAgentWeapons;
			character.UsesPurchasePriceIncrement = from.UsesPurchasePriceIncrement;
			character.RequireSightTable = ScriptableObject.CreateInstance<EquipmentGroup>();
			character.RequireSightTable.ObjectTable = ObjectTableConverter.ConvertObjectTableFromVanilla(from.RequireSightTable);
			character.ValidAmmoEras = from.ValidAmmoEras;
			character.ValidAmmoSets = from.ValidAmmoSets;
			character.EquipmentPools = from.EquipmentPool.Entries.Select(o => EquipmentPoolConverter.ConvertEquipmentPoolEntryFromVanilla(o)).ToList();
			character.Progressions = from.Progressions.Select(o => ProgressionConverter.ConvertProgressionFromVanilla(o)).ToList();
			character.Progressions_Endless = from.Progressions_Endless.Select(o => ProgressionConverter.ConvertProgressionFromVanilla(o)).ToList();
			character.Has_Weapon_Primary = from.Has_Weapon_Primary;
			character.Has_Weapon_Secondary = from.Has_Weapon_Secondary;
			character.Has_Weapon_Tertiary = from.Has_Weapon_Tertiary;
			character.Has_Item_Primary = from.Has_Item_Primary;
			character.Has_Item_Secondary = from.Has_Item_Secondary;
			character.Has_Item_Tertiary = from.Has_Item_Tertiary;
			character.Has_Item_Shield = from.Has_Item_Shield;
			if (from.Weapon_Primary != null) character.Weapon_Primary = LoadoutEntryConverter.ConvertLoadoutEntryFromVanilla(from.Weapon_Primary);
			if (from.Weapon_Secondary != null) character.Weapon_Secondary = LoadoutEntryConverter.ConvertLoadoutEntryFromVanilla(from.Weapon_Secondary);
			if (from.Weapon_Tertiary != null) character.Weapon_Tertiary = LoadoutEntryConverter.ConvertLoadoutEntryFromVanilla(from.Weapon_Tertiary);
			if (from.Item_Primary != null) character.Item_Primary = LoadoutEntryConverter.ConvertLoadoutEntryFromVanilla(from.Item_Primary);
			if (from.Item_Secondary != null) character.Item_Secondary = LoadoutEntryConverter.ConvertLoadoutEntryFromVanilla(from.Item_Secondary);
			if (from.Item_Tertiary != null) character.Item_Tertiary = LoadoutEntryConverter.ConvertLoadoutEntryFromVanilla(from.Item_Tertiary);
			if (from.Item_Shield != null) character.Item_Shield = LoadoutEntryConverter.ConvertLoadoutEntryFromVanilla(from.Item_Shield);

			return character;
        }


		public static TNH_CharacterDef ConvertCharacterToVanilla(Character from)
		{
			TNH_CharacterDef character = ScriptableObject.CreateInstance<TNH_CharacterDef>();
			LogConversionStart(character);

			character.DisplayName = from.DisplayName;
			character.CharacterID = from.CharacterID;
			character.Group = from.Group;
			character.TableID = from.TableID;
			character.Picture = from.Picture;
			character.Description = from.Description;
			character.StartingTokens = from.StartingTokens;
			character.ForceAllAgentWeapons = from.ForceAllAgentWeapons;
			character.UsesPurchasePriceIncrement = from.UsesPurchasePriceIncrement;
			character.RequireSightTable = ObjectTableConverter.ConvertObjectTableToVanilla(from.RequireSightTable.ObjectTable);
			character.ValidAmmoEras = from.ValidAmmoEras;
			character.ValidAmmoSets = from.ValidAmmoSets;
			character.EquipmentPool = ScriptableObject.CreateInstance<EquipmentPoolDef>();
			character.EquipmentPool.Entries = from.EquipmentPools.Select(o => EquipmentPoolConverter.ConvertEquipmentPoolEntryToVanilla(o)).ToList();
			character.Progressions = from.Progressions.Select(o => ProgressionConverter.ConvertProgressionToVanilla(o)).ToList();
			character.Progressions_Endless = from.Progressions_Endless.Select(o => ProgressionConverter.ConvertProgressionToVanilla(o)).ToList();
			character.Has_Weapon_Primary = from.Has_Weapon_Primary;
			character.Has_Weapon_Secondary = from.Has_Weapon_Secondary;
			character.Has_Weapon_Tertiary = from.Has_Weapon_Tertiary;
			character.Has_Item_Primary = from.Has_Item_Primary;
			character.Has_Item_Secondary = from.Has_Item_Secondary;
			character.Has_Item_Tertiary = from.Has_Item_Tertiary;
			character.Has_Item_Shield = from.Has_Item_Shield;
			if (from.Weapon_Primary != null) character.Weapon_Primary = LoadoutEntryConverter.ConvertLoadoutEntryToVanilla(from.Weapon_Primary);
			if (from.Weapon_Secondary != null) character.Weapon_Secondary = LoadoutEntryConverter.ConvertLoadoutEntryToVanilla(from.Weapon_Secondary);
			if (from.Weapon_Tertiary != null) character.Weapon_Tertiary = LoadoutEntryConverter.ConvertLoadoutEntryToVanilla(from.Weapon_Tertiary);
			if (from.Item_Primary != null) character.Item_Primary = LoadoutEntryConverter.ConvertLoadoutEntryToVanilla(from.Item_Primary);
			if (from.Item_Secondary != null) character.Item_Secondary = LoadoutEntryConverter.ConvertLoadoutEntryToVanilla(from.Item_Secondary);
			if (from.Item_Tertiary != null) character.Item_Tertiary = LoadoutEntryConverter.ConvertLoadoutEntryToVanilla(from.Item_Tertiary);
			if (from.Item_Shield != null) character.Item_Shield = LoadoutEntryConverter.ConvertLoadoutEntryToVanilla(from.Item_Shield);
			
			LogConversionEnd(character);
			return character;
		}

		private static void LogConversionStart(TNH_CharacterDef character)
		{
			TNHTweakerLogger.Log("- Starting conversion of character to vanilla -", TNHTweakerLogger.LogType.Loading);
		}

		private static void LogConversionEnd(TNH_CharacterDef character)
		{
			TNHTweakerLogger.Log("Equipment pool count : " + character.EquipmentPool.Entries.Count(), TNHTweakerLogger.LogType.Loading);
			if (character.Has_Weapon_Primary) TNHTweakerLogger.Log("Has primary loadout, is it null? : " + (character.Weapon_Primary == null), TNHTweakerLogger.LogType.Loading);
			if (character.Has_Weapon_Secondary) TNHTweakerLogger.Log("Has secondary loadout, is it null? : " + (character.Weapon_Secondary == null), TNHTweakerLogger.LogType.Loading);
			TNHTweakerLogger.Log("- Successfully converted character to vanilla -", TNHTweakerLogger.LogType.Loading);
		}
	}
}
