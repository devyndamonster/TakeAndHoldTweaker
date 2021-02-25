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
                    List<AmmoObjectDataTemplate> validMagazines = GetMagazinesWithinCapacity(minCapacity, maxCapacity, firearm);
                    if (validMagazines.Count > 0)
                    {
                        return IM.OD[validMagazines.GetRandom().ObjectID];
                    }

                    //If there were no valid magazines, just return the smallest one compatible with the weapon
                    AmmoObjectDataTemplate smallestMagazine = GetSmallestCapacityMagazine(firearm);
                    if (smallestMagazine != null && IM.OD.ContainsKey(smallestMagazine.ObjectID))
                    {
                        return IM.OD[GetSmallestCapacityMagazine(firearm).ObjectID];
                    }
                }

                else if (firearm.CompatibleClips.Count > 0) return firearm.CompatibleClips.GetRandom();
                else return firearm.CompatibleSpeedLoaders.GetRandom();
            }

            return null;
        }

        public static List<AmmoObjectDataTemplate> GetMagazinesWithinCapacity(int minCapacity, int maxCapacity, FVRObject firearm)
        {
            List<AmmoObjectDataTemplate> validMagazines = firearm.CompatibleMagazines.Select(o => LoadedTemplateManager.LoadedMagazineDict[o.ItemID]).ToList();

            for (int i = 0; i < validMagazines.Count; i++)
            {
                if (validMagazines[i].Capacity < minCapacity || validMagazines[i].Capacity > maxCapacity)
                {
                    validMagazines.RemoveAt(i);
                    i -= 1;
                }
            }

            return validMagazines;
        }

        public static AmmoObjectDataTemplate GetSmallestCapacityMagazine(FVRObject firearm)
        {
            List<AmmoObjectDataTemplate> magazines = firearm.CompatibleMagazines.Select(o => LoadedTemplateManager.LoadedMagazineDict[o.ItemID]).ToList();
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


        public static AmmoObjectDataTemplate GetSmallestCapacityAmmoObject(FVRObject firearm)
        {
            if (firearm.CompatibleMagazines.Count != 0)
            {
                return GetSmallestCapacityMagazine(firearm);
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
