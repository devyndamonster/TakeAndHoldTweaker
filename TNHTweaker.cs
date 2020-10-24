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

        private static bool filesBuilt = false;

        //private static FVRObject tokenPrefab = null;


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
                    ObjectBuilder.LoadCompatibleMagazines(characterPath);
                }
                
                TNHTweakerUtils.CreateObjectIDFile(characterPath);
                TNHTweakerUtils.CreateSosigIDFile(characterPath);
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

            //First load the default characters into the custom character dictionary
            foreach(TNH_CharacterDef characterDef in characters)
            {
                ObjectBuilder.RemoveUnloadedObjectIDs(characterDef);

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

                string json = File.ReadAllText(characterDir + "/character.json");
                CustomCharacter character = JsonConvert.DeserializeObject<CustomCharacter>(json);
                TNHTweakerLogger.Log("TNHTWEAKER -- DESERIALIZED", TNHTweakerLogger.LogType.Character);
                TNH_CharacterDef characterDef = character.GetCharacter(ID, characterDir, equipmentIcons);
                TNHTweakerLogger.Log("TNHTWEAKER -- CONVERTED", TNHTweakerLogger.LogType.Character);
                ObjectBuilder.RemoveUnloadedObjectIDs(characterDef);
                customCharDict.Add(characterDef, character);
                ID += 1;

                TNHTweakerLogger.Log("TNHTWEAKER -- CHARACTER LOADED: " + character.DisplayName, TNHTweakerLogger.LogType.Character);
            }
        }

        
        [HarmonyPatch(typeof(TNH_Manager), "GenerateValidPatrol")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool GenerateValidPatrolReplacement(TNH_PatrolChallenge P, int curStandardIndex, int excludeHoldIndex, bool isStart, TNH_Manager __instance, TNH_Progression.Level ___m_curLevel, List<TNH_Manager.SosigPatrolSquad> ___m_patrolSquads, ref float ___m_timeTilPatrolCanSpawn)
        {
            TNHTweakerLogger.Log("TNHTWEAKER -- GENERATING A PATROL -- THERE ARE CURRENTLY " + ___m_patrolSquads.Count + " PATROLS ACTIVE", TNHTweakerLogger.LogType.Patrol);

            if (P.Patrols.Count < 1) return false;

            int patrolIndex = UnityEngine.Random.Range(0, P.Patrols.Count);
            TNH_PatrolChallenge.Patrol patrol = P.Patrols[patrolIndex];

            List<int> validLocations = new List<int>();
            float minDist = __instance.TAHReticle.Range * 1.2f;

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

            TNH_Manager.SosigPatrolSquad squad = GeneratePatrol(validLocations[0], __instance, ___m_curLevel, patrol);

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

        
        public static TNH_Manager.SosigPatrolSquad GeneratePatrol(int HoldPointStart, TNH_Manager instance, TNH_Progression.Level level, TNH_PatrolChallenge.Patrol patrol)
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

            for(int i = 0; i < PatrolSize; i++)
            {
                SosigEnemyTemplate template;
                bool allowAllWeapons;

                if(i == 0)
                {
                    template = ManagerSingleton<IM>.Instance.odicSosigObjsByID[patrol.LType];
                    allowAllWeapons = true;
                }

                else
                {
                    template = ManagerSingleton<IM>.Instance.odicSosigObjsByID[patrol.EType];
                    allowAllWeapons = false;
                }

                CustomCharacter character = customCharDict[instance.C];
                Level currLevel = character.GetCurrentLevel(level);
                Patrol currPatrol = currLevel.GetPatrol(patrol);

                FVRObject droppedObject = instance.Prefab_HealthPickupMinor;

                Sosig sosig;
                if (currPatrol.SwarmPlayer)
                {
                    squad.PatrolPoints[0] = GM.CurrentPlayerBody.transform.position;
                    sosig = instance.SpawnEnemy(template, instance.HoldPoints[HoldPointStart].SpawnPoints_Sosigs_Defense[i], currPatrol.IFFUsed, true, squad.PatrolPoints[0], allowAllWeapons);
                    sosig.SetAssaultSpeed(Sosig.SosigMoveSpeed.Running);
                }
                else
                {
                    sosig = instance.SpawnEnemy(template, instance.HoldPoints[HoldPointStart].SpawnPoints_Sosigs_Defense[i], currPatrol.IFFUsed, true, squad.PatrolPoints[0], allowAllWeapons);
                    sosig.SetAssaultSpeed(currPatrol.AssualtSpeed);
                }

                if(i == 0 && UnityEngine.Random.value < currPatrol.DropChance)
                {
                    sosig.Links[1].RegisterSpawnOnDestroy(droppedObject);
                }

                squad.Squad.Add(sosig);
            }


            return squad;
        }




        [HarmonyPatch(typeof(TNH_Manager), "SetPhase_Take")] // Specify target method with HarmonyPatch attribute
        [HarmonyPostfix]
        public static void AfterSetTake(List<TNH_SupplyPoint> ___SupplyPoints, TNH_Progression.Level ___m_curLevel, TAH_Reticle ___TAHReticle, int ___m_level, TNH_CharacterDef ___C)
        {
            TNHTweakerLogger.Log("TNHTWEAKER -- ADDING ADDITIONAL SUPPLY POINTS", TNHTweakerLogger.LogType.General);

            CustomCharacter character = customCharDict[___C];
            Level currLevel = character.GetCurrentLevel(___m_curLevel);

            List<TNH_SupplyPoint> possiblePoints = new List<TNH_SupplyPoint>(___SupplyPoints);
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
                //SosigEnemyTemplate template = ___M.GetEnemyTemplate(___T.GuardType);
                SosigEnemyTemplate template = ManagerSingleton<IM>.Instance.odicSosigObjsByID[___T.GID];
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

            //Set first enemy to be spawned as leader
            SosigEnemyTemplate enemyTemplate = ManagerSingleton<IM>.Instance.odicSosigObjsByID[curPhase.LType];
            int enemiesToSpawn = UnityEngine.Random.Range(curPhase.MinEnemies, curPhase.MaxEnemies + 1);


            //Figure out if enemies on this hold should go directly toward the player
            CustomCharacter character = customCharDict[M.C];
            Level currLevel = character.GetCurrentLevel((TNH_Progression.Level)Traverse.Create(M).Field("m_curLevel").GetValue());
            Phase currPhase = currLevel.HoldPhases[phaseIndex];
            
            int sosigsSpawned = 0;
            int vectorSpawnPoint = 0;
            Vector3 targetVector;
            int vectorIndex = 0;
            while(sosigsSpawned < enemiesToSpawn)
            {
                TNHTweakerLogger.Log("TNHTWEAKER -- SPAWNING AT ATTACK VECTOR: " + vectorIndex, TNHTweakerLogger.LogType.General);

                if (AttackVectors[vectorIndex].SpawnPoints_Sosigs_Attack.Count <= vectorSpawnPoint) break;

                //Spawn the enemy
                if (currPhase.SwarmPlayer)
                {
                    targetVector = GM.CurrentPlayerBody.TorsoTransform.position;
                }
                else
                {
                    targetVector = SpawnPoints_Turrets[UnityEngine.Random.Range(0, SpawnPoints_Turrets.Count)].position;
                }
                
                Sosig enemy = M.SpawnEnemy(enemyTemplate, AttackVectors[vectorIndex].SpawnPoints_Sosigs_Attack[vectorSpawnPoint], curPhase.IFFUsed, true, targetVector, true);
                ActiveSosigs.Add(enemy);

                TNHTweakerLogger.Log("TNHTWEAKER -- SOSIG SPAWNED", TNHTweakerLogger.LogType.General);

                //At this point, the leader has been spawned, so always set enemy to be regulars
                enemyTemplate = ManagerSingleton<IM>.Instance.odicSosigObjsByID[curPhase.EType];
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
