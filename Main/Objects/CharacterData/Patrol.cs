using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TNHTweaker.Objects.CharacterData
{
    public class Patrol
    {
		public List<SosigEnemyID> EnemyTypes = new List<SosigEnemyID>();

		public SosigEnemyID LeaderType;

		public int PatrolSize;

		public int MaxPatrols;

		public int MaxPatrols_LimitedAmmo;

		public float TimeTilRegen;

		public float TimeTilRegen_LimitedAmmo;

		public int IFFUsed = 1;
	}
}
