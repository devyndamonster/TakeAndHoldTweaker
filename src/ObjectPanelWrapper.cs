using FistVR;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.Cache;
using TNHTweaker.Templates;
using UnityEngine;
using UnityEngine.UI;

namespace TNHTweaker.ObjectPanel
{
    public enum PanelType
    {
        MagDuplicator,
        Recycler,
        AmmoReloader,
        MagUpgrader,
        AddFullAuto
    }

    public class MagUpgrader : MonoBehaviour
    {
        public TNH_MagDuplicator original;

        private FVRFireArmMagazine detectedMag = null;
        private MagazineDataTemplate upgradeMag = null;
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
            if (upgradeMag == null || storedCost > original.M.GetNumTokens() || upgradeMag.ObjectID == detectedMag.ObjectWrapper.ItemID)
            {
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Fail, transform.position);
                return;
            }

            else
            {
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Spawn, transform.position);
                original.M.SubtractTokens(storedCost);
                original.M.Increment(10, false);

                Debug.Log(upgradeMag.ObjectID);

                Instantiate(IM.OD[upgradeMag.ObjectID].GetGameObject(), original.Spawnpoint_Mag.position, original.Spawnpoint_Mag.rotation);
                Destroy(detectedMag.gameObject);

                detectedMag = null;
                upgradeMag = null;
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
                        upgradeMag = GetNextHighestCapacityMagazine(detectedMag);

                        SetCost();

