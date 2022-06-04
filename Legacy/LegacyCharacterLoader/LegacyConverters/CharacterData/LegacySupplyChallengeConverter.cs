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
	public static class LegacySupplyChallengeConverter
	{
		public static SupplyChallenge ConvertSupplyChallengeFromLegacy(LegacyLevel from)
		{
			LogConversionStart(from);
			SupplyChallenge supplyChallenge = ScriptableObject.CreateInstance<SupplyChallenge>();

			supplyChallenge.SosigEnemyIDs.Add(Utilities.LegacyCharacterUtils.GetUniqueSosigIDValue(from.SupplyChallenge.EnemyType));
			supplyChallenge.TurretType = from.SupplyChallenge.TurretType;
			supplyChallenge.IFFUsed = from.SupplyChallenge.IFFUsed;
			supplyChallenge.NumTurrets = from.SupplyChallenge.NumTurrets;
			supplyChallenge.NumGuards = from.SupplyChallenge.NumGuards;
			supplyChallenge.MinConstructors = from.MinConstructors;
			supplyChallenge.MaxConstructors = from.MaxConstructors;
			supplyChallenge.MinPanels = from.MinPanels;
			supplyChallenge.MaxPanels = from.MaxPanels;
			supplyChallenge.MinBoxesSpawned = from.MinBoxesSpawned;
			supplyChallenge.MaxBoxesSpawned = from.MaxBoxesSpawned;
			supplyChallenge.MinTokensPerSupply = from.MinTokensPerSupply;
			supplyChallenge.MaxTokensPerSupply = from.MaxTokensPerSupply;
			supplyChallenge.BoxTokenChance = from.BoxTokenChance;
			supplyChallenge.BoxHealthChance = from.BoxHealthChance;

			LogConversionEnd(supplyChallenge);
			return supplyChallenge;
		}

		private static void LogConversionStart(LegacyLevel from)
		{
			LegacyLogger.Log($"- Starting conversion of legacy supply challenge -", LegacyLogger.LogType.Loading);
		}

		private static void LogConversionEnd(SupplyChallenge to)
		{
			LegacyLogger.Log($"- Finished conversion of legacy supply challenge -", LegacyLogger.LogType.Loading);
		}
	}
}
