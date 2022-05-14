using LegacyCharacterLoader.Objects.SosigData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.Objects.SosigData;
using UnityEngine;

namespace LegacyCharacterLoader.LegacyConverters
{
    public static class LegacyOutfitConfigConverter
    {
		public static OutfitConfig ConvertOutfitConfigFromLegacy(LegacyOutfitConfig from)
		{
			OutfitConfig outfitConfig = ScriptableObject.CreateInstance<OutfitConfig>();

			outfitConfig.Headwear = from.Headwear;
			outfitConfig.Chance_Headwear = from.Chance_Headwear;
			outfitConfig.Eyewear = from.Eyewear;
			outfitConfig.Chance_Eyewear = from.Chance_Eyewear;
			outfitConfig.Facewear = from.Facewear;
			outfitConfig.Chance_Facewear = from.Chance_Facewear;
			outfitConfig.Torsowear = from.Torsowear;
			outfitConfig.Chance_Torsowear = from.Chance_Torsowear;
			outfitConfig.Pantswear = from.Pantswear;
			outfitConfig.Chance_Pantswear = from.Chance_Pantswear;
			outfitConfig.Pantswear_Lower = from.Pantswear_Lower;
			outfitConfig.Chance_Pantswear_Lower = from.Chance_Pantswear_Lower;
			outfitConfig.Backpacks = from.Backpacks;
			outfitConfig.Chance_Backpacks = from.Chance_Backpacks;

			return outfitConfig;
		}
	}
}
