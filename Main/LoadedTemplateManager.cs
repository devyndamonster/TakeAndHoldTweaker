using Deli.Setup;
using Deli.VFS;
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
    public static class LoadedTemplateManager
    {

        public static Dictionary<TNH_CharacterDef, CustomCharacter> LoadedCharactersDict = new Dictionary<TNH_CharacterDef, CustomCharacter>();
        public static Dictionary<SosigEnemyTemplate, SosigTemplate> LoadedSosigsDict = new Dictionary<SosigEnemyTemplate, SosigTemplate>();
        public static Dictionary<EquipmentPoolDef.PoolEntry, EquipmentPool> EquipmentPoolDictionary = new Dictionary<EquipmentPoolDef.PoolEntry, EquipmentPool>();
        public static Dictionary<string, SavedGunSerializable> LoadedVaultFiles = new Dictionary<string, SavedGunSerializable>();
        public static Dictionary<PanelType, Sprite> PanelSprites = new Dictionary<PanelType, Sprite>();
        public static List<CustomCharacter> CustomCharacters = new List<CustomCharacter>();
        public static List<CustomCharacter> DefaultCharacters = new List<CustomCharacter>();
        public static List<SosigTemplate> CustomSosigs = new List<SosigTemplate>();
        public static List<SosigEnemyTemplate> DefaultSosigs = new List<SosigEnemyTemplate>();
        public static Dictionary<string, int> SosigIDDict = new Dictionary<string, int>();
        public static Dictionary<FireArmMagazineType, List<AmmoObjectDataTemplate>> LoadedMagazineTypeDict = new Dictionary<FireArmMagazineType, List<AmmoObjectDataTemplate>>();
        public static Dictionary<FireArmClipType, List<AmmoObjectDataTemplate>> LoadedClipTypeDict = new Dictionary<FireArmClipType, List<AmmoObjectDataTemplate>>();
        public static Dictionary<FireArmRoundType, List<AmmoObjectDataTemplate>> LoadedBulletTypeDict = new Dictionary<FireArmRoundType, List<AmmoObjectDataTemplate>>();
        public static Dictionary<string, AmmoObjectDataTemplate> LoadedMagazineDict = new Dictionary<string, AmmoObjectDataTemplate>();
        public static Dictionary<string, AmmoObjectDataTemplate> LoadedClipDict = new Dictionary<string, AmmoObjectDataTemplate>();
        public static Dictionary<string, AmmoObjectDataTemplate> LoadedBulletDict = new Dictionary<string, AmmoObjectDataTemplate>();

        public static int NewSosigID = 30000;
        public static int NewCharacterID = 30;

        public static Dictionary<string, Sprite> DefaultIconSprites = new Dictionary<string, Sprite>();

        /// <summary>
        /// Takes a custom SosigTemplate object, and adds it to the necessary dictionaries. This method assumes that you are sending a template for a custom sosig, and that it should be given a new the SosigEnemyID
        /// </summary>
        /// <param name="template">A template for a custom sosig (Loaded at runtime)</param>
        public static void AddSosigTemplate(SosigTemplate template)
        {
            SosigEnemyTemplate realTemplate = template.GetSosigEnemyTemplate();

            //Since this template is for a custom sosig, we should give it a brand new SosigEnemyID
            if (!SosigIDDict.ContainsKey(template.SosigEnemyID))
            {
                SosigIDDict.Add(template.SosigEnemyID, NewSosigID);
                NewSosigID += 1;
            }
            else
            {
                TNHTweakerLogger.LogError("TNHTweaker -- Loaded sosig had same SosigEnemyID as another sosig -- SosigEnemyID : " + template.SosigEnemyID);
                return;
            }

            //Now fill out the SosigEnemyIDs values for the real sosig template (These will effectively be ints, but this is ok since enums are just ints in disguise)
            realTemplate.SosigEnemyID = (SosigEnemyID)SosigIDDict[template.SosigEnemyID];

            //Finally add the templates to our global dictionary
            CustomSosigs.Add(template);
            LoadedSosigsDict.Add(realTemplate, template);

            TNHTweakerLogger.Log("TNHTweaker -- Sosig added successfuly : " + template.DisplayName, TNHTweakerLogger.LogType.Character);
        }


        public static void AddSosigTemplate(SosigEnemyTemplate realTemplate)
        {
            SosigTemplate template = new SosigTemplate(realTemplate);

            //This template is from a sogig that already has a valid SosigEnemyID, so we can just add that to the dictionary casted as an int
            if (!SosigIDDict.ContainsKey(template.SosigEnemyID))
            {
                SosigIDDict.Add(template.SosigEnemyID, (int)realTemplate.SosigEnemyID);
            }
            else
            {
                TNHTweakerLogger.LogError("TNHTweaker -- Loaded sosig had same SosigEnemyID as another sosig -- SosigEnemyID : " + template.SosigEnemyID);
                return;
            }

            //Since the real template already had a valid SosigEnemyID, we can skip the part where we reassign them
            DefaultSosigs.Add(realTemplate);
            LoadedSosigsDict.Add(realTemplate, template);

            TNHTweakerLogger.Log("TNHTweaker -- Sosig added successfuly : " + template.DisplayName, TNHTweakerLogger.LogType.Character);
        }


        public static void AddCharacterTemplate(CustomCharacter template, IDirectoryHandle dir, SetupStage stage, Sprite thumbnail)
        {
            CustomCharacters.Add(template);
            LoadedCharactersDict.Add(template.GetCharacter(NewCharacterID, dir, stage, thumbnail), template);

            foreach(EquipmentPool pool in template.EquipmentPools)
            {
                EquipmentPoolDictionary.Add(pool.GetPoolEntry(), pool);
            }

            NewCharacterID += 1;

            TNHTweakerLogger.Log("TNHTweaker -- Character added successfuly : " + template.DisplayName, TNHTweakerLogger.LogType.Character);
        }

        public static void AddCharacterTemplate(TNH_CharacterDef realTemplate)
        {
            CustomCharacter template = new CustomCharacter(realTemplate);

            DefaultCharacters.Add(template);
            LoadedCharactersDict.Add(realTemplate, template);

            foreach (EquipmentPool pool in template.EquipmentPools)
            {
                //Must check for this, since default characters can have references to the same pools
                if (!EquipmentPoolDictionary.ContainsKey(pool.GetPoolEntry()))
                {
                    EquipmentPoolDictionary.Add(pool.GetPoolEntry(), pool);
                }
            }

            TNHTweakerLogger.Log("TNHTweaker -- Character added successfuly : " + realTemplate.DisplayName, TNHTweakerLogger.LogType.Character);
        }

        public static void AddVaultFile(SavedGunSerializable template)
        {
            if (!LoadedVaultFiles.ContainsKey(template.FileName))
            {
                LoadedVaultFiles.Add(template.FileName, template);
            }
        }


        public static void AddMagazineData(CompatibleMagazineCache magazineCache)
        {
            //Load all of this data into the template manager
            LoadedMagazineTypeDict = magazineCache.MagazineData;
            foreach (List<AmmoObjectDataTemplate> magList in LoadedMagazineTypeDict.Values)
            {
                foreach (AmmoObjectDataTemplate template in magList)
                {
                    if (!LoadedMagazineDict.ContainsKey(template.ObjectID))
                    {
                        LoadedMagazineDict.Add(template.ObjectID, template);
                    }

                    else
                    {
                        TNHTweakerLogger.LogWarning("TNHTweaker -- Attempted to add duplicate magazine : " + template.ObjectID);
                    }
                }
            }

            //Load all of this data into the template manager
            LoadedClipTypeDict = magazineCache.ClipData;
            foreach (List<AmmoObjectDataTemplate> clipList in LoadedClipTypeDict.Values)
            {
                foreach (AmmoObjectDataTemplate template in clipList)
                {
                    if (!LoadedClipDict.ContainsKey(template.ObjectID))
                    {
                        LoadedClipDict.Add(template.ObjectID, template);
                    }

                    else
                    {
                        TNHTweakerLogger.LogWarning("TNHTweaker -- Attempted to add duplicate clip : " + template.ObjectID);
                    }
                }
            }

            //Load all of this data into the template manager
            LoadedBulletTypeDict = magazineCache.BulletData;
            foreach (List<AmmoObjectDataTemplate> bulletList in LoadedBulletTypeDict.Values)
            {
                foreach (AmmoObjectDataTemplate template in bulletList)
                {
                    if (!LoadedBulletDict.ContainsKey(template.ObjectID))
                    {
                        LoadedBulletDict.Add(template.ObjectID, template);
                    }

                    else
                    {
                        TNHTweakerLogger.LogWarning("TNHTweaker -- Attempted to add duplicate bullet : " + template.ObjectID);
                    }
                }
            }
        }

        public static void AddMagazineDataFromLoad(CompatibleMagazineCache magazineCache)
        {
            //Loop through all magazine objects by type
            foreach (KeyValuePair<FireArmMagazineType, List<AmmoObjectDataTemplate>> entry in magazineCache.MagazineData)
            {
                //Loop through the magazines of the selected type
                for (int i = 0; i < entry.Value.Count; i++)
                {
                    //If the magazine is not loaded, remove it
                    if (!IM.OD.ContainsKey(entry.Value[i].ObjectID))
                    {
                        TNHTweakerLogger.LogWarning("TNHTweaker -- Magazine in cache was not loaded : " + entry.Value[i].ObjectID);
                        entry.Value.RemoveAt(i);
                        i -= 1;
                    }

                    else if (!LoadedMagazineDict.ContainsKey(entry.Value[i].ObjectID))
                    {
                        entry.Value[i].AmmoObject = IM.OD[entry.Value[i].ObjectID];
                        LoadedMagazineDict.Add(entry.Value[i].ObjectID, entry.Value[i]);
                    }
                }
            }
            LoadedMagazineTypeDict = magazineCache.MagazineData;


            foreach (KeyValuePair<FireArmClipType, List<AmmoObjectDataTemplate>> entry in magazineCache.ClipData)
            {
                for (int i = 0; i < entry.Value.Count; i++)
                {
                    if (!IM.OD.ContainsKey(entry.Value[i].ObjectID))
                    {
                        TNHTweakerLogger.LogWarning("TNHTweaker -- Clip in cache was not loaded : " + entry.Value[i].ObjectID);
                        entry.Value.RemoveAt(i);
                        i -= 1;
                    }

                    else if (!LoadedClipDict.ContainsKey(entry.Value[i].ObjectID))
                    {
                        entry.Value[i].AmmoObject = IM.OD[entry.Value[i].ObjectID];
                        LoadedClipDict.Add(entry.Value[i].ObjectID, entry.Value[i]);
                    }
                }
            }
            LoadedClipTypeDict = magazineCache.ClipData;


            foreach (KeyValuePair<FireArmRoundType, List<AmmoObjectDataTemplate>> entry in magazineCache.BulletData)
            {
                for (int i = 0; i < entry.Value.Count; i++)
                {
                    if (!IM.OD.ContainsKey(entry.Value[i].ObjectID))
                    {
                        TNHTweakerLogger.LogWarning("TNHTweaker -- Bullet in cache was not loaded : " + entry.Value[i].ObjectID);
                        entry.Value.RemoveAt(i);
                        i -= 1;
                    }

                    else if (!LoadedBulletDict.ContainsKey(entry.Value[i].ObjectID))
                    {
                        entry.Value[i].AmmoObject = IM.OD[entry.Value[i].ObjectID];
                        LoadedBulletDict.Add(entry.Value[i].ObjectID, entry.Value[i]);
                    }
                }
            }
            LoadedBulletTypeDict = magazineCache.BulletData;

        }


    }


    

}
