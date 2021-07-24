using System;
using BepInEx;
using UnityEngine;
using FistVR;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using System.IO;
using System.Collections;
using System.Linq;
using UnityEngine.UI;
using TNHTweaker.ObjectTemplates;
using Deli.Newtonsoft.Json;
using Deli.Newtonsoft.Json.Converters;
using Deli.VFS;

namespace TNHTweaker.Utilities
{
    static class TNHTweakerUtils
    {
        public static void CreateObjectIDFile(string path)
        {
            try
            {
                if (File.Exists(path + "/ObjectIDs.csv"))
                {
                    File.Delete(path + "/ObjectIDs.csv");
                }

                // Create a new file     
                using (StreamWriter sw = File.CreateText(path + "/ObjectIDs.csv"))
                {
                    sw.WriteLine("ObjectID,Category,Era,Set,Country of Origin,Attachment Feature,Firearm Action,Firearm Feed Option,Firing Modes,Firearm Mounts,Attachment Mount,Round Power,Size,Melee Handedness,Melee Style,Powerup Type,Thrown Damage Type,Thrown Type");
                    foreach (FVRObject obj in IM.OD.Values)
                    {
                        sw.WriteLine(
                            obj.ItemID + "," + 
                            obj.Category + "," +
                            obj.TagEra + "," +
                            obj.TagSet + "," +
                            obj.TagFirearmCountryOfOrigin + "," +
                            obj.TagAttachmentFeature + "," +
                            obj.TagFirearmAction + "," +
                            string.Join("+", obj.TagFirearmFeedOption.Select(o => o.ToString()).ToArray()) + "," +
                            string.Join("+", obj.TagFirearmFiringModes.Select(o => o.ToString()).ToArray()) + "," +
                            string.Join("+", obj.TagFirearmMounts.Select(o => o.ToString()).ToArray()) + "," +
                            obj.TagAttachmentMount + "," +
                            obj.TagFirearmRoundPower + "," +
                            obj.TagFirearmSize + "," +
                            obj.TagMeleeHandedness + "," +
                            obj.TagMeleeStyle + "," +
                            obj.TagPowerupType + "," +
                            obj.TagThrownDamageType + "," +
                            obj.TagThrownType);
                    }
                    sw.Close();
                }
            }

            catch (Exception ex)
            {
                TNHTweakerLogger.LogError(ex.ToString());
            }
        }


        public static void CreateSosigIDFile(string path)
        {
            try
            {
                if (File.Exists(path + "/SosigIDs.txt"))
                {
                    File.Delete(path + "/SosigIDs.txt");
                }

                // Create a new file     
                using (StreamWriter sw = File.CreateText(path + "/SosigIDs.txt"))
                {
                    sw.WriteLine("#Available Sosig IDs for spawning");
                    foreach (SosigEnemyID ID in ManagerSingleton<IM>.Instance.odicSosigObjsByID.Keys)
                    {
                        sw.WriteLine(ID.ToString());
                    }
                    sw.Close();
                }
            }

            catch (Exception ex)
            {
                TNHTweakerLogger.LogError(ex.ToString());
            }
        }


        public static void CreateIconIDFile(string path, List<string> icons)
        {
            try
            {
                if (File.Exists(path + "/IconIDs.txt"))
                {
                    File.Delete(path + "/IconIDs.txt");
                }

                // Create a new file     
                using (StreamWriter sw = File.CreateText(path + "/IconIDs.txt"))
                {
                    sw.WriteLine("#Available Icons for equipment pools");
                    foreach (string icon in icons)
                    {
                        sw.WriteLine(icon);
                    }
                    sw.Close();
                }
            }

            catch (Exception ex)
            {
                TNHTweakerLogger.LogError(ex.ToString());
            }
        }

        public static void RunCoroutine(IEnumerator routine, Action<Exception> onError = null)
        {
            AnvilManager.Run(RunAndCatch(routine, onError));
        }

        public static IEnumerator RunAndCatch(IEnumerator routine, Action<Exception> onError = null)
        {
            bool more = true;
            while (more)
            {
                try
                {
                    more = routine.MoveNext();
                }
                catch (Exception e)
                {
                    if (onError != null)
                    {
                        onError(e);
                    }

                    yield break;
                }

                if (more)
                {
                    yield return routine.Current;
                }
            }
        }


