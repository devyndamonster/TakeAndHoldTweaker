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

            List<GameObject> toSpawn = new List<GameObject>();
            EquipmentGroup selectedGroup = group.GetSpawnedEquipmentGroups().GetRandom();
            string selectedItem;
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
                        transform.position, transform.rotation));
                }
                else
                {
                    toSpawn.Add(IM.OD[selectedItem].GetGameObject());
                }
            }

            AnvilManager.Run(TNHTweakerUtils.InstantiateList(toSpawn, transform.position));
        }
    }
}