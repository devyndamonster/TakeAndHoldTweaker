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
            return GetCurrentLevel().HoldPhases[baseManager.m_curHoldPoint.m_phaseIndex];
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

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}
