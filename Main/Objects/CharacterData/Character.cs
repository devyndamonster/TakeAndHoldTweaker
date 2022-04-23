﻿using FistVR;
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
    public class Character : ScriptableObject
    {
		[Header("Character Identity Properties")]
		public string DisplayName;
		public TNH_Char CharacterID;
		public TNH_CharacterDef.CharacterGroup Group;
		public string TableID;
		public Sprite Picture;
		public string Description;

		[Header("Gameplay Properties")]
		public int StartingTokens;
		public bool ForceAllAgentWeapons;
		public bool UsesPurchasePriceIncrement = true;

		[Header("Loot Properties")]
		public ObjectTableDef RequireSightTable;
		public List<FVRObject.OTagEra> ValidAmmoEras;
		public List<FVRObject.OTagSet> ValidAmmoSets;
		public List<EquipmentPool> EquipmentPools;

		[Header("Progression Properties")]
		public List<TNH_Progression> Progressions;
		public List<TNH_Progression> Progressions_Endless;

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
