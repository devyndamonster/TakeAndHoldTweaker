using ADepIn;
using Deli.Immediate;
using Deli.Newtonsoft.Json;
using Deli.Setup;
using Deli.VFS;
using FistVR;
using MagazinePatcher;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Policy;
using System.Text;
using TNHTweaker.Utilities;
using UnityEngine;

namespace TNHTweaker.ObjectTemplates
{
    public class CustomCharacter
    {
        public string DisplayName;
        public string Description;
        public int CharacterGroup;
        public string TableID;
        public int StartingTokens;
        public bool ForceAllAgentWeapons;
        public bool ForceDisableOutfitFunctionality;
        public bool UsesPurchasePriceIncrement;
        public bool DisableCleanupSosigDrops;
        public bool HasPrimaryWeapon;
        public bool HasSecondaryWeapon;
        public bool HasTertiaryWeapon;
        public bool HasPrimaryItem;
        public bool HasSecondaryItem;
        public bool HasTertiaryItem;
        public bool HasShield;
        public List<TagEra> ValidAmmoEras;
        public List<TagSet> ValidAmmoSets;
        public List<string> GlobalAmmoBlacklist;
        public List<MagazineBlacklistEntry> MagazineBlacklist;
        
        public EquipmentGroup RequireSightTable;
        public LoadoutEntry PrimaryWeapon;
        public LoadoutEntry SecondaryWeapon;
        public LoadoutEntry TertiaryWeapon;
        public LoadoutEntry PrimaryItem;
        public LoadoutEntry SecondaryItem;
        public LoadoutEntry TertiaryItem;
        public LoadoutEntry Shield;
        public List<EquipmentPool> EquipmentPools;
        public List<Level> Levels;
        public List<Level> LevelsEndless;
        
        [JsonIgnore]
        private TNH_CharacterDef character;

        [JsonIgnore]
        private List<string> completedQuests;

        [JsonIgnore]
        private Dictionary<string, MagazineBlacklistEntry> magazineBlacklistDict;


        public CustomCharacter() {
            ValidAmmoEras = new List<TagEra>();
            ValidAmmoSets = new List<TagSet>();
            GlobalAmmoBlacklist = new List<string>();
            MagazineBlacklist = new List<MagazineBlacklistEntry>();
            RequireSightTable = new EquipmentGroup();
            PrimaryWeapon = new LoadoutEntry();
            SecondaryWeapon = new LoadoutEntry();
            TertiaryWeapon = new LoadoutEntry();
            PrimaryItem = new LoadoutEntry();
            SecondaryItem = new LoadoutEntry();
            TertiaryItem = new LoadoutEntry();
            Shield = new LoadoutEntry();
            EquipmentPools = new List<EquipmentPool>();
            Levels = new List<Level>();
            LevelsEndless = new List<Level>();
        }

        public CustomCharacter(TNH_CharacterDef character)
        {
            DisplayName = character.DisplayName;
            CharacterGroup = (int)character.Group;
            TableID = character.TableID;
            StartingTokens = character.StartingTokens;
            ForceAllAgentWeapons = character.ForceAllAgentWeapons;
            Description = character.Description;
            UsesPurchasePriceIncrement = character.UsesPurchasePriceIncrement;
            HasPrimaryWeapon = character.Has_Weapon_Primary;
            HasSecondaryWeapon = character.Has_Weapon_Secondary;
            HasTertiaryWeapon = character.Has_Weapon_Tertiary;
            HasPrimaryItem = character.Has_Item_Primary;
            HasSecondaryItem = character.Has_Item_Secondary;
            HasTertiaryItem = character.Has_Item_Tertiary;
            HasShield = character.Has_Item_Shield;
            ValidAmmoEras = character.ValidAmmoEras.Select(o => (TagEra)o).ToList();
            ValidAmmoSets = character.ValidAmmoSets.Select(o => (TagSet)o).ToList();
            GlobalAmmoBlacklist = new List<string>();
            MagazineBlacklist = new List<MagazineBlacklistEntry>();
            PrimaryWeapon = new LoadoutEntry(character.Weapon_Primary);
            SecondaryWeapon = new LoadoutEntry(character.Weapon_Secondary);
            TertiaryWeapon = new LoadoutEntry(character.Weapon_Tertiary);
            PrimaryItem = new LoadoutEntry(character.Item_Primary);
            SecondaryItem = new LoadoutEntry(character.Item_Secondary);
            TertiaryItem = new LoadoutEntry(character.Item_Tertiary);
            Shield = new LoadoutEntry(character.Item_Shield);

            RequireSightTable = new EquipmentGroup(character.RequireSightTable);

            EquipmentPools = character.EquipmentPool.Entries.Select(o => new EquipmentPool(o)).ToList();
            Levels = character.Progressions[0].Levels.Select(o => new Level(o)).ToList();
            LevelsEndless = character.Progressions_Endless[0].Levels.Select(o => new Level(o)).ToList();

            ForceDisableOutfitFunctionality = false;

            this.character = character;
        }

