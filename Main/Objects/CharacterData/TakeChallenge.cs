using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TNHTweaker.Objects.CharacterData
{

	[CreateAssetMenu(menuName = "TNHTweaker/TakeChallenge", fileName = "NewTakeChallenge")]
	public class TakeChallenge : ScriptableObject
    {
		public List<SosigEnemyID> SosigEnemyIDs = new List<SosigEnemyID>();

		public TNH_TurretType TurretType;

		public int IFFUsed = 1;

		public int NumTurrets;

		public int NumGuards;
	}
}
