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

namespace FistVR
{

    public class CustomCharData
    {
        public List<int> AdditionalSupplyPoints = new List<int>();
        public List<CustomTNHLevel> Levels = new List<CustomTNHLevel>();
    }

    public class CustomTNHLevel
    {
        public List<CustomTNHPhase> Phases = new List<CustomTNHPhase>();
    }

    public class CustomTNHPhase
    {
        public float GrenadeChance = 0;
        public string GrenadeType = "Sosiggrenade_Flash";
    }




    [BepInPlugin("org.bebinex.plugins.tnhtweaker", "A plugin for tweaking tnh parameters", "1.0.0.0")]
    public class TNHTweaker : BaseUnityPlugin
    {

        private static ConfigEntry<bool> instantRespawn;
        private static ConfigEntry<bool> overridePatrols;
        private static ConfigEntry<bool> allowFriendlyPatrols;
        private static ConfigEntry<bool> alwaysSpawnPatrols;
        private static ConfigEntry<bool> printCharacters;
        private static ConfigEntry<bool> onlyPrintCustomCharacters;

        private static ConfigEntry<int> maxPatrols;
        private static ConfigEntry<int> patrolSize;
        private static ConfigEntry<float> timeTilRegen;
        private static ConfigEntry<int> patrolTeams;
        private static ConfigEntry<string> enemyTypes;
        private static ConfigEntry<string> leaderTypes;

        private static Dictionary<int, List<string>> teamEnemyTypes = new Dictionary<int, List<string>>();
        private static Dictionary<int, List<string>> teamLeaderTypes = new Dictionary<int, List<string>>();

        private static float timeTillForcedSpawn;
        private static string characterPath;
        private static int loadedCharacters = 0;

        private static List<TNH_CharacterDef> customCharacters = new List<TNH_CharacterDef>();

        private static Dictionary<TNH_CharacterDef,CustomCharData> customCharDict = new Dictionary<TNH_CharacterDef, CustomCharData>();

        private void Awake()
        {
            Debug.Log("Patching!");
            Harmony.CreateAndPatchAll(typeof(TNHTweaker));
            Debug.Log("Patched!");
            
            LoadConfigFile();

            SetupCharacterDirectory();

            CreateTemplateFile(characterPath);
        }

        private void LoadConfigFile()
        {
            Debug.Log("TNHTWEAKER -- GETTING CONFIG FILE");

            instantRespawn = Config.Bind("General",
                                         "InstantWaveRespawn",
                                         false,
                                         "After defeating a wave of enemies during a hold, the next wave is sent immediately");

            overridePatrols = Config.Bind("Patrol",
                                         "OverridePatrols",
                                         false,
                                         "Enables patrols to be set based on config settings here");

            allowFriendlyPatrols = Config.Bind("Patrol",
                                               "AllowFriendlyPatrols",
                                               false,
                                               "Allows for some patrols to be on your team when spawned (only applied when more than one team)");

            alwaysSpawnPatrols = Config.Bind("Patrol",
                                             "AlwaysSpawnPatrols",
                                             false,
                                             "Makes it so patrols always spawn during the take phase (normaly patrols only spawn when you're standing inside a supply point)");

            maxPatrols = Config.Bind("Patrol",
                                     "MaxPatrols",
                                     5,
                                     "Sets the max amount of patrols that can be spawned at once");
            patrolSize = Config.Bind("Patrol",
                                     "PatrolSize",
                                     4,
                                     "Sets how many sosigs are in each patrol");

            timeTilRegen = Config.Bind("Patrol",
                                       "TimeTillRegen",
                                       10f,
                                       "Sets delay (in seconds) between spawning of patrols");

            patrolTeams = Config.Bind("Patrol",
                                     "PatrolTeams",
                                     1,
                                     "Sets how many different IFF values patrols can have. Teams are chosen at random for each patrol spawned");

            enemyTypes = Config.Bind("Patrol",
                                     "EnemyTypes",
                                     "M_Swat_Scout,M_Swat_Ranger;D_Bandito,D_Gambler",
                                     "Sets the type of sosig that spawns as the enemy of the patrol. Here are possible types for both enemies and leaders:\nNone\nDummies\nM_Swat_Scout\nM_Swat_Ranger\nM_Swat_Sniper\nM_Swat_Riflewiener\nM_Swat_Officer\nM_Swat_SpecOps\nM_Swat_Markswiener\nM_Swat_Shield\nM_Swat_Heavy\nM_Swat_Breacher\nM_Swat_Guard\nW_Green_Guard\nW_Green_Patrol\nW_Green_Officer\nW_Green_Riflewiener\nW_Green_Grenadier\nW_Green_HeavyRiflewiener\nW_Green_Machinegunner\nW_Green_Flamewiener\nW_Green_Antitank\nW_Tan_Guard\nW_Tan_Patrol\nW_Tan_Officer\nW_Tan_Riflewiener\nW_Tan_Grenadier\nW_Tan_HeavyRiflewiener\nW_Tan_Machinegunner\nW_Tan_Flamewiener\nW_Tan_Antitank\nW_Brown_Guard\nW_Brown_Patrol\nW_Brown_Officer\nW_Brown_Riflewiener\nW_Brown_Grenadier\nW_Brown_HeavyRiflewiener\nW_Brown_Machinegunner\nW_Brown_Flamewiener\nW_Brown_Antitank\nW_Grey_Guard\nW_Grey_Patrol\nW_Grey_Officer\nW_Grey_Riflewiener\nW_Grey_Grenadier\nW_Grey_HeavyRiflewiener\nW_Grey_Machinegunner\nW_Grey_Flamewiener\nW_Grey_Antitank\nD_Gambler\nD_Bandito\nD_Gunfighter\nD_BountyHunter\nD_Sheriff\nD_Boss\nD_Sniper\nD_BountyHunterBoss\nJ_Guard\nJ_Patrol\nJ_Grenadier\nJ_Officer\nJ_Commando\nJ_Riflewiener\nJ_Flamewiener\nJ_Machinegunner\nJ_Sniper\nH_BreadCrabZombie_Fast\nH_BreadCrabZombie_HEV\nH_BreadCrabZombie_Poison\nH_BreadCrabZombie_Standard\nH_BreadCrabZombie_Zombie\nH_CivicErection_Meathack\nH_CivicErection_Melee\nH_CivicErection_Pistol\nH_CivicErection_SMG\nH_OberwurstElite_AR2\nH_OberwurstSoldier_Shotgun\nH_OberwurstSoldier_SMG\nH_OberwurstSoldier_SMGNade\nH_OberwurstSoldier_Sniper");

            leaderTypes = Config.Bind("Patrol",
                                      "LeaderType",
                                      "M_Swat_Officer,M_Swat_Heavy;D_BountyHunterBoss,D_Boss",
                                      "Sets the type of sosig that spawns as a leader of the patrol");

            printCharacters = Config.Bind("Debug",
                                         "PrintCharacterInfo",
                                         false,
                                         "Decide if should print all character info");

            onlyPrintCustomCharacters = Config.Bind("Debug",
                                         "OnlyPrintCustomCharacters",
                                         false,
                                         "Decide if should print only the custom characters info when printing characters");

            timeTillForcedSpawn = timeTilRegen.Value;

            //Load the strings for enemies and leaders into a dictionary
            List<string> enemies = new List<string>(enemyTypes.Value.Split(';'));
            for (int i = 0; i < enemies.Count; i++)
            {
                teamEnemyTypes.Add(i, new List<string>(enemies[i].Split(',')));
            }

            List<string> leaders = new List<string>(leaderTypes.Value.Split(';'));
            for (int i = 0; i < leaders.Count; i++)
            {
                teamLeaderTypes.Add(i, new List<string>(leaders[i].Split(',')));
            }
        }

