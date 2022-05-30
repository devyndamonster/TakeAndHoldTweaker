using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TNHTweaker.Objects.LootPools
{
	[CreateAssetMenu(menuName = "TNHTweaker/ObjectTable", fileName = "NewObjectTable")]
	public class ObjectTable : ScriptableObject
	{
		[Header("General Tags")]
		public FVRObject.ObjectCategory Category;
		public List<FVRObject.OTagEra> Eras = new List<FVRObject.OTagEra>();
		public List<FVRObject.OTagSet> Sets = new List<FVRObject.OTagSet>();
		public List<FVRObject.OTagFirearmCountryOfOrigin> CountriesOfOrigin = new List<FVRObject.OTagFirearmCountryOfOrigin>();
		public int EarliestYear = -1;
		public int LatestYear = -1;

		[Header("Firearm Tags")]
		public List<FVRObject.OTagFirearmSize> Sizes = new List<FVRObject.OTagFirearmSize>();
		public List<FVRObject.OTagFirearmAction> Actions = new List<FVRObject.OTagFirearmAction>();
		public List<FVRObject.OTagFirearmFiringMode> Modes = new List<FVRObject.OTagFirearmFiringMode>();
		public List<FVRObject.OTagFirearmFiringMode> ExcludedModes = new List<FVRObject.OTagFirearmFiringMode>();
		public List<FVRObject.OTagFirearmFeedOption> FeedOptions = new List<FVRObject.OTagFirearmFeedOption>();
		public List<FVRObject.OTagFirearmMount> MountsAvailable = new List<FVRObject.OTagFirearmMount>();
		public List<FVRObject.OTagFirearmRoundPower> RoundPowers = new List<FVRObject.OTagFirearmRoundPower>();

		[Header("Attachment Tags")]
		public List<FVRObject.OTagAttachmentFeature> Features = new List<FVRObject.OTagAttachmentFeature>();
		public List<FVRObject.OTagFirearmMount> MountTypes = new List<FVRObject.OTagFirearmMount>();

		[Header("Melee Tags")]
		public List<FVRObject.OTagMeleeStyle> MeleeStyles = new List<FVRObject.OTagMeleeStyle>();
		public List<FVRObject.OTagMeleeHandedness> MeleeHandedness = new List<FVRObject.OTagMeleeHandedness>();
		public List<FVRObject.OTagThrownType> ThrownTypes = new List<FVRObject.OTagThrownType>();
		public List<FVRObject.OTagThrownDamageType> ThrownDamageTypes = new List<FVRObject.OTagThrownDamageType>();

		[Header("Misc Tags")]
		public List<FVRObject.OTagPowerupType> PowerupTypes = new List<FVRObject.OTagPowerupType>();

		[Header("Ammo Properties")]
		public int MinAmmoCapacity = -1;
		public int MaxAmmoCapacity = -1;
		public bool OverrideMagType;
		public bool OverrideRoundType;
		public FireArmMagazineType MagTypeOverride;
		public FireArmRoundType RoundTypeOverride;

		[Header("Misc Properties")]
		public bool AutoPopulatePools;
		public List<string> WhitelistedObjectIDs = new List<string>();
		public List<string> BlacklistedObjectIDs = new List<string>();

		[NonSerialized]
		public List<FVRObject> GeneratedObjects;

		public bool HasItems()
        {
			return GeneratedObjects != null && GeneratedObjects.Count > 0;
        }

		public void GenerateTable()
		{
			GeneratedObjects = new List<FVRObject>();

			foreach (FVRObject fvr in IM.OD.Values)
			{
				if (WhitelistedObjectIDs.Contains(fvr.ItemID))
				{
					GeneratedObjects.Add(fvr);
					continue;
				}

				if (!AutoPopulatePools) continue;

				if (BlacklistedObjectIDs.Contains(fvr.ItemID)) continue;

				if (!fvr.OSple) continue;

				if (fvr.Category != Category) continue;

				if (Eras.Count > 0 && !Eras.Contains(fvr.TagEra)) continue;

				if (Sets.Count > 0 && !Sets.Contains(fvr.TagSet)) continue;

				if (CountriesOfOrigin.Count > 0 && !CountriesOfOrigin.Contains(fvr.TagFirearmCountryOfOrigin)) continue;

				if (EarliestYear > -1 && fvr.TagFirearmFirstYear < EarliestYear) continue;

				if (LatestYear > -1 && fvr.TagFirearmFirstYear > LatestYear) continue;

				if (Sizes.Count > 0 && !Sizes.Contains(fvr.TagFirearmSize)) continue;

				if (Actions.Count > 0 && !Actions.Contains(fvr.TagFirearmAction)) continue;

				if (Modes.Count > 0 && !Modes.Any(o => fvr.TagFirearmFiringModes.Contains(o))) continue;

				if (ExcludedModes.Count > 0 && ExcludedModes.Any(o => fvr.TagFirearmFiringModes.Contains(o))) continue;

				if (FeedOptions.Count > 0 && !FeedOptions.Any(o => fvr.TagFirearmFeedOption.Contains(o))) continue;

				if (MountsAvailable.Count > 0 && !MountsAvailable.Any(o => fvr.TagFirearmMounts.Contains(o))) continue;

				if (RoundPowers.Count > 0 && !RoundPowers.Contains(fvr.TagFirearmRoundPower)) continue;

				if (Features.Count > 0 && !Features.Contains(fvr.TagAttachmentFeature)) continue;

				if (MountTypes.Count > 0 && !MountTypes.Contains(fvr.TagAttachmentMount)) continue;

				if (MeleeStyles.Count > 0 && !MeleeStyles.Contains(fvr.TagMeleeStyle)) continue;

				if (MeleeHandedness.Count > 0 && !MeleeHandedness.Contains(fvr.TagMeleeHandedness)) continue;

				if (ThrownTypes.Count > 0 && !ThrownTypes.Contains(fvr.TagThrownType)) continue;

				if (ThrownDamageTypes.Count > 0 && !ThrownDamageTypes.Contains(fvr.TagThrownDamageType)) continue;

				if (PowerupTypes.Count > 0 && !PowerupTypes.Contains(fvr.TagPowerupType)) continue;

				if (MinAmmoCapacity > -1 && fvr.MaxCapacityRelated < MinAmmoCapacity) continue;

				if (MaxAmmoCapacity > -1 && fvr.MinCapacityRelated > MaxAmmoCapacity) continue;

				if (OverrideMagType && fvr.MagazineType != MagTypeOverride) continue;

				if (OverrideRoundType && fvr.RoundType != RoundTypeOverride) continue;

				GeneratedObjects.Add(fvr);
			}
		}

        public override string ToString()
        {
            return $"\nObject Table - " +
				   $"\nNum objects = {GeneratedObjects.Count}" +
                   $"\nContents:\n{string.Join("\n", GeneratedObjects.Select(o => "-" + o.ItemID).ToArray())}";
        }
    }
}
