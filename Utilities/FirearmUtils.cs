using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TNHTweaker.Utilities
{
    static class FirearmUtils
    {

        public static FVRObject GetMagazineForEquipped(int minCapacity = 0, int maxCapacity = 9999)
        {
            TNHTweakerLogger.Log("TNHTWEAKER -- Getting magazine for equipped", TNHTweakerLogger.LogType.TNH);

            List<FVRObject> heldGuns = new List<FVRObject>();

            FVRInteractiveObject rightHandObject = GM.CurrentMovementManager.Hands[0].CurrentInteractable;
            FVRInteractiveObject leftHandObject = GM.CurrentMovementManager.Hands[1].CurrentInteractable;

            //First we want to get any firearms that are in the players hands
            if (rightHandObject is FVRFireArm && (rightHandObject as FVRFireArm).ObjectWrapper != null)
            {
                FVRObject firearm = (rightHandObject as FVRFireArm).ObjectWrapper;
                if (firearm.CompatibleClips.Count > 0 || firearm.CompatibleMagazines.Count > 0 || firearm.CompatibleSpeedLoaders.Count > 0)
                {
                    heldGuns.Add(firearm);
                }
            }
            if (leftHandObject is FVRFireArm && (leftHandObject as FVRFireArm).ObjectWrapper != null)
            {
                FVRObject firearm = (leftHandObject as FVRFireArm).ObjectWrapper;
                if (firearm.CompatibleClips.Count > 0 || firearm.CompatibleMagazines.Count > 0 || firearm.CompatibleSpeedLoaders.Count > 0)
                {
                    heldGuns.Add(firearm);
                }
            }

            //After prioritizing the players hands, if the player didn't have any valid guns in their hands, then we can search for guns on their body
            if (heldGuns.Count == 0)
            {
                foreach (FVRQuickBeltSlot slot in GM.CurrentPlayerBody.QuickbeltSlots)
                {
                    if (slot.CurObject is FVRFireArm && (slot.CurObject as FVRFireArm).ObjectWrapper != null)
                    {
                        FVRObject firearm = (slot.CurObject as FVRFireArm).ObjectWrapper;

                        if (firearm.CompatibleClips.Count > 0 || firearm.CompatibleMagazines.Count > 0 || firearm.CompatibleSpeedLoaders.Count > 0)
                        {
                            heldGuns.Add(firearm);
                        }
                    }

                    else if (slot.CurObject is PlayerBackPack && (slot.CurObject as PlayerBackPack).ObjectWrapper != null)
                    {
                        foreach (FVRQuickBeltSlot backpackSlot in GM.CurrentPlayerBody.QuickbeltSlots)
                        {
                            if (backpackSlot.CurObject is FVRFireArm && (backpackSlot.CurObject as FVRFireArm).ObjectWrapper != null)
                            {
                                FVRObject firearm = (backpackSlot.CurObject as FVRFireArm).ObjectWrapper;

                                if (firearm.CompatibleClips.Count > 0 || firearm.CompatibleMagazines.Count > 0 || firearm.CompatibleSpeedLoaders.Count > 0)
                                {
                                    heldGuns.Add(firearm);
                                }
                            }
                        }
                    }
                }
            }

            if (heldGuns.Count > 0)
            {
                FVRObject firearm = heldGuns.GetRandom();

                //If this guns has compatible magazines, priorotize spawing them
                if (firearm.CompatibleMagazines.Count > 0)
                {
                    //First try to return a magazine within the specified capacity
                    List<AmmoObjectDataTemplate> validMagazines = GetCompatibleMagazines(firearm, minCapacity, maxCapacity);
                    if (validMagazines.Count > 0)
                    {
                        return validMagazines.GetRandom().AmmoObject;
                    }

                    //If there were no valid magazines, just return the smallest one compatible with the weapon
                    AmmoObjectDataTemplate smallestMagazine = GetSmallestCapacityMagazine(firearm);
                    if (smallestMagazine != null && IM.OD.ContainsKey(smallestMagazine.ObjectID))
                    {
                        return GetSmallestCapacityMagazine(firearm).AmmoObject;
                    }
                }

                else if (firearm.CompatibleClips.Count > 0) return firearm.CompatibleClips.GetRandom();
                else return firearm.CompatibleSpeedLoaders.GetRandom();
            }

            return null;
        }


        public static List<AmmoObjectDataTemplate> GetCompatibleMagazines(FVRObject firearm, int minCapacity = 0, int maxCapacity = 9999, Dictionary<string, MagazineBlacklistEntry> magazineBlacklist = null)
        {
            TNHTweakerLogger.Log("TNHTWEAKER -- Getting compatible magazines for: " + firearm.ItemID, TNHTweakerLogger.LogType.TNH);

            List<AmmoObjectDataTemplate> validMagazines = new List<AmmoObjectDataTemplate>();

            //If our max capacity is zero or negative, then we return the smallest magazine compatible with the firearm
            if (maxCapacity <= 0)
            {
                AmmoObjectDataTemplate smallest = GetSmallestCapacityMagazine(firearm, magazineBlacklist);
                if(smallest != null)
                {
                    validMagazines.Add(smallest);
                }
                return validMagazines;
            }


            foreach (FVRObject item in firearm.CompatibleMagazines)
            {
                if (LoadedTemplateManager.LoadedMagazineDict.ContainsKey(item.ItemID))
                {
                    validMagazines.Add(LoadedTemplateManager.LoadedMagazineDict[item.ItemID]);
                }

                else
                {
                    TNHTweakerLogger.LogWarning("TNHTweaker -- Firearm had a compatible magazine, but it wasn't loaded in the global magazine dictionary! Magazine won't be included");
                }
            }

            for (int i = 0; i < validMagazines.Count; i++)
            {
                if (magazineBlacklist != null && magazineBlacklist.ContainsKey(firearm.ItemID) && magazineBlacklist[firearm.ItemID].MagazineBlacklist.Contains(validMagazines[i].ObjectID))
                {
                    validMagazines.RemoveAt(i);
                    i -= 1;
                    continue;
                }

                if (validMagazines[i].Capacity < minCapacity || (validMagazines[i].Capacity > maxCapacity))
                {
                    validMagazines.RemoveAt(i);
                    i -= 1;
                }
            }

            return validMagazines;
        }


        public static List<AmmoObjectDataTemplate> GetCompatibleClips(FVRObject firearm, Dictionary<string, MagazineBlacklistEntry> magazineBlacklist = null)
        {
            TNHTweakerLogger.Log("TNHTWEAKER -- Getting compatible clips for: " + firearm.ItemID, TNHTweakerLogger.LogType.TNH);

            List<AmmoObjectDataTemplate> validClips = new List<AmmoObjectDataTemplate>();

            foreach(FVRObject item in firearm.CompatibleClips)
            {
                if (LoadedTemplateManager.LoadedClipDict.ContainsKey(item.ItemID))
                {
                    validClips.Add(LoadedTemplateManager.LoadedClipDict[item.ItemID]);
                }

                else
                {
                    TNHTweakerLogger.LogWarning("TNHTweaker -- Firearm had a compatible clip, but it wasn't loaded in the global clip dictionary! Clip won't be included");
                }
            }
               
            for (int i = 0; i < validClips.Count; i++)
            {
                if (magazineBlacklist != null && magazineBlacklist.ContainsKey(firearm.ItemID) && magazineBlacklist[firearm.ItemID].ClipBlacklist.Contains(validClips[i].ObjectID))
                {
                    validClips.RemoveAt(i);
                    i -= 1;
                    continue;
                }
            }

            return validClips;
        }


        public static List<AmmoObjectDataTemplate> GetCompatibleBullets(FVRObject firearm, List<FVRObject.OTagEra> eras = null, List<FVRObject.OTagSet> sets = null, List<string> ammoBlacklist = null, Dictionary<string, MagazineBlacklistEntry> magazineBlacklist = null)
        {
            TNHTweakerLogger.Log("TNHTWEAKER -- Getting compatible bullets for: " + firearm.ItemID, TNHTweakerLogger.LogType.TNH);

            if (firearm.CompatibleSingleRounds.Count > 0)
            {
                List<AmmoObjectDataTemplate> validBullets = firearm.CompatibleSingleRounds.Select(o => LoadedTemplateManager.LoadedBulletDict[o.ItemID]).ToList();

                for (int i = 0; i < validBullets.Count; i++)
                {
                    if(eras != null && !eras.Contains(validBullets[i].AmmoObject.TagEra))
                    {
                        validBullets.RemoveAt(i);
                        i -= 1;
                        continue;
                    }

                    if(sets != null && !sets.Contains(validBullets[i].AmmoObject.TagSet))
                    {
                        validBullets.RemoveAt(i);
                        i -= 1;
                        continue;
                    }

                    if(ammoBlacklist != null && ammoBlacklist.Contains(validBullets[i].ObjectID)){
                        validBullets.RemoveAt(i);
                        i -= 1;
                        continue;
                    }
                }

                return validBullets;
            }

            return new List<AmmoObjectDataTemplate>();
        }


        public static AmmoObjectDataTemplate GetSmallestCapacityMagazine(FVRObject firearm, Dictionary<string, MagazineBlacklistEntry> magazineBlacklist = null)
        {
            TNHTweakerLogger.Log("TNHTWEAKER -- Getting smallest compatible mag for: " + firearm.ItemID, TNHTweakerLogger.LogType.TNH);

            List<AmmoObjectDataTemplate> magazines = new List<AmmoObjectDataTemplate>();

            foreach (FVRObject item in firearm.CompatibleMagazines)
            {
                if (LoadedTemplateManager.LoadedMagazineDict.ContainsKey(item.ItemID))
                {
                    if(magazineBlacklist == null || !magazineBlacklist.ContainsKey(firearm.ItemID) || !magazineBlacklist[firearm.ItemID].MagazineBlacklist.Contains(item.ItemID))
                    {
                        magazines.Add(LoadedTemplateManager.LoadedMagazineDict[item.ItemID]);
                    }
                }
            }


            if (magazines.Count == 0) return null;

            AmmoObjectDataTemplate smallestMagazine = magazines[0];
            foreach (AmmoObjectDataTemplate magazine in magazines)
            {
                if (magazine.Capacity < smallestMagazine.Capacity)
                {
                    smallestMagazine = magazine;
                }
            }

            return smallestMagazine;
        }


        public static AmmoObjectDataTemplate GetSmallestCapacityAmmoObject(List<AmmoObjectDataTemplate> objects)
        {
            TNHTweakerLogger.Log("TNHTWEAKER -- Getting compatible ammo object from list", TNHTweakerLogger.LogType.TNH);

            AmmoObjectDataTemplate smallest = null;

            foreach(AmmoObjectDataTemplate item in objects)
            {
                if(smallest == null || smallest.Capacity < item.Capacity)
                {
                    smallest = item;
                }
            }

            return smallest;
        }


        public static AmmoObjectDataTemplate GetSmallestCapacityAmmoObject(FVRObject firearm, Dictionary<string, MagazineBlacklistEntry> magazineBlacklist = null)
        {
            TNHTweakerLogger.Log("TNHTWEAKER -- Getting smallest compatible ammo object for: " + firearm.ItemID, TNHTweakerLogger.LogType.TNH);

            List<AmmoObjectDataTemplate> compatibleMagazines = GetCompatibleMagazines(firearm, 0, 9999, magazineBlacklist);
            if (compatibleMagazines.Count != 0)
            {
                return GetSmallestCapacityAmmoObject(compatibleMagazines);
            }

            else if (firearm.CompatibleClips.Count != 0)
            {
                return LoadedTemplateManager.LoadedClipDict[firearm.CompatibleClips[0].ItemID];
            }

            else if (firearm.CompatibleSpeedLoaders.Count != 0)
            {
                AmmoObjectDataTemplate speedLoaderTemplate = new AmmoObjectDataTemplate();
                speedLoaderTemplate.ObjectID = firearm.CompatibleSpeedLoaders[0].ItemID;
                speedLoaderTemplate.Capacity = 6;
                speedLoaderTemplate.AmmoObject = IM.OD[speedLoaderTemplate.ObjectID];
                return speedLoaderTemplate;
            }

            return null;
        }

        public static bool FVRObjectHasAmmoContainer(FVRObject item)
        {
            if (item == null) return false;
            return item.CompatibleClips.Count != 0 || item.CompatibleMagazines.Count != 0 || item.CompatibleSpeedLoaders.Count != 0;
        }


        public static bool FVRObjectHasAmmoObject(FVRObject item)
        {
            if (item == null) return false;
            return (item.CompatibleSingleRounds != null && item.CompatibleSingleRounds.Count != 0) || (item.CompatibleClips != null && item.CompatibleClips.Count > 0) || (item.CompatibleMagazines != null && item.CompatibleMagazines.Count > 0) || (item.CompatibleSpeedLoaders != null && item.CompatibleSpeedLoaders.Count != 0);
        }


        /// <summary>
        /// Returns a object representing the next largest possible magazine of the same type as the sent mag. If there are multiple possible mags of the same size, a random magazine from those possibilities will be selected
        /// </summary>
        /// <param name="mag"></param>
        /// <returns></returns>
        public static AmmoObjectDataTemplate GetNextHighestCapacityMagazine(FVRFireArmMagazine mag)
        {
            List<AmmoObjectDataTemplate> possibleMags = new List<AmmoObjectDataTemplate>();
            possibleMags.Add(new AmmoObjectDataTemplate(mag));

            foreach (AmmoObjectDataTemplate magTemplate in LoadedTemplateManager.LoadedMagazineTypeDict[mag.MagazineType])
            {
                //If are next largest is the same size as the original, then we take a larger magazine
                if (magTemplate.Capacity > mag.m_capacity && mag.m_capacity == possibleMags[0].Capacity)
                {
                    possibleMags.Clear();
                    possibleMags.Add(magTemplate);
                }

                //We want the next largest mag size, so the minimum mag size that's also greater than the current mag size
                else if (magTemplate.Capacity > mag.m_capacity && magTemplate.Capacity < possibleMags[0].Capacity)
                {
                    possibleMags.Clear();
                    possibleMags.Add(magTemplate);
                }

                //If this magazine has the same capacity as the next largest magazines, add it to the list of options
                else if (magTemplate.Capacity == possibleMags[0].Capacity)
                {
                    possibleMags.Add(magTemplate);
                }
            }

            return possibleMags.GetRandom();
        }

    }
}
