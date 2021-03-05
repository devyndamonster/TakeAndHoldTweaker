using Deli.Newtonsoft.Json;
using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.Utilities;
using UnityEngine;

namespace TNHTweaker.ObjectTemplates
{

    public class SavedGunSerializable
    {
        public string FileName;
        public List<FireArmRoundClass> LoadedRoundsInMag;
        public List<FireArmRoundClass> LoadedRoundsInChambers;
        public List<string> SavedFlags;
        public bool OverrideFireRate;
        public float SpeedRearward;
        public float SpeedForward;
        public float SpringStiffness;
        public bool OverrideFireSelectors;
        public List<FireSelectorMode> FireSelectorModes;
        public List<SavedGunComponentSerializable> Components;

        [JsonIgnore]
        private SavedGun gun;

        public SavedGunSerializable() { }

        public SavedGunSerializable(SavedGun gun)
        {
            FileName = gun.FileName;
            Components = gun.Components.Select(o => new SavedGunComponentSerializable(o)).ToList();
            LoadedRoundsInMag = gun.LoadedRoundsInMag;
            LoadedRoundsInChambers = gun.LoadedRoundsInChambers;
            SavedFlags = gun.SavedFlags;

            FireSelectorModes = new List<FireSelectorMode>();
            LoadFirearmProperties();

            this.gun = gun;
        }

        public SavedGun GetSavedGun()
        {
            if(gun == null)
            {
                gun = new SavedGun();
                gun.FileName = FileName;
                gun.Components = Components.Select(o => o.GetGunComponent()).ToList();
                gun.LoadedRoundsInMag = LoadedRoundsInMag;
                gun.LoadedRoundsInChambers = LoadedRoundsInChambers;
                gun.SavedFlags = SavedFlags;
                gun.DateMade = default(DateTime);
            }

            return gun;
        }

        public bool AllComponentsLoaded()
        {
            foreach(SavedGunComponentSerializable component in Components)
            {
                if (!IM.OD.ContainsKey(component.ObjectID))
                {
                    return false;
                }
            }

            return true;
        }

        public FVRObject GetGunObject()
        {
            foreach(SavedGunComponentSerializable component in Components)
            {
                if (component.IsFirearm) return IM.OD[component.ObjectID];
            }

            return null;
        }

        private void LoadFirearmProperties()
        {
            GameObject gunObject = GetGunObject().GetGameObject();

            Handgun handgunComp = gunObject.GetComponent<Handgun>();
            if(handgunComp != null)
            {
                foreach(Handgun.FireSelectorMode mode in handgunComp.FireSelectorModes)
                {
                    FireSelectorModes.Add(new FireSelectorMode(mode));
                }

                SpeedForward = handgunComp.Slide.Speed_Forward;
                SpeedRearward = handgunComp.Slide.Speed_Rearward;
                SpringStiffness = handgunComp.Slide.SpringStiffness;

                return;
            }

            ClosedBoltWeapon closedBoltComp = gunObject.GetComponent<ClosedBoltWeapon>();
            if (closedBoltComp != null)
            {
                foreach (ClosedBoltWeapon.FireSelectorMode mode in closedBoltComp.FireSelector_Modes)
                {
                    FireSelectorModes.Add(new FireSelectorMode(mode));
                }

                SpeedForward = closedBoltComp.Bolt.Speed_Forward;
                SpeedRearward = closedBoltComp.Bolt.Speed_Rearward;
                SpringStiffness = closedBoltComp.Bolt.SpringStiffness;

                return;
            }

            OpenBoltReceiver openBoltComp = gunObject.GetComponent<OpenBoltReceiver>();
            if (openBoltComp != null)
            {
                foreach (OpenBoltReceiver.FireSelectorMode mode in openBoltComp.FireSelector_Modes)
                {
                    FireSelectorModes.Add(new FireSelectorMode(mode));
                }

                SpeedForward = openBoltComp.Bolt.BoltSpeed_Forward;
                SpeedRearward = openBoltComp.Bolt.BoltSpeed_Rearward;
                SpringStiffness = openBoltComp.Bolt.BoltSpringStiffness;

                return;
            }
        }

        public void ApplyFirearmProperties(FVRFireArm firearm)
        {
            if (!OverrideFireRate && !OverrideFireSelectors) return;

            Handgun handgunComp = firearm.gameObject.GetComponent<Handgun>();
            if (handgunComp != null)
            {

                if (OverrideFireSelectors)
                {
                    List<Handgun.FireSelectorMode> modeList = new List<Handgun.FireSelectorMode>();
                    foreach (FireSelectorMode mode in FireSelectorModes)
                    {
                        modeList.Add(mode.GetHandgunMode());
                    }
                    handgunComp.FireSelectorModes = modeList.ToArray();
                }

                if (OverrideFireRate)
                {
                    handgunComp.Slide.Speed_Forward = SpeedForward;
                    handgunComp.Slide.Speed_Rearward = SpeedRearward;
                    handgunComp.Slide.SpringStiffness = SpringStiffness;
                }

                return;
            }

            ClosedBoltWeapon closedBoltComp = firearm.gameObject.GetComponent<ClosedBoltWeapon>();
            if (closedBoltComp != null)
            {
                if (OverrideFireSelectors)
                {
                    List<ClosedBoltWeapon.FireSelectorMode> modeList = new List<ClosedBoltWeapon.FireSelectorMode>();
                    foreach (FireSelectorMode mode in FireSelectorModes)
                    {
                        modeList.Add(mode.GetClosedBoltMode());
                    }
                    closedBoltComp.FireSelector_Modes = modeList.ToArray();
                }

                if (OverrideFireRate)
                {
                    closedBoltComp.Bolt.Speed_Forward = SpeedForward;
                    closedBoltComp.Bolt.Speed_Rearward = SpeedRearward;
                    closedBoltComp.Bolt.SpringStiffness = SpringStiffness;
                }

                return;
            }

            OpenBoltReceiver openBoltComp = firearm.gameObject.GetComponent<OpenBoltReceiver>();
            if (openBoltComp != null)
            {
                if (OverrideFireSelectors)
                {
                    List<OpenBoltReceiver.FireSelectorMode> modeList = new List<OpenBoltReceiver.FireSelectorMode>();
                    foreach (FireSelectorMode mode in FireSelectorModes)
                    {
                        modeList.Add(mode.GetOpenBoltMode());
                    }
                    openBoltComp.FireSelector_Modes = modeList.ToArray();
                }
                
                if (OverrideFireRate)
                {
                    openBoltComp.Bolt.BoltSpeed_Forward = SpeedForward;
                    openBoltComp.Bolt.BoltSpeed_Rearward = SpeedRearward;
                    openBoltComp.Bolt.BoltSpringStiffness = SpringStiffness;
                }

                return;
            }
        }

    }




