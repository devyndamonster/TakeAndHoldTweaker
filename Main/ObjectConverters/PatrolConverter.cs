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
	public static class PatrolConverter
	{
		public static Patrol ConvertPatrolFromVanilla(TNH_PatrolChallenge.Patrol from)
		{
			Patrol patrol = ScriptableObject.CreateInstance<Patrol>();

			patrol.EnemyTypes.Add(from.EType);
			patrol.LeaderType = from.LType;
			patrol.PatrolSize = from.PatrolSize;
			patrol.MaxPatrols = from.MaxPatrols;
			patrol.MaxPatrols_LimitedAmmo = from.MaxPatrols_LimitedAmmo;
			patrol.TimeTilRegen = from.TimeTilRegen;
			patrol.TimeTilRegen_LimitedAmmo = from.TimeTilRegen_LimitedAmmo;
			patrol.IFFUsed = from.IFFUsed;

			return patrol;
		}


		public static TNH_PatrolChallenge.Patrol ConvertPatrolToVanilla(Patrol from)
		{
			TNH_PatrolChallenge.Patrol patrol = new TNH_PatrolChallenge.Patrol();

			patrol.EType = from.EnemyTypes.FirstOrDefault();
			patrol.LType = from.LeaderType;
			patrol.PatrolSize = from.PatrolSize;
			patrol.MaxPatrols = from.MaxPatrols;
			patrol.MaxPatrols_LimitedAmmo = from.MaxPatrols_LimitedAmmo;
			patrol.TimeTilRegen = from.TimeTilRegen;
			patrol.TimeTilRegen_LimitedAmmo = from.TimeTilRegen_LimitedAmmo;
			patrol.IFFUsed = from.IFFUsed;

			return patrol;
		}
	}
}
