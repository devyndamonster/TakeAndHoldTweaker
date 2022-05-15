using Deli.Setup;
using FistVR;
using LegacyCharacterLoader.Loaders;
using LegacyCharacterLoader.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegacyCharacterLoader
{
    public class LegacyCharacterLoader : DeliBehaviour
    {
        public static Dictionary<string, SosigEnemyID> SosigStringToID = new Dictionary<string, SosigEnemyID>();
        public static Dictionary<string, TNH_Char> CharacterStringToID = new Dictionary<string, TNH_Char>();

        private void Awake()
        {
            LegacyLogger.Init();
            LegacyLogger.Log("Beginning setup stage of legacy character files", LegacyLogger.LogType.Loading);
            Stages.Setup += OnSetup;
        }

        private void Start()
        {
            StartCoroutine(WaitToLoadCharacters());
        }

        private void OnSetup(SetupStage stage)
        {
            stage.SetupAssetLoaders[Source, "sosig"] = new SosigLoader().LoadAsset;
            stage.SetupAssetLoaders[Source, "vault_file"] = new VaultFileLoader().LoadAsset;
            stage.SetupAssetLoaders[Source, "character"] = new Loaders.CharacterLoader().LoadAsset;
        }

        public static IEnumerator WaitToLoadCharacters()
        {
            LegacyLogger.Log("Waiting to convert legacy character files", LegacyLogger.LogType.Loading);
            while (MagazinePatcher.PatcherStatus.PatcherProgress < 1) yield return null;
            CharacterLoader.DelayedLoadAllCharacters();
            SosigLoader.DelayedConvertAllSosigs();
        }
    }
}
