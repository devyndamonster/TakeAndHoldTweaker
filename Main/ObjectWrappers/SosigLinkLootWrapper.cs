using FistVR;
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

            EquipmentGroup selectedGroup = group.GetSpawnedEquipmentGroups().GetRandom();
            string selectedItem;
            for (int itemIndex = 0; itemIndex < group.ItemsToSpawn; itemIndex++)
            {
                if (selectedGroup.IsCompatibleMagazine)
                {
                    FVRObject mag = FirearmUtils.GetMagazineForEquipped(selectedGroup.MinAmmoCapacity,
                        selectedGroup.MaxAmmoCapacity);
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
                    Instantiate(IM.OD[selectedItem].GetGameObject(), transform.position+ Vector3.up * 0.02f * itemIndex, transform.rotation);
                }
            }
        }
    }
}