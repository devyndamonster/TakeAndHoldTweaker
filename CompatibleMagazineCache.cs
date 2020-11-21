using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Valve.Newtonsoft.Json;

namespace FistVR
{
    public class CompatibleMagazineCache
    {
        public List<string> Firearms;
        public List<string> Magazines;
        public List<MagazineCacheEntry> Entries;

        [JsonIgnore]
        public List<FVRFireArmMagazine> MagazineObjects;

        public CompatibleMagazineCache()
        {
            Firearms = new List<string>();
            Magazines = new List<string>();
            Entries = new List<MagazineCacheEntry>();
            MagazineObjects = new List<FVRFireArmMagazine>();
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
}
