using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Valve.Newtonsoft.Json;

namespace TNHTweaker
{
    public class SosigTemplate
    {
        public string DisplayName;
        public SosigEnemyCategory SosigEnemyCategory;
        public string SosigEnemyID;
        public string EnemyType;
        public List<string> SosigPrefabs;
        public List<SosigConfig> Configs;
        public List<SosigConfig> ConfigsEasy;
        public List<OutfitConfig> OutfitConfigs;
        public List<string> WeaponOptions;
        public List<string> WeaponOptionsSecondary;
        public List<string> WeaponOptionsTertiary;
        public float SecondaryChance;
        public float TertiaryChance;
		public float DroppedLootChance;
		public ObjectPool DroppedObjectPool;

        [JsonIgnore]
        private SosigEnemyTemplate template;

        public SosigTemplate() { }

        public SosigTemplate(SosigEnemyTemplate template) {

            DisplayName = template.DisplayName;
            SosigEnemyCategory = template.SosigEnemyCategory;
            SosigEnemyID = template.SosigEnemyID.ToString();
            EnemyType = template.EnemyType.ToString();
            SecondaryChance = template.SecondaryChance;
            TertiaryChance = template.TertiaryChance;

            SosigPrefabs = template.SosigPrefabs.Select(o => o.ItemID).ToList();
            WeaponOptions = template.WeaponOptions.Select(o => o.ItemID).ToList();
            WeaponOptionsSecondary = template.WeaponOptions_Secondary.Select(o => o.ItemID).ToList();
            WeaponOptionsTertiary = template.WeaponOptions_Tertiary.Select(o => o.ItemID).ToList();

			Configs = template.ConfigTemplates.Select(o => new SosigConfig(o)).ToList();
			ConfigsEasy = template.ConfigTemplates_Easy.Select(o => new SosigConfig(o)).ToList();
			OutfitConfigs = template.OutfitConfig.Select(o => new OutfitConfig(o)).ToList();

			DroppedLootChance = 0;
			DroppedObjectPool = new ObjectPool();

			this.template = template;
		}

		public SosigEnemyTemplate GetSosigEnemyTemplate()
        {
			if(template == null)
            {
				template = (SosigEnemyTemplate)ScriptableObject.CreateInstance(typeof(SosigEnemyTemplate));

				template.DisplayName = DisplayName;
				template.SosigEnemyCategory = SosigEnemyCategory;
				template.SecondaryChance = SecondaryChance;
				template.TertiaryChance = TertiaryChance;

				template.ConfigTemplates = Configs.Select(o => o.GetConfigTemplate()).ToList();
				template.ConfigTemplates_Easy = ConfigsEasy.Select(o => o.GetConfigTemplate()).ToList();
				template.OutfitConfig = OutfitConfigs.Select(o => o.GetOutfitConfig()).ToList();
			}

			return template;
        }

		public void DelayedInit()
        {
			if(template != null)
            {
				TNHTweakerUtils.RemoveUnloadedObjectIDs(this);

				template.SosigPrefabs = SosigPrefabs.Select(o => IM.OD[o]).ToList();
				template.WeaponOptions = WeaponOptions.Select(o => IM.OD[o]).ToList();
				template.WeaponOptions_Secondary = WeaponOptionsSecondary.Select(o => IM.OD[o]).ToList();
				template.WeaponOptions_Tertiary = WeaponOptionsTertiary.Select(o => IM.OD[o]).ToList();

				foreach(OutfitConfig outfit in OutfitConfigs)
                {
					outfit.DelayedInit();
                }

				if(DroppedObjectPool != null)
                {
					DroppedObjectPool.DelayedInit();
				}
				
				//Add the new sosig template to the global dictionaries
				ManagerSingleton<IM>.Instance.odicSosigObjsByID.Add(template.SosigEnemyID, template);
				ManagerSingleton<IM>.Instance.odicSosigIDsByCategory[template.SosigEnemyCategory].Add(template.SosigEnemyID);
				ManagerSingleton<IM>.Instance.odicSosigObjsByCategory[template.SosigEnemyCategory].Add(template);
			}
		}

    }

