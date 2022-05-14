﻿using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TNHTweaker.ObjectConverters;
using TNHTweaker.Objects.CharacterData;
using TNHTweaker.Objects.SosigData;
using TNHTweaker.Utilities;
using UnityEngine;

namespace TNHTweaker
{
    public class CharacterLoader
    {
        public delegate void CharacterLoaded(Character character);
        public delegate void SosigLoaded(SosigTemplate sosig);

        public static event CharacterLoaded OnCharacterLoaded;
        public static event SosigLoaded OnSosigLoaded;

        private static List<string> customCharacterBundlePaths = new List<string>();


        public static void RegisterCharacterBundleToLoad(string modPath, string bundleName)
        {
            if (customCharacterBundlePaths.Count() == 0)
            {
                TNHTweaker.Instance.StartCoroutine(WaitToLoadCharacters());
            }

            string bundlePath = Path.Combine(modPath, bundleName);
            customCharacterBundlePaths.Add(bundlePath);
        }

        private static IEnumerator WaitToLoadCharacters()
        {
            while(MagazinePatcher.PatcherStatus.PatcherProgress < 1) yield return null;

            foreach(string characterBundlePath in customCharacterBundlePaths)
            {
                try
                {
                    LoadCharacterBundle(characterBundlePath);
                }
                catch(Exception e)
                {
                    TNHTweakerLogger.LogError(e.ToString());
                }
            }
        }

        private static void LoadCharacterBundle(string bundlePath)
        {
            TNHTweakerLogger.Log("Loading character from bundle at path: " + bundlePath, TNHTweakerLogger.LogType.Loading);
            AssetBundle characterBundle = AssetBundle.LoadFromFile(bundlePath);

            LoadSosigsFromBundle(characterBundle);
            LoadCharactersFromBundle(characterBundle);
        }

        private static void LoadSosigsFromBundle(AssetBundle bundle)
        {
            SosigTemplate[] sosigs = bundle.LoadAllAssets<SosigTemplate>();

            foreach (SosigTemplate sosig in sosigs)
            {
                LoadSosig(sosig);
            }
        }

        private static void LoadCharactersFromBundle(AssetBundle bundle)
        {
            Character[] characters = bundle.LoadAllAssets<Character>();

            foreach (Character character in characters)
            {
                LoadCharacter(character);
            }
        }

        public static void LoadCharacter(Character character)
        {
            TNH_CharacterDef baseCharacter = CharacterConverter.ConvertCharacterToVanilla(character);
            TNHTweaker.CustomCharacterDict[baseCharacter] = character;
            TNHTweaker.BaseCharacterDict[character] = baseCharacter;
            OnCharacterLoaded?.Invoke(character);
        }

        public static void LoadSosig(SosigTemplate sosig)
        {
            SosigEnemyTemplate baseSosig = SosigTemplateConverter.ConvertSosigTemplateToVanilla(sosig);
            TNHTweaker.CustomSosigDict[baseSosig] = sosig;
            TNHTweaker.BaseSosigDict[sosig] = baseSosig;
            LoadSosigIntoVanillaDictionaries(baseSosig);
            OnSosigLoaded?.Invoke(sosig);
        }

        private static void LoadSosigIntoVanillaDictionaries(SosigEnemyTemplate sosig)
        {
            if (!IM.Instance.olistSosigCats.Contains(sosig.SosigEnemyCategory))
            {
                IM.Instance.olistSosigCats.Add(sosig.SosigEnemyCategory);
            }

            if (!IM.Instance.odicSosigIDsByCategory.ContainsKey(sosig.SosigEnemyCategory))
            {
                List<SosigEnemyID> sosigIds = new List<SosigEnemyID>();
                sosigIds.Add(sosig.SosigEnemyID);
                IM.Instance.odicSosigIDsByCategory[sosig.SosigEnemyCategory] = sosigIds;

                List<SosigEnemyTemplate> sosigTemplates = new List<SosigEnemyTemplate>();
                sosigTemplates.Add(sosig);
                IM.Instance.odicSosigObjsByCategory[sosig.SosigEnemyCategory] = sosigTemplates;
            }

            if (!IM.Instance.odicSosigObjsByID.ContainsKey(sosig.SosigEnemyID))
            {
                IM.Instance.odicSosigObjsByID[sosig.SosigEnemyID] = sosig;
            }
        }

    }
}
