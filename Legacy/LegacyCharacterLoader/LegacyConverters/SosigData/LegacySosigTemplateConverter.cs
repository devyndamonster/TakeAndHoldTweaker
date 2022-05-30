﻿using FistVR;
using LegacyCharacterLoader.Objects.SosigData;
using LegacyCharacterLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.Objects.SosigData;
using UnityEngine;

namespace LegacyCharacterLoader.LegacyConverters
{
    public static class LegacySosigTemplateConverter
    {
		public static SosigTemplate ConvertSosigTemplateFromLegacy(LegacySosigTemplate from)
		{
			LogConversionStart(from);
			SosigTemplate sosigTemplate = ScriptableObject.CreateInstance<SosigTemplate>();

			sosigTemplate.DisplayName = from.DisplayName;
			sosigTemplate.SosigEnemyCategory = from.SosigEnemyCategory;
			sosigTemplate.SosigEnemyID = Utilities.LegacyCharacterUtils.GetUniqueSosigIDValue(from.SosigEnemyID);
			sosigTemplate.SosigPrefabs = from.SosigPrefabs;
			sosigTemplate.Configs = from.Configs.Select(o => LegacySosigConfigConverter.ConvertSosigConfigFromLegacy(o)).ToList();
			sosigTemplate.ConfigsEasy = from.ConfigsEasy.Select(o => LegacySosigConfigConverter.ConvertSosigConfigFromLegacy(o)).ToList();
			sosigTemplate.OutfitConfigs = from.OutfitConfigs.Select(o => LegacyOutfitConfigConverter.ConvertOutfitConfigFromLegacy(o)).ToList();
			sosigTemplate.WeaponOptions = from.WeaponOptions;
			sosigTemplate.WeaponOptionsSecondary = from.WeaponOptionsSecondary;
			sosigTemplate.WeaponOptionsTertiary = from.WeaponOptionsTertiary;
			sosigTemplate.SecondaryChance = from.SecondaryChance;
			sosigTemplate.TertiaryChance = from.TertiaryChance;
			sosigTemplate.DroppedLootChance = from.DroppedLootChance;
			sosigTemplate.DroppedLootPool = LegacyEquipmentGroupConverter.ConvertEquipmentGroupFromLegacy(from.DroppedObjectPool);

			LogConversionEnd(sosigTemplate);
			return sosigTemplate;
		}

		private static void LogConversionStart(LegacySosigTemplate from)
		{
			LegacyLogger.Log($"- Starting conversion of legacy sosig template -", LegacyLogger.LogType.Loading);
		}

		private static void LogConversionEnd(SosigTemplate to)
		{
			LegacyLogger.Log($"- Finished conversion of legacy sosig template -", LegacyLogger.LogType.Loading);
		}
	}
}