using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TNHTweaker.Objects.CharacterData
{
    public class TakeChallenge : ScriptableObject
    {
		public List<SosigEnemyID> SosigEnemyIDs;

		public TNH_TurretType TurretType;

		public int IFFUsed = 1;

		public int NumTurrets;

		public int NumGuards;
	}
}
