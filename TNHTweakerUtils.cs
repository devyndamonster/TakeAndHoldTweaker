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
    class TNHTweakerUtils
    {

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



        public static void PrintPatrolList(TNH_PatrolChallenge patrolChallenge)
        {
            foreach (TNH_PatrolChallenge.Patrol patrol in patrolChallenge.Patrols)
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
            foreach (string id in def.IDOverride)
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

    }

}
