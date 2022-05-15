using LegacyCharacterLoader.Objects.CharacterData;
using LegacyCharacterLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.Objects.CharacterData;
using UnityEngine;

namespace LegacyCharacterLoader.LegacyConverters
{
    public static class LegacyHoldPhaseConverter
    {
		public static HoldPhase ConvertHoldPhaseFromLegacy(LegacyHoldPhase from)
		{
			LogConversionStart(from);
			HoldPhase holdPhase = ScriptableObject.CreateInstance<HoldPhase>();

			holdPhase.Encryptions = from.Encryptions;
			holdPhase.MinTargets = from.MinTargets;
			holdPhase.MaxTargets = from.MaxTargets;
			holdPhase.EnemyTypes = from.EnemyType.Select(o => Utilities.LegacyCharacterUtils.GetUniqueSosigIDValue(o)).ToList();
			holdPhase.LeaderType = Utilities.LegacyCharacterUtils.GetUniqueSosigIDValue(from.LeaderType);
			holdPhase.MinEnemies = from.MinEnemies;
			holdPhase.MaxEnemies = from.MaxEnemies;
			holdPhase.SpawnCadence = from.SpawnCadence;
			holdPhase.MaxEnemiesAlive = from.MaxEnemiesAlive;
			holdPhase.MaxDirections = from.MaxDirections;
			holdPhase.ScanTime = from.ScanTime;
			holdPhase.WarmUp = from.WarmupTime;
			holdPhase.IFFUsed = from.IFFUsed;

			LogConversionEnd(holdPhase);
			return holdPhase;
		}

		private static void LogConversionStart(LegacyHoldPhase from)
		{
			LegacyLogger.Log($"- Starting conversion of legacy hold phase -", LegacyLogger.LogType.Loading);
		}

		private static void LogConversionEnd(HoldPhase to)
		{
			LegacyLogger.Log($"- Finished conversion of legacy hold phase -", LegacyLogger.LogType.Loading);
		}
	}
}
