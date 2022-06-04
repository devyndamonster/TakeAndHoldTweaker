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
	public static class SupplyChallengeConverter
	{
		public static SupplyChallenge ConvertSupplyChallengeFromVanilla(TNH_TakeChallenge from)
		{
			SupplyChallenge supplyChallenge = ScriptableObject.CreateInstance<SupplyChallenge>();

			supplyChallenge.SosigEnemyIDs.Add(from.GID);
			supplyChallenge.TurretType = from.TurretType;
			supplyChallenge.IFFUsed = from.IFFUsed;
			supplyChallenge.NumTurrets = from.NumTurrets;
			supplyChallenge.NumGuards = from.NumGuards;

			return supplyChallenge;
		}


		public static TNH_TakeChallenge ConvertSupplyChallengeToVanilla(SupplyChallenge from)
		{
			return TakeChallengeConverter.ConvertTakeChallengeToVanilla(from);
		}
	}
}
