using ADepIn;
using Deli;
using Deli.Immediate;
using Deli.Newtonsoft.Json;
using Deli.Newtonsoft.Json.Linq;
using Deli.Runtime;
using Deli.Runtime.Yielding;
using Deli.Setup;
using Deli.VFS;
using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TNHTweaker.ObjectTemplates;
using TNHTweaker.Utilities;
using UnityEngine;

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

            try
            {
                SosigTemplate sosig = stage.ImmediateReaders.Get<JToken>()(file).ToObject<SosigTemplate>();
                TNHTweakerLogger.Log("TNHTweaker -- Sosig loaded successfuly : " + sosig.DisplayName, TNHTweakerLogger.LogType.File);

                LoadedTemplateManager.AddSosigTemplate(sosig);
            }
            catch (Exception e)
            {
                TNHTweakerLogger.LogError("Failed to load setup assets for sosig file! Caused Error: " + e.ToString());
            }

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


            try
            {
                CustomCharacter character = null;
                Sprite thumbnail = null;

                foreach (IFileHandle file in dir.GetFiles())
                {
                    if (file.Path.EndsWith("character.json"))
                    {
                        string charString = stage.ImmediateReaders.Get<string>()(file);
                        JsonSerializerSettings settings = new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        };
                        character = JsonConvert.DeserializeObject<CustomCharacter>(charString, settings);
                    }
                    else if (file.Path.EndsWith("thumb.png"))
                    {
                        thumbnail = TNHTweakerUtils.LoadSprite(file);
                    }
                }

                if (character == null)
                {
                    TNHTweakerLogger.LogError("TNHTweaker -- Failed to load custom character! No character.json file found");
                    return;
                }

                else if (thumbnail == null)
                {
                    TNHTweakerLogger.LogError("TNHTweaker -- Failed to load custom character! No thumb.png file found");
                    return;
                }

                //Now we want to load the icons for each pool
                foreach (IFileHandle iconFile in dir.GetFiles())
                {
                    foreach (EquipmentPool pool in character.EquipmentPools)
                    {
                        if (iconFile.Path.EndsWith(pool.IconName))
                        {
                            pool.GetPoolEntry().TableDef.Icon = TNHTweakerUtils.LoadSprite(iconFile);
                        }
                    }
                }

                TNHTweakerLogger.Log("TNHTweaker -- Character loaded successfuly : " + character.DisplayName, TNHTweakerLogger.LogType.File);

                LoadedTemplateManager.AddCharacterTemplate(character, thumbnail);
            }
            catch(Exception e)
            {
                TNHTweakerLogger.LogError("Failed to load setup assets for character! Caused Error: " + e.ToString());
            }
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

            try
            {
                SavedGunSerializable savedGun = stage.ImmediateReaders.Get<JToken>()(file).ToObject<SavedGunSerializable>();

                TNHTweakerLogger.Log("TNHTweaker -- Vault file loaded successfuly : " + savedGun.FileName, TNHTweakerLogger.LogType.File);
                TNHTweakerLogger.Log("TNHTweaker -- Vault file loaded successfuly : " + savedGun.FileName, TNHTweakerLogger.LogType.File);

                LoadedTemplateManager.AddVaultFile(savedGun);
            }
            catch(Exception e)
            {
                TNHTweakerLogger.LogError("Failed to load setup assets for vault file! Caused Error: " + e.ToString());
            }
        }
    }


}