        public TNH_CharacterDef GetCharacter(int ID, Sprite thumbnail)
        {
            if(character == null)
            {

                character = (TNH_CharacterDef)ScriptableObject.CreateInstance(typeof(TNH_CharacterDef));
                character.DisplayName = DisplayName;
                character.CharacterID = (TNH_Char)ID;
                character.Group = (TNH_CharacterDef.CharacterGroup)CharacterGroup;
                character.TableID = TableID;
                character.StartingTokens = StartingTokens;
                character.ForceAllAgentWeapons = ForceAllAgentWeapons;
                character.Description = Description;
                character.UsesPurchasePriceIncrement = UsesPurchasePriceIncrement;
                character.Has_Weapon_Primary = HasPrimaryWeapon;
                character.Has_Weapon_Secondary = HasSecondaryWeapon;
                character.Has_Weapon_Tertiary = HasTertiaryWeapon;
                character.Has_Item_Primary = HasPrimaryItem;
                character.Has_Item_Secondary = HasSecondaryItem;
                character.Has_Item_Tertiary = HasTertiaryItem;
                character.Has_Item_Shield = HasShield;
                character.ValidAmmoEras = ValidAmmoEras.Select(o => (FVRObject.OTagEra)o).ToList();
                character.ValidAmmoSets = ValidAmmoSets.Select(o => (FVRObject.OTagSet)o).ToList();
                character.Picture = thumbnail;
                character.Weapon_Primary = PrimaryWeapon.GetLoadoutEntry();
                character.Weapon_Secondary = SecondaryWeapon.GetLoadoutEntry();
                character.Weapon_Tertiary = TertiaryWeapon.GetLoadoutEntry();
                character.Item_Primary = PrimaryItem.GetLoadoutEntry();
                character.Item_Secondary = SecondaryItem.GetLoadoutEntry();
                character.Item_Tertiary = TertiaryItem.GetLoadoutEntry();
                character.Item_Shield = Shield.GetLoadoutEntry();
                character.RequireSightTable = RequireSightTable.GetObjectTableDef();
                character.EquipmentPool = (EquipmentPoolDef)ScriptableObject.CreateInstance(typeof(EquipmentPoolDef));
                character.EquipmentPool.Entries = EquipmentPools.Select(o => o.GetPoolEntry()).ToList();

                character.Progressions = new List<TNH_Progression>();
                character.Progressions.Add((TNH_Progression)ScriptableObject.CreateInstance(typeof(TNH_Progression)));
                character.Progressions[0].Levels = new List<TNH_Progression.Level>();
                foreach (Level level in Levels)
                {
                    character.Progressions[0].Levels.Add(level.GetLevel());
                }
                

                character.Progressions_Endless = new List<TNH_Progression>();
                character.Progressions_Endless.Add((TNH_Progression)ScriptableObject.CreateInstance(typeof(TNH_Progression)));
                character.Progressions_Endless[0].Levels = new List<TNH_Progression.Level>();
                foreach (Level level in LevelsEndless)
                {
                    character.Progressions_Endless[0].Levels.Add(level.GetLevel());
                }
                

            }

            return character;
        }


        public TNH_CharacterDef GetCharacter()
        {
            if(character == null)
            {
                TNHTweakerLogger.LogError("TNHTweaker -- Tried to get character, but it hasn't been initialized yet! Returning null! Character Name : " + DisplayName);
                return null;
            }

            return character;
        }


        public Dictionary<string, MagazineBlacklistEntry> GetMagazineBlacklist()
        {
            return magazineBlacklistDict;
        }


        public Level GetCurrentLevel(TNH_Progression.Level currLevel)
        {
            foreach(Level level in Levels)
            {
                if (level.GetLevel().Equals(currLevel))
                {
                    return level;
                }
            }

            foreach (Level level in LevelsEndless)
            {
                if (level.GetLevel().Equals(currLevel))
                {
                    return level;
                }
            }

            return null;
        }

        public Phase GetCurrentPhase(TNH_HoldChallenge.Phase currPhase)
        {
            foreach(Level level in Levels)
            {
                foreach(Phase phase in level.HoldPhases)
                {
                    if (phase.GetPhase().Equals(currPhase))
                    {
                        return phase;
                    }
                }
            }

            foreach (Level level in LevelsEndless)
            {
                foreach (Phase phase in level.HoldPhases)
                {
                    if (phase.GetPhase().Equals(currPhase))
                    {
                        return phase;
                    }
                }
            }

            return null;
        }

        public bool CharacterUsesSosig(string id)
        {
            foreach(Level level in Levels)
            {
                if (level.LevelUsesSosig(id)) return true;
            }

            foreach (Level level in LevelsEndless)
            {
                if (level.LevelUsesSosig(id)) return true;
            }

            return false;
        }

