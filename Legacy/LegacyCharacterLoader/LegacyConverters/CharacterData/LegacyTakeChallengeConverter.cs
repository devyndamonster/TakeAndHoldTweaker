using LegacyCharacterLoader.Objects.CharacterData;
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
			TakeChallenge takeChallenge = ScriptableObject.CreateInstance<TakeChallenge>();

			takeChallenge.SosigEnemyIDs.Add(Utilities.LegacyCharacterUtils.GetUniqueSosigIDValue(from.EnemyType));
			takeChallenge.TurretType = from.TurretType;
			takeChallenge.IFFUsed = from.IFFUsed;
			takeChallenge.NumTurrets = from.NumTurrets;
			takeChallenge.NumGuards = from.NumGuards;

			return takeChallenge;
		}
	}
}
