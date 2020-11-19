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


        public static Dictionary<string, Sprite> GetAllIcons(TNH_CharacterDatabase database)
        {
            Dictionary<string, Sprite> icons = new Dictionary<string, Sprite>();

            foreach(TNH_CharacterDef character in database.Characters)
            {
                foreach(EquipmentPoolDef.PoolEntry pool in character.EquipmentPool.Entries)
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

        public static void CreateDefaultCharacterFiles(TNH_CharacterDatabase database, string path)
        {

            try
            {
                path = path + "/DefaultCharacters";

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                
                foreach (TNH_CharacterDef charDef in database.Characters)
                {
                    if (File.Exists(path + "/" + charDef.DisplayName + ".json"))
                    {
                        File.Delete(path + "/" + charDef.DisplayName + ".json");
                    }

                    // Create a new file     
                    using (StreamWriter sw = File.CreateText(path + "/" + charDef.DisplayName + ".json"))
                    {
                        TNHTweakerLogger.Log("TNHTWEAKER -- CREATING CHARACTER OBJECT", TNHTweakerLogger.LogType.File);
                        CustomCharacter character = new CustomCharacter(charDef);
                        TNHTweakerLogger.Log("TNHTWEAKER -- SERIALIZING", TNHTweakerLogger.LogType.File);
                        string characterString = JsonConvert.SerializeObject(character, Formatting.Indented, new StringEnumConverter());
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

        public static void CreateDefaultSosigTemplateFiles(string path)
        {
            try
            {
                path = path + "/DefaultSosigTemplates";

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                foreach (SosigEnemyTemplate template in ManagerSingleton<IM>.Instance.odicSosigObjsByID.Values)
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

        public static void RemoveUnloadedObjectIDs(TNH_CharacterDef character)
        {
            if (character.Has_Weapon_Primary)
            {
                foreach (ObjectTableDef table in character.Weapon_Primary.TableDefs)
                {
                    RemoveUnloadedObjectIDs(table);
                }
            }

            if (character.Has_Weapon_Secondary)
            {
                foreach (ObjectTableDef table in character.Weapon_Secondary.TableDefs)
                {
                    RemoveUnloadedObjectIDs(table);
                }
            }

            if (character.Has_Weapon_Tertiary)
            {
                foreach (ObjectTableDef table in character.Weapon_Tertiary.TableDefs)
                {
                    RemoveUnloadedObjectIDs(table);
                }
            }

            if (character.Has_Item_Primary)
            {
                foreach (ObjectTableDef table in character.Item_Primary.TableDefs)
                {
                    RemoveUnloadedObjectIDs(table);
                }
            }

            if (character.Has_Item_Secondary)
            {
                foreach (ObjectTableDef table in character.Item_Secondary.TableDefs)
                {
                    RemoveUnloadedObjectIDs(table);
                }
            }

            if (character.Has_Item_Tertiary)
            {
                foreach (ObjectTableDef table in character.Item_Tertiary.TableDefs)
                {
                    RemoveUnloadedObjectIDs(table);
                }
            }

            if (character.Has_Item_Shield)
            {
                foreach (ObjectTableDef table in character.Item_Shield.TableDefs)
                {
                    RemoveUnloadedObjectIDs(table);
                }
            }

            foreach (EquipmentPoolDef.PoolEntry pool in character.EquipmentPool.Entries)
            {
                RemoveUnloadedObjectIDs(pool.TableDef);
            }
        }

        public static void RemoveUnloadedObjectIDs(ObjectTableDef table)
        {
            if (table.UseIDListOverride)
            {
                for (int i = 0; i < table.IDOverride.Count; i++)
                {
                    if (!IM.OD.ContainsKey(table.IDOverride[i]))
                    {
                        table.IDOverride.RemoveAt(i);
                        i--;
                    }
                }
            }
        }


        public static string GetIndent(int count)
        {
            string indent = "";

            for (int i = 0; i < count; i++)
            {
                indent = string.Concat(indent, "- ");
            }

            return indent;
        }

        public static string GetTagFromLine(string line)
        {
            if (line.Contains("="))
            {
                return line.Split('=')[0];
            }

            return "";
        }

        public static string GetStringFromLine(string line)
        {
            if (line.Contains("="))
            {
                return line.Split('=')[1];
            }

            return "";
        }

        public static int GetIntFromLine(string line)
        {
            int result = -1;

            if (line.Contains("="))
            {
                if (!int.TryParse(line.Split('=')[1], out result))
                {
                    return -1;
                }
            }

            return result;
        }

        public static float GetFloatFromLine(string line)
        {
            float result = -1;

            if (line.Contains("="))
            {
                if (!float.TryParse(line.Split('=')[1], out result))
                {
                    return -1;
                }
            }

            return result;
        }

        public static bool GetBoolFromLine(string line)
        {
            bool result = false;

            if (line.Contains("="))
            {
                bool.TryParse(line.Split('=')[1], out result);
            }

            return result;
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



        public static void BuildCompatibleMagazineCache(string path)
        {
            TNHTweakerLogger.Log("TNHTWEAKER -- BUILDING COMPATIBLE MAGAZINE CACHE", TNHTweakerLogger.LogType.File);

            StreamWriter writer = File.CreateText(path);

            List<FVRFireArmMagazine> magazines = new List<FVRFireArmMagazine>();
            List<string> magIDs = new List<string>();
            List<string> firearmIDs = new List<string>();
            List<string> firearmCachedValues = new List<string>();

            foreach (FVRObject magazine in ManagerSingleton<IM>.Instance.odicTagCategory[FVRObject.ObjectCategory.Magazine])
            {
                FVRFireArmMagazine magComp = magazine.GetGameObject().GetComponent<FVRFireArmMagazine>();

                magIDs.Add(magazine.ItemID);

                if (magComp != null)
                {
                    magazines.Add(magComp);
                }
            }

            foreach (FVRObject firearm in ManagerSingleton<IM>.Instance.odicTagCategory[FVRObject.ObjectCategory.Firearm])
            {

                //Debug.Log("Firearm with magazines: " + firearm.ItemID);
                //foreach (FVRObject mag in firearm.CompatibleMagazines)
                //{
                //    Debug.Log(mag.ItemID);
                //}

                FVRFireArm firearmComp = firearm.GetGameObject().GetComponent<FVRFireArm>();

                firearmIDs.Add(firearm.ItemID);

                if (firearmComp == null) continue;

                bool addedMagazine = false;

                foreach (FVRFireArmMagazine magazine in magazines)
                {
                    if (firearmComp.MagazineType == magazine.MagazineType && !TNHTweakerUtils.ListContainsObjectID(firearm.CompatibleMagazines, magazine.ObjectWrapper.ItemID))
                    {
                        //Debug.Log("New magazine found: " + magazine.name);
                        firearm.CompatibleMagazines.Add(magazine.ObjectWrapper);

                        if (firearm.MaxCapacityRelated < magazine.m_capacity)
                        {
                            firearm.MaxCapacityRelated = magazine.m_capacity;
                        }

                        if (firearm.MinCapacityRelated > magazine.m_capacity)
                        {
                            firearm.MinCapacityRelated = magazine.m_capacity;
                        }

                        addedMagazine = true;

                        TNHTweakerLogger.Log("TNHTWEAKER -- ADDED COMPATIBLE MAGAZINE (" + magazine.ObjectWrapper.ItemID + ") TO FIREARM (" + firearm.ItemID + ")", TNHTweakerLogger.LogType.File);
                    }
                }

                if (addedMagazine)
                {
                    firearmCachedValues.Add(firearm.ItemID + "=" + firearm.MinCapacityRelated + "," + firearm.MaxCapacityRelated + "," + string.Join(",", firearm.CompatibleMagazines.Select(f => f.ItemID).ToArray()));
                }
            }

            writer.WriteLine("magazines=" + string.Join(",", magIDs.ToArray()));
            writer.WriteLine("firearms=" + string.Join(",", firearmIDs.ToArray()));
            foreach (string entry in firearmCachedValues)
            {
                writer.WriteLine(entry);
            }

            writer.Close();
        }


        public static void LoadCompatibleMagazines(string path)
        {
            try
            {
                if (!File.Exists(path + "/CachedCompatibleMags.txt"))
                {
                    BuildCompatibleMagazineCache(path + "/CachedCompatibleMags.txt");
                    return;
                }

                string[] lines = File.ReadAllLines(path + "/CachedCompatibleMags.txt");

                List<string> magList = new List<string>();
                List<string> firearmList = new List<string>();
                List<string> cachedValues = new List<string>();

                foreach (string line in lines)
                {
                    if (GetTagFromLine(line).Equals("magazines"))
                    {
                        magList.AddRange(GetStringFromLine(line).Split(','));
                    }

                    else if (GetTagFromLine(line).Equals("firearms"))
                    {
                        firearmList.AddRange(GetStringFromLine(line).Split(','));
                    }

                    else if (line.Contains("="))
                    {
                        cachedValues.Add(line);
                    }
                }

                bool magsSame = magList.Count == ManagerSingleton<IM>.Instance.odicTagCategory[FVRObject.ObjectCategory.Magazine].Count && magList.All(ManagerSingleton<IM>.Instance.odicTagCategory[FVRObject.ObjectCategory.Magazine].Select(f => f.ItemID).Contains);
                bool firearmsSame = firearmList.Count == ManagerSingleton<IM>.Instance.odicTagCategory[FVRObject.ObjectCategory.Firearm].Count && firearmList.All(ManagerSingleton<IM>.Instance.odicTagCategory[FVRObject.ObjectCategory.Firearm].Select(f => f.ItemID).Contains);

                //If the cached items are the same as items loaded in game, we can just rely on the cached values for each firearm
                if (magsSame && firearmsSame)
                {
                    TNHTweakerLogger.Log("TNHTWEAKER -- LOADING COMPATIBLE MAGAZINES FROM CACHE", TNHTweakerLogger.LogType.File);

                    FVRObject obj;

                    foreach (string entry in cachedValues)
                    {
                        List<string> properties = new List<string>(GetStringFromLine(entry).Split(','));
                        obj = IM.OD[GetTagFromLine(entry)];
                        obj.MinCapacityRelated = int.Parse(properties[0]);
                        obj.MaxCapacityRelated = int.Parse(properties[1]);

                        for (int i = 2; i < properties.Count; i++)
                        {
                            if (!TNHTweakerUtils.ListContainsObjectID(obj.CompatibleMagazines, properties[i]))
                            {
                                TNHTweakerLogger.Log("TNHTWEAKER -- ADDED COMPATIBLE MAGAZINE (" + properties[i] + ") TO FIREARM (" + obj.ItemID + ")", TNHTweakerLogger.LogType.File);
                                obj.CompatibleMagazines.Add(IM.OD[properties[i]]);
                            }
                        }
                    }
                }

                else
                {
                    TNHTweakerLogger.Log("TNHTWEAKER -- CACHE OUT OF DATE!", TNHTweakerLogger.LogType.File);
                    TNHTweakerLogger.Log("Magazine Change? " + !magsSame, TNHTweakerLogger.LogType.File);
                    TNHTweakerLogger.Log("Firearm Change? " + !firearmsSame, TNHTweakerLogger.LogType.File);
                    TNHTweakerLogger.Log("Magazine List sizes: " + magList.Count + ", " + ManagerSingleton<IM>.Instance.odicTagCategory[FVRObject.ObjectCategory.Magazine].Count, TNHTweakerLogger.LogType.File);
                    TNHTweakerLogger.Log("Firearm List sizes: " + firearmList.Count + ", " + ManagerSingleton<IM>.Instance.odicTagCategory[FVRObject.ObjectCategory.Firearm].Count, TNHTweakerLogger.LogType.File);

                    File.Delete(path + "/CachedCompatibleMags.txt");
                    BuildCompatibleMagazineCache(path + "/CachedCompatibleMags.txt");
                    return;
                }


            }

            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }
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
            
            Debug.Log("Getting vector: " + v);
            Debug.Log("Initial X: " + x);
            Debug.Log("Initial Y: " + y);

            return v;
        }
    }




}