        public void DelayedInit(bool isCustom)
        {
            TNHTweakerLogger.Log("TNHTweaker -- Delayed init of character: " + DisplayName, TNHTweakerLogger.LogType.Character);

            TNHTweakerLogger.Log("TNHTweaker -- Init of Primary Weapon", TNHTweakerLogger.LogType.Character);
            if (HasPrimaryWeapon && !PrimaryWeapon.DelayedInit(completedQuests))
            {
                TNHTweakerLogger.LogWarning("TNHTweaker -- Primary starting weapon had no pools to spawn from, and will not spawn equipment!");
                HasPrimaryWeapon = false;
                character.Has_Weapon_Primary = false;
            }

            TNHTweakerLogger.Log("TNHTweaker -- Init of Secondary Weapon", TNHTweakerLogger.LogType.Character);
            if (HasSecondaryWeapon && !SecondaryWeapon.DelayedInit(completedQuests))
            {
                TNHTweakerLogger.LogWarning("TNHTweaker -- Secondary starting weapon had no pools to spawn from, and will not spawn equipment!");
                HasSecondaryWeapon = false;
                character.Has_Weapon_Secondary = false;
            }

            TNHTweakerLogger.Log("TNHTweaker -- Init of Tertiary Weapon", TNHTweakerLogger.LogType.Character);
            if (HasTertiaryWeapon && !TertiaryWeapon.DelayedInit(completedQuests))
            {
                TNHTweakerLogger.LogWarning("TNHTweaker -- Tertiary starting weapon had no pools to spawn from, and will not spawn equipment!");
                HasTertiaryWeapon = false;
                character.Has_Weapon_Tertiary = false;
            }

            TNHTweakerLogger.Log("TNHTweaker -- Init of Primary Item", TNHTweakerLogger.LogType.Character);
            if (HasPrimaryItem && !PrimaryItem.DelayedInit(completedQuests))
            {
                TNHTweakerLogger.LogWarning("TNHTweaker -- Primary starting item had no pools to spawn from, and will not spawn equipment!");
                HasPrimaryItem = false;
                character.Has_Item_Primary = false;
            }

            TNHTweakerLogger.Log("TNHTweaker -- Init of Secondary Item", TNHTweakerLogger.LogType.Character);
            if (HasSecondaryItem && !SecondaryItem.DelayedInit(completedQuests))
            {
                TNHTweakerLogger.LogWarning("TNHTweaker -- Secondary starting item had no pools to spawn from, and will not spawn equipment!");
                HasSecondaryItem = false;
                character.Has_Item_Secondary = false;
            }

            TNHTweakerLogger.Log("TNHTweaker -- Init of Tertiary Item", TNHTweakerLogger.LogType.Character);
            if (HasTertiaryItem && !TertiaryItem.DelayedInit(completedQuests))
            {
                TNHTweakerLogger.LogWarning("TNHTweaker -- Tertiary starting item had no pools to spawn from, and will not spawn equipment!");
                HasTertiaryItem = false;
                character.Has_Item_Tertiary = false;
            }

            TNHTweakerLogger.Log("TNHTweaker -- Init of Shield", TNHTweakerLogger.LogType.Character);
            if (HasShield && !Shield.DelayedInit(completedQuests))
            {
                TNHTweakerLogger.LogWarning("TNHTweaker -- Shield starting item had no pools to spawn from, and will not spawn equipment!");
                HasShield = false;
                character.Has_Item_Shield = false;
            }

            TNHTweakerLogger.Log("TNHTweaker -- Init of required sights table", TNHTweakerLogger.LogType.Character);
            if (RequireSightTable != null && !RequireSightTable.DelayedInit(completedQuests))
            {
                TNHTweakerLogger.LogWarning("TNHTweaker -- Required sight table was empty, guns will not spawn with required sights");
                RequireSightTable = null;
            }

            TNHTweakerLogger.Log("TNHTweaker -- Init of equipment pools", TNHTweakerLogger.LogType.Character);
            magazineBlacklistDict = new Dictionary<string, MagazineBlacklistEntry>();
            if (MagazineBlacklist != null)
            {
                foreach (MagazineBlacklistEntry entry in MagazineBlacklist)
                {
                    magazineBlacklistDict.Add(entry.FirearmID, entry);
                }
            }

            for (int i = 0; i < EquipmentPools.Count; i++)
            {
                EquipmentPool pool = EquipmentPools[i];
                if(!pool.DelayedInit(completedQuests)){
                    TNHTweakerLogger.LogWarning("TNHTweaker -- Equipment pool had an empty table! Removing it so that it can't spawn!");
                    EquipmentPools.RemoveAt(i);
                    character.EquipmentPool.Entries.RemoveAt(i);
                    i-=1;
                }
            }

            TNHTweakerLogger.Log("TNHTweaker -- Init of levels", TNHTweakerLogger.LogType.Character);
            for (int i = 0; i < Levels.Count; i++)
            {
                Levels[i].DelayedInit(isCustom, i);
            }

            TNHTweakerLogger.Log("TNHTweaker -- Init of endless levels", TNHTweakerLogger.LogType.Character);
            for (int i = 0; i < LevelsEndless.Count; i++)
            {
                LevelsEndless[i].DelayedInit(isCustom, i);
            }
        }

    }


    /// <summary>
    /// An equipment pool is an entry that can spawn at a constructor panel
    /// </summary>
    public class EquipmentPool
    {
        public EquipmentPoolDef.PoolEntry.PoolEntryType Type;
        public string IconName;
        public int TokenCost;
        public int TokenCostLimited;
        public int MinLevelAppears;
        public int MaxLevelAppears;
        public bool SpawnsInSmallCase;
        public bool SpawnsInLargeCase;
        public EquipmentGroup PrimaryGroup;
        public EquipmentGroup BackupGroup;

        [JsonIgnore]
        private EquipmentPoolDef.PoolEntry pool;

        public EquipmentPool() {
            PrimaryGroup = new EquipmentGroup();
            BackupGroup = new EquipmentGroup();
            Type = EquipmentPoolDef.PoolEntry.PoolEntryType.Firearm;
        }

        public EquipmentPool(EquipmentPoolDef.PoolEntry pool)
        {
            Type = pool.Type;
            IconName = pool.TableDef.Icon.name;
            TokenCost = pool.TokenCost;
            TokenCostLimited = pool.TokenCost_Limited;
            MinLevelAppears = pool.MinLevelAppears;
            MaxLevelAppears = pool.MaxLevelAppears;
            PrimaryGroup = new EquipmentGroup(pool.TableDef);
            PrimaryGroup.Rarity = pool.Rarity;
            SpawnsInLargeCase = pool.TableDef.SpawnsInLargeCase;
            SpawnsInSmallCase = pool.TableDef.SpawnsInSmallCase;
            BackupGroup = new EquipmentGroup();

            this.pool = pool;
        }

        public EquipmentPoolDef.PoolEntry GetPoolEntry()
        {
            if(pool == null)
            {
                pool = new EquipmentPoolDef.PoolEntry();
                pool.Type = Type;
                pool.TokenCost = TokenCost;
                pool.TokenCost_Limited = TokenCostLimited;
                pool.MinLevelAppears = MinLevelAppears;
                pool.MaxLevelAppears = MaxLevelAppears;

                if(PrimaryGroup != null)
                {
                    pool.Rarity = PrimaryGroup.Rarity;
                }
                else
                {
                    pool.Rarity = 1;
                }

                pool.TableDef = PrimaryGroup.GetObjectTableDef();
            }

            return pool;
        }


