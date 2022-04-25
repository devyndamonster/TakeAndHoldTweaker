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
	public static class ObjectTableConverter
	{
		public static Objects.LootPools.ObjectTable ConvertObjectTableFromVanilla(ObjectTableDef from)
		{
			Objects.LootPools.ObjectTable objectTable = ScriptableObject.CreateInstance<Objects.LootPools.ObjectTable>();

			objectTable.Category = from.Category;
			objectTable.Eras = from.Eras;
			objectTable.Sets = from.Sets;
			objectTable.Sizes = from.Sizes;
			objectTable.Actions = from.Actions;
			objectTable.Modes = from.Modes;
			objectTable.ExcludedModes = from.ExcludeModes;
			objectTable.FeedOptions = from.Feedoptions;
			objectTable.MountsAvailable = from.MountsAvailable;
			objectTable.RoundPowers = from.RoundPowers;
			objectTable.Features = from.Features;
			objectTable.MountTypes = from.MountTypes;
			objectTable.MeleeStyles = from.MeleeStyles;
			objectTable.MeleeHandedness = from.MeleeHandedness;
			objectTable.ThrownTypes = from.ThrownTypes;
			objectTable.ThrownDamageTypes = from.ThrownDamageTypes;
			objectTable.PowerupTypes = from.PowerupTypes;
			objectTable.MinAmmoCapacity = from.MinAmmoCapacity;
			objectTable.MaxAmmoCapacity = from.MaxAmmoCapacity;
			objectTable.WhitelistedObjectIDs = from.IDOverride;

			return objectTable;
		}


		public static ObjectTableDef ConvertObjectTableToVanilla(EquipmentPool from)
		{
			return ConvertObjectTableToVanilla(from.EquipmentGroup.ObjectTable, from.Icon, from.SpawnsInSmallCase, from.SpawnsInLargeCase);
		}


		public static ObjectTableDef ConvertObjectTableToVanilla(Objects.LootPools.ObjectTable from, Sprite icon = null, bool spawnsInSmallCase = false, bool spawnsInLargeCase = false)
		{
			ObjectTableDef objectTable = ScriptableObject.CreateInstance<ObjectTableDef>();
			LogConversionStart(objectTable);

			objectTable.Icon = icon;
			objectTable.SpawnsInSmallCase = spawnsInSmallCase;
			objectTable.SpawnsInLargeCase = spawnsInLargeCase;
			objectTable.Category = from.Category;
			objectTable.Eras = from.Eras;
			objectTable.Sets = from.Sets;
			objectTable.Sizes = from.Sizes;
			objectTable.Actions = from.Actions;
			objectTable.Modes = from.Modes;
			objectTable.ExcludeModes = from.ExcludedModes;
			objectTable.Feedoptions = from.FeedOptions;
			objectTable.MountsAvailable = from.MountsAvailable;
			objectTable.RoundPowers = from.RoundPowers;
			objectTable.Features = from.Features;
			objectTable.MountTypes = from.MountTypes;
			objectTable.MeleeStyles = from.MeleeStyles;
			objectTable.MeleeHandedness = from.MeleeHandedness;
			objectTable.ThrownTypes = from.ThrownTypes;
			objectTable.ThrownDamageTypes = from.ThrownDamageTypes;
			objectTable.PowerupTypes = from.PowerupTypes;
			objectTable.MinAmmoCapacity = from.MinAmmoCapacity;
			objectTable.MaxAmmoCapacity = from.MaxAmmoCapacity;
			objectTable.IDOverride = from.WhitelistedObjectIDs;
			objectTable.UseIDListOverride = from.WhitelistedObjectIDs.Count > 0;

			LogConversionEnd(objectTable);
			return objectTable;
		}

		private static void LogConversionStart(ObjectTableDef objectTable)
		{
			TNHTweakerLogger.Log("- Starting conversion of object table to vanilla -", TNHTweakerLogger.LogType.Loading);
		}

		private static void LogConversionEnd(ObjectTableDef objectTable)
		{
			TNHTweakerLogger.Log("SpawnsInSmallCase : " + objectTable.SpawnsInSmallCase, TNHTweakerLogger.LogType.Loading);
			TNHTweakerLogger.Log("SpawnsInLargeCase : " + objectTable.SpawnsInLargeCase, TNHTweakerLogger.LogType.Loading);
			TNHTweakerLogger.Log("- Successfully converted object table to vanilla -", TNHTweakerLogger.LogType.Loading);
		}
	}
}
