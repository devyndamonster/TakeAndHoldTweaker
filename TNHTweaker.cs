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
using BepInEx.Logging;
using System.IO.IsolatedStorage;
using Valve.Newtonsoft.Json;

namespace FistVR
{
    [BepInPlugin("org.bebinex.plugins.tnhtweaker", "A plugin for tweaking tnh parameters", "1.0.0.0")]
    public class TNHTweaker : BaseUnityPlugin
    {

        private static ConfigEntry<bool> printCharacters;
        private static ConfigEntry<bool> logPatrols;
        private static ConfigEntry<bool> logFileReads;
        private static ConfigEntry<bool> allowLog;
        private static ConfigEntry<bool> cacheCompatibleMagazines;

        private static Dictionary<string, Sprite> equipmentIcons = new Dictionary<string, Sprite>();

        private static string characterPath;

        private static Dictionary<TNH_CharacterDef,CustomCharacter> customCharDict = new Dictionary<TNH_CharacterDef,CustomCharacter>();
        private static Dictionary<SosigEnemyTemplate, SosigTemplate> customSosigs = new Dictionary<SosigEnemyTemplate, SosigTemplate>();

        private static List<int> spawnedBossIndexes = new List<int>();

        private static bool filesBuilt = false;
        private static bool preventOutfitFunctionality = false;

        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(TNHTweaker));

            LoadConfigFile();

