using ADepIn;
using Deli.Immediate;
using Deli.Newtonsoft.Json;
using Deli.Setup;
using Deli.VFS;
using FistVR;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public int CharacterID;
        public int CharacterGroup;
        public string TableID;
        public string CharacterIconName;
        public int StartingTokens;
        public bool ForceAllAgentWeapons;
        public bool ForceDisableOutfitFunctionality;
        public bool UsesPurchasePriceIncrement;
        public bool HasPrimaryWeapon;
        public bool HasSecondaryWeapon;
        public bool HasTertiaryWeapon;
        public bool HasPrimaryItem;
        public bool HasSecondaryItem;
        public bool HasTertiaryItem;
        public bool HasShield;
        public List<FVRObject.OTagEra> ValidAmmoEras;
        public List<FVRObject.OTagSet> ValidAmmoSets;
        public LoadoutEntry PrimaryWeapon;
        public LoadoutEntry SecondaryWeapon;
        public LoadoutEntry TertiaryWeapon;
        public LoadoutEntry PrimaryItem;
        public LoadoutEntry SecondaryItem;
        public LoadoutEntry TertiaryItem;
        public LoadoutEntry Shield;
        public ObjectPool RequireSightTable;
        public List<EquipmentPool> EquipmentPools;
        public List<Level> Levels;
        public List<Level> LevelsEndless;
        
        [JsonIgnore]
        private TNH_CharacterDef character;

        [JsonIgnore]
        private ObjectTable requiredSightsTable;


        public CustomCharacter() { }

        public CustomCharacter(TNH_CharacterDef character)
        {
            DisplayName = character.DisplayName;
            CharacterID = (int)character.CharacterID;
            CharacterGroup = (int)character.Group;
            TableID = character.TableID;
            CharacterIconName = character.Picture.name;
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
            ValidAmmoEras = character.ValidAmmoEras;
            ValidAmmoSets = character.ValidAmmoSets;

            PrimaryWeapon = new LoadoutEntry(character.Weapon_Primary);
            SecondaryWeapon = new LoadoutEntry(character.Weapon_Secondary);
            TertiaryWeapon = new LoadoutEntry(character.Weapon_Tertiary);
            PrimaryItem = new LoadoutEntry(character.Item_Primary);
            SecondaryItem = new LoadoutEntry(character.Item_Secondary);
            TertiaryItem = new LoadoutEntry(character.Item_Tertiary);
            Shield = new LoadoutEntry(character.Item_Shield);

            RequireSightTable = new ObjectPool(character.RequireSightTable);

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
                character.ValidAmmoEras = ValidAmmoEras;
                character.ValidAmmoSets = ValidAmmoSets;
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
                //character.Progressions[0].Levels = Levels.Select(o => o.GetLevel()).ToList();

                character.Progressions_Endless = new List<TNH_Progression>();
                character.Progressions_Endless.Add((TNH_Progression)ScriptableObject.CreateInstance(typeof(TNH_Progression)));
                character.Progressions_Endless[0].Levels = new List<TNH_Progression.Level>();
                foreach (Level level in LevelsEndless)
                {
                    character.Progressions_Endless[0].Levels.Add(level.GetLevel());
                }
                //character.Progressions_Endless[0].Levels = LevelsEndless.Select(o => o.GetLevel()).ToList();
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

        public ObjectTable GetRequiredSightsTable()
        {
            return requiredSightsTable;
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

            if (HasPrimaryWeapon && !PrimaryWeapon.DelayedInit(isCustom))
            {
                TNHTweakerLogger.LogWarning("TNHTweaker -- Primary starting weapon had no pools to spawn from, and will not spawn equipment!");
                HasPrimaryWeapon = false;
                character.Has_Weapon_Primary = false;
            }
            if (HasSecondaryWeapon && !SecondaryWeapon.DelayedInit(isCustom))
            {
                TNHTweakerLogger.LogWarning("TNHTweaker -- Secondary starting weapon had no pools to spawn from, and will not spawn equipment!");
                HasSecondaryWeapon = false;
                character.Has_Weapon_Secondary = false;
            }
            if (HasTertiaryWeapon && !TertiaryWeapon.DelayedInit(isCustom))
            {
                TNHTweakerLogger.LogWarning("TNHTweaker -- Tertiary starting weapon had no pools to spawn from, and will not spawn equipment!");
                HasTertiaryWeapon = false;
                character.Has_Weapon_Tertiary = false;
            }
            if (HasPrimaryItem && !PrimaryItem.DelayedInit(isCustom))
            {
                TNHTweakerLogger.LogWarning("TNHTweaker -- Primary starting item had no pools to spawn from, and will not spawn equipment!");
                HasPrimaryItem = false;
                character.Has_Item_Primary = false;
            }
            if (HasSecondaryItem && !SecondaryItem.DelayedInit(isCustom))
            {
                TNHTweakerLogger.LogWarning("TNHTweaker -- Secondary starting item had no pools to spawn from, and will not spawn equipment!");
                HasSecondaryItem = false;
                character.Has_Item_Secondary = false;
            }
            if (HasTertiaryItem && !TertiaryItem.DelayedInit(isCustom))
            {
                TNHTweakerLogger.LogWarning("TNHTweaker -- Tertiary starting item had no pools to spawn from, and will not spawn equipment!");
                HasTertiaryItem = false;
                character.Has_Item_Tertiary = false;
            }
            if (HasShield && !Shield.DelayedInit(isCustom))
            {
                TNHTweakerLogger.LogWarning("TNHTweaker -- Shield starting item had no pools to spawn from, and will not spawn equipment!");
                HasShield = false;
                character.Has_Item_Shield = false;
            }
            
            if(RequireSightTable != null)
            {
                requiredSightsTable = new ObjectTable();
                requiredSightsTable.Initialize(RequireSightTable.GetObjectTableDef());
            }
            
            for(int i = 0; i < EquipmentPools.Count; i++)
            {
                EquipmentPool pool = EquipmentPools[i];
                if(!pool.DelayedInit()){
                    TNHTweakerLogger.LogWarning("TNHTweaker -- Equipment pool had an empty table! Removing it so that it can't spawn!");
                    EquipmentPools.RemoveAt(i);
                    character.EquipmentPool.Entries.RemoveAt(i);
                    i-=1;
                }
            }

            for(int i = 0; i < Levels.Count; i++)
            {
                Levels[i].DelayedInit(isCustom, i);
            }

            for (int i = 0; i < LevelsEndless.Count; i++)
            {
                LevelsEndless[i].DelayedInit(isCustom, i);
            }
        }

    }

    public class EquipmentPool
    {
        public EquipmentPoolDef.PoolEntry.PoolEntryType Type;
        public string IconName;
        public int TokenCost;
        public int TokenCostLimited;
        public int MinLevelAppears;
        public int MaxLevelAppears;
        public float Rarity;
        public List<ObjectPool> Tables;

        [JsonIgnore]
        private EquipmentPoolDef.PoolEntry pool;

        public EquipmentPool() { }

        public EquipmentPool(EquipmentPoolDef.PoolEntry pool)
        {
            Type = pool.Type;
            IconName = pool.TableDef.Icon.name;
            TokenCost = pool.TokenCost;
            TokenCostLimited = pool.TokenCost_Limited;
            MinLevelAppears = pool.MinLevelAppears;
            MaxLevelAppears = pool.MaxLevelAppears;
            Rarity = pool.Rarity;
            Tables = new List<ObjectPool>();
            Tables.Add(new ObjectPool(pool.TableDef));

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
                pool.Rarity = Rarity;
                pool.TableDef = Tables[0].GetObjectTableDef();
            }

            return pool;
        }


        public bool DelayedInit()
        {
            if (pool != null)
            {
                bool allTablesValid = true;
                foreach(ObjectPool table in Tables)
                {
                    if (!table.DelayedInit())
                    {
                        allTablesValid = false;
                    }
                }

                return allTablesValid;
            }

            return false;
        }
    }

    public class ObjectPool
    {
        public FVRObject.ObjectCategory Category;
        public int ItemsToSpawn;
        public int MinAmmoCapacity;
        public int MaxAmmoCapacity;
        public int NumMagsSpawned;
        public int NumRoundsSpawned;
        public float BespokeAttachmentChance;
        public int RequiredExactCapacity;
        public bool IsBlanked;
        public bool IsCompatibleMagazine;
        public bool SpawnsInSmallCase;
        public bool SpawnsInLargeCase;
        public bool UseIDOverride;
        public List<string> IDOverride;
        public List<FVRObject.OTagEra> Eras;
        public List<FVRObject.OTagSet> Sets;
        public List<FVRObject.OTagFirearmSize> Sizes;
        public List<FVRObject.OTagFirearmAction> Actions;
        public List<FVRObject.OTagFirearmFiringMode> Modes;
        public List<FVRObject.OTagFirearmFiringMode> ExcludedModes;
        public List<FVRObject.OTagFirearmFeedOption> FeedOptions;
        public List<FVRObject.OTagFirearmMount> MountsAvailable;
        public List<FVRObject.OTagFirearmRoundPower> RoundPowers;
        public List<FVRObject.OTagAttachmentFeature> Features;
        public List<FVRObject.OTagMeleeStyle> MeleeStyles;
        public List<FVRObject.OTagMeleeHandedness> MeleeHandedness;
        public List<FVRObject.OTagFirearmMount> MountTypes;
        public List<FVRObject.OTagPowerupType> PowerupTypes;
        public List<FVRObject.OTagThrownType> ThrownTypes;
        public List<FVRObject.OTagThrownDamageType> ThrownDamageTypes;

        [JsonIgnore]
        private ObjectTableDef objectTableDef;

        [JsonIgnore]
        private ObjectTable objectTable;

        [JsonIgnore]
        private List<string> objects = new List<string>();

        public ObjectPool() { }

        public ObjectPool(ObjectTableDef objectTableDef)
        {
            Category = objectTableDef.Category;
            ItemsToSpawn = 1;
            MinAmmoCapacity = objectTableDef.MinAmmoCapacity;
            MaxAmmoCapacity = objectTableDef.MaxAmmoCapacity;
            NumMagsSpawned = 3;
            NumRoundsSpawned = 8;
            BespokeAttachmentChance = 0.5f;
            RequiredExactCapacity = objectTableDef.RequiredExactCapacity;
            IsBlanked = objectTableDef.IsBlanked;
            IsCompatibleMagazine = false;
            SpawnsInSmallCase = objectTableDef.SpawnsInSmallCase;
            SpawnsInLargeCase = objectTableDef.SpawnsInLargeCase;
            UseIDOverride = objectTableDef.UseIDListOverride;
            IDOverride = objectTableDef.IDOverride;
            Eras = objectTableDef.Eras;
            Sets = objectTableDef.Sets;
            Sizes = objectTableDef.Sizes;
            Actions = objectTableDef.Actions;
            Modes = objectTableDef.Modes;
            ExcludedModes = objectTableDef.ExcludeModes;
            FeedOptions = objectTableDef.Feedoptions;
            MountsAvailable = objectTableDef.MountsAvailable;
            RoundPowers = objectTableDef.RoundPowers;
            Features = objectTableDef.Features;
            MeleeHandedness = objectTableDef.MeleeHandedness;
            MeleeStyles = objectTableDef.MeleeStyles;
            MountTypes = objectTableDef.MountTypes;
            PowerupTypes = objectTableDef.PowerupTypes;
            ThrownTypes = objectTableDef.ThrownTypes;
            ThrownDamageTypes = objectTableDef.ThrownDamageTypes;

            this.objectTableDef = objectTableDef;
        }

        public ObjectTableDef GetObjectTableDef()
        {
            if(objectTableDef == null)
            {
                objectTableDef = (ObjectTableDef)ScriptableObject.CreateInstance(typeof(ObjectTableDef));
                objectTableDef.Category = Category;
                objectTableDef.MinAmmoCapacity = MinAmmoCapacity;
                objectTableDef.MaxAmmoCapacity = MaxAmmoCapacity;
                objectTableDef.RequiredExactCapacity = RequiredExactCapacity;
                objectTableDef.IsBlanked = IsBlanked;
                objectTableDef.SpawnsInSmallCase = SpawnsInSmallCase;
                objectTableDef.SpawnsInLargeCase = SpawnsInLargeCase;
                objectTableDef.UseIDListOverride = UseIDOverride;
                objectTableDef.IDOverride = IDOverride;
                objectTableDef.Eras = Eras;
                objectTableDef.Sets = Sets;
                objectTableDef.Sizes = Sizes;
                objectTableDef.Actions = Actions;
                objectTableDef.Modes = Modes;
                objectTableDef.ExcludeModes = ExcludedModes;
                objectTableDef.Feedoptions = FeedOptions;
                objectTableDef.MountsAvailable = MountsAvailable;
                objectTableDef.RoundPowers = RoundPowers;
                objectTableDef.Features = Features;
                objectTableDef.MeleeHandedness = MeleeHandedness;
                objectTableDef.MeleeStyles = MeleeStyles;
                objectTableDef.MountTypes = MountTypes;
                objectTableDef.PowerupTypes = PowerupTypes;
                objectTableDef.ThrownTypes = ThrownTypes;
                objectTableDef.ThrownDamageTypes = ThrownDamageTypes;
            }
            return objectTableDef;
        }

        public ObjectTable GetObjectTable()
        {
            return objectTable;
        }

        public List<string> GetObjects()
        {
            return objects;
        }

        /// <summary>
        /// Fills out the object table and removes any unloaded items
        /// </summary>
        /// <returns> Returns true if valid, and false if empty </returns>
        public bool DelayedInit()
        {
            TNHTweakerUtils.RemoveUnloadedObjectIDs(this);

            objectTable = new ObjectTable();

            //If this pool isn't a compatible magazine or manually set, then we need to populate it based on its parameters
            if (!IsCompatibleMagazine && !UseIDOverride)
            {
                objectTable.Initialize(GetObjectTableDef());
                foreach(FVRObject obj in objectTable.Objs)
                {
                    objects.Add(obj.ItemID);
                }
            }

            //The table is valid if it has items in it, or is a compatible magazine
            return objects.Count != 0 || IsCompatibleMagazine;
        }
    }

    public class LoadoutEntry
    {
        public int NumMags;
        public int NumRounds;
        public string AmmoOverride;
        public List<ObjectPool> Tables;
        public List<string> ListOverride;
        

        [JsonIgnore]
        private TNH_CharacterDef.LoadoutEntry loadout;

        public LoadoutEntry() { }

        public LoadoutEntry(TNH_CharacterDef.LoadoutEntry loadout)
        {
            if (loadout == null)
            {
                loadout = new TNH_CharacterDef.LoadoutEntry();
                loadout.TableDefs = new List<ObjectTableDef>();
                loadout.ListOverride = new List<FVRObject>();
            }

            NumMags = loadout.Num_Mags_SL_Clips;
            NumRounds = loadout.Num_Rounds;
            if (loadout.TableDefs != null) Tables = loadout.TableDefs.Select(o => new ObjectPool(o)).ToList();
            if (loadout.ListOverride != null) ListOverride = loadout.ListOverride.Select(o => o.ItemID).ToList();
            if (loadout.AmmoObjectOverride != null) AmmoOverride = loadout.AmmoObjectOverride.ItemID;

            this.loadout = loadout;
        }

        public TNH_CharacterDef.LoadoutEntry GetLoadoutEntry()
        {
            if(loadout == null)
            {
                loadout = new TNH_CharacterDef.LoadoutEntry();
                loadout.Num_Mags_SL_Clips = NumMags;
                loadout.Num_Rounds = NumRounds;
                loadout.TableDefs = Tables.Select(o => o.GetObjectTableDef()).ToList();
            }

            return loadout;
        }

        public bool DelayedInit(bool isCustom)
        {
            if (loadout != null)
            {
                for(int i = 0; i < Tables.Count; i++)
                {
                    ObjectPool pool = Tables[i];
                    if (!pool.DelayedInit())
                    {
                        Tables.RemoveAt(i);
                        loadout.TableDefs.RemoveAt(i);
                        i -= 1;
                    }
                }

                if (isCustom)
                {
                    loadout.ListOverride = new List<FVRObject>();
                    foreach (string item in ListOverride)
                    {
                        if (IM.OD.ContainsKey(item)) loadout.ListOverride.Add(IM.OD[item]);
                    }

                    if (AmmoOverride != null && IM.OD.ContainsKey(AmmoOverride)) loadout.AmmoObjectOverride = IM.OD[AmmoOverride];
                }
                
                return Tables.Count != 0 || loadout.ListOverride.Count != 0;
            }

            return false;
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

        public Level() { }

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

        public Phase() { }

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

        public Patrol() { }

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
