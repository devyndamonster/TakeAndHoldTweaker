using BepInEx.Logging;
using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.Utilities;
using UnityEngine;

namespace TNHTweaker.Objects.SosigData
{
    public class SosigTemplate : ScriptableObject
    {
        public string DisplayName;
        public SosigEnemyCategory SosigEnemyCategory;
        public string SosigEnemyID;
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

        public SosigTemplate() { }

        public SosigTemplate(SosigEnemyTemplate template) {

            DisplayName = template.DisplayName;
            SosigEnemyCategory = template.SosigEnemyCategory;
            SosigEnemyID = template.SosigEnemyID.ToString();
            SecondaryChance = template.SecondaryChance;
            TertiaryChance = template.TertiaryChance;

            SosigPrefabs = template.SosigPrefabs.Select(o => o.ItemID).ToList();
            WeaponOptions = template.WeaponOptions.Select(o => o.ItemID).ToList();
            WeaponOptionsSecondary = template.WeaponOptions_Secondary.Select(o => o.ItemID).ToList();
            WeaponOptionsTertiary = template.WeaponOptions_Tertiary.Select(o => o.ItemID).ToList();

			Configs = template.ConfigTemplates.Select(o => new SosigConfig(o)).ToList();
			ConfigsEasy = template.ConfigTemplates_Easy.Select(o => new SosigConfig(o)).ToList();
			OutfitConfigs = template.OutfitConfig.Select(o => new OutfitConfig(o)).ToList();

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
		public List<Vector2> StartingLinkIntegrity;
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
			//StartingLinkIntegrity = template.StartingLinkIntegrity.Select(o => new Vector2(o)).ToList();
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

        }
	}
}
