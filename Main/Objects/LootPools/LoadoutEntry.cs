using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TNHTweaker.Objects.LootPools
{
    public class LoadoutEntry : ScriptableObject
    {
        public List<EquipmentGroup> EquipmentGroups = new List<EquipmentGroup>();
    }
}