                        return;
                    }
                }
            }

            //If we make it to this point, the magazines must be null
            detectedMag = null;
            upgradeMag = null;

            SetCost();
        }

        private void SetCost()
        {
            if(upgradeMag != null && detectedMag != null && detectedMag.ObjectWrapper.ItemID != upgradeMag.ObjectID)
            {
                storedCost = (upgradeMag.Capacity - detectedMag.m_capacity) / 5;
                storedCost = Mathf.Clamp(storedCost, 1, 6);
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

            foreach(MagazineDataTemplate magTemplate in LoadedTemplateManager.LoadedMagazineTypeDict[mag.MagazineType])
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







    public class FullAutoEnabler : MonoBehaviour
    {
        public TNH_MagDuplicator original;

        public Handgun detectedHandgun = null;
        public ClosedBoltWeapon detectedClosedBolt = null;
        public OpenBoltReceiver detectedOpenBolt = null;

        private int storedCost = 0;
        private Collider[] colBuffer = new Collider[50];
        private float scanTick = 1f;

        public void Awake()
        {
            original = gameObject.GetComponent<TNH_MagDuplicator>();

            if (original == null) Debug.LogError("Full Auto Enabler failed, original Mag Duplicator was null!");

            Traverse.Create(original).Field("m_scanTick").SetValue(999999);

            original.OCIcon.Image.sprite = LoadedTemplateManager.PanelSprites[PanelType.AddFullAuto];
            original.OCIcon.Sprite_Cancel = LoadedTemplateManager.PanelSprites[PanelType.AddFullAuto];
        }


        public void ButtonPressed()
        {
            if ((detectedHandgun == null && detectedClosedBolt == null && detectedOpenBolt == null) || storedCost > original.M.GetNumTokens())
            {
                //Debug.Log("Can't add full auto!");
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Fail, transform.position);
                return;
            }

            else
            {
                //Debug.Log("Adding full auto!");
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Spawn, transform.position);
                original.M.SubtractTokens(storedCost);
                original.M.Increment(10, false);

                if(detectedHandgun != null)
                {
                    AddFullAutoToHandgun(detectedHandgun);
                    detectedHandgun = null;
                }

                else if (detectedClosedBolt != null)
                {
                    AddFullAutoToClosedBolt(detectedClosedBolt);
                    detectedClosedBolt = null;
                }

                else if(detectedOpenBolt != null)
                {
                    AddFullAutoToOpenBolt(detectedOpenBolt);
                    detectedOpenBolt = null;
                }
            }
        }


        private void AddFullAutoToHandgun(Handgun gun)
        {
            Handgun.FireSelectorMode fullAuto = new Handgun.FireSelectorMode();
            fullAuto.SelectorPosition = 0;
            fullAuto.ModeType = Handgun.FireSelectorModeType.FullAuto;

            if(gun.FireSelectorModes != null)
            {
                //Debug.Log("Fire Selector options before addition:");
                foreach (Handgun.FireSelectorMode mode in gun.FireSelectorModes)
                {
                    Debug.Log(mode.ModeType);
                }
            }
            
            if (gun.FireSelectorModes == null || gun.FireSelectorModes.Length == 0)
            {
                //Debug.Log("Handgun did not have any fire selector modes, so we're adding defaults");

                Handgun.FireSelectorMode single = new Handgun.FireSelectorMode();
                single.SelectorPosition = 0;
                single.ModeType = Handgun.FireSelectorModeType.Single;

                gun.FireSelectorModes = new Handgun.FireSelectorMode[] { single, fullAuto };
            }
            else
            {
                //Debug.Log("Handgun had atleast one fire selector mode, Adding full auto");
                List<Handgun.FireSelectorMode> modes = new List<Handgun.FireSelectorMode>(gun.FireSelectorModes);
                modes.Add(fullAuto);
                gun.FireSelectorModes = modes.ToArray();

                //Debug.Log("Array count: " + gun.FireSelectorModes.Length + ", List count: " + modes.Count);
            }

            //Debug.Log("Fire Selector options after adding full auto:");
            foreach (Handgun.FireSelectorMode mode in gun.FireSelectorModes)
            {
                //Debug.Log(mode.ModeType);
            }
            
            if (!gun.HasFireSelector)
            {
                //Debug.Log("Handgun was not originally configured to have fire selector. Configuring");

                gun.HasFireSelector = true;

                if (gun.HasSafety)
                {
                    //Debug.Log("Using safety object as fire selector");
                    gun.FireSelectorAxis = gun.SafetyAxis;
                    gun.FireSelectorInterpStyle = gun.Safety_Interp;
                    gun.FireSelector = gun.Safety;
                }

                else if (gun.FireSelector == null)
                {
                    //Debug.Log("Creating dummy game object for fire selector");
                    gun.FireSelector = Instantiate(new GameObject(), gun.GameObject.transform).transform;
                }
            }

            if (gun.HasSafety)
            {
                //Debug.Log("Handgun originally had a safety, so we're removing that");
                //gun.SetSafetyState(false);
                //gun.FireSelectorModes = gun.FireSelectorModes.Where(o => o.ModeType != Handgun.FireSelectorModeType.Safe).ToArray();
                gun.AudioClipSet.FireSelector = gun.AudioClipSet.Safety;
                gun.HasSafety = false;
            }
        }


        private void AddFullAutoToClosedBolt(ClosedBoltWeapon gun)
        {
            ClosedBoltWeapon.FireSelectorMode fullAuto = new ClosedBoltWeapon.FireSelectorMode();
            fullAuto.ModeType = ClosedBoltWeapon.FireSelectorModeType.FullAuto;
            fullAuto.SelectorPosition = 0;

            //Debug.Log("Adding full auto to closed bolt");

            if (gun.FireSelector_Modes != null)
            {
                //Debug.Log("Fire Selector options before addition:");
                foreach (ClosedBoltWeapon.FireSelectorMode mode in gun.FireSelector_Modes)
                {
                    //Debug.Log(mode.ModeType);
                }
            }

            if (gun.FireSelector_Modes == null || gun.FireSelector_Modes.Length == 0)
            {
                //Debug.Log("Gun did not have fire selector, adding full");

                ClosedBoltWeapon.FireSelectorMode single = new ClosedBoltWeapon.FireSelectorMode();
                single.ModeType = ClosedBoltWeapon.FireSelectorModeType.Single;
                single.SelectorPosition = 0;

                gun.FireSelector_Modes = new ClosedBoltWeapon.FireSelectorMode[] { single, fullAuto };
            }
            else
            {
                //Debug.Log("Gun had fire selector, adding full");
                List<ClosedBoltWeapon.FireSelectorMode> modes = new List<ClosedBoltWeapon.FireSelectorMode>(gun.FireSelector_Modes);
                modes.Add(fullAuto);
                gun.FireSelector_Modes = modes.ToArray();
            }
        }

        private void AddFullAutoToOpenBolt(OpenBoltReceiver gun)
        {
            OpenBoltReceiver.FireSelectorMode fullAuto = new OpenBoltReceiver.FireSelectorMode();
            fullAuto.ModeType = OpenBoltReceiver.FireSelectorModeType.FullAuto;
            fullAuto.SelectorPosition = 0;

            //Debug.Log("Adding full auto to open bolt");

            if (gun.FireSelector_Modes != null)
            {
                //Debug.Log("Fire Selector options before addition:");
                foreach (OpenBoltReceiver.FireSelectorMode mode in gun.FireSelector_Modes)
                {
                    //Debug.Log(mode.ModeType);
                }
            }

            if (gun.FireSelector_Modes == null || gun.FireSelector_Modes.Length == 0)
            {
                //Debug.Log("Gun did not have fire selector, adding full");

                OpenBoltReceiver.FireSelectorMode single = new OpenBoltReceiver.FireSelectorMode();
                single.ModeType = OpenBoltReceiver.FireSelectorModeType.Single;
                single.SelectorPosition = 0;

                gun.FireSelector_Modes = new OpenBoltReceiver.FireSelectorMode[] { single, fullAuto };
            }
            else
            {
                //Debug.Log("Gun had fire selector, adding full");
                List<OpenBoltReceiver.FireSelectorMode> modes = new List<OpenBoltReceiver.FireSelectorMode>(gun.FireSelector_Modes);
                modes.Add(fullAuto);
                gun.FireSelector_Modes = modes.ToArray();
            }
        }


        private void Update()
        {
            scanTick -= Time.deltaTime;
            if (scanTick <= 0)
            {
                scanTick = 1;
                if (Vector3.Distance(transform.position, GM.CurrentPlayerBody.transform.position) < 12)
                {
                    Scan();
                }
            }
        }

        private void Scan()
        {
            int colliderCount = Physics.OverlapBoxNonAlloc(original.ScanningVolume.position, original.ScanningVolume.localScale * 0.5f, colBuffer, original.ScanningVolume.rotation, original.ScanningLM, QueryTriggerInteraction.Collide);

            for (int i = 0; i < colliderCount; i++)
            {
                if (colBuffer[i].attachedRigidbody != null)
                {
                    Handgun handgun = colBuffer[i].GetComponent<Handgun>();
                    if(handgun != null)
                    {
                        if(handgun.FireSelectorModes == null || !handgun.FireSelectorModes.Any(o => o.ModeType == Handgun.FireSelectorModeType.FullAuto))
                        {
                            //Debug.Log("Hand gun detected!");
                            detectedHandgun = handgun;
                            SetCost();
                            return;
                        }
                    }

                    ClosedBoltWeapon closedBolt = colBuffer[i].GetComponent<ClosedBoltWeapon>();
                    if(closedBolt != null)
                    {
                        if(closedBolt.FireSelector_Modes == null || !closedBolt.FireSelector_Modes.Any(o => o.ModeType == ClosedBoltWeapon.FireSelectorModeType.FullAuto))
                        {
                            //Debug.Log("Closed bolt detected!");
                            detectedClosedBolt = closedBolt;
                            SetCost();
                            return;
                        }
                    }

                    OpenBoltReceiver openBolt = colBuffer[i].GetComponent<OpenBoltReceiver>();
                    if(openBolt != null)
                    {
                        if(openBolt.FireSelector_Modes == null || !openBolt.FireSelector_Modes.Any(o => o.ModeType == OpenBoltReceiver.FireSelectorModeType.FullAuto))
                        {
                            //Debug.Log("Open bolt detected!");
                            detectedOpenBolt = openBolt;
                            SetCost();
                            return;
                        }
                    }
                }
            }

            //If we get to this point, there must not be a handgun
            detectedHandgun = null;
            detectedClosedBolt = null;
            detectedOpenBolt = null;
            SetCost();
        }

        private void SetCost()
        {
            if(detectedHandgun != null || detectedClosedBolt != null || detectedOpenBolt != null)
            {
                storedCost = 4;
                original.OCIcon.SetOption(TNH_ObjectConstructorIcon.IconState.Item, original.OCIcon.Sprite_Accept, storedCost);
            }
            else
            {
                storedCost = 0;
                original.OCIcon.SetOption(TNH_ObjectConstructorIcon.IconState.Cancel, original.OCIcon.Sprite_Cancel, storedCost);
            }
        }
    }


}
