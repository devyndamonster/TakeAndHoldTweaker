using Deli;
using Deli.Setup;
using Deli.VFS;
using LegacyCharacterLoader.LegacyConverters;
using LegacyCharacterLoader.Objects.SosigData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker;
using TNHTweaker.Objects.SosigData;
using TNHTweaker.Utilities;
using Valve.Newtonsoft.Json;

namespace LegacyCharacterLoader.Loaders
{
    public class LegacySosigLoader
    {
        public void LoadAsset(SetupStage stage, Mod mod, IHandle handle)
        {
            if (handle is not IFileHandle file)
            {
                throw new ArgumentException("Could not load sosig! Make sure you're pointing to a sosig template json file in the manifest");
            }
            try
            {
                LegacySosigTemplate legacySosig = LoadLegacySosig(stage, file);
                SosigTemplate convertedSosig = LegacySosigTemplateConverter.ConvertSosigTemplateFromLegacy(legacySosig);
                CharacterLoader.LoadSosig(convertedSosig);
            }
            catch (Exception ex)
            {
                TNHTweakerLogger.LogError("Failed to load legacy sosig! Error:\n" + ex.ToString());
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
    }
}
