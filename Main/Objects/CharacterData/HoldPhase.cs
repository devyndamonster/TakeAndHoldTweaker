using FistVR;
using Sodalite.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TNHTweaker.Objects.CharacterData
{
	/// <summary>
	/// The HoldPhase determines what spawns during each hold phase
	/// </summary>
	[CreateAssetMenu(menuName = "TNHTweaker/HoldPhase", fileName = "NewPhase")]
	public class HoldPhase : ScriptableObject
    {
		/// <summary> The enryptions that will spawn during this phase </summary>
		/// <remarks> Encryptions are spawned in the order they are given, and types will repeat from beginning of list if more are spawned than what is given </remarks>
		public List<TNH_EncryptionType> Encryptions = new List<TNH_EncryptionType>();
		/// <summary> The minimum number of encryptions that can spawn during this hold phase </summary>
		public int MinTargets;
		/// <summary> The maximum number of encryptions that can spawn during this hold phase </summary>
		public int MaxTargets;
		/// <summary> The minimum number of encryptions that can spawn during this hold phase on limited ammo mode </summary>
		public int MinTargetsLimited;
		/// <summary> The maximum number of encryptions that can spawn during this hold phase on limited ammo mode </summary>
		public int MaxTargetsLimited;
		/// <summary> The types of enemies that can spawn each hold wave </summary>
		public List<SosigEnemyID> EnemyTypes = new List<SosigEnemyID>();
		/// <summary> The leader enemy type that can spawn each hold wave </summary>
		/// <remarks> Only one of these enemies will spawn per wave </remarks>
		public SosigEnemyID LeaderType;
		/// <summary> The minumum number of enemies that will spawn per enemy wave </summary>
		public int MinEnemies = 3;
		/// <summary> The maximum number of enemies that will spawn per enemy wave </summary>
		public int MaxEnemies = 4;
		/// <summary> The delay between enemy waves (in seconds) </summary>
		public float SpawnCadence = 20;
		/// <summary> The maximum number of enemies that can be alive during the hold at any time </summary>
		public int MaxEnemiesAlive = 6;
		/// <summary> The maximum number of directions enemies can attack from each enemy wave </summary>
		public int MaxDirections = 2;
		/// <summary> TODO document this </summary>
		public float ScanTime = 25;
		/// <summary> TODO document this </summary>
		public float WarmUp = 7;
		/// <summary> The IFF of the enemies that will spawn each wave </summary>
		public int IFFUsed = 1;

		public TNH_EncryptionType GetEncryptionFromIndex(int encryptionIndex)
        {
			return Encryptions[encryptionIndex % Encryptions.Count];
        }

		public int GetNumTargetsToSpawn(TNHSetting_EquipmentMode equipmentMode)
        {
			if (equipmentMode == TNHSetting_EquipmentMode.LimitedAmmo) 
			{
				return UnityEngine.Random.Range(MinTargetsLimited, MaxTargetsLimited + 1);
			}
			
			return UnityEngine.Random.Range(MinTargets, MaxTargets + 1);
		}

		public SosigEnemyID GetRandomEnemyId()
		{
			return EnemyTypes.GetRandom();
		}

	}

}
