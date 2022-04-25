﻿using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TNHTweaker.Objects.CharacterData
{
	[CreateAssetMenu(menuName = "TNHTweaker/HoldPhase", fileName = "NewPhase")]
	public class HoldPhase : ScriptableObject
    {
		public List<TNH_EncryptionType> Encryptions = new List<TNH_EncryptionType>();

		public int MinTargets;

		public int MaxTargets;

		public List<SosigEnemyID> EnemyTypes = new List<SosigEnemyID>();

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
