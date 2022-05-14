using LegacyCharacterLoader.Objects.LootPools;
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
		public static EquipmentPool ConvertEquipmentPoolFromLegacy(LegacyEquipmentPool from, string characterPath)
		{
			EquipmentPool equipmentPool = ScriptableObject.CreateInstance<EquipmentPool>();

			equipmentPool.Type = from.Type;
			equipmentPool.Icon = ImageUtils.LoadSpriteFromPath(Path.Combine(characterPath, from.IconName));
			equipmentPool.TokenCost = from.TokenCost;
			equipmentPool.TokenCostLimited = from.TokenCostLimited;
			equipmentPool.MinLevelAppears = from.MinLevelAppears;
			equipmentPool.MaxLevelAppears = from.MaxLevelAppears;
			equipmentPool.SpawnsInSmallCase = from.SpawnsInSmallCase;
			equipmentPool.SpawnsInLargeCase = from.SpawnsInLargeCase;
			equipmentPool.EquipmentGroup = LegacyEquipmentGroupConverter.ConvertEquipmentGroupFromLegacy(from.PrimaryGroup);

			return equipmentPool;
		}
	}
}
