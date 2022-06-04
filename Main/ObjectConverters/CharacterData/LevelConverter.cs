using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.Objects.CharacterData;
using TNHTweaker.Objects.LootPools;
using UnityEngine;

namespace TNHTweaker.ObjectConverters
{
	public static class LevelConverter
	{
		public static Level ConvertLevelFromVanilla(TNH_Progression.Level from)
		{
			Level level = ScriptableObject.CreateInstance<Level>();

			level.NumOverrideTokensForHold = from.NumOverrideTokensForHold;
			level.TakeChallenge = TakeChallengeConverter.ConvertTakeChallengeFromVanilla(from.TakeChallenge);
			level.SupplyChallenge = SupplyChallengeConverter.ConvertSupplyChallengeFromVanilla(from.SupplyChallenge);
			level.Patrols = from.PatrolChallenge.Patrols.Select(o => PatrolConverter.ConvertPatrolFromVanilla(o)).ToList();
			level.HoldPhases = from.HoldChallenge.Phases.Select(o => HoldPhaseConverter.ConvertHoldPhaseFromVanilla(o)).ToList();

			return level;
		}


		public static TNH_Progression.Level ConvertLevelToVanilla(Level from)
		{
			TNH_Progression.Level level = new TNH_Progression.Level();

			level.NumOverrideTokensForHold = from.NumOverrideTokensForHold;
			level.TakeChallenge = TakeChallengeConverter.ConvertTakeChallengeToVanilla(from.TakeChallenge);
			level.SupplyChallenge = SupplyChallengeConverter.ConvertSupplyChallengeToVanilla(from.SupplyChallenge);
			level.PatrolChallenge = ScriptableObject.CreateInstance<TNH_PatrolChallenge>();
			level.PatrolChallenge.Patrols = from.Patrols.Select(o => PatrolConverter.ConvertPatrolToVanilla(o)).ToList();
			level.HoldChallenge = ScriptableObject.CreateInstance<TNH_HoldChallenge>();
			level.HoldChallenge.Phases = from.HoldPhases.Select(o => HoldPhaseConverter.ConvertHoldPhaseToVanilla(o)).ToList();

			return level;
		}
	}
}
