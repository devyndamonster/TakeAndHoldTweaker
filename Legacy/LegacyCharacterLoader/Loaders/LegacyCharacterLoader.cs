using Deli;
using Deli.Setup;
using Deli.VFS;
using LegacyCharacterLoader.LegacyConverters;
using LegacyCharacterLoader.Objects.CharacterData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TNHTweaker;
using TNHTweaker.Objects.CharacterData;
using TNHTweaker.Utilities;
using UnityEngine;
using Valve.Newtonsoft.Json;

namespace LegacyCharacterLoader.Loaders
{
    public class LegacyCharacterLoader
    {
        public void LoadAsset(SetupStage stage, Mod mod, IHandle handle)
        {
            if (handle is not IDirectoryHandle dir)
            {
                throw new ArgumentException("Could not load character! Character should point to a folder holding the character.json and thumb.png");
            }

            try
            {
                LegacyCharacter legacyCharacter = LoadLegacyCharacter(stage, dir);
                Character convertedCharacter = LegacyCharacterConverter.ConvertCharacterFromLegacy(legacyCharacter, dir);
                CharacterLoader.LoadCharacter(convertedCharacter);
            }
            catch(Exception ex)
            {
                TNHTweakerLogger.LogError("Failed to load legacy character! Error:\n" + ex.ToString());
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

    }
}
