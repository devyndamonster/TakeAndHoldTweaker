using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.Objects.CharacterData;
using UnityEngine;

namespace TNHTweaker.ObjectWrappers
{
    public class TNHManagerStateWrapper : MonoBehaviour
    {
        public static TNHManagerStateWrapper Instance;
        private TNH_Manager baseManager;

        private void Awake()
        {
            Instance = this;
            baseManager = GetComponent<TNH_Manager>();
        }

        public TNH_EncryptionType GetPrimaryEncryptionType()
        {
            if(baseManager.TargetMode == TNHSetting_TargetMode.Simple)
            {
                return TNH_EncryptionType.Static;
            }

            return GetCurrentHoldPhase().Encryptions.FirstOrDefault();
        }

        public HoldPhase GetCurrentHoldPhase()
        {
            Level currentLevel = GetCurrentLevel();
            int currentPhaseIndex = Mathf.Clamp(baseManager.m_curHoldPoint.m_phaseIndex, 0, currentLevel.HoldPhases.Count - 1);

            return currentLevel.HoldPhases[currentPhaseIndex];
        }

        public Level GetCurrentLevel()
        {
            return GetCurrentProgression().Levels[baseManager.m_level];
        }

        public Progression GetCurrentProgression()
        {
            if(baseManager.ProgressionMode == TNHSetting_ProgressionType.Marathon)
            {
                return GetCurrentCharacter().Progressions_Endless[GetSelectedEndlessProgressionIndex()];
            }

            return GetCurrentCharacter().Progressions[GetSelectedNormalProgressionIndex()];
        }

        public Character GetCurrentCharacter()
        {
            return TNHTweaker.CustomCharacterDict[baseManager.C];
        }

        private int GetSelectedEndlessProgressionIndex()
        {
            return baseManager.C.Progressions_Endless.IndexOf(baseManager.m_curProgressionEndless);
        }

        private int GetSelectedNormalProgressionIndex()
        {
            return baseManager.C.Progressions.IndexOf(baseManager.m_curProgression);
        }

        public bool IsTagActive(string tag)
        {
            string tagType = GetTagType(tag);
            string tagValue = GetTagValue(tag);

            switch (tagType)
            {
                case "QuestComplete":
                    return IsQuestComplete(tagValue);
                case "EquipmentMode":
                    return IsEquipmentMode(tagValue);
                case "Map":
                    return IsCurrentMap(tagValue);
                case "ItemExists":
                    return DoesItemExist(tagValue);
                case "ItemUnlocked":
                    return IsItemUnlocked(tagValue);
                default:
                    return false;
            }
        }

        public bool IsQuestComplete(string quest)
        {
            return true;
        }

        public bool IsEquipmentMode(string ammoMode)
        {
            return ammoMode == baseManager.EquipmentMode.ToString();
        }

        public bool IsCurrentMap(string mapName)
        {
            return mapName == baseManager.LevelName;
        }

        public bool DoesItemExist(string itemID)
        {
            return IM.OD.ContainsKey(itemID);
        }

        public bool IsItemUnlocked(string itemID)
        {
            return DoesItemExist(itemID) && OtherLoader.OtherLoader.UnlockSaveData.IsItemUnlocked(itemID);
        }

        private string GetTagType(string tag)
        {
            return tag.Substring(0, tag.IndexOf(':'));
        }

        private string GetTagValue(string tag)
        {
            return tag.Substring(tag.IndexOf(':') + 1);
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}
