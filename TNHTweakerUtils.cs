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
    class TNHTweakerUtils
    {

        public static void PrintCharacterInfo(TNH_CharacterDef character)
        {
            TNHTweakerLogger.Log("\n\n\nTNHTWEAKER -- CHARACTER INFO", TNHTweakerLogger.LogType.Character);
            TNHTweakerLogger.Log("Name: " + character.DisplayName, TNHTweakerLogger.LogType.Character);
            TNHTweakerLogger.Log("ID: " + character.CharacterID, TNHTweakerLogger.LogType.Character);
            TNHTweakerLogger.Log("Group: " + character.Group, TNHTweakerLogger.LogType.Character);
            TNHTweakerLogger.Log("TableID: " + character.TableID, TNHTweakerLogger.LogType.Character);
            TNHTweakerLogger.Log("Starting Tokens: " + character.StartingTokens, TNHTweakerLogger.LogType.Character);
            TNHTweakerLogger.Log("Force Agent Weapons: " + character.ForceAllAgentWeapons, TNHTweakerLogger.LogType.Character);
            TNHTweakerLogger.Log("Printing Pools:", TNHTweakerLogger.LogType.Character);
            TNHTweakerLogger.Log("Num entries: " + character.EquipmentPool.Entries.Count, TNHTweakerLogger.LogType.Character);

            foreach (EquipmentPoolDef.PoolEntry entry in character.EquipmentPool.Entries)
            {
                PrintPoolEntry(entry);
            }

        }

        public static void PrintPoolEntry(EquipmentPoolDef.PoolEntry entry)
        {
            TNHTweakerLogger.Log("\n--Pool Entry--", TNHTweakerLogger.LogType.Character);
            TNHTweakerLogger.Log("EntryType: " + entry.Type, TNHTweakerLogger.LogType.Character);
            TNHTweakerLogger.Log("Token Cost: " + entry.TokenCost, TNHTweakerLogger.LogType.Character);
            TNHTweakerLogger.Log("Token Cost Limited: " + entry.TokenCost_Limited, TNHTweakerLogger.LogType.Character);
            TNHTweakerLogger.Log("Min Level Appears: " + entry.MinLevelAppears, TNHTweakerLogger.LogType.Character);
            TNHTweakerLogger.Log("Max Level Appears: " + entry.MaxLevelAppears, TNHTweakerLogger.LogType.Character);
            TNHTweakerLogger.Log("Rarity: " + entry.Rarity, TNHTweakerLogger.LogType.Character);
            TNHTweakerLogger.Log("Object Table Def:", TNHTweakerLogger.LogType.Character);
            PrintObjectTableDef(entry.TableDef);
        }



        public static void PrintPatrolList(TNH_PatrolChallenge patrolChallenge)
        {
            foreach (TNH_PatrolChallenge.Patrol patrol in patrolChallenge.Patrols)
            {
                TNHTweakerLogger.Log("TNHTWEAKER -- Patrol:", TNHTweakerLogger.LogType.Patrol);
                TNHTweakerLogger.Log("MaxPatrols = " + patrol.MaxPatrols, TNHTweakerLogger.LogType.Patrol);
                TNHTweakerLogger.Log("PatrolSize = " + patrol.PatrolSize, TNHTweakerLogger.LogType.Patrol);
                TNHTweakerLogger.Log("TimeTilRegen = " + patrol.TimeTilRegen, TNHTweakerLogger.LogType.Patrol);
                TNHTweakerLogger.Log("IFF = " + patrol.IFFUsed, TNHTweakerLogger.LogType.Patrol);
                TNHTweakerLogger.Log("EnemyType = " + patrol.EType, TNHTweakerLogger.LogType.Patrol);
                TNHTweakerLogger.Log("LeaderType = " + patrol.LType, TNHTweakerLogger.LogType.Patrol);
            }
        }

        public static void PrintObjectTableDef(ObjectTableDef def)
        {
            TNHTweakerLogger.Log(" - Category: " + def.Category, TNHTweakerLogger.LogType.Character);
            TNHTweakerLogger.Log(" - MinAmmoCapacity: " + def.MinAmmoCapacity, TNHTweakerLogger.LogType.Character);
            TNHTweakerLogger.Log(" - MaxAmmoCapacity: " + def.MaxAmmoCapacity, TNHTweakerLogger.LogType.Character);
            TNHTweakerLogger.Log(" - IsBlanked: " + def.IsBlanked, TNHTweakerLogger.LogType.Character);
            TNHTweakerLogger.Log(" - SpawnsInSmallCase: " + def.SpawnsInSmallCase, TNHTweakerLogger.LogType.Character);
            TNHTweakerLogger.Log(" - SpawnsInLargeCase: " + def.SpawnsInLargeCase, TNHTweakerLogger.LogType.Character);
            TNHTweakerLogger.Log(" - UseIDListOverride: " + def.UseIDListOverride, TNHTweakerLogger.LogType.Character);

            TNHTweakerLogger.Log(" - IDOverrides: ", TNHTweakerLogger.LogType.Character);
            foreach (string id in def.IDOverride)
            {
                TNHTweakerLogger.Log(" - - " + id, TNHTweakerLogger.LogType.Character);
            }

            TNHTweakerLogger.Log(" - Contained Objects: ", TNHTweakerLogger.LogType.Character);
            foreach (FVRObject obj in def.Objs)
            {
                TNHTweakerLogger.Log(" - - " + obj.DisplayName, TNHTweakerLogger.LogType.Character);
            }

            TNHTweakerLogger.Log(" - Eras: ", TNHTweakerLogger.LogType.Character);
            foreach (FVRObject.OTagEra item in def.Eras)
            {
                TNHTweakerLogger.Log(" - - " + item, TNHTweakerLogger.LogType.Character);
            }

            TNHTweakerLogger.Log(" - Sets: ", TNHTweakerLogger.LogType.Character);
            foreach (FVRObject.OTagSet item in def.Sets)
            {
                TNHTweakerLogger.Log(" - - " + item, TNHTweakerLogger.LogType.Character);
            }

            TNHTweakerLogger.Log(" - Sizes: ", TNHTweakerLogger.LogType.Character);
            foreach (FVRObject.OTagFirearmSize item in def.Sizes)
            {
                TNHTweakerLogger.Log(" - - " + item, TNHTweakerLogger.LogType.Character);
            }

            TNHTweakerLogger.Log(" - Actions: ", TNHTweakerLogger.LogType.Character);
            foreach (FVRObject.OTagFirearmAction item in def.Actions)
            {
                TNHTweakerLogger.Log(" - - " + item, TNHTweakerLogger.LogType.Character);
            }

            TNHTweakerLogger.Log(" - Modes: ", TNHTweakerLogger.LogType.Character);
            foreach (FVRObject.OTagFirearmFiringMode item in def.Modes)
            {
                TNHTweakerLogger.Log(" - - " + item, TNHTweakerLogger.LogType.Character);
            }

            TNHTweakerLogger.Log(" - ExludedModes: ", TNHTweakerLogger.LogType.Character);
            foreach (FVRObject.OTagFirearmFiringMode item in def.ExcludeModes)
            {
                TNHTweakerLogger.Log(" - - " + item, TNHTweakerLogger.LogType.Character);
            }

            TNHTweakerLogger.Log(" - FeedOptions: ", TNHTweakerLogger.LogType.Character);
            foreach (FVRObject.OTagFirearmFeedOption item in def.Feedoptions)
            {
                TNHTweakerLogger.Log(" - - " + item, TNHTweakerLogger.LogType.Character);
            }

            TNHTweakerLogger.Log(" - Mounts: ", TNHTweakerLogger.LogType.Character);
            foreach (FVRObject.OTagFirearmMount item in def.MountsAvailable)
            {
                TNHTweakerLogger.Log(" - - " + item, TNHTweakerLogger.LogType.Character);
            }

            TNHTweakerLogger.Log(" - RoundPowers: ", TNHTweakerLogger.LogType.Character);
            foreach (FVRObject.OTagFirearmRoundPower item in def.RoundPowers)
            {
                TNHTweakerLogger.Log(" - - " + item, TNHTweakerLogger.LogType.Character);
            }

            TNHTweakerLogger.Log(" - Features: ", TNHTweakerLogger.LogType.Character);
            foreach (FVRObject.OTagAttachmentFeature item in def.Features)
            {
                TNHTweakerLogger.Log(" - - " + item, TNHTweakerLogger.LogType.Character);
            }

            TNHTweakerLogger.Log(" - MeleeStyles: ", TNHTweakerLogger.LogType.Character);
            foreach (FVRObject.OTagMeleeStyle item in def.MeleeStyles)
            {
                TNHTweakerLogger.Log(" - - " + item, TNHTweakerLogger.LogType.Character);
            }

            TNHTweakerLogger.Log(" - MeleeHandedness: ", TNHTweakerLogger.LogType.Character);
            foreach (FVRObject.OTagMeleeHandedness item in def.MeleeHandedness)
            {
                TNHTweakerLogger.Log(" - - " + item, TNHTweakerLogger.LogType.Character);
            }

            TNHTweakerLogger.Log(" - MountTypes: ", TNHTweakerLogger.LogType.Character);
            foreach (FVRObject.OTagFirearmMount item in def.MountTypes)
            {
                TNHTweakerLogger.Log(" - - " + item, TNHTweakerLogger.LogType.Character);
            }

            TNHTweakerLogger.Log(" - PowerupTypes: ", TNHTweakerLogger.LogType.Character);
            foreach (FVRObject.OTagPowerupType item in def.PowerupTypes)
            {
                TNHTweakerLogger.Log(" - - " + item, TNHTweakerLogger.LogType.Character);
            }

            TNHTweakerLogger.Log(" - ThrownType: ", TNHTweakerLogger.LogType.Character);
            foreach (FVRObject.OTagThrownType item in def.ThrownTypes)
            {
                TNHTweakerLogger.Log(" - - " + item, TNHTweakerLogger.LogType.Character);
            }

            TNHTweakerLogger.Log(" - ThrownDamageTypes: ", TNHTweakerLogger.LogType.Character);
            foreach (FVRObject.OTagThrownDamageType item in def.ThrownDamageTypes)
            {
                TNHTweakerLogger.Log(" - - " + item, TNHTweakerLogger.LogType.Character);
            }

            TNHTweakerLogger.Log(" - End of object table", TNHTweakerLogger.LogType.Character);

        }

        /// <summary>
        /// Creates a template file for character creation. Follows info found here: https://www.c-sharpcorner.com/UploadFile/mahesh/create-a-text-file-in-C-Sharp/
        /// </summary>
        /// <param name="path"></param>
        public static void CreateTemplateFile(string path)
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
                    sw.WriteLine("      @AdditionalSupplyPoints=1");

                    //Open Take Challenge
                    sw.WriteLine("");
                    sw.WriteLine("      TakeChallenge{");
                    sw.WriteLine("          #Options: " + string.Join(",", Enum.GetNames(typeof(TNH_TurretType))));
                    sw.WriteLine("          TurretType=SMG");
                    sw.WriteLine("          NumTurrets=0");
                    sw.WriteLine("          GID=M_Swat_Scout");
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
                    sw.WriteLine("              EType=M_Swat_Scout");
                    sw.WriteLine("              LType=M_Swat_Scout");
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
                    sw.WriteLine("          GID=M_Swat_Scout");
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
                    sw.WriteLine("              EType=M_Swat_Scout");
                    sw.WriteLine("              LType=M_Swat_Scout");
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

    }

}
