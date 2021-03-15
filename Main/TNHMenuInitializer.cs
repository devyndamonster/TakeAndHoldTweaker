using Deli.Newtonsoft.Json;
using Deli.Newtonsoft.Json.Converters;
using FistVR;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TNHTweaker.ObjectTemplates;
using TNHTweaker.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace TNHTweaker
{
    public static class TNHMenuInitializer
    {

        public static bool TNHInitialized = false;
        public static bool MagazineCacheLoaded = false;

        public static IEnumerator InitializeTNHMenuAsync(string path, Text text, SceneLoader hotdog, List<TNH_UIManager.CharacterCategory> Categories, TNH_CharacterDatabase CharDatabase, TNH_UIManager instance, bool outputFiles)
        {
            hotdog.gameObject.SetActive(false);

            //First thing we want to do is wait for all asset bundles to be loaded in
            float itemLoadProgress = 0;
            do
            {
                yield return null;
                itemLoadProgress = AsyncLoadMonitor.GetProgress();
                text.text = "LOADING ITEMS : " + (int)(itemLoadProgress * 100) + "%";
            }
            while (itemLoadProgress <= 1);


            //Now that everything is loaded, we can perform magazine caching
            AnvilManager.Run(LoadMagazineCacheAsync(path, text));
            while (!MagazineCacheLoaded) yield return null;

            //Now perform final steps of loading characters
            LoadTNHTemplates(CharDatabase);

            if (outputFiles)
            {
                CreateTNHFiles(path);
            }

            RefreshTNHUI(instance, Categories, CharDatabase);

            hotdog.gameObject.SetActive(true);
            TNHInitialized = true;
        }


        public static void LoadTNHTemplates(TNH_CharacterDatabase CharDatabase)
        {
            TNHTweakerLogger.Log("TNHTweaker -- Performing TNH Initialization", TNHTweakerLogger.LogType.General);

            //Load all of the default templates into our dictionaries
            TNHTweakerLogger.Log("TNHTweaker -- Adding default sosigs to template dictionary", TNHTweakerLogger.LogType.General);
            LoadDefaultSosigs();
            TNHTweakerLogger.Log("TNHTweaker -- Adding default characters to template dictionary", TNHTweakerLogger.LogType.General);
            LoadDefaultCharacters(CharDatabase.Characters);

            LoadedTemplateManager.DefaultIconSprites = TNHTweakerUtils.GetAllIcons(LoadedTemplateManager.DefaultCharacters);

            TNHTweakerLogger.Log("TNHTweaker -- Delayed Init of default characters", TNHTweakerLogger.LogType.General);
            InitCharacters(LoadedTemplateManager.DefaultCharacters, false);

            TNHTweakerLogger.Log("TNHTweaker -- Delayed Init of custom characters", TNHTweakerLogger.LogType.General);
            InitCharacters(LoadedTemplateManager.CustomCharacters, true);

            TNHTweakerLogger.Log("TNHTweaker -- Delayed Init of custom sosigs", TNHTweakerLogger.LogType.General);
            InitSosigs(LoadedTemplateManager.CustomSosigs);
        }



        public static void CreateTNHFiles(string path)
        {
            //Create files relevant for character creation
            TNHTweakerLogger.Log("TNHTweaker -- Creating character creation files", TNHTweakerLogger.LogType.General);
            TNHTweakerUtils.CreateDefaultSosigTemplateFiles(LoadedTemplateManager.DefaultSosigs, path);
            TNHTweakerUtils.CreateDefaultCharacterFiles(LoadedTemplateManager.DefaultCharacters, path);
            TNHTweakerUtils.CreateIconIDFile(path, LoadedTemplateManager.DefaultIconSprites.Keys.ToList());
            TNHTweakerUtils.CreateObjectIDFile(path);
            TNHTweakerUtils.CreateSosigIDFile(path);
            TNHTweakerUtils.CreateJsonVaultFiles(path);
            TNHTweakerUtils.CreateGeneratedTables(path);
            TNHTweakerUtils.CreatePopulatedCharacterTemplate(path);
        }



        /// <summary>
        /// Loads all default sosigs into the template manager
        /// </summary>
        private static void LoadDefaultSosigs()
        {
            foreach (SosigEnemyTemplate sosig in ManagerSingleton<IM>.Instance.odicSosigObjsByID.Values)
            {
                LoadedTemplateManager.AddSosigTemplate(sosig);
            }
        }

        /// <summary>
        /// Loads all default characters into the template manager
        /// </summary>
        /// <param name="characters">A list of TNH characters</param>
        private static void LoadDefaultCharacters(List<TNH_CharacterDef> characters)
        {
            foreach (TNH_CharacterDef character in characters)
            {
                LoadedTemplateManager.AddCharacterTemplate(character);
            }
        }

        /// <summary>
        /// Performs a delayed init on the sent list of custom characters, and removes any characters that failed to init
        /// </summary>
        /// <param name="characters"></param>
        /// <param name="isCustom"></param>
        private static void InitCharacters(List<CustomCharacter> characters, bool isCustom)
        {
            for (int i = 0; i < characters.Count; i++)
            {
                CustomCharacter character = characters[i];

                try
                {
                    character.DelayedInit(isCustom);
                }
                catch (Exception e)
                {
                    TNHTweakerLogger.LogError("TNHTweaker -- Failed to load character: " + character.DisplayName + ". Error Output:\n" + e.ToString());
                    characters.RemoveAt(i);
                    LoadedTemplateManager.LoadedCharactersDict.Remove(character.GetCharacter());
                    i -= 1;
                }
            }
        }

        /// <summary>
        /// Performs a delayed init on the sent list of sosigs. If a sosig fails to init, any character using that sosig will be removed
        /// </summary>
        /// <param name="sosigs"></param>
        private static void InitSosigs(List<SosigTemplate> sosigs)
        {
            for (int i = 0; i < sosigs.Count; i++)
            {
                SosigTemplate sosig = sosigs[i];

                try
                {
                    sosig.DelayedInit();
                }
                catch (Exception e)
                {
                    TNHTweakerLogger.LogError("TNHTweaker -- Failed to load sosig: " + sosig.DisplayName + ". Error Output:\n" + e.ToString());

                    //Find any characters that use this sosig, and remove them
                    for (int j = 0; j < LoadedTemplateManager.LoadedCharactersDict.Values.Count; j++)
                    {
                        //This is probably monsterously inefficient, but if you're at this point you're already fucked :)
                        KeyValuePair<TNH_CharacterDef, CustomCharacter> value_pair = LoadedTemplateManager.LoadedCharactersDict.ToList()[j];

                        if (value_pair.Value.CharacterUsesSosig(sosig.SosigEnemyID))
                        {
                            TNHTweakerLogger.LogError("TNHTweaker -- Removing character that used removed sosig: " + value_pair.Value.DisplayName);
                            LoadedTemplateManager.LoadedCharactersDict.Remove(value_pair.Key);
                            j -= 1;
                        }
                    }
                }
            }
        }


        public static void RefreshTNHUI(TNH_UIManager instance, List<TNH_UIManager.CharacterCategory> Categories, TNH_CharacterDatabase CharDatabase)
        {
            TNHTweakerLogger.Log("TNHTweaker -- Refreshing TNH UI", TNHTweakerLogger.LogType.General);

            //Load all characters into the UI
            foreach (TNH_CharacterDef character in LoadedTemplateManager.LoadedCharactersDict.Keys)
            {
                if (!Categories[(int)character.Group].Characters.Contains(character.CharacterID))
                {
                    Categories[(int)character.Group].Characters.Add(character.CharacterID);
                    CharDatabase.Characters.Add(character);
                }
            }

            //Update the UI
            Traverse instanceTraverse = Traverse.Create(instance);
            int selectedCategory = (int)instanceTraverse.Field("m_selectedCategory").GetValue();
            int selectedCharacter = (int)instanceTraverse.Field("m_selectedCharacter").GetValue();

            instanceTraverse.Method("SetSelectedCategory", selectedCategory).GetValue();
            instance.OBS_CharCategory.SetSelectedButton(selectedCharacter);
        }



        public static IEnumerator LoadMagazineCacheAsync(string path, Text text)
        {
            CompatibleMagazineCache magazineCache = null;
            MagazineCacheLoaded = false;

            string cachePath = path + "/CachedCompatibleMags.json";
            List<string> blacklist = TNHTweakerUtils.GetMagazineCacheBlacklist(path);

            text.text = "BUILDING CACHE : 0%";
            
            //If the cache exists, we load it and check it's validity
            if (File.Exists(cachePath))
            {
                try
                {
                    string cacheJson = File.ReadAllText(cachePath);
                    magazineCache = JsonConvert.DeserializeObject<CompatibleMagazineCache>(cacheJson);

                    if (!IsMagazineCacheValid(magazineCache, blacklist))
                    {
                        TNHTweakerLogger.Log("TNHTweaker -- Existing magazine cache was not valid", TNHTweakerLogger.LogType.General);
                        File.Delete(cachePath);
                        magazineCache = null;
                    }
                }
                catch
                {
                    magazineCache = null;
                }
            }


            //If the magazine cache file didn't exist, or wasn't valid, we must build a new one
            if (magazineCache == null)
            {
                TNHTweakerLogger.Log("TNHTweaker -- Building new magazine cache -- This may take a while!", TNHTweakerLogger.LogType.General);
                magazineCache = new CompatibleMagazineCache();

                //Load all of the magazines into the cache
                List<FVRObject> magazines = ManagerSingleton<IM>.Instance.odicTagCategory[FVRObject.ObjectCategory.Magazine];
                List<FVRObject> clips = ManagerSingleton<IM>.Instance.odicTagCategory[FVRObject.ObjectCategory.Clip];
                List<FVRObject> bullets = ManagerSingleton<IM>.Instance.odicTagCategory[FVRObject.ObjectCategory.Cartridge];
                List<FVRObject> firearms = ManagerSingleton<IM>.Instance.odicTagCategory[FVRObject.ObjectCategory.Firearm];
                AnvilCallback<GameObject> gameObjectCallback;
                int totalObjects = magazines.Count + clips.Count + bullets.Count + firearms.Count;
                int progress = 0;
                DateTime start = DateTime.Now;

                

                //Loop through all magazines and build a list of magazine components
                TNHTweakerLogger.Log("TNHTweaker -- Loading all magazines", TNHTweakerLogger.LogType.General);
                for (int i = 0; i < magazines.Count; i++)
                {
                    if ((DateTime.Now - start).TotalSeconds > 2)
                    {
                        start = DateTime.Now;
                        TNHTweakerLogger.Log("-- " + ((int)(((float)progress) / totalObjects * 100)) + "% --", TNHTweakerLogger.LogType.General);
                        text.text = "BUILDING CACHE : " + ((int)(((float)progress) / totalObjects * 100)) + "%";
                    }
                    progress += 1;

                    FVRObject magazine = magazines[i];
                    if (!magazineCache.Magazines.Contains(magazine.ItemID))
                    {
                        gameObjectCallback = magazine.GetGameObjectAsync();
                        yield return gameObjectCallback;
                        FVRFireArmMagazine magComp = gameObjectCallback.Result.GetComponent<FVRFireArmMagazine>();

                        magazineCache.Magazines.Add(magazine.ItemID);

                        if (magComp != null)
                        {
                            magazineCache.MagazineObjects.Add(magComp);
                            magazineCache.AddMagazineData(magComp);
                        }

                    }
                }


                GC.Collect();
                GC.WaitForPendingFinalizers();


                //Loop through all clips and build a list of stripper clip components
                TNHTweakerLogger.Log("TNHTweaker -- Loading all clips", TNHTweakerLogger.LogType.General);
                for (int i = 0; i < clips.Count; i++)
                {
                    if ((DateTime.Now - start).TotalSeconds > 2)
                    {
                        start = DateTime.Now;
                        TNHTweakerLogger.Log("-- " + ((int)(((float)progress) / totalObjects * 100)) + "% --", TNHTweakerLogger.LogType.General);
                        text.text = "BUILDING CACHE : " + ((int)(((float)progress) / totalObjects * 100)) + "%";
                    }
                    progress += 1;

                    FVRObject clip = clips[i];
                    if (!magazineCache.Clips.Contains(clip.ItemID))
                    {
                        gameObjectCallback = clip.GetGameObjectAsync();
                        yield return gameObjectCallback;
                        FVRFireArmClip clipComp = gameObjectCallback.Result.GetComponent<FVRFireArmClip>();

                        magazineCache.Clips.Add(clip.ItemID);

                        if (clipComp != null)
                        {
                            magazineCache.ClipObjects.Add(clipComp);
                            magazineCache.AddClipData(clipComp);
                        }

                    }
                }


                GC.Collect();
                GC.WaitForPendingFinalizers();

                //Loop through all clips and build a list of stripper clip components
                TNHTweakerLogger.Log("TNHTweaker -- Loading all bullets", TNHTweakerLogger.LogType.General);
                for (int i = 0; i < bullets.Count; i++)
                {
                    if ((DateTime.Now - start).TotalSeconds > 2)
                    {
                        start = DateTime.Now;
                        TNHTweakerLogger.Log("-- " + ((int)(((float)progress) / totalObjects * 100)) + "% --", TNHTweakerLogger.LogType.General);
                        text.text = "BUILDING CACHE : " + ((int)(((float)progress) / totalObjects * 100)) + "%";
                    }
                    progress += 1;

                    FVRObject bullet = bullets[i];
                    if (!magazineCache.Bullets.Contains(bullet.ItemID))
                    {
                        gameObjectCallback = bullet.GetGameObjectAsync();
                        yield return gameObjectCallback;
                        FVRFireArmRound bulletComp = gameObjectCallback.Result.GetComponent<FVRFireArmRound>();

                        magazineCache.Bullets.Add(bullet.ItemID);

                        if (bulletComp != null)
                        {
                            magazineCache.BulletObjects.Add(bulletComp);
                            magazineCache.AddBulletData(bulletComp);
                        }

                    }
                }



                GC.Collect();
                GC.WaitForPendingFinalizers();

                //Load all firearms into the cache
                TNHTweakerLogger.Log("TNHTweaker -- Applying cache to firearms", TNHTweakerLogger.LogType.General);
                for (int i = 0; i < firearms.Count; i++)
                {
                    if ((DateTime.Now - start).TotalSeconds > 2)
                    {
                        start = DateTime.Now;
                        TNHTweakerLogger.Log("-- " + ((int)(((float)progress) / totalObjects * 100)) + "% --", TNHTweakerLogger.LogType.General);
                        text.text = "BUILDING CACHE : " + ((int)(((float)progress) / totalObjects * 100)) + "%";
                    }
                    progress += 1;

                    //First we should try and get the component of the firearm
                    FVRObject firearm = firearms[i];

                    gameObjectCallback = firearm.GetGameObjectAsync();
                    yield return gameObjectCallback;
                    FVRFireArm firearmComp = gameObjectCallback.Result.GetComponent<FVRFireArm>();

                    magazineCache.Firearms.Add(firearm.ItemID);

                    if (firearmComp == null) continue;

                    //If this firearm is valid, then we create a magazine cache entry for it
                    MagazineCacheEntry entry = new MagazineCacheEntry();
                    magazineCache.Entries.Add(entry);
                    entry.FirearmID = firearm.ItemID;
                    entry.MaxAmmo = firearm.MaxCapacityRelated;
                    entry.MinAmmo = firearm.MinCapacityRelated;

                    if (magazineCache.MagazineData.ContainsKey(firearmComp.MagazineType))
                    {
                        foreach (AmmoObjectDataTemplate magazine in magazineCache.MagazineData[firearmComp.MagazineType])
                        {
                            //If the firearm isn't in the blacklist, and has a new magazine that isn't compatible yet, then we make it compatible
                            if (!TNHTweakerUtils.FVRObjectListContainsID(firearm.CompatibleMagazines, magazine.ObjectID) && !blacklist.Contains(firearm.ItemID))
                            {
                                if (firearm.MaxCapacityRelated < magazine.Capacity)
                                {
                                    firearm.MaxCapacityRelated = magazine.Capacity;
                                }
                                if (firearm.MinCapacityRelated > magazine.Capacity)
                                {
                                    firearm.MinCapacityRelated = magazine.Capacity;
                                }

                                firearm.CompatibleMagazines.Add(magazine.AmmoObject);
                            }

                            entry.MaxAmmo = firearm.MaxCapacityRelated;
                            entry.MinAmmo = firearm.MinCapacityRelated;
                            entry.CompatibleMagazines.Add(magazine.ObjectID);
                        }
                    }

                    if (magazineCache.ClipData.ContainsKey(firearmComp.ClipType))
                    {
                        foreach (AmmoObjectDataTemplate clip in magazineCache.ClipData[firearmComp.ClipType])
                        {
                            if (!TNHTweakerUtils.FVRObjectListContainsID(firearm.CompatibleClips, clip.ObjectID))
                            {
                                //Clips don't modify capacity of guns, so we don't modify capacity related on the firearm object
                                firearm.CompatibleClips.Add(clip.AmmoObject);
                            }

                            entry.MaxAmmo = firearm.MaxCapacityRelated;
                            entry.MinAmmo = firearm.MinCapacityRelated;
                            entry.CompatibleClips.Add(clip.ObjectID);
                        }
                    }

                    if (magazineCache.BulletData.ContainsKey(firearmComp.RoundType))
                    {
                        foreach (AmmoObjectDataTemplate bullet in magazineCache.BulletData[firearmComp.RoundType])
                        {
                            if (!TNHTweakerUtils.FVRObjectListContainsID(firearm.CompatibleSingleRounds, bullet.ObjectID))
                            {
                                //Bullets don't modify capacity of guns, so we don't modify capacity related on the firearm object
                                firearm.CompatibleSingleRounds.Add(bullet.AmmoObject);
                            }

                            entry.MaxAmmo = firearm.MaxCapacityRelated;
                            entry.MinAmmo = firearm.MinCapacityRelated;
                            entry.CompatibleBullets.Add(bullet.ObjectID);
                        }
                    }
                }


                LoadedTemplateManager.AddMagazineData(magazineCache);

                //Create the cache file 
                using (StreamWriter sw = File.CreateText(cachePath))
                {
                    string cacheString = JsonConvert.SerializeObject(magazineCache, Formatting.Indented, new StringEnumConverter());
                    sw.WriteLine(cacheString);
                    sw.Close();
                }


                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            //If the cache is valid, we can just load each entry from the cache
            else
            {
                TNHTweakerLogger.Log("TNHTweaker -- Loading existing magazine cache", TNHTweakerLogger.LogType.General);

                //Apply the magazine cache values to every firearm that is loaded
                foreach (MagazineCacheEntry entry in magazineCache.Entries)
                {
                    if (blacklist.Contains(entry.FirearmID)) continue;

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
                        foreach (string clip in entry.CompatibleClips)
                        {
                            if (IM.OD.ContainsKey(clip))
                            {
                                firearm.CompatibleClips.Add(IM.OD[clip]);
                            }
                        }
                        foreach (string bullet in entry.CompatibleBullets)
                        {
                            if (IM.OD.ContainsKey(bullet))
                            {
                                firearm.CompatibleSingleRounds.Add(IM.OD[bullet]);
                            }
                        }
                        firearm.MaxCapacityRelated = entry.MaxAmmo;
                        firearm.MinCapacityRelated = entry.MinAmmo;
                    }
                }

                LoadedTemplateManager.AddMagazineDataFromLoad(magazineCache);
            }

            text.text = "CACHE BUILT";
            MagazineCacheLoaded = true;
        }




        /// <summary>
        /// Returns true if every gun and magazine is found within the cache
        /// </summary>
        /// <param name="magazineCache"></param>
        /// <returns></returns>
        public static bool IsMagazineCacheValid(CompatibleMagazineCache magazineCache, List<string> blacklist)
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
                    TNHTweakerLogger.LogWarning("TNHTweaker -- Magazine not found in cache: " + mag);
                    cacheValid = false;
                }
            }
            foreach (string firearm in ManagerSingleton<IM>.Instance.odicTagCategory[FVRObject.ObjectCategory.Firearm].Select(f => f.ItemID))
            {
                if (!magazineCache.Firearms.Contains(firearm) && !blacklist.Contains(firearm))
                {
                    TNHTweakerLogger.LogWarning("TNHTweaker -- Firearm not found in cache: " + firearm);
                    cacheValid = false;
                }
            }

            return cacheValid;
        }

    }
}
