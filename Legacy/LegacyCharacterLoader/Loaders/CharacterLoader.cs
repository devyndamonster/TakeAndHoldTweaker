using Deli;
using Deli.Setup;
using Deli.VFS;
using LegacyCharacterLoader.LegacyConverters;
using LegacyCharacterLoader.Objects.CharacterData;
using LegacyCharacterLoader.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TNHTweaker;
using TNHTweaker.Objects.CharacterData;
using UnityEngine;
using Valve.Newtonsoft.Json;

namespace LegacyCharacterLoader.Loaders
{
    public class CharacterLoader
    {
        private static Dictionary<LegacyCharacter, IDirectoryHandle> CharacterDirectoryDict = new Dictionary<LegacyCharacter, IDirectoryHandle>();

        public void LoadAsset(SetupStage stage, Mod mod, IHandle handle)
        {
            if (handle is not IDirectoryHandle dir)
            {
                throw new ArgumentException("Could not load character! Character should point to a folder holding the character.json and thumb.png");
            }

            try
            {
                LegacyLogger.Log("Loading legacy character: " + dir.Path, LegacyLogger.LogType.Loading);
                LegacyCharacter legacyCharacter = LoadLegacyCharacter(stage, dir);
                CharacterDirectoryDict[legacyCharacter] = dir;
                LegacyLogger.Log(legacyCharacter.ToString(), LegacyLogger.LogType.Loading);
            }
            catch(Exception ex)
            {
                LegacyLogger.LogError("Failed to load legacy character! Error:\n" + ex.ToString());
            }

        }

        private LegacyCharacter LoadLegacyCharacter(SetupStage stage, IDirectoryHandle dir)
        {
            string characterJson = stage.ImmediateReaders.Get<string>()(dir.GetFile("character.json"));

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            return JsonConvert.DeserializeObject<LegacyCharacter>(characterJson, settings);
        }

        public static void DelayedLoadAllCharacters()
        {
            foreach (KeyValuePair<LegacyCharacter, IDirectoryHandle> pair in CharacterDirectoryDict)
            {
                LegacyLogger.Log("Converting legacy character to new format: " + pair.Key.DisplayName, LegacyLogger.LogType.Loading);
                Character convertedCharacter = LegacyCharacterConverter.ConvertCharacterFromLegacy(pair.Key, pair.Value);
                TNHTweaker.CharacterLoader.LoadCharacter(convertedCharacter);
            }
        }

        

    }
}
