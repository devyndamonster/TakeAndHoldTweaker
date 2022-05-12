using FistVR;
using LegacyCharacterLoader.Objects.CharacterData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TNHTweaker.Objects.CharacterData;
using TNHTweaker.Objects.LootPools;
using TNHTweaker.Utilities;
using UnityEngine;

namespace LegacyCharacterLoader.LegacyConverters
{
    public static class LegacyCharacterConverter
    {
		public static Character ConvertCharacterFromLegacy(LegacyCharacter from, string characterPath)
		{
			Character character = ScriptableObject.CreateInstance<Character>();

			character.DisplayName = from.DisplayName;
			character.CharacterID = CharacterUtils.GetUniqueTNHCharValue();
			character.Group = CharacterUtils.GetGroupStringFromEnum((TNH_CharacterDef.CharacterGroup)from.CharacterGroup);
			character.TableID = from.TableID;
			character.Picture = ImageUtils.LoadSpriteFromPath(Path.Combine(characterPath, "thumb.png"));
			character.Description = from.Description;
			character.StartingTokens = from.StartingTokens;
			character.ForceAllAgentWeapons = from.ForceAllAgentWeapons;
			character.UsesPurchasePriceIncrement = from.UsesPurchasePriceIncrement;
			character.RequireSightTable = LegacyEquipmentGroupConverter.ConvertObjectTableFromLegacy(from.RequireSightTable);
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
			if (from.PrimaryWeapon != null) character.Weapon_Primary = LoadoutEntryConverter.ConvertLoadoutEntryFromVanilla(from.Weapon_Primary);
			if (from.SecondaryWeapon != null) character.Weapon_Secondary = LoadoutEntryConverter.ConvertLoadoutEntryFromVanilla(from.Weapon_Secondary);
			if (from.TertiaryWeapon != null) character.Weapon_Tertiary = LoadoutEntryConverter.ConvertLoadoutEntryFromVanilla(from.Weapon_Tertiary);
			if (from.PrimaryItem != null) character.Item_Primary = LoadoutEntryConverter.ConvertLoadoutEntryFromVanilla(from.Item_Primary);
			if (from.SecondaryItem != null) character.Item_Secondary = LoadoutEntryConverter.ConvertLoadoutEntryFromVanilla(from.Item_Secondary);
			if (from.TertiaryItem != null) character.Item_Tertiary = LoadoutEntryConverter.ConvertLoadoutEntryFromVanilla(from.Item_Tertiary);
			if (from.Shield != null) character.Item_Shield = LoadoutEntryConverter.ConvertLoadoutEntryFromVanilla(from.Item_Shield);

			return character;
		}
	}
}
