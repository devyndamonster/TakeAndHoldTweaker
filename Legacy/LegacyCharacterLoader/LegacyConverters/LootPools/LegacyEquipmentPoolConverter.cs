using Deli.VFS;
using LegacyCharacterLoader.Objects.LootPools;
using LegacyCharacterLoader.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TNHTweaker.Objects.LootPools;
using TNHTweaker.Utilities;
using UnityEngine;

namespace LegacyCharacterLoader.LegacyConverters
{
    public static class LegacyEquipmentPoolConverter
    {
		public static EquipmentPool ConvertEquipmentPoolFromLegacy(LegacyEquipmentPool from, IDirectoryHandle dir)
		{
			LogConversionStart(from);
			EquipmentPool equipmentPool = ScriptableObject.CreateInstance<EquipmentPool>();

			equipmentPool.Type = from.Type;
			equipmentPool.Icon = LegacyImageUtils.LoadSpriteFromFileHandle(dir.GetFile(from.IconName));
			equipmentPool.TokenCost = from.TokenCost;
			equipmentPool.TokenCostLimited = from.TokenCostLimited;
			equipmentPool.MinLevelAppears = from.MinLevelAppears;
			equipmentPool.MaxLevelAppears = from.MaxLevelAppears;
			equipmentPool.SpawnsInSmallCase = from.SpawnsInSmallCase;
			equipmentPool.SpawnsInLargeCase = from.SpawnsInLargeCase;
			equipmentPool.EquipmentGroup = LegacyEquipmentGroupConverter.ConvertEquipmentGroupFromLegacy(from.PrimaryGroup);

			LogConversionEnd(equipmentPool);
			return equipmentPool;
		}

		private static void LogConversionStart(LegacyEquipmentPool from)
		{
			LegacyLogger.Log($"- Starting conversion of legacy equipment pool -", LegacyLogger.LogType.Loading);
		}

		private static void LogConversionEnd(EquipmentPool to)
		{
			LegacyLogger.Log($"- Finished conversion of legacy equipment pool -", LegacyLogger.LogType.Loading);
		}
	}
}
