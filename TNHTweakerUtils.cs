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
