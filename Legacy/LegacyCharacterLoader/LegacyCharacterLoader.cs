using Deli.Setup;
using LegacyCharacterLoader.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegacyCharacterLoader
{
    public class LegacyCharacterLoader : DeliBehaviour
    {
        private void Awake()
        {
            Stages.Setup += OnSetup;
        }

        private void OnSetup(SetupStage stage)
        {
            stage.SetupAssetLoaders[Source, "sosig"] = new SosigLoader().LoadAsset;
            stage.SetupAssetLoaders[Source, "vault_file"] = new VaultFileLoader().LoadAsset;
            stage.SetupAssetLoaders[Source, "character"] = new CharacterLoader().LoadAsset;
        }
    }
}