        public bool DelayedInit(List<string> completedQuests)
        {
            if (pool != null)
            {
                if (LoadedTemplateManager.DefaultIconSprites.ContainsKey(IconName))
                {
                    pool.TableDef.Icon = LoadedTemplateManager.DefaultIconSprites[IconName];
                }

                if (PrimaryGroup != null)
                {
                    if (!PrimaryGroup.DelayedInit(completedQuests))
                    {
                        TNHTweakerLogger.Log("TNHTweaker -- Primary group for equipment pool entry was empty, setting to null!", TNHTweakerLogger.LogType.Character);
                        PrimaryGroup = null;
                    }
                }

                if (BackupGroup != null)
                {
                    if (!BackupGroup.DelayedInit(completedQuests))
                    {
                        if(PrimaryGroup == null) TNHTweakerLogger.Log("TNHTweaker -- Backup group for equipment pool entry was empty, setting to null!", TNHTweakerLogger.LogType.Character);
                        BackupGroup = null;
                    }
                }

                return PrimaryGroup != null || BackupGroup != null;
            }

            return false;
        }


        public List<EquipmentGroup> GetSpawnedEquipmentGroups()
        {
            if (PrimaryGroup != null)
            {
                return PrimaryGroup.GetSpawnedEquipmentGroups();
            }

            else if(BackupGroup != null)
            {
                return BackupGroup.GetSpawnedEquipmentGroups();
            }

            TNHTweakerLogger.LogWarning("TNHTweaker -- EquipmentPool had both PrimaryGroup and BackupGroup set to null! Returning an empty list for spawned equipment");
            return new List<EquipmentGroup>();
        }


        public override string ToString()
        {
            string output = "Equipment Pool : IconName=" + IconName + " : CostLimited=" + TokenCostLimited + " : CostSpawnlock=" + TokenCost;

            if(PrimaryGroup != null)
            {
                output += "\nPrimary Group";
                output += PrimaryGroup.ToString(0);
            }

            if(BackupGroup != null)
            {
                output += "\nBackup Group";
                output += BackupGroup.ToString(0);
            }
            
            return output;
        }

    }

    public class EquipmentGroup
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
        public List<EquipmentGroup> SubGroups;

        [JsonIgnore]
        private ObjectTableDef objectTableDef;

        [JsonIgnore]
        private List<string> objects = new List<string>();

        public EquipmentGroup() {
            Category = ObjectCategory.Firearm;
            IDOverride = new List<string>();
            Eras = new List<TagEra>();
            Sets = new List<TagSet>();
            Sizes = new List<TagFirearmSize>();
            Actions = new List<TagFirearmAction>();
            Modes = new List<TagFirearmFiringMode>();
            ExcludedModes = new List<TagFirearmFiringMode>();
            FeedOptions = new List<TagFirearmFeedOption>();
            MountsAvailable = new List<TagFirearmMount>();
            RoundPowers = new List<TagFirearmRoundPower>();
            Features = new List<TagAttachmentFeature>();
            MeleeStyles = new List<TagMeleeStyle>();
            MeleeHandedness = new List<TagMeleeHandedness>();
            MountTypes = new List<TagFirearmMount>();
            ThrownTypes = new List<TagThrownType>();
            ThrownDamageTypes = new List<TagThrownDamageType>();
            SubGroups = new List<EquipmentGroup>();
        }

        public EquipmentGroup(ObjectTableDef objectTableDef)
        {
            Category = (ObjectCategory)objectTableDef.Category;
            ItemsToSpawn = 1;
            MinAmmoCapacity = objectTableDef.MinAmmoCapacity;
            MaxAmmoCapacity = objectTableDef.MaxAmmoCapacity;
            NumMagsSpawned = 3;
            NumClipsSpawned = 3;
            NumRoundsSpawned = 8;
            BespokeAttachmentChance = 0.5f;
            IsCompatibleMagazine = false;
            AutoPopulateGroup = !objectTableDef.UseIDListOverride;
            IDOverride = new List<string>(objectTableDef.IDOverride);
            objectTableDef.IDOverride.Clear();
            Eras = objectTableDef.Eras.Select(o => (TagEra)o).ToList();
            Sets = objectTableDef.Sets.Select(o => (TagSet)o).ToList();
            Sizes = objectTableDef.Sizes.Select(o => (TagFirearmSize)o).ToList();
            Actions = objectTableDef.Actions.Select(o => (TagFirearmAction)o).ToList();
            Modes = objectTableDef.Modes.Select(o => (TagFirearmFiringMode)o).ToList();
            ExcludedModes = objectTableDef.ExcludeModes.Select(o => (TagFirearmFiringMode)o).ToList();
            FeedOptions = objectTableDef.Feedoptions.Select(o => (TagFirearmFeedOption)o).ToList();
            MountsAvailable = objectTableDef.MountsAvailable.Select(o => (TagFirearmMount)o).ToList();
            RoundPowers = objectTableDef.RoundPowers.Select(o => (TagFirearmRoundPower)o).ToList();
            Features = objectTableDef.Features.Select(o => (TagAttachmentFeature)o).ToList();
            MeleeHandedness = objectTableDef.MeleeHandedness.Select(o => (TagMeleeHandedness)o).ToList();
            MeleeStyles = objectTableDef.MeleeStyles.Select(o => (TagMeleeStyle)o).ToList();
            MountTypes = objectTableDef.MountTypes.Select(o => (TagFirearmMount)o).ToList();
            PowerupTypes = objectTableDef.PowerupTypes.Select(o => (TagPowerupType)o).ToList();
            ThrownTypes = objectTableDef.ThrownTypes.Select(o => (TagThrownType)o).ToList();
            ThrownDamageTypes = objectTableDef.ThrownDamageTypes.Select(o => (TagThrownDamageType)o).ToList();

            this.objectTableDef = objectTableDef;
        }

