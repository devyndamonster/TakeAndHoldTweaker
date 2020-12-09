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
using Deli;
using ADepIn;

namespace FistVR
{
    [BepInPlugin("org.bebinex.plugins.tnhtweaker", "A plugin for tweaking tnh parameters", "0.1.4.0")]
    public class TNHTweaker : DeliMod
    {
        private static ConfigEntry<bool> printCharacters;
        private static ConfigEntry<bool> logPatrols;
        private static ConfigEntry<bool> logFileReads;
        private static ConfigEntry<bool> allowLog;
        private static ConfigEntry<bool> cacheCompatibleMagazines;

        private static string OutputFilePath;

        private static List<int> spawnedBossIndexes = new List<int>();
        private static List<GameObject> SpawnedConstructors = new List<GameObject>();
        private static List<GameObject> SpawnedPanels = new List<GameObject>();

        private static bool filesBuilt = false;
        private static bool preventOutfitFunctionality = false;

        private void Awake()
        {
            Debug.Log("Hello World (from TNH Tweaker)");

            Harmony.CreateAndPatchAll(typeof(TNHTweaker));

            LoadConfigFile();

            SetupOutputDirectory();

            LoadPanelSprites();
        }


        private void LoadPanelSprites()
        {
            Option<Texture2D> magUpgradeContent = BaseMod.Resources.Get<Texture2D>("mag_upgrade.png");
            LoadedTemplateManager.PanelSprites.Add(PanelType.MagUpgrader, TNHTweakerUtils.LoadSprite(magUpgradeContent.Expect("TNHTweaker -- Failed to load Mag Upgrader icon!")));

            Option<Texture2D> fullAutoContent = BaseMod.Resources.Get<Texture2D>("full_auto.png");
            LoadedTemplateManager.PanelSprites.Add(PanelType.AddFullAuto, TNHTweakerUtils.LoadSprite(fullAutoContent.Expect("TNHTweaker -- Failed to load Full Auto Adder icon!")));

        }

        private void LoadConfigFile()
        {
            Debug.Log("TNHTWEAKER -- GETTING CONFIG FILE");

            cacheCompatibleMagazines = BaseMod.Config.Bind("General",
                                    "CacheCompatibleMagazines",
                                    false,
                                    "If true, guns will be able to spawn with any compatible mag in TNH (Eg. by default the VSS cannot spawn with 30rnd magazines)");

            allowLog = BaseMod.Config.Bind("Debug",
                                    "EnableLogging",
                                    false,
                                    "Set to true to enable logging");

            printCharacters = BaseMod.Config.Bind("Debug",
                                         "PrintCharacterInfo",
                                         false,
                                         "Decide if should print all character info");

            logPatrols = BaseMod.Config.Bind("Debug",
                                    "LogPatrolSpawns",
                                    false,
                                    "If true, patrols that spawn will have log output");

            logFileReads = BaseMod.Config.Bind("Debug",
                                    "LogFileReads",
                                    false,
                                    "If true, reading from a file will log the reading process");

            //TNHTweakerLogger.LogGeneral = allowLog.Value;
            //TNHTweakerLogger.LogCharacter = printCharacters.Value;
            //TNHTweakerLogger.LogPatrol = logPatrols.Value;
            //TNHTweakerLogger.LogFile = logFileReads.Value;

            TNHTweakerLogger.LogGeneral = true;
            TNHTweakerLogger.LogCharacter = true;
            TNHTweakerLogger.LogPatrol = true;
            TNHTweakerLogger.LogFile = true;

        }

        private void SetupOutputDirectory()
        {
            OutputFilePath = Application.dataPath.Replace("/h3vr_Data", "/TNH_Tweaker");

            if (!Directory.Exists(OutputFilePath))
            {
                Directory.CreateDirectory(OutputFilePath);
            }
        }

        [HarmonyPatch(typeof(IM), "GenerateItemDBs")] // Specify target method with HarmonyPatch attribute
        [HarmonyPostfix]
        public static void DelayedItemInit()
        {
            //Debug.Log("TNHTweaker -- Performing delayed init for all loaded characters and sosigs");

        }


