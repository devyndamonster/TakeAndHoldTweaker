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

namespace FistVR
{

    public class ObjectBuilder
    {
        public static TNH_CharacterDef GetCharacterFromString(string charPath, Dictionary<TNH_CharacterDef, CustomCharData> customCharDict, Dictionary<string, Sprite> icons, TNH_CharacterDef backupCharacter)
        {
            TNH_CharacterDef character = (TNH_CharacterDef)ScriptableObject.CreateInstance(typeof(TNH_CharacterDef));
            customCharDict.Add(character, new CustomCharData());

            //Get initial data from character directory
            string imagePath = Directory.GetFiles(charPath, "thumb.png")[0];
            string dataPath = Directory.GetFiles(charPath, "character.txt")[0];
            Sprite charSprite = LoadSprite(imagePath);
            string[] characterData = File.ReadAllLines(dataPath);

            //First, assign default values for this character for the values that we will be loading from a file
            character.DisplayName = "NAME NOT FOUND";
            character.Group = TNH_CharacterDef.CharacterGroup.MemetasticMeats;
            character.StartingTokens = 3;
            character.ForceAllAgentWeapons = false;
            character.Description = "DESCRIPTION NOT FOUND";
            character.RequireSightTable = backupCharacter.RequireSightTable;
            character.ValidAmmoEras = backupCharacter.ValidAmmoEras;
            character.ValidAmmoSets = backupCharacter.ValidAmmoSets;
            character.Progressions_Endless = backupCharacter.Progressions_Endless;
            character.Has_Weapon_Primary = backupCharacter.Has_Weapon_Primary;
            character.Has_Weapon_Secondary = backupCharacter.Has_Weapon_Secondary;
            character.Has_Weapon_Tertiary = backupCharacter.Has_Weapon_Tertiary;
            character.Has_Item_Shield = backupCharacter.Has_Item_Shield;
            character.Weapon_Primary = backupCharacter.Weapon_Primary;
            character.Weapon_Secondary = backupCharacter.Weapon_Secondary;
            character.Weapon_Tertiary = backupCharacter.Weapon_Tertiary;
            character.Item_Primary = backupCharacter.Item_Primary;
            character.Item_Secondary = backupCharacter.Item_Secondary;
            character.Item_Tertiary = backupCharacter.Item_Tertiary;
            character.Item_Shield = backupCharacter.Item_Shield;
            character.CharacterID = (TNH_Char)(30 + customCharDict.Count);
            character.TableID = string.Concat("CUSTOM_", (1 + customCharDict.Count));
            character.Picture = charSprite;

            Traverse currentObject = Traverse.Create(character);

            Stack<Traverse> traverseStack = new Stack<Traverse>();
            traverseStack.Push(currentObject);

            //Loop through each line of the file
            for (int i = 0; i < characterData.Length; i++)
            {
                string line = characterData[i].TrimStart();

                //Handle cases where we skip a line
                if (line.Length == 0 || line.StartsWith("#")) continue;

                TNHTweakerLogger.Log("" + GetIndent(traverseStack.Count) + "READING LINE: " + line, TNHTweakerLogger.LogType.Character);

                if (line.Contains("[]"))
                {
                    string fieldName = line.Replace("[]", "");

                    //If the field exists, we want to instantiate the list, and do nothing else with it
                    if (traverseStack.Peek().Field(fieldName).FieldExists())
                    {
                        Type fieldType = traverseStack.Peek().Field(fieldName).GetValueType();

                        if (IsGenericList(fieldType))
                        {
                            Type propertyType = fieldType.GetGenericArguments()[0];
                            traverseStack.Peek().Field(fieldName).SetValue(CreateGenericList(propertyType));

                            TNHTweakerLogger.Log("" + GetIndent(traverseStack.Count) + "CREATING EMPTY LIST FOR FIELD (" + fieldName + ")", TNHTweakerLogger.LogType.Character);
                        }

                        else
                        {
                            Debug.LogError("" + GetIndent(traverseStack.Count) + "EMPTY LIST FOUND IN FILE, BUT FIELD IS NOT ACTUALLY A LIST (" + fieldType.FullName + ") -- SKIPPING FIELD");
                            continue;
                        }
                    }
                }

                //Entering an object
                else if (line.Contains("{"))
                {
                    string fieldName = line.Replace("{", "");
                    Type fieldType = null;
                    Traverse newElement;

                    //If the previous item in the stack is a list, add this object to that list
                    if (IsGenericList(traverseStack.Peek().GetValue().GetType()))
                    {
                        fieldType = traverseStack.Peek().GetValue().GetType().GetGenericArguments()[0];
                    }

                    //If the previous object in the stack is an object, check if this is a field of the object
                    else if (traverseStack.Peek().Field(fieldName).FieldExists())
                    {
                        //Add the new object to the stack, and get its type
                        fieldType = traverseStack.Peek().Field(fieldName).GetValueType();
                    }

                    else
                    {
                        Debug.LogError("" + GetIndent(traverseStack.Count) + "OBJECT FIELD DOES NOT EXIST (" + fieldName + ") -- STOPPING FILE READ");
                        break;
                    }

                    if (fieldType == typeof(EquipmentPoolDef))
                    {
                        newElement = Traverse.Create((EquipmentPoolDef)ScriptableObject.CreateInstance(typeof(EquipmentPoolDef)));
                    }

                    else if (fieldType == typeof(EquipmentPoolDef.PoolEntry))
                    {
                        newElement = Traverse.Create(new EquipmentPoolDef.PoolEntry());
                    }

                    else if (fieldType == typeof(TNH_CharacterDef.LoadoutEntry))
                    {
                        newElement = Traverse.Create(new TNH_CharacterDef.LoadoutEntry());
                    }

                    else if (fieldType == typeof(ObjectTableDef))
                    {
                        newElement = Traverse.Create((ObjectTableDef)ScriptableObject.CreateInstance(typeof(ObjectTableDef)));
                    }

                    else if (fieldType == typeof(TNH_Progression))
                    {
                        newElement = Traverse.Create((TNH_Progression)ScriptableObject.CreateInstance(typeof(TNH_Progression)));
                    }

                    else if (fieldType == typeof(TNH_Progression.Level))
                    {
                        newElement = Traverse.Create(new TNH_Progression.Level());
                        customCharDict[character].Levels.Add(new CustomTNHLevel());
                    }

                    else if (fieldType == typeof(TNH_TakeChallenge))
                    {
                        newElement = Traverse.Create((TNH_TakeChallenge)ScriptableObject.CreateInstance(typeof(TNH_TakeChallenge)));
                    }

                    else if (fieldType == typeof(TNH_HoldChallenge))
                    {
                        newElement = Traverse.Create((TNH_HoldChallenge)ScriptableObject.CreateInstance(typeof(TNH_HoldChallenge)));
                    }

                    else if (fieldType == typeof(TNH_HoldChallenge.Phase))
                    {
                        newElement = Traverse.Create(new TNH_HoldChallenge.Phase());
                        customCharDict[character].Levels[customCharDict[character].Levels.Count - 1].Phases.Add(new CustomTNHPhase());
                    }

                    else if (fieldType == typeof(TNH_PatrolChallenge))
                    {
                        newElement = Traverse.Create((TNH_PatrolChallenge)ScriptableObject.CreateInstance(typeof(TNH_PatrolChallenge)));
                    }

                    else if (fieldType == typeof(TNH_PatrolChallenge.Patrol))
                    {
                        newElement = Traverse.Create(new TNH_PatrolChallenge.Patrol());
                    }

                    else if (fieldType == typeof(TNH_TrapsChallenge))
                    {
                        newElement = Traverse.Create((TNH_TrapsChallenge)ScriptableObject.CreateInstance(typeof(TNH_TrapsChallenge)));
                    }

                    else if (fieldType == typeof(TNH_TrapsChallenge.TrapOccurance))
                    {
                        newElement = Traverse.Create(new TNH_TrapsChallenge.TrapOccurance());
                    }

                    else
                    {
                        Debug.LogError("" + GetIndent(traverseStack.Count) + "OBJECT TYPE NOT RECOGNIZED IN CHARACTER FILE (" + fieldType.FullName + ") -- STOPPING FILE READ");
                        break;
                    }


                    //If the previous item in the stack is a list, add this object to that list
                    if (IsGenericList(traverseStack.Peek().GetValue().GetType()))
                    {
                        TNHTweakerLogger.Log("" + GetIndent(traverseStack.Count) + "CREATING NEW LIST ELEMENT (" + fieldType.FullName + ")", TNHTweakerLogger.LogType.Character);

                        Traverse currentList = traverseStack.Peek();
                        currentList.Method("Add", newElement.GetValue()).GetValue();
                        traverseStack.Push(newElement);
                    }

                    //If the previous object in the stack is an object, check if this is a field of the object
                    else if (traverseStack.Peek().Field(fieldName).FieldExists())
                    {
                        TNHTweakerLogger.Log("" + GetIndent(traverseStack.Count) + "CREATING NEW OBJECT FIELD (" + fieldName + ")", TNHTweakerLogger.LogType.Character);

                        traverseStack.Peek().Field(fieldName).SetValue(newElement.GetValue());
                        traverseStack.Push(newElement);
                    }

                }



                //Exitting an object
                else if (line.Contains("}"))
                {
                    //If there is more than just the character on the stack, pop that field
                    if (traverseStack.Count > 1)
                    {
                        TNHTweakerLogger.Log("" + GetIndent(traverseStack.Count) + "END OF OBJECT", TNHTweakerLogger.LogType.Character);
                        traverseStack.Pop();
                    }

                    else
                    {
                        Debug.LogWarning("" + GetIndent(traverseStack.Count) + "CLOSING BRACKET FOR OBJECT FOUND, BUT THERE WAS NO OBJECT TO CLOSE");
                    }

                }

                //Handling a list of objects
                else if (line.Contains("["))
                {
                    string fieldName = line.Replace("[", "");

                    //If the field exists, we can add this new object to the stack
                    if (traverseStack.Peek().Field(fieldName).FieldExists())
                    {
                        Type fieldType = traverseStack.Peek().Field(fieldName).GetValueType();

                        if (IsGenericList(fieldType))
                        {
                            //Add the generic list to the stack
                            traverseStack.Push(traverseStack.Peek().Field(fieldName));
                            Type propertyType = fieldType.GetGenericArguments()[0];
                            traverseStack.Peek().SetValue(CreateGenericList(propertyType));

                            TNHTweakerLogger.Log("" + GetIndent(traverseStack.Count) + "CREATING LIST FOR FIELD (" + fieldName + ")", TNHTweakerLogger.LogType.Character);
                        }

                        else
                        {
                            Debug.LogError("" + GetIndent(traverseStack.Count) + "LIST FOUND IN FILE, BUT FIELD IS NOT ACTUALLY A LIST (" + fieldType.FullName + ") -- STOPPING FILE READ");
                            break;
                        }
                    }

                    else
                    {
                        Debug.LogError("" + GetIndent(traverseStack.Count) + "LIST FIELD DOES NOT EXIST (" + fieldName + ") -- STOPPING FILE READ");
                        break;
                    }

                }

                //Exitting list of objects
                else if (line.Contains("]"))
                {
                    //If there is more than just the character on the stack, pop that field
                    if (IsGenericList(traverseStack.Peek().GetValueType()))
                    {
                        TNHTweakerLogger.Log("" + GetIndent(traverseStack.Count) + "END OF LIST", TNHTweakerLogger.LogType.Character);
                        traverseStack.Pop();
                    }

                    else
                    {
                        Debug.LogWarning("" + GetIndent(traverseStack.Count) + "CLOSING BRACKET FOR LIST FOUND, BUT THERE WAS NO LIST TO CLOSE");
                    }
                }

                //Handling a field
                else if (line.Contains("="))
                {
                    try
                    {
                        Traverse currField;
                        if (line.StartsWith("@"))
                        {
                            line = line.TrimStart('@');


                            //If we are inside a phase, redirect this field to the current custom phase
                            if (traverseStack.Peek().GetValue().GetType() == typeof(TNH_HoldChallenge.Phase))
                            {
                                TNHTweakerLogger.Log("" + GetIndent(traverseStack.Count) + "FIELD IS FOR CUSTOM DATA OF HOLD PHASE", TNHTweakerLogger.LogType.Character);
                                int levelIndex = customCharDict[character].Levels.Count - 1;
                                int phaseIndex = customCharDict[character].Levels[levelIndex].Phases.Count - 1;
                                currField = Traverse.Create(customCharDict[character].Levels[levelIndex].Phases[phaseIndex]).Field(GetTagFromLine(line));
                            }


                            //If we are inside level, redirect field to current custom level
                            else if (traverseStack.Peek().GetValue().GetType() == typeof(TNH_Progression.Level))
                            {
                                TNHTweakerLogger.Log("" + GetIndent(traverseStack.Count) + "FIELD IS FOR CUSTOM DATA OF LEVEL", TNHTweakerLogger.LogType.Character);
                                int levelIndex = customCharDict[character].Levels.Count - 1;
                                currField = Traverse.Create(customCharDict[character].Levels[levelIndex]).Field(GetTagFromLine(line));
                            }

                            //Otherwise check if this is a field in the custom character file
                            else if (Traverse.Create(customCharDict[character]).Field(GetTagFromLine(line)).FieldExists())
                            {
                                TNHTweakerLogger.Log("" + GetIndent(traverseStack.Count) + "FIELD IS FOR CUSTOM DATA OF CHARACTER", TNHTweakerLogger.LogType.Character);
                                currField = Traverse.Create(customCharDict[character]).Field(GetTagFromLine(line));
                            }

                            else
                            {
                                Debug.LogWarning("" + GetIndent(traverseStack.Count) + "CUSTOM FIELD IS NOT FOUND - SKIPPING ENTRY");
                                continue;
                            }
                        }

                        else
                        {
                            currField = traverseStack.Peek().Field(GetTagFromLine(line));
                        }
                        Type currType = currField.GetValueType();

                        //If it is a list, then we need to handle it differently
                        if (IsGenericList(currType))
                        {
                            //Get the type of the elements for this list
                            Type propertyType = currType.GetGenericArguments()[0];

                            //Instantiate the list of items
                            currField.SetValue(CreateGenericList(propertyType));

                            TNHTweakerLogger.Log("" + GetIndent(traverseStack.Count) + "FIELD: GENERIC LIST OF ELEMENT TYPE (" + propertyType.FullName + ")", TNHTweakerLogger.LogType.Character);

                            //Go through each element of the list and add it to the field
                            string[] values = GetStringFromLine(line).Split(',');
                            foreach (string value in values)
                            {
                                if (value.Length == 0) continue;

                                //If these are primitives, add them to the list based on there type
                                else if (propertyType == typeof(string))
                                {
                                    currField.Method("Add", value).GetValue();
                                }

                                else if (propertyType == typeof(int))
                                {
                                    currField.Method("Add", int.Parse(value)).GetValue();
                                }

                                else if (propertyType == typeof(float))
                                {
                                    currField.Method("Add", float.Parse(value)).GetValue();
                                }

                                else if (propertyType == typeof(bool))
                                {
                                    currField.Method("Add", bool.Parse(value)).GetValue();
                                }

                                //Otherwise, if these are enums, try to parse it from the string
                                else if (propertyType.IsEnum)
                                {
                                    currField.Method("Add", Enum.Parse(propertyType, value)).GetValue();
                                }

                                else
                                {
                                    Debug.LogError("" + GetIndent(traverseStack.Count) + "ELEMENT TYPE NOT SUPPORTED FOR LIST -- LIST NOT POPULATED");
                                    break;
                                }

                            }
                        }

                        else if (currType == typeof(int))
                        {
                            TNHTweakerLogger.Log("" + GetIndent(traverseStack.Count) + "FIELD: INT", TNHTweakerLogger.LogType.Character);
                            currField.SetValue(GetIntFromLine(line));
                        }

                        else if (currType == typeof(float))
                        {
                            TNHTweakerLogger.Log("" + GetIndent(traverseStack.Count) + "FIELD: FLOAT", TNHTweakerLogger.LogType.Character);
                            currField.SetValue(GetFloatFromLine(line));
                        }

                        else if (currType == typeof(bool))
                        {
                            TNHTweakerLogger.Log("" + GetIndent(traverseStack.Count) + "FIELD: BOOL", TNHTweakerLogger.LogType.Character);
                            currField.SetValue(GetBoolFromLine(line));
                        }

                        else if (currType == typeof(string))
                        {
                            TNHTweakerLogger.Log("" + GetIndent(traverseStack.Count) + "FIELD: STRING", TNHTweakerLogger.LogType.Character);
                            currField.SetValue(GetStringFromLine(line));
                        }

                        //Handle case where it is an enum
                        else if (currType.IsEnum)
                        {
                            //TODO this won't work for enum values that are actually -1
                            if (GetIntFromLine(line) == -1)
                            {
                                TNHTweakerLogger.Log("" + GetIndent(traverseStack.Count) + "FIELD: ENUM, ASSIGNED BY STRING", TNHTweakerLogger.LogType.Character);
                                currField.SetValue(Enum.Parse(currType, GetStringFromLine(line)));
                            }
                            else
                            {
                                TNHTweakerLogger.Log("" + GetIndent(traverseStack.Count) + "FIELD: ENUM, ASSIGNED BY INT", TNHTweakerLogger.LogType.Character);
                                currField.SetValue(Enum.ToObject(currType, GetIntFromLine(line)));
                            }
                        }

                        else if (currType == typeof(Sprite))
                        {
                            string value = GetStringFromLine(line);

                            //If this starts with an @, it is a default icon
                            if (value.StartsWith("@"))
                            {
                                TNHTweakerLogger.Log("" + GetIndent(traverseStack.Count) + "FIELD: ICON, LOADED FROM DEFAULT ICON DICTIONARY", TNHTweakerLogger.LogType.Character);

                                value = value.TrimStart('@');
                                if (icons.ContainsKey(value)){
                                    currField.SetValue(icons[value]);
                                }
                                else
                                {
                                    Debug.LogWarning("" + GetIndent(traverseStack.Count) + "ICON NOT FOUND");
                                }
                            }

                            else
                            {
                                TNHTweakerLogger.Log("" + GetIndent(traverseStack.Count) + "FIELD: ICON, LOADING FROM " + charPath + "/" + value, TNHTweakerLogger.LogType.Character);
                                currField.SetValue(LoadSprite(charPath + "/" + value));
                            }
                        }

                        else
                        {
                            Debug.LogError("" + GetIndent(traverseStack.Count) + "UNSUPPORTED TYPE READ FOR FIELD (" + currType.FullName + ") -- SKIPPING FIELD (" + GetTagFromLine(line) + ")");
                            continue;
                        }

                    }

                    catch (Exception ex)
                    {
                        Debug.LogWarning("" + GetIndent(traverseStack.Count) + "DID NOT PARSE LINE: " + line);
                        Debug.LogWarning("" + GetIndent(traverseStack.Count) + "OUTPUT: " + ex.Message);
                    }
                }

            }

            return character;
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
                    
                    foreach(string entry in cachedValues)
                    {
                        List<string> properties = new List<string>(GetStringFromLine(entry).Split(','));
                        obj = IM.OD[GetTagFromLine(entry)];
                        obj.MinCapacityRelated = int.Parse(properties[0]);
                        obj.MaxCapacityRelated = int.Parse(properties[1]);

                        for (int i = 2; i < properties.Count; i++)
                        {
                            if(!TNHTweakerUtils.ListContainsObjectID(obj.CompatibleMagazines, properties[i]))
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

}