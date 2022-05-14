using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker;

namespace LegacyCharacterLoader.Objects.CharacterData
{
    public class LegacyLevel
    {
        public int NumOverrideTokensForHold;
        public int MinSupplyPoints;
        public int MaxSupplyPoints;
        public int MinConstructors;
        public int MaxConstructors;
        public int MinPanels;
        public int MaxPanels;
        public int MinBoxesSpawned;
        public int MaxBoxesSpawned;
        public int MinTokensPerSupply;
        public int MaxTokensPerSupply;
        public float BoxTokenChance;
        public float BoxHealthChance;
        public List<PanelType> PossiblePanelTypes;
        public LegacyTakeChallenge TakeChallenge;
        public List<LegacyHoldPhase> HoldPhases;
        public LegacyTakeChallenge SupplyChallenge;
        public List<LegacyPatrol> Patrols;
    }
}
