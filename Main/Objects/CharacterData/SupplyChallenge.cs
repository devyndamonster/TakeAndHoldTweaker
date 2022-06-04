using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TNHTweaker.Objects.CharacterData
{
    /// <summary>
	/// The SupplyChallenge determines settings of the supply points that can spawn
	/// </summary>
	[CreateAssetMenu(menuName = "TNHTweaker/SupplyChallenge", fileName = "NewSupplyChallenge")]
	public class SupplyChallenge : TakeChallenge
	{
        /// <summary> The minimum number of constructors (primary purchase panel) that can spawn at a supply point </summary>
        public int MinConstructors = 1;
        /// <summary> The maximum number of constructors (primary purchase panel) that can spawn at a supply point </summary>
        public int MaxConstructors = 1;
        /// <summary> The minimum number of secondary panels that can spawn at a supply point </summary>
        public int MinPanels = 1;
        /// <summary> The maximum number of secondary panels that can spawn at a supply point </summary>
        public int MaxPanels = 1;
        /// <summary> The minimum number of boxes that can spawn at a supply point </summary>
        public int MinBoxesSpawned = 1;
        /// <summary> The maximum number of boxes that can spawn at a supply point </summary>
        public int MaxBoxesSpawned = 3;
        /// <summary> The minimum number of tokens that can spawn in boxes at a supply point </summary>
        public int MinTokensPerSupply = 1;
        /// <summary> The maximum number of tokens that can spawn in boxes at a supply point </summary>
        public int MaxTokensPerSupply = 1;
        /// <summary> The change that a token will spawn per supply box </summary>
        public float BoxTokenChance = 1;
        /// <summary> The change that a health drop will spawn per supply box </summary>
        public float BoxHealthChance = 1;
    }
}
