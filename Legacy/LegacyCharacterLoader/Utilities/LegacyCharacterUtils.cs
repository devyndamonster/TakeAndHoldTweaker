using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegacyCharacterLoader.Utilities
{
    public static class LegacyCharacterUtils
    {
        public static TNH_Char GetUniqueTNHCharValue(string characterName)
        {
            if (LegacyCharacterLoader.CharacterStringToID.ContainsKey(characterName))
            {
                return LegacyCharacterLoader.CharacterStringToID[characterName];
            }

            else
            {
                int characterIDValue = 111000 + LegacyCharacterLoader.CharacterStringToID.Keys.Count();
                TNH_Char characterID = (TNH_Char)characterIDValue;
                LegacyCharacterLoader.CharacterStringToID[characterName] = characterID;
                LegacyLogger.Log($"Assigning character ({characterName}) value ({characterIDValue})", LegacyLogger.LogType.Loading);
                return characterID;
            }
        }

        public static SosigEnemyID GetUniqueSosigIDValue(string sosigTypeString)
        {
            if (LegacyCharacterLoader.SosigStringToID.ContainsKey(sosigTypeString))
            {
                return LegacyCharacterLoader.SosigStringToID[sosigTypeString];
            }

            else
            {
                int sosigIDValue = 111000 + LegacyCharacterLoader.SosigStringToID.Keys.Count();
                SosigEnemyID sosigID = (SosigEnemyID)sosigIDValue;
                LegacyCharacterLoader.SosigStringToID[sosigTypeString] = sosigID;
                LegacyLogger.Log($"Assigning sosig ({sosigTypeString}) value ({sosigIDValue})", LegacyLogger.LogType.Loading);
                return sosigID;
            }
        }
    }
}
