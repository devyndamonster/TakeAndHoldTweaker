using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TNHTweaker.Objects.SosigData
{
	public class OutfitConfig : ScriptableObject
	{
		public List<string> Headwear = new List<string>();
		public float Chance_Headwear;
		public List<string> Eyewear = new List<string>();
		public float Chance_Eyewear;
		public List<string> Facewear = new List<string>();
		public float Chance_Facewear;
		public List<string> Torsowear = new List<string>();
		public float Chance_Torsowear;
		public List<string> Pantswear = new List<string>();
		public float Chance_Pantswear;
		public List<string> Pantswear_Lower = new List<string>();
		public float Chance_Pantswear_Lower;
		public List<string> Backpacks = new List<string>();
		public float Chance_Backpacks;
		public List<string> TorsoDecoration = new List<string>();
		public float Chance_TorosDecoration;
		public List<string> Belts = new List<string>();
		public float Chance_Belts;

		public bool HeadUsesTorsoIndex;
		public bool PantsUsesTorsoIndex;
		public bool PantsLowerUsesPantsIndex;
	}
}