        public ObjectTableDef GetObjectTableDef()
        {
            if(objectTableDef == null)
            {
                objectTableDef = (ObjectTableDef)ScriptableObject.CreateInstance(typeof(ObjectTableDef));
                objectTableDef.Category = (FVRObject.ObjectCategory)Category;
                objectTableDef.MinAmmoCapacity = MinAmmoCapacity;
                objectTableDef.MaxAmmoCapacity = MaxAmmoCapacity;
                objectTableDef.RequiredExactCapacity = -1;
                objectTableDef.IsBlanked = false;
                objectTableDef.SpawnsInSmallCase = false;
                objectTableDef.SpawnsInLargeCase = false;
                objectTableDef.UseIDListOverride = !AutoPopulateGroup;
                objectTableDef.IDOverride = new List<string>();
                objectTableDef.Eras = Eras.Select(o => (FVRObject.OTagEra)o).ToList();
                objectTableDef.Sets = Sets.Select(o => (FVRObject.OTagSet)o).ToList();
                objectTableDef.Sizes = Sizes.Select(o => (FVRObject.OTagFirearmSize)o).ToList();
                objectTableDef.Actions = Actions.Select(o => (FVRObject.OTagFirearmAction)o).ToList();
                objectTableDef.Modes = Modes.Select(o => (FVRObject.OTagFirearmFiringMode)o).ToList();
                objectTableDef.ExcludeModes = ExcludedModes.Select(o => (FVRObject.OTagFirearmFiringMode)o).ToList();
                objectTableDef.Feedoptions = FeedOptions.Select(o => (FVRObject.OTagFirearmFeedOption)o).ToList();
                objectTableDef.MountsAvailable = MountsAvailable.Select(o => (FVRObject.OTagFirearmMount)o).ToList();
                objectTableDef.RoundPowers = RoundPowers.Select(o => (FVRObject.OTagFirearmRoundPower)o).ToList();
                objectTableDef.Features = Features.Select(o => (FVRObject.OTagAttachmentFeature)o).ToList();
                objectTableDef.MeleeHandedness = MeleeHandedness.Select(o => (FVRObject.OTagMeleeHandedness)o).ToList();
                objectTableDef.MeleeStyles = MeleeStyles.Select(o => (FVRObject.OTagMeleeStyle)o).ToList();
                objectTableDef.MountTypes = MountTypes.Select(o => (FVRObject.OTagFirearmMount)o).ToList();
                objectTableDef.PowerupTypes = PowerupTypes.Select(o => (FVRObject.OTagPowerupType)o).ToList();
                objectTableDef.ThrownTypes = ThrownTypes.Select(o => (FVRObject.OTagThrownType)o).ToList();
                objectTableDef.ThrownDamageTypes = ThrownDamageTypes.Select(o => (FVRObject.OTagThrownDamageType)o).ToList();
            }
            return objectTableDef;
        }

        public List<string> GetObjects()
        {
            return objects;
        }


        public List<EquipmentGroup> GetSpawnedEquipmentGroups()
        {
            List<EquipmentGroup> result;

            if (IsCompatibleMagazine || SubGroups == null || SubGroups.Count == 0)
            {
                result = new List<EquipmentGroup>();
                result.Add(this);
                return result;
            }

            else if (ForceSpawnAllSubPools)
            {
                result = new List<EquipmentGroup>();

                if(objects.Count > 0)
                {
                    result.Add(this);
                }

                foreach(EquipmentGroup group in SubGroups)
                {
                    result.AddRange(group.GetSpawnedEquipmentGroups());
                }
                
                return result;
            }

            else
            {
                float combinedRarity = objects.Count;
                foreach (EquipmentGroup group in SubGroups)
                {
                    combinedRarity += group.Rarity;
                }

                float randomSelection = UnityEngine.Random.Range(0, combinedRarity);

                if(randomSelection < objects.Count)
                {
                    result = new List<EquipmentGroup>();
                    result.Add(this);
                    return result;
                }

                else
                {
                    float progress = objects.Count;
                    for (int i = 0; i < SubGroups.Count; i++)
                    {
                        progress += SubGroups[i].Rarity;
                        if (randomSelection < progress)
                        {
                            return SubGroups[i].GetSpawnedEquipmentGroups();
                        }
                    }
                }
            }

            return new List<EquipmentGroup>();
        }



        /// <summary>
        /// Fills out the object table and removes any unloaded items
        /// </summary>
        /// <returns> Returns true if valid, and false if empty </returns>
        public bool DelayedInit(List<string> completedQuests = null)
        {
            //Start off by checking if this pool is even unlocked from a quest
            if (!string.IsNullOrEmpty(RequiredQuest))
            {
                if (completedQuests == null || !completedQuests.Contains(RequiredQuest))
                {
                    return false;
                }
            }

            //Before we add anything from the IDOverride list, remove anything that isn't loaded
            TNHTweakerUtils.RemoveUnloadedObjectIDs(this);


            //Every item in IDOverride gets added to the list of spawnable objects
            if (IDOverride != null)
            {
                objects.AddRange(IDOverride);
            }


            //If this pool isn't a compatible magazine or manually set, then we need to populate it based on its parameters
            if (!IsCompatibleMagazine && AutoPopulateGroup)
            {
                ObjectTable objectTable = new ObjectTable();
                objectTable.Initialize(GetObjectTableDef());
                foreach(FVRObject obj in objectTable.Objs)
                {
                    objects.Add(obj.ItemID);
                }
            }

            
            //Perform delayed init on all subgroups. If they are empty, we remove them
            if (SubGroups != null)
            {
                for (int i = 0; i < SubGroups.Count; i++)
                {
                    if (!SubGroups[i].DelayedInit(completedQuests))
                    {
                        //TNHTweakerLogger.Log("TNHTweaker -- Subgroup was empty, removing it!", TNHTweakerLogger.LogType.Character);
                        SubGroups.RemoveAt(i);
                        i -= 1;
                    }
                }
            }

            if (Rarity <= 0)
            {
                //TNHTweakerLogger.Log("TNHTweaker -- Equipment group had a rarity of 0 or less! Setting rarity to 1", TNHTweakerLogger.LogType.Character);
                Rarity = 1;
            }

            //The table is valid if it has items in it, or is a compatible magazine
            return objects.Count != 0 || IsCompatibleMagazine || (SubGroups != null && SubGroups.Count != 0);
        }


