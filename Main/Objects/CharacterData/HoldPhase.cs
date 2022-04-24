using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TNHTweaker.Objects.CharacterData
{
    public class HoldPhase : ScriptableObject
    {
		public TNH_EncryptionType Encryptions;

		public int MinTargets;

		public int MaxTargets;

		public SosigEnemyID EnemyType;

		public SosigEnemyID LeaderType;

		public int MinEnemies = 3;

		public int MaxEnemies = 4;

		public float SpawnCadence = 20;

		public int MaxEnemiesAlive = 6;

		public int MaxDirections = 2;

		public float ScanTime = 25;

		public float WarmUp = 7;

		public int IFFUsed = 1;

	}
}
