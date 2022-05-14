using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.Objects.CharacterData;
using TNHTweaker.Objects.LootPools;
using TNHTweaker.Objects.SosigData;
using UnityEngine;

namespace TNHTweaker.ObjectConverters
{
	public static class SosigTemplateConverter
	{
		public static SosigTemplate ConvertSosigTemplateFromVanilla(SosigEnemyTemplate from)
		{
			SosigTemplate sosigTemplate = ScriptableObject.CreateInstance<SosigTemplate>();

			sosigTemplate.DisplayName = from.DisplayName;
			sosigTemplate.SosigEnemyCategory = from.SosigEnemyCategory;
			sosigTemplate.SosigEnemyID = from.SosigEnemyID;
			sosigTemplate.SosigPrefabs = from.SosigPrefabs.Select(o => o.ItemID).ToList();
			sosigTemplate.Configs = from.ConfigTemplates;
			sosigTemplate.ConfigsEasy = from.ConfigTemplates_Easy;
			sosigTemplate.OutfitConfigs = from.OutfitConfig.Select(o => OutfitConfigConverter.ConvertOutfitConfigFromVanilla(o)).ToList();
			sosigTemplate.WeaponOptions = from.WeaponOptions.Select(o => o.ItemID).ToList();
			sosigTemplate.WeaponOptionsSecondary = from.WeaponOptions_Secondary.Select(o => o.ItemID).ToList();
			sosigTemplate.WeaponOptionsTertiary = from.WeaponOptions_Tertiary.Select(o => o.ItemID).ToList();
			sosigTemplate.SecondaryChance = from.SecondaryChance;
			sosigTemplate.TertiaryChance = from.TertiaryChance;
			sosigTemplate.DroppedLootChance = 0;
			sosigTemplate.DroppedLootPool = ScriptableObject.CreateInstance<EquipmentGroup>();

			return sosigTemplate;
		}


		public static SosigEnemyTemplate ConvertSosigTemplateToVanilla(SosigTemplate from)
		{
			SosigEnemyTemplate sosigTemplate = ScriptableObject.CreateInstance<SosigEnemyTemplate>();

			sosigTemplate.DisplayName = from.DisplayName;
			sosigTemplate.SosigEnemyCategory = from.SosigEnemyCategory;
			sosigTemplate.SosigEnemyID = from.SosigEnemyID;
			sosigTemplate.SosigPrefabs = from.SosigPrefabs.Select(o => IM.OD[o]).ToList();
			sosigTemplate.ConfigTemplates = from.Configs;
			sosigTemplate.ConfigTemplates_Easy = from.ConfigsEasy;
			sosigTemplate.OutfitConfig = from.OutfitConfigs.Select(o => OutfitConfigConverter.ConvertOutfitConfigToVanilla(o)).ToList();
			sosigTemplate.WeaponOptions = from.WeaponOptions.Select(o => IM.OD[o]).ToList();
			sosigTemplate.WeaponOptions_Secondary = from.WeaponOptionsSecondary.Select(o => IM.OD[o]).ToList();
			sosigTemplate.WeaponOptions_Tertiary = from.WeaponOptionsTertiary.Select(o => IM.OD[o]).ToList();
			sosigTemplate.SecondaryChance = from.SecondaryChance;
			sosigTemplate.TertiaryChance = from.TertiaryChance;

			return sosigTemplate;
		}
	}
}
