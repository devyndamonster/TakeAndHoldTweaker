using Deli.VFS;
using FistVR;
using LegacyCharacterLoader.Objects.CharacterData;
using LegacyCharacterLoader.Utilities;
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
		public static Character ConvertCharacterFromLegacy(LegacyCharacter from, IDirectoryHandle characterDirectory)
		{
			Character character = ScriptableObject.CreateInstance<Character>();

			character.DisplayName = from.DisplayName;
			character.CharacterID = Utilities.LegacyCharacterUtils.GetUniqueTNHCharValue(from.DisplayName);
			character.Group = CharacterUtils.GetGroupStringFromEnum((TNH_CharacterDef.CharacterGroup)from.CharacterGroup);
			character.TableID = from.TableID;
			character.Picture = LegacyImageUtils.LoadSpriteFromFileHandle(characterDirectory.GetFile("thumb.png"));
			character.Description = from.Description;
			character.StartingTokens = from.StartingTokens;
			character.ForceAllAgentWeapons = from.ForceAllAgentWeapons;
			character.UsesPurchasePriceIncrement = from.UsesPurchasePriceIncrement;
			character.RequireSightTable = LegacyEquipmentGroupConverter.ConvertEquipmentGroupFromLegacy(from.RequireSightTable);
			character.ValidAmmoEras = from.ValidAmmoEras.Select(o => (FVRObject.OTagEra)o).ToList();
			character.ValidAmmoSets = from.ValidAmmoSets.Select(o => (FVRObject.OTagSet)o).ToList();
			character.EquipmentPools = from.EquipmentPools.Select(o => LegacyEquipmentPoolConverter.ConvertEquipmentPoolFromLegacy(o, characterDirectory)).ToList();
			character.Progressions.Add(LegacyProgressionConverter.ConvertProgressionFromLegacy(from.Levels));
			character.Progressions_Endless.Add(LegacyProgressionConverter.ConvertProgressionFromLegacy(from.LevelsEndless));
			character.Has_Weapon_Primary = from.HasPrimaryWeapon;
			character.Has_Weapon_Secondary = from.HasSecondaryWeapon;
			character.Has_Weapon_Tertiary = from.HasTertiaryWeapon;
			character.Has_Item_Primary = from.HasPrimaryItem;
			character.Has_Item_Secondary = from.HasSecondaryItem;
			character.Has_Item_Tertiary = from.HasTertiaryItem;
			character.Has_Item_Shield = from.HasShield;
			if (from.PrimaryWeapon != null) character.Weapon_Primary = LegacyLoadoutEntryConverter.ConvertLoadoutEntryFromLegacy(from.PrimaryWeapon);
			if (from.SecondaryWeapon != null) character.Weapon_Secondary = LegacyLoadoutEntryConverter.ConvertLoadoutEntryFromLegacy(from.SecondaryWeapon);
			if (from.TertiaryWeapon != null) character.Weapon_Tertiary = LegacyLoadoutEntryConverter.ConvertLoadoutEntryFromLegacy(from.TertiaryWeapon);
			if (from.PrimaryItem != null) character.Item_Primary = LegacyLoadoutEntryConverter.ConvertLoadoutEntryFromLegacy(from.PrimaryItem);
			if (from.SecondaryItem != null) character.Item_Secondary = LegacyLoadoutEntryConverter.ConvertLoadoutEntryFromLegacy(from.SecondaryItem);
			if (from.TertiaryItem != null) character.Item_Tertiary = LegacyLoadoutEntryConverter.ConvertLoadoutEntryFromLegacy(from.TertiaryItem);
			if (from.Shield != null) character.Item_Shield = LegacyLoadoutEntryConverter.ConvertLoadoutEntryFromLegacy(from.Shield);

			return character;
		}
	}
}
