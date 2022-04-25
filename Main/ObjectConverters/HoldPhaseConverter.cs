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
	public static class HoldPhaseConverter
	{
		public static HoldPhase ConvertHoldPhaseFromVanilla(TNH_HoldChallenge.Phase from)
		{
			HoldPhase holdPhase = ScriptableObject.CreateInstance<HoldPhase>();

			holdPhase.Encryptions.Add(from.Encryption);
			holdPhase.MinTargets = from.MinTargets;
			holdPhase.MaxTargets = from.MaxTargets;
			holdPhase.EnemyTypes.Add(from.EType);
			holdPhase.LeaderType = from.LType;
			holdPhase.MinEnemies = from.MinEnemies;
			holdPhase.MaxEnemies = from.MaxEnemies;
			holdPhase.SpawnCadence = from.SpawnCadence;
			holdPhase.MaxEnemiesAlive = from.MaxEnemiesAlive;
			holdPhase.MaxDirections = from.MaxDirections;
			holdPhase.ScanTime = from.ScanTime;
			holdPhase.WarmUp = from.WarmUp;
			holdPhase.IFFUsed = from.IFFUsed;

			return holdPhase;
		}


		public static TNH_HoldChallenge.Phase ConvertHoldPhaseToVanilla(HoldPhase from)
		{
			TNH_HoldChallenge.Phase holdPhase = new TNH_HoldChallenge.Phase();

			holdPhase.Encryption = from.Encryptions.FirstOrDefault();
			holdPhase.MinTargets = from.MinTargets;
			holdPhase.MaxTargets = from.MaxTargets;
			holdPhase.EType = from.EnemyTypes.FirstOrDefault();
			holdPhase.LType = from.LeaderType;
			holdPhase.MinEnemies = from.MinEnemies;
			holdPhase.MaxEnemies = from.MaxEnemies;
			holdPhase.SpawnCadence = from.SpawnCadence;
			holdPhase.MaxEnemiesAlive = from.MaxEnemiesAlive;
			holdPhase.MaxDirections = from.MaxDirections;
			holdPhase.ScanTime = from.ScanTime;
			holdPhase.WarmUp = from.WarmUp;
			holdPhase.IFFUsed = from.IFFUsed;

			return holdPhase;
		}
	}
}
