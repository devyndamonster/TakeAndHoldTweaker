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

            SosigTemplate sosig = stage.ImmediateReaders.Get<JToken>()(file).ToObject<SosigTemplate>();
            TNHTweakerLogger.Log("TNHTweaker -- Sosig loaded successfuly : " + sosig.DisplayName, TNHTweakerLogger.LogType.File);

            LoadedTemplateManager.AddSosigTemplate(sosig);
        }
    }



    public class CharacterLoader
    {
        public IEnumerator LoadAsset(RuntimeStage stage, Mod mod, IHandle handle)
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
                    character = stage.ImmediateReaders.Get<JToken>()(file).ToObject<CustomCharacter>();
                }
                else if (file.Path.EndsWith("thumb.png"))
                {
                    ResultYieldInstruction<Texture2D> resultDelayed = stage.GetReader<Texture2D>()(file);
                    yield return resultDelayed;
                    thumbnail = TNHTweakerUtils.LoadSprite(resultDelayed.Result);
                }
            }

            if(character == null)
            {
                TNHTweakerLogger.LogError("TNHTweaker -- Failed to load custom character! No character.json file found");
                yield break;
            }

            else if(thumbnail == null)
            {
                TNHTweakerLogger.LogError("TNHTweaker -- Failed to load custom character! No thumb.png file found");
                yield break;
            }

            //Now we want to load the icons for each pool
            foreach(EquipmentPool pool in character.EquipmentPools)
            {
                if (LoadedTemplateManager.DefaultIconSprites.ContainsKey(pool.IconName))
                {
                    pool.GetPoolEntry().TableDef.Icon = LoadedTemplateManager.DefaultIconSprites[pool.IconName];
                }

                else
                {
                    foreach (IFileHandle iconFile in dir.GetFiles())
                    {
                        if (iconFile.Path.EndsWith(pool.IconName))
                        {
                            ResultYieldInstruction<Texture2D> resultDelayed = stage.GetReader<Texture2D>()(iconFile);
                            yield return resultDelayed;
                            pool.GetPoolEntry().TableDef.Icon = TNHTweakerUtils.LoadSprite(resultDelayed.Result);
                        }
                    }
                } 
            }

            TNHTweakerLogger.Log("TNHTweaker -- Character loaded successfuly : " + character.DisplayName, TNHTweakerLogger.LogType.File);

            LoadedTemplateManager.AddCharacterTemplate(character, thumbnail);
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

            SavedGunSerializable savedGun = stage.ImmediateReaders.Get<JToken>()(file).ToObject<SavedGunSerializable>();

            TNHTweakerLogger.Log("TNHTweaker -- Vault file loaded successfuly : " + savedGun.FileName, TNHTweakerLogger.LogType.File);

            LoadedTemplateManager.AddVaultFile(savedGun);
        }
    }


}
