using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegacyCharacterLoader.Objects.LootPools
{
    public class LegacyEquipmentGroup
    {
        public ObjectCategory Category;
        public float Rarity;
        public string RequiredQuest;
        public int ItemsToSpawn;
        public int MinAmmoCapacity;
        public int MaxAmmoCapacity;
        public int NumMagsSpawned;
        public int NumClipsSpawned;
        public int NumRoundsSpawned;
        public bool SpawnMagAndClip;
        public float BespokeAttachmentChance;
        public bool IsCompatibleMagazine;
        public bool AutoPopulateGroup;
        public bool ForceSpawnAllSubPools;
        public List<string> IDOverride;
        public List<TagEra> Eras;
        public List<TagSet> Sets;
        public List<TagFirearmSize> Sizes;
        public List<TagFirearmAction> Actions;
        public List<TagFirearmFiringMode> Modes;
        public List<TagFirearmFiringMode> ExcludedModes;
        public List<TagFirearmFeedOption> FeedOptions;
        public List<TagFirearmMount> MountsAvailable;
        public List<TagFirearmRoundPower> RoundPowers;
        public List<TagAttachmentFeature> Features;
        public List<TagMeleeStyle> MeleeStyles;
        public List<TagMeleeHandedness> MeleeHandedness;
        public List<TagFirearmMount> MountTypes;
        public List<TagPowerupType> PowerupTypes;
        public List<TagThrownType> ThrownTypes;
        public List<TagThrownDamageType> ThrownDamageTypes;
        public List<LegacyEquipmentGroup> SubGroups;
    }
}