        public static Dictionary<string, Sprite> GetAllIcons(List<CustomCharacter> characters)
        {
            Dictionary<string, Sprite> icons = new Dictionary<string, Sprite>();

            foreach(CustomCharacter character in characters)
            {
                foreach(EquipmentPoolDef.PoolEntry pool in character.GetCharacter().EquipmentPool.Entries)
                {
                    if (!icons.ContainsKey(pool.TableDef.Icon.name))
                    {
                        TNHTweakerLogger.Log("TNHTweaker -- Icon found (" + pool.TableDef.Icon.name + ")", TNHTweakerLogger.LogType.Character);
                        icons.Add(pool.TableDef.Icon.name, pool.TableDef.Icon);
                    }
                }
            }

            return icons;
        }


        public static void CreateDefaultCharacterFiles(List<CustomCharacter> characters, string path)
        {

            try
            {
                TNHTweakerLogger.Log("TNHTweaker -- Creating default character template files", TNHTweakerLogger.LogType.File);

                path = path + "/DefaultCharacters";

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                
                foreach (CustomCharacter charDef in characters)
                {
                    if (File.Exists(path + "/" + charDef.DisplayName + ".json"))
                    {
                        File.Delete(path + "/" + charDef.DisplayName + ".json");
                    }

                    // Create a new file     
                    using (StreamWriter sw = File.CreateText(path + "/" + charDef.DisplayName + ".json"))
                    {
                        string characterString = JsonConvert.SerializeObject(charDef, Formatting.Indented, new StringEnumConverter());
                        sw.WriteLine(characterString);
                        sw.Close();
                    }
                }
            }

            catch (Exception ex)
            {
                TNHTweakerLogger.LogError(ex.ToString());
            }
        }


        public static void CreatePopulatedCharacterTemplate(string path)
        {
            try
            {
                TNHTweakerLogger.Log("TNHTweaker -- Creating populated character template file", TNHTweakerLogger.LogType.File);

                path = path + "/PopulatedCharacterTemplate.json";

                if (!File.Exists(path))
                {
                    File.Delete(path);
                }

                using (StreamWriter sw = File.CreateText(path))
                {
                    CustomCharacter character = new CustomCharacter();
                    string characterString = JsonConvert.SerializeObject(character, Formatting.Indented, new StringEnumConverter());
                    sw.WriteLine(characterString);
                    sw.Close();
                }
            }

            catch (Exception ex)
            {
                TNHTweakerLogger.LogError(ex.ToString());
            }
        }


        public static void CreateDefaultSosigTemplateFiles(List<SosigEnemyTemplate> sosigs, string path)
        {
            try
            {
                TNHTweakerLogger.Log("TNHTweaker -- Creating default sosig template files", TNHTweakerLogger.LogType.File);

                path = path + "/DefaultSosigTemplates";

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                foreach (SosigEnemyTemplate template in sosigs)
                {
                    if (File.Exists(path + "/" + template.SosigEnemyID + ".json"))
                    {
                        File.Delete(path + "/" + template.SosigEnemyID + ".json");
                    }

                    // Create a new file     
                    using (StreamWriter sw = File.CreateText(path + "/" + template.SosigEnemyID + ".json"))
                    {
                        SosigTemplate sosig = new SosigTemplate(template);
                        string characterString = JsonConvert.SerializeObject(sosig, Formatting.Indented, new StringEnumConverter());
                        sw.WriteLine(characterString);
                        sw.Close();
                    }
                }

            }

            catch (Exception ex)
            {
                TNHTweakerLogger.LogError(ex.ToString());
            }
        }

