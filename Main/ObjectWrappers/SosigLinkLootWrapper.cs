using FistVR;
using MagazinePatcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.ObjectTemplates;
using TNHTweaker.Utilities;
using UnityEngine;

namespace TNHTweaker
{
    public class SosigLinkLootWrapper : MonoBehaviour
    {
        public EquipmentGroup group;

        void OnDestroy()
        {
            TNHTweakerLogger.Log("TNHTweaker -- Lootable link was destroyed!", TNHTweakerLogger.LogType.TNH);

            List<EquipmentGroup> selectedGroups = group.GetSpawnedEquipmentGroups();
            string selectedItem;
            int spawnedItems = 0;

            foreach(EquipmentGroup selectedGroup in selectedGroups)
            {
                for (int itemIndex = 0; itemIndex < selectedGroup.ItemsToSpawn; itemIndex++)
                {
                    if (selectedGroup.IsCompatibleMagazine)
                    {
                        FVRObject mag = FirearmUtils.GetAmmoContainerForEquipped(selectedGroup.MinAmmoCapacity, selectedGroup.MaxAmmoCapacity);
                        if (mag != null)
                        {
                            selectedItem = mag.ItemID;
                        }
                        else
                        {
                            TNHTweakerLogger.Log(
                                "TNHTweaker -- Spawning nothing, since group was compatible magazines, and could not find a compatible magazine for player",
                                TNHTweakerLogger.LogType.TNH);
                            return;
                        }
                    }

                    else
                    {
                        selectedItem = selectedGroup.GetObjects().GetRandom();
                    }

                    if (LoadedTemplateManager.LoadedVaultFiles.ContainsKey(selectedItem))
                    {
                        AnvilManager.Run(TNHTweakerUtils.SpawnFirearm(LoadedTemplateManager.LoadedVaultFiles[selectedItem],
                            transform.position + (Vector3.up * 0.1f * spawnedItems) , transform.rotation));
                    }
                    else
                    {
                        Instantiate(IM.OD[selectedItem].GetGameObject(), transform.position + (Vector3.up * 0.1f * spawnedItems), transform.rotation);
                    }

                    spawnedItems += 1;
                }
            }
        }
    }
}