using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.Objects.CharacterData;

namespace TNHTweaker.Objects
{
    public class SavedLevelData
    {
        public Dictionary<Patrol, int> PatrolSpawnCount = new Dictionary<Patrol, int>();

        public void RegisterPatrolSpawned(Patrol patrol)
        {
            if(PatrolSpawnCount.ContainsKey(patrol)) PatrolSpawnCount[patrol] = 0;
            PatrolSpawnCount[patrol] = PatrolSpawnCount[patrol] + 1;
        }

    }
}
