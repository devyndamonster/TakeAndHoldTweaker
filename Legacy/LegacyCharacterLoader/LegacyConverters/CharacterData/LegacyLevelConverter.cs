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
    public static class LegacyLevelConverter
    {
		public static Level ConvertLevelFromLegacy(LegacyLevel from)
		{
			LogConversionStart(from);
			Level level = ScriptableObject.CreateInstance<Level>();

			level.NumOverrideTokensForHold = from.NumOverrideTokensForHold;
			level.TakeChallenge = LegacyTakeChallengeConverter.ConvertTakeChallengeFromLegacy(from.TakeChallenge);
			level.SupplyChallenge = LegacyTakeChallengeConverter.ConvertTakeChallengeFromLegacy(from.SupplyChallenge);
			level.Patrols = from.Patrols.Select(o => LegacyPatrolConverter.ConvertPatrolFromLegacy(o)).ToList();
			level.HoldPhases = from.HoldPhases.Select(o => LegacyHoldPhaseConverter.ConvertHoldPhaseFromLegacy(o)).ToList();

			LogConversionEnd(level);
			return level;
		}

		private static void LogConversionStart(LegacyLevel from)
		{
			LegacyLogger.Log($"- Starting conversion of legacy level -", LegacyLogger.LogType.Loading);
		}

		private static void LogConversionEnd(Level to)
		{
			LegacyLogger.Log($"- Finished conversion of legacy level -", LegacyLogger.LogType.Loading);
		}
	}
}
