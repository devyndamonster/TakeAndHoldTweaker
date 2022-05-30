using FistVR;
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
    public static class LegacyEquipmentGroupConverter
    {
		public static EquipmentGroup ConvertEquipmentGroupFromLegacy(LegacyEquipmentGroup from)
		{
			LogConversionStart(from);
            TNHTweaker.Objects.LootPools.ObjectTable objectTable = ScriptableObject.CreateInstance<TNHTweaker.Objects.LootPools.ObjectTable>();

			objectTable.Category = (FVRObject.ObjectCategory)from.Category;
			objectTable.Eras = from.Eras.Select(o => (FVRObject.OTagEra)o).ToList();
			objectTable.Sets = from.Sets.Select(o => (FVRObject.OTagSet)o).ToList();
			objectTable.Sizes = from.Sizes.Select(o => (FVRObject.OTagFirearmSize)o).ToList();
			objectTable.Actions = from.Actions.Select(o => (FVRObject.OTagFirearmAction)o).ToList();
			objectTable.Modes = from.Modes.Select(o => (FVRObject.OTagFirearmFiringMode)o).ToList();
			objectTable.ExcludedModes = from.ExcludedModes.Select(o => (FVRObject.OTagFirearmFiringMode)o).ToList();
			objectTable.FeedOptions = from.FeedOptions.Select(o => (FVRObject.OTagFirearmFeedOption)o).ToList();
			objectTable.MountsAvailable = from.MountsAvailable.Select(o => (FVRObject.OTagFirearmMount)o).ToList();
			objectTable.RoundPowers = from.RoundPowers.Select(o => (FVRObject.OTagFirearmRoundPower)o).ToList();
			objectTable.Features = from.Features.Select(o => (FVRObject.OTagAttachmentFeature)o).ToList();
			objectTable.MountTypes = from.MountTypes.Select(o => (FVRObject.OTagFirearmMount)o).ToList();
			objectTable.MeleeStyles = from.MeleeStyles.Select(o => (FVRObject.OTagMeleeStyle)o).ToList();
			objectTable.MeleeHandedness = from.MeleeHandedness.Select(o => (FVRObject.OTagMeleeHandedness)o).ToList();
			objectTable.ThrownTypes = from.ThrownTypes.Select(o => (FVRObject.OTagThrownType)o).ToList();
			objectTable.ThrownDamageTypes = from.ThrownDamageTypes.Select(o => (FVRObject.OTagThrownDamageType)o).ToList();
			objectTable.PowerupTypes = from.PowerupTypes.Select(o => (FVRObject.OTagPowerupType)o).ToList();
			objectTable.MinAmmoCapacity = from.MinAmmoCapacity;
			objectTable.MaxAmmoCapacity = from.MaxAmmoCapacity;
			objectTable.WhitelistedObjectIDs = from.IDOverride;
			objectTable.AutoPopulatePools = from.AutoPopulateGroup;

			EquipmentGroup equipmentGroup = ScriptableObject.CreateInstance<EquipmentGroup>();

			equipmentGroup.ObjectTable = objectTable;
			equipmentGroup.Rarity = from.Rarity;
			equipmentGroup.ItemsToSpawn = from.ItemsToSpawn;
			equipmentGroup.NumMagsSpawned = from.NumMagsSpawned;
			equipmentGroup.NumClipsSpawned = from.NumClipsSpawned;
			equipmentGroup.NumRoundsSpawned = from.NumRoundsSpawned;
			equipmentGroup.SpawnMagAndClip = from.SpawnMagAndClip;
			equipmentGroup.BespokeAttachmentChance = from.BespokeAttachmentChance;
			equipmentGroup.ForceSpawnAllSubGroups = from.ForceSpawnAllSubPools;

			if(from.SubGroups != null)
            {
				equipmentGroup.SubGroups = from.SubGroups.Select(o => ConvertEquipmentGroupFromLegacy(o)).ToList();
			}
			
			LogConversionEnd(equipmentGroup);
			return equipmentGroup;
		}

		private static void LogConversionStart(LegacyEquipmentGroup from)
		{
			LegacyLogger.Log($"- Starting conversion of legacy equipment group -", LegacyLogger.LogType.Loading);
		}

		private static void LogConversionEnd(EquipmentGroup to)
		{
			LegacyLogger.Log($"- Finished conversion of legacy equipment group -", LegacyLogger.LogType.Loading);
		}
	}
}
