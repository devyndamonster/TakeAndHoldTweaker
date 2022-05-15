using Deli;
using Deli.Setup;
using Deli.VFS;
using LegacyCharacterLoader.LegacyConverters;
using LegacyCharacterLoader.Objects.SosigData;
using LegacyCharacterLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker;
using TNHTweaker.Objects.SosigData;
using Valve.Newtonsoft.Json;

namespace LegacyCharacterLoader.Loaders
{
    public class SosigLoader
    {
        private static List<LegacySosigTemplate> LegacySosigTemplates = new List<LegacySosigTemplate>();

        public void LoadAsset(SetupStage stage, Mod mod, IHandle handle)
        {
            if (handle is not IFileHandle file)
            {
                throw new ArgumentException("Could not load sosig! Make sure you're pointing to a sosig template json file in the manifest");
            }
            try
            {
                LegacyLogger.Log("Loading legacy sosig file: " + file.Path, LegacyLogger.LogType.Loading);
                LegacySosigTemplate legacySosig = LoadLegacySosig(stage, file);
                LegacySosigTemplates.Add(legacySosig);
                LegacyLogger.Log(legacySosig.ToString(), LegacyLogger.LogType.Loading);
            }
            catch (Exception ex)
            {
                LegacyLogger.LogError("Failed to load legacy sosig! Error:\n" + ex.ToString());
            }
        }

        private LegacySosigTemplate LoadLegacySosig(SetupStage stage, IFileHandle file)
        {
            string sosigJson = stage.ImmediateReaders.Get<string>()(file);

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            return JsonConvert.DeserializeObject<LegacySosigTemplate>(sosigJson, settings);
        }

        public static void DelayedConvertAllSosigs()
        {
            foreach (LegacySosigTemplate legacySosig in LegacySosigTemplates)
            {
                LegacyLogger.Log("Converting legacy sosig to new format: " + legacySosig.DisplayName, LegacyLogger.LogType.Loading);
                SosigTemplate convertedSosig = LegacySosigTemplateConverter.ConvertSosigTemplateFromLegacy(legacySosig);
                TNHTweaker.CharacterLoader.LoadSosig(convertedSosig);
            }
        }


    }
}
