using LegacyCharacterLoader.Objects.LootPools;
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
			LoadoutEntry loadoutEntry = ScriptableObject.CreateInstance<LoadoutEntry>();

			loadoutEntry.EquipmentGroups.Add(LegacyEquipmentGroupConverter.ConvertEquipmentGroupFromLegacy(from.PrimaryGroup));
			loadoutEntry.EquipmentGroups.Add(LegacyEquipmentGroupConverter.ConvertEquipmentGroupFromLegacy(from.BackupGroup));

			return loadoutEntry;
		}
	}
}
