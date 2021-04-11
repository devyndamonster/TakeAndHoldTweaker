using ADepIn;
using BepInEx.Configuration;
using Deli;
using Deli.Immediate;
using Deli.Setup;
using Deli.VFS;
using Deli.Runtime;
using FistVR;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TNHTweaker.ObjectTemplates;
using TNHTweaker.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Deli.Runtime.Yielding;
using Anvil;

namespace TNHTweaker
{
    public class TNHTweaker : DeliBehaviour
    {
        private static ConfigEntry<bool> printCharacters;
        private static ConfigEntry<bool> logTNH;
        private static ConfigEntry<bool> logFileReads;
        private static ConfigEntry<bool> allowLog;
        private static ConfigEntry<bool> buildCharacterFiles;

        private static string OutputFilePath;

        private static List<int> spawnedBossIndexes = new List<int>();
        private static List<GameObject> SpawnedConstructors = new List<GameObject>();
        private static List<GameObject> SpawnedPanels = new List<GameObject>();

        private static bool preventOutfitFunctionality = false;

        ///////////////////////////////////////////////
        //INITIALIZING THE TAKE AND HOLD TWEAKER PLUGIN
        ///////////////////////////////////////////////

        /// <summary>
        /// First method that gets called
        /// </summary>
        private void Awake()
        {
            TNHTweakerLogger.Init();
            TNHTweakerLogger.Log("Hello World (from TNH Tweaker)", TNHTweakerLogger.LogType.General);

            Harmony.CreateAndPatchAll(typeof(TNHTweaker));

            Stages.Setup += OnSetup;
        }


        /// <summary>
        /// Performs initial setup for TNH Tweaker
        /// </summary>
        /// <param name="stage"></param>
        private void OnSetup(SetupStage stage)
        {
            LoadConfigFile();
            SetupOutputDirectory();
            LoadPanelSprites(stage);

            stage.SetupAssetLoaders[Source, "sosig"] = new SosigLoader().LoadAsset;
            stage.SetupAssetLoaders[Source, "vault_file"] = new VaultFileLoader().LoadAsset;
            stage.SetupAssetLoaders[Source, "character"] = new CharacterLoader().LoadAsset;
        }



        /// <summary>
        /// Loads the sprites used in secondary panels in TNH
        /// </summary>
        private void LoadPanelSprites(SetupStage stage)
        {
            IFileHandle? file = Source.Resources.GetFile("mag_upgrade.png");
            Sprite result = TNHTweakerUtils.LoadSprite(file);
            LoadedTemplateManager.PanelSprites.Add(PanelType.MagUpgrader, result);

            file = Source.Resources.GetFile("full_auto.png");
            result = TNHTweakerUtils.LoadSprite(file);
            LoadedTemplateManager.PanelSprites.Add(PanelType.AddFullAuto, result);

            file = Source.Resources.GetFile("ammo_purchase.png");
            result = TNHTweakerUtils.LoadSprite(file);
            LoadedTemplateManager.PanelSprites.Add(PanelType.AmmoPurchase, result);

            file = Source.Resources.GetFile("mag_purchase.png");
            result = TNHTweakerUtils.LoadSprite(file);
            LoadedTemplateManager.PanelSprites.Add(PanelType.MagPurchase, result);

            file = Source.Resources.GetFile("gas_up.png");
            result = TNHTweakerUtils.LoadSprite(file);
            LoadedTemplateManager.PanelSprites.Add(PanelType.FireRateUp, result);

            file = Source.Resources.GetFile("gas_down.png");
            result = TNHTweakerUtils.LoadSprite(file);
            LoadedTemplateManager.PanelSprites.Add(PanelType.FireRateDown, result);
        }




        /// <summary>
        /// Loads the bepinex config file, and applys those settings
        /// </summary>
        private void LoadConfigFile()
        {
            TNHTweakerLogger.Log("TNHTweaker -- Getting config file", TNHTweakerLogger.LogType.File);

            buildCharacterFiles = Source.Config.Bind("General",
                                    "BuildCharacterFiles",
                                    false,
                                    "If true, files useful for character creation will be generated in TNHTweaker folder");


            allowLog = Source.Config.Bind("Debug",
                                    "EnableLogging",
                                    true,
                                    "Set to true to enable logging");

            printCharacters = Source.Config.Bind("Debug",
                                         "LogCharacterInfo",
                                         false,
                                         "Decide if should print all character info");

            logTNH = Source.Config.Bind("Debug",
                                    "LogTNH",
                                    false,
                                    "If true, general TNH information will be logged");

            logFileReads = Source.Config.Bind("Debug",
                                    "LogFileReads",
                                    false,
                                    "If true, reading from a file will log the reading process");

            TNHTweakerLogger.AllowLogging = allowLog.Value;
            TNHTweakerLogger.LogCharacter = printCharacters.Value;
            TNHTweakerLogger.LogTNH = logTNH.Value;
            TNHTweakerLogger.LogFile = logFileReads.Value;
        }


        /// <summary>
        /// Creates the main TNH Tweaker file folder
        /// </summary>
        private void SetupOutputDirectory()
        {
            OutputFilePath = Application.dataPath.Replace("/h3vr_Data", "/TNH_Tweaker");

            if (!Directory.Exists(OutputFilePath))
            {
                Directory.CreateDirectory(OutputFilePath);
            }
        }



        /// <summary>
        /// Every time an asset bundle is loaded asyncronously, the callback is added to a global list which can be monitored to see when loading is complete
        /// </summary>
        /// <param name="__result"></param>
        [HarmonyPatch(typeof(AnvilManager), "GetBundleAsync")]
        [HarmonyPostfix]
        public static void AddMonitoredAnvilCallback(AnvilCallback<AssetBundle> __result)
        {
            AsyncLoadMonitor.CallbackList.Add(__result);
            TNHTweakerLogger.Log("TNHTweaker -- Added AssetBundle anvil callback to monitored callbacks!", TNHTweakerLogger.LogType.File);
        }


        //////////////////////////////////
        //INITIALIZING TAKE AND HOLD SCENE
        //////////////////////////////////


