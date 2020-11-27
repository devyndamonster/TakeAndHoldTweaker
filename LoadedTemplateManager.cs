using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FistVR
{
    public static class LoadedTemplateManager
    {

        public static Dictionary<TNH_CharacterDef, CustomCharacter> LoadedCharacters = new Dictionary<TNH_CharacterDef, CustomCharacter>();
        public static Dictionary<SosigEnemyTemplate, SosigTemplate> LoadedSosigs = new Dictionary<SosigEnemyTemplate, SosigTemplate>();
        public static Dictionary<string, int> SosigIDDict = new Dictionary<string, int>();
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

            //Add the new sosig template to the global dictionaries
            ManagerSingleton<IM>.Instance.odicSosigObjsByID.Add(realTemplate.SosigEnemyID, realTemplate);
            ManagerSingleton<IM>.Instance.odicSosigIDsByCategory[realTemplate.SosigEnemyCategory].Add(realTemplate.SosigEnemyID);
            ManagerSingleton<IM>.Instance.odicSosigObjsByCategory[realTemplate.SosigEnemyCategory].Add(realTemplate);

            //Finally add the templates to our global dictionary
            LoadedSosigs.Add(realTemplate, template);

            TNHTweakerLogger.Log("TNHTweaker -- Sosig added successfuly : " + template.DisplayName, TNHTweakerLogger.LogType.File);
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
            LoadedSosigs.Add(realTemplate, template);

            TNHTweakerLogger.Log("TNHTweaker -- Sosig added successfuly : " + template.DisplayName, TNHTweakerLogger.LogType.File);
        }


        public static void AddCharacterTemplate(CustomCharacter template, string path)
        {
            LoadedCharacters.Add(template.GetCharacter(NewCharacterID, path, DefaultIconSprites), template);
            NewCharacterID += 1;

            TNHTweakerLogger.Log("TNHTweaker -- Character added successfuly : " + template.DisplayName, TNHTweakerLogger.LogType.File);
        }

        public static void AddCharacterTemplate(TNH_CharacterDef realTemplate)
        {
            LoadedCharacters.Add(realTemplate, new CustomCharacter(realTemplate));

            TNHTweakerLogger.Log("TNHTweaker -- Character added successfuly : " + realTemplate.DisplayName, TNHTweakerLogger.LogType.File);
        }

    }
}
