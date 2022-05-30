using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TNHTweaker.Objects.LootPools
{
    [CreateAssetMenu(menuName = "TNHTweaker/EquipmentPool", fileName = "NewEquipmentPool")]
    public class EquipmentPool : ScriptableObject
    {
        public EquipmentPoolDef.PoolEntry.PoolEntryType Type;
        public Sprite Icon;
        public int TokenCost;
        public int TokenCostLimited;
        public int MinLevelAppears;
        public int MaxLevelAppears;
        public bool SpawnsInSmallCase;
        public bool SpawnsInLargeCase;
        public EquipmentGroup EquipmentGroup;

        public void GenerateTables()
        {
            EquipmentGroup.GenerateTables();
        }
    }
}
