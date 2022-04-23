using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.Objects.CharacterData;
using TNHTweaker.Objects.LootPools;
using TNHTweaker.Objects.SosigData;
using UnityEngine;

namespace TNHTweaker.ObjectConverters
{
	public static class OutfitConfigConverter
	{
		public static OutfitConfig ConvertOutfitConfigFromVanilla(SosigOutfitConfig from)
		{
			OutfitConfig outfitConfig = ScriptableObject.CreateInstance<OutfitConfig>();

			outfitConfig.Headwear = from.Headwear.Select(o => o.ItemID).ToList();
			outfitConfig.Chance_Headwear = from.Chance_Headwear;
			outfitConfig.Eyewear = from.Eyewear.Select(o => o.ItemID).ToList();
			outfitConfig.Chance_Eyewear = from.Chance_Eyewear;
			outfitConfig.Facewear = from.Facewear.Select(o => o.ItemID).ToList();
			outfitConfig.Chance_Facewear = from.Chance_Facewear;
			outfitConfig.Torsowear = from.Torsowear.Select(o => o.ItemID).ToList();
			outfitConfig.Chance_Torsowear = from.Chance_Torsowear;
			outfitConfig.Pantswear = from.Pantswear.Select(o => o.ItemID).ToList();
			outfitConfig.Chance_Pantswear = from.Chance_Pantswear;
			outfitConfig.Pantswear_Lower = from.Pantswear_Lower.Select(o => o.ItemID).ToList();
			outfitConfig.Chance_Pantswear_Lower = from.Chance_Pantswear_Lower;
			outfitConfig.Backpacks = from.Backpacks.Select(o => o.ItemID).ToList();
			outfitConfig.Chance_Backpacks = from.Chance_Backpacks;

			return outfitConfig;
		}


		public static SosigOutfitConfig ConvertOutfitConfigToVanilla(OutfitConfig from)
		{
			SosigOutfitConfig outfitConfig = ScriptableObject.CreateInstance<SosigOutfitConfig>();

			outfitConfig.Headwear = from.Headwear.Select(o => IM.OD[o]).ToList();
			outfitConfig.Chance_Headwear = from.Chance_Headwear;
			outfitConfig.Eyewear = from.Eyewear.Select(o => IM.OD[o]).ToList();
			outfitConfig.Chance_Eyewear = from.Chance_Eyewear;
			outfitConfig.Facewear = from.Facewear.Select(o => IM.OD[o]).ToList();
			outfitConfig.Chance_Facewear = from.Chance_Facewear;
			outfitConfig.Torsowear = from.Torsowear.Select(o => IM.OD[o]).ToList();
			outfitConfig.Chance_Torsowear = from.Chance_Torsowear;
			outfitConfig.Pantswear = from.Pantswear.Select(o => IM.OD[o]).ToList();
			outfitConfig.Chance_Pantswear = from.Chance_Pantswear;
			outfitConfig.Pantswear_Lower = from.Pantswear_Lower.Select(o => IM.OD[o]).ToList();
			outfitConfig.Chance_Pantswear_Lower = from.Chance_Pantswear_Lower;
			outfitConfig.Backpacks = from.Backpacks.Select(o => IM.OD[o]).ToList();
			outfitConfig.Chance_Backpacks = from.Chance_Backpacks;

			return outfitConfig;
		}
	}
}
