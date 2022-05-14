using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegacyCharacterLoader.Objects.CharacterData
{
    public class LegacyPatrol
    {
        public List<string> EnemyType;
        public string LeaderType;
        public int PatrolSize;
        public int MaxPatrols;
        public int MaxPatrolsLimited;
        public float PatrolCadence;
        public float PatrolCadenceLimited;
        public int IFFUsed;
        public bool SwarmPlayer;
        public Sosig.SosigMoveSpeed AssualtSpeed;
        public bool IsBoss;
        public float DropChance;
        public bool DropsHealth;
    }
}
