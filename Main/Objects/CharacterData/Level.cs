﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TNHTweaker.Objects.CharacterData
{
    public class Level : ScriptableObject
    {
        public int NumOverrideTokensForHold;

        public TakeChallenge SupplyChallenge;

        public TakeChallenge TakeChallenge;

        public List<Patrol> Patrols = new List<Patrol>();

        public List<HoldPhase> HoldPhases = new List<HoldPhase>();

    }
}
