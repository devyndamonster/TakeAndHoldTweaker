using FistVR;
using MagazinePatcher;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public class MagazinePanel : MonoBehaviour
    {
        public TNH_MagDuplicator original;

        public Dictionary<string, MagazineBlacklistEntry> blacklist;
        public int DupeCost = 2;
        public int UpgradeCost = 3;
        public int PurchaseCost = 2;

        public static Sprite background;

        private TNH_ObjectConstructorIcon DupeIcon;
        private TNH_ObjectConstructorIcon UpgradeIcon;
        private TNH_ObjectConstructorIcon PurchaseIcon;

        private Text priceText_0;
        private Text priceText_1;
        private Text priceText_2;

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
            original.enabled = false;

            blacklist = LoadedTemplateManager.LoadedCharactersDict[original.M.C].GetMagazineBlacklist();

            InitPanel();
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
            iconTransform_2.localPosition = new Vector3(275, -200, 0);

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

            priceText_0 = AddPriceText(iconTransform_0, new Vector3(-235, 155, 0));
            priceText_1 = AddPriceText(iconTransform_1, new Vector3(40, 155, 0));
            priceText_2 = AddPriceText(iconTransform_2, new Vector3(355, 150, 0));
            priceText_2.alignment = TextAnchor.MiddleLeft;

            priceText_0.text = "x" + DupeCost;
            priceText_1.text = "x" + UpgradeCost;
            priceText_2.text = "x" + PurchaseCost;
        }


        private Text AddPriceText(Transform iconTransform, Vector3 localPosition)
        {
            GameObject canvas = new GameObject("PriceCanvas");
            canvas.transform.SetParent(iconTransform.parent);
            canvas.transform.rotation = iconTransform.rotation;
            canvas.transform.localPosition = localPosition;

            Canvas canvasComp = canvas.AddComponent<Canvas>();
            RectTransform rect = canvasComp.GetComponent<RectTransform>();
            canvasComp.renderMode = RenderMode.WorldSpace;
            rect.sizeDelta = new Vector2(1, 1);

            GameObject text = new GameObject("Text");
            text.transform.SetParent(canvas.transform);
            text.transform.rotation = canvas.transform.rotation;
            text.transform.localPosition = Vector3.zero;

            text.AddComponent<CanvasRenderer>();
            Text textComp = text.AddComponent<Text>();
            Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");

            textComp.text = "x?";
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.fontSize = 30;
            textComp.fontStyle = FontStyle.Bold;
            text.transform.localScale = new Vector3(0.0015f, 0.0015f, 0.0015f);
            textComp.font = ArialFont;
            textComp.horizontalOverflow = HorizontalWrapMode.Overflow;

            return textComp;
        }

        private void DupeMagButton()
        {
            if((detectedMag == null && detectedSpeedLoader == null) || original.M.GetNumTokens() < DupeCost)
            {
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Fail, transform.position);
            }

            else
            {
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Spawn, transform.position);
                original.M.SubtractTokens(DupeCost);
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
                UpdateIcons();
            }
        }

        private void UpgradeMagButton()
        {
            if (upgradeMag == null || original.M.GetNumTokens() < UpgradeCost)
            {
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Fail, transform.position);
            }

            else
            {
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Spawn, transform.position);
                original.M.SubtractTokens(UpgradeCost);
                original.M.Increment(10, false);

                Destroy(detectedMag.GameObject);
                Instantiate(upgradeMag.GetGameObject(), original.Spawnpoint_Mag.position, original.Spawnpoint_Mag.rotation);

                upgradeMag = null;
                detectedMag = null;
                UpdateIcons();
            }
        }

        private void PurchaseMagButton()
        {
            if (purchaseMag == null || original.M.GetNumTokens() < PurchaseCost)
            {
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Fail, transform.position);
            }

            else
            {
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Spawn, transform.position);
                original.M.SubtractTokens(PurchaseCost);
                original.M.Increment(10, false);

                Instantiate(purchaseMag.GetGameObject(), original.Spawnpoint_Mag.position, original.Spawnpoint_Mag.rotation);

                purchaseMag = null;
                UpdateIcons();
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
                    UpdateIcons();
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




    public class AmmoPurchasePanel : MonoBehaviour
    {
        public TNH_MagDuplicator original;
        public Dictionary<string, MagazineBlacklistEntry> blacklist;
        public int PanelCost = 1;

        public static Sprite background;

        private TNH_ObjectConstructorIcon PurchaseIcon;

        private FVRFireArm detectedFirearm = null;
        private Collider[] colBuffer = new Collider[50];
        private float scanTick = 1f;

        public void Awake()
        {
            original = gameObject.GetComponent<TNH_MagDuplicator>();
            if (original == null) TNHTweakerLogger.LogError("Ammo Purchaser failed, original mag duplicator was null!");
            original.enabled = false;

            InitPanel();
            UpdateIcons();
        }


        private void InitPanel()
        {
            Transform backingTransform = original.transform.Find("_CanvasHolder/_UITest_Canvas/Backing");

            Transform canvasHolder = original.transform.Find("_CanvasHolder/_UITest_Canvas");

            Transform iconTransform_0 = canvasHolder.Find("Icon_0");
            iconTransform_0.localPosition = new Vector3(0, -290, 0);

            Transform iconTransform_1 = canvasHolder.Find("Icon_1");
            Destroy(iconTransform_1.gameObject);

            Transform buttonTransform_0 = original.transform.Find("PointableButton_0");
            buttonTransform_0.position = iconTransform_0.position;

            Transform buttonTransform_1 = original.transform.Find("PointableButton_1");
            Destroy(buttonTransform_1.gameObject);

            Image backgroundImage = backingTransform.gameObject.GetComponent<Image>();
            backgroundImage.sprite = background;
;
            PurchaseIcon = iconTransform_0.gameObject.GetComponent<TNH_ObjectConstructorIcon>();

            Button button_0 = buttonTransform_0.gameObject.GetComponent<Button>();
            button_0.onClick = new Button.ButtonClickedEvent();
            button_0.onClick.AddListener(() => { PurchaseAmmoButton(); });
        }


        public void PurchaseAmmoButton()
        {
            if (detectedFirearm == null || original.M.GetNumTokens() < PanelCost)
            {
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Fail, transform.position);
                return;
            }

            else
            {
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Spawn, transform.position);
                original.M.SubtractTokens(PanelCost);
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
                UpdateIcons();
            }
        }

        public IEnumerator SpawnRounds(FVRObject bullet, int count)
        {
            GameObject bulletObject = bullet.GetGameObject();
            return TNHTweakerUtils.InstantiateMutltiple(bulletObject, original.Spawnpoint_Mag.position, count);;
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
                    UpdateIcons();
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
                        return;
                    }
                }
            }
        }

        private void UpdateIcons()
        {
            PurchaseIcon.State = TNH_ObjectConstructorIcon.IconState.Cancel;

            if (detectedFirearm != null) PurchaseIcon.State = TNH_ObjectConstructorIcon.IconState.Accept;

            PurchaseIcon.UpdateIconDisplay();
        }
    }


    public class FullAutoPanel : MonoBehaviour
    {
        public TNH_MagDuplicator original;
        public int PanelCost = 4;

        public static Sprite background;

        private TNH_ObjectConstructorIcon PurchaseIcon;

        public Handgun detectedHandgun = null;
        public ClosedBoltWeapon detectedClosedBolt = null;
        public OpenBoltReceiver detectedOpenBolt = null;

        private Collider[] colBuffer = new Collider[50];
        private float scanTick = 1f;

        public void Awake()
        {
            original = gameObject.GetComponent<TNH_MagDuplicator>();
            if (original == null) TNHTweakerLogger.LogError("Full Auto Enabler failed, original Mag Duplicator was null!");
            original.enabled = false;

            InitPanel();
            UpdateIcons();
        }

        private void InitPanel()
        {
            Transform backingTransform = original.transform.Find("_CanvasHolder/_UITest_Canvas/Backing");

            Transform canvasHolder = original.transform.Find("_CanvasHolder/_UITest_Canvas");

            Transform iconTransform_0 = canvasHolder.Find("Icon_0");
            iconTransform_0.localPosition = new Vector3(0, -290, 0);

            Transform iconTransform_1 = canvasHolder.Find("Icon_1");
            Destroy(iconTransform_1.gameObject);

            Transform buttonTransform_0 = original.transform.Find("PointableButton_0");
            buttonTransform_0.position = iconTransform_0.position;

            Transform buttonTransform_1 = original.transform.Find("PointableButton_1");
            Destroy(buttonTransform_1.gameObject);

            Image backgroundImage = backingTransform.gameObject.GetComponent<Image>();
            backgroundImage.sprite = background;

            PurchaseIcon = iconTransform_0.gameObject.GetComponent<TNH_ObjectConstructorIcon>();

            Button button_0 = buttonTransform_0.gameObject.GetComponent<Button>();
            button_0.onClick = new Button.ButtonClickedEvent();
            button_0.onClick.AddListener(() => { AddFullAutoButton(); });
        }


        public void AddFullAutoButton()
        {
            if ((detectedHandgun == null && detectedClosedBolt == null && detectedOpenBolt == null) || PanelCost > original.M.GetNumTokens())
            {
                //Debug.Log("Can't add full auto!");
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Fail, transform.position);
                return;
            }

            else
            {
                //Debug.Log("Adding full auto!");
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Spawn, transform.position);
                original.M.SubtractTokens(PanelCost);
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
                    UpdateIcons();
                }
            }
        }

        private void Scan()
        {
            detectedHandgun = null;
            detectedClosedBolt = null;
            detectedOpenBolt = null;

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
                            return;
                        }
                    }
                }
            }
        }

        private void UpdateIcons()
        {
            PurchaseIcon.State = TNH_ObjectConstructorIcon.IconState.Cancel;

            if (detectedHandgun != null || detectedClosedBolt != null || detectedOpenBolt != null) PurchaseIcon.State = TNH_ObjectConstructorIcon.IconState.Accept;

            PurchaseIcon.UpdateIconDisplay();
        }
    }


    public class FireRatePanel : MonoBehaviour
    {
        public TNH_MagDuplicator original;

        public static Sprite background;
        public static Sprite plusSprite;
        public static Sprite minusSprite;

        private TNH_ObjectConstructorIcon PlusIcon;
        private TNH_ObjectConstructorIcon MinusIcon;

        public Handgun detectedHandgun = null;
        public ClosedBoltWeapon detectedClosedBolt = null;
        public OpenBoltReceiver detectedOpenBolt = null;

        private int PanelCost = 1;
        private Collider[] colBuffer = new Collider[50];
        private float scanTick = 1f;

        private float fireRateMultiplier = 1.5f;

        public void Awake()
        {
            original = gameObject.GetComponent<TNH_MagDuplicator>();
            if (original == null) TNHTweakerLogger.LogError("Fire Rate Modifier failed, original Mag Duplicator was null!");
            original.enabled = false;

            InitPanel();
            UpdateIcons();
        }

        public void InitPanel()
        {
            Transform backingTransform = original.transform.Find("_CanvasHolder/_UITest_Canvas/Backing");

            Transform canvasHolder = original.transform.Find("_CanvasHolder/_UITest_Canvas");

            Transform iconTransform_0 = canvasHolder.Find("Icon_0");
            iconTransform_0.localPosition = new Vector3(-165, -290, 0);

            Transform iconTransform_1 = canvasHolder.Find("Icon_1");
            iconTransform_1.localPosition = new Vector3(165, -290, 0);

            Transform buttonTransform_0 = original.transform.Find("PointableButton_0");
            buttonTransform_0.position = iconTransform_0.position;

            Transform buttonTransform_1 = original.transform.Find("PointableButton_1");
            buttonTransform_1.position = iconTransform_1.position;

            Image backgroundImage = backingTransform.gameObject.GetComponent<Image>();

            backgroundImage.sprite = background;

            MinusIcon = iconTransform_0.gameObject.GetComponent<TNH_ObjectConstructorIcon>();
            PlusIcon = iconTransform_1.gameObject.GetComponent<TNH_ObjectConstructorIcon>();

            MinusIcon.Sprite_Accept = minusSprite;
            PlusIcon.Sprite_Accept = plusSprite;

            Button button_0 = buttonTransform_0.gameObject.GetComponent<Button>();
            button_0.onClick = new Button.ButtonClickedEvent();
            button_0.onClick.AddListener(() => { DecreaseFireRateButton(); });

            Button button_1 = buttonTransform_1.gameObject.GetComponent<Button>();
            button_1.onClick = new Button.ButtonClickedEvent();
            button_1.onClick.AddListener(() => { IncreaseFireRateButton(); });
        }


        public void IncreaseFireRateButton()
        {
            if ((detectedHandgun == null && detectedClosedBolt == null && detectedOpenBolt == null) || PanelCost > original.M.GetNumTokens())
            {
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Fail, transform.position);
                return;
            }

            else
            {
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Spawn, transform.position);
                original.M.SubtractTokens(PanelCost);
                original.M.Increment(10, false);

                IncreaseFireRate();
                detectedHandgun = null;
                detectedOpenBolt = null;
                detectedClosedBolt = null;
            }
        }

        public void DecreaseFireRateButton()
        {
            if ((detectedHandgun == null && detectedClosedBolt == null && detectedOpenBolt == null) || PanelCost > original.M.GetNumTokens())
            {
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Fail, transform.position);
                return;
            }

            else
            {
                SM.PlayCoreSound(FVRPooledAudioType.UIChirp, original.AudEvent_Spawn, transform.position);
                original.M.SubtractTokens(PanelCost);
                original.M.Increment(10, false);

                DecreaseFireRate();
                detectedHandgun = null;
                detectedOpenBolt = null;
                detectedClosedBolt = null;
            }
        }


        public void IncreaseFireRate()
        {
            if (detectedHandgun != null)
            {
                detectedHandgun.Slide.SpringStiffness *= fireRateMultiplier;
                detectedHandgun.Slide.Speed_Rearward *= fireRateMultiplier;
                detectedHandgun.Slide.Speed_Forward *= fireRateMultiplier;
                return;
            }

            else if (detectedClosedBolt != null)
            {
                detectedClosedBolt.Bolt.SpringStiffness *= fireRateMultiplier;
                detectedClosedBolt.Bolt.Speed_Forward *= fireRateMultiplier;
                detectedClosedBolt.Bolt.Speed_Rearward *= fireRateMultiplier;
                return;
            }

            else
            {
                detectedOpenBolt.Bolt.BoltSpringStiffness *= fireRateMultiplier;
                detectedOpenBolt.Bolt.BoltSpeed_Forward *= fireRateMultiplier;
                detectedOpenBolt.Bolt.BoltSpeed_Rearward *= fireRateMultiplier;
                return;
            }


        }

        public void DecreaseFireRate()
        {
            if(detectedHandgun != null)
            {
                detectedHandgun.Slide.SpringStiffness *= (1f / fireRateMultiplier);
                detectedHandgun.Slide.Speed_Rearward *= (1f / fireRateMultiplier);
                detectedHandgun.Slide.Speed_Forward *= (1f / fireRateMultiplier);
                return;
            }

            else if(detectedClosedBolt != null)
            {
                detectedClosedBolt.Bolt.SpringStiffness *= (1f / fireRateMultiplier);
                detectedClosedBolt.Bolt.Speed_Rearward *= (1f / fireRateMultiplier);
                detectedClosedBolt.Bolt.Speed_Forward *= (1f / fireRateMultiplier);
                return;
            }

            else
            {
                detectedOpenBolt.Bolt.BoltSpringStiffness *= (1f / fireRateMultiplier);
                detectedOpenBolt.Bolt.BoltSpeed_Forward *= (1f / fireRateMultiplier);
                detectedOpenBolt.Bolt.BoltSpeed_Rearward *= (1f / fireRateMultiplier);
                return;
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
                    UpdateIcons();
                }
            }
        }

        private void Scan()
        {
            detectedHandgun = null;
            detectedClosedBolt = null;
            detectedOpenBolt = null;

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
                        return;
                    }

                    ClosedBoltWeapon closedBolt = colBuffer[i].GetComponent<ClosedBoltWeapon>();
                    if (closedBolt != null)
                    {
                        //Debug.Log("Closed bolt detected!");
                        detectedClosedBolt = closedBolt;
                        return;
                    }

                    OpenBoltReceiver openBolt = colBuffer[i].GetComponent<OpenBoltReceiver>();
                    if (openBolt != null)
                    {
                        //Debug.Log("Open bolt detected!");
                        detectedOpenBolt = openBolt;
                        return;
                    }
                }
            }
        }

        private void UpdateIcons()
        {
            PlusIcon.State = TNH_ObjectConstructorIcon.IconState.Cancel;
            MinusIcon.State = TNH_ObjectConstructorIcon.IconState.Cancel;

            if (detectedHandgun != null || detectedClosedBolt != null || detectedOpenBolt != null)
            {
                PlusIcon.State = TNH_ObjectConstructorIcon.IconState.Accept;
                MinusIcon.State = TNH_ObjectConstructorIcon.IconState.Accept;
            }

            PlusIcon.UpdateIconDisplay();
            MinusIcon.UpdateIconDisplay();
        }
    }


}
