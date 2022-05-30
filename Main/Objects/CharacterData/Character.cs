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

	/// <summary>
	/// Custom character class
	/// </summary>
	[CreateAssetMenu(menuName = "TNHTweaker/Character", fileName = "NewCharacter")]
	public class Character : ScriptableObject
    {
		/// <summary> Name of character that is displayed in TNH Menu </summary>
		[Header("Character Identity Properties")]
		public string DisplayName;
		/// <summary> A unique itentifier for character </summary>
		public TNH_Char CharacterID;
		/// <summary> The character category that the character will appear in. If a unique group name is entered here, a new category will be created </summary>
		public string Group;
		/// <summary> Determines the scoreboard that this character will end up. Usually should be unique to the character </summary>
		public string TableID;
		/// <summary> The icon that gets displayed for the character in the TNH menu </summary>
		public Sprite Picture;
		/// <summary> Description text that will appear below the character in the TNH menu </summary>
		public string Description;

		/// <summary> Number of tokens given to the player that the start of the game </summary>
		[Header("Gameplay Properties")]
		public int StartingTokens;
		/// <summary> When true, sosigs will always spawn with all of there equipment </summary>
		public bool ForceAllAgentWeapons;
		/// <summary> When true, purchasing items at an object constructor will increase the cost of the selected pool each time </summary>
		public bool UsesPurchasePriceIncrement = true;

		/// <summary> A group of items from which a random item will be spawned from when an item that requires sights is purchased </summary>
		[Header("Loot Properties")]
		public EquipmentGroup RequireSightTable;
		/// <summary> List of eras determining what type of ammo can spawn </summary>
		public List<FVRObject.OTagEra> ValidAmmoEras = new List<FVRObject.OTagEra>();
		/// <summary> List of sets determining what type of ammo can spawn </summary>
		public List<FVRObject.OTagSet> ValidAmmoSets = new List<FVRObject.OTagSet>();
		/// <summary> A collection of EquipmentPools that can appear in the object constructor </summary>
		public List<EquipmentPool> EquipmentPools = new List<EquipmentPool>();

		/// <summary> A collection of progressions which the character could play through on standard length progression setting </summary>
		[Header("Progression Properties")]
		public List<Progression> Progressions = new List<Progression>();
		/// <summary> A collection of progressions which the character could play through on endless length progression setting </summary>
		public List<Progression> Progressions_Endless = new List<Progression>();

		/// <summary> When true, a primary weapon will spawn at the start of TNH </summary>
		[Header("Starting Loadout Properties")]
		public bool Has_Weapon_Primary;
		/// <summary> When true, a secondary weapon will spawn at the start of TNH </summary>
		public bool Has_Weapon_Secondary;
		/// <summary> When true, a tertiary weapon will spawn at the start of TNH </summary>
		public bool Has_Weapon_Tertiary;
		/// <summary> When true, a primary item will spawn at the start of TNH </summary>
		public bool Has_Item_Primary;
		/// <summary> When true, a secondar item will spawn at the start of TNH </summary>
		public bool Has_Item_Secondary;
		/// <summary> When true, a tertiary item will spawn at the start of TNH </summary>
		public bool Has_Item_Tertiary;
		/// <summary> When true, a shield will spawn at the start of TNH </summary>
		public bool Has_Item_Shield;
		/// <summary> A pool of possible starting primary weapons (spawns in large crate) </summary>
		public LoadoutEntry Weapon_Primary;
		/// <summary> A pool of possible starting secondary weapons (spawns in small crate) </summary>
		public LoadoutEntry Weapon_Secondary;
		/// <summary> A pool of possible starting tertiary weapons </summary>
		public LoadoutEntry Weapon_Tertiary;
		/// <summary> A pool of possible starting primary items </summary>
		public LoadoutEntry Item_Primary;
		/// <summary> A pool of possible starting secondary items </summary>
		public LoadoutEntry Item_Secondary;
		/// <summary> A pool of possible starting tertiary items </summary>
		public LoadoutEntry Item_Tertiary;
		/// <summary> A pool of possible starting shields </summary>
		public LoadoutEntry Item_Shield;
	}
 
}
