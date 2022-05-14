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
                TNH_Char characterID = (TNH_Char)(111000 + LegacyCharacterLoader.CharacterStringToID.Keys.Count());
                LegacyCharacterLoader.CharacterStringToID[characterName] = characterID;
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
                SosigEnemyID sosigID = (SosigEnemyID)(111000 + LegacyCharacterLoader.CharacterStringToID.Keys.Count());
                LegacyCharacterLoader.SosigStringToID[sosigTypeString] = sosigID;
                return sosigID;
            }
        }
    }
}