        public static void CreateJsonVaultFiles(string path)
        {
            try
            {
                path = path + "/VaultFiles";

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string[] vaultFiles = ES2.GetFiles(string.Empty, "*.txt");
                List<SavedGunSerializable> savedGuns = new List<SavedGunSerializable>();
                foreach(string name in vaultFiles)
                {
                    try
                    {
                        if (name.Contains("DONTREMOVETHISPARTOFFILENAMEV02a"))
                        {
                            if (ES2.Exists(name))
                            {
                                using (ES2Reader reader = ES2Reader.Create(name))
                                {
                                    savedGuns.Add(new SavedGunSerializable(reader.Read<SavedGun>("SavedGun")));
                                }
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        TNHTweakerLogger.LogError("Vault File could not be loaded");
                    }
                }
                
                foreach (SavedGunSerializable savedGun in savedGuns)
                {
                    if (File.Exists(path + "/" + savedGun.FileName + ".json"))
                    {
                        File.Delete(path + "/" + savedGun.FileName + ".json");
                    }

                    // Create a new file     
                    using (StreamWriter sw = File.CreateText(path + "/" + savedGun.FileName + ".json"))
                    {
                        string characterString = JsonConvert.SerializeObject(savedGun, Formatting.Indented, new StringEnumConverter());
                        sw.WriteLine(characterString);
                        sw.Close();
                    }
                }
            }

            catch (Exception ex)
            {
                TNHTweakerLogger.LogError(ex.ToString());
            }
        }



        public static void CreateGeneratedTables(string path)
        {
            try
            {
                path = path + "/GeneratedEquipmentPools";

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }


                foreach(CustomCharacter character in LoadedTemplateManager.LoadedCharactersDict.Values)
                {
                    // Create a new file     
                    using (StreamWriter sw = File.CreateText(path + "/" + character.DisplayName + ".txt"))
                    {
                        sw.WriteLine("Primary Starting Weapon");
                        if(character.HasPrimaryWeapon)
                        {
                            sw.WriteLine(character.PrimaryWeapon.ToString());
                        }

                        sw.WriteLine("\n\nSecondary Starting Weapon");
                        if (character.HasSecondaryWeapon)
                        {
                            sw.WriteLine(character.SecondaryWeapon.ToString());
                        }

                        sw.WriteLine("\n\nTertiary Starting Weapon");
                        if (character.HasTertiaryWeapon)
                        {
                            sw.WriteLine(character.TertiaryWeapon.ToString());
                        }

                        sw.WriteLine("\n\nPrimary Starting Item");
                        if (character.HasPrimaryItem)
                        {
                            sw.WriteLine(character.PrimaryItem.ToString());
                        }

                        sw.WriteLine("\n\nSecondary Starting Item");
                        if (character.HasSecondaryItem)
                        {
                            sw.WriteLine(character.SecondaryItem.ToString());
                        }

                        sw.WriteLine("\n\nTertiary Starting Item");
                        if (character.HasTertiaryItem)
                        {
                            sw.WriteLine(character.TertiaryItem.ToString());
                        }

                        sw.WriteLine("\n\nStarting Shield");
                        if (character.HasShield)
                        {
                            sw.WriteLine(character.Shield.ToString());
                        }

                        foreach(EquipmentPool pool in character.EquipmentPools)
                        {
                            sw.WriteLine("\n\n" + pool.ToString());
                        }

                        sw.Close();
                    }
                }
            }

            catch (Exception ex)
            {
                //Debug.LogError(ex.ToString());
            }
        }


        public static void RemoveUnloadedObjectIDs(EquipmentGroup group)
        {
            if (group.IDOverride != null)
            {
                for (int i = 0; i < group.IDOverride.Count; i++)
                {
                    if (!IM.OD.ContainsKey(group.IDOverride[i]))
                    {
                        //If this is a vaulted gun with all it's components loaded, we should still have this in the object list
                        if (LoadedTemplateManager.LoadedVaultFiles.ContainsKey(group.IDOverride[i]))
                        {
                            if (!LoadedTemplateManager.LoadedVaultFiles[group.IDOverride[i]].AllComponentsLoaded())
                            {
                                TNHTweakerLogger.LogWarning("TNHTweaker -- Vaulted gun in table does not have all components loaded, removing it! VaultID : " + group.IDOverride[i]);
                                group.IDOverride.RemoveAt(i);
                                i -= 1;
                            }
                        }

                        //If this was not a vaulted gun, remove it
                        else
                        {
                            TNHTweakerLogger.LogWarning("TNHTweaker -- Object in table not loaded, removing it from object table! ObjectID : " + group.IDOverride[i]);
                            group.IDOverride.RemoveAt(i);
                            i -= 1;
                        }
                    }
                }
            }
        }


        public static void RemoveUnloadedObjectIDs(SosigTemplate template)
        {
            
            //Loop through all outfit configs and remove any clothing objects that don't exist
            foreach (OutfitConfig config in template.OutfitConfigs)
            {
                for(int i = 0; i < config.Headwear.Count; i++)
                {
                    if (!IM.OD.ContainsKey(config.Headwear[i]))
                    {
                        TNHTweakerLogger.LogWarning("TNHTweaker -- Clothing item not loaded, removing it from clothing config! ObjectID : " + config.Headwear[i]);
                        config.Headwear.RemoveAt(i);
                        i -= 1;
                    }
                }
                if (config.Headwear.Count == 0) config.Chance_Headwear = 0;

                for (int i = 0; i < config.Facewear.Count; i++)
                {
                    if (!IM.OD.ContainsKey(config.Facewear[i]))
                    {
                        TNHTweakerLogger.LogWarning("TNHTweaker -- Clothing item not loaded, removing it from clothing config! ObjectID : " + config.Facewear[i]);
                        config.Facewear.RemoveAt(i);
                        i -= 1;
                    }
                }
                if (config.Facewear.Count == 0) config.Chance_Facewear = 0;

                for (int i = 0; i < config.Eyewear.Count; i++)
                {
                    if (!IM.OD.ContainsKey(config.Eyewear[i]))
                    {
                        TNHTweakerLogger.LogWarning("TNHTweaker -- Clothing item not loaded, removing it from clothing config! ObjectID : " + config.Eyewear[i]);
                        config.Eyewear.RemoveAt(i);
                        i -= 1;
                    }
                }
                if (config.Eyewear.Count == 0) config.Chance_Eyewear = 0;

                for (int i = 0; i < config.Torsowear.Count; i++)
                {
                    if (!IM.OD.ContainsKey(config.Torsowear[i]))
                    {
                        TNHTweakerLogger.LogWarning("TNHTweaker -- Clothing item not loaded, removing it from clothing config! ObjectID : " + config.Torsowear[i]);
                        config.Torsowear.RemoveAt(i);
                        i -= 1;
                    }
                }
                if (config.Torsowear.Count == 0) config.Chance_Torsowear = 0;

                for (int i = 0; i < config.Pantswear.Count; i++)
                {
                    if (!IM.OD.ContainsKey(config.Pantswear[i]))
                    {
                        TNHTweakerLogger.LogWarning("TNHTweaker -- Clothing item not loaded, removing it from clothing config! ObjectID : " + config.Pantswear[i]);
                        config.Pantswear.RemoveAt(i);
                        i -= 1;
                    }
                }
                if (config.Pantswear.Count == 0) config.Chance_Pantswear = 0;

                for (int i = 0; i < config.Pantswear_Lower.Count; i++)
                {
                    if (!IM.OD.ContainsKey(config.Pantswear_Lower[i]))
                    {
                        TNHTweakerLogger.LogWarning("TNHTweaker -- Clothing item not loaded, removing it from clothing config! ObjectID : " + config.Pantswear_Lower[i]);
                        config.Pantswear_Lower.RemoveAt(i);
                        i -= 1;
                    }
                }
                if (config.Pantswear_Lower.Count == 0) config.Chance_Pantswear_Lower = 0;

                for (int i = 0; i < config.Backpacks.Count; i++)
                {
                    if (!IM.OD.ContainsKey(config.Backpacks[i]))
                    {
                        TNHTweakerLogger.LogWarning("TNHTweaker -- Clothing item not loaded, removing it from clothing config! ObjectID : " + config.Backpacks[i]);
                        config.Backpacks.RemoveAt(i);
                        i -= 1;
                    }
                }
                if (config.Backpacks.Count == 0) config.Chance_Backpacks = 0;
            }

        }


        /// <summary>
        /// Loads a sprite from a file path. Solution found here: https://forum.unity.com/threads/generating-sprites-dynamically-from-png-or-jpeg-files-in-c.343735/
        /// </summary>
        /// <param name=""></param>
        /// <param name="pixelsPerUnit"></param>
        /// <returns></returns>
        public static Sprite LoadSprite(IFileHandle file)
        {
            Texture2D spriteTexture = LoadTexture(file);
            if (spriteTexture == null) return null;
            Sprite sprite = Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height), new Vector2(0, 0), 100f);
            sprite.name = file.Name;
            return sprite;
        }



