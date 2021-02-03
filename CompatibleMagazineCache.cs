using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Valve.Newtonsoft.Json;

namespace TNHTweaker
{
    public class CompatibleMagazineCache
    {
        public List<string> Firearms;
        public List<string> Magazines;
        public List<MagazineCacheEntry> Entries;
        public Dictionary<FireArmMagazineType, List<MagazineDataTemplate>> MagazineData;

        [JsonIgnore]
        public List<FVRFireArmMagazine> MagazineObjects;

        public CompatibleMagazineCache()
        {
            Firearms = new List<string>();
            Magazines = new List<string>();
            Entries = new List<MagazineCacheEntry>();
            MagazineData = new Dictionary<FireArmMagazineType, List<MagazineDataTemplate>>();
            MagazineObjects = new List<FVRFireArmMagazine>();
        }

        public void AddMagazineData(FVRFireArmMagazine mag)
        {
            if (!MagazineData.ContainsKey(mag.MagazineType))
            {
                MagazineData.Add(mag.MagazineType, new List<MagazineDataTemplate>());
            }

            MagazineData[mag.MagazineType].Add(new MagazineDataTemplate(mag));
        }
    }

    public class MagazineCacheEntry
    {
        public string FirearmID;
        public int MinAmmo;
        public int MaxAmmo;
        public List<string> CompatibleMagazines;

        public MagazineCacheEntry()
        {
            CompatibleMagazines = new List<string>();
        }
    }

    public class MagazineDataTemplate
    {
        public string ObjectID;
        public int Capacity;

        public MagazineDataTemplate() { }

        public MagazineDataTemplate(FVRFireArmMagazine mag)
        {
            ObjectID = mag.ObjectWrapper.ItemID;
            Capacity = mag.m_capacity;
        }
    }
}
