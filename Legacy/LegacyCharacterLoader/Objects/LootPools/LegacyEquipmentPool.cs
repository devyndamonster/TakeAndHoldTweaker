using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegacyCharacterLoader.Objects.LootPools
{
    public class LegacyEquipmentPool
    {
        public EquipmentPoolDef.PoolEntry.PoolEntryType Type;
        public string IconName;
        public int TokenCost;
        public int TokenCostLimited;
        public int MinLevelAppears;
        public int MaxLevelAppears;
        public bool SpawnsInSmallCase;
        public bool SpawnsInLargeCase;
        public LegacyEquipmentGroup PrimaryGroup;
        public LegacyEquipmentGroup BackupGroup;
    }
}
