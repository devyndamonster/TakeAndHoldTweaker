using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TNHTweaker.Objects.CharacterData
{
	/// <summary>
	/// The Patrol determines the spawning behaviour of a given patrol
	/// </summary>
	[CreateAssetMenu(menuName = "TNHTweaker/Patrol", fileName = "NewPatrol")]
	public class Patrol : ScriptableObject
    {
		/// <summary> The types of enemies that can spawn in this patrol </summary>
		public List<SosigEnemyID> EnemyTypes = new List<SosigEnemyID>();
		/// <summary> The leader enemy type that can spawn in this patrol </summary>
		/// <remarks> Only one of these enemies will spawn per patrol </remarks>
		public SosigEnemyID LeaderType;
		/// <summary> The number of sosigs that can spawn in a patrol </summary>
		public int PatrolSize;
		/// <summary> The maximum number of patrols that can be active at once </summary>
		public int MaxPatrols;
		/// <summary> The maximum number of patrols that can be active at once in limited ammo mode</summary>
		public int MaxPatrols_LimitedAmmo;
		/// <summary> The delay between patrol spawns </summary>
		public float TimeTilRegen;
		/// <summary> The delay between patrol spawns in limited ammo mode</summary>
		public float TimeTilRegen_LimitedAmmo;
		/// <summary> The IFF of the patrols sosigs </summary>
		public int IFFUsed = 1;
	}
}