    public class SosigConfig
    {
        public float ViewDistance;
        public float HearingDistance;
        public float MaxFOV;
        public float SearchExtentsModifier;
        public bool DoesAggroOnFriendlyFire;
        public bool HasABrain;
        public bool DoesDropWeaponsOnBallistic;
        public bool CanPickupRanged;
        public bool CanPickupMelee;
        public bool CanPickupOther;
        public int TargetCapacity;
        public float TargetTrackingTime;
        public float NoFreshTargetTime;
		public float AssaultPointOverridesSkirmishPointWhenFurtherThan;
		public float RunSpeed;
		public float WalkSpeed;
		public float SneakSpeed;
		public float CrawlSpeed;
		public float TurnSpeed;
		public float MaxJointLimit;
		public float MovementRotMagnitude;
		public float TotalMustard;
		public float BleedDamageMult;
		public float BleedRateMultiplier;
		public float BleedVFXIntensity;
		public float DamMult_Projectile;
		public float DamMult_Explosive;
		public float DamMult_Melee;
		public float DamMult_Piercing;
		public float DamMult_Blunt;
		public float DamMult_Cutting;
		public float DamMult_Thermal;
		public float DamMult_Chilling;
		public float DamMult_EMP;
		public List<float> LinkDamageMultipliers;
		public List<float> LinkStaggerMultipliers;
		public List<Vector2Serializable> StartingLinkIntegrity;
		public List<float> StartingChanceBrokenJoint;
		public float ShudderThreshold;
		public float ConfusionThreshold;
		public float ConfusionMultiplier;
		public float ConfusionTimeMax;
		public float StunThreshold;
		public float StunMultiplier;
		public float StunTimeMax;
		public bool CanBeGrabbed;
		public bool CanBeSevered;
		public bool CanBeStabbed;
		public bool CanBeSurpressed;
		public float SuppressionMult;
		public bool DoesJointBreakKill_Head;
		public bool DoesJointBreakKill_Upper;
		public bool DoesJointBreakKill_Lower;
		public bool DoesSeverKill_Head;
		public bool DoesSeverKill_Upper;
		public bool DoesSeverKill_Lower;
		public bool DoesExplodeKill_Head;
		public bool DoesExplodeKill_Upper;
		public bool DoesExplodeKill_Lower;

        [JsonIgnore]
        private SosigConfigTemplate template;

		public SosigConfig() { }
        public SosigConfig(SosigConfigTemplate template)
        {
			ViewDistance = template.ViewDistance;
			HearingDistance = template.HearingDistance;
			MaxFOV = template.MaxFOV;
			SearchExtentsModifier = template.SearchExtentsModifier;
			DoesAggroOnFriendlyFire = template.DoesAggroOnFriendlyFire;
			HasABrain = template.HasABrain;
			DoesDropWeaponsOnBallistic = template.DoesDropWeaponsOnBallistic;
			CanPickupRanged = template.CanPickup_Ranged;
			CanPickupMelee = template.CanPickup_Melee;
			CanPickupOther = template.CanPickup_Other;
			TargetCapacity = template.TargetCapacity;
			TargetTrackingTime = template.TargetTrackingTime;
			NoFreshTargetTime = template.NoFreshTargetTime;
			AssaultPointOverridesSkirmishPointWhenFurtherThan = template.AssaultPointOverridesSkirmishPointWhenFurtherThan;
			RunSpeed = template.RunSpeed;
			WalkSpeed = template.WalkSpeed;
			SneakSpeed = template.SneakSpeed;
			CrawlSpeed = template.CrawlSpeed;
			TurnSpeed = template.TurnSpeed;
			MaxJointLimit = template.MaxJointLimit;
			MovementRotMagnitude = template.MovementRotMagnitude;
			TotalMustard = template.TotalMustard;
			BleedDamageMult = template.BleedDamageMult;
			BleedRateMultiplier = template.BleedRateMultiplier;
			BleedVFXIntensity = template.BleedVFXIntensity;
			DamMult_Projectile = template.DamMult_Projectile;
			DamMult_Explosive = template.DamMult_Explosive;
			DamMult_Melee = template.DamMult_Melee;
			DamMult_Piercing = template.DamMult_Piercing;
			DamMult_Blunt = template.DamMult_Blunt;
			DamMult_Cutting = template.DamMult_Cutting;
			DamMult_Thermal = template.DamMult_Thermal;
			DamMult_Chilling = template.DamMult_Chilling;
			DamMult_EMP = template.DamMult_EMP;
			LinkDamageMultipliers = template.LinkDamageMultipliers;
			LinkStaggerMultipliers = template.LinkStaggerMultipliers;
			StartingLinkIntegrity = template.StartingLinkIntegrity.Select(o => new Vector2Serializable(o)).ToList();
			StartingChanceBrokenJoint = template.StartingChanceBrokenJoint;
			ShudderThreshold = template.ShudderThreshold;
			ConfusionThreshold = template.ConfusionThreshold;
			ConfusionMultiplier = template.ConfusionMultiplier;
			ConfusionTimeMax = template.ConfusionTimeMax;
			StunThreshold = template.StunThreshold;
			StunMultiplier = template.StunMultiplier;
			StunTimeMax = template.StunTimeMax;
			CanBeGrabbed = template.CanBeGrabbed;
			CanBeSevered = template.CanBeSevered;
			CanBeStabbed = template.CanBeStabbed;
			CanBeSurpressed = template.CanBeSurpressed;
			SuppressionMult = template.SuppressionMult;
			DoesJointBreakKill_Head = template.DoesJointBreakKill_Head;
			DoesJointBreakKill_Upper = template.DoesJointBreakKill_Upper;
			DoesJointBreakKill_Lower = template.DoesJointBreakKill_Lower;
			DoesSeverKill_Head = template.DoesSeverKill_Head;
			DoesSeverKill_Upper = template.DoesSeverKill_Upper;
			DoesSeverKill_Lower = template.DoesSeverKill_Lower;
			DoesExplodeKill_Head = template.DoesExplodeKill_Head;
			DoesExplodeKill_Upper = template.DoesExplodeKill_Upper;
			DoesExplodeKill_Lower = template.DoesExplodeKill_Lower;

			this.template = template;
        }

