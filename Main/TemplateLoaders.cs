using ADepIn;
using Deli;
using Deli.Immediate;
using Deli.Setup;
using Deli.VFS;
using FistVR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TNHTweaker.ObjectTemplates;
using TNHTweaker.Utilities;
using UnityEngine;
using Valve.Newtonsoft.Json;
using Valve.Newtonsoft.Json.Converters;

namespace TNHTweaker
{


    public class SosigLoader
    {
        public void LoadAsset(SetupStage stage, Mod mod, IHandle handle)
        {

            if(handle is not IFileHandle file)
            {
                throw new ArgumentException("Could not load sosig! Make sure you're pointing to a sosig template json file in the manifest");
            }

            ImmediateReader<SosigTemplate> reader = stage.RegisterJson<SosigTemplate>();
            SosigTemplate sosig = reader(file);

            TNHTweakerLogger.Log("TNHTweaker -- Sosig loaded successfuly : " + sosig.DisplayName, TNHTweakerLogger.LogType.File);

            LoadedTemplateManager.AddSosigTemplate(sosig);
        }
    }



    public class CharacterLoader
    {
        public void LoadAsset(SetupStage stage, Mod mod, IHandle handle)
        {
            
            if(handle is not IDirectoryHandle dir)
            {
                throw new ArgumentException("Could not load character! Character should point to a folder holding the character.json and thumb.png");
            }


            CustomCharacter character = null;
            Sprite thumbnail = null;

            foreach(IFileHandle file in dir.GetFiles())
            {
                if(file.Path.EndsWith("character.json"))
                {
                    ImmediateReader<CustomCharacter> reader = stage.RegisterJson<CustomCharacter>();
                    character = reader(file);
                }
                else if (file.Path.EndsWith("thumb.png"))
                {
                    ImmediateReader<Texture2D> reader = stage.ImmediateReaders.Get<Texture2D>();
                    thumbnail = TNHTweakerUtils.LoadSprite(reader(file));
                }
            }

            if(character == null)
            {
                TNHTweakerLogger.LogError("TNHTweaker -- Failed to load custom character! No character.json file found");
                return;
            }

            else if(thumbnail == null)
            {
                TNHTweakerLogger.LogError("TNHTweaker -- Failed to load custom character! No thumb.png file found");
                return;
            }

            TNHTweakerLogger.Log("TNHTweaker -- Character loaded successfuly : " + character.DisplayName, TNHTweakerLogger.LogType.File);

            LoadedTemplateManager.AddCharacterTemplate(character, dir, stage, thumbnail);
        }
    }



    public class VaultFileLoader
    {
        public void LoadAsset(SetupStage stage, Mod mod, IHandle handle)
        {

            if (handle is not IFileHandle file)
            {
                throw new ArgumentException("Could not load vault file! Make sure you're pointing to a vault json file in the manifest");
            }

            ImmediateReader<SavedGunSerializable> reader = stage.RegisterJson<SavedGunSerializable>();
            SavedGunSerializable savedGun = reader(file);

            TNHTweakerLogger.Log("TNHTweaker -- Vault file loaded successfuly : " + savedGun.FileName, TNHTweakerLogger.LogType.File);

            LoadedTemplateManager.AddVaultFile(savedGun);
        }
    }


}