        /// <summary>
        /// Performs initial setup of the TNH Scene when loaded
        /// </summary>
        /// <param name="___Categories"></param>
        /// <param name="___CharDatabase"></param>
        /// <param name="__instance"></param>
        /// <returns></returns>
        [HarmonyPatch(typeof(TNH_UIManager), "Start")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool InitTNH(List<TNH_UIManager.CharacterCategory> ___Categories, TNH_CharacterDatabase ___CharDatabase, TNH_UIManager __instance)
        {
            TNHTweakerLogger.Log("Start method of TNH_UIManager just got called!", TNHTweakerLogger.LogType.General);

            GM.TNHOptions.Char = TNH_Char.DD_ClassicLoudoutLouis;

            Text magazineCacheText = CreateMagazineCacheText(__instance);
            ExpandCharacterUI(__instance);

            //Perform first time setup of all files
            if (!TNHMenuInitializer.TNHInitialized)
            {
                SceneLoader sceneHotDog = FindObjectOfType<SceneLoader>();

                if (!TNHMenuInitializer.MagazineCacheFailed)
                {
                    AnvilManager.Run(TNHMenuInitializer.InitializeTNHMenuAsync(OutputFilePath, magazineCacheText, sceneHotDog, ___Categories, ___CharDatabase, __instance, buildCharacterFiles.Value));
                }

                //If the magazine cache has previously failed, we shouldn't let the player continue
                else
                {
                    sceneHotDog.gameObject.SetActive(false);
                    magazineCacheText.text = "FAILED! SEE LOG!";
                }
                
            }
            else
            {
                TNHMenuInitializer.RefreshTNHUI(__instance, ___Categories, ___CharDatabase);
                magazineCacheText.text = "CACHE BUILT";
            }

            return true;
        }



        /// <summary>
        /// Creates the additional text above the character select screen, and returns that text component
        /// </summary>
        /// <param name="manager"></param>
        /// <returns></returns>
        private static Text CreateMagazineCacheText(TNH_UIManager manager)
        {
            Text magazineCacheText = Instantiate(manager.SelectedCharacter_Title.gameObject, manager.SelectedCharacter_Title.transform.parent).GetComponent<Text>();
            magazineCacheText.transform.localPosition = new Vector3(0, 550, 0);
            magazineCacheText.transform.localScale = new Vector3(2, 2, 2);
            magazineCacheText.text = "EXAMPLE TEXT";

            return magazineCacheText;
        }


        /// <summary>
        /// Adds more space for characters to be displayed in the TNH menu
        /// </summary>
        /// <param name="manager"></param>
        private static void ExpandCharacterUI(TNH_UIManager manager)
        {
            //Add additional character buttons
            OptionsPanel_ButtonSet buttonSet = manager.LBL_CharacterName[1].transform.parent.GetComponent<OptionsPanel_ButtonSet>();
            List<FVRPointableButton> buttonList = new List<FVRPointableButton>(buttonSet.ButtonsInSet);
            for (int i = 0; i < 6; i++)
            {
                Text newCharacterLabel = Instantiate(manager.LBL_CharacterName[1].gameObject, manager.LBL_CharacterName[1].transform.parent).GetComponent<Text>();
                Button newButton = newCharacterLabel.gameObject.GetComponent<Button>();

                int buttonIndex = 6 + i;

                newButton.onClick = new Button.ButtonClickedEvent();
                newButton.onClick.AddListener(() => { manager.SetSelectedCharacter(buttonIndex); });
                newButton.onClick.AddListener(() => { buttonSet.SetSelectedButton(buttonIndex); });

                manager.LBL_CharacterName.Add(newCharacterLabel);
                buttonList.Add(newCharacterLabel.gameObject.GetComponent<FVRPointableButton>());
            }
            buttonSet.ButtonsInSet = buttonList.ToArray();

            //Adjust buttons to be tighter together
            float prevY = manager.LBL_CharacterName[0].transform.localPosition.y;
            for (int i = 1; i < manager.LBL_CharacterName.Count; i++)
            {
                prevY = prevY - 35f;
                manager.LBL_CharacterName[i].transform.localPosition = new Vector3(250, prevY, 0);
            }
        }



        /////////////////////////////
        //PATCHES FOR PATROL SPAWNING
        /////////////////////////////


        /// <summary>
        /// Finds an index in the patrols list which can spawn, preventing bosses that have already spawned from spawning again
        /// </summary>
        /// <param name="patrols">List of patrols that can spawn</param>
        /// <returns>Returns -1 if no valid index is found, otherwise returns a random index for a patrol </returns>
        private static int GetValidPatrolIndex(List<TNH_PatrolChallenge.Patrol> patrols)
        {
            int index = UnityEngine.Random.Range(0, patrols.Count);
            int attempts = 0;

            while(spawnedBossIndexes.Contains(index) && attempts < patrols.Count)
            {
                attempts += 1;
                index += 1;
                if (index >= patrols.Count) index = 0;
            }

            if (spawnedBossIndexes.Contains(index)) return -1;

            return index;
        }


        /// <summary>
        /// Decides the spawning location and patrol pathing for sosig patrols, and then spawns the patrol
        /// </summary>
        /// <param name="P"></param>
        /// <param name="curStandardIndex"></param>
        /// <param name="excludeHoldIndex"></param>
        /// <param name="isStart"></param>
        /// <param name="__instance"></param>
        /// <param name="___m_curLevel"></param>
        /// <param name="___m_patrolSquads"></param>
        /// <param name="___m_timeTilPatrolCanSpawn"></param>
        /// <returns></returns>
        [HarmonyPatch(typeof(TNH_Manager), "GenerateValidPatrol")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool GenerateValidPatrolReplacement(TNH_PatrolChallenge P, int curStandardIndex, int excludeHoldIndex, bool isStart, TNH_Manager __instance, TNH_Progression.Level ___m_curLevel, List<TNH_Manager.SosigPatrolSquad> ___m_patrolSquads, ref float ___m_timeTilPatrolCanSpawn)
        {
            TNHTweakerLogger.Log("TNHTWEAKER -- Generating a patrol -- There are currently " + ___m_patrolSquads.Count + " patrols active", TNHTweakerLogger.LogType.TNH);

            if (P.Patrols.Count < 1) return false;

            //Get a valid patrol index, and exit if there are no valid patrols
            int patrolIndex = GetValidPatrolIndex(P.Patrols);
            if(patrolIndex == -1)
            {
                TNHTweakerLogger.Log("TNHTWEAKER -- No valid patrols can spawn", TNHTweakerLogger.LogType.TNH);
                ___m_timeTilPatrolCanSpawn = 999;
                return false;
            }

            TNHTweakerLogger.Log("TNHTWEAKER -- Valid patrol found", TNHTweakerLogger.LogType.TNH);

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

        
        /// <summary>
        /// Spawns a patrol at the desire patrol point
        /// </summary>
        /// <param name="HoldPointStart"></param>
        /// <param name="instance"></param>
        /// <param name="level"></param>
        /// <param name="patrol"></param>
        /// <param name="patrolIndex"></param>
        /// <returns></returns>
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

            TNHTweakerLogger.Log("TNHTWEAKER -- Is patrol a boss?: " + currPatrol.IsBoss, TNHTweakerLogger.LogType.TNH);

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



        ///////////////////////////////////////////
        //PATCHES FOR SUPPLY POINTS AND TAKE POINTS
        ///////////////////////////////////////////


        [HarmonyPatch(typeof(TNH_SupplyPoint), "ConfigureAtBeginning")]
        [HarmonyPrefix]
        public static bool SpawnStartingEquipment(TNH_SupplyPoint __instance)
        {
            __instance.m_trackedObjects.Clear();
            if(__instance.M.ItemSpawnerMode == TNH_ItemSpawnerMode.On)
            {
                __instance.M.ItemSpawner.transform.position = __instance.SpawnPoints_Panels[0].position + Vector3.up * 0.8f;
                __instance.M.ItemSpawner.transform.rotation = __instance.SpawnPoints_Panels[0].rotation;
                __instance.M.ItemSpawner.SetActive(true);
            }

            for (int i = 0; i < __instance.SpawnPoint_Tables.Count; i++)
            {
                GameObject item = Instantiate(__instance.M.Prefab_MetalTable, __instance.SpawnPoint_Tables[i].position, __instance.SpawnPoint_Tables[i].rotation);
                __instance.m_trackedObjects.Add(item);
            }

            CustomCharacter character = LoadedTemplateManager.LoadedCharactersDict[__instance.M.C];
            if (character.HasPrimaryWeapon)
            {
                EquipmentGroup selectedGroup = character.PrimaryWeapon.PrimaryGroup;
                if(selectedGroup == null) selectedGroup = character.PrimaryWeapon.BackupGroup;
                
                if(selectedGroup != null)
                {
                    selectedGroup = selectedGroup.GetSpawnedEquipmentGroups().GetRandom();
                    FVRObject selectedItem = IM.OD[selectedGroup.GetObjects().GetRandom()];
                    GameObject weaponCase = __instance.M.SpawnWeaponCase(__instance.M.Prefab_WeaponCaseLarge, __instance.SpawnPoint_CaseLarge.position, __instance.SpawnPoint_CaseLarge.forward, selectedItem, selectedGroup.NumMagsSpawned, selectedGroup.NumRoundsSpawned, selectedGroup.MinAmmoCapacity, selectedGroup.MaxAmmoCapacity);
                    __instance.m_trackedObjects.Add(weaponCase);
                    weaponCase.GetComponent<TNH_WeaponCrate>().M = __instance.M;
                }
            }

            if (character.HasSecondaryWeapon)
            {
                EquipmentGroup selectedGroup = character.SecondaryWeapon.PrimaryGroup;
                if (selectedGroup == null) selectedGroup = character.SecondaryWeapon.BackupGroup;

                if (selectedGroup != null)
                {
                    selectedGroup = selectedGroup.GetSpawnedEquipmentGroups().GetRandom();
                    FVRObject selectedItem = IM.OD[selectedGroup.GetObjects().GetRandom()];
                    GameObject weaponCase = __instance.M.SpawnWeaponCase(__instance.M.Prefab_WeaponCaseSmall, __instance.SpawnPoint_CaseSmall.position, __instance.SpawnPoint_CaseSmall.forward, selectedItem, selectedGroup.NumMagsSpawned, selectedGroup.NumRoundsSpawned, selectedGroup.MinAmmoCapacity, selectedGroup.MaxAmmoCapacity);
                    __instance.m_trackedObjects.Add(weaponCase);
                    weaponCase.GetComponent<TNH_WeaponCrate>().M = __instance.M;
                }
            }

            if (character.HasTertiaryWeapon)
            {
                EquipmentGroup selectedGroup = character.TertiaryWeapon.PrimaryGroup;
                if (selectedGroup == null) selectedGroup = character.TertiaryWeapon.BackupGroup;

                if (selectedGroup != null)
                {
                    selectedGroup = selectedGroup.GetSpawnedEquipmentGroups().GetRandom();
                    FVRObject selectedItem = IM.OD[selectedGroup.GetObjects().GetRandom()];
                    GameObject item = Instantiate(selectedItem.GetGameObject(), __instance.SpawnPoint_Melee.position, __instance.SpawnPoint_Melee.rotation);
                    __instance.M.AddObjectToTrackedList(item);
                }
            }

            if (character.HasPrimaryItem)
            {
                EquipmentGroup selectedGroup = character.PrimaryItem.PrimaryGroup;
                if (selectedGroup == null) selectedGroup = character.PrimaryItem.BackupGroup;

                if (selectedGroup != null)
                {
                    selectedGroup = selectedGroup.GetSpawnedEquipmentGroups().GetRandom();
                    FVRObject selectedItem = IM.OD[selectedGroup.GetObjects().GetRandom()];
                    GameObject item = Instantiate(selectedItem.GetGameObject(), __instance.SpawnPoints_SmallItem[0].position, __instance.SpawnPoints_SmallItem[0].rotation);
                    __instance.M.AddObjectToTrackedList(item);
                }
            }

            if (character.HasSecondaryItem)
            {
                EquipmentGroup selectedGroup = character.SecondaryItem.PrimaryGroup;
                if (selectedGroup == null) selectedGroup = character.SecondaryItem.BackupGroup;

                if (selectedGroup != null)
                {
                    selectedGroup = selectedGroup.GetSpawnedEquipmentGroups().GetRandom();
                    FVRObject selectedItem = IM.OD[selectedGroup.GetObjects().GetRandom()];
                    GameObject item = Instantiate(selectedItem.GetGameObject(), __instance.SpawnPoints_SmallItem[1].position, __instance.SpawnPoints_SmallItem[1].rotation);
                    __instance.M.AddObjectToTrackedList(item);
                }
            }

            if (character.HasTertiaryItem)
            {
                EquipmentGroup selectedGroup = character.TertiaryItem.PrimaryGroup;
                if (selectedGroup == null) selectedGroup = character.TertiaryItem.BackupGroup;

                if (selectedGroup != null)
                {
                    selectedGroup = selectedGroup.GetSpawnedEquipmentGroups().GetRandom();
                    FVRObject selectedItem = IM.OD[selectedGroup.GetObjects().GetRandom()];
                    GameObject item = Instantiate(selectedItem.GetGameObject(), __instance.SpawnPoints_SmallItem[2].position, __instance.SpawnPoints_SmallItem[2].rotation);
                    __instance.M.AddObjectToTrackedList(item);
                }
            }

            if (character.HasShield)
            {
                EquipmentGroup selectedGroup = character.Shield.PrimaryGroup;
                if (selectedGroup == null) selectedGroup = character.Shield.BackupGroup;

                if (selectedGroup != null)
                {
                    selectedGroup = selectedGroup.GetSpawnedEquipmentGroups().GetRandom();
                    FVRObject selectedItem = IM.OD[selectedGroup.GetObjects().GetRandom()];
                    GameObject item = Instantiate(selectedItem.GetGameObject(), __instance.SpawnPoint_Shield.position, __instance.SpawnPoint_Shield.rotation);
                    __instance.M.AddObjectToTrackedList(item);
                }
            }

            return false;
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

            TNHTweakerLogger.Log("Makarov rounds: " + IM.OD["Makarov"].CompatibleSingleRounds.Count, TNHTweakerLogger.LogType.General);

            //Clear the TNH radar
            if (__instance.RadarMode == TNHModifier_RadarMode.Standard)
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
            level.PossiblePanelTypes.Shuffle();

            TNHTweakerLogger.Log("TNHTWEAKER -- Panel types for this hold:", TNHTweakerLogger.LogType.TNH);
            level.PossiblePanelTypes.ForEach(o => TNHTweakerLogger.Log(o.ToString(), TNHTweakerLogger.LogType.TNH));

            TNHTweakerLogger.Log("TNHTWEAKER -- Spawning " + numSupplyPoints + " supply points", TNHTweakerLogger.LogType.TNH);
            for (int i = 0; i < numSupplyPoints; i++)
            {
                TNH_SupplyPoint supplyPoint = __instance.SupplyPoints[supplyPointsIndexes[i]];
                ConfigureSupplyPoint(supplyPoint, level, i);
                TAH_ReticleContact contact = __instance.TAHReticle.RegisterTrackedObject(supplyPoint.SpawnPoint_PlayerSpawn, TAH_ReticleContact.ContactType.Supply);
                supplyPoint.SetContact(contact);
            }

            if(__instance.BGAudioMode == TNH_BGAudioMode.Default)
            {
                __instance.FMODController.SwitchTo(0, 2f, false, false);
            }

            return false;
        }

        public static void ConfigureSupplyPoint(TNH_SupplyPoint supplyPoint, Level level, int supplyIndex)
        {
            TNHTweakerLogger.Log("TNHTWEAKER -- Configuring supply point : " + supplyIndex, TNHTweakerLogger.LogType.TNH);

            supplyPoint.T = level.SupplyChallenge.GetTakeChallenge();

            SpawnSupplyGroup(supplyPoint, level);

            SpawnSupplyTurrets(supplyPoint, level);

            int numConstructors = UnityEngine.Random.Range(level.MinConstructors, level.MaxConstructors + 1);

            SpawnSupplyConstructor(supplyPoint, numConstructors);

            SpawnSecondarySupplyPanel(supplyPoint, level, numConstructors, supplyIndex);

            SpawnSupplyBoxes(supplyPoint, level);

            supplyPoint.m_hasBeenVisited = false;
        }


        public static void SpawnSupplyConstructor(TNH_SupplyPoint point, int toSpawn)
        {
            TNHTweakerLogger.Log("TNHTWEAKER -- Spawning constructor panel", TNHTweakerLogger.LogType.TNH);

            point.SpawnPoints_Panels.Shuffle();
            
            for(int i = 0; i < toSpawn && i < point.SpawnPoints_Panels.Count; i++)
            {
                GameObject constructor = point.M.SpawnObjectConstructor(point.SpawnPoints_Panels[i]);
                SpawnedConstructors.Add(constructor);
            }
        }
        
        public static void SpawnSecondarySupplyPanel(TNH_SupplyPoint point, Level level, int startingPanelIndex, int supplyIndex)
        {
            TNHTweakerLogger.Log("TNHTWEAKER -- Spawning secondary panels", TNHTweakerLogger.LogType.TNH);

            PanelType panelType;
            List<PanelType> panelTypes = new List<PanelType>(level.PossiblePanelTypes);

            if (point.M.EquipmentMode != TNHSetting_EquipmentMode.LimitedAmmo)
            {
                TNHTweakerLogger.Log("TNHTWEAKER -- Removing mag duplicator since we are on limited ammo mode", TNHTweakerLogger.LogType.TNH);
                panelTypes.Remove(PanelType.MagDuplicator);
            }

            int numPanels = UnityEngine.Random.Range(level.MinPanels, level.MaxPanels + 1);

            for (int i = startingPanelIndex; i < startingPanelIndex + numPanels && i < point.SpawnPoints_Panels.Count && panelTypes.Count > 0; i++)
            {
                TNHTweakerLogger.Log("TNHTWEAKER -- Panel index : " + i, TNHTweakerLogger.LogType.TNH);

                //If this is the first panel, we should ensure that it is an ammo resupply
                if (panelTypes.Contains(PanelType.AmmoReloader) && point.M.EquipmentMode == TNHSetting_EquipmentMode.LimitedAmmo && i == startingPanelIndex && supplyIndex == 0)
                {
                    TNHTweakerLogger.Log("TNHTWEAKER -- First supply and first panel on limited ammo, forcing ammo reloader to spawn", TNHTweakerLogger.LogType.TNH);
                    panelType = PanelType.AmmoReloader;
                    panelTypes.Remove(PanelType.AmmoReloader);
                }

                //Otherwise we just select a random panel from valid panels
                else
                {
                    if (supplyIndex >= panelTypes.Count) supplyIndex = 0;
                    panelType = panelTypes[supplyIndex];
                    supplyIndex += 1;

                    TNHTweakerLogger.Log("TNHTWEAKER -- Panel type selected : " + panelType, TNHTweakerLogger.LogType.TNH);
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
                    panel = point.M.SpawnMagDuplicator(point.SpawnPoints_Panels[i]);
                    panel.AddComponent(typeof(MagUpgrader));
                }

                else if (panelType == PanelType.AddFullAuto)
                {
                    panel = point.M.SpawnMagDuplicator(point.SpawnPoints_Panels[i]);
                    panel.AddComponent(typeof(FullAutoEnabler));
                }

                else if (panelType == PanelType.FireRateUp || panelType == PanelType.FireRateDown)
                {
                    panel = point.M.SpawnMagDuplicator(point.SpawnPoints_Panels[i]);
                    FireRateModifier component = (FireRateModifier)panel.AddComponent(typeof(FireRateModifier));
                    component.Init(panelType);
                }

                else if (panelType == PanelType.MagPurchase)
                {
                    panel = point.M.SpawnMagDuplicator(point.SpawnPoints_Panels[i]);
                    panel.AddComponent(typeof(MagPurchaser));
                }

                else if (panelType == PanelType.AmmoPurchase)
                {
                    panel = point.M.SpawnMagDuplicator(point.SpawnPoints_Panels[i]);
                    panel.AddComponent(typeof(AmmoPurchaser));
                }

                //If we spawned a panel, add it to the global list
                if (panel != null)
                {
                    TNHTweakerLogger.Log("TNHTWEAKER -- Panel spawned successfully", TNHTweakerLogger.LogType.TNH);
                    SpawnedPanels.Add(panel);
                }
                else
                {
                    TNHTweakerLogger.LogWarning("TNHTWEAKER -- Failed to spawn secondary panel!");
                }
            }
        }

        public static void SpawnSupplyGroup(TNH_SupplyPoint point, Level level)
        {
            point.SpawnPoints_Sosigs_Defense.Shuffle<Transform>();

            for (int i = 0; i < level.SupplyChallenge.NumGuards && i < point.SpawnPoints_Sosigs_Defense.Count; i++)
            {
                Transform transform = point.SpawnPoints_Sosigs_Defense[i];
                SosigEnemyTemplate template = ManagerSingleton<IM>.Instance.odicSosigObjsByID[level.SupplyChallenge.GetTakeChallenge().GID];
                SosigTemplate customTemplate = LoadedTemplateManager.LoadedSosigsDict[template];

                Sosig enemy = SpawnEnemy(customTemplate, LoadedTemplateManager.LoadedCharactersDict[point.M.C], transform, point.M.AI_Difficulty, level.SupplyChallenge.IFFUsed, false, transform.position, true);

                point.m_activeSosigs.Add(enemy);
            }
        }


        public static void SpawnSupplyTurrets(TNH_SupplyPoint point, Level level)
        {
            point.SpawnPoints_Turrets.Shuffle<Transform>();
            FVRObject turretPrefab = point.M.GetTurretPrefab(level.SupplyChallenge.TurretType);

            for (int i = 0; i < level.SupplyChallenge.NumTurrets && i < point.SpawnPoints_Turrets.Count; i++)
            {
                Vector3 pos = point.SpawnPoints_Turrets[i].position + Vector3.up * 0.25f;
                AutoMeater turret = Instantiate<GameObject>(turretPrefab.GetGameObject(), pos, point.SpawnPoints_Turrets[i].rotation).GetComponent<AutoMeater>();
                point.m_activeTurrets.Add(turret);
            }

        }


        public static void SpawnSupplyBoxes(TNH_SupplyPoint point, Level level)
        {
            point.SpawnPoints_Boxes.Shuffle();

            int boxesToSpawn = UnityEngine.Random.Range(level.MinBoxesSpawned, level.MaxBoxesSpawned + 1);

            TNHTweakerLogger.Log("TNHTWEAKER -- Going to spawn " + boxesToSpawn + " boxes at this point -- Min (" + level.MinBoxesSpawned + "), Max (" + level.MaxBoxesSpawned + ")", TNHTweakerLogger.LogType.TNH);

            for (int i = 0; i < boxesToSpawn; i++)
            {
                Transform spawnTransform = point.SpawnPoints_Boxes[UnityEngine.Random.Range(0, point.SpawnPoints_Boxes.Count)];
                Vector3 position = spawnTransform.position + Vector3.up * 0.1f + Vector3.right * UnityEngine.Random.Range(-0.5f, 0.5f) + Vector3.forward * UnityEngine.Random.Range(-0.5f, 0.5f);
                Quaternion rotation = Quaternion.Slerp(spawnTransform.rotation, UnityEngine.Random.rotation, 0.1f);
                GameObject box = Instantiate(point.M.Prefabs_ShatterableCrates[UnityEngine.Random.Range(0, point.M.Prefabs_ShatterableCrates.Count)], position, rotation);
                point.m_spawnBoxes.Add(box);
            }

            int tokensSpawned = 0;

            foreach (GameObject boxObj in point.m_spawnBoxes)
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
                //Debug.Log("Take challenge sosig ID : " + ___T.GID);
                SosigEnemyTemplate template = ManagerSingleton<IM>.Instance.odicSosigObjsByID[___T.GID];
                SosigTemplate customTemplate = LoadedTemplateManager.LoadedSosigsDict[template];

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



        ///////////////////////////////
        //PATCHES FOR DURING HOLD POINT
        ///////////////////////////////



        [HarmonyPatch(typeof(TNH_HoldPoint), "IdentifyEncryption")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool IdentifyEncryptionReplacement(TNH_HoldPoint __instance, TNH_HoldChallenge.Phase ___m_curPhase)
        {
            CustomCharacter character = LoadedTemplateManager.LoadedCharactersDict[__instance.M.C];
            Phase currentPhase = character.GetCurrentPhase(___m_curPhase);

            //If we shouldnt spawn any targets, we exit out early
            if ((currentPhase.MaxTargets < 1 && __instance.M.EquipmentMode == TNHSetting_EquipmentMode.Spawnlocking) ||
                (currentPhase.MaxTargetsLimited < 1 && __instance.M.EquipmentMode == TNHSetting_EquipmentMode.LimitedAmmo))
            {
                __instance.CompletePhase();
                return false;
            }

            __instance.m_state = TNH_HoldPoint.HoldState.Hacking;
            __instance.m_tickDownToFailure = 120f;

            __instance.M.EnqueueEncryptionLine(currentPhase.Encryptions[0]);

            __instance.DeleteAllActiveWarpIns();
            SpawnEncryptionReplacement(__instance, currentPhase);
            __instance.m_systemNode.SetNodeMode(TNH_HoldPointSystemNode.SystemNodeMode.Indentified);

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

        public static void SpawnGrenades(List<TNH_HoldPoint.AttackVector> AttackVectors, TNH_Manager M, int m_phaseIndex)
        {
            CustomCharacter character = LoadedTemplateManager.LoadedCharactersDict[M.C];
            Level currLevel = character.GetCurrentLevel(M.m_curLevel);
            Phase currPhase = currLevel.HoldPhases[m_phaseIndex];

            float grenadeChance = currPhase.GrenadeChance;
            string grenadeType = currPhase.GrenadeType;

            if (grenadeChance >= UnityEngine.Random.Range(0f, 1f))
            {
                TNHTweakerLogger.Log("TNHTWEAKER -- Throwing grenade ", TNHTweakerLogger.LogType.TNH);

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
            TNHTweakerLogger.Log("TNHTWEAKER -- Spawning enemy wave", TNHTweakerLogger.LogType.TNH);

            //TODO add custom property form MinDirections
            int numAttackVectors = UnityEngine.Random.Range(1, curPhase.MaxDirections + 1);
            numAttackVectors = Mathf.Clamp(numAttackVectors, 1, AttackVectors.Count);

            //Get the custom character data
            CustomCharacter character = LoadedTemplateManager.LoadedCharactersDict[M.C];
            Level currLevel = character.GetCurrentLevel(M.m_curLevel);
            Phase currPhase = currLevel.HoldPhases[phaseIndex];

            //Set first enemy to be spawned as leader
            SosigEnemyTemplate enemyTemplate = ManagerSingleton<IM>.Instance.odicSosigObjsByID[(SosigEnemyID)LoadedTemplateManager.SosigIDDict[currPhase.LeaderType]];
            int enemiesToSpawn = UnityEngine.Random.Range(curPhase.MinEnemies, curPhase.MaxEnemies + 1);

            int sosigsSpawned = 0;
            int vectorSpawnPoint = 0;
            Vector3 targetVector;
            int vectorIndex = 0;
            while (sosigsSpawned < enemiesToSpawn)
            {
                TNHTweakerLogger.Log("TNHTWEAKER -- Spawning at attack vector: " + vectorIndex, TNHTweakerLogger.LogType.TNH);

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

                //At this point, the leader has been spawned, so always set enemy to be regulars
                enemyTemplate = ManagerSingleton<IM>.Instance.odicSosigObjsByID[(SosigEnemyID)LoadedTemplateManager.SosigIDDict[currPhase.EnemyType.GetRandom<string>()]];
                sosigsSpawned += 1;

                vectorIndex += 1;
                if (vectorIndex >= numAttackVectors)
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
                if (___m_state == TNH_HoldPoint.HoldState.Analyzing)
                {
                    ___m_tickDownToNextGroupSpawn -= Time.deltaTime;
                }
            }

            if (!___m_hasThrownNadesInWave && ___m_tickDownToNextGroupSpawn <= 5f && !___m_isFirstWave)
            {
                SpawnGrenades(___AttackVectors, ___M, ___m_phaseIndex);
                ___m_hasThrownNadesInWave = true;
            }

            //Handle spawning of a wave if it is time
            if (___m_tickDownToNextGroupSpawn <= 0 && ___m_activeSosigs.Count + ___m_curPhase.MaxEnemies <= ___m_curPhase.MaxEnemiesAlive)
            {
                ___AttackVectors.Shuffle();

                SpawnHoldEnemyGroup(___m_curPhase, ___m_phaseIndex, ___AttackVectors, ___SpawnPoints_Turrets, ___m_activeSosigs, ___M, ref ___m_isFirstWave);
                ___m_hasThrownNadesInWave = false;
                ___m_tickDownToNextGroupSpawn = ___m_curPhase.SpawnCadence;
            }


            return false;
        }




        /////////////////////////////
        //PATCHES FOR SPAWNING SOSIGS
        /////////////////////////////


        public static Sosig SpawnEnemy(SosigTemplate template, CustomCharacter character, Transform spawnLocation, TNHModifier_AIDifficulty difficulty, int IFF, bool isAssault, Vector3 pointOfInterest, bool allowAllWeapons)
        {
            if (character.ForceAllAgentWeapons) allowAllWeapons = true;

            TNHTweakerLogger.Log("TNHTWEAKER -- Spawning sosig: " + template.SosigEnemyID, TNHTweakerLogger.LogType.TNH);

            //Create the sosig object
            GameObject sosigPrefab = Instantiate(IM.OD[template.SosigPrefabs.GetRandom<string>()].GetGameObject(), spawnLocation.position, spawnLocation.rotation);
            Sosig sosigComponent = sosigPrefab.GetComponentInChildren<Sosig>();

            //Fill out the sosigs config based on the difficulty
            SosigConfig config;

            if (difficulty == TNHModifier_AIDifficulty.Arcade && template.ConfigsEasy.Count > 0) config = template.ConfigsEasy.GetRandom<SosigConfig>();
            else if(template.Configs.Count > 0) config = template.Configs.GetRandom<SosigConfig>();
            else
            {
                TNHTweakerLogger.LogError("TNHTweaker -- Sosig did not have normal difficulty config when playing on normal difficulty! Not spawning this enemy!");
                return null;
            } 

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
            if (UnityEngine.Random.value < template.DroppedLootChance && template.DroppedObjectPool != null)
            {
                SosigLinkLootWrapper component = sosigComponent.Links[2].gameObject.AddComponent<SosigLinkLootWrapper>();
                component.group = template.DroppedObjectPool;
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


                    foreach (GameObject item in ___m_sosigPlayerBody.m_curClothes)
                    {
                        Destroy(item);
                    }
                    ___m_sosigPlayerBody.m_curClothes.Clear();

                    if (outfitConfig.Chance_Headwear >= UnityEngine.Random.value)
                    {
                        EquipSosigClothing(outfitConfig.Headwear, ___m_sosigPlayerBody.m_curClothes, ___m_sosigPlayerBody.Sosig_Head, outfitConfig.ForceWearAllHead);
                    }

                    if (outfitConfig.Chance_Facewear >= UnityEngine.Random.value)
                    {
                        EquipSosigClothing(outfitConfig.Facewear, ___m_sosigPlayerBody.m_curClothes, ___m_sosigPlayerBody.Sosig_Head, outfitConfig.ForceWearAllFace);
                    }

                    if (outfitConfig.Chance_Eyewear >= UnityEngine.Random.value)
                    {
                        EquipSosigClothing(outfitConfig.Eyewear, ___m_sosigPlayerBody.m_curClothes, ___m_sosigPlayerBody.Sosig_Head, outfitConfig.ForceWearAllEye);
                    }

                    if (outfitConfig.Chance_Torsowear >= UnityEngine.Random.value)
                    {
                        EquipSosigClothing(outfitConfig.Torsowear, ___m_sosigPlayerBody.m_curClothes, ___m_sosigPlayerBody.Sosig_Torso, outfitConfig.ForceWearAllTorso);
                    }

                    if (outfitConfig.Chance_Pantswear >= UnityEngine.Random.value)
                    {
                        EquipSosigClothing(outfitConfig.Pantswear, ___m_sosigPlayerBody.m_curClothes, ___m_sosigPlayerBody.Sosig_Abdomen, outfitConfig.ForceWearAllPants);
                    }

                    if (outfitConfig.Chance_Pantswear_Lower >= UnityEngine.Random.value)
                    {
                        EquipSosigClothing(outfitConfig.Pantswear_Lower, ___m_sosigPlayerBody.m_curClothes, ___m_sosigPlayerBody.Sosig_Legs, outfitConfig.ForceWearAllPantsLower);
                    }

                    if (outfitConfig.Chance_Backpacks >= UnityEngine.Random.value)
                    {
                        EquipSosigClothing(outfitConfig.Backpacks, ___m_sosigPlayerBody.m_curClothes, ___m_sosigPlayerBody.Sosig_Torso, outfitConfig.ForceWearAllBackpacks);
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

            TNHTweakerLogger.Log("TNHTWEAKER -- Equipping sosig weapon: " + weapon.gameObject.name, TNHTweakerLogger.LogType.TNH);

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




        //////////////////////////////////////////////
        //PATCHES FOR CONSTRUCTOR AND SECONDARY PANELS
        //////////////////////////////////////////////


        /// <summary>
        /// This is a patch for using a characters global ammo blacklist in an ammo reloader
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="__result"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        [HarmonyPatch(typeof(TNH_AmmoReloader), "GetClassFromType")]
        [HarmonyPrefix]
        public static bool AmmoReloaderGetAmmo(TNH_AmmoReloader __instance, ref FireArmRoundClass __result, FireArmRoundType t)
        {
            if (!__instance.m_decidedTypes.ContainsKey(t))
            {
                List<FireArmRoundClass> list = new List<FireArmRoundClass>();
                CustomCharacter character = LoadedTemplateManager.LoadedCharactersDict[__instance.M.C];

                for (int i = 0; i < AM.SRoundDisplayDataDic[t].Classes.Length; i++)
                {
                    FVRObject objectID = AM.SRoundDisplayDataDic[t].Classes[i].ObjectID;
                    if (__instance.m_validEras.Contains(objectID.TagEra) && __instance.m_validSets.Contains(objectID.TagSet))
                    {
                        if(character.GlobalAmmoBlacklist == null || !character.GlobalAmmoBlacklist.Contains(objectID.ItemID)){
                            list.Add(AM.SRoundDisplayDataDic[t].Classes[i].Class);
                        }
                    }
                }
                if (list.Count > 0)
                {
                    __instance.m_decidedTypes.Add(t, list[UnityEngine.Random.Range(0, list.Count)]);
                }
                else
                {
                    __instance.m_decidedTypes.Add(t, AM.GetRandomValidRoundClass(t));
                }
            }

            __result = __instance.m_decidedTypes[t];
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
            
            __instance.UpdateRerollButtonState(false);

            if (!___allowEntry)
            {
                return false;
            }
            
            if(__instance.State == TNH_ObjectConstructor.ConstructorState.EntryList)
            {

                int cost = ___m_poolEntries[i].GetCost(__instance.M.EquipmentMode) + ___m_poolAddedCost[i];
                if(__instance.M.GetNumTokens() >= cost)
                {
                    __instance.SetState(TNH_ObjectConstructor.ConstructorState.Confirm, i);
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
                    __instance.SetState(TNH_ObjectConstructor.ConstructorState.EntryList, 0);
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

                            AnvilManager.Run(SpawnObjectAtConstructor(___m_poolEntries[___m_selectedEntry], __instance));
                            ___m_numTokensSelected = 0;
                            __instance.M.SubtractTokens(cost);
                            SM.PlayCoreSound(FVRPooledAudioType.UIChirp, __instance.AudEvent_Spawn, __instance.transform.position);

                            if (__instance.M.C.UsesPurchasePriceIncrement)
                            {
                                ___m_poolAddedCost[___m_selectedEntry] += 1;
                            }

                            __instance.SetState(TNH_ObjectConstructor.ConstructorState.EntryList, 0);
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


        private static IEnumerator SpawnObjectAtConstructor(EquipmentPoolDef.PoolEntry entry, TNH_ObjectConstructor constructor)
        {
            TNHTweakerLogger.Log("TNHTWEAKER -- Spawning item at constructor", TNHTweakerLogger.LogType.TNH);

            constructor.allowEntry = false;
            EquipmentPool pool = LoadedTemplateManager.EquipmentPoolDictionary[entry];
            CustomCharacter character = LoadedTemplateManager.LoadedCharactersDict[constructor.M.C];
            List<EquipmentGroup> selectedGroups = pool.GetSpawnedEquipmentGroups();
            AnvilCallback<GameObject> gameObjectCallback;

            if (pool.SpawnsInLargeCase || pool.SpawnsInSmallCase)
            {
                TNHTweakerLogger.Log("TNHTWEAKER -- Item will spawn in a container", TNHTweakerLogger.LogType.TNH);

                GameObject caseFab = constructor.M.Prefab_WeaponCaseLarge;
                if (pool.SpawnsInSmallCase) caseFab = constructor.M.Prefab_WeaponCaseSmall;

                FVRObject item = IM.OD[selectedGroups[0].GetObjects().GetRandom()];
                GameObject itemCase = constructor.M.SpawnWeaponCase(caseFab, constructor.SpawnPoint_Case.position, constructor.SpawnPoint_Case.forward, item, selectedGroups[0].NumMagsSpawned, selectedGroups[0].NumRoundsSpawned, selectedGroups[0].MinAmmoCapacity, selectedGroups[0].MaxAmmoCapacity);

                constructor.m_spawnedCase = itemCase;
                itemCase.GetComponent<TNH_WeaponCrate>().M = constructor.M;
            }

            else
            {
                TNHTweakerLogger.Log("TNHTWEAKER -- Item will spawn without a container", TNHTweakerLogger.LogType.TNH);

                int mainSpawnCount = 0;
                int requiredSpawnCount = 0;
                int ammoSpawnCount = 0;
                int objectSpawnCount = 0;

                TNHTweakerLogger.Log("TNHTWEAKER -- Pool has " + selectedGroups.Count + " groups to spawn from" ,TNHTweakerLogger.LogType.TNH);
                for (int groupIndex = 0; groupIndex < selectedGroups.Count; groupIndex++)
                {
                    EquipmentGroup group = selectedGroups[groupIndex];

                    TNHTweakerLogger.Log("TNHTWEAKER -- Group will spawn " + group.ItemsToSpawn + " items from it", TNHTweakerLogger.LogType.TNH);
                    for (int itemIndex = 0; itemIndex < group.ItemsToSpawn; itemIndex++)
                    {
                        FVRObject mainObject;
                        SavedGunSerializable vaultFile = null;

                        Transform primarySpawn = constructor.SpawnPoint_Object;
                        Transform requiredSpawn = constructor.SpawnPoint_Object;
                        Transform ammoSpawn = constructor.SpawnPoint_Mag;
                        float objectDistancing = 0.2f;

                        if (group.IsCompatibleMagazine)
                        {
                            TNHTweakerLogger.Log("TNHTWEAKER -- Item will be a compatible magazine", TNHTweakerLogger.LogType.TNH);
                            mainObject = FirearmUtils.GetMagazineForEquipped(group.MinAmmoCapacity, group.MaxAmmoCapacity);
                            if (mainObject == null)
                            {
                                TNHTweakerLogger.LogWarning("TNHTWEAKER -- Failed to spawn a compatible magazine!");
                                break;
                            }
                        }

                        else
                        {
                            string item = group.GetObjects().GetRandom();
                            TNHTweakerLogger.Log("TNHTWEAKER -- Item selected: " + item, TNHTweakerLogger.LogType.TNH);

                            if (LoadedTemplateManager.LoadedVaultFiles.ContainsKey(item))
                            {
                                TNHTweakerLogger.Log("TNHTWEAKER -- Item is a vaulted gun", TNHTweakerLogger.LogType.TNH);
                                vaultFile = LoadedTemplateManager.LoadedVaultFiles[item];
                                mainObject = vaultFile.GetGunObject();
                            }

                            else
                            {
                                TNHTweakerLogger.Log("TNHTWEAKER -- Item is a normal object", TNHTweakerLogger.LogType.TNH);
                                mainObject = IM.OD[item];
                            }
                        }

                        //Assign spawn points based on the type of item we are spawning
                        if (mainObject.Category == FVRObject.ObjectCategory.Firearm)
                        {
                            primarySpawn = constructor.SpawnPoints_GunsSize[Mathf.Clamp(mainObject.TagFirearmSize - FVRObject.OTagFirearmSize.Pocket, 0, constructor.SpawnPoints_GunsSize.Count - 1)];
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
                        else if (mainObject.Category == FVRObject.ObjectCategory.Cartridge)
                        {
                            primarySpawn = constructor.SpawnPoint_Ammo;
                            objectDistancing = 0.05f;
                            mainSpawnCount += 1;
                        }

                        //If this is a vault file, we have to spawn it through a routine. Otherwise we just instantiate it
                        if (vaultFile != null)
                        {
                            AnvilManager.Run(TNHTweakerUtils.SpawnFirearm(vaultFile, primarySpawn.position, primarySpawn.rotation));
                            TNHTweakerLogger.Log("TNHTWEAKER -- Vaulted gun spawned", TNHTweakerLogger.LogType.TNH);
                        }
                        else
                        {
                            gameObjectCallback = mainObject.GetGameObjectAsync();
                            yield return gameObjectCallback;
                            GameObject spawnedObject = Instantiate(gameObjectCallback.Result, primarySpawn.position + Vector3.up * objectDistancing * mainSpawnCount, primarySpawn.rotation);
                            TNHTweakerLogger.Log("TNHTWEAKER -- Normal item spawned", TNHTweakerLogger.LogType.TNH);
                        }

                        
                        //Spawn any required objects
                        for (int j = 0; j < mainObject.RequiredSecondaryPieces.Count; j++)
                        {
                            gameObjectCallback = mainObject.RequiredSecondaryPieces[j].GetGameObjectAsync();
                            yield return gameObjectCallback;
                            GameObject requiredItem = Instantiate(gameObjectCallback.Result, requiredSpawn.position + -requiredSpawn.right * 0.2f * requiredSpawnCount + Vector3.up * 0.2f * j, requiredSpawn.rotation);
                            requiredSpawnCount += 1;
                            TNHTweakerLogger.Log("TNHTWEAKER -- Required item spawned", TNHTweakerLogger.LogType.TNH);
                        }


                        //Handle spawning for ammo objects if the main object has any
                        if (FirearmUtils.FVRObjectHasAmmoObject(mainObject))
                        {
                            //Get lists of ammo objects for this firearm with filters and blacklists applied
                            List<FVRObject> compatibleMagazines = FirearmUtils.GetCompatibleMagazines(mainObject, group.MinAmmoCapacity, group.MaxAmmoCapacity, character.GetMagazineBlacklist()).Select(o => o.AmmoObject).ToList();
                            List<FVRObject> compatibleRounds = FirearmUtils.GetCompatibleBullets(mainObject, character.ValidAmmoEras, character.ValidAmmoSets, character.GlobalAmmoBlacklist, character.GetMagazineBlacklist()).Select(o => o.AmmoObject).ToList();
                            List<FVRObject> compatibleClips = FirearmUtils.GetCompatibleClips(mainObject, character.GetMagazineBlacklist()).Select(o => o.AmmoObject).ToList();

                            TNHTweakerLogger.Log("TNHTWEAKER -- Compatible Mag Count: " + compatibleMagazines.Count, TNHTweakerLogger.LogType.TNH);
                            TNHTweakerLogger.Log("TNHTWEAKER -- Compatible Clip Count: " + compatibleClips.Count, TNHTweakerLogger.LogType.TNH);
                            TNHTweakerLogger.Log("TNHTWEAKER -- Compatible Round Count: " + compatibleRounds.Count, TNHTweakerLogger.LogType.TNH);

                            //If we are supposed to spawn magazines and clips, perform special logic for that
                            if (group.SpawnMagAndClip && compatibleMagazines.Count > 0 && compatibleClips.Count > 0 && group.NumMagsSpawned > 0 && group.NumClipsSpawned > 0)
                            {
                                TNHTweakerLogger.Log("TNHTWEAKER -- Spawning with both magazine and clips", TNHTweakerLogger.LogType.TNH);

                                FVRObject magazineObject = compatibleMagazines.GetRandom();
                                FVRObject clipObject = compatibleClips.GetRandom();
                                ammoSpawn = constructor.SpawnPoint_Mag;

                                gameObjectCallback = magazineObject.GetGameObjectAsync();
                                yield return gameObjectCallback;
                                GameObject spawnedMag = Instantiate(gameObjectCallback.Result, ammoSpawn.position + ammoSpawn.up * 0.05f * ammoSpawnCount, ammoSpawn.rotation);
                                ammoSpawnCount += 1;

                                gameObjectCallback = clipObject.GetGameObjectAsync();
                                yield return gameObjectCallback;
                                for (int i = 0; i < group.NumClipsSpawned; i++)
                                {
                                    GameObject spawnedClip = Instantiate(gameObjectCallback.Result, ammoSpawn.position + ammoSpawn.up * 0.05f * ammoSpawnCount, ammoSpawn.rotation);
                                    ammoSpawnCount += 1;
                                }
                            }

                            //Otherwise, perform normal logic for spawning ammo objects from current group
                            else
                            {
                                FVRObject ammoObject;
                                int numSpawned = 0;

                                if (compatibleMagazines.Count > 0 && group.NumMagsSpawned > 0)
                                {
                                    ammoObject = compatibleMagazines.GetRandom();
                                    numSpawned = group.NumMagsSpawned;
                                    ammoSpawn = constructor.SpawnPoint_Mag;
                                }
                                else if(compatibleClips.Count > 0 && group.NumClipsSpawned > 0)
                                {
                                    ammoObject = compatibleClips.GetRandom();
                                    numSpawned = group.NumClipsSpawned;
                                    ammoSpawn = constructor.SpawnPoint_Mag;
                                }
                                else if(mainObject.CompatibleSpeedLoaders != null && mainObject.CompatibleSpeedLoaders.Count > 0 && group.NumClipsSpawned > 0)
                                {
                                    ammoObject = mainObject.CompatibleSpeedLoaders.GetRandom();
                                    numSpawned = group.NumClipsSpawned;
                                    ammoSpawn = constructor.SpawnPoint_Mag;
                                }
                                else
                                {
                                    ammoObject = compatibleRounds.GetRandom();
                                    numSpawned = group.NumRoundsSpawned;
                                    ammoSpawn = constructor.SpawnPoint_Ammo;
                                }

                                gameObjectCallback = ammoObject.GetGameObjectAsync();
                                yield return gameObjectCallback;

                                for (int i = 0; i < numSpawned; i++)
                                {
                                    GameObject spawned = Instantiate(gameObjectCallback.Result, ammoSpawn.position + ammoSpawn.up * 0.05f * ammoSpawnCount, ammoSpawn.rotation);
                                    ammoSpawnCount += 1;
                                }
                            }
                        }


                        //If this object equires picatinny sights, we should try to spawn one
                        if (mainObject.RequiresPicatinnySight && character.RequireSightTable != null)
                        {
                            TNHTweakerLogger.Log("TNHTWEAKER -- Spawning required sights", TNHTweakerLogger.LogType.TNH);

                            FVRObject sight = IM.OD[character.RequireSightTable.GetSpawnedEquipmentGroups().GetRandom().GetObjects().GetRandom()];
                            gameObjectCallback = sight.GetGameObjectAsync();
                            yield return gameObjectCallback;
                            GameObject spawnedSight = Instantiate(gameObjectCallback.Result, constructor.SpawnPoint_Object.position + -constructor.SpawnPoint_Object.right * 0.15f * objectSpawnCount, constructor.SpawnPoint_Object.rotation);

                            TNHTweakerLogger.Log("TNHTWEAKER -- Required sight spawned", TNHTweakerLogger.LogType.TNH);

                            for (int j = 0; j < sight.RequiredSecondaryPieces.Count; j++)
                            {
                                gameObjectCallback = sight.RequiredSecondaryPieces[j].GetGameObjectAsync();
                                yield return gameObjectCallback;
                                GameObject spawnedRequired = Instantiate(gameObjectCallback.Result, constructor.SpawnPoint_Object.position + -constructor.SpawnPoint_Object.right * 0.15f * objectSpawnCount + Vector3.up * 0.15f * j, constructor.SpawnPoint_Object.rotation);
                                TNHTweakerLogger.Log("TNHTWEAKER -- Required item for sight spawned", TNHTweakerLogger.LogType.TNH);
                            }

                            objectSpawnCount += 1;
                        }

                        //If this object has bespoke attachments we'll try to spawn one
                        else if (mainObject.BespokeAttachments.Count > 0 && UnityEngine.Random.value < group.BespokeAttachmentChance)
                        {
                            FVRObject bespoke = mainObject.BespokeAttachments.GetRandom();
                            gameObjectCallback = bespoke.GetGameObjectAsync();
                            yield return gameObjectCallback;
                            GameObject bespokeObject = Instantiate(gameObjectCallback.Result, constructor.SpawnPoint_Object.position + -constructor.SpawnPoint_Object.right * 0.15f * objectSpawnCount, constructor.SpawnPoint_Object.rotation);
                            objectSpawnCount += 1;

                            TNHTweakerLogger.Log("TNHTWEAKER -- Bespoke item spawned", TNHTweakerLogger.LogType.TNH);
                        }
                    }
                }
            }

            constructor.allowEntry = true;
            yield break;
        }



        //////////////////////////
        //MISC PATCHES AND METHODS
        //////////////////////////


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
            //Debug.Log("Destroying constructors");
            while (SpawnedConstructors.Count > 0)
            {
                try
                {
                    TNH_ObjectConstructor constructor = SpawnedConstructors[0].GetComponent<TNH_ObjectConstructor>();

                    if (constructor != null)
                    {
                        constructor.ClearCase();
                    }

                    Destroy(SpawnedConstructors[0]);
                }
                catch
                {
                    TNHTweakerLogger.LogWarning("TNHTWEAKER -- Failed to destroy constructor! It's likely that the constructor is already destroyed, so everything is probably just fine :)");
                }

                SpawnedConstructors.RemoveAt(0);
            }

            //Debug.Log("Destroying panels");
            while (SpawnedPanels.Count > 0)
            {
                Destroy(SpawnedPanels[0]);
                SpawnedPanels.RemoveAt(0);
            }
        }

        [HarmonyPatch(typeof(Sosig), "BuffHealing_Invis")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool OverrideCloaking()
        {
            return !preventOutfitFunctionality;
        }


    }
}
