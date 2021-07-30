using FistVR;
using MagazinePatcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TNHTweaker.Utilities
{
    static class FirearmUtils
    {

		/// <summary>
		/// Returns a list of magazines, clips, or speedloaders compatible with the firearm, and also within any of the optional criteria
		/// </summary>
		/// <param name="firearm">The FVRObject of the firearm</param>
		/// <param name="minCapacity">The minimum capacity for desired containers</param>
		/// <param name="maxCapacity">The maximum capacity for desired containers. If this values is zero or negative, it is interpreted as no capacity ceiling</param>
		/// <param name="smallestIfEmpty">If true, when the returned list would normally be empty, will instead return the smallest capacity magazine compatible with the firearm</param>
		/// <param name="blacklistedContainers">A list of ItemIDs for magazines, clips, or speedloaders that will be excluded</param>
		/// <returns> A list of ammo container FVRObjects that are compatible with the given firearm </returns>
		public static List<FVRObject> GetCompatibleAmmoContainers(FVRObject firearm, int minCapacity = 0, int maxCapacity = 9999, bool smallestIfEmpty = true, MagazineBlacklistEntry blacklist = null)
		{
			//Refresh the FVRObject to have data directly from object dictionary
			firearm = IM.OD[firearm.ItemID];

			//If the max capacity is zero or negative, we iterpret that as no limit on max capacity
			if (maxCapacity <= 0) maxCapacity = 9999;

			//Create a list containing all compatible ammo containers
			List<FVRObject> compatibleContainers = new List<FVRObject>();
			if (firearm.CompatibleSpeedLoaders is not null) compatibleContainers.AddRange(firearm.CompatibleSpeedLoaders);


			//Go through each magazine and add compatible ones
			foreach(FVRObject magazine in firearm.CompatibleMagazines)
			{
				if (blacklist is not null && (!blacklist.IsMagazineAllowed(magazine.ItemID)))
				{
					continue;
				}

				else if (magazine.MagazineCapacity < minCapacity || magazine.MagazineCapacity > maxCapacity)
				{
					continue;
				}

				compatibleContainers.Add(magazine);
			}


			//Go through each magazine and add compatible ones
			foreach (FVRObject clip in firearm.CompatibleClips)
			{
				if (blacklist is not null && (!blacklist.IsClipAllowed(clip.ItemID)))
				{
					continue;
				}

				else if (clip.MagazineCapacity < minCapacity || clip.MagazineCapacity > maxCapacity)
				{
					continue;
				}

				compatibleContainers.Add(clip);
			}


			//If the resulting list is empty, and smallestIfEmpty is true, add the smallest capacity magazine to the list
			if (compatibleContainers.Count == 0 && smallestIfEmpty && firearm.CompatibleMagazines is not null)
			{
				FVRObject magazine = GetSmallestCapacityMagazine(firearm.CompatibleMagazines);
				if (magazine is not null) compatibleContainers.Add(magazine);
			}

			return compatibleContainers;
		}




		public static List<FVRObject> GetCompatibleMagazines(FVRObject firearm, int minCapacity = 0, int maxCapacity = 9999, bool smallestIfEmpty = true, MagazineBlacklistEntry blacklist = null)
		{
			//Refresh the FVRObject to have data directly from object dictionary
			firearm = IM.OD[firearm.ItemID];

			//If the max capacity is zero or negative, we iterpret that as no limit on max capacity
			if (maxCapacity <= 0) maxCapacity = 9999;

			//Create a list containing all compatible ammo containers
			List<FVRObject> compatibleMagazines = new List<FVRObject>();
			if (firearm.CompatibleMagazines is not null) compatibleMagazines.AddRange(firearm.CompatibleMagazines);

			//Go through these containers and remove any that don't fit given criteria
			for (int i = compatibleMagazines.Count - 1; i >= 0; i--)
			{
				if (blacklist is not null && (!blacklist.IsMagazineAllowed(compatibleMagazines[i].ItemID)))
				{
					compatibleMagazines.RemoveAt(i);
				}

				else if (compatibleMagazines[i].MagazineCapacity < minCapacity || compatibleMagazines[i].MagazineCapacity > maxCapacity)
				{
					compatibleMagazines.RemoveAt(i);
				}
			}

			//If the resulting list is empty, and smallestIfEmpty is true, add the smallest capacity magazine to the list
			if (compatibleMagazines.Count == 0 && smallestIfEmpty && firearm.CompatibleMagazines is not null)
			{
				FVRObject magazine = GetSmallestCapacityMagazine(firearm.CompatibleMagazines, blacklist);
				if (magazine is not null) compatibleMagazines.Add(magazine);
			}

			return compatibleMagazines;
		}


		public static List<FVRObject> GetCompatibleClips(FVRObject firearm, int minCapacity = 0, int maxCapacity = 9999, MagazineBlacklistEntry blacklist = null)
		{
			//Refresh the FVRObject to have data directly from object dictionary
			firearm = IM.OD[firearm.ItemID];

			//If the max capacity is zero or negative, we iterpret that as no limit on max capacity
			if (maxCapacity <= 0) maxCapacity = 9999;

			//Create a list containing all compatible ammo containers
			List<FVRObject> compatibleClips = new List<FVRObject>();
			if (firearm.CompatibleClips is not null) compatibleClips.AddRange(firearm.CompatibleClips);

			//Go through these containers and remove any that don't fit given criteria
			for (int i = compatibleClips.Count - 1; i >= 0; i--)
			{
				if (blacklist is not null && (!blacklist.IsClipAllowed(compatibleClips[i].ItemID)))
				{
					compatibleClips.RemoveAt(i);
				}

				else if (compatibleClips[i].MagazineCapacity < minCapacity || compatibleClips[i].MagazineCapacity > maxCapacity)
				{
					compatibleClips.RemoveAt(i);
				}
			}

			return compatibleClips;
		}


		public static List<FVRObject> GetCompatibleRounds(FVRObject firearm, List<FVRObject.OTagEra> eras, List<FVRObject.OTagSet> sets, List<string> globalBulletBlacklist = null, MagazineBlacklistEntry blacklist = null)
		{
			//Refresh the FVRObject to have data directly from object dictionary
			firearm = IM.OD[firearm.ItemID];

			//Create a list containing all compatible ammo containers
			List<FVRObject> compatibleRounds = new List<FVRObject>();
			if (firearm.CompatibleSingleRounds is not null) compatibleRounds.AddRange(firearm.CompatibleSingleRounds);

			//Go through these containers and remove any that don't fit given criteria
			for (int i = compatibleRounds.Count - 1; i >= 0; i--)
			{
				if (blacklist is not null && (!blacklist.IsRoundAllowed(compatibleRounds[i].ItemID)))
				{
					compatibleRounds.RemoveAt(i);
				}

				else if(globalBulletBlacklist is not null && globalBulletBlacklist.Contains(compatibleRounds[i].ItemID))
                {
					compatibleRounds.RemoveAt(i);
                }

				else if(!eras.Contains(compatibleRounds[i].TagEra) || !sets.Contains(compatibleRounds[i].TagSet))
                {
					compatibleRounds.RemoveAt(i);
                }
			}

			return compatibleRounds;
		}


		/// <summary>
		/// Returns the smallest capacity magazine from the given list of magazine FVRObjects
		/// </summary>
		/// <param name="magazines">A list of magazine FVRObjects</param>
		/// <param name="blacklistedMagazines">A list of ItemIDs for magazines that will be excluded</param>
		/// <returns>An FVRObject for the smallest magazine. Can be null if magazines list is empty</returns>
		public static FVRObject GetSmallestCapacityMagazine(List<FVRObject> magazines, MagazineBlacklistEntry blacklist = null)
		{
			if (magazines is null || magazines.Count == 0) return null;

			//This was done with a list because whenever there are multiple smallest magazines of the same size, we want to return a random one from those options
			List<FVRObject> smallestMagazines = new List<FVRObject>();

			foreach (FVRObject magazine in magazines)
			{
				if (blacklist is not null && (!blacklist.IsMagazineAllowed(magazine.ItemID))) continue;

				else if (smallestMagazines.Count == 0) smallestMagazines.Add(magazine);

				//If we find a new smallest mag, clear the list and add the new smallest
				else if (magazine.MagazineCapacity < smallestMagazines[0].MagazineCapacity)
				{
					smallestMagazines.Clear();
					smallestMagazines.Add(magazine);
				}

				//If the magazine is the same capacity as current smallest, add it to the list
				else if (magazine.MagazineCapacity == smallestMagazines[0].MagazineCapacity)
				{
					smallestMagazines.Add(magazine);
				}
			}


			if (smallestMagazines.Count == 0) return null;

			//Return a random magazine from the smallest
			return smallestMagazines.GetRandom();
		}



		/// <summary>
		/// Returns the smallest capacity magazine that is compatible with the given firearm
		/// </summary>
		/// <param name="firearm">The FVRObject of the firearm</param>
		/// <param name="blacklistedMagazines">A list of ItemIDs for magazines that will be excluded</param>
		/// <returns>An FVRObject for the smallest magazine. Can be null if firearm has no magazines</returns>
		public static FVRObject GetSmallestCapacityMagazine(FVRObject firearm, MagazineBlacklistEntry blacklist = null)
		{
			//Refresh the FVRObject to have data directly from object dictionary
			firearm = IM.OD[firearm.ItemID];

			return GetSmallestCapacityMagazine(firearm.CompatibleMagazines, blacklist);
		}



		/// <summary>
		/// Returns true if the given FVRObject has any compatible rounds, clips, magazines, or speedloaders
		/// </summary>
		/// <param name="item">The FVRObject that is being checked</param>
		/// <returns>True if the FVRObject has any compatible rounds, clips, magazines, or speedloaders. False if it contains none of these</returns>
		public static bool FVRObjectHasAmmoObject(FVRObject item)
		{
			if (item == null) return false;

			//Refresh the FVRObject to have data directly from object dictionary
			item = IM.OD[item.ItemID];

			return (item.CompatibleSingleRounds != null && item.CompatibleSingleRounds.Count != 0) || (item.CompatibleClips != null && item.CompatibleClips.Count > 0) || (item.CompatibleMagazines != null && item.CompatibleMagazines.Count > 0) || (item.CompatibleSpeedLoaders != null && item.CompatibleSpeedLoaders.Count != 0);
		}


		/// <summary>
		/// Returns true if the given FVRObject has any compatible clips, magazines, or speedloaders
		/// </summary>
		/// <param name="item">The FVRObject that is being checked</param>
		/// <returns>True if the FVRObject has any compatible clips, magazines, or speedloaders. False if it contains none of these</returns>
		public static bool FVRObjectHasAmmoContainer(FVRObject item)
		{
			if (item == null) return false;

			//Refresh the FVRObject to have data directly from object dictionary
			item = IM.OD[item.ItemID];

			return (item.CompatibleClips != null && item.CompatibleClips.Count > 0) || (item.CompatibleMagazines != null && item.CompatibleMagazines.Count > 0) || (item.CompatibleSpeedLoaders != null && item.CompatibleSpeedLoaders.Count != 0);
		}




		/// <summary>
		/// Returns the next largest magazine when compared to the current magazine. Only magazines from the possibleMagazines list are considered as next largest magazine candidates
		/// </summary>
		/// <param name="currentMagazine">The base magazine FVRObject, for which we are getting the next largest magazine</param>
		/// <param name="possibleMagazines">A list of magazine FVRObjects, which are the candidates for being the next largest magazine</param>
		/// <param name="blacklistedMagazines">A list of ItemIDs for magazines that will be excluded</param>
		/// <returns>An FVRObject for the next largest magazine. Can be null if no next largest magazine is found</returns>
		public static FVRObject GetNextHighestCapacityMagazine(FVRObject currentMagazine, List<FVRObject> possibleMagazines, List<string> blacklistedMagazines = null)
		{
			if (possibleMagazines is null || possibleMagazines.Count == 0) return null;

			//We make this a list so that when several next largest mags have the same capacity, we can return a random magazine from that selection
			List<FVRObject> nextLargestMagazines = new List<FVRObject>();

			foreach (FVRObject magazine in possibleMagazines)
			{
				if (blacklistedMagazines is not null && blacklistedMagazines.Contains(magazine.ItemID)) continue;

				else if (nextLargestMagazines.Count == 0) nextLargestMagazines.Add(magazine);

				//If our next largest mag is the same size as the original, then we take the new larger mag
				if (magazine.MagazineCapacity > currentMagazine.MagazineCapacity && currentMagazine.MagazineCapacity == nextLargestMagazines[0].MagazineCapacity)
				{
					nextLargestMagazines.Clear();
					nextLargestMagazines.Add(magazine);
				}

				//We want the next largest mag size, so the minimum mag size that's also greater than the current mag size
				else if (magazine.MagazineCapacity > currentMagazine.MagazineCapacity && magazine.MagazineCapacity < nextLargestMagazines[0].MagazineCapacity)
				{
					nextLargestMagazines.Clear();
					nextLargestMagazines.Add(magazine);
				}

				//If this magazine has the same capacity as the next largest magazines, add it to the list of options
				else if (magazine.MagazineCapacity == nextLargestMagazines[0].MagazineCapacity)
				{
					nextLargestMagazines.Add(magazine);
				}
			}

			//If the capacity has not increased compared to the original, we should return null
			if (nextLargestMagazines[0].MagazineCapacity == currentMagazine.MagazineCapacity) return null;

			return nextLargestMagazines.GetRandom();
		}




		/// <summary>
		/// Returns a list of FVRPhysicalObjects for items that are either in the players hand, or in one of the players quickbelt slots. This also includes any items in a players backpack if they are wearing one
		/// </summary>
		/// <returns>A list of FVRPhysicalObjects equipped on the player</returns>
		public static List<FVRPhysicalObject> GetEquippedItems()
		{
			List<FVRPhysicalObject> heldItems = new List<FVRPhysicalObject>();

			FVRInteractiveObject rightHandObject = GM.CurrentMovementManager.Hands[0].CurrentInteractable;
			FVRInteractiveObject leftHandObject = GM.CurrentMovementManager.Hands[1].CurrentInteractable;

			//Get any items in the players hands
			if (rightHandObject is FVRPhysicalObject)
			{
				heldItems.Add((FVRPhysicalObject)rightHandObject);
			}

			if (leftHandObject is FVRPhysicalObject)
			{
				heldItems.Add((FVRPhysicalObject)leftHandObject);
			}

			//Get any items on the players body
			foreach (FVRQuickBeltSlot slot in GM.CurrentPlayerBody.QuickbeltSlots)
			{
				if (slot.CurObject is not null && slot.CurObject.ObjectWrapper is not null)
				{
					heldItems.Add(slot.CurObject);
				}

				//If the player has a backpack on, we should search through that as well
				if (slot.CurObject is PlayerBackPack && ((PlayerBackPack)slot.CurObject).ObjectWrapper is not null)
				{
					foreach (FVRQuickBeltSlot backpackSlot in GM.CurrentPlayerBody.QuickbeltSlots)
					{
						if (backpackSlot.CurObject is not null)
						{
							heldItems.Add(backpackSlot.CurObject);
						}
					}
				}
			}

			return heldItems;
		}



		/// <summary>
		/// Returns a list of FVRObjects for all of the items that are equipped on the player. Items without a valid FVRObject are excluded. There may also be duplicate entries if the player has identical items equipped
		/// </summary>
		/// <returns>A list of FVRObjects equipped on the player</returns>
		public static List<FVRObject> GetEquippedFVRObjects()
		{
			List<FVRObject> equippedFVRObjects = new List<FVRObject>();

			foreach (FVRPhysicalObject item in GetEquippedItems())
			{
				if (item.ObjectWrapper is null) continue;

				equippedFVRObjects.Add(item.ObjectWrapper);
			}

			return equippedFVRObjects;
		}





		/// <summary>
		/// Returns a random magazine, clip, or speedloader that is compatible with one of the players equipped items
		/// </summary>
		/// <param name="minCapacity">The minimum capacity for desired containers</param>
		/// <param name="maxCapacity">The maximum capacity for desired containers</param>
		/// <param name="blacklistedContainers">A list of ItemIDs for magazines that will be excluded</param>
		/// <returns>An FVRObject for an ammo container. Can be null if no container is found</returns>
		public static FVRObject GetAmmoContainerForEquipped(int minCapacity = 0, int maxCapacity = 9999, Dictionary<string, MagazineBlacklistEntry> blacklist = null)
		{
			List<FVRObject> heldItems = GetEquippedFVRObjects();

			//Iterpret -1 as having no max capacity
			if (maxCapacity == -1) maxCapacity = 9999;

			//Go through and remove any items that have no ammo containers
			for (int i = heldItems.Count - 1; i >= 0; i--)
			{
				if (!FVRObjectHasAmmoContainer(heldItems[i]))
				{
					heldItems.RemoveAt(i);
				}
			}

			//Now go through all items that do have ammo containers, and try to get an ammo container for one of them
			heldItems.Shuffle();
			foreach (FVRObject item in heldItems)
			{
				MagazineBlacklistEntry blacklistEntry = null;
				if (blacklist != null && blacklist.ContainsKey(item.ItemID)) blacklistEntry = blacklist[item.ItemID];

				List<FVRObject> containers = GetCompatibleAmmoContainers(item, minCapacity, maxCapacity, false, blacklistEntry);
				if (containers.Count > 0) return containers.GetRandom();
			}

			return null;
		}




		/// <summary>
		/// Returns a list of all attached objects on the given firearm. This includes attached magazines
		/// </summary>
		/// <param name="fireArm">The firearm that is being scanned for attachmnets</param>
		/// <param name="includeSelf">If true, includes the given firearm as the first item in the list of attached objects</param>
		/// <returns>A list containing every attached item on the given firearm</returns>
		public static List<FVRPhysicalObject> GetAllAttachedObjects(FVRFireArm fireArm, bool includeSelf = false)
		{
			List<FVRPhysicalObject> detectedObjects = new List<FVRPhysicalObject>();

			if (includeSelf) detectedObjects.Add(fireArm);

			if (fireArm.Magazine is not null && !fireArm.Magazine.IsIntegrated && fireArm.Magazine.ObjectWrapper is not null)
			{
				detectedObjects.Add(fireArm.Magazine);
			}

			foreach (FVRFireArmAttachment attachment in fireArm.Attachments)
			{
				if (attachment.ObjectWrapper is not null) detectedObjects.Add(attachment);
			}

			return detectedObjects;
		}


		public static List<FVRObject> GetLoadedFVRObjectsFromTemplateList(List<AmmoObjectDataTemplate> items)
        {
			List<FVRObject> loadedItems = new List<FVRObject>();

			foreach(AmmoObjectDataTemplate item in items)
            {
				if (IM.OD.ContainsKey(item.ObjectID)) loadedItems.Add(IM.OD[item.ObjectID]);
            }

			return loadedItems;
        }

	}
}