    public class SavedGunComponentSerializable
    {
        public int Index;
        public string ObjectID;
        public Vector3Serializable PosOffset;
        public Vector3Serializable OrientationForward;
        public Vector3Serializable OrientationUp;
        public int ObjectAttachedTo;
        public int MountAttachedTo;
        public bool IsFirearm;
        public bool IsMagazine;
        public bool IsAttachment;
        public Dictionary<string, string> Flags;

        [JsonIgnore]
        private SavedGunComponent component;

        public SavedGunComponentSerializable() { }

        public SavedGunComponentSerializable(SavedGunComponent component)
        {
            Index = component.Index;
            ObjectID = component.ObjectID;
            PosOffset = new Vector3Serializable(component.PosOffset);
            OrientationForward = new Vector3Serializable(component.OrientationForward);
            OrientationUp = new Vector3Serializable(component.OrientationUp);
            ObjectAttachedTo = component.ObjectAttachedTo;
            MountAttachedTo = component.MountAttachedTo;
            IsFirearm = component.isFirearm;
            IsMagazine = component.isMagazine;
            IsAttachment = component.isAttachment;
            Flags = component.Flags;

            this.component = component;
        }

        public SavedGunComponent GetGunComponent()
        {
            if(component == null)
            {
                component = new SavedGunComponent();
                component.Index = Index;
                component.ObjectID = ObjectID;
                component.PosOffset = PosOffset.GetVector3();
                component.OrientationForward = OrientationForward.GetVector3();
                component.OrientationUp = OrientationUp.GetVector3();
                component.ObjectAttachedTo = ObjectAttachedTo;
                component.MountAttachedTo = MountAttachedTo;
                component.isFirearm = IsFirearm;
                component.isMagazine = IsMagazine;
                component.isAttachment = IsAttachment;
                component.Flags = Flags;
            }

            return component;
        }

    }


    public class FireSelectorMode
    {
        public float SelectorPosition;
        public FireSelectorModeType ModeType;
        public int BurstAmount;

        public FireSelectorMode() { }

        public FireSelectorMode(Handgun.FireSelectorMode mode)
        {
            SelectorPosition = mode.SelectorPosition;
            ModeType = (FireSelectorModeType)Enum.Parse(typeof(FireSelectorModeType), mode.ModeType.ToString());
            BurstAmount = mode.BurstAmount;
        }

        public FireSelectorMode(ClosedBoltWeapon.FireSelectorMode mode)
        {
            SelectorPosition = mode.SelectorPosition;
            ModeType = (FireSelectorModeType)Enum.Parse(typeof(FireSelectorModeType), mode.ModeType.ToString());
            BurstAmount = mode.BurstAmount;
        }

        public FireSelectorMode(OpenBoltReceiver.FireSelectorMode mode)
        {
            SelectorPosition = mode.SelectorPosition;
            ModeType = (FireSelectorModeType)Enum.Parse(typeof(FireSelectorModeType), mode.ModeType.ToString());
            BurstAmount = -1;
        }

        public Handgun.FireSelectorMode GetHandgunMode()
        {
            Handgun.FireSelectorMode mode = new Handgun.FireSelectorMode();
            mode.SelectorPosition = SelectorPosition;
            mode.ModeType = (Handgun.FireSelectorModeType)Enum.Parse(typeof(Handgun.FireSelectorModeType), ModeType.ToString());
            mode.BurstAmount = BurstAmount;
            return mode;
        }

        public OpenBoltReceiver.FireSelectorMode GetOpenBoltMode()
        {
            OpenBoltReceiver.FireSelectorMode mode = new OpenBoltReceiver.FireSelectorMode();
            mode.SelectorPosition = SelectorPosition;
            mode.ModeType = (OpenBoltReceiver.FireSelectorModeType)Enum.Parse(typeof(OpenBoltReceiver.FireSelectorModeType), ModeType.ToString());
            return mode;
        }

        public ClosedBoltWeapon.FireSelectorMode GetClosedBoltMode()
        {
            ClosedBoltWeapon.FireSelectorMode mode = new ClosedBoltWeapon.FireSelectorMode();
            mode.SelectorPosition = SelectorPosition;
            mode.ModeType = (ClosedBoltWeapon.FireSelectorModeType)Enum.Parse(typeof(ClosedBoltWeapon.FireSelectorModeType), ModeType.ToString());
            mode.BurstAmount = BurstAmount;
            return mode;
        }
    }


    public enum FireSelectorModeType
    {
        Safe,
        Single,
        Burst,
        FullAuto,
        SuperFastBurst
    }

}