        [HarmonyPatch(typeof(TNH_UIManager), "Start")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool InitTNH(List<TNH_UIManager.CharacterCategory> ___Categories, TNH_CharacterDatabase ___CharDatabase)
        {
            GM.TNHOptions.Char = TNH_Char.DD_ClassicLoudoutLouis;

            //Perform first time setup of all files
            if (!filesBuilt)
            {
                TNHTweakerLogger.Log("TNHTweaker -- Performing TNH Initialization", TNHTweakerLogger.LogType.File);

                //Load all of the default templates into our dictionaries
                LoadDefaultSosigs();
                LoadDefaultCharacters(___CharDatabase.Characters);
                LoadedTemplateManager.DefaultIconSprites = TNHTweakerUtils.GetAllIcons(LoadedTemplateManager.DefaultCharacters);

                //Remove all objects that havn't been loaded
                foreach (SosigTemplate template in LoadedTemplateManager.CustomSosigs)
                {
                    TNHTweakerUtils.RemoveUnloadedObjectIDs(template);
                }

                //Perform the delayed init for default characters
                foreach (CustomCharacter character in LoadedTemplateManager.DefaultCharacters)
                {
                    character.DelayedInit(false);
                }

                //Perform the delayed init for all custom loaded characters and sosigs
                foreach (CustomCharacter character in LoadedTemplateManager.CustomCharacters)
                {
                    character.DelayedInit(true);
                }
                foreach (SosigTemplate sosig in LoadedTemplateManager.CustomSosigs)
                {
                    sosig.DelayedInit();
                }

                //Create files relevant for character creation
                TNHTweakerUtils.CreateDefaultSosigTemplateFiles(LoadedTemplateManager.DefaultSosigs, OutputFilePath);
                TNHTweakerUtils.CreateDefaultCharacterFiles(LoadedTemplateManager.DefaultCharacters, OutputFilePath);
                TNHTweakerUtils.CreateIconIDFile(OutputFilePath, LoadedTemplateManager.DefaultIconSprites.Keys.ToList());
                TNHTweakerUtils.CreateObjectIDFile(OutputFilePath);
                TNHTweakerUtils.CreateSosigIDFile(OutputFilePath);
                TNHTweakerUtils.CreateJsonVaultFiles(OutputFilePath);
                
                if (cacheCompatibleMagazines.Value)
                {
                    TNHTweakerUtils.LoadMagazineCache(OutputFilePath);
                }
            }
            
            //Load all characters into the UI
            foreach (TNH_CharacterDef character in LoadedTemplateManager.LoadedCharactersDict.Keys)
            {
                if (!___Categories[(int)character.Group].Characters.Contains(character.CharacterID))
                {
                    ___Categories[(int)character.Group].Characters.Add(character.CharacterID);
                    ___CharDatabase.Characters.Add(character);
                }
            }

            filesBuilt = true;
            return true;
        }

        private static void LoadDefaultSosigs()
        {
            TNHTweakerLogger.Log("TNHTweaker -- Adding default sosigs", TNHTweakerLogger.LogType.File);

            //Now load all default sosig templates into custom sosig dictionary
            foreach (SosigEnemyTemplate sosig in ManagerSingleton<IM>.Instance.odicSosigObjsByID.Values)
            {
                LoadedTemplateManager.AddSosigTemplate(sosig);
            }
        }

        private static void LoadDefaultCharacters(List<TNH_CharacterDef> characters)
        {
            TNHTweakerLogger.Log("TNHTweaker -- Adding default characters", TNHTweakerLogger.LogType.File);

            foreach (TNH_CharacterDef character in characters)
            {
                LoadedTemplateManager.AddCharacterTemplate(character);
            }
        }


        [HarmonyPatch(typeof(TNH_Manager), "InitTables")] // Specify target method with HarmonyPatch attribute
        [HarmonyPostfix]
        public static void PrintGenerateTables(Dictionary<ObjectTableDef, ObjectTable> ___m_objectTableDics)
        {
            try
            {
                string path = OutputFilePath + "/pool_contents.txt";

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

            CustomCharacter character = LoadedTemplateManager.LoadedCharactersDict[instance.C];
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
                    template = ManagerSingleton<IM>.Instance.odicSosigObjsByID[(SosigEnemyID)LoadedTemplateManager.SosigIDDict[currPatrol.LeaderType]];
                    allowAllWeapons = true;
                }

                else
                {
                    template = ManagerSingleton<IM>.Instance.odicSosigObjsByID[(SosigEnemyID)LoadedTemplateManager.SosigIDDict[currPatrol.EnemyType.GetRandom<string>()]];
                    allowAllWeapons = false;
                }


                SosigTemplate customTemplate = LoadedTemplateManager.LoadedSosigsDict[template];
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

                squad.Squad.Add(sosig);
            }


            return squad;
        }


        [HarmonyPatch(typeof(TNH_Manager), "SetPhase_Hold")] // Specify target method with HarmonyPatch attribute
        [HarmonyPostfix]
        public static void AfterSetHold()
        {
            ClearAllPanels();
        }

        [HarmonyPatch(typeof(TNH_Manager), "SetPhase_Dead")] // Specify target method with HarmonyPatch attribute
        [HarmonyPostfix]
        public static void AfterSetDead()
        {
            ClearAllPanels();
        }

        [HarmonyPatch(typeof(TNH_Manager), "SetPhase_Completed")] // Specify target method with HarmonyPatch attribute
        [HarmonyPostfix]
        public static void AfterSetComplete()
        {
            ClearAllPanels();
        }

        public static void ClearAllPanels()
        {
            Debug.Log("Destroying constructors");
            while (SpawnedConstructors.Count > 0)
            {
                try
                {
                    TNH_ObjectConstructor constructor = SpawnedConstructors[0].GetComponent<TNH_ObjectConstructor>();

                    if(constructor != null)
                    {
                        constructor.ClearCase();
                    }

                    Destroy(SpawnedConstructors[0]);
                }
                catch
                {
                    Debug.LogWarning("Failed to destroy constructor! It's likely that the constructor is already destroyed, so everything is probably just fine :)");
                }
                
                SpawnedConstructors.RemoveAt(0);
            }

            Debug.Log("Destroying panels");
            while (SpawnedPanels.Count > 0)
            {
                Destroy(SpawnedPanels[0]);
                SpawnedPanels.RemoveAt(0);
            }
        }