        private void SetupCharacterDirectory()
        {
            characterPath = Application.dataPath.Replace("/h3vr_Data", "/CustomCharacters");
            Debug.Log("TNHTWEAKER -- CHARACTER FILE PATH IS: " + characterPath);

            if (Directory.Exists(characterPath))
            {
                Debug.Log("Folder exists!");
            }
            else
            {
                Debug.Log("Folder does not exist! Creating");
                Directory.CreateDirectory(characterPath);
            }
        }

        [HarmonyPatch(typeof(TNH_UIManager), "Start")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool AddCharacters(List<TNH_UIManager.CharacterCategory> ___Categories, TNH_CharacterDatabase ___CharDatabase)
        {

            CreateObjectIDFile(characterPath);

            GM.TNHOptions.Char = TNH_Char.DD_ClassicLoudoutLouis;

            Debug.Log("TNHTWEAKER -- CLEARING CATEGORIES");

            //When starting out in the TNH lobby, clear all custom characters
            foreach (TNH_CharacterDef character in customCharacters)
            {
                ___Categories[(int)character.Group].Characters.Remove(character.CharacterID);
                ___CharDatabase.Characters.Remove(character);
            }
            customCharacters.Clear();
            customCharDict.Clear();

            Debug.Log("TNHTWEAKER -- PRE LOADING CHECK -- THESE SHOULD ONLY BE THE DEFAULT CHARACTERS");
            foreach (TNH_CharacterDef character in ___CharDatabase.Characters)
            {
                Debug.Log("CHARACTER IN DATABASE: " + character.DisplayName);
            }

            foreach (TNH_UIManager.CharacterCategory category in ___Categories)
            {
                Debug.Log("UI CATEGORY: " + category.CategoryName);

                foreach (TNH_Char character in category.Characters)
                {
                    Debug.Log("CHARACTER IN UI CATEGORY: " + character);
                }
            }

            LoadCustomCharacters(___CharDatabase.Characters[0]);

            foreach (TNH_CharacterDef newCharacter in customCharacters)
            {
                ___Categories[(int)newCharacter.Group].Characters.Add(newCharacter.CharacterID);
                ___CharDatabase.Characters.Add(newCharacter);
            }

            if (printCharacters.Value)
            {
                if (onlyPrintCustomCharacters.Value)
                {
                    Debug.Log("TNHTWEAKER -- PRINTING ONLY CUSTOM CHARACTERS\n");
                    foreach (TNH_CharacterDef ch in customCharacters)
                    {
                        PrintCharacterInfo(ch);
                    }
                }

                else
                {
                    Debug.Log("TNHTWEAKER -- PRINTING ALL LOADED CHARACTERS\n");
                    foreach (TNH_CharacterDef ch in ___CharDatabase.Characters)
                    {
                        PrintCharacterInfo(ch);
                    }
                }
            }

            return true;
        }


        private static void LoadCustomCharacters(TNH_CharacterDef backupCharacter)
        {

            Debug.Log("TNHTWEAKER -- LOADING CUSTOM CHARACTERS");

            string[] characterDirs = Directory.GetDirectories(characterPath);

            foreach (string characterDir in characterDirs)
            {

                if (!File.Exists(characterDir + "/thumb.png") || !File.Exists(characterDir + "/character.txt"))
                {
                    Debug.LogError("TNHTWEAKER -- CHARACTER DIRECTORY FOUND, BUT MISSING ONE OR MORE OF THE FOLLOWING: character.txt , thumb.png");
                    continue;
                }

                TNH_CharacterDef character = GetCharacterFromString(characterDir, backupCharacter);
                customCharacters.Add(character);
                loadedCharacters += 1;

                Debug.Log("TNHTWEAKER -- CHARACTER LOADED: " + character.DisplayName);
            }
        }

        
        public static TNH_CharacterDef GetCharacterFromString(string charPath, TNH_CharacterDef backupCharacter)
        {
            TNH_CharacterDef character = (TNH_CharacterDef)ScriptableObject.CreateInstance(typeof(TNH_CharacterDef));
            customCharDict.Add(character, new CustomCharData());

            //Get initial data from character directory
            string imagePath = Directory.GetFiles(charPath, "thumb.png")[0];
            string dataPath = Directory.GetFiles(charPath, "character.txt")[0];
            Sprite charSprite = LoadSprite(imagePath);
            string[] characterData = File.ReadAllLines(dataPath);

            /////Yu[

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
            character.CharacterID = (TNH_Char)(30 + loadedCharacters);
            character.TableID = string.Concat("CUSTOM_", (1 + loadedCharacters));
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

                Debug.Log("" + GetIndent(traverseStack.Count) + "READING LINE: " + line);

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

                            Debug.Log("" + GetIndent(traverseStack.Count) + "CREATING EMPTY LIST FOR FIELD (" + fieldName + ")");
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
                        Debug.Log("" + GetIndent(traverseStack.Count) + "CREATING NEW LIST ELEMENT (" + fieldType.FullName + ")");

                        Traverse currentList = traverseStack.Peek();
                        currentList.Method("Add", newElement.GetValue()).GetValue();
                        traverseStack.Push(newElement);
                    }

                    //If the previous object in the stack is an object, check if this is a field of the object
                    else if (traverseStack.Peek().Field(fieldName).FieldExists())
                    {
                        Debug.Log("" + GetIndent(traverseStack.Count) + "CREATING NEW OBJECT FIELD (" + fieldName + ")");

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
                        Debug.Log("" + GetIndent(traverseStack.Count) + "END OF OBJECT");
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

                            Debug.Log("" + GetIndent(traverseStack.Count) + "CREATING LIST FOR FIELD (" + fieldName + ")");
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
                        Debug.Log("" + GetIndent(traverseStack.Count) + "END OF LIST");
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
                            if(traverseStack.Peek().GetValue().GetType() == typeof(TNH_HoldChallenge.Phase))
                            {
                                Debug.Log("" + GetIndent(traverseStack.Count) + "FIELD IS FOR CUSTOM DATA OF HOLD PHASE");
                                int levelIndex = customCharDict[character].Levels.Count - 1;
                                int phaseIndex = customCharDict[character].Levels[levelIndex].Phases.Count - 1;
                                currField = Traverse.Create(customCharDict[character].Levels[levelIndex].Phases[phaseIndex]).Field(GetTagFromLine(line));
                            }

                            //Otherwise the field is inside the character
                            else
                            {
                                Debug.Log("" + GetIndent(traverseStack.Count) + "FIELD IS FOR CUSTOM DATA OF CHARACTER");
                                currField = Traverse.Create(customCharDict[character]).Field(GetTagFromLine(line));
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

                            Debug.Log("" + GetIndent(traverseStack.Count) + "FIELD: GENERIC LIST OF ELEMENT TYPE (" + propertyType.FullName + ")");

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

                                else if(propertyType == typeof(int))
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
                            Debug.Log("" + GetIndent(traverseStack.Count) + "FIELD: INT");
                            currField.SetValue(GetIntFromLine(line));
                        }

                        else if (currType == typeof(float))
                        {
                            Debug.Log("" + GetIndent(traverseStack.Count) + "FIELD: FLOAT");
                            currField.SetValue(GetFloatFromLine(line));
                        }

                        else if (currType == typeof(bool))
                        {
                            Debug.Log("" + GetIndent(traverseStack.Count) + "FIELD: BOOL");
                            currField.SetValue(GetBoolFromLine(line));
                        }

                        else if (currType == typeof(string))
                        {
                            Debug.Log("" + GetIndent(traverseStack.Count) + "FIELD: STRING");
                            currField.SetValue(GetStringFromLine(line));
                        }

                        //Handle case where it is an enum
                        else if (currType.IsEnum)
                        {

                            if (GetIntFromLine(line) == -1)
                            {
                                Debug.Log("" + GetIndent(traverseStack.Count) + "FIELD: ENUM, ASSIGNED BY STRING");
                                currField.SetValue(Enum.Parse(currType, GetStringFromLine(line)));
                            }
                            else
                            {
                                Debug.Log("" + GetIndent(traverseStack.Count) + "FIELD: ENUM, ASSIGNED BY INT");
                                currField.SetValue(Enum.ToObject(currType, GetIntFromLine(line)));
                            }
                        }

                        else if (currType == typeof(Sprite))
                        {
                            Debug.Log("" + GetIndent(traverseStack.Count) + "FIELD: ICON, LOADING FROM " + charPath + "/" + GetStringFromLine(line));
                            currField.SetValue(LoadSprite(charPath + "/" + GetStringFromLine(line)));
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

            for(int i = 0; i < count; i++)
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
                if(!int.TryParse(line.Split('=')[1], out result))
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

        public static void PrintCharacterInfo(TNH_CharacterDef character)
        {
            Debug.Log("\n\n\nTNHTWEAKER -- CHARACTER INFO");
            Debug.Log("Name: " + character.DisplayName);
            Debug.Log("ID: " + character.CharacterID);
            Debug.Log("Group: " + character.Group);
            Debug.Log("TableID: " + character.TableID);
            Debug.Log("Starting Tokens: " + character.StartingTokens);
            Debug.Log("Force Agent Weapons: " + character.ForceAllAgentWeapons);
            Debug.Log("Printing Pools:");

            Debug.Log("Num entries: " + character.EquipmentPool.Entries.Count);

            foreach (EquipmentPoolDef.PoolEntry entry in character.EquipmentPool.Entries)
            {
                PrintPoolEntry(entry);
            }
            
        }

        public static void PrintPoolEntry(EquipmentPoolDef.PoolEntry entry)
        {
            Debug.Log("\n--Pool Entry--");
            Debug.Log("EntryType: " + entry.Type);
            Debug.Log("Token Cost: " + entry.TokenCost);
            Debug.Log("Token Cost Limited: " + entry.TokenCost_Limited);
            Debug.Log("Min Level Appears: " + entry.MinLevelAppears);
            Debug.Log("Max Level Appears: " + entry.MaxLevelAppears);
            Debug.Log("Rarity: " + entry.Rarity);
            Debug.Log("Object Table Def:");
            PrintObjectTableDef(entry.TableDef);
        }

        public static void PrintObjectTableDef(ObjectTableDef def)
        {
            Debug.Log(" - Category: " + def.Category);
            Debug.Log(" - MinAmmoCapacity: " + def.MinAmmoCapacity);
            Debug.Log(" - MaxAmmoCapacity: " + def.MaxAmmoCapacity);
            Debug.Log(" - IsBlanked: " + def.IsBlanked);
            Debug.Log(" - SpawnsInSmallCase: " + def.SpawnsInSmallCase);
            Debug.Log(" - SpawnsInLargeCase: " + def.SpawnsInLargeCase);
            Debug.Log(" - UseIDListOverride: " + def.UseIDListOverride);

            Debug.Log(" - IDOverrides: ");
            foreach(string id in def.IDOverride)
            {
                Debug.Log(" - - " + id);
            }

            Debug.Log(" - Contained Objects: ");
            foreach (FVRObject obj in def.Objs)
            {
                Debug.Log(" - - " + obj.DisplayName);
            }

            Debug.Log(" - Eras: ");
            foreach (FVRObject.OTagEra item in def.Eras)
            {
                Debug.Log(" - - " + item);
            }

            Debug.Log(" - Sets: ");
            foreach (FVRObject.OTagSet item in def.Sets)
            {
                Debug.Log(" - - " + item);
            }

            Debug.Log(" - Sizes: ");
            foreach (FVRObject.OTagFirearmSize item in def.Sizes)
            {
                Debug.Log(" - - " + item);
            }

            Debug.Log(" - Actions: ");
            foreach (FVRObject.OTagFirearmAction item in def.Actions)
            {
                Debug.Log(" - - " + item);
            }

            Debug.Log(" - Modes: ");
            foreach (FVRObject.OTagFirearmFiringMode item in def.Modes)
            {
                Debug.Log(" - - " + item);
            }

            Debug.Log(" - ExludedModes: ");
            foreach (FVRObject.OTagFirearmFiringMode item in def.ExcludeModes)
            {
                Debug.Log(" - - " + item);
            }

            Debug.Log(" - FeedOptions: ");
            foreach (FVRObject.OTagFirearmFeedOption item in def.Feedoptions)
            {
                Debug.Log(" - - " + item);
            }

            Debug.Log(" - Mounts: ");
            foreach (FVRObject.OTagFirearmMount item in def.MountsAvailable)
            {
                Debug.Log(" - - " + item);
            }

            Debug.Log(" - RoundPowers: ");
            foreach (FVRObject.OTagFirearmRoundPower item in def.RoundPowers)
            {
                Debug.Log(" - - " + item);
            }

            Debug.Log(" - Features: ");
            foreach (FVRObject.OTagAttachmentFeature item in def.Features)
            {
                Debug.Log(" - - " + item);
            }

            Debug.Log(" - MeleeStyles: ");
            foreach (FVRObject.OTagMeleeStyle item in def.MeleeStyles)
            {
                Debug.Log(" - - " + item);
            }

            Debug.Log(" - MeleeHandedness: ");
            foreach (FVRObject.OTagMeleeHandedness item in def.MeleeHandedness)
            {
                Debug.Log(" - - " + item);
            }

            Debug.Log(" - MountTypes: ");
            foreach (FVRObject.OTagFirearmMount item in def.MountTypes)
            {
                Debug.Log(" - - " + item);
            }

            Debug.Log(" - PowerupTypes: ");
            foreach (FVRObject.OTagPowerupType item in def.PowerupTypes)
            {
                Debug.Log(" - - " + item);
            }

            Debug.Log(" - ThrownType: ");
            foreach (FVRObject.OTagThrownType item in def.ThrownTypes)
            {
                Debug.Log(" - - " + item);
            }

            Debug.Log(" - ThrownDamageTypes: ");
            foreach (FVRObject.OTagThrownDamageType item in def.ThrownDamageTypes)
            {
                Debug.Log(" - - " + item);
            }

            Debug.Log(" - End of object table");

        }

        /// <summary>
        /// Creates a template file for character creation. Follows info found here: https://www.c-sharpcorner.com/UploadFile/mahesh/create-a-text-file-in-C-Sharp/
        /// </summary>
        /// <param name="path"></param>
        private static void CreateTemplateFile(string path)
        {
            try
            {
                if (File.Exists(path + "/character_template.txt"))
                {
                    File.Delete(path + "/character_template.txt");
                }

                // Create a new file     
                using (StreamWriter sw = File.CreateText(path + "/character_template.txt"))
                {
                    sw.WriteLine("#BASIC CHARACTER INFO");
                    sw.WriteLine("DisplayName=temp name");
                    sw.WriteLine("Group=0");
                    sw.WriteLine("StartingTokens=3");
                    sw.WriteLine("ForceAllAgentWeapons=false");
                    sw.WriteLine("Description=a fitting description");
                    sw.WriteLine("UsesPurchasePriceIncrement=true");
                    sw.WriteLine("Has_Weapon_Primary=true");
                    sw.WriteLine("Has_Weapon_Secondary=false");
                    sw.WriteLine("Has_Weapon_Tertiary=false");
                    sw.WriteLine("Has_Item_Primary=false");
                    sw.WriteLine("Has_Item_Secondary=false");
                    sw.WriteLine("Has_Item_Tertiary=false");
                    sw.WriteLine("Has_Item_Shield=false");
                    sw.WriteLine("@AdditionalSupplyPoints=2,2,2,2,2");


                    sw.WriteLine("\n");
                    sw.WriteLine("#START OF PRIMARY STARTING WEAPON");
                    sw.WriteLine("Weapon_Primary{");
                    sw.WriteLine("  ListOverride[]");
                    sw.WriteLine("  Num_Mags_SL_Clips=2");
                    sw.WriteLine("  Num_Rounds=0");

                    sw.WriteLine("  TableDefs[");

                    sw.WriteLine("");
                    sw.WriteLine("  {");
                    sw.WriteLine("      #Options: " + string.Join(",", Enum.GetNames(typeof(EquipmentPoolDef.PoolEntry.PoolEntryType))));
                    sw.WriteLine("      Category=Firearm");
                    sw.WriteLine("      IsBlanked=false");
                    sw.WriteLine("      MinAmmoCapacity=1");
                    sw.WriteLine("      MaxAmmoCapacity=20");
                    sw.WriteLine("      UseIDListOverride=true");
                    sw.WriteLine("      IDOverride=Makarov");
                    sw.WriteLine("  }");
                    sw.WriteLine("");

                    sw.WriteLine("  ]");
                    sw.WriteLine("}");
                    sw.WriteLine("#END OF PRIMARY STARTING WEAPON");


                    sw.WriteLine("\n");
                    sw.WriteLine("#START OF EQUIPMENT POOLS");
                    sw.WriteLine("EquipmentPool{");
                    sw.WriteLine("  Entries[");
                    sw.WriteLine("");

                    //Open First Entry
                    sw.WriteLine("  #START OF POOL ENTRY");
                    sw.WriteLine("  {");
                    sw.WriteLine("      #Options: " + string.Join(",", Enum.GetNames(typeof(EquipmentPoolDef.PoolEntry.PoolEntryType))));
                    sw.WriteLine("      Type=Firearm");
                    sw.WriteLine("      TokenCost=1");
                    sw.WriteLine("      TokenCost_Limited=1");
                    sw.WriteLine("      MinLevelAppears=0");
                    sw.WriteLine("      MaxLevelAppears=99");
                    sw.WriteLine("      Rarity=1");

                    sw.WriteLine("");
                    sw.WriteLine("      TableDef{");
                    sw.WriteLine("          Icon=thumb.png");
                    sw.WriteLine("          #Options: " + string.Join(",", Enum.GetNames(typeof(FVRObject.ObjectCategory))));
                    sw.WriteLine("          Category=Firearm");
                    sw.WriteLine("          MinAmmoCapacity=-1");
                    sw.WriteLine("          MaxAmmoCapacity=-1");
                    sw.WriteLine("          IsBlanked=false");
                    sw.WriteLine("          SpawnsInSmallCase=false");
                    sw.WriteLine("          SpawnsInLargeCase=false");
                    sw.WriteLine("          UseIDListOverride=false");
                    sw.WriteLine("          IDOverride=");
                    sw.WriteLine("          #Options: " + string.Join(",", Enum.GetNames(typeof(FVRObject.OTagEra))));
                    sw.WriteLine("          Eras=WW1,WW2");
                    sw.WriteLine("          #Options: " + string.Join(",", Enum.GetNames(typeof(FVRObject.OTagSet))));
                    sw.WriteLine("          Sets=Real");
                    sw.WriteLine("          #Options: " + string.Join(",", Enum.GetNames(typeof(FVRObject.OTagFirearmSize))));
                    sw.WriteLine("          Sizes=FullSize,Pistol");
                    sw.WriteLine("          #Options: " + string.Join(",", Enum.GetNames(typeof(FVRObject.OTagFirearmAction))));
                    sw.WriteLine("          Actions=");
                    sw.WriteLine("          #Options: " + string.Join(",", Enum.GetNames(typeof(FVRObject.OTagFirearmFiringMode))));
                    sw.WriteLine("          Modes=");
                    sw.WriteLine("          #Options: " + string.Join(",", Enum.GetNames(typeof(FVRObject.OTagFirearmFiringMode))));
                    sw.WriteLine("          ExcludeModes=");
                    sw.WriteLine("          #Options: " + string.Join(",", Enum.GetNames(typeof(FVRObject.OTagFirearmFeedOption))));
                    sw.WriteLine("          Feedoptions=");
                    sw.WriteLine("          #Options: " + string.Join(",", Enum.GetNames(typeof(FVRObject.OTagFirearmMount))));
                    sw.WriteLine("          MountsAvailable=");
                    sw.WriteLine("          #Options: " + string.Join(",", Enum.GetNames(typeof(FVRObject.OTagFirearmRoundPower))));
                    sw.WriteLine("          RoundPowers=");
                    sw.WriteLine("          #Options: " + string.Join(",", Enum.GetNames(typeof(FVRObject.OTagAttachmentFeature))));
                    sw.WriteLine("          Features=");
                    sw.WriteLine("          #Options: " + string.Join(",", Enum.GetNames(typeof(FVRObject.OTagMeleeStyle))));
                    sw.WriteLine("          MeleeStyles=");
                    sw.WriteLine("          #Options: " + string.Join(",", Enum.GetNames(typeof(FVRObject.OTagMeleeHandedness))));
                    sw.WriteLine("          MeleeHandedness=");
                    sw.WriteLine("          #Options: " + string.Join(",", Enum.GetNames(typeof(FVRObject.OTagFirearmMount))));
                    sw.WriteLine("          MountTypes=");
                    sw.WriteLine("          #Options: " + string.Join(",", Enum.GetNames(typeof(FVRObject.OTagPowerupType))));
                    sw.WriteLine("          PowerupTypes=");
                    sw.WriteLine("          #Options: " + string.Join(",", Enum.GetNames(typeof(FVRObject.OTagThrownType))));
                    sw.WriteLine("          ThrownTypes=");
                    sw.WriteLine("          #Options: " + string.Join(",", Enum.GetNames(typeof(FVRObject.OTagThrownDamageType))));
                    sw.WriteLine("          ThrownDamageTypes=");
                    sw.WriteLine("          Objs[]");
                    sw.WriteLine("      }");
                    sw.WriteLine("  }");
                    sw.WriteLine("  #END OF POOL ENTRY");

                    sw.WriteLine("");
                    sw.WriteLine("  ]");
                    sw.WriteLine("}");
                    sw.WriteLine("#END OF EQUIPMENT POOLS");


                    //Open Character Progression
                    sw.WriteLine("\n");
                    sw.WriteLine("#START OF PROGRESSIONS");
                    sw.WriteLine("#NOTE: Generally only need one progression object, and then the levels[] are what determine each round of Take and Hold");
                    sw.WriteLine("Progressions[");
                    sw.WriteLine("{");

                    sw.WriteLine("");
                    sw.WriteLine("  #START OF LEVELS");
                    sw.WriteLine("  Levels[");

                    sw.WriteLine("");
                    sw.WriteLine("  #START OF LEVEL 1");
                    sw.WriteLine("  {");
                    sw.WriteLine("      NumOverrideTokensForHold=5");

                    //Open Take Challenge
                    sw.WriteLine("");
                    sw.WriteLine("      TakeChallenge{");
                    sw.WriteLine("          #Options: " + string.Join(",", Enum.GetNames(typeof(TNH_TurretType))));
                    sw.WriteLine("          TurretType=SMG");
                    sw.WriteLine("          NumTurrets=0");
                    sw.WriteLine("          #Options: " + string.Join(",", Enum.GetNames(typeof(TNH_EnemyType))));
                    sw.WriteLine("          GuardType=M_Swat_Scout");
                    sw.WriteLine("          NumGuards=2");
                    sw.WriteLine("          IFFUsed=1");
                    sw.WriteLine("      }");
                    //Close Take Challenge


                    //Open Hold Challenge
                    sw.WriteLine("");
                    sw.WriteLine("      HoldChallenge{");
                    sw.WriteLine("          Phases[");
                    sw.WriteLine("");


                    

                    //Open Phase
                    sw.WriteLine("          {");
                    sw.WriteLine("              #Options: " + string.Join(",", Enum.GetNames(typeof(TNH_EncryptionType))));
                    sw.WriteLine("              Encryption=Static");
                    sw.WriteLine("              #Options: " + string.Join(",", Enum.GetNames(typeof(TNH_EnemyType))));
                    sw.WriteLine("              EnemyType=M_Swat_Scout");
                    sw.WriteLine("              #Options: " + string.Join(",", Enum.GetNames(typeof(TNH_EnemyType))));
                    sw.WriteLine("              EnemyType_Leader=M_Swat_Scout");
                    sw.WriteLine("              MinTargets=3");
                    sw.WriteLine("              MaxTargets=4");
                    sw.WriteLine("              MinEnemies=3");
                    sw.WriteLine("              MaxEnemies=4");
                    sw.WriteLine("              SpawnCadence=10");
                    sw.WriteLine("              MaxEnemiesAlive=10");
                    sw.WriteLine("              MaxDirections=2");
                    sw.WriteLine("              ScanTime=20");
                    sw.WriteLine("              WarmUp=5");
                    sw.WriteLine("              IFFUsed=1");
                    sw.WriteLine("              @GrenadeChance=0");
                    sw.WriteLine("              #Options: Sosiggrenade_Flash,Sosiggrenade_Frag,SosiggrenadeFragWW2,Sosiggrenade_HLHE,Sosiggrenade_Incendiary,Sosiggrenade_Meathack,SosiggrenadePoisonBreadCrap,Sosiggrenade_Smoke");
                    sw.WriteLine("              @GrenadeType=Sosiggrenade_Flash");
                    sw.WriteLine("          }");
                    //Close Phase

                    sw.WriteLine("");
                    sw.WriteLine("          ]");
                    sw.WriteLine("      }");
                    //Close Hold Challenge


                    //Open Supply Challenge
                    sw.WriteLine("");
                    sw.WriteLine("      SupplyChallenge{");
                    sw.WriteLine("          #Options: " + string.Join(",", Enum.GetNames(typeof(TNH_TurretType))));
                    sw.WriteLine("          TurretType=SMG");
                    sw.WriteLine("          NumTurrets=0");
                    sw.WriteLine("          #Options: " + string.Join(",", Enum.GetNames(typeof(TNH_EnemyType))));
                    sw.WriteLine("          GuardType=M_Swat_Scout");
                    sw.WriteLine("          NumGuards=2");
                    sw.WriteLine("          IFFUsed=1");
                    sw.WriteLine("      }");
                    //Close Supply Challenge


                    //Open Patrol Challenge
                    sw.WriteLine("");
                    sw.WriteLine("      PatrolChallenge{");
                    sw.WriteLine("          Patrols[");

                    //Open Patrol
                    sw.WriteLine("");
                    sw.WriteLine("          {");
                    sw.WriteLine("              #Options: " + string.Join(",", Enum.GetNames(typeof(TNH_EnemyType))));
                    sw.WriteLine("              EnemyType=M_Swat_Scout");
                    sw.WriteLine("              #Options: " + string.Join(",", Enum.GetNames(typeof(TNH_EnemyType))));
                    sw.WriteLine("              LeaderType=M_Swat_Scout");
                    sw.WriteLine("              PatrolSize=4");
                    sw.WriteLine("              MaxPatrols=4");
                    sw.WriteLine("              MaxPatrols_LimitedAmmo=4");
                    sw.WriteLine("              TimeTilRegen=5");
                    sw.WriteLine("              TimeTilRegen_LimitedAmmo=5");
                    sw.WriteLine("              IFFUsed=1");
                    sw.WriteLine("          }");
                    //Close Patrol

                    sw.WriteLine("");
                    sw.WriteLine("          ]");
                    sw.WriteLine("      }");
                    //Close Patrol Challenge


                    //Open Traps Challenge
                    sw.WriteLine("");
                    sw.WriteLine("      TrapsChallenge{");
                    sw.WriteLine("          Traps[");

                    //Open Trap
                    sw.WriteLine("");
                    sw.WriteLine("          {");
                    sw.WriteLine("              #Options: " + string.Join(",", Enum.GetNames(typeof(TNH_TrapType))));
                    sw.WriteLine("              Type=Poppers");
                    sw.WriteLine("              MinNumber=0");
                    sw.WriteLine("              MaxNumber=0");
                    sw.WriteLine("          }");
                    //Close Trap

                    sw.WriteLine("");
                    sw.WriteLine("          ]");
                    sw.WriteLine("      }");
                    //Close Traps Challenge



                    sw.WriteLine("");
                    sw.WriteLine("  }");
                    sw.WriteLine("  #END OF LEVEL 1");
                    //Close Level

                    sw.WriteLine("");
                    sw.WriteLine("  ]");
                    sw.WriteLine("  #END OF LEVELS");

                    sw.WriteLine("");
                    sw.WriteLine("}");
                    sw.WriteLine("]");
                    sw.WriteLine("#END OF PROGRESSIONS");
                    //Close Progression List

                    sw.Close();
                }
            }

            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }


        }

        private static void CreateObjectIDFile(string path)
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

        [HarmonyPatch(typeof(TNH_HoldPoint), "SpawningRoutineUpdate")] // Specify target method with HarmonyPatch attribute
        [HarmonyPostfix]
        public static void SpawningRoutineUpdateAfter(ref float ___m_tickDownToNextGroupSpawn, List<Sosig> ___m_activeSosigs)
        {
            if(___m_activeSosigs.Count == 0 && instantRespawn.Value)
            {
                if(___m_tickDownToNextGroupSpawn > 1)
                {
                    Debug.Log("TNHTWEAKER -- FORCING NEXT WAVE TO BEGIN");
                    ___m_tickDownToNextGroupSpawn = 1;
                }
            }
        }

        [HarmonyPatch(typeof(TNH_Manager), "SetPhase_Take")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool OverridePatrolValues(TNH_Progression.Level ___m_curLevel)
        {
            if (overridePatrols.Value)
            {
                Debug.Log("TNHTWEAKER -- OVERRIDING PATROLS");

                TNH_PatrolChallenge.Patrol onlyPatrol = ___m_curLevel.PatrolChallenge.Patrols[0];

                onlyPatrol.MaxPatrols = maxPatrols.Value;
                onlyPatrol.MaxPatrols_LimitedAmmo = maxPatrols.Value;
                onlyPatrol.PatrolSize = patrolSize.Value;
                onlyPatrol.TimeTilRegen = timeTilRegen.Value;
                onlyPatrol.TimeTilRegen_LimitedAmmo = timeTilRegen.Value;

                ___m_curLevel.PatrolChallenge.Patrols.Clear();
                ___m_curLevel.PatrolChallenge.Patrols.Add(onlyPatrol);

                Debug.Log("TNHTWEAKER -- PATROLS OVERWRITTEN:");
            }

            return true;
        }

        [HarmonyPatch(typeof(TNH_Manager), "UpdatePatrols")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool BeforeUpdatePatrol(TNH_Manager __instance, List<TNH_Manager.SosigPatrolSquad> ___m_patrolSquads, ref float ___m_timeTilPatrolCanSpawn, TNH_Progression.Level ___m_curLevel, int ___m_curHoldIndex)
        {

            if (overridePatrols.Value && alwaysSpawnPatrols.Value)
            {
                timeTillForcedSpawn -= Time.deltaTime;

                ___m_timeTilPatrolCanSpawn = 999999f;

                if (___m_patrolSquads.Count < ___m_curLevel.PatrolChallenge.Patrols[0].MaxPatrols && timeTillForcedSpawn <= 0)
                {
                    ___m_timeTilPatrolCanSpawn = timeTilRegen.Value * 2;
                    timeTillForcedSpawn = timeTilRegen.Value;

                    Debug.Log("TNHTWEAKER -- FORCING A PATROL TO SPAWN -- " + timeTillForcedSpawn + " SECONDS UNTIL NEXT PATROL");
                    Debug.Log("Possible methods: " + Traverse.Create(__instance).Methods().ToString());

                    Traverse.Create(__instance).Method("GenerateValidPatrol", ___m_curLevel.PatrolChallenge, GetClosestSupplyPointIndex(__instance.SupplyPoints, GM.CurrentPlayerBody.Head.position), ___m_curHoldIndex, true).GetValue();
                    
                }
                
            }

            return true;
        }

        [HarmonyPatch(typeof(TNH_Manager), "GenerateValidPatrol")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool BeforeGeneratePatrol(TNH_PatrolChallenge P, List<TNH_Manager.SosigPatrolSquad> ___m_patrolSquads)
        {
            Debug.Log("TNHTWEAKER -- GENERATING A PATROL -- THERE ARE CURRENTLY " + ___m_patrolSquads.Count + " PATROLS ACTIVE");

            if (overridePatrols.Value)
            {
                int team;
                team = UnityEngine.Random.Range(1, patrolTeams.Value + 1);

                string enemyType;
                string leaderType;

                List<string> possibleTypes;
                if (teamEnemyTypes.TryGetValue(team - 1, out possibleTypes))
                {
                    enemyType = possibleTypes[UnityEngine.Random.Range(0, possibleTypes.Count)];
                }
                else
                {
                    enemyType = "M_Swat_Scout";
                }


                possibleTypes = null;
                if (teamLeaderTypes.TryGetValue(team - 1, out possibleTypes))
                {
                    leaderType = possibleTypes[UnityEngine.Random.Range(0, possibleTypes.Count)];
                }
                else
                {
                    leaderType = "M_Swat_Scout";
                }

                P.Patrols[0].EnemyType = (TNH_EnemyType)Enum.Parse(typeof(TNH_EnemyType), enemyType, true);
                P.Patrols[0].LeaderType = (TNH_EnemyType)Enum.Parse(typeof(TNH_EnemyType), leaderType, true);

                //Allow for an IFF to be 0 instead of 1
                if (allowFriendlyPatrols.Value && patrolTeams.Value >= 2)
                {
                    team = team - 1;
                }
                P.Patrols[0].IFFUsed = team;
                
            }
            
            Debug.Log("TNHTWEAKER -- LISTING CURRENT PATROLS AVAILABLE:");
            PrintPatrolList(P);

            return true;
        }

        public static void PrintPatrolList(TNH_PatrolChallenge patrolChallenge)
        {
            foreach(TNH_PatrolChallenge.Patrol patrol in patrolChallenge.Patrols)
            {
                Debug.Log("TNHTWEAKER -- Patrol:");
                Debug.Log("MaxPatrols = " + patrol.MaxPatrols);
                Debug.Log("PatrolSize = " + patrol.PatrolSize);
                Debug.Log("TimeTilRegen = " + patrol.TimeTilRegen);
                Debug.Log("IFF = " + patrol.IFFUsed);
                Debug.Log("EnemyType = " + patrol.EnemyType);
                Debug.Log("LeaderType = " + patrol.LeaderType);
            }
        }

        
        [HarmonyPatch(typeof(TNH_HoldPoint), "BeginHoldChallenge")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool BeginHoldChallengeBefore()
        {
            Debug.Log("TNHTWEAKER -- BEGINNING HOLD");

            return true;
        }


        [HarmonyPatch(typeof(TNH_Manager), "SetPhase_Take")] // Specify target method with HarmonyPatch attribute
        [HarmonyPostfix]
        public static void AfterSetTake(List<TNH_SupplyPoint> ___SupplyPoints, TNH_Progression.Level ___m_curLevel, TAH_Reticle ___TAHReticle, int ___m_level, TNH_CharacterDef ___C)
        {
            Debug.Log("TNHTWEAKER -- ADDING ADDITIONAL SUPPLY POINTS");

            CustomCharData characterData;
            if(customCharDict.TryGetValue(___C, out characterData))
            {
                if(characterData.AdditionalSupplyPoints.Count > ___m_level)
                {

                    List<TNH_SupplyPoint> possiblePoints = new List<TNH_SupplyPoint>(___SupplyPoints);
                    possiblePoints.Remove(___SupplyPoints[GetClosestSupplyPointIndex(___SupplyPoints, GM.CurrentPlayerBody.Head.position)]);

                    foreach(TNH_SupplyPoint point in ___SupplyPoints)
                    {
                        if((int)Traverse.Create(point).Field("m_activeSosigs").Property("Count").GetValue() > 0)
                        {
                            Debug.Log("TNHTWEAKER -- FOUND ALREADY POPULATED POINT");
                            possiblePoints.Remove(point);
                        }
                    }

                    possiblePoints.Shuffle();

                    //Now that we have a list of valid points, set up some of those points
                    for(int i = 0; i < characterData.AdditionalSupplyPoints[___m_level] && i < possiblePoints.Count; i++)
                    {
                        TNH_SupplyPoint.SupplyPanelType panelType = TNH_SupplyPoint.SupplyPanelType.GunRecycler;
                        possiblePoints[i].Configure(___m_curLevel.SupplyChallenge, true, true, true, panelType, 1, 2);
                        TAH_ReticleContact contact = ___TAHReticle.RegisterTrackedObject(possiblePoints[i].SpawnPoint_PlayerSpawn, TAH_ReticleContact.ContactType.Supply);
                        possiblePoints[i].SetContact(contact);

                        Debug.Log("TNHTWEAKER -- GENERATED AN ADDITIONAL SUPPLY POINT");
                    }
                }
            }
        }


        [HarmonyPatch(typeof(TNH_HoldPoint), "SpawnTakeEnemyGroup")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool SpawnTakeGroupReplacement(List<Transform> ___SpawnPoints_Sosigs_Defense, TNH_TakeChallenge ___T, TNH_Manager ___M, List<Sosig> ___m_activeSosigs)
        {
            ___SpawnPoints_Sosigs_Defense.Shuffle<Transform>();

            for(int i = 0; i < ___T.NumGuards && i < ___SpawnPoints_Sosigs_Defense.Count; i++)
            {
                Transform transform = ___SpawnPoints_Sosigs_Defense[i];
                SosigEnemyTemplate template = ___M.GetEnemyTemplate(___T.GuardType);
                Sosig enemy = ___M.SpawnEnemy(template, transform, ___T.IFFUsed, false, transform.position, true);
                ___m_activeSosigs.Add(enemy);
            }

            return false;
        }



        [HarmonyPatch(typeof(TNH_HoldPoint), "SpawnTurrets")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool SpawnTurretsReplacement(List<Transform> ___SpawnPoints_Turrets, TNH_TakeChallenge ___T, TNH_Manager ___M, List<AutoMeater> ___m_activeTurrets)
        {
            ___SpawnPoints_Turrets.Shuffle<Transform>();
            FVRObject turretPrefab = ___M.GetTurretPrefab(___T.TurretType);

            for (int i = 0; i < ___T.NumTurrets && i < ___SpawnPoints_Turrets.Count; i++)
            {
                Vector3 pos = ___SpawnPoints_Turrets[i].position + Vector3.up * 0.25f;
                AutoMeater turret = Instantiate<GameObject>(turretPrefab.GetGameObject(), pos, ___SpawnPoints_Turrets[i].rotation).GetComponent<AutoMeater>();
                ___m_activeTurrets.Add(turret);
            }

            return false;
        }




        [HarmonyPatch(typeof(TNH_SupplyPoint), "SpawnTakeEnemyGroup")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool SpawnSupplyGroupReplacement(List<Transform> ___SpawnPoints_Sosigs_Defense, TNH_TakeChallenge ___T, TNH_Manager ___M, List<Sosig> ___m_activeSosigs)
        {
            ___SpawnPoints_Sosigs_Defense.Shuffle<Transform>();

            for (int i = 0; i < ___T.NumGuards && i < ___SpawnPoints_Sosigs_Defense.Count; i++)
            {
                Transform transform = ___SpawnPoints_Sosigs_Defense[i];
                SosigEnemyTemplate template = ___M.GetEnemyTemplate(___T.GuardType);
                Sosig enemy = ___M.SpawnEnemy(template, transform, ___T.IFFUsed, false, transform.position, true);
                ___m_activeSosigs.Add(enemy);
            }

            return false;
        }




        [HarmonyPatch(typeof(TNH_SupplyPoint), "SpawnDefenses")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool SpawnSupplyTurretsReplacement(List<Transform> ___SpawnPoints_Turrets, TNH_TakeChallenge ___T, TNH_Manager ___M, List<AutoMeater> ___m_activeTurrets)
        {
            ___SpawnPoints_Turrets.Shuffle<Transform>();
            FVRObject turretPrefab = ___M.GetTurretPrefab(___T.TurretType);

            for (int i = 0; i < ___T.NumTurrets && i < ___SpawnPoints_Turrets.Count; i++)
            {
                Vector3 pos = ___SpawnPoints_Turrets[i].position + Vector3.up * 0.25f;
                AutoMeater turret = Instantiate<GameObject>(turretPrefab.GetGameObject(), pos, ___SpawnPoints_Turrets[i].rotation).GetComponent<AutoMeater>();
                ___m_activeTurrets.Add(turret);
            }

            return false;
        }

        



        
        [HarmonyPatch(typeof(TNH_HoldPoint), "SpawnHoldEnemyGroup")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool SpawnGrenadesDuringHold(List<TNH_HoldPoint.AttackVector> ___AttackVectors, TNH_Manager ___M, int ___m_phaseIndex)
        {

            CustomCharData characterData;
            if (customCharDict.TryGetValue(___M.C, out characterData))
            {

                Debug.Log("TNHTWEAKER -- SPAWNING A WAVE");


                int currLevel = (int)Traverse.Create(___M).Field("m_level").GetValue();

                if (characterData.Levels.Count > currLevel)
                {
                    if(characterData.Levels[currLevel].Phases.Count > ___m_phaseIndex)
                    {
                        float grenadeChance = characterData.Levels[currLevel].Phases[___m_phaseIndex].GrenadeChance;
                        string grenadeType = characterData.Levels[currLevel].Phases[___m_phaseIndex].GrenadeType;
                        
                        if(grenadeChance >= UnityEngine.Random.Range(0f, 1f))
                        {
                            Debug.Log("TNHTWEAKER -- THROWING A GRENADE");

                            //Get a random grenade vector to spawn a grenade at
                            TNH_HoldPoint.AttackVector randAttackVector = ___AttackVectors[UnityEngine.Random.Range(0, ___AttackVectors.Count)];

                            //Instantiate the grenade object
                            GameObject grenadeObject = Instantiate(IM.OD[grenadeType].GetGameObject(), randAttackVector.GrenadeVector.position, randAttackVector.GrenadeVector.rotation);

                            //Give the grenade an initial velocity based on the grenade vector
                            grenadeObject.GetComponent<Rigidbody>().velocity = randAttackVector.GrenadeVelRange.y * randAttackVector.GrenadeVector.forward;
                            grenadeObject.GetComponent<SosigWeapon>().FuseGrenade();
                        }
                    }
                }
            }

            return true;
        }
        



        public static int GetClosestSupplyPointIndex(List<TNH_SupplyPoint> SupplyPoints, Vector3 playerPosition)
        {
            float minDist = 999999999f;
            int minIndex = 0;

            for (int i = 0; i < SupplyPoints.Count; i++)
            {
                float dist = Vector3.Distance(SupplyPoints[i].SpawnPoint_PlayerSpawn.position, playerPosition);
                if(dist < minDist)
                {
                    minDist = dist;
                    minIndex = i;
                }
            }

            return minIndex;
        }

    }
}
