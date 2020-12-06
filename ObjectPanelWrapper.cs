using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace FistVR
{
    public enum PanelType
    {
        MagDuplicator,
        Recycler,
        AmmoReloader,
        MagUpgrader,
        AddFullAuto,
        HigherFireRate
    }

    public class MagUpgrader : MonoBehaviour
    {
        public TNH_MagDuplicator original;

        private FVRFireArmMagazine detectedMag = null;
        private FVRObject upgradeMag = null;
        private int storedCost = 0;
        private Collider[] colBuffer = new Collider[50];
        private float scanTick = 1f;

        public void Awake()
        {
            original = gameObject.GetComponent<TNH_MagDuplicator>();

            if (original == null) Debug.LogError("Mag Upgrader failed, original Mag Duplicator was null!");

            Traverse.Create(original).Field("m_scanTick").SetValue(999999);

            original.OCIcon.Image.sprite = LoadedTemplateManager.PanelSprites[PanelType.MagUpgrader];
            original.OCIcon.Sprite_Cancel = LoadedTemplateManager.PanelSprites[PanelType.MagUpgrader];
        }


        public void ButtonPressed()
        {
            if(upgradeMag == null || storedCost > original.M.GetNumTokens() || upgradeMag.ItemID == detectedMag.ObjectWrapper.ItemID)
            {
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Fail, transform.position);
                return;
            }

            else
            {
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Spawn, transform.position);
                original.M.SubtractTokens(storedCost);
                original.M.Increment(10, false);
                Instantiate(upgradeMag.GetGameObject(), original.Spawnpoint_Mag.position, original.Spawnpoint_Mag.rotation);
                Destroy(detectedMag.gameObject);
            }
        }


        private void Update()
        {
            scanTick -= Time.deltaTime;
            if(scanTick <= 0)
            {
                scanTick = 1;
                if(Vector3.Distance(transform.position, GM.CurrentPlayerBody.transform.position) < 12)
                {
                    Scan();
                }
            }
        }

        private void Scan()
        {
            int colliderCount = Physics.OverlapBoxNonAlloc(original.ScanningVolume.position, original.ScanningVolume.localScale * 0.5f, colBuffer, original.ScanningVolume.rotation, original.ScanningLM, QueryTriggerInteraction.Collide);

            detectedMag = null;
            upgradeMag = null;

            for(int i = 0; i < colliderCount; i++)
            {
                if(colBuffer[i].attachedRigidbody != null)
                {
                    FVRFireArmMagazine mag = colBuffer[i].GetComponent<FVRFireArmMagazine>();

                    if (mag != null && mag.FireArm == null && !mag.IsHeld && mag.QuickbeltSlot == null)
                    {
                        detectedMag = mag;
                        MagazineDataTemplate nextLargestMag = GetNextHighestCapacityMagazine(detectedMag);
                        upgradeMag = IM.OD[nextLargestMag.ObjectID];
                        break;
                    }
                }
                
            }

            SetCost();
        }

        private void SetCost()
        {
            if(upgradeMag != null && detectedMag != null && detectedMag.ObjectWrapper.ItemID != upgradeMag.ItemID)
            {
                storedCost = 1;
                original.OCIcon.SetOption(TNH_ObjectConstructorIcon.IconState.Item, original.OCIcon.Sprite_Accept, storedCost);
            }
            else
            {
                storedCost = 0;
                original.OCIcon.SetOption(TNH_ObjectConstructorIcon.IconState.Cancel, original.OCIcon.Sprite_Cancel, storedCost);
            }
        }

        private MagazineDataTemplate GetNextHighestCapacityMagazine(FVRFireArmMagazine mag)
        {
            MagazineDataTemplate nextLargestMag = new MagazineDataTemplate(mag);

            foreach(MagazineDataTemplate magTemplate in LoadedTemplateManager.LoadedMagazines[mag.MagazineType])
            {
                //If are next largest is the same size as the original, then we take a larger magazine
                if(magTemplate.Capacity > mag.m_capacity && mag.m_capacity == nextLargestMag.Capacity)
                {
                    nextLargestMag = magTemplate;
                }

                //We want the next largest mag size, so the minimum mag size that's also greater than the current mag size
                else if (magTemplate.Capacity > mag.m_capacity && magTemplate.Capacity < nextLargestMag.Capacity)
                {
                    nextLargestMag = magTemplate;
                }
            }

            return nextLargestMag;
        }
    }
}
