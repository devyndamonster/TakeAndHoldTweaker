using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.Objects.CharacterData;
using TNHTweaker.Objects.LootPools;
using UnityEngine;

namespace TNHTweaker.ObjectConverters
{
	public static class EquipmentPoolConverter
	{
		public static EquipmentPool ConvertEquipmentPoolEntryFromVanilla(EquipmentPoolDef.PoolEntry from)
		{
			EquipmentPool equipmentPool = ScriptableObject.CreateInstance<EquipmentPool>();

			equipmentPool.Type = from.Type;
			equipmentPool.Icon = from.TableDef.Icon;
			equipmentPool.TokenCost = from.TokenCost;
			equipmentPool.TokenCostLimited = from.TokenCost_Limited;
			equipmentPool.MinLevelAppears = from.MinLevelAppears;
			equipmentPool.MaxLevelAppears = from.MaxLevelAppears;
			equipmentPool.SpawnsInSmallCase = from.TableDef.SpawnsInSmallCase;
			equipmentPool.SpawnsInLargeCase = from.TableDef.SpawnsInLargeCase;

			equipmentPool.EquipmentGroup = ScriptableObject.CreateInstance<EquipmentGroup>();
			equipmentPool.EquipmentGroup.Rarity = from.Rarity;
			equipmentPool.EquipmentGroup.ObjectTable = ObjectTableConverter.ConvertObjectTableFromVanilla(from.TableDef);

			return equipmentPool;
		}


		public static EquipmentPoolDef.PoolEntry ConvertEquipmentPoolEntryToVanilla(EquipmentPool from)
		{
			EquipmentPoolDef.PoolEntry equipmentPoolEntry = new EquipmentPoolDef.PoolEntry();

			equipmentPoolEntry.Type = from.Type;
			equipmentPoolEntry.TokenCost = from.TokenCost;
			equipmentPoolEntry.TokenCost_Limited = from.TokenCostLimited;
			equipmentPoolEntry.MinLevelAppears = from.MinLevelAppears;
			equipmentPoolEntry.MaxLevelAppears = from.MaxLevelAppears;
			equipmentPoolEntry.Rarity = from.EquipmentGroup.Rarity;
			equipmentPoolEntry.TableDef = ObjectTableConverter.ConvertObjectTableToVanilla(from);

			return equipmentPoolEntry;
		}
	}
}