        public string ToString(int level)
        {
            string prefix = "\n-";
            for (int i = 0; i < level; i++) prefix += "-";

            string output = prefix + "Group : Rarity=" + Rarity;
            
            if (IsCompatibleMagazine)
            {
                output += prefix + "Compatible Magazine";
            }

            else
            {
                foreach(string item in objects)
                {
                    output += prefix + item;
                }

                if(SubGroups != null)
                {
                    foreach(EquipmentGroup group in SubGroups)
                    {
                        output += group.ToString(level + 1);
                    }
                }
            }

            return output;
        }

    }

    public class LoadoutEntry
    {
        public EquipmentGroup PrimaryGroup;
        public EquipmentGroup BackupGroup;
        
        [JsonIgnore]
        private TNH_CharacterDef.LoadoutEntry loadout;

        public LoadoutEntry() {
            PrimaryGroup = new EquipmentGroup();
            BackupGroup = new EquipmentGroup();
        }

        public LoadoutEntry(TNH_CharacterDef.LoadoutEntry loadout)
        {
            if (loadout == null)
            {
                loadout = new TNH_CharacterDef.LoadoutEntry();
                loadout.TableDefs = new List<ObjectTableDef>();
                loadout.ListOverride = new List<FVRObject>();
            }

            else if(loadout.ListOverride != null && loadout.ListOverride.Count > 0)
            {
                PrimaryGroup = new EquipmentGroup();
                PrimaryGroup.Rarity = 1;
                PrimaryGroup.IDOverride = loadout.ListOverride.Select(o => o.ItemID).ToList();
                PrimaryGroup.ItemsToSpawn = 1;
                PrimaryGroup.MinAmmoCapacity = -1;
                PrimaryGroup.MaxAmmoCapacity = 9999;
                PrimaryGroup.NumMagsSpawned = loadout.Num_Mags_SL_Clips;
                PrimaryGroup.NumClipsSpawned = loadout.Num_Mags_SL_Clips;
                PrimaryGroup.NumRoundsSpawned = loadout.Num_Rounds;
            }

            else if (loadout.TableDefs != null && loadout.TableDefs.Count > 0)
            {
                //If we have just one pool, then the primary pool becomes that pool
                if(loadout.TableDefs.Count == 1)
                {
                    PrimaryGroup = new EquipmentGroup(loadout.TableDefs[0]);
                    PrimaryGroup.Rarity = 1;
                    PrimaryGroup.NumMagsSpawned = loadout.Num_Mags_SL_Clips;
                    PrimaryGroup.NumClipsSpawned = loadout.Num_Mags_SL_Clips;
                    PrimaryGroup.NumRoundsSpawned = loadout.Num_Rounds;
                }

                else
                {
                    PrimaryGroup = new EquipmentGroup();
                    PrimaryGroup.Rarity = 1;
                    PrimaryGroup.SubGroups = new List<EquipmentGroup>();
                    foreach(ObjectTableDef table in loadout.TableDefs)
                    {
                        EquipmentGroup group = new EquipmentGroup(table);
                        group.Rarity = 1;
                        group.NumMagsSpawned = loadout.Num_Mags_SL_Clips;
                        group.NumClipsSpawned = loadout.Num_Mags_SL_Clips;
                        group.NumRoundsSpawned = loadout.Num_Rounds;
                        PrimaryGroup.SubGroups.Add(group);
                    }
                }
            }

            this.loadout = loadout;
        }

        public TNH_CharacterDef.LoadoutEntry GetLoadoutEntry()
        {
            if(loadout == null)
            {
                loadout = new TNH_CharacterDef.LoadoutEntry();
                loadout.Num_Mags_SL_Clips = 3;
                loadout.Num_Rounds = 9;

                if(PrimaryGroup != null)
                {
                    loadout.TableDefs = new List<ObjectTableDef>();
                    loadout.TableDefs.Add(PrimaryGroup.GetObjectTableDef());
                }
            }

            return loadout;
        }



        public bool DelayedInit(List<string> completedQuests)
        {
            if (loadout != null)
            {
                if(PrimaryGroup != null)
                {
                    if (!PrimaryGroup.DelayedInit(completedQuests))
                    {
                        TNHTweakerLogger.Log("TNHTweaker -- Primary group for loadout entry was empty, setting to null!", TNHTweakerLogger.LogType.Character);
                        PrimaryGroup = null;
                    }
                }

                if(BackupGroup != null)
                {
                    if (!BackupGroup.DelayedInit(completedQuests))
                    {
                        if (PrimaryGroup == null) TNHTweakerLogger.Log("TNHTweaker -- Backup group for loadout entry was empty, setting to null!", TNHTweakerLogger.LogType.Character);
                        BackupGroup = null;
                    }
                }

                return PrimaryGroup != null || BackupGroup != null;
            }

            return false;
        }


        public override string ToString()
        {
            string output = "Loadout Entry";

            if (PrimaryGroup != null)
            {
                output += "\nPrimary Group";
                output += PrimaryGroup.ToString(0);
            }

            if (BackupGroup != null)
            {
                output += "\nBackup Group";
                output += BackupGroup.ToString(0);
            }

            return output;
        }
    }

    public class Level
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
        public TakeChallenge TakeChallenge;
        public List<Phase> HoldPhases;
        public TakeChallenge SupplyChallenge;
        public List<Patrol> Patrols;

        [JsonIgnore]
        private TNH_Progression.Level level;

        public Level() {
            PossiblePanelTypes = new List<PanelType>();
            TakeChallenge = new TakeChallenge();
            HoldPhases = new List<Phase>();
            SupplyChallenge = new TakeChallenge();
            Patrols = new List<Patrol>();
        }

        public Level(TNH_Progression.Level level)
        {
            NumOverrideTokensForHold = level.NumOverrideTokensForHold;
            TakeChallenge = new TakeChallenge(level.TakeChallenge);
            SupplyChallenge = new TakeChallenge(level.TakeChallenge);
            HoldPhases = level.HoldChallenge.Phases.Select(o => new Phase(o)).ToList();
            Patrols = level.PatrolChallenge.Patrols.Select(o => new Patrol(o)).ToList();
            PossiblePanelTypes = new List<PanelType>();
            PossiblePanelTypes.Add(PanelType.AmmoReloader);
            PossiblePanelTypes.Add(PanelType.MagDuplicator);
            PossiblePanelTypes.Add(PanelType.Recycler);
            MinConstructors = 1;
            MaxConstructors = 1;
            MinPanels = 1;
            MaxPanels = 1;
            MinSupplyPoints = 2;
            MaxSupplyPoints = 3;
            MinBoxesSpawned = 2;
            MaxBoxesSpawned = 4;
            MinTokensPerSupply = 1;
            MaxTokensPerSupply = 1;
            BoxTokenChance = 0;
            BoxHealthChance = 0.5f;

            this.level = level;
        }

        public TNH_Progression.Level GetLevel()
        {
            if(level == null)
            {
                level = new TNH_Progression.Level();
                level.NumOverrideTokensForHold = NumOverrideTokensForHold;
                level.TakeChallenge = TakeChallenge.GetTakeChallenge();

                level.HoldChallenge = (TNH_HoldChallenge)ScriptableObject.CreateInstance(typeof(TNH_HoldChallenge));
                level.HoldChallenge.Phases = new List<TNH_HoldChallenge.Phase>();
                foreach(Phase phase in HoldPhases)
                {
                    level.HoldChallenge.Phases.Add(phase.GetPhase());
                }
                //level.HoldChallenge.Phases = HoldPhases.Select(o => o.GetPhase()).ToList();

                level.SupplyChallenge = SupplyChallenge.GetTakeChallenge();
                level.PatrolChallenge = (TNH_PatrolChallenge)ScriptableObject.CreateInstance(typeof(TNH_PatrolChallenge));
                level.PatrolChallenge.Patrols = Patrols.Select(o => o.GetPatrol()).ToList();
                level.TrapsChallenge = (TNH_TrapsChallenge)ScriptableObject.CreateInstance(typeof(TNH_TrapsChallenge));
            }

            return level;
        }

        public Patrol GetPatrol(TNH_PatrolChallenge.Patrol patrol)
        {
            if (Patrols.Select(o => o.GetPatrol()).Contains(patrol))
            {
                return Patrols.Find(o => o.GetPatrol().Equals(patrol));
            }

            return null;
        }

        public void DelayedInit(bool isCustom, int levelIndex)
        {
            //If this is a level for a default character, we should try to replicate the vanilla layout
            if (!isCustom)
            {
                MaxSupplyPoints = Mathf.Clamp(levelIndex + 1, 1, 3);
                MinSupplyPoints = Mathf.Clamp(levelIndex + 1, 1, 3);

                foreach (Phase phase in HoldPhases)
                {
                    phase.DelayedInit(isCustom);
                }
            }
        }

        public bool LevelUsesSosig(string id)
        {
            if (TakeChallenge.EnemyType == id)
            {
                return true;
            }

            else if (SupplyChallenge.EnemyType == id)
            {
                return true;
            }

            foreach (Patrol patrol in Patrols)
            {
                if (patrol.LeaderType == id)
                {
                    return true;
                }

                foreach (string sosigID in patrol.EnemyType)
                {
                    if (sosigID == id)
                    {
                        return true;
                    }
                }
            }

            foreach (Phase phase in HoldPhases)
            {
                if (phase.LeaderType == id)
                {
                    return true;
                }

                foreach (string sosigID in phase.EnemyType)
                {
                    if (sosigID == id)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }


    public class TakeChallenge
    {
        public TNH_TurretType TurretType;
        public string EnemyType;
        
        public int NumTurrets;
        public int NumGuards;
        public int IFFUsed;
        

        [JsonIgnore]
        private TNH_TakeChallenge takeChallenge;

        public TakeChallenge() { }

        public TakeChallenge(TNH_TakeChallenge takeChallenge)
        {
            TurretType = takeChallenge.TurretType;
            EnemyType = takeChallenge.GID.ToString();
            NumGuards = takeChallenge.NumGuards;
            NumTurrets = takeChallenge.NumTurrets;
            IFFUsed = takeChallenge.IFFUsed;
            
            this.takeChallenge = takeChallenge;
        }

        public TNH_TakeChallenge GetTakeChallenge()
        {
            if(takeChallenge == null)
            {
                takeChallenge = (TNH_TakeChallenge)ScriptableObject.CreateInstance(typeof(TNH_TakeChallenge));
                takeChallenge.TurretType = TurretType;

                //Try to get the necessary SosigEnemyIDs
                if (LoadedTemplateManager.SosigIDDict.ContainsKey(EnemyType))
                {
                    takeChallenge.GID = (SosigEnemyID)LoadedTemplateManager.SosigIDDict[EnemyType];
                }
                else
                {
                    takeChallenge.GID = (SosigEnemyID)Enum.Parse(typeof(SosigEnemyID), EnemyType);
                }

                takeChallenge.NumTurrets = NumTurrets;
                takeChallenge.NumGuards = NumGuards;
                takeChallenge.IFFUsed = IFFUsed;
            }

            return takeChallenge;
        }
    }

    public class Phase
    {
        public List<TNH_EncryptionType> Encryptions;
        public int MinTargets;
        public int MaxTargets;
        public int MinTargetsLimited;
        public int MaxTargetsLimited;
        public List<string> EnemyType;
        public string LeaderType;
        public int MinEnemies;
        public int MaxEnemies;
        public int MaxEnemiesAlive;
        public int MaxDirections;
        public float SpawnCadence;
        public float ScanTime;
        public float WarmupTime;
        public int IFFUsed;
        public float GrenadeChance;
        public string GrenadeType;
        public bool SwarmPlayer;

        [JsonIgnore]
        private TNH_HoldChallenge.Phase phase;

        public Phase() {
            Encryptions = new List<TNH_EncryptionType>();
            EnemyType = new List<string>();
        }

        public Phase(TNH_HoldChallenge.Phase phase)
        {
            Encryptions = new List<TNH_EncryptionType>();
            Encryptions.Add(phase.Encryption);
            MinTargets = phase.MinTargets;
            MaxTargets = phase.MaxTargets;
            MinTargetsLimited = 1;
            MaxTargetsLimited = 1;
            EnemyType = new List<string>();
            EnemyType.Add(phase.EType.ToString());
            LeaderType = phase.LType.ToString();
            MinEnemies = phase.MinEnemies;
            MaxEnemies = phase.MaxEnemies;
            MaxEnemiesAlive = phase.MaxEnemiesAlive;
            MaxDirections = phase.MaxDirections;
            SpawnCadence = phase.SpawnCadence;
            ScanTime = phase.ScanTime;
            WarmupTime = phase.WarmUp;
            IFFUsed = phase.IFFUsed;
            GrenadeChance = 0;
            GrenadeType = "Sosiggrenade_Flash";
            SwarmPlayer = false;

            this.phase = phase;
        }

        public TNH_HoldChallenge.Phase GetPhase()
        {
            if(phase == null)
            {
                phase = new TNH_HoldChallenge.Phase();
                phase.Encryption = Encryptions[0];
                phase.MinTargets = MinTargets;
                phase.MaxTargets = MaxTargets;
                phase.MinEnemies = MinEnemies;
                phase.MaxEnemies = MaxEnemies;
                phase.MaxEnemiesAlive = MaxEnemiesAlive;
                phase.MaxDirections = MaxDirections;
                phase.SpawnCadence = SpawnCadence;
                phase.ScanTime = ScanTime;
                phase.WarmUp = WarmupTime;
                phase.IFFUsed = IFFUsed;

                //Try to get the necessary SosigEnemyIDs
                if (LoadedTemplateManager.SosigIDDict.ContainsKey(EnemyType[0]))
                {
                    phase.EType = (SosigEnemyID)LoadedTemplateManager.SosigIDDict[EnemyType[0]];
                }
                else
                {
                    phase.EType = (SosigEnemyID)Enum.Parse(typeof(SosigEnemyID), EnemyType[0]);
                }

                if (LoadedTemplateManager.SosigIDDict.ContainsKey(LeaderType))
                {
                    phase.LType = (SosigEnemyID)LoadedTemplateManager.SosigIDDict[LeaderType];
                }
                else
                {
                    phase.LType = (SosigEnemyID)Enum.Parse(typeof(SosigEnemyID), LeaderType);
                }
                
            }

            return phase;
        }

        public void DelayedInit(bool isCustom)
        {
            if (!isCustom)
            {
                if(Encryptions[0] == TNH_EncryptionType.Static)
                {
                    MinTargetsLimited = 3;
                    MaxTargetsLimited = 3;
                }
            }
        }
    }

    public class Patrol
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

        [JsonIgnore]
        private TNH_PatrolChallenge.Patrol patrol;

        public Patrol() {
            EnemyType = new List<string>();
        }

        public Patrol(TNH_PatrolChallenge.Patrol patrol)
        {
            EnemyType = new List<string>();
            EnemyType.Add(patrol.EType.ToString());
            LeaderType = patrol.LType.ToString();
            PatrolSize = patrol.PatrolSize;
            MaxPatrols = patrol.MaxPatrols;
            MaxPatrolsLimited = patrol.MaxPatrols_LimitedAmmo;
            PatrolCadence = patrol.TimeTilRegen;
            PatrolCadenceLimited = patrol.TimeTilRegen_LimitedAmmo;
            IFFUsed = patrol.IFFUsed;
            SwarmPlayer = false;
            AssualtSpeed = Sosig.SosigMoveSpeed.Walking;
            DropChance = 0.65f;
            DropsHealth = true;
            IsBoss = false;

            this.patrol = patrol;
        }

        public TNH_PatrolChallenge.Patrol GetPatrol()
        {
            if(patrol == null)
            {
                patrol = new TNH_PatrolChallenge.Patrol();

                //Try to get the necessary SosigEnemyIDs
                if (LoadedTemplateManager.SosigIDDict.ContainsKey(EnemyType[0]))
                {
                    patrol.EType = (SosigEnemyID)LoadedTemplateManager.SosigIDDict[EnemyType[0]];
                }
                else
                {
                    patrol.EType = (SosigEnemyID)Enum.Parse(typeof(SosigEnemyID), EnemyType[0]);
                }

                if (LoadedTemplateManager.SosigIDDict.ContainsKey(LeaderType))
                {
                    patrol.LType = (SosigEnemyID)LoadedTemplateManager.SosigIDDict[LeaderType];
                }
                else
                {
                    patrol.LType = (SosigEnemyID)Enum.Parse(typeof(SosigEnemyID), LeaderType);
                }

                patrol.PatrolSize = PatrolSize;
                patrol.MaxPatrols = MaxPatrols;
                patrol.MaxPatrols_LimitedAmmo = MaxPatrolsLimited;
                patrol.TimeTilRegen = PatrolCadence;
                patrol.TimeTilRegen_LimitedAmmo = PatrolCadenceLimited;
                patrol.IFFUsed = IFFUsed;
            }

            return patrol;
        }

    }



}
