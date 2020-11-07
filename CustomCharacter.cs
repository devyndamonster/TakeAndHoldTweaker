using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using UnityEngine;
using Valve.Newtonsoft.Json;
using Valve.Newtonsoft.Json.Converters;
using Valve.Newtonsoft.Json.Serialization;

namespace FistVR
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

            this.character = character;
        }

        public TNH_CharacterDef GetCharacter(int ID, string path, Dictionary<string, Sprite> sprites)
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
                character.Picture = ObjectBuilder.LoadSprite(path + "/thumb.png");
                character.Weapon_Primary = PrimaryWeapon.GetLoadoutEntry();
                character.Weapon_Secondary = SecondaryWeapon.GetLoadoutEntry();
                character.Weapon_Tertiary = TertiaryWeapon.GetLoadoutEntry();
                character.Item_Primary = PrimaryItem.GetLoadoutEntry();
                character.Item_Secondary = SecondaryItem.GetLoadoutEntry();
                character.Item_Tertiary = TertiaryItem.GetLoadoutEntry();
                character.Item_Shield = Shield.GetLoadoutEntry();
                character.RequireSightTable = RequireSightTable.GetObjectTable();
                character.EquipmentPool = (EquipmentPoolDef)ScriptableObject.CreateInstance(typeof(EquipmentPoolDef));
                character.EquipmentPool.Entries = EquipmentPools.Select(o => o.GetPoolEntry(sprites, path)).ToList();
                character.Progressions = new List<TNH_Progression>();
                character.Progressions.Add((TNH_Progression)ScriptableObject.CreateInstance(typeof(TNH_Progression)));
                character.Progressions[0].Levels = Levels.Select(o => o.GetLevel()).ToList();
                character.Progressions_Endless = new List<TNH_Progression>();
                character.Progressions_Endless.Add((TNH_Progression)ScriptableObject.CreateInstance(typeof(TNH_Progression)));
                character.Progressions_Endless[0].Levels = LevelsEndless.Select(o => o.GetLevel()).ToList();
            }

            return character;
        }

        public Level GetCurrentLevel(TNH_Progression.Level level)
        {
            if (Levels.Select(o => o.GetLevel()).Contains(level))
            {
                return Levels.Find(o => o.GetLevel().Equals(level));
            }
            else if(LevelsEndless.Select(o => o.GetLevel()).Contains(level))
            {
                return LevelsEndless.Find(o => o.GetLevel().Equals(level));
            }

            return null;
        }

    }

    public class EquipmentPool
    {
        public EquipmentPoolDef.PoolEntry.PoolEntryType Type;
        public int TokenCost;
        public int TokenCostLimited;
        public int MinLevelAppears;
        public int MaxLevelAppears;
        public float Rarity;
        public ObjectPool Table;

        [JsonIgnore]
        private EquipmentPoolDef.PoolEntry pool;

        public EquipmentPool() { }

        public EquipmentPool(EquipmentPoolDef.PoolEntry pool)
        {
            Type = pool.Type;
            TokenCost = pool.TokenCost;
            TokenCostLimited = pool.TokenCost_Limited;
            MinLevelAppears = pool.MinLevelAppears;
            MaxLevelAppears = pool.MaxLevelAppears;
            Rarity = pool.Rarity;
            Table = new ObjectPool(pool.TableDef);

            this.pool = pool;
        }

        public EquipmentPoolDef.PoolEntry GetPoolEntry(Dictionary<string, Sprite> sprites, string path)
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
                pool.TableDef = Table.GetObjectTable();

                if (sprites.ContainsKey(Table.IconName))
                {
                    pool.TableDef.Icon = sprites[Table.IconName];
                }
                else
                {
                    pool.TableDef.Icon = ObjectBuilder.LoadSprite(path + "/" + Table.IconName);
                }
            }

            return pool;
        }
    }

    public class ObjectPool
    {
        public string IconName;
        public FVRObject.ObjectCategory Category;
        public int MinAmmoCapacity;
        public int MaxAmmoCapacity;
        public int RequiredExactCapacity;
        public bool IsBlanked;
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

        public ObjectPool() { }

        public ObjectPool(ObjectTableDef objectTableDef)
        {
            IconName = "Not Used";
            Category = objectTableDef.Category;
            MinAmmoCapacity = objectTableDef.MinAmmoCapacity;
            MaxAmmoCapacity = objectTableDef.MaxAmmoCapacity;
            RequiredExactCapacity = objectTableDef.RequiredExactCapacity;
            IsBlanked = objectTableDef.IsBlanked;
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

        public ObjectTableDef GetObjectTable()
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
                loadout.TableDefs = Tables.Select(o => o.GetObjectTable()).ToList();
                if (ListOverride.Count > 0) loadout.ListOverride = ListOverride.Select(o => IM.OD[o]).ToList();
                else loadout.ListOverride = new List<FVRObject>();
                if(AmmoOverride != null && IM.OD.ContainsKey(AmmoOverride)) loadout.AmmoObjectOverride = IM.OD[AmmoOverride];

            }

            return loadout;
        }
    }

    public class Level
    {
        public int NumOverrideTokensForHold;
        public int AdditionalSupplyPoints;
        public int MinBoxesSpawned;
        public int MaxBoxesSpawned;
        public int MinTokensPerSupply;
        public int MaxTokensPerSupply;
        public float BoxTokenChance;
        public float BoxHealthChance;
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
            AdditionalSupplyPoints = 0;
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
                level.HoldChallenge.Phases = HoldPhases.Select(o => o.GetPhase()).ToList();
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
                takeChallenge.GID = (SosigEnemyID)SosigTemplate.SosigIDDict[EnemyType];
                takeChallenge.NumTurrets = NumTurrets;
                takeChallenge.NumGuards = NumGuards;
                takeChallenge.IFFUsed = IFFUsed;
            }

            return takeChallenge;
        }

    }

    public class Phase
    {
        public TNH_EncryptionType Encryption;
        public int MinTargets;
        public int MaxTargets;
        public string EnemyType;
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
            Encryption = phase.Encryption;
            MinTargets = phase.MinTargets;
            MaxTargets = phase.MaxTargets;
            EnemyType = phase.EType.ToString();
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
                phase.Encryption = Encryption;
                phase.MinTargets = MinTargets;
                phase.MaxTargets = MaxTargets;
                phase.EType = (SosigEnemyID)SosigTemplate.SosigIDDict[EnemyType];
                phase.LType = (SosigEnemyID)SosigTemplate.SosigIDDict[LeaderType];
                phase.MinEnemies = MinEnemies;
                phase.MaxEnemies = MaxEnemies;
                phase.MaxEnemiesAlive = MaxEnemiesAlive;
                phase.MaxDirections = MaxDirections;
                phase.SpawnCadence = SpawnCadence;
                phase.ScanTime = ScanTime;
                phase.WarmUp = WarmupTime;
                phase.IFFUsed = IFFUsed;
            }

            return phase;
        }

    }

    public class Patrol
    {
        public string EnemyType;
        public string LeaderType;
        public int PatrolSize;
        public int MaxPatrols;
        public int MaxPatrolsLimited;
        public float PatrolCadence;
        public float PatrolCadenceLimited;
        public int IFFUsed;
        public bool SwarmPlayer;
        public Sosig.SosigMoveSpeed AssualtSpeed;
        public float DropChance;
        public bool DropsHealth;
        public bool DropsMagazine;

        [JsonIgnore]
        private TNH_PatrolChallenge.Patrol patrol;

        public Patrol() { }

        public Patrol(TNH_PatrolChallenge.Patrol patrol)
        {
            EnemyType = patrol.EType.ToString();
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
            DropsMagazine = false;

            this.patrol = patrol;
        }

        public TNH_PatrolChallenge.Patrol GetPatrol()
        {
            if(patrol == null)
            {
                patrol = new TNH_PatrolChallenge.Patrol();
                patrol.EType = (SosigEnemyID)SosigTemplate.SosigIDDict[EnemyType];
                patrol.LType = (SosigEnemyID)SosigTemplate.SosigIDDict[LeaderType];
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
