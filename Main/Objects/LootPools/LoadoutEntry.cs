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
            List<EquipmentGroup> validGroups = EquipmentGroups.Where(o => o.CanSpawnFromPool()).ToList();
            float totalChanceRange = validGroups.Sum(o => o.Rarity);
            float randomValueSelection = UnityEngine.Random.Range(0, totalChanceRange);
            float summedChanceValue = 0;

            foreach(EquipmentGroup group in validGroups)
            {
                if(summedChanceValue >= randomValueSelection)
                {
                    return group;
                }
                summedChanceValue += group.Rarity;
            }

            return validGroups.LastOrDefault();
        }

        public override string ToString()
        {
            return  $"\nLoadoutEntry - " +
                    $"\nNum EquipmentGroups = {EquipmentGroups.Count}" +
                    $"\nGroups:\n{string.Join("\n\n", EquipmentGroups.Select(o => o.ToString()).ToArray())}";
        }
    }
}
