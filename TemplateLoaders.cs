using ADepIn;
using Deli;
using FistVR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

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
            if (!Directory.Exists(path))
            {
                Debug.LogError("TNHTweaker -- Character mod path was not a folder! Please ensure that your character mod is a folder containing a 'character.json' and 'thumb.png' file");
                return;
            }

            string templatePath = path + "/character.json";
            Option<Option<CustomCharacter>> content = mod.Resources.Get<Option<CustomCharacter>>(templatePath);

            Option<CustomCharacter> flattened = content.Flatten();

            CustomCharacter template = flattened.Expect("Failed to read custom character template!");

            TNHTweakerLogger.Log("TNHTweaker -- Character loaded successfuly : " + template.DisplayName, TNHTweakerLogger.LogType.File);

            LoadedTemplateManager.AddCharacterTemplate(template, path);
        }
    }
}
