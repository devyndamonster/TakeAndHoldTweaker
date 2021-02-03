using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public static Dictionary<FireArmMagazineType, List<MagazineDataTemplate>> LoadedMagazineTypeDict = new Dictionary<FireArmMagazineType, List<MagazineDataTemplate>>();
        public static Dictionary<string, MagazineDataTemplate> LoadedMagazineDict = new Dictionary<string, MagazineDataTemplate>();

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
                Debug.LogError("TNHTweaker -- Loaded sosig had same SosigEnemyID as another sosig -- SosigEnemyID : " + template.SosigEnemyID);
                return;
            }

            //Now fill out the SosigEnemyIDs values for the real sosig template (These will effectively be ints, but this is ok since enums are just ints in disguise)
            realTemplate.SosigEnemyID = (SosigEnemyID)SosigIDDict[template.SosigEnemyID];
            realTemplate.EnemyType = (TNH_EnemyType)SosigIDDict[template.SosigEnemyID];

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
                Debug.LogError("TNHTweaker -- Loaded sosig had same SosigEnemyID as another sosig -- SosigEnemyID : " + template.SosigEnemyID);
                return;
            }

            //Since the real template already had a valid SosigEnemyID, we can skip the part where we reassign them
            DefaultSosigs.Add(realTemplate);
            LoadedSosigsDict.Add(realTemplate, template);

            TNHTweakerLogger.Log("TNHTweaker -- Sosig added successfuly : " + template.DisplayName, TNHTweakerLogger.LogType.Character);
        }


        public static void AddCharacterTemplate(CustomCharacter template, Deli.Mod mod, string path, Sprite thumbnail)
        {
            CustomCharacters.Add(template);
            LoadedCharactersDict.Add(template.GetCharacter(NewCharacterID, mod, path, thumbnail), template);

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


    }


    

}