        public static Sprite LoadSprite(Texture2D spriteTexture, float pixelsPerUnit = 100f)
        {
            return Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height), new Vector2(0, 0), pixelsPerUnit);
        }



        /// <summary>
        /// Loads a texture2D from the sent file. Source: https://stackoverflow.com/questions/1080442/how-to-convert-an-stream-into-a-byte-in-c
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static Texture2D LoadTexture(IFileHandle file)
        {
            // Load a PNG or JPG file from disk to a Texture2D
            // Returns null if load fails

            Stream fileStream = file.OpenRead();
            MemoryStream mem = new MemoryStream();

            CopyStream(fileStream, mem);

            byte[] fileData = mem.ToArray();

            Texture2D tex2D = new Texture2D(2, 2);          
            if (tex2D.LoadImage(fileData)) return tex2D;

            return null;                     
        }



        /// <summary>
        /// Copies the input stream into the output stream. Source: https://stackoverflow.com/questions/1080442/how-to-convert-an-stream-into-a-byte-in-c
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        public static void CopyStream(Stream input, Stream output)
        {
            byte[] b = new byte[32768];
            int r;
            while ((r = input.Read(b, 0, b.Length)) > 0)
                output.Write(b, 0, r);
        }



        public static IEnumerator SpawnFirearm(SavedGunSerializable savedGun, Vector3 position, Quaternion rotation)
        {
            List<GameObject> toDealWith = new List<GameObject>();
            List<GameObject> toMoveToTrays = new List<GameObject>();
            FVRFireArm myGun = null;
            FVRFireArmMagazine myMagazine = null;
            List<int> validIndexes = new List<int>();
            Dictionary<GameObject, SavedGunComponent> dicGO = new Dictionary<GameObject, SavedGunComponent>();
            Dictionary<int, GameObject> dicByIndex = new Dictionary<int, GameObject>();
            List<AnvilCallback<GameObject>> callbackList = new List<AnvilCallback<GameObject>>();

            SavedGun gun = savedGun.GetSavedGun();

            for (int i = 0; i < gun.Components.Count; i++)
            {
                callbackList.Add(IM.OD[gun.Components[i].ObjectID].GetGameObjectAsync());
            }
            yield return callbackList;
            for (int j = 0; j < gun.Components.Count; j++)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(callbackList[j].Result);

                dicGO.Add(gameObject, gun.Components[j]);
                dicByIndex.Add(gun.Components[j].Index, gameObject);
                if (gun.Components[j].isFirearm)
                {
                    myGun = gameObject.GetComponent<FVRFireArm>();
                    savedGun.ApplyFirearmProperties(myGun);
                    
                    validIndexes.Add(j);
                    gameObject.transform.position = position;
                    gameObject.transform.rotation = Quaternion.identity;
                }
                else if (gun.Components[j].isMagazine)
                {
                    myMagazine = gameObject.GetComponent<FVRFireArmMagazine>();
                    validIndexes.Add(j);
                    if (myMagazine != null)
                    {
                        gameObject.transform.position = myGun.GetMagMountPos(myMagazine.IsBeltBox).position;
                        gameObject.transform.rotation = myGun.GetMagMountPos(myMagazine.IsBeltBox).rotation;
                        myMagazine.Load(myGun);
                        myMagazine.IsInfinite = false;
                    }
                }
                else if (gun.Components[j].isAttachment)
                {
                    toDealWith.Add(gameObject);
                }
                else
                {
                    toMoveToTrays.Add(gameObject);
                    if (gameObject.GetComponent<Speedloader>() != null && gun.LoadedRoundsInMag.Count > 0)
                    {
                        Speedloader component = gameObject.GetComponent<Speedloader>();
                        component.ReloadSpeedLoaderWithList(gun.LoadedRoundsInMag);
                    }
                    else if (gameObject.GetComponent<FVRFireArmClip>() != null && gun.LoadedRoundsInMag.Count > 0)
                    {
                        FVRFireArmClip component2 = gameObject.GetComponent<FVRFireArmClip>();
                        component2.ReloadClipWithList(gun.LoadedRoundsInMag);
                    }
                }
                gameObject.GetComponent<FVRPhysicalObject>().ConfigureFromFlagDic(gun.Components[j].Flags);
            }
            if (myGun.Magazine != null && gun.LoadedRoundsInMag.Count > 0)
            {
                myGun.Magazine.ReloadMagWithList(gun.LoadedRoundsInMag);
                myGun.Magazine.IsInfinite = false;
            }
            int BreakIterator = 200;
            while (toDealWith.Count > 0 && BreakIterator > 0)
            {
                BreakIterator--;
                for (int k = toDealWith.Count - 1; k >= 0; k--)
                {
                    SavedGunComponent savedGunComponent = dicGO[toDealWith[k]];
                    if (validIndexes.Contains(savedGunComponent.ObjectAttachedTo))
                    {
                        GameObject gameObject2 = toDealWith[k];
                        FVRFireArmAttachment component3 = gameObject2.GetComponent<FVRFireArmAttachment>();
                        FVRFireArmAttachmentMount mount = GetMount(dicByIndex[savedGunComponent.ObjectAttachedTo], savedGunComponent.MountAttachedTo);
                        gameObject2.transform.rotation = Quaternion.LookRotation(savedGunComponent.OrientationForward, savedGunComponent.OrientationUp);
                        gameObject2.transform.position = GetPositionRelativeToGun(savedGunComponent, myGun.transform);
                        if (component3.CanScaleToMount && mount.CanThisRescale())
                        {
                            component3.ScaleToMount(mount);
                        }
                        component3.AttachToMount(mount, false);
                        if (component3 is Suppressor)
                        {
                            (component3 as Suppressor).AutoMountWell();
                        }
                        validIndexes.Add(savedGunComponent.Index);
                        toDealWith.RemoveAt(k);
                    }
                }
            }
            int trayIndex = 0;
            int itemIndex = 0;
            for (int l = 0; l < toMoveToTrays.Count; l++)
            {
                toMoveToTrays[l].transform.position = position + (float)itemIndex * 0.1f * Vector3.up;
                toMoveToTrays[l].transform.rotation = rotation;
                itemIndex++;
                trayIndex++;
                if (trayIndex > 2)
                {
                    trayIndex = 0;
                }
            }
            myGun.SetLoadedChambers(gun.LoadedRoundsInChambers);
            myGun.SetFromFlagList(gun.SavedFlags);
            myGun.transform.rotation = rotation;
            yield break;
        }

        public static FVRFireArmAttachmentMount GetMount(GameObject obj, int index)
        {
            return obj.GetComponent<FVRPhysicalObject>().AttachmentMounts[index];
        }

        public static Vector3 GetPositionRelativeToGun(SavedGunComponent data, Transform gun)
        {
            Vector3 a = gun.position;
            a += gun.up * data.PosOffset.y;
            a += gun.right * data.PosOffset.x;
            return a + gun.forward * data.PosOffset.z;
        }
        /// <summary>
        /// Used to spawn more than one, same objects at a position
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="position"></param>
        /// <param name="count"></param>
        /// <param name="tolerance"></param>
        public static IEnumerator InstantiateMutltiple(GameObject gameObject, Vector3 position, int count,
            float tolerance = 1.3f)
        {
            float heightNeeded = (gameObject.GetMaxBounds().size.y / 2) * tolerance;
            for (var index = 0; index < count; index++)
            {
                float current = index * heightNeeded;
                UnityEngine.Object.Instantiate(gameObject, position + (Vector3.up * current),
                    new Quaternion());
                yield return null;
            }
        }
    }
}
