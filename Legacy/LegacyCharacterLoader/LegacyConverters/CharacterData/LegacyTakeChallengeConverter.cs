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
    public static class LegacyTakeChallengeConverter
    {
		public static TakeChallenge ConvertTakeChallengeFromLegacy(LegacyTakeChallenge from)
		{
			LogConversionStart(from);
			TakeChallenge takeChallenge = ScriptableObject.CreateInstance<TakeChallenge>();

			takeChallenge.SosigEnemyIDs.Add(Utilities.LegacyCharacterUtils.GetUniqueSosigIDValue(from.EnemyType));
			takeChallenge.TurretType = from.TurretType;
			takeChallenge.IFFUsed = from.IFFUsed;
			takeChallenge.NumTurrets = from.NumTurrets;
			takeChallenge.NumGuards = from.NumGuards;

			LogConversionEnd(takeChallenge);
			return takeChallenge;
		}

		private static void LogConversionStart(LegacyTakeChallenge from)
		{
			LegacyLogger.Log($"- Starting conversion of legacy take challenge -", LegacyLogger.LogType.Loading);
		}

		private static void LogConversionEnd(TakeChallenge to)
		{
			LegacyLogger.Log($"- Finished conversion of legacy take challenge -", LegacyLogger.LogType.Loading);
		}
	}
}
