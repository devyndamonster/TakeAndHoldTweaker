using LegacyCharacterLoader.Objects.CharacterData;
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
			Level level = ScriptableObject.CreateInstance<Level>();

			level.NumOverrideTokensForHold = from.NumOverrideTokensForHold;
			level.TakeChallenge = LegacyTakeChallengeConverter.ConvertTakeChallengeFromLegacy(from.TakeChallenge);
			level.SupplyChallenge = LegacyTakeChallengeConverter.ConvertTakeChallengeFromLegacy(from.SupplyChallenge);
			level.Patrols = from.Patrols.Select(o => LegacyPatrolConverter.ConvertPatrolFromLegacy(o)).ToList();
			level.HoldPhases = from.HoldPhases.Select(o => LegacyHoldPhaseConverter.ConvertHoldPhaseFromLegacy(o)).ToList();

			return level;
		}
	}
}