		public SosigConfigTemplate GetConfigTemplate()
        {
			if(template == null)
            {
				template = (SosigConfigTemplate)ScriptableObject.CreateInstance(typeof(SosigConfigTemplate));

				template.ViewDistance = ViewDistance;
				template.HearingDistance = HearingDistance;
				template.MaxFOV = MaxFOV;
				template.SearchExtentsModifier = SearchExtentsModifier;
				template.DoesAggroOnFriendlyFire = DoesAggroOnFriendlyFire;
				template.HasABrain = HasABrain;
				template.DoesDropWeaponsOnBallistic = DoesDropWeaponsOnBallistic;
				template.CanPickup_Ranged = CanPickupRanged;
				template.CanPickup_Melee = CanPickupMelee;
				template.CanPickup_Other = CanPickupOther;
				template.TargetCapacity = TargetCapacity;
				template.TargetTrackingTime = TargetTrackingTime;
				template.NoFreshTargetTime = NoFreshTargetTime;
				template.AssaultPointOverridesSkirmishPointWhenFurtherThan = AssaultPointOverridesSkirmishPointWhenFurtherThan;
				template.RunSpeed = RunSpeed;
				template.WalkSpeed = WalkSpeed;
				template.SneakSpeed = SneakSpeed;
				template.CrawlSpeed = CrawlSpeed;
				template.TurnSpeed = TurnSpeed;
				template.MaxJointLimit = MaxJointLimit;
				template.MovementRotMagnitude = MovementRotMagnitude;
				template.TotalMustard =	TotalMustard;
				template.BleedDamageMult = BleedDamageMult;
				template.BleedRateMultiplier = BleedRateMultiplier;
				template.BleedVFXIntensity = BleedVFXIntensity;
				template.DamMult_Projectile = DamMult_Projectile;
				template.DamMult_Explosive = DamMult_Explosive;
				template.DamMult_Melee = DamMult_Melee;
				template.DamMult_Piercing = DamMult_Piercing;
				template.DamMult_Blunt = DamMult_Blunt;
				template.DamMult_Cutting = DamMult_Cutting;
				template.DamMult_Thermal = DamMult_Thermal;
				template.DamMult_Chilling = DamMult_Chilling;
				template.DamMult_EMP = DamMult_EMP;
				template.LinkDamageMultipliers = LinkDamageMultipliers;
				template.LinkStaggerMultipliers = LinkStaggerMultipliers;
				template.StartingLinkIntegrity = StartingLinkIntegrity.Select(o => o.GetVector2()).ToList();
				template.StartingChanceBrokenJoint = StartingChanceBrokenJoint;
				template.ShudderThreshold = ShudderThreshold;
				template.ConfusionThreshold = ConfusionThreshold;
				template.ConfusionMultiplier = ConfusionMultiplier;
				template.ConfusionTimeMax = ConfusionTimeMax;
				template.StunThreshold = StunThreshold;
				template.StunMultiplier = StunMultiplier;
				template.StunTimeMax = StunTimeMax;
				template.CanBeGrabbed = CanBeGrabbed;
				template.CanBeSevered = CanBeSevered;
				template.CanBeStabbed = CanBeStabbed;
				template.CanBeSurpressed = CanBeSurpressed;
				template.SuppressionMult = SuppressionMult;
				template.DoesJointBreakKill_Head = DoesJointBreakKill_Head;
				template.DoesJointBreakKill_Upper = DoesJointBreakKill_Upper;
				template.DoesJointBreakKill_Lower = DoesJointBreakKill_Lower;
				template.DoesSeverKill_Head = DoesSeverKill_Head;
				template.DoesSeverKill_Upper = DoesSeverKill_Upper;
				template.DoesSeverKill_Lower = DoesSeverKill_Lower;
				template.DoesExplodeKill_Head = DoesExplodeKill_Head;
				template.DoesExplodeKill_Upper = DoesExplodeKill_Upper;
				template.DoesExplodeKill_Lower = DoesExplodeKill_Lower;

				template.UsesLinkSpawns = false;
				template.LinkSpawns = new List<FVRObject>();
				template.LinkSpawnChance = new List<float>();
				template.OverrideSpeech = false;
            }

			return template;
        }

	}

