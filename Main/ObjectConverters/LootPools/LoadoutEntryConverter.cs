using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.Objects.CharacterData;
using TNHTweaker.Objects.LootPools;
using TNHTweaker.Utilities;
using UnityEngine;

namespace TNHTweaker.ObjectConverters
{
	public static class LoadoutEntryConverter
	{
		public static LoadoutEntry ConvertLoadoutEntryFromVanilla(TNH_CharacterDef.LoadoutEntry from)
		{
			LoadoutEntry loadoutEntry = ScriptableObject.CreateInstance<LoadoutEntry>();

			if (from.ListOverride != null && from.ListOverride.Count > 0)
            {
				EquipmentGroup equipmentGroup = ScriptableObject.CreateInstance<EquipmentGroup>();
				equipmentGroup.ObjectTable = ScriptableObject.CreateInstance<Objects.LootPools.ObjectTable>();
				equipmentGroup.ObjectTable.WhitelistedObjectIDs = from.ListOverride.Select(o => o.ItemID).ToList();

				loadoutEntry.EquipmentGroups.Add(equipmentGroup);
            }

            else
            {
				foreach(ObjectTableDef table in from.TableDefs)
                {
					EquipmentGroup equipmentGroup = ScriptableObject.CreateInstance<EquipmentGroup>();
					equipmentGroup.NumMagsSpawned = from.Num_Mags_SL_Clips;
					equipmentGroup.NumClipsSpawned = from.Num_Mags_SL_Clips;
					equipmentGroup.NumRoundsSpawned = from.Num_Rounds;
					equipmentGroup.ObjectTable = ObjectTableConverter.ConvertObjectTableFromVanilla(table);

					loadoutEntry.EquipmentGroups.Add(equipmentGroup);
				}
			}

			return loadoutEntry;
		}


		public static TNH_CharacterDef.LoadoutEntry ConvertLoadoutEntryToVanilla(LoadoutEntry from)
		{
			TNH_CharacterDef.LoadoutEntry loadoutEntry = new TNH_CharacterDef.LoadoutEntry();
			LogConversionStart(loadoutEntry);

			loadoutEntry.TableDefs = from.EquipmentGroups.Select(o => ObjectTableConverter.ConvertObjectTableToVanilla(o.ObjectTable)).ToList();
			loadoutEntry.ListOverride = new List<FVRObject>();

			if(from.EquipmentGroups.Count > 0)
            {
				loadoutEntry.Num_Mags_SL_Clips = from.EquipmentGroups[0].NumMagsSpawned;
				loadoutEntry.Num_Rounds = from.EquipmentGroups[0].NumRoundsSpawned;
			}

			LogConversionEnd(loadoutEntry);
			return loadoutEntry;
		}

		private static void LogConversionStart(TNH_CharacterDef.LoadoutEntry loadoutEntry)
		{
			TNHTweakerLogger.Log("- Starting conversion of loadout entry to vanilla -", TNHTweakerLogger.LogType.Loading);
		}

		private static void LogConversionEnd(TNH_CharacterDef.LoadoutEntry loadoutEntry)
        {
			TNHTweakerLogger.Log("Num_Mags_SL_Clips : " + loadoutEntry.Num_Mags_SL_Clips, TNHTweakerLogger.LogType.Loading);
			TNHTweakerLogger.Log("Num_Rounds : " + loadoutEntry.Num_Rounds, TNHTweakerLogger.LogType.Loading);
			TNHTweakerLogger.Log("- Successfully converted loadout entry to vanilla -", TNHTweakerLogger.LogType.Loading);
		}

	}
}
