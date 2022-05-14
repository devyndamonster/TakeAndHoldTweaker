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
	public static class TakeChallengeConverter
	{
		public static TakeChallenge ConvertTakeChallengeFromVanilla(TNH_TakeChallenge from)
		{
			TakeChallenge takeChallenge = ScriptableObject.CreateInstance<TakeChallenge>();

			takeChallenge.SosigEnemyIDs.Add(from.GID);
			takeChallenge.TurretType = from.TurretType;
			takeChallenge.IFFUsed = from.IFFUsed;
			takeChallenge.NumTurrets = from.NumTurrets;
			takeChallenge.NumGuards = from.NumGuards;

			return takeChallenge;
		}


		public static TNH_TakeChallenge ConvertTakeChallengeToVanilla(TakeChallenge from)
		{
			TNH_TakeChallenge takeChallenge = ScriptableObject.CreateInstance<TNH_TakeChallenge>();

			takeChallenge.GID = from.SosigEnemyIDs.FirstOrDefault();
			takeChallenge.TurretType = from.TurretType;
			takeChallenge.IFFUsed = from.IFFUsed;
			takeChallenge.NumTurrets = from.NumTurrets;
			takeChallenge.NumGuards = from.NumGuards;

			return takeChallenge;
		}
	}
}
