using ADepIn;
using Deli;
using FistVR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Valve.Newtonsoft.Json;
using Valve.Newtonsoft.Json.Converters;

namespace FistVR
{

    [QuickNamedBind("Sosig")]
    public class SosigLoader : IAssetLoader
    {
        public void LoadAsset(IServiceKernel kernel, Mod mod, string path)
        {
            Option<Option<SosigTemplate>> content = mod.Resources.Get<Option<SosigTemplate>>(path);

            Option<SosigTemplate> flattened = content.Flatten();

            SosigTemplate template = flattened.Expect("Failed to read sosig template!");

            TNHTweakerLogger.Log("TNHTweaker -- Sosig loaded successfuly : " + template.DisplayName, TNHTweakerLogger.LogType.File);

            LoadedTemplateManager.AddSosigTemplate(template);
        }
    }

    [QuickNamedBind("Character")]
    public class CharacterLoader : IAssetLoader
    {
        public void LoadAsset(IServiceKernel kernel, Mod mod, string path)
        {
            
            string templatePath = path + "/character.json";
            Option<Option<CustomCharacter>> characterContent = mod.Resources.Get<Option<CustomCharacter>>(templatePath);
            CustomCharacter template = characterContent.Flatten().Expect("TNHTweaker -- Failed to read custom character template! Character will not be loaded");

            string imagePath = path + "/thumb.png";
            Option<Texture2D> imageContent = mod.Resources.Get<Texture2D>(imagePath);
            Sprite thumbnail = TNHTweakerUtils.LoadSprite(imageContent.Expect("TNHTweaker -- Failed to get character thumbnail! Character will not be loaded"));

            TNHTweakerLogger.Log("TNHTweaker -- Character loaded successfuly : " + template.DisplayName, TNHTweakerLogger.LogType.File);

            LoadedTemplateManager.AddCharacterTemplate(template, mod, path, thumbnail);
        }
    }

    internal class JsonAssetReaderEntry : IEntryModule<JsonAssetReaderEntry>
    {
        public void Load(IServiceKernel kernel)
        {
            kernel.BindJson<CustomCharacter>();
            kernel.BindJson<SosigTemplate>();
        }
    }

}
