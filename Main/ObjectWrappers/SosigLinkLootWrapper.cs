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
            string selectedItem = selectedGroup.GetObjects().GetRandom();

            if (LoadedTemplateManager.LoadedVaultFiles.ContainsKey(selectedItem))
            {
                AnvilManager.Run(TNHTweakerUtils.SpawnFirearm(LoadedTemplateManager.LoadedVaultFiles[selectedItem], transform));
            }
            else
            {
                Instantiate(IM.OD[selectedItem].GetGameObject(), transform.position, transform.rotation);
            }
        }


    }
}
