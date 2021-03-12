using Deli.Newtonsoft.Json;
using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TNHTweaker
{
    public class CompatibleMagazineCache
    {
        public List<string> Firearms;
        public List<string> Magazines;
        public List<string> Clips;
        public List<string> Bullets;
        public List<MagazineCacheEntry> Entries;
        public Dictionary<FireArmMagazineType, List<AmmoObjectDataTemplate>> MagazineData;
        public Dictionary<FireArmClipType, List<AmmoObjectDataTemplate>> ClipData;
        public Dictionary<FireArmRoundType, List<AmmoObjectDataTemplate>> BulletData;

        [JsonIgnore]
        public List<FVRFireArmMagazine> MagazineObjects;
        [JsonIgnore]
        public List<FVRFireArmClip> ClipObjects;
        [JsonIgnore]
        public List<FVRFireArmRound> BulletObjects;

        public CompatibleMagazineCache()
        {
            Firearms = new List<string>();
            Magazines = new List<string>();
            Clips = new List<string>();
            Bullets = new List<string>();
            Entries = new List<MagazineCacheEntry>();
            MagazineData = new Dictionary<FireArmMagazineType, List<AmmoObjectDataTemplate>>();
            ClipData = new Dictionary<FireArmClipType, List<AmmoObjectDataTemplate>>();
            BulletData = new Dictionary<FireArmRoundType, List<AmmoObjectDataTemplate>>();
            MagazineObjects = new List<FVRFireArmMagazine>();
            ClipObjects = new List<FVRFireArmClip>();
            BulletObjects = new List<FVRFireArmRound>();

        }

        public void AddMagazineData(FVRFireArmMagazine mag)
        {
            if (!MagazineData.ContainsKey(mag.MagazineType))
            {
                MagazineData.Add(mag.MagazineType, new List<AmmoObjectDataTemplate>());
            }

            MagazineData[mag.MagazineType].Add(new AmmoObjectDataTemplate(mag));
        }

        public void AddClipData(FVRFireArmClip clip)
        {
            if (!ClipData.ContainsKey(clip.ClipType))
            {
                ClipData.Add(clip.ClipType, new List<AmmoObjectDataTemplate>());
            }

            ClipData[clip.ClipType].Add(new AmmoObjectDataTemplate(clip));
        }

        public void AddBulletData(FVRFireArmRound bullet)
        {
            if (!BulletData.ContainsKey(bullet.RoundType))
            {
                BulletData.Add(bullet.RoundType, new List<AmmoObjectDataTemplate>());
            }

            BulletData[bullet.RoundType].Add(new AmmoObjectDataTemplate(bullet));
        }
    }

    public class MagazineCacheEntry
    {
        public string FirearmID;
        public int MinAmmo;
        public int MaxAmmo;
        public List<string> CompatibleMagazines;
        public List<string> CompatibleClips;
        public List<string> CompatibleBullets;

        public MagazineCacheEntry()
        {
            CompatibleMagazines = new List<string>();
            CompatibleClips = new List<string>();
            CompatibleBullets = new List<string>();
        }
    }

    public class AmmoObjectDataTemplate
    {
        public string ObjectID;
        public int Capacity;

        [JsonIgnore]
        public FVRObject AmmoObject;
        
        public AmmoObjectDataTemplate() { }

        public AmmoObjectDataTemplate(FVRFireArmMagazine mag)
        {
            ObjectID = mag.ObjectWrapper.ItemID;
            Capacity = mag.m_capacity;
            AmmoObject = mag.ObjectWrapper;
        }

        public AmmoObjectDataTemplate(FVRFireArmClip clip)
        {
            ObjectID = clip.ObjectWrapper.ItemID;
            Capacity = clip.m_capacity;
            AmmoObject = clip.ObjectWrapper;
        }

        public AmmoObjectDataTemplate(FVRFireArmRound bullet)
        {
            ObjectID = bullet.ObjectWrapper.ItemID;
            Capacity = -1;
            AmmoObject = bullet.ObjectWrapper;
        }
    }


    public class MagazineBlacklistEntry
    {
        public List<string> MagazineBlacklist;
        public List<string> ClipBlacklist;

        public MagazineBlacklistEntry()
        {
            MagazineBlacklist = new List<string>();
            ClipBlacklist = new List<string>();
        }

    }

}
