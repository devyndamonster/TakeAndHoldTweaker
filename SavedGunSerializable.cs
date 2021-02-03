using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Valve.Newtonsoft.Json;

namespace TNHTweaker
{

    public class SavedGunSerializable
    {
        public string FileName;
        public List<SavedGunComponentSerializable> Components;
        public List<FireArmRoundClass> LoadedRoundsInMag;
        public List<FireArmRoundClass> LoadedRoundsInChambers;
        public List<string> SavedFlags;

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
}
