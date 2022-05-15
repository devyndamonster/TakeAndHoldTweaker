using FistVR;
using MagazinePatcher;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Policy;
using System.Text;
using TNHTweaker.Objects.LootPools;
using TNHTweaker.Utilities;
using UnityEngine;

namespace TNHTweaker.Objects.CharacterData
{

	[CreateAssetMenu(menuName = "TNHTweaker/Character", fileName = "NewCharacter")]
	public class Character : ScriptableObject
    {
		[Header("Character Identity Properties")]
		public string DisplayName;
		public TNH_Char CharacterID;
		public string Group;
		public string TableID;
		public Sprite Picture;
		public string Description;

		[Header("Gameplay Properties")]
		public int StartingTokens;
		public bool ForceAllAgentWeapons;
		public bool UsesPurchasePriceIncrement = true;

		[Header("Loot Properties")]
		public EquipmentGroup RequireSightTable;
		public List<FVRObject.OTagEra> ValidAmmoEras = new List<FVRObject.OTagEra>();
		public List<FVRObject.OTagSet> ValidAmmoSets = new List<FVRObject.OTagSet>();
		public List<EquipmentPool> EquipmentPools = new List<EquipmentPool>();

		[Header("Progression Properties")]
		public List<Progression> Progressions = new List<Progression>();
		public List<Progression> Progressions_Endless = new List<Progression>();

		[Header("Starting Loadout Properties")]
		public bool Has_Weapon_Primary;
		public bool Has_Weapon_Secondary;
		public bool Has_Weapon_Tertiary;
		public bool Has_Item_Primary;
		public bool Has_Item_Secondary;
		public bool Has_Item_Tertiary;
		public bool Has_Item_Shield;
		public LoadoutEntry Weapon_Primary;
		public LoadoutEntry Weapon_Secondary;
		public LoadoutEntry Weapon_Tertiary;
		public LoadoutEntry Item_Primary;
		public LoadoutEntry Item_Secondary;
		public LoadoutEntry Item_Tertiary;
		public LoadoutEntry Item_Shield;
	}
 
}
