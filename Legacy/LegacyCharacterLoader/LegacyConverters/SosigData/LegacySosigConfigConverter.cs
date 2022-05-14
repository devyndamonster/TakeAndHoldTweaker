using FistVR;
using LegacyCharacterLoader.Objects.SosigData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LegacyCharacterLoader.LegacyConverters
{
    public static class LegacySosigConfigConverter
    {
		public static SosigConfigTemplate ConvertSosigConfigFromLegacy(LegacySosigConfig from)
		{
			SosigConfigTemplate sosigConfig = ScriptableObject.CreateInstance<SosigConfigTemplate>();

			sosigConfig.ViewDistance = from.ViewDistance;
			sosigConfig.HearingDistance = from.HearingDistance;
			sosigConfig.MaxFOV = from.MaxFOV;
			sosigConfig.SearchExtentsModifier = from.SearchExtentsModifier;
			sosigConfig.DoesAggroOnFriendlyFire = from.DoesAggroOnFriendlyFire;
			sosigConfig.HasABrain = from.HasABrain;
			sosigConfig.DoesDropWeaponsOnBallistic = from.DoesDropWeaponsOnBallistic;
			sosigConfig.CanPickup_Ranged = from.CanPickupRanged;
			sosigConfig.CanPickup_Melee = from.CanPickupMelee;
			sosigConfig.CanPickup_Other = from.CanPickupOther;
			sosigConfig.TargetCapacity = from.TargetCapacity;
			sosigConfig.TargetTrackingTime = from.TargetTrackingTime;
			sosigConfig.NoFreshTargetTime = from.NoFreshTargetTime;
			sosigConfig.AssaultPointOverridesSkirmishPointWhenFurtherThan = from.AssaultPointOverridesSkirmishPointWhenFurtherThan;
			sosigConfig.RunSpeed = from.RunSpeed;
			sosigConfig.WalkSpeed = from.WalkSpeed;
			sosigConfig.SneakSpeed = from.SneakSpeed;
			sosigConfig.CrawlSpeed = from.CrawlSpeed;
			sosigConfig.TurnSpeed = from.TurnSpeed;
			sosigConfig.MaxJointLimit = from.MaxJointLimit;
			sosigConfig.MovementRotMagnitude = from.MovementRotMagnitude;
			sosigConfig.TotalMustard = from.TotalMustard;
			sosigConfig.BleedDamageMult = from.BleedDamageMult;
			sosigConfig.BleedRateMultiplier = from.BleedRateMultiplier;
			sosigConfig.BleedVFXIntensity = from.BleedVFXIntensity;
			sosigConfig.DamMult_Projectile = from.DamMult_Projectile;
			sosigConfig.DamMult_Explosive = from.DamMult_Explosive;
			sosigConfig.DamMult_Melee = from.DamMult_Melee;
			sosigConfig.DamMult_Piercing = from.DamMult_Piercing;
			sosigConfig.DamMult_Blunt = from.DamMult_Blunt;
			sosigConfig.DamMult_Cutting = from.DamMult_Cutting;
			sosigConfig.DamMult_Thermal = from.DamMult_Thermal;
			sosigConfig.DamMult_Chilling = from.DamMult_Chilling;
			sosigConfig.DamMult_EMP = from.DamMult_EMP;
			sosigConfig.LinkDamageMultipliers = from.LinkDamageMultipliers;
			sosigConfig.LinkStaggerMultipliers = from.LinkStaggerMultipliers;
			sosigConfig.StartingLinkIntegrity = from.StartingLinkIntegrity.Select(o => o.GetVector2()).ToList();
			sosigConfig.StartingChanceBrokenJoint = from.StartingChanceBrokenJoint;
			sosigConfig.ShudderThreshold = from.ShudderThreshold;
			sosigConfig.ConfusionThreshold = from.ConfusionThreshold;
			sosigConfig.ConfusionMultiplier = from.ConfusionMultiplier;
			sosigConfig.ConfusionTimeMax = from.ConfusionTimeMax;
			sosigConfig.StunThreshold = from.StunThreshold;
			sosigConfig.StunMultiplier = from.StunMultiplier;
			sosigConfig.StunTimeMax = from.StunTimeMax;
			sosigConfig.CanBeGrabbed = from.CanBeGrabbed;
			sosigConfig.CanBeSevered = from.CanBeSevered;
			sosigConfig.CanBeStabbed = from.CanBeStabbed;
			sosigConfig.CanBeSurpressed = from.CanBeSurpressed;
			sosigConfig.SuppressionMult = from.SuppressionMult;
			sosigConfig.DoesJointBreakKill_Head = from.DoesJointBreakKill_Head;
			sosigConfig.DoesJointBreakKill_Upper = from.DoesJointBreakKill_Upper;
			sosigConfig.DoesJointBreakKill_Lower = from.DoesJointBreakKill_Lower;
			sosigConfig.DoesSeverKill_Head = from.DoesSeverKill_Head;
			sosigConfig.DoesSeverKill_Upper = from.DoesSeverKill_Upper;
			sosigConfig.DoesSeverKill_Lower = from.DoesSeverKill_Lower;
			sosigConfig.DoesExplodeKill_Head = from.DoesExplodeKill_Head;
			sosigConfig.DoesExplodeKill_Upper = from.DoesExplodeKill_Upper;
			sosigConfig.DoesExplodeKill_Lower = from.DoesExplodeKill_Lower;

			return sosigConfig;
		}
	}
}
