using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TNHTweaker.Objects.CharacterData
{
	/// <summary>
	/// The TakeChallenge determines settings of the hold point defenses during "take phase" that can spawn
	/// </summary>
	[CreateAssetMenu(menuName = "TNHTweaker/TakeChallenge", fileName = "NewTakeChallenge")]
	public class TakeChallenge : ScriptableObject
    {
		/// <summary> The possible sosig enemies that can spawn from this challenge </summary>
		public List<SosigEnemyID> SosigEnemyIDs = new List<SosigEnemyID>();
		/// <summary> The type of defense turret that can spawn from this challenge </summary>
		public TNH_TurretType TurretType;
		/// <summary> The IFF that the spawned defenders of this challenge uses </summary>
		public int IFFUsed = 1;
		/// <summary> The number of turrets that can spawn at this challenge </summary>
		public int NumTurrets = 0;
		/// <summary> The number of sosig guards that can spawn at this challenge </summary>
		public int NumGuards = 2;
	}
}
