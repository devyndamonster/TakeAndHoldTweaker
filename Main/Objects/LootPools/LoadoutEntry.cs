using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TNHTweaker.Objects.LootPools
{
    [CreateAssetMenu(menuName = "TNHTweaker/LoadoutEntry", fileName = "NewLoadoutEntry")]
    public class LoadoutEntry : ScriptableObject
    {
        public List<EquipmentGroup> EquipmentGroups = new List<EquipmentGroup>();
    }
}
