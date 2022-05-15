using LegacyCharacterLoader.Objects.LootPools;
using LegacyCharacterLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.Objects.LootPools;
using UnityEngine;

namespace LegacyCharacterLoader.LegacyConverters
{
    public static class LegacyLoadoutEntryConverter
    {
		public static LoadoutEntry ConvertLoadoutEntryFromLegacy(LegacyLoadoutEntry from)
		{
			LogConversionStart(from);
			LoadoutEntry loadoutEntry = ScriptableObject.CreateInstance<LoadoutEntry>();

			loadoutEntry.EquipmentGroups.Add(LegacyEquipmentGroupConverter.ConvertEquipmentGroupFromLegacy(from.PrimaryGroup));
			loadoutEntry.EquipmentGroups.Add(LegacyEquipmentGroupConverter.ConvertEquipmentGroupFromLegacy(from.BackupGroup));

			LogConversionEnd(loadoutEntry);
			return loadoutEntry;
		}

		private static void LogConversionStart(LegacyLoadoutEntry from)
		{
			LegacyLogger.Log($"- Starting conversion of legacy equipment pool -", LegacyLogger.LogType.Loading);
		}

		private static void LogConversionEnd(LoadoutEntry to)
		{
			LegacyLogger.Log($"- Finished conversion of legacy equipment pool -", LegacyLogger.LogType.Loading);
		}
	}
}
