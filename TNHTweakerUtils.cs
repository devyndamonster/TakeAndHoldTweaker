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
using Valve.Newtonsoft.Json;
using Valve.Newtonsoft.Json.Converters;

namespace FistVR
{
    class TNHTweakerUtils
    {
        public static bool CacheLoaded = true;

        public static void CreateObjectIDFile(string path)
        {
            try
            {
                if (File.Exists(path + "/ObjectIDs.txt"))
                {
                    File.Delete(path + "/ObjectIDs.txt");
                }

                // Create a new file     
                using (StreamWriter sw = File.CreateText(path + "/ObjectIDs.txt"))
                {
                    sw.WriteLine("#Available object IDs for overrides");
                    foreach (string objID in IM.OD.Keys)
                    {
                        sw.WriteLine(objID);
                    }
                    sw.Close();
                }
            }

            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
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
                Debug.LogError(ex.ToString());
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
                Debug.LogError(ex.ToString());
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
                        TNHTweakerLogger.Log("TNHTWEAKER -- ICON FOUND (" + pool.TableDef.Icon.name + ")", TNHTweakerLogger.LogType.Character);
                        icons.Add(pool.TableDef.Icon.name, pool.TableDef.Icon);
                    }
                }
            }

            return icons;
        }


        public static bool ListContainsObjectID(List<FVRObject> objs, string ID)
        {
            foreach(FVRObject obj in objs)
            {
                if (obj.ItemID.Equals(ID))
                {
                    return true;
                }
            }

            return false;
        }

        public static void CreateDefaultCharacterFiles(List<CustomCharacter> characters, string path)
        {

            try
            {
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
                        TNHTweakerLogger.Log("TNHTWEAKER -- SERIALIZING", TNHTweakerLogger.LogType.File);
                        string characterString = JsonConvert.SerializeObject(charDef, Formatting.Indented, new StringEnumConverter());
                        //TNHTweakerLogger.Log(characterString, TNHTweakerLogger.LogType.File);
                        sw.WriteLine(characterString);
                        sw.Close();
                        TNHTweakerLogger.Log("TNHTWEAKER -- LOADED CHARACTER:", TNHTweakerLogger.LogType.File);
                    }
                }

                TNHTweakerLogger.Log("TNHTWEAKER -- FINISHED LOADING DEFAULT CHARACTERS", TNHTweakerLogger.LogType.File);
            }

            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }
        }

        public static void CreateDefaultSosigTemplateFiles(List<SosigEnemyTemplate> sosigs, string path)
        {
            try
            {
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
                        TNHTweakerLogger.Log("TNHTWEAKER -- CREATING SOSIG TEMPLATE OBJECT", TNHTweakerLogger.LogType.File);
                        SosigTemplate sosig = new SosigTemplate(template);
                        TNHTweakerLogger.Log("TNHTWEAKER -- SERIALIZING", TNHTweakerLogger.LogType.File);
                        string characterString = JsonConvert.SerializeObject(sosig, Formatting.Indented, new StringEnumConverter());
                        //TNHTweakerLogger.Log(characterString, TNHTweakerLogger.LogType.File);
                        sw.WriteLine(characterString);
                        sw.Close();
                        TNHTweakerLogger.Log("TNHTWEAKER -- LOADED SOSIG:", TNHTweakerLogger.LogType.File);
                    }
                }

                TNHTweakerLogger.Log("TNHTWEAKER -- FINISHED LOADING DEFAULT SOSIG TEMPLATES", TNHTweakerLogger.LogType.File);
            }

            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
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
                        if (ES2.Exists(name))
                        {
                            using (ES2Reader reader = ES2Reader.Create(name))
                            {
                                savedGuns.Add(new SavedGunSerializable(reader.Read<SavedGun>("SavedGun")));
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.LogError("Vault File could not be loaded");
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
                Debug.LogError(ex.ToString());
            }
        }

        public static void RemoveUnloadedObjectIDs(CustomCharacter character)
        {
            if (character.HasPrimaryWeapon)
            {
                foreach (ObjectPool table in character.PrimaryWeapon.Tables)
                {
                    RemoveUnloadedObjectIDs(table);
                }
            }

            if (character.HasSecondaryWeapon)
            {
                foreach (ObjectPool table in character.SecondaryWeapon.Tables)
                {
                    RemoveUnloadedObjectIDs(table);
                }
            }

            if (character.HasTertiaryWeapon)
            {
                foreach (ObjectPool table in character.TertiaryWeapon.Tables)
                {
                    RemoveUnloadedObjectIDs(table);
                }
            }

            if (character.HasPrimaryItem)
            {
                foreach (ObjectPool table in character.PrimaryItem.Tables)
                {
                    RemoveUnloadedObjectIDs(table);
                }
            }

            if (character.HasSecondaryItem)
            {
                foreach (ObjectPool table in character.SecondaryItem.Tables)
                {
                    RemoveUnloadedObjectIDs(table);
                }
            }

            if (character.HasTertiaryItem)
            {
                foreach (ObjectPool table in character.TertiaryItem.Tables)
                {
                    RemoveUnloadedObjectIDs(table);
                }
            }

            if (character.HasShield)
            {
                foreach (ObjectPool table in character.Shield.Tables)
                {
                    RemoveUnloadedObjectIDs(table);
                }
            }

            foreach (EquipmentPool pool in character.EquipmentPools)
            {
                foreach(ObjectPool table in pool.Tables)
                {
                    RemoveUnloadedObjectIDs(table);
                }
            }
        }

        public static void RemoveUnloadedObjectIDs(ObjectPool pool)
        {
            ObjectTableDef table = pool.GetObjectTableDef();

            if (table.UseIDListOverride)
            {
                for (int i = 0; i < table.IDOverride.Count; i++)
                {
                    if (!IM.OD.ContainsKey(table.IDOverride[i]))
                    {
                        if (LoadedTemplateManager.LoadedVaultFiles.ContainsKey(table.IDOverride[i]))
                        {
                            pool.GetObjects().Add(table.IDOverride[i]);
                        }

                        Debug.LogWarning("TNHTweaker -- Object in table not loaded, removing it from object table! ObjectID : " + table.IDOverride[i]);
                        table.IDOverride.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        pool.GetObjects().Add(table.IDOverride[i]);
                    }
                }
            }
        }


        public static void RemoveUnloadedObjectIDs(SosigTemplate template)
        {
            if (template.DroppedObjectPool != null)
            {
                RemoveUnloadedObjectIDs(template.DroppedObjectPool);
            }
            
            //Loop through all outfit configs and remove any clothing objects that don't exist
            foreach (OutfitConfig config in template.OutfitConfigs)
            {
                for(int i = 0; i < config.Headwear.Count; i++)
                {
                    if (!IM.OD.ContainsKey(config.Headwear[i]))
                    {
                        Debug.LogWarning("TNHTweaker -- Clothing item not loaded, removing it from clothing config! ObjectID : " + config.Headwear[i]);
                        config.Headwear.RemoveAt(i);
                        i -= 1;
                    }
                }

                for (int i = 0; i < config.Facewear.Count; i++)
                {
                    if (!IM.OD.ContainsKey(config.Facewear[i]))
                    {
                        Debug.LogWarning("TNHTweaker -- Clothing item not loaded, removing it from clothing config! ObjectID : " + config.Facewear[i]);
                        config.Facewear.RemoveAt(i);
                        i -= 1;
                    }
                }

                for (int i = 0; i < config.Eyewear.Count; i++)
                {
                    if (!IM.OD.ContainsKey(config.Eyewear[i]))
                    {
                        Debug.LogWarning("TNHTweaker -- Clothing item not loaded, removing it from clothing config! ObjectID : " + config.Eyewear[i]);
                        config.Eyewear.RemoveAt(i);
                        i -= 1;
                    }
                }

                for (int i = 0; i < config.Torsowear.Count; i++)
                {
                    if (!IM.OD.ContainsKey(config.Torsowear[i]))
                    {
                        Debug.LogWarning("TNHTweaker -- Clothing item not loaded, removing it from clothing config! ObjectID : " + config.Torsowear[i]);
                        config.Torsowear.RemoveAt(i);
                        i -= 1;
                    }
                }

                for (int i = 0; i < config.Pantswear.Count; i++)
                {
                    if (!IM.OD.ContainsKey(config.Pantswear[i]))
                    {
                        Debug.LogWarning("TNHTweaker -- Clothing item not loaded, removing it from clothing config! ObjectID : " + config.Pantswear[i]);
                        config.Pantswear.RemoveAt(i);
                        i -= 1;
                    }
                }

                for (int i = 0; i < config.Pantswear_Lower.Count; i++)
                {
                    if (!IM.OD.ContainsKey(config.Pantswear_Lower[i]))
                    {
                        Debug.LogWarning("TNHTweaker -- Clothing item not loaded, removing it from clothing config! ObjectID : " + config.Pantswear_Lower[i]);
                        config.Pantswear_Lower.RemoveAt(i);
                        i -= 1;
                    }
                }

                for (int i = 0; i < config.Backpacks.Count; i++)
                {
                    if (!IM.OD.ContainsKey(config.Backpacks[i]))
                    {
                        Debug.LogWarning("TNHTweaker -- Clothing item not loaded, removing it from clothing config! ObjectID : " + config.Backpacks[i]);
                        config.Backpacks.RemoveAt(i);
                        i -= 1;
                    }
                }
            }

        }


        /// <summary>
        /// Returns wether of not the type sent is a type of generic list. Solution found here: https://stackoverflow.com/questions/794198/how-do-i-check-if-a-given-value-is-a-generic-list/41687428
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsGenericList(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }

        /// <summary>
        /// Creates a list of the sent type. Solution found here: https://stackoverflow.com/questions/2493215/create-list-of-variable-type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IList CreateGenericList(Type type)
        {
            Type genericListType = typeof(List<>).MakeGenericType(type);
            return (IList)Activator.CreateInstance(genericListType);
        }

        /// <summary>
        /// Loads a sprite from a file path. Solution found here: https://forum.unity.com/threads/generating-sprites-dynamically-from-png-or-jpeg-files-in-c.343735/
        /// </summary>
        /// <param name=""></param>
        /// <param name="pixelsPerUnit"></param>
        /// <returns></returns>
        public static Sprite LoadSprite(string path, float pixelsPerUnit = 100f)
        {
            Texture2D spriteTexture = LoadTexture(path);
            if (spriteTexture == null) return null;
            Sprite sprite = Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height), new Vector2(0, 0), pixelsPerUnit);
            sprite.name = path;
            return sprite;
        }

        public static Sprite LoadSprite(Texture2D spriteTexture, float pixelsPerUnit = 100f)
        {
            return Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height), new Vector2(0, 0), pixelsPerUnit);
        }

        public static Texture2D LoadTexture(string FilePath)
        {
            // Load a PNG or JPG file from disk to a Texture2D
            // Returns null if load fails

            Texture2D Tex2D;
            byte[] FileData;

            if (File.Exists(FilePath))
            {
                FileData = File.ReadAllBytes(FilePath);
                Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
                if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                    return Tex2D;                 // If data = readable -> return texture
            }
            return null;                     // Return null if load failed
        }


        public static FVRObject GetMagazineForEquipped()
        {
            List<FVRObject> heldGuns = new List<FVRObject>();

            FVRInteractiveObject rightHandObject = GM.CurrentMovementManager.Hands[0].CurrentInteractable;
            FVRInteractiveObject leftHandObject = GM.CurrentMovementManager.Hands[1].CurrentInteractable;

            if(rightHandObject is FVRFireArm && (rightHandObject as FVRFireArm).ObjectWrapper != null)
            {
                heldGuns.Add((rightHandObject as FVRFireArm).ObjectWrapper);
            }
            if (leftHandObject is FVRFireArm && (leftHandObject as FVRFireArm).ObjectWrapper != null)
            {
                heldGuns.Add((leftHandObject as FVRFireArm).ObjectWrapper);
            }

            foreach(FVRQuickBeltSlot slot in GM.CurrentPlayerBody.QuickbeltSlots)
            {
                if (slot.CurObject is FVRFireArm && (slot.CurObject as FVRFireArm).ObjectWrapper != null)
                {
                    FVRObject firearm = (slot.CurObject as FVRFireArm).ObjectWrapper;

                    if(firearm.CompatibleClips.Count > 0 || firearm.CompatibleMagazines.Count > 0 || firearm.CompatibleSpeedLoaders.Count > 0)
                    {
                        heldGuns.Add(firearm);
                    }
                }

                else if (slot.CurObject is PlayerBackPack && (slot.CurObject as PlayerBackPack).ObjectWrapper != null)
                {
                    foreach (FVRQuickBeltSlot backpackSlot in GM.CurrentPlayerBody.QuickbeltSlots)
                    {
                        if (backpackSlot.CurObject is FVRFireArm && (backpackSlot.CurObject as FVRFireArm).ObjectWrapper != null)
                        {
                            FVRObject firearm = (backpackSlot.CurObject as FVRFireArm).ObjectWrapper;

                            if (firearm.CompatibleClips.Count > 0 || firearm.CompatibleMagazines.Count > 0 || firearm.CompatibleSpeedLoaders.Count > 0)
                            {
                                heldGuns.Add(firearm);
                            }
                        }
                    }
                }
            }

            if(heldGuns.Count > 0)
            {
                FVRObject firearm = heldGuns.GetRandom();

                if (firearm.CompatibleMagazines.Count > 0) return firearm.CompatibleMagazines.GetRandom();
                else if (firearm.CompatibleClips.Count > 0) return firearm.CompatibleClips.GetRandom();
                else return firearm.CompatibleSpeedLoaders.GetRandom();
            }

            return null;
        }

        
        public static void LoadMagazineCache(string path)
        {
            CompatibleMagazineCache magazineCache = null;

            path = path + "/CachedCompatibleMags.json";

            //If the cache exists, we load it and check it's validity
            if (File.Exists(path))
            {
                string cacheJson = File.ReadAllText(path);
                magazineCache = JsonConvert.DeserializeObject<CompatibleMagazineCache>(cacheJson);

                if (!IsMagazineCacheValid(magazineCache))
                {
                    File.Delete(path);
                    magazineCache = null;
                }
            }

            //If the magazine cache file didn't exist, or wasn't valid, we must build a new one
            if (magazineCache == null)
            {
                TNHTweakerLogger.Log("TNHTWEAKER -- BUILDING NEW MAGAZINE CACHE", TNHTweakerLogger.LogType.File);
                magazineCache = new CompatibleMagazineCache();

                //Load all of the magazines into the cache
                foreach (FVRObject magazine in ManagerSingleton<IM>.Instance.odicTagCategory[FVRObject.ObjectCategory.Magazine])
                {
                    FVRFireArmMagazine magComp = magazine.GetGameObject().GetComponent<FVRFireArmMagazine>();
                    magazineCache.Magazines.Add(magazine.ItemID);

                    if (magComp != null)
                    {
                        magazineCache.MagazineObjects.Add(magComp);
                        magazineCache.AddMagazineData(magComp);
                    }
                }

                //Load all firearms into the cache
                foreach (FVRObject firearm in ManagerSingleton<IM>.Instance.odicTagCategory[FVRObject.ObjectCategory.Firearm])
                {
                    FVRFireArm firearmComp = firearm.GetGameObject().GetComponent<FVRFireArm>();

                    magazineCache.Firearms.Add(firearm.ItemID);

                    if (firearmComp == null)
                    {
                        continue;
                    }

                    bool addedMagazine = false;
                    foreach (FVRFireArmMagazine magazine in magazineCache.MagazineObjects)
                    {
                        if (firearmComp.MagazineType == magazine.MagazineType && !ListContainsObjectID(firearm.CompatibleMagazines, magazine.ObjectWrapper.ItemID))
                        {
                            //If this is the first time a magazine has been added to this firearm, we need to create an entry
                            if (!addedMagazine)
                            {
                                magazineCache.Entries.Add(new MagazineCacheEntry());
                                magazineCache.Entries.Last().FirearmID = firearm.ItemID;
                            }
                            addedMagazine = true;

                            //Update the ammo and compatible magazines for the entry
                            if (firearm.MaxCapacityRelated < magazine.m_capacity)
                            {
                                firearm.MaxCapacityRelated = magazine.m_capacity;
                            }
                            if (firearm.MinCapacityRelated > magazine.m_capacity)
                            {
                                firearm.MinCapacityRelated = magazine.m_capacity;
                            }
                            magazineCache.Entries.Last().MaxAmmo = firearm.MaxCapacityRelated;
                            magazineCache.Entries.Last().MinAmmo = firearm.MinCapacityRelated;
                            firearm.CompatibleMagazines.Add(magazine.ObjectWrapper);
                            magazineCache.Entries.Last().CompatibleMagazines.Add(magazine.ObjectWrapper.ItemID);
                            
                            TNHTweakerLogger.Log("TNHTWEAKER -- ADDED COMPATIBLE MAGAZINE (" + magazine.ObjectWrapper.ItemID + ") TO FIREARM (" + firearm.ItemID + ")", TNHTweakerLogger.LogType.File);
                        }
                    }

                }

                LoadedTemplateManager.LoadedMagazines = magazineCache.MagazineData;

                //Create the cache file 
                using (StreamWriter sw = File.CreateText(path))
                {
                    string cacheString = JsonConvert.SerializeObject(magazineCache, Formatting.Indented, new StringEnumConverter());
                    sw.WriteLine(cacheString);
                    sw.Close();
                }
            }

            //If the cache is valid, we can just load each entry from the cache
            else
            {
                TNHTweakerLogger.Log("TNHTWEAKER -- LOADING EXISTING MAGAZINE CACHE", TNHTweakerLogger.LogType.File);

                foreach (MagazineCacheEntry entry in magazineCache.Entries)
                {
                    if (IM.OD.ContainsKey(entry.FirearmID))
                    {
                        FVRObject firearm = IM.OD[entry.FirearmID];
                        foreach (string mag in entry.CompatibleMagazines)
                        {
                            if (IM.OD.ContainsKey(mag))
                            {
                                firearm.CompatibleMagazines.Add(IM.OD[mag]);
                            }
                        }
                        firearm.MaxCapacityRelated = entry.MaxAmmo;
                        firearm.MinCapacityRelated = entry.MinAmmo;
                    }
                }

                LoadedTemplateManager.LoadedMagazines = magazineCache.MagazineData;
            }

            CacheLoaded = true;
        }


        /// <summary>
        /// Returns true if every gun and magazine is found within the cache
        /// </summary>
        /// <param name="magazineCache"></param>
        /// <returns></returns>
        public static bool IsMagazineCacheValid(CompatibleMagazineCache magazineCache)
        {
            bool cacheValid = true;

            //TODO: There will likely be problems with firearms max and min capacities not being correct if the cache was built with mods, and then had mods disabled

            /* NOTE: This optimization is disabled for the sake of debugging
            //Determine if the magazine cache is valid
            if (magazineCache.Magazines.Count != ManagerSingleton<IM>.Instance.odicTagCategory[FVRObject.ObjectCategory.Magazine].Count ||
                magazineCache.Firearms.Count != ManagerSingleton<IM>.Instance.odicTagCategory[FVRObject.ObjectCategory.Firearm].Count) 
            {
                return false;
            }
            */
             
            //NOTE: you could return false immediately in here, but we don't for the sake of debugging
            foreach (string mag in ManagerSingleton<IM>.Instance.odicTagCategory[FVRObject.ObjectCategory.Magazine].Select(f => f.ItemID))
            {
                if (!magazineCache.Magazines.Contains(mag))
                {
                    TNHTweakerLogger.Log("TNHTWEAKER -- MAGAZINE NOT FOUND IN CACHE: " + mag, TNHTweakerLogger.LogType.File);
                    cacheValid = false;
                }
            }
            foreach (string firearm in ManagerSingleton<IM>.Instance.odicTagCategory[FVRObject.ObjectCategory.Firearm].Select(f => f.ItemID))
            {
                if (!magazineCache.Firearms.Contains(firearm))
                {
                    TNHTweakerLogger.Log("TNHTWEAKER -- FIREARM NOT FOUND IN CACHE: " + firearm, TNHTweakerLogger.LogType.File);
                    cacheValid = false;
                }
            }
            

            return cacheValid;
        }


        public static IEnumerator SpawnFirearm(SavedGunSerializable savedGun, Transform spawnPoint)
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
                    validIndexes.Add(j);
                    gameObject.transform.position = spawnPoint.position;
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
                        Debug.Log("Loading round set to speedloader");
                        Speedloader component = gameObject.GetComponent<Speedloader>();
                        component.ReloadSpeedLoaderWithList(gun.LoadedRoundsInMag);
                    }
                    else if (gameObject.GetComponent<FVRFireArmClip>() != null && gun.LoadedRoundsInMag.Count > 0)
                    {
                        Debug.Log("Loading round set to clip");
                        FVRFireArmClip component2 = gameObject.GetComponent<FVRFireArmClip>();
                        component2.ReloadClipWithList(gun.LoadedRoundsInMag);
                    }
                }
                gameObject.GetComponent<FVRPhysicalObject>().ConfigureFromFlagDic(gun.Components[j].Flags);
            }
            if (myGun.Magazine != null && gun.LoadedRoundsInMag.Count > 0)
            {
                Debug.Log("Loading round set to magazine");
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
                toMoveToTrays[l].transform.position = spawnPoint.position + (float)itemIndex * 0.1f * Vector3.up;
                toMoveToTrays[l].transform.rotation = spawnPoint.rotation;
                itemIndex++;
                trayIndex++;
                if (trayIndex > 2)
                {
                    trayIndex = 0;
                }
            }
            myGun.SetLoadedChambers(gun.LoadedRoundsInChambers);
            myGun.SetFromFlagList(gun.SavedFlags);
            myGun.transform.rotation = spawnPoint.rotation;
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
    }

    


    public class Vector2Serializable
    {
        public float x;
        public float y;

        [JsonIgnore]
        private Vector2 v;

        public Vector2Serializable() { }

        public Vector2Serializable(Vector2 v)
        {
            x = v.x;
            y = v.y;
            this.v = v;
        }

        public Vector2 GetVector2()
        {
            v = new Vector2(x,y);
            return v;
        }
    }

    public class Vector3Serializable
    {
        public float x;
        public float y;
        public float z;

        [JsonIgnore]
        private Vector3 v;

        public Vector3Serializable() { }

        public Vector3Serializable(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
            this.v = v;
        }

        public Vector3 GetVector3()
        {
            v = new Vector3(x, y, z);
            return v;
        }
    }




}
