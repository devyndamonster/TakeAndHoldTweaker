using FistVR;
using HarmonyLib;
using MagazinePatcher;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.ObjectTemplates;
using TNHTweaker.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace TNHTweaker
{
    public enum PanelType
    {
        MagDuplicator,
        Recycler,
        AmmoReloader,
        MagUpgrader,
        AddFullAuto,
        AmmoPurchase,
        FireRateUp,
        FireRateDown,
        MagPurchase,
    }

    public class MagUpgrader : MonoBehaviour
    {
        public TNH_MagDuplicator original;

        public Dictionary<string, MagazineBlacklistEntry> blacklist;

        public static Sprite background;
        public static Sprite buttonIcon;

        private TNH_ObjectConstructorIcon DupeIcon;
        private TNH_ObjectConstructorIcon UpgradeIcon;
        private TNH_ObjectConstructorIcon PurchaseIcon;

        private FVRFireArmMagazine detectedMag = null;
        private Speedloader detectedSpeedLoader = null;
        private FVRObject purchaseMag = null;
        private FVRObject upgradeMag = null;

        private Collider[] colBuffer = new Collider[50];
        private float scanTick = 1f;

        public void Awake()
        {
            original = gameObject.GetComponent<TNH_MagDuplicator>();
            if (original == null) TNHTweakerLogger.LogError("Mag Upgrader failed, original Mag Duplicator was null!");
            blacklist = LoadedTemplateManager.LoadedCharactersDict[original.M.C].GetMagazineBlacklist();

            InitPanel();

            original.enabled = false;

            UpdateIcons();
        }


        private void InitPanel()
        {
            Transform backingTransform = original.transform.Find("_CanvasHolder/_UITest_Canvas/Backing");

            Transform canvasHolder = original.transform.Find("_CanvasHolder/_UITest_Canvas");

            Transform iconTransform_0 = canvasHolder.Find("Icon_0");
            iconTransform_0.localPosition = new Vector3(-270, -200, 0);

            Transform iconTransform_1 = canvasHolder.Find("Icon_1");
            iconTransform_1.localPosition = new Vector3(0, -200, 0);

            Transform iconTransform_2 = Instantiate(iconTransform_1.gameObject, canvasHolder).transform;
            iconTransform_2.localPosition = new Vector3(270, -200, 0);

            Transform buttonTransform_0 = original.transform.Find("PointableButton_0");
            buttonTransform_0.position = iconTransform_0.position;

            Transform buttonTransform_1 = original.transform.Find("PointableButton_1");
            buttonTransform_1.position = iconTransform_1.position;

            Transform buttonTransform_2 = Instantiate(buttonTransform_1.gameObject, buttonTransform_1.parent).transform;
            buttonTransform_2.position = iconTransform_2.position;

            Image backgroundImage = backingTransform.gameObject.GetComponent<Image>();

            backgroundImage.sprite = background;

            DupeIcon = iconTransform_0.gameObject.GetComponent<TNH_ObjectConstructorIcon>();
            UpgradeIcon = iconTransform_1.gameObject.GetComponent<TNH_ObjectConstructorIcon>();
            PurchaseIcon = iconTransform_2.gameObject.GetComponent<TNH_ObjectConstructorIcon>();

            Button button_0 = buttonTransform_0.gameObject.GetComponent<Button>();
            button_0.onClick = new Button.ButtonClickedEvent();
            button_0.onClick.AddListener(() => { DupeMagButton(); });

            Button button_1 = buttonTransform_1.gameObject.GetComponent<Button>();
            button_1.onClick = new Button.ButtonClickedEvent();
            button_1.onClick.AddListener(() => { UpgradeMagButton(); });

            Button button_2 = buttonTransform_2.gameObject.GetComponent<Button>();
            button_2.onClick = new Button.ButtonClickedEvent();
            button_2.onClick.AddListener(() => { PurchaseMagButton(); });
        }

        private void DupeMagButton()
        {
            if((detectedMag == null && detectedSpeedLoader == null) || original.M.GetNumTokens() == 0)
            {
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Fail, transform.position);
            }

            else
            {
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Spawn, transform.position);
                original.M.SubtractTokens(1);
                original.M.Increment(10, false);

                if(detectedMag != null)
                {
                    FirearmUtils.SpawnDuplicateMagazine(detectedMag, original.Spawnpoint_Mag.position, original.Spawnpoint_Mag.rotation);
                }

                else
                {
                    FirearmUtils.SpawnDuplicateSpeedloader(detectedSpeedLoader, original.Spawnpoint_Mag.position, original.Spawnpoint_Mag.rotation);
                }

                detectedMag = null;
                detectedSpeedLoader = null;
            }
        }

        private void UpgradeMagButton()
        {
            if (upgradeMag == null || original.M.GetNumTokens() < 2)
            {
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Fail, transform.position);
            }

            else
            {
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Spawn, transform.position);
                original.M.SubtractTokens(2);
                original.M.Increment(10, false);
                Destroy(detectedMag.GameObject);
                Instantiate(upgradeMag.GetGameObject(), original.Spawnpoint_Mag.position, original.Spawnpoint_Mag.rotation);
            }
        }

        private void PurchaseMagButton()
        {
            if (purchaseMag == null || original.M.GetNumTokens() < 1)
            {
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Fail, transform.position);
            }

            else
            {
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Spawn, transform.position);
                original.M.SubtractTokens(1);
                original.M.Increment(10, false);

                Instantiate(purchaseMag.GetGameObject(), original.Spawnpoint_Mag.position, original.Spawnpoint_Mag.rotation);

                purchaseMag = null;
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
            detectedSpeedLoader = null;
            purchaseMag = null;
            upgradeMag = null;

            for(int i = 0; i < colliderCount; i++)
            {
                if(colBuffer[i].attachedRigidbody != null)
                {
                    FVRFireArm firearm = colBuffer[i].GetComponent<FVRFireArm>();
                    if (purchaseMag == null && firearm != null && !firearm.IsHeld && firearm.QuickbeltSlot == null)
                    {
                        MagazineBlacklistEntry entry = null;
                        if (blacklist.ContainsKey(firearm.ObjectWrapper.ItemID)) entry = blacklist[firearm.ObjectWrapper.ItemID];
                        List<FVRObject> spawnableMags = FirearmUtils.GetCompatibleMagazines(firearm.ObjectWrapper, -1, -1, false, entry);

                        if (spawnableMags.Count > 0)
                        {
                            purchaseMag = FirearmUtils.GetSmallestCapacityMagazine(spawnableMags);
                        }
                    }

                    FVRFireArmMagazine mag = colBuffer[i].GetComponent<FVRFireArmMagazine>();
                    if (mag != null && mag.FireArm == null && (!mag.IsHeld) && mag.QuickbeltSlot == null && (!mag.IsIntegrated))
                    {
                        detectedMag = mag;
                    }

                    Speedloader speedloader = colBuffer[i].GetComponent<Speedloader>();
                    if (speedloader != null && (!speedloader.IsHeld) && speedloader.QuickbeltSlot == null && speedloader.IsPretendingToBeAMagazine)
                    {
                        detectedSpeedLoader = speedloader;
                    }

                    //If at this point we have a valid ammo container and firearm, we can stop looping
                    if (purchaseMag != null && (detectedMag != null || detectedSpeedLoader != null)) break;
                }
            }

            UpdateIcons();
        }

        private void UpdateIcons()
        {
            DupeIcon.State = TNH_ObjectConstructorIcon.IconState.Cancel;
            UpgradeIcon.State = TNH_ObjectConstructorIcon.IconState.Cancel;
            PurchaseIcon.State = TNH_ObjectConstructorIcon.IconState.Cancel;

            if (detectedMag != null || detectedSpeedLoader != null) DupeIcon.State = TNH_ObjectConstructorIcon.IconState.Accept;

            if (purchaseMag != null) PurchaseIcon.State = TNH_ObjectConstructorIcon.IconState.Accept;

            if (detectedMag != null)
            {
                upgradeMag = FirearmUtils.GetNextHighestCapacityMagazine(detectedMag.ObjectWrapper);
                if (upgradeMag != null) UpgradeIcon.State = TNH_ObjectConstructorIcon.IconState.Accept;
            }
            
            DupeIcon.UpdateIconDisplay();
            UpgradeIcon.UpdateIconDisplay();
            PurchaseIcon.UpdateIconDisplay();
        }

        
    }

    /*

    public class MagPurchaser : MonoBehaviour
    {
        public TNH_MagDuplicator original;

        private FVRObject ammoObject = null;

        private int storedCost = 0;
        private Collider[] colBuffer = new Collider[50];
        private float scanTick = 1f;

        public void Awake()
        {
            original = gameObject.GetComponent<TNH_MagDuplicator>();

            if (original == null) TNHTweakerLogger.LogError("Mag Purchaser failed, original mag duplicator was null!");

            Button button = original.GetComponentInChildren<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(() => { ButtonPressed(); });

            original.enabled = false;

            original.OCIcon.Image.sprite = LoadedTemplateManager.PanelSprites[PanelType.MagPurchase];
            original.OCIcon.Sprite_Cancel = LoadedTemplateManager.PanelSprites[PanelType.MagPurchase];
        }


        public void ButtonPressed()
        {
            if (ammoObject == null || storedCost > original.M.GetNumTokens())
            {
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Fail, transform.position);
                return;
            }

            else
            {
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Spawn, transform.position);
                original.M.SubtractTokens(storedCost);
                original.M.Increment(10, false);

                Instantiate(ammoObject.GetGameObject(), original.Spawnpoint_Mag.position, original.Spawnpoint_Mag.rotation);
                
                ammoObject = null;
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

            ammoObject = null;

            for (int i = 0; i < colliderCount; i++)
            {
                if (colBuffer[i].attachedRigidbody != null)
                {
                    FVRFireArm firearm = colBuffer[i].GetComponent<FVRFireArm>();

                    if (firearm != null && !firearm.IsHeld && firearm.QuickbeltSlot == null)
                    {
                        //NOTE: We access IM.OD[] because the ObjectWrapper is not properly updated from caching for soME FUCKING REASON AHHHHHH
                        CustomCharacter character = LoadedTemplateManager.LoadedCharactersDict[original.M.C];
                        List<FVRObject> compatibleAmmoContainers = FirearmUtils.GetCompatibleAmmoContainers(firearm.ObjectWrapper);

                        MagazineBlacklistEntry blacklistEntry = null;
                        if (character.GetMagazineBlacklist().ContainsKey(firearm.ObjectWrapper.ItemID)) blacklistEntry = character.GetMagazineBlacklist()[firearm.ObjectWrapper.ItemID];

                        ammoObject = FirearmUtils.GetSmallestCapacityMagazine(compatibleAmmoContainers, blacklistEntry);

                        if (ammoObject != null)
                        {
                            SetCost();
                            return;
                        }
                    }
                }
            }

            SetCost();
        }

        private void SetCost()
        {
            if (ammoObject != null)
            {
                storedCost = (ammoObject.MagazineCapacity / 5) + 1;
                original.OCIcon.SetOption(TNH_ObjectConstructorIcon.IconState.Item, original.OCIcon.Sprite_Accept, storedCost);
            }
            else
            {
                storedCost = 0;
                original.OCIcon.SetOption(TNH_ObjectConstructorIcon.IconState.Cancel, original.OCIcon.Sprite_Cancel, storedCost);
            }
        }
    }

    



    public class AmmoPurchaser : MonoBehaviour
    {
        public TNH_MagDuplicator original;

        private FVRFireArm detectedFirearm = null;

        private int storedCost = 0;
        private Collider[] colBuffer = new Collider[50];
        private float scanTick = 1f;

        public void Awake()
        {
            original = gameObject.GetComponent<TNH_MagDuplicator>();

            if (original == null) TNHTweakerLogger.LogError("Ammo Purchaser failed, original mag duplicator was null!");

            Button button = original.GetComponentInChildren<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(() => { ButtonPressed(); });

            original.enabled = false;

            original.OCIcon.Image.sprite = LoadedTemplateManager.PanelSprites[PanelType.AmmoPurchase];
            original.OCIcon.Sprite_Cancel = LoadedTemplateManager.PanelSprites[PanelType.AmmoPurchase];
        }


        public void ButtonPressed()
        {
            if (detectedFirearm == null || storedCost > original.M.GetNumTokens())
            {
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Fail, transform.position);
                return;
            }

            else
            {
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Spawn, transform.position);
                original.M.SubtractTokens(storedCost);
                original.M.Increment(10, false);

                FVRObject.OTagFirearmRoundPower roundPower = AM.GetRoundPower(detectedFirearm.RoundType);
                int numSpawned = GetRoundsToSpawn(roundPower);

                TNHTweakerLogger.Log("Compatible rounds count for " + detectedFirearm.ObjectWrapper.ItemID + ": " + IM.OD[detectedFirearm.ObjectWrapper.ItemID].CompatibleSingleRounds.Count, TNHTweakerLogger.LogType.General);

                CustomCharacter character = LoadedTemplateManager.LoadedCharactersDict[original.M.C];
                MagazineBlacklistEntry blacklistEntry = null;
                if (character.GetMagazineBlacklist().ContainsKey(detectedFirearm.ObjectWrapper.ItemID)) blacklistEntry = character.GetMagazineBlacklist()[detectedFirearm.ObjectWrapper.ItemID];

                FVRObject compatibleRound = FirearmUtils.GetCompatibleRounds(detectedFirearm.ObjectWrapper, character.ValidAmmoEras, character.ValidAmmoSets, character.GlobalAmmoBlacklist, blacklistEntry).GetRandom();

                AnvilManager.Run(SpawnRounds(compatibleRound, numSpawned));

                detectedFirearm = null;
            }
        }

        public IEnumerator SpawnRounds(FVRObject bullet, int count)
        {
            GameObject bulletObject = bullet.GetGameObject();

            for(int i = 0; i < count; i++)
            {
                Instantiate(bulletObject, original.Spawnpoint_Mag.position + (Vector3.up * 0.02f * i), original.Spawnpoint_Mag.rotation);
                yield return null;
            }
            

        }

        public int GetRoundsToSpawn(FVRObject.OTagFirearmRoundPower roundPower)
        {
            if (roundPower == FVRObject.OTagFirearmRoundPower.Shotgun) return 18;
            if (roundPower == FVRObject.OTagFirearmRoundPower.Pistol) return 30;
            if (roundPower == FVRObject.OTagFirearmRoundPower.Tiny) return 30;
            if (roundPower == FVRObject.OTagFirearmRoundPower.Intermediate) return 20;
            if (roundPower == FVRObject.OTagFirearmRoundPower.FullPower) return 16;
            if (roundPower == FVRObject.OTagFirearmRoundPower.Exotic) return 16;
            if (roundPower == FVRObject.OTagFirearmRoundPower.AntiMaterial) return 6;
            if (roundPower == FVRObject.OTagFirearmRoundPower.Ordnance) return 3;

            return 10;
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

            detectedFirearm = null;

            for (int i = 0; i < colliderCount; i++)
            {
                if (colBuffer[i].attachedRigidbody != null)
                {
                    FVRFireArm firearm = colBuffer[i].GetComponent<FVRFireArm>();

                    if (firearm != null && !firearm.IsHeld && firearm.QuickbeltSlot == null)
                    {
                        detectedFirearm = firearm;

                        SetCost();

                        return;
                    }
                }
            }

            SetCost();
        }

        private void SetCost()
        {
            if (detectedFirearm != null)
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

            if (original == null) TNHTweakerLogger.LogError("Full Auto Enabler failed, original Mag Duplicator was null!");

            Button button = original.GetComponentInChildren<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(() => { ButtonPressed(); });

            original.enabled = false;

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




    public class FireRateModifier : MonoBehaviour
    {
        public TNH_MagDuplicator original;
        public PanelType panelType;

        public Handgun detectedHandgun = null;
        public ClosedBoltWeapon detectedClosedBolt = null;
        public OpenBoltReceiver detectedOpenBolt = null;

        private int storedCost = 0;
        private Collider[] colBuffer = new Collider[50];
        private float scanTick = 1f;

        private float fireRateMultiplier = 2;

        public void Awake()
        {
            original = gameObject.GetComponent<TNH_MagDuplicator>();

            if (original == null) {
                TNHTweakerLogger.LogError("Fire Rate Modifier failed, original Mag Duplicator was null!");
                return;
            }

            Button button = original.GetComponentInChildren<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(() => { ButtonPressed(); });

            original.enabled = false;
        }

        public void Init(PanelType type)
        {
            panelType = type;
            original.OCIcon.Image.sprite = LoadedTemplateManager.PanelSprites[panelType];
            original.OCIcon.Sprite_Cancel = LoadedTemplateManager.PanelSprites[panelType];
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

                if (detectedHandgun != null)
                {
                    ApplyFireRateHandgun();
                    detectedHandgun = null;
                }

                else if (detectedClosedBolt != null)
                {
                    ApplyFireRateClosedBolt();
                    detectedClosedBolt = null;
                }

                else if (detectedOpenBolt != null)
                {
                    ApplyFireRateOpenBolt();
                    detectedOpenBolt = null;
                }
            }
        }


        public void ApplyFireRateHandgun()
        {
            //TNHTweakerLogger.Log("Before: (Stiff=" + detectedHandgun.Slide.SpringStiffness + ") (Rearward=" + detectedHandgun.Slide.Speed_Rearward + ")", TNHTweakerLogger.LogType.General);

            if (panelType == PanelType.FireRateUp)
            {
                detectedHandgun.Slide.SpringStiffness *= fireRateMultiplier;
                detectedHandgun.Slide.Speed_Rearward *= fireRateMultiplier;
                detectedHandgun.Slide.Speed_Forward *= fireRateMultiplier;
            }
            else if (panelType == PanelType.FireRateDown)
            {
                detectedHandgun.Slide.SpringStiffness *= (1f / fireRateMultiplier);
                detectedHandgun.Slide.Speed_Rearward *= (1f / fireRateMultiplier);
                detectedHandgun.Slide.Speed_Forward *= (1f / fireRateMultiplier);
            }
            
            //TNHTweakerLogger.Log("After: (Stiff=" + detectedHandgun.Slide.SpringStiffness + ") (Rearward=" + detectedHandgun.Slide.Speed_Rearward + ")", TNHTweakerLogger.LogType.General);
        }

        public void ApplyFireRateClosedBolt()
        {
            if (panelType == PanelType.FireRateUp)
            {
                detectedClosedBolt.Bolt.SpringStiffness *= fireRateMultiplier;
                detectedClosedBolt.Bolt.Speed_Forward *= fireRateMultiplier;
                detectedClosedBolt.Bolt.Speed_Rearward *= fireRateMultiplier;
            }
            else if (panelType == PanelType.FireRateDown)
            {
                detectedClosedBolt.Bolt.SpringStiffness *= (1f / fireRateMultiplier);
                detectedClosedBolt.Bolt.Speed_Rearward *= (1f / fireRateMultiplier);
                detectedClosedBolt.Bolt.Speed_Forward *= (1f / fireRateMultiplier);
            }
        }

        public void ApplyFireRateOpenBolt()
        {
            if (panelType == PanelType.FireRateUp)
            {
                detectedOpenBolt.Bolt.BoltSpringStiffness *= fireRateMultiplier;
                detectedOpenBolt.Bolt.BoltSpeed_Forward *= fireRateMultiplier;
                detectedOpenBolt.Bolt.BoltSpeed_Rearward *= fireRateMultiplier;
            }
            else if (panelType == PanelType.FireRateDown)
            {
                detectedOpenBolt.Bolt.BoltSpringStiffness *= (1f / fireRateMultiplier);
                detectedOpenBolt.Bolt.BoltSpeed_Forward *= (1f / fireRateMultiplier);
                detectedOpenBolt.Bolt.BoltSpeed_Rearward *= (1f / fireRateMultiplier);
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
                    if (handgun != null)
                    {
                        //Debug.Log("Hand gun detected!");
                        detectedHandgun = handgun;
                        SetCost();
                        return;
                    }

                    ClosedBoltWeapon closedBolt = colBuffer[i].GetComponent<ClosedBoltWeapon>();
                    if (closedBolt != null)
                    {
                        //Debug.Log("Closed bolt detected!");
                        detectedClosedBolt = closedBolt;
                        SetCost();
                        return;
                    }

                    OpenBoltReceiver openBolt = colBuffer[i].GetComponent<OpenBoltReceiver>();
                    if (openBolt != null)
                    {
                        //Debug.Log("Open bolt detected!");
                        detectedOpenBolt = openBolt;
                        SetCost();
                        return;
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
            if (detectedHandgun != null || detectedClosedBolt != null || detectedOpenBolt != null)
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
    }

    */


}
