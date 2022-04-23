using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TNHTweaker.Objects.SosigData
{
	public class OutfitConfig : ScriptableObject
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
	}
}