        [HarmonyPatch(typeof(TNH_Manager), "SetPhase_Take")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool SetPhase_Take_Replacement(
            TNH_Manager __instance,
            int ___m_level,
            TNH_Progression.Level ___m_curLevel,
            TNH_PointSequence ___m_curPointSequence,
            ref int ___m_curHoldIndex,
            ref TNH_HoldPoint ___m_curHoldPoint)
        {
            spawnedBossIndexes.Clear();
            preventOutfitFunctionality = LoadedTemplateManager.LoadedCharactersDict[__instance.C].ForceDisableOutfitFunctionality;


            //Clear the TNH radar
            if(__instance.RadarMode == TNHModifier_RadarMode.Standard)
            {
                __instance.TAHReticle.GetComponent<AIEntity>().LM_VisualOcclusionCheck = __instance.ReticleMask_Take;
            }
            else if(__instance.RadarMode == TNHModifier_RadarMode.Omnipresent)
            {
                __instance.TAHReticle.GetComponent<AIEntity>().LM_VisualOcclusionCheck = __instance.ReticleMask_Hold;
            }

            __instance.TAHReticle.DeRegisterTrackedType(TAH_ReticleContact.ContactType.Hold);
            __instance.TAHReticle.DeRegisterTrackedType(TAH_ReticleContact.ContactType.Supply);


            //Get the next hold point and configure it
            ___m_curHoldIndex = GetNextHoldPointIndex(__instance, ___m_curPointSequence, ___m_level, ___m_curHoldIndex);
            ___m_curHoldPoint = __instance.HoldPoints[___m_curHoldIndex];
            ___m_curHoldPoint.ConfigureAsSystemNode(___m_curLevel.TakeChallenge, ___m_curLevel.HoldChallenge, ___m_curLevel.NumOverrideTokensForHold);
            
            __instance.TAHReticle.RegisterTrackedObject(___m_curHoldPoint.SpawnPoint_SystemNode, TAH_ReticleContact.ContactType.Hold);

            CustomCharacter character = LoadedTemplateManager.LoadedCharactersDict[__instance.C];
            Level level = character.GetCurrentLevel(___m_curLevel);


            //Generate all of the supply points for this level
            List<int> supplyPointsIndexes = GetNextSupplyPointIndexes(__instance, ___m_curPointSequence, ___m_level, ___m_curHoldIndex);
            int numSupplyPoints = UnityEngine.Random.Range(level.MinSupplyPoints, level.MaxSupplyPoints + 1);
            numSupplyPoints = Mathf.Clamp(numSupplyPoints, 0, supplyPointsIndexes.Count);

            TNHTweakerLogger.Log("TNHTWEAKER -- Spawning " + numSupplyPoints + " supply points", TNHTweakerLogger.LogType.Character);
            for (int i = 0; i < numSupplyPoints; i++)
            {
                TNH_SupplyPoint supplyPoint = __instance.SupplyPoints[supplyPointsIndexes[i]];
                ConfigureSupplyPoint(supplyPoint, level, i==0);
                TAH_ReticleContact contact = __instance.TAHReticle.RegisterTrackedObject(supplyPoint.SpawnPoint_PlayerSpawn, TAH_ReticleContact.ContactType.Supply);
                supplyPoint.SetContact(contact);
            }

            if(__instance.BGAudioMode == TNH_BGAudioMode.Default)
            {
                __instance.FMODController.SwitchTo(0, 2f, false, false);
            }

            return false;
        }

        public static void ConfigureSupplyPoint(TNH_SupplyPoint supplyPoint, Level level, bool isFirst)
        {
            TNHTweakerLogger.Log("TNHTWEAKER -- Configuring supply point", TNHTweakerLogger.LogType.Character);

            supplyPoint.T = level.TakeChallenge.GetTakeChallenge();

            Traverse pointTraverse = Traverse.Create(supplyPoint);

            SpawnSupplyGroup(supplyPoint, level);

            SpawnSupplyTurrets(supplyPoint, level);

            int numConstructors = UnityEngine.Random.Range(level.MinConstructors, level.MaxConstructors + 1);

            SpawnSupplyConstructor(supplyPoint, numConstructors);

            SpawnSecondarySupplyPanel(supplyPoint, level, numConstructors, isFirst);

            SpawnSupplyBoxes(supplyPoint, level);

            pointTraverse.Field("m_hasBeenVisited").SetValue(false);
        }


        public static void SpawnSupplyConstructor(TNH_SupplyPoint point, int toSpawn)
        {
            point.SpawnPoints_Panels.Shuffle();
            
            for(int i = 0; i < toSpawn && i < point.SpawnPoints_Panels.Count; i++)
            {
                GameObject constructor = point.M.SpawnObjectConstructor(point.SpawnPoints_Panels[i]);
                SpawnedConstructors.Add(constructor);
            }
        }
        
        public static void SpawnSecondarySupplyPanel(TNH_SupplyPoint point, Level level, int startingIndex, bool isFirst)
        {
            PanelType panelType;
            List<PanelType> panelTypes = new List<PanelType>(level.PossiblePanelTypes);

            if (panelTypes.Count == 0) return;

            int numPanels = UnityEngine.Random.Range(level.MinPanels, level.MaxPanels + 1);

            
            if (point.M.EquipmentMode != TNHSetting_EquipmentMode.LimitedAmmo || !isFirst)
            {
                panelTypes.Remove(PanelType.AmmoReloader);
            }

            for (int i = startingIndex; i < startingIndex + numPanels && i < point.SpawnPoints_Panels.Count && panelTypes.Count > 0; i++)
            {
                //If this is the first panel, and it's limited, we should ensure that it is an ammo resupply
                if (panelTypes.Contains(PanelType.AmmoReloader) && point.M.EquipmentMode == TNHSetting_EquipmentMode.LimitedAmmo && i == startingIndex && isFirst)
                {
                    panelType = PanelType.AmmoReloader;
                    panelTypes.Remove(PanelType.AmmoReloader);
                }

                //Otherwise we just select a random panel from valid panels
                else
                {
                    panelType = panelTypes.GetRandom();
                    panelTypes.Remove(panelType);
                }

                //If the valid panel was an ammo resupply, and we aren't limited, then we swap it for something else
                if(panelType == PanelType.MagDuplicator && point.M.EquipmentMode == TNHSetting_EquipmentMode.Spawnlocking && panelTypes.Count > 0)
                {
                    panelType = panelTypes.GetRandom();
                    panelTypes.Remove(panelType);
                }

                GameObject panel = null;

                if (panelType == PanelType.AmmoReloader)
                {
                    panel = point.M.SpawnAmmoReloader(point.SpawnPoints_Panels[i]);
                }

                else if (panelType == PanelType.MagDuplicator)
                {
                    panel = point.M.SpawnMagDuplicator(point.SpawnPoints_Panels[i]);
                }

                else if (panelType == PanelType.Recycler)
                {
                    panel = point.M.SpawnGunRecycler(point.SpawnPoints_Panels[i]);
                }

                else if (panelType == PanelType.MagUpgrader)
                {
                    Debug.Log("Spawning Mag Upgrader!");
                    panel = point.M.SpawnMagDuplicator(point.SpawnPoints_Panels[i]);
                    panel.AddComponent(typeof(MagUpgrader));
                }

                else if (panelType == PanelType.AddFullAuto)
                {
                    Debug.Log("Spawning Full Auto Adder!");
                    panel = point.M.SpawnMagDuplicator(point.SpawnPoints_Panels[i]);
                    panel.AddComponent(typeof(FullAutoEnabler));
                }

                //If we spawned a panel, add it to the global list
                if (panel != null)
                {
                    SpawnedPanels.Add(panel);
                }
            }
        }

        public static void SpawnSupplyGroup(TNH_SupplyPoint point, Level level)
        {
            point.SpawnPoints_Sosigs_Defense.Shuffle<Transform>();

            Traverse pointTraverse = Traverse.Create(point);

            for (int i = 0; i < level.TakeChallenge.NumGuards && i < point.SpawnPoints_Sosigs_Defense.Count; i++)
            {
                Transform transform = point.SpawnPoints_Sosigs_Defense[i];
                SosigEnemyTemplate template = ManagerSingleton<IM>.Instance.odicSosigObjsByID[level.TakeChallenge.GetTakeChallenge().GID];
                SosigTemplate customTemplate = LoadedTemplateManager.LoadedSosigsDict[template];

                TNHTweakerLogger.Log("TNHTWEAKER -- SPAWNING SUPPLY GROUP AT " + transform.position, TNHTweakerLogger.LogType.Patrol);

                Sosig enemy = SpawnEnemy(customTemplate, LoadedTemplateManager.LoadedCharactersDict[point.M.C], transform, point.M.AI_Difficulty, level.TakeChallenge.IFFUsed, false, transform.position, true);

                pointTraverse.Field("m_activeSosigs").Method("Add", enemy).GetValue();
            }
        }


        public static void SpawnSupplyTurrets(TNH_SupplyPoint point, Level level)
        {
            point.SpawnPoints_Turrets.Shuffle<Transform>();
            FVRObject turretPrefab = point.M.GetTurretPrefab(level.TakeChallenge.TurretType);

            Traverse pointTraverse = Traverse.Create(point);

            for (int i = 0; i < level.TakeChallenge.NumTurrets && i < point.SpawnPoints_Turrets.Count; i++)
            {
                Vector3 pos = point.SpawnPoints_Turrets[i].position + Vector3.up * 0.25f;
                AutoMeater turret = Instantiate<GameObject>(turretPrefab.GetGameObject(), pos, point.SpawnPoints_Turrets[i].rotation).GetComponent<AutoMeater>();
                pointTraverse.Field("m_activeTurrets").Method("Add", turret).GetValue();
            }

        }


        public static void SpawnSupplyBoxes(TNH_SupplyPoint point, Level level)
        {
            List<GameObject> boxes = (List<GameObject>)Traverse.Create(point).Field("m_spawnBoxes").GetValue();

            point.SpawnPoints_Boxes.Shuffle();

            int boxesToSpawn = UnityEngine.Random.Range(level.MinBoxesSpawned, level.MaxBoxesSpawned + 1);

            TNHTweakerLogger.Log("TNHTWEAKER -- GOING TO SPAWN " + boxesToSpawn + " BOXES AT THIS SUPPLY POINT -- MIN (" + level.MinBoxesSpawned + "), MAX (" + level.MaxBoxesSpawned + ")", TNHTweakerLogger.LogType.General);

            for (int i = 0; i < boxesToSpawn; i++)
            {
                Transform spawnTransform = point.SpawnPoints_Boxes[UnityEngine.Random.Range(0, point.SpawnPoints_Boxes.Count)];
                Vector3 position = spawnTransform.position + Vector3.up * 0.1f + Vector3.right * UnityEngine.Random.Range(-0.5f, 0.5f) + Vector3.forward * UnityEngine.Random.Range(-0.5f, 0.5f);
                Quaternion rotation = Quaternion.Slerp(spawnTransform.rotation, UnityEngine.Random.rotation, 0.1f);
                GameObject box = Instantiate(point.M.Prefabs_ShatterableCrates[UnityEngine.Random.Range(0, point.M.Prefabs_ShatterableCrates.Count)], position, rotation);
                boxes.Add(box);
                TNHTweakerLogger.Log("TNHTWEAKER -- BOX SPAWNED", TNHTweakerLogger.LogType.General);
            }

            int tokensSpawned = 0;

            foreach (GameObject boxObj in boxes)
            {
                if (tokensSpawned < level.MinTokensPerSupply)
                {
                    boxObj.GetComponent<TNH_ShatterableCrate>().SetHoldingToken(point.M);
                    tokensSpawned += 1;
                }

                else if (tokensSpawned < level.MaxTokensPerSupply && UnityEngine.Random.value < level.BoxTokenChance)
                {
                    boxObj.GetComponent<TNH_ShatterableCrate>().SetHoldingToken(point.M);
                    tokensSpawned += 1;
                }

                else if (UnityEngine.Random.value < level.BoxHealthChance)
                {
                    boxObj.GetComponent<TNH_ShatterableCrate>().SetHoldingHealth(point.M);
                }
            }
        }


        public static int GetNextHoldPointIndex(TNH_Manager M, TNH_PointSequence pointSequence, int currLevel, int currHoldIndex)
        {
            int index;

            //If we havn't gone through all the hold points, we just select the next one we havn't been to
            if (currLevel < pointSequence.HoldPoints.Count)
            {
                index = pointSequence.HoldPoints[currLevel];
            }

            //If we have been to all the points, then we just select a random safe one
            else
            {
                List<int> pointIndexes = new List<int>();
                for (int i = 0; i < M.SafePosMatrix.Entries_HoldPoints[currHoldIndex].SafePositions_HoldPoints.Count; i++)
                {
                    if (i != currHoldIndex && M.SafePosMatrix.Entries_HoldPoints[currHoldIndex].SafePositions_HoldPoints[i])
                    {
                        pointIndexes.Add(i);
                    }
                }

                index = pointIndexes.GetRandom();
            }

            return index;
        }


        public static List<int> GetNextSupplyPointIndexes(TNH_Manager M, TNH_PointSequence pointSequence, int currLevel, int currHoldIndex)
        {
            List<int> indexes = new List<int>();

            if(currLevel == 0)
            {
                for(int i = 0; i < M.SafePosMatrix.Entries_SupplyPoints[pointSequence.StartSupplyPointIndex].SafePositions_SupplyPoints.Count; i++)
                {
                    if (M.SafePosMatrix.Entries_SupplyPoints[pointSequence.StartSupplyPointIndex].SafePositions_SupplyPoints[i])
                    {
                        indexes.Add(i);
                    }
                }
            }
            else
            {
                for(int i = 0; i < M.SafePosMatrix.Entries_HoldPoints[currHoldIndex].SafePositions_SupplyPoints.Count; i++)
                {
                    if (M.SafePosMatrix.Entries_HoldPoints[currHoldIndex].SafePositions_SupplyPoints[i])
                    {
                        indexes.Add(i);
                    }
                }
            }

            indexes.Shuffle();

            return indexes;
        }


        [HarmonyPatch(typeof(TNH_HoldPoint), "SpawnTakeEnemyGroup")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool SpawnTakeGroupReplacement(List<Transform> ___SpawnPoints_Sosigs_Defense, TNH_TakeChallenge ___T, TNH_Manager ___M, List<Sosig> ___m_activeSosigs)
        {
            ___SpawnPoints_Sosigs_Defense.Shuffle<Transform>();

            for(int i = 0; i < ___T.NumGuards && i < ___SpawnPoints_Sosigs_Defense.Count; i++)
            {
                Transform transform = ___SpawnPoints_Sosigs_Defense[i];
                Debug.Log("Take challenge sosig ID : " + ___T.GID);
                SosigEnemyTemplate template = ManagerSingleton<IM>.Instance.odicSosigObjsByID[___T.GID];
                SosigTemplate customTemplate = LoadedTemplateManager.LoadedSosigsDict[template];

                TNHTweakerLogger.Log("TNHTWEAKER -- SPAWNING TAKE GROUP AT " + transform.position, TNHTweakerLogger.LogType.Patrol);

                Sosig enemy = SpawnEnemy(customTemplate, LoadedTemplateManager.LoadedCharactersDict[___M.C], transform, ___M.AI_Difficulty, ___T.IFFUsed, false, transform.position, true);

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


        [HarmonyPatch(typeof(TNH_HoldPoint), "IdentifyEncryption")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool IdentifyEncryptionReplacement(TNH_HoldPoint __instance, TNH_HoldChallenge.Phase ___m_curPhase)
        {
            CustomCharacter character = LoadedTemplateManager.LoadedCharactersDict[__instance.M.C];
            Phase currentPhase = character.GetCurrentPhase(___m_curPhase);
            Traverse holdTraverse = Traverse.Create(__instance);

            //If we shouldnt spawn any targets, we exit out early
            if ((currentPhase.MaxTargets < 1 && __instance.M.EquipmentMode == TNHSetting_EquipmentMode.Spawnlocking) ||
                (currentPhase.MaxTargetsLimited < 1 && __instance.M.EquipmentMode == TNHSetting_EquipmentMode.LimitedAmmo))
            {
                holdTraverse.Method("CompletePhase").GetValue();
                return false;
            }

            holdTraverse.Field("m_state").SetValue(TNH_HoldPoint.HoldState.Hacking);
            holdTraverse.Field("m_tickDownToFailure").SetValue(120f);

            __instance.M.EnqueueEncryptionLine(currentPhase.Encryptions[0]);
            
            holdTraverse.Method("DeleteAllActiveWarpIns").GetValue();
            SpawnEncryptionReplacement(__instance, currentPhase);
            holdTraverse.Field("m_systemNode").Method("SetNodeMode", TNH_HoldPointSystemNode.SystemNodeMode.Indentified).GetValue();

            return false;
        }


        public static void SpawnEncryptionReplacement(TNH_HoldPoint holdPoint, Phase currentPhase)
        {
            int numTargets;
            if (holdPoint.M.EquipmentMode == TNHSetting_EquipmentMode.LimitedAmmo)
            {
                numTargets = UnityEngine.Random.Range(currentPhase.MinTargetsLimited, currentPhase.MaxTargetsLimited + 1);
            }
            else
            {
                numTargets = UnityEngine.Random.Range(currentPhase.MinTargets, currentPhase.MaxTargets + 1);
            }

            List<FVRObject> encryptions = currentPhase.Encryptions.Select(o => holdPoint.M.GetEncryptionPrefab(o)).ToList();
            for(int i = 0; i < numTargets && i < holdPoint.SpawnPoints_Targets.Count; i++)
            {
                GameObject gameObject = Instantiate(encryptions[i % encryptions.Count].GetGameObject(), holdPoint.SpawnPoints_Targets[i].position, holdPoint.SpawnPoints_Targets[i].rotation);
                TNH_EncryptionTarget encryption = gameObject.GetComponent<TNH_EncryptionTarget>();
                encryption.SetHoldPoint(holdPoint);
                holdPoint.RegisterNewTarget(encryption);
            }
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

            //Handle sosig dropping custom loot
            if (UnityEngine.Random.value < template.DroppedLootChance)
            {
                sosigComponent.Links[2].RegisterSpawnOnDestroy(template.TableDef.GetRandomObject());
            }

            return sosigComponent;
        }


        [HarmonyPatch(typeof(FVRPlayerBody), "SetOutfit")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool SetOutfitReplacement(SosigEnemyTemplate tem, PlayerSosigBody ___m_sosigPlayerBody)
        {
            if (___m_sosigPlayerBody == null) return false;

            GM.Options.ControlOptions.MBClothing = tem.SosigEnemyID;
            if(tem.SosigEnemyID != SosigEnemyID.None)
            {
                if(tem.OutfitConfig.Count > 0 && LoadedTemplateManager.LoadedSosigsDict.ContainsKey(tem))
                {
                    OutfitConfig outfitConfig = LoadedTemplateManager.LoadedSosigsDict[tem].OutfitConfigs.GetRandom();

                    List<GameObject> clothing = Traverse.Create(___m_sosigPlayerBody).Field("m_curClothes").GetValue<List<GameObject>>();
                    foreach (GameObject item in clothing)
                    {
                        Destroy(item);
                    }
                    clothing.Clear();

                    if (outfitConfig.Chance_Headwear >= UnityEngine.Random.value)
                    {
                        EquipSosigClothing(outfitConfig.Headwear, clothing, ___m_sosigPlayerBody.Sosig_Head, outfitConfig.ForceWearAllHead);
                    }

                    if (outfitConfig.Chance_Facewear >= UnityEngine.Random.value)
                    {
                        EquipSosigClothing(outfitConfig.Facewear, clothing, ___m_sosigPlayerBody.Sosig_Head, outfitConfig.ForceWearAllFace);
                    }

                    if (outfitConfig.Chance_Eyewear >= UnityEngine.Random.value)
                    {
                        EquipSosigClothing(outfitConfig.Eyewear, clothing, ___m_sosigPlayerBody.Sosig_Head, outfitConfig.ForceWearAllEye);
                    }

                    if (outfitConfig.Chance_Torsowear >= UnityEngine.Random.value)
                    {
                        EquipSosigClothing(outfitConfig.Torsowear, clothing, ___m_sosigPlayerBody.Sosig_Torso, outfitConfig.ForceWearAllTorso);
                    }

                    if (outfitConfig.Chance_Pantswear >= UnityEngine.Random.value)
                    {
                        EquipSosigClothing(outfitConfig.Pantswear, clothing, ___m_sosigPlayerBody.Sosig_Abdomen, outfitConfig.ForceWearAllPants);
                    }

                    if (outfitConfig.Chance_Pantswear_Lower >= UnityEngine.Random.value)
                    {
                        EquipSosigClothing(outfitConfig.Pantswear_Lower, clothing, ___m_sosigPlayerBody.Sosig_Legs, outfitConfig.ForceWearAllPantsLower);
                    }

                    if (outfitConfig.Chance_Backpacks >= UnityEngine.Random.value)
                    {
                        EquipSosigClothing(outfitConfig.Backpacks, clothing, ___m_sosigPlayerBody.Sosig_Torso, outfitConfig.ForceWearAllBackpacks);
                    }

                }
            }

            return false;
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


        public static void EquipSosigClothing(List<string> options, List<GameObject> playerClothing, Transform link,  bool wearAll)
        {
            if (wearAll)
            {
                foreach (string clothing in options)
                {
                    GameObject clothingObject = Instantiate(IM.OD[clothing].GetGameObject(), link.position, link.rotation);

                    Component[] children = clothingObject.GetComponentsInChildren<Component>(true);
                    foreach(Component child in children)
                    {
                        child.gameObject.layer = LayerMask.NameToLayer("ExternalCamOnly");

                        if(!(child is Transform) && !(child is MeshFilter) && !(child is MeshRenderer))
                        {
                            Destroy(child);
                        }
                    }

                    playerClothing.Add(clothingObject);
                    clothingObject.transform.SetParent(link);
                }
            }

            else
            {
                GameObject clothingObject = Instantiate(IM.OD[options.GetRandom<string>()].GetGameObject(), link.position, link.rotation);

                Component[] children = clothingObject.GetComponentsInChildren<Component>(true);
                foreach (Component child in children)
                {
                    child.gameObject.layer = LayerMask.NameToLayer("ExternalCamOnly");

                    if (!(child is Transform) && !(child is MeshFilter) && !(child is MeshRenderer))
                    {
                        Destroy(child);
                    }
                }

                playerClothing.Add(clothingObject);
                clothingObject.transform.SetParent(link);
            }
        }


        


        public static void SpawnGrenades(List<TNH_HoldPoint.AttackVector> AttackVectors, TNH_Manager M, int m_phaseIndex)
        {
            CustomCharacter character = LoadedTemplateManager.LoadedCharactersDict[M.C];
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
            CustomCharacter character = LoadedTemplateManager.LoadedCharactersDict[M.C];
            Level currLevel = character.GetCurrentLevel((TNH_Progression.Level)Traverse.Create(M).Field("m_curLevel").GetValue());
            Phase currPhase = currLevel.HoldPhases[phaseIndex];

            //Set first enemy to be spawned as leader
            SosigEnemyTemplate enemyTemplate = ManagerSingleton<IM>.Instance.odicSosigObjsByID[(SosigEnemyID)LoadedTemplateManager.SosigIDDict[currPhase.LeaderType]];
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

                SosigTemplate customTemplate = LoadedTemplateManager.LoadedSosigsDict[enemyTemplate];

                Sosig enemy = SpawnEnemy(customTemplate, character, AttackVectors[vectorIndex].SpawnPoints_Sosigs_Attack[vectorSpawnPoint], M.AI_Difficulty, curPhase.IFFUsed, true, targetVector, true);

                ActiveSosigs.Add(enemy);

                TNHTweakerLogger.Log("TNHTWEAKER -- SOSIG SPAWNED", TNHTweakerLogger.LogType.General);

                //At this point, the leader has been spawned, so always set enemy to be regulars
                enemyTemplate = ManagerSingleton<IM>.Instance.odicSosigObjsByID[(SosigEnemyID)LoadedTemplateManager.SosigIDDict[currPhase.EnemyType.GetRandom<string>()]];
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


        [HarmonyPatch(typeof(TNH_ObjectConstructor), "ButtonClicked")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool ButtonClickedReplacement(int i,
            TNH_ObjectConstructor __instance,
            EquipmentPoolDef ___m_pool,
            int ___m_curLevel,
            ref int ___m_selectedEntry,
            ref int ___m_numTokensSelected,
            bool ___allowEntry,
            List<EquipmentPoolDef.PoolEntry> ___m_poolEntries,
            List<int> ___m_poolAddedCost,
            GameObject ___m_spawnedCase)
        {
            Traverse constructorTraverse = Traverse.Create(__instance);

            constructorTraverse.Method("UpdateRerollButtonState", false).GetValue();

            if (!___allowEntry)
            {
                return false;
            }
            
            if(__instance.State == TNH_ObjectConstructor.ConstructorState.EntryList)
            {

                int cost = ___m_poolEntries[i].GetCost(__instance.M.EquipmentMode) + ___m_poolAddedCost[i];
                if(__instance.M.GetNumTokens() >= cost)
                {
                    constructorTraverse.Method("SetState", TNH_ObjectConstructor.ConstructorState.Confirm, i).GetValue();
                    SM.PlayCoreSound(FVRPooledAudioType.UIChirp, __instance.AudEvent_Select, __instance.transform.position);
                }
                else
                {
                    SM.PlayCoreSound(FVRPooledAudioType.UIChirp, __instance.AudEvent_Fail, __instance.transform.position);
                }
            }

            else if(__instance.State == TNH_ObjectConstructor.ConstructorState.Confirm)
            {

                if (i == 0)
                {
                    constructorTraverse.Method("SetState", TNH_ObjectConstructor.ConstructorState.EntryList, 0).GetValue();
                    ___m_selectedEntry = -1;
                    SM.PlayCoreSound(FVRPooledAudioType.UIChirp, __instance.AudEvent_Back, __instance.transform.position);
                }
                else if(i == 2)
                {
                    int cost = ___m_poolEntries[___m_selectedEntry].GetCost(__instance.M.EquipmentMode) + ___m_poolAddedCost[___m_selectedEntry];
                    if (__instance.M.GetNumTokens() >= cost)
                    {

                        if ((!___m_poolEntries[___m_selectedEntry].TableDef.SpawnsInSmallCase && !___m_poolEntries[___m_selectedEntry].TableDef.SpawnsInSmallCase) || ___m_spawnedCase == null)
                        {

                            AnvilManager.Run(SpawnObjectAtConstructor(___m_poolEntries[___m_selectedEntry], __instance, constructorTraverse));
                            ___m_numTokensSelected = 0;
                            __instance.M.SubtractTokens(cost);
                            SM.PlayCoreSound(FVRPooledAudioType.UIChirp, __instance.AudEvent_Spawn, __instance.transform.position);

                            if (__instance.M.C.UsesPurchasePriceIncrement)
                            {
                                ___m_poolAddedCost[___m_selectedEntry] += 1;
                            }

                            constructorTraverse.Method("SetState", TNH_ObjectConstructor.ConstructorState.EntryList, 0).GetValue();
                            ___m_selectedEntry = -1;
                        }

                        else
                        {
                            SM.PlayCoreSound(FVRPooledAudioType.UIChirp, __instance.AudEvent_Fail, __instance.transform.position);
                        }
                    }
                    else
                    {
                        SM.PlayCoreSound(FVRPooledAudioType.UIChirp, __instance.AudEvent_Fail, __instance.transform.position);
                    }
                }
            }

            return false;
        }


        private static IEnumerator SpawnObjectAtConstructor(EquipmentPoolDef.PoolEntry entry, TNH_ObjectConstructor constructor, Traverse constructorTraverse)
        {
            constructorTraverse.Field("allowEntry").SetValue(false);
            EquipmentPool pool = LoadedTemplateManager.EquipmentPoolDictionary[entry];
            CustomCharacter character = LoadedTemplateManager.LoadedCharactersDict[constructor.M.C];
            List<GameObject> trackedObjects = (List<GameObject>)(constructorTraverse.Field("m_trackedObjects").GetValue());

            if(pool.Tables[0].SpawnsInLargeCase || pool.Tables[0].SpawnsInSmallCase)
            {
                GameObject caseFab = constructor.M.Prefab_WeaponCaseLarge;
                if (pool.Tables[0].SpawnsInSmallCase) caseFab = constructor.M.Prefab_WeaponCaseSmall;

                FVRObject item = IM.OD[pool.Tables[0].GetObjects().GetRandom()];
                GameObject itemCase = constructor.M.SpawnWeaponCase(caseFab, constructor.SpawnPoint_Case.position, constructor.SpawnPoint_Case.forward, item, pool.Tables[0].NumMagsSpawned, pool.Tables[0].NumRoundsSpawned, pool.Tables[0].MinAmmoCapacity, pool.Tables[0].MaxAmmoCapacity);

                constructorTraverse.Field("m_spawnedCase").SetValue(itemCase);
                itemCase.GetComponent<TNH_WeaponCrate>().M = constructor.M;
            }

            else
            {
                int mainSpawnCount = 0;
                int requiredSpawnCount = 0;
                int ammoSpawnCount = 0;
                int objectSpawnCount = 0;

                for (int tableIndex = 0; tableIndex < pool.Tables.Count; tableIndex++)
                {
                    ObjectPool table = pool.Tables[tableIndex];

                    for(int itemIndex = 0; itemIndex < table.ItemsToSpawn; itemIndex++)
                    {
                        FVRObject mainObject;

                        Transform primarySpawn = constructor.SpawnPoint_Object;
                        Transform requiredSpawn = constructor.SpawnPoint_Object;
                        Transform ammoSpawn = constructor.SpawnPoint_Mag;

                        if (table.IsCompatibleMagazine)
                        {
                            mainObject = TNHTweakerUtils.GetMagazineForEquipped(table.MinAmmoCapacity, table.MaxAmmoCapacity);
                            if(mainObject == null)
                            {
                                break;
                            }
                        }
                        else
                        {
                            Debug.Log("Objects count: " + table.GetObjects().Count);
                            string item = table.GetObjects().GetRandom();

                            if (LoadedTemplateManager.LoadedVaultFiles.ContainsKey(item))
                            {
                                AnvilManager.Run(TNHTweakerUtils.SpawnFirearm(LoadedTemplateManager.LoadedVaultFiles[item], primarySpawn));
                                mainSpawnCount += 1;
                                break;
                            }

                            else
                            {
                                mainObject = IM.OD[item];
                            }
                        }

                        if (mainObject.Category == FVRObject.ObjectCategory.Firearm)
                        {
                            primarySpawn = constructor.SpawnPoints_GunsSize[mainObject.TagFirearmSize - FVRObject.OTagFirearmSize.Pocket];
                            requiredSpawn = constructor.SpawnPoint_Grenade;
                            mainSpawnCount += 1;
                        }
                        else if (mainObject.Category == FVRObject.ObjectCategory.Explosive || mainObject.Category == FVRObject.ObjectCategory.Thrown)
                        {
                            primarySpawn = constructor.SpawnPoint_Grenade;
                        }
                        else if (mainObject.Category == FVRObject.ObjectCategory.MeleeWeapon)
                        {
                            primarySpawn = constructor.SpawnPoint_Melee;
                        }

                        //Spawn the main object
                        yield return mainObject.GetGameObjectAsync();
                        GameObject spawnedObject = Instantiate(mainObject.GetGameObject(), primarySpawn.position + Vector3.up * 0.2f * mainSpawnCount, primarySpawn.rotation);
                        trackedObjects.Add(spawnedObject);

                        //Spawn any required objects
                        for (int j = 0; j < mainObject.RequiredSecondaryPieces.Count; j++)
                        {
                            yield return mainObject.RequiredSecondaryPieces[j].GetGameObjectAsync();
                            GameObject requiredItem = Instantiate(mainObject.RequiredSecondaryPieces[j].GetGameObject(), requiredSpawn.position + -requiredSpawn.right * 0.2f * requiredSpawnCount + Vector3.up * 0.2f * j, requiredSpawn.rotation);
                            trackedObjects.Add(requiredItem);
                            requiredSpawnCount += 1;
                        }

                        //If this object has compatible ammo object, then we should spawn those
                        FVRObject ammoObject = mainObject.GetRandomAmmoObject(mainObject, character.ValidAmmoEras, table.MinAmmoCapacity, table.MaxAmmoCapacity, character.ValidAmmoSets);
                        if (ammoObject != null)
                        {
                            int spawnCount = table.NumMagsSpawned;

                            if (ammoObject.Category == FVRObject.ObjectCategory.Cartridge)
                            {
                                ammoSpawn = constructor.SpawnPoint_Ammo;
                                spawnCount = table.NumRoundsSpawned;
                            }

                            yield return ammoObject.GetGameObjectAsync();

                            for (int j = 0; j < spawnCount; j++)
                            {
                                GameObject spawnedAmmo = Instantiate(ammoObject.GetGameObject(), ammoSpawn.position + -ammoSpawn.right * 0.15f * ammoSpawnCount + ammoSpawn.up * 0.15f * j, ammoSpawn.rotation);
                                trackedObjects.Add(spawnedAmmo);
                            }

                            ammoSpawnCount += 1;
                        }

                        //If this object equires picatinny sights, we should try to spawn one
                        if (mainObject.RequiresPicatinnySight && character.GetRequiredSightsTable() != null)
                        {
                            FVRObject sight = character.GetRequiredSightsTable().GetRandomObject();
                            yield return sight.GetGameObjectAsync();
                            GameObject spawnedSight = Instantiate(sight.GetGameObject(), constructor.SpawnPoint_Object.position + -constructor.SpawnPoint_Object.right * 0.15f * objectSpawnCount, constructor.SpawnPoint_Object.rotation);
                            trackedObjects.Add(spawnedSight);

                            for (int j = 0; j < sight.RequiredSecondaryPieces.Count; j++)
                            {
                                yield return sight.RequiredSecondaryPieces[j].GetGameObjectAsync();
                                GameObject spawnedRequired = Instantiate(sight.RequiredSecondaryPieces[j].GetGameObject(), constructor.SpawnPoint_Object.position + -constructor.SpawnPoint_Object.right * 0.15f * objectSpawnCount + Vector3.up * 0.15f * j, constructor.SpawnPoint_Object.rotation);
                                trackedObjects.Add(spawnedRequired);
                            }

                            objectSpawnCount += 1;
                        }

                        //If this object has bespoke attachments we'll try to spawn one
                        else if (mainObject.BespokeAttachments.Count > 0 && UnityEngine.Random.value < table.BespokeAttachmentChance)
                        {
                            FVRObject bespoke = mainObject.BespokeAttachments.GetRandom();
                            yield return bespoke.GetGameObjectAsync();
                            GameObject bespokeObject = Instantiate(bespoke.GetGameObject(), constructor.SpawnPoint_Object.position + -constructor.SpawnPoint_Object.right * 0.15f * objectSpawnCount, constructor.SpawnPoint_Object.rotation);
                            trackedObjects.Add(bespokeObject);

                            objectSpawnCount += 1;
                        }
                    }
                }
            }

            constructorTraverse.Field("allowEntry").SetValue(true);
            yield break;
        }


        [HarmonyPatch(typeof(TNH_MagDuplicator), "Button_Duplicate")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool ButtonPressed(TNH_MagDuplicator __instance)
        {
            MagUpgrader upgraderPanel = __instance.GetComponent<MagUpgrader>();
            if (upgraderPanel != null)
            {
                upgraderPanel.ButtonPressed();
                return false;
            }

            FullAutoEnabler fullAutoPanel = __instance.GetComponent<FullAutoEnabler>();
            if(fullAutoPanel != null)
            {
                fullAutoPanel.ButtonPressed();
                return false;
            }

            return true;
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
