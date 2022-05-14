using LegacyCharacterLoader.Objects.Serializable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegacyCharacterLoader.Objects.SosigData
{
    public class LegacySosigConfig
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
	}
}
