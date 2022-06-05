using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TNHTweaker.Objects.CharacterData
{
    /// <summary>
	/// The Level contains all settings for a level, where a level consists of a "Take" phase and a "Hold" phase
	/// </summary>
    [CreateAssetMenu(menuName = "TNHTweaker/Level", fileName = "NewLevel")]
    public class Level : ScriptableObject
    {
        /// <summary> The number of tokens the player will get for completing this levels hold </summary>
        public int NumOverrideTokensForHold;
        /// <summary> The settings for all supply points in this level </summary>
        public SupplyChallenge SupplyChallenge;
        /// <summary> The settings for the hold point defenders during this levels take phase </summary>
        public TakeChallenge TakeChallenge;
        /// <summary> The possible patrols that can spawn during this level </summary>
        public List<Patrol> Patrols = new List<Patrol>();
        /// <summary> The hold phases that will occur during the hold stage. Phases will occur in the order provided </summary>
        public List<HoldPhase> HoldPhases = new List<HoldPhase>();

    }
}