    public class OutfitConfig
    {
		public List<string> Headwear;
		public float Chance_Headwear;
		public bool ForceWearAllHead;
		public List<string> Eyewear;
		public float Chance_Eyewear;
		public bool ForceWearAllEye;
		public List<string> Facewear;
		public float Chance_Facewear;
		public bool ForceWearAllFace;
		public List<string> Torsowear;
		public float Chance_Torsowear;
		public bool ForceWearAllTorso;
		public List<string> Pantswear;
		public float Chance_Pantswear;
		public bool ForceWearAllPants;
		public List<string> Pantswear_Lower;
		public float Chance_Pantswear_Lower;
		public bool ForceWearAllPantsLower;
		public List<string> Backpacks;
		public float Chance_Backpacks;
		public bool ForceWearAllBackpacks;

		[JsonIgnore]
		private SosigOutfitConfig template;

		public OutfitConfig() { }

		public OutfitConfig(SosigOutfitConfig template)
        {
			Headwear = template.Headwear.Select(o => o.ItemID).ToList();
			Eyewear = template.Eyewear.Select(o => o.ItemID).ToList();
			Facewear = template.Facewear.Select(o => o.ItemID).ToList();
			Torsowear = template.Torsowear.Select(o => o.ItemID).ToList();
			Pantswear = template.Pantswear.Select(o => o.ItemID).ToList();
			Pantswear_Lower = template.Pantswear_Lower.Select(o => o.ItemID).ToList();
			Backpacks = template.Backpacks.Select(o => o.ItemID).ToList();
			Chance_Headwear = template.Chance_Headwear;
			Chance_Eyewear = template.Chance_Eyewear;
			Chance_Facewear = template.Chance_Facewear;
			Chance_Torsowear = template.Chance_Torsowear;
			Chance_Pantswear = template.Chance_Pantswear;
			Chance_Pantswear_Lower = template.Chance_Pantswear_Lower;
			Chance_Backpacks = template.Chance_Backpacks;

			this.template = template;
        }

		public SosigOutfitConfig GetOutfitConfig()
        {
			if(template == null)
            {
				template = (SosigOutfitConfig)ScriptableObject.CreateInstance(typeof(SosigOutfitConfig));
				
				template.Chance_Headwear = Chance_Headwear;
				template.Chance_Eyewear = Chance_Eyewear;
				template.Chance_Facewear = Chance_Facewear;
				template.Chance_Torsowear = Chance_Torsowear;
				template.Chance_Pantswear = Chance_Pantswear;
				template.Chance_Pantswear_Lower = Chance_Pantswear_Lower;
				template.Chance_Backpacks = Chance_Backpacks;
			}

			return template;
        }

		public void DelayedInit()
        {
			template.Headwear = Headwear.Select(o => IM.OD[o]).ToList();
			template.Eyewear = Eyewear.Select(o => IM.OD[o]).ToList();
			template.Facewear = Facewear.Select(o => IM.OD[o]).ToList();
			template.Torsowear = Torsowear.Select(o => IM.OD[o]).ToList();
			template.Pantswear = Pantswear.Select(o => IM.OD[o]).ToList();
			template.Pantswear_Lower = Pantswear_Lower.Select(o => IM.OD[o]).ToList();
			template.Backpacks = Backpacks.Select(o => IM.OD[o]).ToList();
		}

	}
}