            SetupCharacterDirectory();
        }

        private void LoadConfigFile()
        {
            Debug.Log("TNHTWEAKER -- GETTING CONFIG FILE");

            cacheCompatibleMagazines = Config.Bind("General",
                                    "CacheCompatibleMagazines",
                                    false,
                                    "If true, guns will be able to spawn with any compatible mag in TNH (Eg. by default the VSS cannot spawn with 30rnd magazines)");

            allowLog = Config.Bind("Debug",
                                    "EnableLogging",
                                    false,
                                    "Set to true to enable logging");

            printCharacters = Config.Bind("Debug",
                                         "PrintCharacterInfo",
                                         false,
                                         "Decide if should print all character info");

            logPatrols = Config.Bind("Debug",
                                    "LogPatrolSpawns",
                                    false,
                                    "If true, patrols that spawn will have log output");

            logFileReads = Config.Bind("Debug",
                                    "LogFileReads",
                                    false,
                                    "If true, reading from a file will log the reading process");

            TNHTweakerLogger.LogGeneral = allowLog.Value;
            TNHTweakerLogger.LogCharacter = printCharacters.Value;
            TNHTweakerLogger.LogPatrol = logPatrols.Value;
            TNHTweakerLogger.LogFile = logFileReads.Value;

        }

        private void SetupCharacterDirectory()
        {
            characterPath = Application.dataPath.Replace("/h3vr_Data", "/CustomCharacters");
            TNHTweakerLogger.Log("TNHTWEAKER -- CHARACTER FILE PATH IS: " + characterPath, TNHTweakerLogger.LogType.Character);

            if (Directory.Exists(characterPath))
            {
                TNHTweakerLogger.Log("Folder exists!", TNHTweakerLogger.LogType.Character);
            }
            else
            {
                TNHTweakerLogger.Log("Folder does not exist! Creating", TNHTweakerLogger.LogType.Character);
                Directory.CreateDirectory(characterPath);
            }
        }


        [HarmonyPatch(typeof(TNH_UIManager), "Start")] // Specify target method with HarmonyPatch attribute
        [HarmonyPostfix]
        public static void AfterTNHMenuLoaded()
        {
            if (!filesBuilt)
            {
                if (cacheCompatibleMagazines.Value)
                {
                    TNHTweakerUtils.LoadMagazineCache(characterPath);
                }

                TNHTweakerUtils.CreateObjectIDFile(characterPath);
            }

            filesBuilt = true;
        }


        [HarmonyPatch(typeof(TNH_UIManager), "Start")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool AddCharacters(List<TNH_UIManager.CharacterCategory> ___Categories, TNH_CharacterDatabase ___CharDatabase)
        {
            GM.TNHOptions.Char = TNH_Char.DD_ClassicLoudoutLouis;

            if (!filesBuilt)
            {
                TNHTweakerUtils.CreateSosigIDFile(characterPath);
                TNHTweakerUtils.CreateDefaultSosigTemplateFiles(characterPath);
                TNHTweakerUtils.CreateDefaultCharacterFiles(___CharDatabase, characterPath);
                equipmentIcons = TNHTweakerUtils.GetAllIcons(___CharDatabase);
                TNHTweakerUtils.CreateIconIDFile(characterPath, equipmentIcons.Keys.ToList());
                LoadCustomCharacters(___CharDatabase.Characters);
            }
            
            foreach (TNH_CharacterDef character in customCharDict.Keys)
            {
                if (!___Categories[(int)character.Group].Characters.Contains(character.CharacterID))
                {
                    ___Categories[(int)character.Group].Characters.Add(character.CharacterID);
                    ___CharDatabase.Characters.Add(character);
                }
            }

            TNHTweakerLogger.Log("TNHTWEAKER -- CHARACTERS LOADED: " + ___CharDatabase.Characters.Count, TNHTweakerLogger.LogType.Character);

            return true;
        }



        private static void LoadCustomCharacters(List<TNH_CharacterDef> characters)
        {

            TNHTweakerLogger.Log("TNHTWEAKER -- LOADING CUSTOM CHARACTERS", TNHTweakerLogger.LogType.Character);

            string[] characterDirs = Directory.GetDirectories(characterPath);

            int ID = 30;

            //First, load all of the default sosig IDs into the sosig ID dict
            foreach(SosigEnemyID sosig in Enum.GetValues(typeof(SosigEnemyID)))
            {
                if (!SosigTemplate.SosigIDDict.ContainsKey(sosig.ToString()))
                {
                    SosigTemplate.SosigIDDict.Add(sosig.ToString(), (int)sosig);
                }
            }

            //Now load all default sosig templates into custom sosig dictionary
            foreach(SosigEnemyTemplate config in ManagerSingleton<IM>.Instance.odicSosigObjsByID.Values)
            {
                SosigTemplate customTemplate = new SosigTemplate(config);
                customSosigs.Add(config, customTemplate);
            }

            //Load the default characters into the custom character dictionary
            foreach(TNH_CharacterDef characterDef in characters)
            {
                TNHTweakerUtils.RemoveUnloadedObjectIDs(characterDef);

                CustomCharacter character = new CustomCharacter(characterDef);
                customCharDict.Add(characterDef, character);

                TNHTweakerLogger.Log("TNHTWEAKER -- CHARACTER LOADED: " + character.DisplayName, TNHTweakerLogger.LogType.Character);
            }

            //Now load custom characters from the directory
            foreach (string characterDir in characterDirs)
            {
                if (!File.Exists(characterDir + "/thumb.png") || !File.Exists(characterDir + "/character.json"))
                {
                    Debug.LogError("TNHTWEAKER -- CHARACTER DIRECTORY FOUND, BUT MISSING ONE OR MORE OF THE FOLLOWING: character.json , thumb.png");
                    continue;
                }

                //Load all of the custom sosig templates for this character
                foreach (string file in Directory.GetFiles(characterDir))
                {
                    if (file.Contains("sosig_"))
                    {
                        string sosigJson = File.ReadAllText(file);
                        SosigTemplate template = JsonConvert.DeserializeObject<SosigTemplate>(sosigJson);
                        TNHTweakerLogger.Log("TNHTWEAKER -- DESERIALIZED SOSIG TEMPLATE", TNHTweakerLogger.LogType.Character);

                        if (SosigTemplate.SosigIDDict.ContainsKey(template.SosigEnemyID))
                        {
                            TNHTweakerLogger.Log("TNHTWEAKER -- SOSIG TEMPLATE ALREADY EXISTS! SKIPPING", TNHTweakerLogger.LogType.Character);
                            continue;
                        }

                        SosigEnemyTemplate enemyTemplate = template.GetSosigEnemyTemplate();
                        ManagerSingleton<IM>.Instance.odicSosigObjsByID.Add(enemyTemplate.SosigEnemyID, enemyTemplate);

                        if(template.DroppedObjectPool != null)
                        {
                            TNHTweakerUtils.RemoveUnloadedObjectIDs(template.DroppedObjectPool.GetObjectTable());
                            template.TableDef.Initialize(template.DroppedObjectPool.GetObjectTable());
                        }
                       
                        customSosigs.Add(enemyTemplate, template);
                        TNHTweakerLogger.Log("TNHTWEAKER -- ADDED SOSIG ID (" + enemyTemplate.SosigEnemyID + ") FOR SOSIG (" + template.SosigEnemyID + ")", TNHTweakerLogger.LogType.Character);
                    }
                }

                //Load the character itself
                string json = File.ReadAllText(characterDir + "/character.json");
                CustomCharacter character = JsonConvert.DeserializeObject<CustomCharacter>(json);
                TNHTweakerLogger.Log("TNHTWEAKER -- DESERIALIZED", TNHTweakerLogger.LogType.Character);
                TNH_CharacterDef characterDef = character.GetCharacter(ID, characterDir, equipmentIcons);
                TNHTweakerLogger.Log("TNHTWEAKER -- CONVERTED", TNHTweakerLogger.LogType.Character);
                TNHTweakerUtils.RemoveUnloadedObjectIDs(characterDef);
                customCharDict.Add(characterDef, character);

                ID += 1;
                TNHTweakerLogger.Log("TNHTWEAKER -- CHARACTER LOADED: " + character.DisplayName, TNHTweakerLogger.LogType.Character);
            }
        }


        [HarmonyPatch(typeof(TNH_Manager), "InitTables")] // Specify target method with HarmonyPatch attribute
        [HarmonyPostfix]
        public static void PrintGenerateTables(Dictionary<ObjectTableDef, ObjectTable> ___m_objectTableDics)
        {
            try
            {
                string path = characterPath + "/pool_contents.txt";

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                // Create a new file     
                using (StreamWriter sw = File.CreateText(path))
                {
                    foreach (KeyValuePair<ObjectTableDef, ObjectTable> pool in ___m_objectTableDics)
                    {
                        sw.WriteLine("Pool: " + pool.Key.Icon.name);
                        foreach(FVRObject obj in pool.Value.Objs)
                        {
                            if(obj == null)
                            {
                                TNHTweakerLogger.Log("TNHTWEAKER -- NULL OBJECT IN TABLE", TNHTweakerLogger.LogType.Character);
                                continue;
                            }
                            sw.WriteLine("-" + obj.ItemID);
                        }
                        sw.WriteLine("");
                    }
                }
            }

            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }

        }

        private static int GetValidPatrolIndex(List<TNH_PatrolChallenge.Patrol> patrols)
        {
            int index = UnityEngine.Random.Range(0, patrols.Count);
            int attempts = 0;

            while(spawnedBossIndexes.Contains(index) && attempts < patrols.Count)
            {
                index += 1;
                if (index >= patrols.Count) index = 0;
            }

            if (spawnedBossIndexes.Contains(index)) return -1;

            return index;
        }


        [HarmonyPatch(typeof(TNH_Manager), "GenerateValidPatrol")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool GenerateValidPatrolReplacement(TNH_PatrolChallenge P, int curStandardIndex, int excludeHoldIndex, bool isStart, TNH_Manager __instance, TNH_Progression.Level ___m_curLevel, List<TNH_Manager.SosigPatrolSquad> ___m_patrolSquads, ref float ___m_timeTilPatrolCanSpawn)
        {
            TNHTweakerLogger.Log("TNHTWEAKER -- GENERATING A PATROL -- THERE ARE CURRENTLY " + ___m_patrolSquads.Count + " PATROLS ACTIVE", TNHTweakerLogger.LogType.Patrol);

            if (P.Patrols.Count < 1) return false;

            //Get a valid patrol index, and exit if there are no valid patrols
            int patrolIndex = GetValidPatrolIndex(P.Patrols);
            if(patrolIndex == -1)
            {
                TNHTweakerLogger.Log("TNHTWEAKER -- NO VALID PATROLS", TNHTweakerLogger.LogType.Patrol);
                ___m_timeTilPatrolCanSpawn = 999;
                return false;
            }

            TNHTweakerLogger.Log("TNHTWEAKER -- VALID PATROL FOUND", TNHTweakerLogger.LogType.Patrol);

            TNH_PatrolChallenge.Patrol patrol = P.Patrols[patrolIndex];

            List<int> validLocations = new List<int>();
            float minDist = __instance.TAHReticle.Range * 1.2f;

            //Get a safe starting point for the patrol to spawn
            TNH_SafePositionMatrix.PositionEntry startingEntry;
            if (isStart) startingEntry = __instance.SafePosMatrix.Entries_SupplyPoints[curStandardIndex];
            else startingEntry = __instance.SafePosMatrix.Entries_HoldPoints[curStandardIndex];


            for(int i = 0; i < startingEntry.SafePositions_HoldPoints.Count; i++)
            {
                if(i != excludeHoldIndex && startingEntry.SafePositions_HoldPoints[i])
                {
                    float playerDist = Vector3.Distance(GM.CurrentPlayerBody.transform.position, __instance.HoldPoints[i].transform.position);
                    if(playerDist > minDist)
                    {
                        validLocations.Add(i);
                    }
                }
            }

            if (validLocations.Count < 1) return false;
            validLocations.Shuffle();

            TNH_Manager.SosigPatrolSquad squad = GeneratePatrol(validLocations[0], __instance, ___m_curLevel, patrol, patrolIndex);

            if(__instance.EquipmentMode == TNHSetting_EquipmentMode.Spawnlocking)
            {
                ___m_timeTilPatrolCanSpawn = patrol.TimeTilRegen;
            }
            else
            {
                ___m_timeTilPatrolCanSpawn = patrol.TimeTilRegen_LimitedAmmo;
            }

            ___m_patrolSquads.Add(squad);

            return false;
        }

        
        public static TNH_Manager.SosigPatrolSquad GeneratePatrol(int HoldPointStart, TNH_Manager instance, TNH_Progression.Level level, TNH_PatrolChallenge.Patrol patrol, int patrolIndex)
        {
            TNH_Manager.SosigPatrolSquad squad = new TNH_Manager.SosigPatrolSquad();

            squad.PatrolPoints = new List<Vector3>();
            foreach(TNH_HoldPoint holdPoint in instance.HoldPoints)
            {
                squad.PatrolPoints.Add(holdPoint.SpawnPoints_Sosigs_Defense.GetRandom<Transform>().position);
            }

            Vector3 startingPoint = squad.PatrolPoints[HoldPointStart];
            squad.PatrolPoints.RemoveAt(HoldPointStart);
            squad.PatrolPoints.Insert(0, startingPoint);

            int PatrolSize = Mathf.Clamp(patrol.PatrolSize, 0, instance.HoldPoints[HoldPointStart].SpawnPoints_Sosigs_Defense.Count);

            CustomCharacter character = customCharDict[instance.C];
            Level currLevel = character.GetCurrentLevel(level);
            Patrol currPatrol = currLevel.GetPatrol(patrol);

            TNHTweakerLogger.Log("TNHTWEAKER -- IS PATROL BOSS: " + currPatrol.IsBoss, TNHTweakerLogger.LogType.Patrol);

            for (int i = 0; i < PatrolSize; i++)
            {
                SosigEnemyTemplate template;
                bool allowAllWeapons;

                //If this is a boss, then we can only spawn it once, so add it to the list of spawned bosses
                if (currPatrol.IsBoss)
                {
                    spawnedBossIndexes.Add(patrolIndex);
                }

                //Select a sosig template from the custom character patrol
                if (i == 0)
                {
                    template = ManagerSingleton<IM>.Instance.odicSosigObjsByID[(SosigEnemyID)SosigTemplate.SosigIDDict[currPatrol.LeaderType]];
                    allowAllWeapons = true;
                }

                else
                {
                    template = ManagerSingleton<IM>.Instance.odicSosigObjsByID[(SosigEnemyID)SosigTemplate.SosigIDDict[currPatrol.EnemyType.GetRandom<string>()]];
                    allowAllWeapons = false;
                }


                SosigTemplate customTemplate = customSosigs[template];
                FVRObject droppedObject = instance.Prefab_HealthPickupMinor;

                //If squad is set to swarm, the first point they path to should be the players current position
                Sosig sosig;
                if (currPatrol.SwarmPlayer)
                {
                    squad.PatrolPoints[0] = GM.CurrentPlayerBody.transform.position;
                    sosig = SpawnEnemy(customTemplate, character, instance.HoldPoints[HoldPointStart].SpawnPoints_Sosigs_Defense[i], instance.AI_Difficulty, currPatrol.IFFUsed, true, squad.PatrolPoints[0], allowAllWeapons);
                    sosig.SetAssaultSpeed(currPatrol.AssualtSpeed);
                }
                else
                {
                    sosig = SpawnEnemy(customTemplate, character, instance.HoldPoints[HoldPointStart].SpawnPoints_Sosigs_Defense[i], instance.AI_Difficulty, currPatrol.IFFUsed, true, squad.PatrolPoints[0], allowAllWeapons);
                    sosig.SetAssaultSpeed(currPatrol.AssualtSpeed);
                }

                //Handle patrols dropping health
                if(i == 0 && UnityEngine.Random.value < currPatrol.DropChance)
                {
                    sosig.Links[1].RegisterSpawnOnDestroy(droppedObject);
                }

                //Handle sosig dropping custom loot
                if (UnityEngine.Random.value < customTemplate.DroppedLootChance)
                {
                    sosig.Links[2].RegisterSpawnOnDestroy(customTemplate.TableDef.GetRandomObject());
                }

                squad.Squad.Add(sosig);
            }


            return squad;
        }


        [HarmonyPatch(typeof(TNH_Manager), "SetPhase_Take")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static void BeforeSetTake(TNH_CharacterDef ___C)
        {
            spawnedBossIndexes.Clear();
            preventOutfitFunctionality = customCharDict[___C].ForceDisableOutfitFunctionality;
        }


        [HarmonyPatch(typeof(TNH_Manager), "SetPhase_Take")] // Specify target method with HarmonyPatch attribute
        [HarmonyPostfix]
        public static void AfterSetTake(List<TNH_SupplyPoint> ___SupplyPoints, TNH_Progression.Level ___m_curLevel, TAH_Reticle ___TAHReticle, int ___m_level, TNH_CharacterDef ___C)
        {
            TNHTweakerLogger.Log("TNHTWEAKER -- ADDING ADDITIONAL SUPPLY POINTS", TNHTweakerLogger.LogType.General);

            CustomCharacter character = customCharDict[___C];
            Level currLevel = character.GetCurrentLevel(___m_curLevel);

            List <TNH_SupplyPoint> possiblePoints = new List<TNH_SupplyPoint>(___SupplyPoints);
            possiblePoints.Remove(___SupplyPoints[GetClosestSupplyPointIndex(___SupplyPoints, GM.CurrentPlayerBody.Head.position)]);

            foreach(TNH_SupplyPoint point in ___SupplyPoints)
            {
                if((int)Traverse.Create(point).Field("m_activeSosigs").Property("Count").GetValue() > 0)
                {
                    TNHTweakerLogger.Log("TNHTWEAKER -- FOUND ALREADY POPULATED POINT", TNHTweakerLogger.LogType.General);
                    possiblePoints.Remove(point);
                }
            }

            possiblePoints.Shuffle();

            //Now that we have a list of valid points, set up some of those points
            for(int i = 0; i < currLevel.AdditionalSupplyPoints && i < possiblePoints.Count; i++)
            {
                TNH_SupplyPoint.SupplyPanelType panelType = (TNH_SupplyPoint.SupplyPanelType)UnityEngine.Random.Range(1,3);
                        
                possiblePoints[i].Configure(___m_curLevel.SupplyChallenge, true, true, true, panelType, 1, 2);
                TAH_ReticleContact contact = ___TAHReticle.RegisterTrackedObject(possiblePoints[i].SpawnPoint_PlayerSpawn, TAH_ReticleContact.ContactType.Supply);
                possiblePoints[i].SetContact(contact);

                TNHTweakerLogger.Log("TNHTWEAKER -- GENERATED AN ADDITIONAL SUPPLY POINT", TNHTweakerLogger.LogType.General);
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
                SosigEnemyTemplate template = ManagerSingleton<IM>.Instance.odicSosigObjsByID[___T.GID];
                SosigTemplate customTemplate = customSosigs[template];

                TNHTweakerLogger.Log("TNHTWEAKER -- SPAWNING TAKE GROUP AT " + transform.position, TNHTweakerLogger.LogType.Patrol);

                Sosig enemy = SpawnEnemy(customTemplate, customCharDict[___M.C], transform, ___M.AI_Difficulty, ___T.IFFUsed, false, transform.position, true);

                //Handle sosig dropping custom loot
                if (UnityEngine.Random.value < customTemplate.DroppedLootChance)
                {
                    enemy.Links[2].RegisterSpawnOnDestroy(customTemplate.TableDef.GetRandomObject());
                }

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
                SosigEnemyTemplate template = ManagerSingleton<IM>.Instance.odicSosigObjsByID[___T.GID];
                SosigTemplate customTemplate = customSosigs[template];

                TNHTweakerLogger.Log("TNHTWEAKER -- SPAWNING SUPPLY GROUP AT " + transform.position, TNHTweakerLogger.LogType.Patrol);

                Sosig enemy = SpawnEnemy(customTemplate, customCharDict[___M.C], transform, ___M.AI_Difficulty, ___T.IFFUsed, false, transform.position, true);

                //Handle sosig dropping custom loot
                if (UnityEngine.Random.value < customTemplate.DroppedLootChance)
                {
                    enemy.Links[2].RegisterSpawnOnDestroy(customTemplate.TableDef.GetRandomObject());
                }

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




        [HarmonyPatch(typeof(TNH_HoldPoint), "IdentifyEncryption")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool SpawnEncryptionReplacement(TNH_HoldPoint __instance, TNH_HoldChallenge.Phase ___m_curPhase)
        {
            if(___m_curPhase.MaxTargets <= 0)
            {
                Traverse.Create(__instance).Method("CompletePhase").GetValue();
                return false;
            }

            return true;
        }


        public static Sosig SpawnEnemy(SosigTemplate template, CustomCharacter character, Transform spawnLocation, TNHModifier_AIDifficulty difficulty, int IFF, bool isAssault, Vector3 pointOfInterest, bool allowAllWeapons)
        {
            if (character.ForceAllAgentWeapons) allowAllWeapons = true;

            TNHTweakerLogger.Log("TNHTWEAKER -- SPAWNING SOSIG: " + template.SosigEnemyID, TNHTweakerLogger.LogType.Patrol);

            //Create the sosig object
            GameObject sosigPrefab = Instantiate(IM.OD[template.SosigPrefabs.GetRandom<string>()].GetGameObject(), spawnLocation.position, spawnLocation.rotation);
            Sosig sosigComponent = sosigPrefab.GetComponentInChildren<Sosig>();

            //Fill out the sosigs config based on the difficulty
            SosigConfig config;

            if (difficulty == TNHModifier_AIDifficulty.Arcade) config = template.ConfigsEasy.GetRandom<SosigConfig>();
            else config = template.Configs.GetRandom<SosigConfig>();
            sosigComponent.Configure(config.GetConfigTemplate());
            sosigComponent.E.IFFCode = IFF;

            //Setup the sosigs inventory
            sosigComponent.Inventory.Init();
            sosigComponent.Inventory.FillAllAmmo();
            sosigComponent.InitHands();

            //Equip the sosigs weapons
            if(template.WeaponOptions.Count > 0)
            {
                GameObject weaponPrefab = IM.OD[template.WeaponOptions.GetRandom<string>()].GetGameObject();
                EquipSosigWeapon(sosigComponent, weaponPrefab, difficulty);
            }

            if (template.WeaponOptionsSecondary.Count > 0 && allowAllWeapons && template.SecondaryChance >= UnityEngine.Random.value)
            {
                GameObject weaponPrefab = IM.OD[template.WeaponOptionsSecondary.GetRandom<string>()].GetGameObject();
                EquipSosigWeapon(sosigComponent, weaponPrefab, difficulty);
            }

            if (template.WeaponOptionsTertiary.Count > 0 && allowAllWeapons && template.TertiaryChance >= UnityEngine.Random.value)
            {
                GameObject weaponPrefab = IM.OD[template.WeaponOptionsTertiary.GetRandom<string>()].GetGameObject();
                EquipSosigWeapon(sosigComponent, weaponPrefab, difficulty);
            }

            //Equip clothing to the sosig
            OutfitConfig outfitConfig = template.OutfitConfigs.GetRandom<OutfitConfig>();
            if(outfitConfig.Chance_Headwear >= UnityEngine.Random.value)
            {
                EquipSosigClothing(outfitConfig.Headwear, sosigComponent.Links[0], outfitConfig.ForceWearAllHead);
            }

            if (outfitConfig.Chance_Facewear >= UnityEngine.Random.value)
            {
                EquipSosigClothing(outfitConfig.Facewear, sosigComponent.Links[0], outfitConfig.ForceWearAllFace);
            }

            if (outfitConfig.Chance_Eyewear >= UnityEngine.Random.value)
            {
                EquipSosigClothing(outfitConfig.Eyewear, sosigComponent.Links[0], outfitConfig.ForceWearAllEye);
            }

            if (outfitConfig.Chance_Torsowear >= UnityEngine.Random.value)
            {
                EquipSosigClothing(outfitConfig.Torsowear, sosigComponent.Links[1], outfitConfig.ForceWearAllTorso);
            }

            if (outfitConfig.Chance_Pantswear >= UnityEngine.Random.value)
            {
                EquipSosigClothing(outfitConfig.Pantswear, sosigComponent.Links[2], outfitConfig.ForceWearAllPants);
            }

            if (outfitConfig.Chance_Pantswear_Lower >= UnityEngine.Random.value)
            {
                EquipSosigClothing(outfitConfig.Pantswear_Lower, sosigComponent.Links[3], outfitConfig.ForceWearAllPantsLower);
            }

            if (outfitConfig.Chance_Backpacks >= UnityEngine.Random.value)
            {
                EquipSosigClothing(outfitConfig.Backpacks, sosigComponent.Links[1], outfitConfig.ForceWearAllBackpacks);
            }

            //Setup link spawns
            if (config.GetConfigTemplate().UsesLinkSpawns)
            {
                for(int i = 0; i < sosigComponent.Links.Count; i++)
                {
                    if(config.GetConfigTemplate().LinkSpawnChance[i] >= UnityEngine.Random.value)
                    {
                        if(config.GetConfigTemplate().LinkSpawns.Count > i && config.GetConfigTemplate().LinkSpawns[i] != null && config.GetConfigTemplate().LinkSpawns[i].Category != FVRObject.ObjectCategory.Loot)
                        {
                            sosigComponent.Links[i].RegisterSpawnOnDestroy(config.GetConfigTemplate().LinkSpawns[i]);
                        }
                    }
                }
            }

            //Setup the sosigs orders
            if (isAssault)
            {
                sosigComponent.CurrentOrder = Sosig.SosigOrder.Assault;
                sosigComponent.FallbackOrder = Sosig.SosigOrder.Assault;
                sosigComponent.CommandAssaultPoint(pointOfInterest);
            }
            else
            {
                sosigComponent.CurrentOrder = Sosig.SosigOrder.Wander;
                sosigComponent.FallbackOrder = Sosig.SosigOrder.Wander;
                sosigComponent.CommandGuardPoint(pointOfInterest, true);
                sosigComponent.SetDominantGuardDirection(UnityEngine.Random.onUnitSphere);
            }
            sosigComponent.SetGuardInvestigateDistanceThreshold(25f);

            return sosigComponent;
        }

        public static void EquipSosigWeapon(Sosig sosig, GameObject weaponPrefab, TNHModifier_AIDifficulty difficulty)
        {
            SosigWeapon weapon = Instantiate(weaponPrefab, sosig.transform.position + Vector3.up * 0.1f, sosig.transform.rotation).GetComponent<SosigWeapon>();
            weapon.SetAutoDestroy(true);
            weapon.O.SpawnLockable = false;

            TNHTweakerLogger.Log("TNHTWEAKER -- EQUIPPING WEAPON: " + weapon.gameObject.name, TNHTweakerLogger.LogType.Patrol);

            //Equip the sosig weapon to the sosig
            sosig.ForceEquip(weapon);
            weapon.SetAmmoClamping(true);
            if (difficulty == TNHModifier_AIDifficulty.Arcade) weapon.FlightVelocityMultiplier = 0.3f;
        }

        public static void EquipSosigClothing(List<string> options, SosigLink link, bool wearAll)
        {
            if (wearAll)
            {
                foreach(string clothing in options)
                {
                    GameObject clothingObject = Instantiate(IM.OD[clothing].GetGameObject(), link.transform.position, link.transform.rotation);
                    clothingObject.transform.SetParent(link.transform);
                    clothingObject.GetComponent<SosigWearable>().RegisterWearable(link);
                }
            }

            else
            {
                GameObject clothingObject = Instantiate(IM.OD[options.GetRandom<string>()].GetGameObject(), link.transform.position, link.transform.rotation);
                clothingObject.transform.SetParent(link.transform);
                clothingObject.GetComponent<SosigWearable>().RegisterWearable(link);
            }
        }


        [HarmonyPatch(typeof(TNH_SupplyPoint), "SpawnBoxes")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool SpawnBoxesReplacement(TNH_SupplyPoint __instance, TNH_Manager ___M, List<GameObject> ___m_spawnBoxes)
        {

            CustomCharacter character = customCharDict[___M.C];
            Level currLevel = character.GetCurrentLevel((TNH_Progression.Level)Traverse.Create(___M).Field("m_curLevel").GetValue());

            __instance.SpawnPoints_Boxes.Shuffle();

            int boxesToSpawn = UnityEngine.Random.Range(currLevel.MinBoxesSpawned, currLevel.MaxBoxesSpawned + 1);

            TNHTweakerLogger.Log("TNHTWEAKER -- GOING TO SPAWN " + boxesToSpawn + " BOXES AT THIS SUPPLY POINT -- MIN (" + currLevel.MinBoxesSpawned + "), MAX (" + currLevel.MaxBoxesSpawned + ")", TNHTweakerLogger.LogType.General);

            for (int i = 0; i < boxesToSpawn; i++)
            {
                Transform spawnTransform = __instance.SpawnPoints_Boxes[UnityEngine.Random.Range(0, __instance.SpawnPoints_Boxes.Count)];
                Vector3 position = spawnTransform.position + Vector3.up * 0.1f + Vector3.right * UnityEngine.Random.Range(-0.5f, 0.5f) + Vector3.forward * UnityEngine.Random.Range(-0.5f, 0.5f);
                Quaternion rotation = Quaternion.Slerp(spawnTransform.rotation, UnityEngine.Random.rotation, 0.1f);
                GameObject box = Instantiate(___M.Prefabs_ShatterableCrates[UnityEngine.Random.Range(0, ___M.Prefabs_ShatterableCrates.Count)], position, rotation);
                ___m_spawnBoxes.Add(box);
                TNHTweakerLogger.Log("TNHTWEAKER -- BOX SPAWNED", TNHTweakerLogger.LogType.General);
            }

            int tokensSpawned = 0;

            foreach(GameObject boxObj in ___m_spawnBoxes)
            {
                if(tokensSpawned < currLevel.MinTokensPerSupply)
                {
                    boxObj.GetComponent<TNH_ShatterableCrate>().SetHoldingToken(___M);
                    tokensSpawned += 1;
                }

                else if (tokensSpawned < currLevel.MaxTokensPerSupply && UnityEngine.Random.value < currLevel.BoxTokenChance)
                {
                    boxObj.GetComponent<TNH_ShatterableCrate>().SetHoldingToken(___M);
                    tokensSpawned += 1;
                }

                else if (UnityEngine.Random.value < currLevel.BoxHealthChance)
                {
                    boxObj.GetComponent<TNH_ShatterableCrate>().SetHoldingHealth(___M);
                }
            }

            return false;
                
        }


        public static void SpawnGrenades(List<TNH_HoldPoint.AttackVector> AttackVectors, TNH_Manager M, int m_phaseIndex)
        {
            CustomCharacter character = customCharDict[M.C];
            Level currLevel = character.GetCurrentLevel((TNH_Progression.Level)Traverse.Create(M).Field("m_curLevel").GetValue());
            Phase currPhase = currLevel.HoldPhases[m_phaseIndex];

            float grenadeChance = currPhase.GrenadeChance;
            string grenadeType = currPhase.GrenadeType;
                        
            if(grenadeChance >= UnityEngine.Random.Range(0f, 1f))
            {
                TNHTweakerLogger.Log("TNHTWEAKER -- THROWING A GRENADE ", TNHTweakerLogger.LogType.General);

                //Get a random grenade vector to spawn a grenade at
                TNH_HoldPoint.AttackVector randAttackVector = AttackVectors[UnityEngine.Random.Range(0, AttackVectors.Count)];

                //Instantiate the grenade object
                GameObject grenadeObject = Instantiate(IM.OD[grenadeType].GetGameObject(), randAttackVector.GrenadeVector.position, randAttackVector.GrenadeVector.rotation);

                //Give the grenade an initial velocity based on the grenade vector
                grenadeObject.GetComponent<Rigidbody>().velocity = 15 * randAttackVector.GrenadeVector.forward;
                grenadeObject.GetComponent<SosigWeapon>().FuseGrenade();
            }
        }

        public static void SpawnHoldEnemyGroup(TNH_HoldChallenge.Phase curPhase, int phaseIndex, List<TNH_HoldPoint.AttackVector> AttackVectors, List<Transform> SpawnPoints_Turrets, List<Sosig> ActiveSosigs, TNH_Manager M, ref bool isFirstWave)
        {
            TNHTweakerLogger.Log("TNHTWEAKER -- SPAWNING AN ENEMY WAVE", TNHTweakerLogger.LogType.General);

            //TODO add custom property form MinDirections
            int numAttackVectors = UnityEngine.Random.Range(1, curPhase.MaxDirections + 1);
            numAttackVectors = Mathf.Clamp(numAttackVectors, 1, AttackVectors.Count);

            //Get the custom character data
            CustomCharacter character = customCharDict[M.C];
            Level currLevel = character.GetCurrentLevel((TNH_Progression.Level)Traverse.Create(M).Field("m_curLevel").GetValue());
            Phase currPhase = currLevel.HoldPhases[phaseIndex];

            //Set first enemy to be spawned as leader
            SosigEnemyTemplate enemyTemplate = ManagerSingleton<IM>.Instance.odicSosigObjsByID[(SosigEnemyID)SosigTemplate.SosigIDDict[currPhase.LeaderType]];
            int enemiesToSpawn = UnityEngine.Random.Range(curPhase.MinEnemies, curPhase.MaxEnemies + 1);

            int sosigsSpawned = 0;
            int vectorSpawnPoint = 0;
            Vector3 targetVector;
            int vectorIndex = 0;
            while(sosigsSpawned < enemiesToSpawn)
            {
                TNHTweakerLogger.Log("TNHTWEAKER -- SPAWNING AT ATTACK VECTOR: " + vectorIndex, TNHTweakerLogger.LogType.General);

                if (AttackVectors[vectorIndex].SpawnPoints_Sosigs_Attack.Count <= vectorSpawnPoint) break;

                //Set the sosigs target position
                if (currPhase.SwarmPlayer)
                {
                    targetVector = GM.CurrentPlayerBody.TorsoTransform.position;
                }
                else
                {
                    targetVector = SpawnPoints_Turrets[UnityEngine.Random.Range(0, SpawnPoints_Turrets.Count)].position;
                }

                SosigTemplate customTemplate = customSosigs[enemyTemplate];

                Sosig enemy = SpawnEnemy(customTemplate, character, AttackVectors[vectorIndex].SpawnPoints_Sosigs_Attack[vectorSpawnPoint], M.AI_Difficulty, curPhase.IFFUsed, true, targetVector, true);

                //Handle sosig dropping custom loot
                if (UnityEngine.Random.value < customTemplate.DroppedLootChance)
                {
                    enemy.Links[2].RegisterSpawnOnDestroy(customTemplate.TableDef.GetRandomObject());
                }

                ActiveSosigs.Add(enemy);

                TNHTweakerLogger.Log("TNHTWEAKER -- SOSIG SPAWNED", TNHTweakerLogger.LogType.General);

                //At this point, the leader has been spawned, so always set enemy to be regulars
                enemyTemplate = ManagerSingleton<IM>.Instance.odicSosigObjsByID[(SosigEnemyID)SosigTemplate.SosigIDDict[currPhase.EnemyType.GetRandom<string>()]];
                sosigsSpawned += 1;

                vectorIndex += 1;
                if(vectorIndex >= numAttackVectors)
                {
                    vectorIndex = 0;
                    vectorSpawnPoint += 1;
                }

                
            }
            isFirstWave = false;

        }



        [HarmonyPatch(typeof(TNH_HoldPoint), "SpawningRoutineUpdate")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool SpawningUpdateReplacement(
            ref float ___m_tickDownToNextGroupSpawn,
            List<Sosig> ___m_activeSosigs,
            TNH_HoldPoint.HoldState ___m_state,
            ref bool ___m_hasThrownNadesInWave,
            List<TNH_HoldPoint.AttackVector> ___AttackVectors,
            List<Transform> ___SpawnPoints_Turrets,
            TNH_Manager ___M,
            TNH_HoldChallenge.Phase ___m_curPhase,
            int ___m_phaseIndex,
            ref bool ___m_isFirstWave)
        {

            ___m_tickDownToNextGroupSpawn -= Time.deltaTime;

            
            if (___m_activeSosigs.Count < 1)
            {
                if(___m_state == TNH_HoldPoint.HoldState.Analyzing)
                {
                    ___m_tickDownToNextGroupSpawn -= Time.deltaTime;
                }
            }

            if(!___m_hasThrownNadesInWave && ___m_tickDownToNextGroupSpawn <= 5f && !___m_isFirstWave)
            {
                SpawnGrenades(___AttackVectors, ___M, ___m_phaseIndex);
                ___m_hasThrownNadesInWave = true;
            }

            //Handle spawning of a wave if it is time
            if(___m_tickDownToNextGroupSpawn <= 0 && ___m_activeSosigs.Count + ___m_curPhase.MaxEnemies <= ___m_curPhase.MaxEnemiesAlive)
            {
                ___AttackVectors.Shuffle();

                SpawnHoldEnemyGroup(___m_curPhase, ___m_phaseIndex, ___AttackVectors, ___SpawnPoints_Turrets, ___m_activeSosigs, ___M, ref ___m_isFirstWave);
                ___m_hasThrownNadesInWave = false;
                ___m_tickDownToNextGroupSpawn = ___m_curPhase.SpawnCadence;
            }


            return false;
        }



        [HarmonyPatch(typeof(TNH_SupplyPoint), "Configure")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool PrintSupplyPoint(TNH_SupplyPoint.SupplyPanelType panelType)
        {
            TNHTweakerLogger.Log("TNHTWEAKER -- CONFIGURING SUPPLY POINT -- PANEL TYPE: " + panelType.ToString(), TNHTweakerLogger.LogType.General);
            return true;
        }


        [HarmonyPatch(typeof(FVRObject), "GetRandomAmmoObject")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool PrintCompatableMagazines(FVRObject __instance)
        {
            TNHTweakerLogger.Log("TNHTWEAKER -- COMPATABLE MAGAZINE COUNT: " + __instance.CompatibleMagazines.Count, TNHTweakerLogger.LogType.General);
            foreach(FVRObject mag in __instance.CompatibleMagazines)
            {
                TNHTweakerLogger.Log(mag.ItemID, TNHTweakerLogger.LogType.General);
            }
            return true;
        }


        [HarmonyPatch(typeof(Sosig), "BuffHealing_Invis")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool OverrideCloaking()
        {
            return !preventOutfitFunctionality;
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
