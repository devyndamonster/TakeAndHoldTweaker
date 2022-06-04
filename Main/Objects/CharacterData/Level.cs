using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TNHTweaker.Objects.CharacterData
{

    [CreateAssetMenu(menuName = "TNHTweaker/Level", fileName = "NewLevel")]
    public class Level : ScriptableObject
    {
        public int NumOverrideTokensForHold;

        public SupplyChallenge SupplyChallenge;

        public TakeChallenge TakeChallenge;

        public List<Patrol> Patrols = new List<Patrol>();

        public List<HoldPhase> HoldPhases = new List<HoldPhase>();

    }
}
