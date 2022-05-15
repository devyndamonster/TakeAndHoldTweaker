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
    public static class LegacyPatrolConverter
    {
		public static Patrol ConvertPatrolFromLegacy(LegacyPatrol from)
		{
			LogConversionStart(from);
			Patrol patrol = ScriptableObject.CreateInstance<Patrol>();

			patrol.EnemyTypes = from.EnemyType.Select(o => Utilities.LegacyCharacterUtils.GetUniqueSosigIDValue(o)).ToList();
			patrol.LeaderType = Utilities.LegacyCharacterUtils.GetUniqueSosigIDValue(from.LeaderType);
			patrol.PatrolSize = from.PatrolSize;
			patrol.MaxPatrols = from.MaxPatrols;
			patrol.MaxPatrols_LimitedAmmo = from.MaxPatrolsLimited;
			patrol.TimeTilRegen = from.PatrolCadence;
			patrol.TimeTilRegen_LimitedAmmo = from.PatrolCadenceLimited;
			patrol.IFFUsed = from.IFFUsed;

			LogConversionEnd(patrol);
			return patrol;
		}

		private static void LogConversionStart(LegacyPatrol from)
		{
			LegacyLogger.Log($"- Starting conversion of legacy patrol -", LegacyLogger.LogType.Loading);
		}

		private static void LogConversionEnd(Patrol to)
		{
			LegacyLogger.Log($"- Finished conversion of legacy patrol -", LegacyLogger.LogType.Loading);
		}
	}
}
