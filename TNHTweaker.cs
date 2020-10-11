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

    public class CustomCharData
    {
        public List<CustomTNHLevel> Levels = new List<CustomTNHLevel>();
    }

    public class CustomTNHLevel
    {
        public int AdditionalSupplyPoints = 0;
        public int MaxBoxesSpawned = 4;
        public int MinBoxesSpawned = 2;
        public int MaxTokensPerSupply = 1;
        public int MinTokensPerSupply = 1;
        public float BoxTokenChance = 1;
        public float BoxHealthChance = 0.8f;
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
        private static Dictionary<string, Sprite> equipmentIcons = new Dictionary<string, Sprite>();

        private static float timeTillForcedSpawn;
        private static string characterPath;

        private static List<TNH_CharacterDef> customCharacters = new List<TNH_CharacterDef>();

        private static Dictionary<TNH_CharacterDef,CustomCharData> customCharDict = new Dictionary<TNH_CharacterDef, CustomCharData>();

        private void Awake()
        {
            Debug.Log("Patching!");
            Harmony.CreateAndPatchAll(typeof(TNHTweaker));
            Debug.Log("Patched!");
            
            LoadConfigFile();

            SetupCharacterDirectory();

            TNHTweakerUtils.CreateTemplateFile(characterPath);
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

            TNHTweakerUtils.CreateObjectIDFile(characterPath);
            TNHTweakerUtils.CreateSosigIDFile(characterPath);

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

            equipmentIcons = TNHTweakerUtils.GetAllIcons(___CharDatabase);

            TNHTweakerUtils.CreateIconIDFile(characterPath, equipmentIcons.Keys.ToList());

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
                        TNHTweakerUtils.PrintCharacterInfo(ch);
                    }
                }

                else
                {
                    Debug.Log("TNHTWEAKER -- PRINTING ALL LOADED CHARACTERS\n");
                    foreach (TNH_CharacterDef ch in ___CharDatabase.Characters)
                    {
                        TNHTweakerUtils.PrintCharacterInfo(ch);
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


                TNH_CharacterDef character = ObjectBuilder.GetCharacterFromString(characterDir, customCharDict, equipmentIcons, backupCharacter);
                customCharacters.Add(character);

                Debug.Log("TNHTWEAKER -- CHARACTER LOADED: " + character.DisplayName);
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

                P.Patrols[0].EType = (SosigEnemyID)Enum.Parse(typeof(SosigEnemyID), enemyType, true);
                P.Patrols[0].LType = (SosigEnemyID)Enum.Parse(typeof(SosigEnemyID), leaderType, true);

                //Allow for an IFF to be 0 instead of 1
                if (allowFriendlyPatrols.Value && patrolTeams.Value >= 2)
                {
                    team = team - 1;
                }
                P.Patrols[0].IFFUsed = team;
                
            }
            
            Debug.Log("TNHTWEAKER -- LISTING CURRENT PATROLS AVAILABLE:");
            TNHTweakerUtils.PrintPatrolList(P);

            return true;
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
                if(characterData.Levels.Count > ___m_level)
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
                    for(int i = 0; i < characterData.Levels[___m_level].AdditionalSupplyPoints && i < possiblePoints.Count; i++)
                    {
                        TNH_SupplyPoint.SupplyPanelType panelType = (TNH_SupplyPoint.SupplyPanelType)UnityEngine.Random.Range(1,3);
                        
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
            CustomCharData characterData;
            if (customCharDict.TryGetValue(___M.C, out characterData)){

                int currLevel = (int)Traverse.Create(___M).Field("m_level").GetValue();

                if(characterData.Levels.Count > currLevel)
                {
                    __instance.SpawnPoints_Boxes.Shuffle();

                    int boxesToSpawn = UnityEngine.Random.Range(characterData.Levels[currLevel].MinBoxesSpawned, characterData.Levels[currLevel].MaxBoxesSpawned + 1);

                    Debug.Log("TNHTWEAKER -- GOING TO SPAWN " + boxesToSpawn + " BOXES AT THIS SUPPLY POINT -- MIN (" + characterData.Levels[currLevel].MinBoxesSpawned + "), MAX (" + characterData.Levels[currLevel].MaxBoxesSpawned + ")");

                    for (int i = 0; i < boxesToSpawn; i++)
                    {
                        Transform spawnTransform = __instance.SpawnPoints_Boxes[UnityEngine.Random.Range(0, __instance.SpawnPoints_Boxes.Count)];
                        Vector3 position = spawnTransform.position + Vector3.up * 0.1f + Vector3.right * UnityEngine.Random.Range(-0.5f, 0.5f) + Vector3.forward * UnityEngine.Random.Range(-0.5f, 0.5f);
                        Quaternion rotation = Quaternion.Slerp(spawnTransform.rotation, UnityEngine.Random.rotation, 0.1f);
                        GameObject box = Instantiate(___M.Prefabs_ShatterableCrates[UnityEngine.Random.Range(0, ___M.Prefabs_ShatterableCrates.Count)], position, rotation);
                        ___m_spawnBoxes.Add(box);
                        Debug.Log("TNHTWEAKER -- BOX SPAWNED");
                    }

                    int tokensSpawned = 0;

                    foreach(GameObject boxObj in ___m_spawnBoxes)
                    {
                        if(tokensSpawned < characterData.Levels[currLevel].MinTokensPerSupply)
                        {
                            boxObj.GetComponent<TNH_ShatterableCrate>().SetHoldingToken(___M);
                            tokensSpawned += 1;
                        }

                        else if (tokensSpawned < characterData.Levels[currLevel].MaxTokensPerSupply && UnityEngine.Random.value < characterData.Levels[currLevel].BoxTokenChance)
                        {
                            boxObj.GetComponent<TNH_ShatterableCrate>().SetHoldingToken(___M);
                            tokensSpawned += 1;
                        }

                        else if (UnityEngine.Random.value < characterData.Levels[currLevel].BoxHealthChance)
                        {
                            boxObj.GetComponent<TNH_ShatterableCrate>().SetHoldingHealth(___M);
                        }
                    }

                    return false;
                }
            }
            
            return true;
        }


        public static void SpawnGrenades(List<TNH_HoldPoint.AttackVector> AttackVectors, TNH_Manager M, int m_phaseIndex)
        {
            CustomCharData characterData;
            if (customCharDict.TryGetValue(M.C, out characterData))
            {

                int currLevel = (int)Traverse.Create(M).Field("m_level").GetValue();

                if (characterData.Levels.Count > currLevel)
                {
                    if(characterData.Levels[currLevel].Phases.Count > m_phaseIndex)
                    {
                        float grenadeChance = characterData.Levels[currLevel].Phases[m_phaseIndex].GrenadeChance;
                        string grenadeType = characterData.Levels[currLevel].Phases[m_phaseIndex].GrenadeType;
                        
                        if(grenadeChance >= UnityEngine.Random.Range(0f, 1f))
                        {
                            Debug.Log("TNHTWEAKER -- THROWING A GRENADE ");

                            //Get a random grenade vector to spawn a grenade at
                            TNH_HoldPoint.AttackVector randAttackVector = AttackVectors[UnityEngine.Random.Range(0, AttackVectors.Count)];

                            //Instantiate the grenade object
                            GameObject grenadeObject = Instantiate(IM.OD[grenadeType].GetGameObject(), randAttackVector.GrenadeVector.position, randAttackVector.GrenadeVector.rotation);

                            //Give the grenade an initial velocity based on the grenade vector
                            grenadeObject.GetComponent<Rigidbody>().velocity = 15 * randAttackVector.GrenadeVector.forward;
                            grenadeObject.GetComponent<SosigWeapon>().FuseGrenade();
                        }
                    }
                }
            }
        }



        public static void SpawnHoldEnemyGroup(TNH_HoldChallenge.Phase curPhase, List<TNH_HoldPoint.AttackVector> AttackVectors, List<Transform> SpawnPoints_Turrets, List<Sosig> ActiveSosigs, TNH_Manager M, ref bool isFirstWave)
        {
            Debug.Log("TNHTWEAKER -- SPAWNING AN ENEMY WAVE");

            //TODO add custom property form MinDirections
            int numAttackVectors = UnityEngine.Random.Range(1, curPhase.MaxDirections + 1);
            numAttackVectors = Mathf.Clamp(numAttackVectors, 1, AttackVectors.Count);

            //Set first enemy to be spawned as leader
            SosigEnemyTemplate enemyTemplate = ManagerSingleton<IM>.Instance.odicSosigObjsByID[curPhase.LType];
            int enemiesToSpawn = UnityEngine.Random.Range(curPhase.MinEnemies, curPhase.MaxEnemies + 1);

            int sosigsSpawned = 0;
            int vectorSpawnPoint = 0;
            Vector3 targetVector;
            while(sosigsSpawned < enemiesToSpawn)
            {
                //Loop through each attack vector, and spawn a sosig at the current selected point
                for (int i = 0; i < numAttackVectors; i++)
                {
                    Debug.Log("TNHTWEAKER -- SPAWNING AT ATTCK VECTOR: " + i);

                    if (AttackVectors[i].SpawnPoints_Sosigs_Attack.Count <= vectorSpawnPoint) return;

                    //Spawn the enemy
                    targetVector = SpawnPoints_Turrets[UnityEngine.Random.Range(0, SpawnPoints_Turrets.Count)].position;
                    Sosig enemy = M.SpawnEnemy(enemyTemplate, AttackVectors[i].SpawnPoints_Sosigs_Attack[vectorSpawnPoint], curPhase.IFFUsed, true, targetVector, true);
                    ActiveSosigs.Add(enemy);

                    Debug.Log("TNHTWEAKER -- SOSIG SPAWNED");

                    //At this point, the leader has been spawned, so always set enemy to be regulars
                    enemyTemplate = ManagerSingleton<IM>.Instance.odicSosigObjsByID[curPhase.EType];
                    sosigsSpawned += 1;
                }

                vectorSpawnPoint += 1;
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
                if (instantRespawn.Value)
                {
                    ___m_tickDownToNextGroupSpawn = 0;
                }

                else if(___m_state == TNH_HoldPoint.HoldState.Analyzing)
                {
                    ___m_tickDownToNextGroupSpawn -= Time.deltaTime;
                }
            }

            if(___m_tickDownToNextGroupSpawn <= 0)
            {
                Debug.Log("TNHTWEAKER -- TIME TO SPAWN!");
                Debug.Log("Max Alive: " + ___m_curPhase.MaxEnemiesAlive);
                Debug.Log("Max Spawned: " + ___m_curPhase.MaxEnemies);
                Debug.Log("Currently Active: " + ___m_activeSosigs.Count);
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

                SpawnHoldEnemyGroup(___m_curPhase, ___AttackVectors, ___SpawnPoints_Turrets, ___m_activeSosigs, ___M, ref ___m_isFirstWave);
                ___m_hasThrownNadesInWave = false;
                ___m_tickDownToNextGroupSpawn = ___m_curPhase.SpawnCadence;
            }


            return false;
        }



        [HarmonyPatch(typeof(TNH_SupplyPoint), "Configure")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool PrintSupplyPoint(TNH_SupplyPoint.SupplyPanelType panelType)
        {
            Debug.Log("TNHTWEAKER -- CONFIGURING SUPPLY POINT -- PANEL TYPE: " + panelType.ToString());
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
