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

        public void GenerateTables()
        {
            foreach(EquipmentGroup group in EquipmentGroups)
            {
                group.GenerateTables();
            }
        }

        public List<EquipmentGroup> GetStartingEquipmentGroups()
        {
            return SelectRandomEquipmentGroup().GetEquipmentGroupsToSpawnFrom();
        }

        private EquipmentGroup SelectRandomEquipmentGroup()
        {
            float totalChanceRange = EquipmentGroups.Sum(o => o.Rarity);
            float randomValueSelection = UnityEngine.Random.Range(0, totalChanceRange);
            float summedChanceValue = 0;

            foreach(EquipmentGroup group in EquipmentGroups)
            {
                if(summedChanceValue >= randomValueSelection)
                {
                    return group;
                }
                summedChanceValue += group.Rarity;
            }

            return EquipmentGroups.LastOrDefault();
        }
    }
}
