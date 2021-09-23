using FistVR;
using HarmonyLib;
using MagazinePatcher;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TNHTweaker.ObjectTemplates;
using TNHTweaker.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace TNHTweaker.Patches
{
    public class TNHPatches
    {

        private static bool preventOutfitFunctionality = false;

        private static List<int> spawnedBossIndexes = new List<int>();
        private static List<int> supplyPointIFFList = new List<int>();
        private static List<GameObject> SpawnedConstructors = new List<GameObject>();
        private static List<GameObject> SpawnedPanels = new List<GameObject>();
        private static List<EquipmentPoolDef.PoolEntry> SpawnedPools = new List<EquipmentPoolDef.PoolEntry>();

        #region Initializing TNH

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
            Text itemsText = CreateItemsText(__instance);
            ExpandCharacterUI(__instance);

            //Perform first time setup of all files
            if (!TNHMenuInitializer.TNHInitialized)
            {
                SceneLoader sceneHotDog = UnityEngine.Object.FindObjectOfType<SceneLoader>();

                if (!TNHMenuInitializer.MagazineCacheFailed)
                {
                    AnvilManager.Run(TNHMenuInitializer.InitializeTNHMenuAsync(TNHTweaker.OutputFilePath, magazineCacheText, itemsText, sceneHotDog, ___Categories, ___CharDatabase, __instance, TNHTweaker.BuildCharacterFiles.Value));
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
            Text magazineCacheText = UnityEngine.Object.Instantiate(manager.SelectedCharacter_Title.gameObject, manager.SelectedCharacter_Title.transform.parent).GetComponent<Text>();
            magazineCacheText.transform.localPosition = new Vector3(0, 550, 0);
            magazineCacheText.transform.localScale = new Vector3(2, 2, 2);
            magazineCacheText.horizontalOverflow = HorizontalWrapMode.Overflow;
            magazineCacheText.text = "EXAMPLE TEXT";

            return magazineCacheText;
        }

        private static Text CreateItemsText(TNH_UIManager manager)
        {
            Text itemsText = UnityEngine.Object.Instantiate(manager.SelectedCharacter_Title.gameObject, manager.SelectedCharacter_Title.transform.parent).GetComponent<Text>();
            itemsText.transform.localPosition = new Vector3(-30, 630, 0);
            itemsText.transform.localScale = new Vector3(1, 1, 1);
            itemsText.text = "";
            itemsText.supportRichText = true;
            itemsText.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            itemsText.alignment = TextAnchor.LowerLeft;
            itemsText.verticalOverflow = VerticalWrapMode.Overflow;
            itemsText.horizontalOverflow = HorizontalWrapMode.Overflow;

            return itemsText;
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
                Text newCharacterLabel = UnityEngine.Object.Instantiate(manager.LBL_CharacterName[1].gameObject, manager.LBL_CharacterName[1].transform.parent).GetComponent<Text>();
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

        #endregion


        #region Patrol Spawning

        /////////////////////////////
        //PATCHES FOR PATROL SPAWNING
        /////////////////////////////


        /// <summary>
        /// Finds an index in the patrols list which can spawn, preventing bosses that have already spawned from spawning again
        /// </summary>
        /// <param name="patrols">List of patrols that can spawn</param>
        /// <returns>Returns -1 if no valid index is found, otherwise returns a random index for a patrol </returns>
        private static int GetValidPatrolIndex(List<Patrol> patrols)
        {
            int index = UnityEngine.Random.Range(0, patrols.Count);
            int attempts = 0;

            while (spawnedBossIndexes.Contains(index) && attempts < patrols.Count)
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
        [HarmonyPatch(typeof(TNH_Manager), "GenerateValidPatrol")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool GenerateValidPatrolReplacement(TNH_PatrolChallenge P, int curStandardIndex, int excludeHoldIndex, bool isStart, TNH_Manager __instance, TNH_Progression.Level ___m_curLevel, List<TNH_Manager.SosigPatrolSquad> ___m_patrolSquads, ref float ___m_timeTilPatrolCanSpawn)
        {
            TNHTweakerLogger.Log("TNHTWEAKER -- Generating a patrol -- There are currently " + ___m_patrolSquads.Count + " patrols active", TNHTweakerLogger.LogType.TNH);

            if (P.Patrols.Count < 1) return false;

            CustomCharacter character = LoadedTemplateManager.LoadedCharactersDict[__instance.C];
            Level currLevel = character.GetCurrentLevel(__instance.m_curLevel);

            //Get a valid patrol index, and exit if there are no valid patrols
            int patrolIndex = GetValidPatrolIndex(currLevel.Patrols);
            if (patrolIndex == -1)
            {
                TNHTweakerLogger.Log("TNHTWEAKER -- No valid patrols can spawn", TNHTweakerLogger.LogType.TNH);
                ___m_timeTilPatrolCanSpawn = 999;
                return false;
            }

            TNHTweakerLogger.Log("TNHTWEAKER -- Valid patrol found", TNHTweakerLogger.LogType.TNH);

            Patrol patrol = currLevel.Patrols[patrolIndex];

            List<int> validLocations = new List<int>();
            float minDist = __instance.TAHReticle.Range * 1.2f;

            //Get a safe starting point for the patrol to spawn
            TNH_SafePositionMatrix.PositionEntry startingEntry;
            if (isStart) startingEntry = __instance.SafePosMatrix.Entries_SupplyPoints[curStandardIndex];
            else startingEntry = __instance.SafePosMatrix.Entries_HoldPoints[curStandardIndex];


            for (int i = 0; i < startingEntry.SafePositions_HoldPoints.Count; i++)
            {
                if (i != excludeHoldIndex && startingEntry.SafePositions_HoldPoints[i])
                {
                    float playerDist = Vector3.Distance(GM.CurrentPlayerBody.transform.position, __instance.HoldPoints[i].transform.position);
                    if (playerDist > minDist)
                    {
                        validLocations.Add(i);
                    }
                }
            }


            if (validLocations.Count < 1) return false;
            validLocations.Shuffle();

            TNH_Manager.SosigPatrolSquad squad = GeneratePatrol(validLocations[0], __instance, patrol, patrolIndex);
            ___m_patrolSquads.Add(squad);

            if (__instance.EquipmentMode == TNHSetting_EquipmentMode.Spawnlocking)
            {
                ___m_timeTilPatrolCanSpawn = patrol.PatrolCadence;
            }
            else
            {
                ___m_timeTilPatrolCanSpawn = patrol.PatrolCadenceLimited;
            }

            return false;
        }


        [HarmonyPatch(typeof(TNH_Manager), "GenerateInitialTakeSentryPatrols")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool GenerateInitialTakeSentryPatrolPatch()
        {


            return false;
        }



        [HarmonyPatch(typeof(TNH_Manager), "GenerateSentryPatrol")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool GenerateSentryPatrolPatch(
            List<Vector3> SpawnPoints, 
            List<Vector3> ForwardVectors, 
            List<Vector3> PatrolPoints, 
            TNH_Manager __instance, 
            List<TNH_Manager.SosigPatrolSquad> ___m_patrolSquads, 
            ref float ___m_timeTilPatrolCanSpawn,
            ref TNH_Manager.SosigPatrolSquad __result
            )
        {
            TNHTweakerLogger.Log("TNHTWEAKER -- Generating a sentry patrol -- There are currently " + ___m_patrolSquads.Count + " patrols active", TNHTweakerLogger.LogType.TNH);

            CustomCharacter character = LoadedTemplateManager.LoadedCharactersDict[__instance.C];
            Level currLevel = character.GetCurrentLevel(__instance.m_curLevel);

            if (currLevel.Patrols.Count < 1) return false;

            //Get a valid patrol index, and exit if there are no valid patrols
            int patrolIndex = GetValidPatrolIndex(currLevel.Patrols);
            if (patrolIndex == -1)
            {
                TNHTweakerLogger.Log("TNHTWEAKER -- No valid patrols can spawn", TNHTweakerLogger.LogType.TNH);
                ___m_timeTilPatrolCanSpawn = 999;

                //Returning an empty squad is the easiest way to not generate a patrol when no valid ones are found
                //This could cause strange and unpredictable behaviour
                //Good luck!
                __result = new TNH_Manager.SosigPatrolSquad();
                __result.PatrolPoints = new List<Vector3>();
                __result.Squad = new List<Sosig>();

                return false;
            }

            TNHTweakerLogger.Log("TNHTWEAKER -- Valid patrol found", TNHTweakerLogger.LogType.TNH);

            Patrol patrol = currLevel.Patrols[patrolIndex];
            TNH_Manager.SosigPatrolSquad squad = GeneratePatrol(__instance, patrol, SpawnPoints, ForwardVectors, PatrolPoints, patrolIndex);

            //We don't add this patrol because it's tracked outside of this method
            //___m_patrolSquads.Add(squad);

            if (__instance.EquipmentMode == TNHSetting_EquipmentMode.Spawnlocking)
            {
                ___m_timeTilPatrolCanSpawn = patrol.PatrolCadence;
            }
            else
            {
                ___m_timeTilPatrolCanSpawn = patrol.PatrolCadenceLimited;
            }

            __result = squad;
            return false;
        }


        public static TNH_Manager.SosigPatrolSquad GeneratePatrol(TNH_Manager instance, Patrol patrol, List<Vector3> SpawnPoints, List<Vector3> ForwardVectors, List<Vector3> PatrolPoints, int patrolIndex)
        {
            TNH_Manager.SosigPatrolSquad squad = new TNH_Manager.SosigPatrolSquad();
            squad.PatrolPoints = new List<Vector3>(PatrolPoints);

            for (int i = 0; i < patrol.PatrolSize && i < SpawnPoints.Count; i++)
            {
                SosigEnemyTemplate template;

                bool allowAllWeapons;

                //If this is a boss, then we can only spawn it once, so add it to the list of spawned bosses
                if (patrol.IsBoss)
                {
                    spawnedBossIndexes.Add(patrolIndex);
                }

                //Select a sosig template from the custom character patrol
                if (i == 0)
                {
                    template = IM.Instance.odicSosigObjsByID[(SosigEnemyID)LoadedTemplateManager.SosigIDDict[patrol.LeaderType]];
                    allowAllWeapons = true;
                }

                else
                {
                    template = IM.Instance.odicSosigObjsByID[(SosigEnemyID)LoadedTemplateManager.SosigIDDict[patrol.EnemyType.GetRandom()]];
                    allowAllWeapons = false;
                }

                CustomCharacter character = LoadedTemplateManager.LoadedCharactersDict[instance.C];
                SosigTemplate customTemplate = LoadedTemplateManager.LoadedSosigsDict[template];
                FVRObject droppedObject = instance.Prefab_HealthPickupMinor;

                //If squad is set to swarm, the first point they path to should be the players current position
                Sosig sosig;
                if (patrol.SwarmPlayer)
                {
                    squad.PatrolPoints[0] = GM.CurrentPlayerBody.transform.position;
                    sosig = SpawnEnemy(customTemplate, character, SpawnPoints[i], Quaternion.LookRotation(ForwardVectors[i], Vector3.up), instance.AI_Difficulty, patrol.IFFUsed, true, squad.PatrolPoints[0], allowAllWeapons);
                    sosig.SetAssaultSpeed(patrol.AssualtSpeed);
                }
                else
                {
                    sosig = SpawnEnemy(customTemplate, character, SpawnPoints[i], Quaternion.LookRotation(ForwardVectors[i], Vector3.up), instance.AI_Difficulty, patrol.IFFUsed, true, squad.PatrolPoints[0], allowAllWeapons);
                    sosig.SetAssaultSpeed(patrol.AssualtSpeed);
                }

                //Handle patrols dropping health
                if (i == 0 && UnityEngine.Random.value < patrol.DropChance)
                {
                    sosig.Links[1].RegisterSpawnOnDestroy(droppedObject);
                }

                squad.Squad.Add(sosig);
            }

            return squad;
        }




        /// <summary>
        /// Spawns a patrol at the desire patrol point
        /// </summary>
        public static TNH_Manager.SosigPatrolSquad GeneratePatrol(int HoldPointStart, TNH_Manager instance, Patrol patrol, int patrolIndex)
        {
            List<Vector3> SpawnPoints = new List<Vector3>();
            List<Vector3> PatrolPoints = new List<Vector3>();
            List<Vector3> ForwardVectors = new List<Vector3>();

            foreach (TNH_HoldPoint holdPoint in instance.HoldPoints)
            {
                PatrolPoints.Add(holdPoint.SpawnPoints_Sosigs_Defense.GetRandom<Transform>().position);
            }

            foreach(Transform spawnPoint in instance.HoldPoints[HoldPointStart].SpawnPoints_Sosigs_Defense)
            {
                SpawnPoints.Add(spawnPoint.position);
                ForwardVectors.Add(spawnPoint.forward);
            }

            //Insert spawn point as first patrol point
            Vector3 startingPoint = PatrolPoints[HoldPointStart];
            PatrolPoints.RemoveAt(HoldPointStart);
            PatrolPoints.Insert(0, startingPoint);

            return GeneratePatrol(instance, patrol, SpawnPoints, ForwardVectors, PatrolPoints, patrolIndex);
        }


        #endregion


        #region Supply and Take Points

        ///////////////////////////////////////////
        //PATCHES FOR SUPPLY POINTS AND TAKE POINTS
        ///////////////////////////////////////////


        [HarmonyPatch(typeof(TNH_SupplyPoint), "ConfigureAtBeginning")]
        [HarmonyPrefix]
        public static bool SpawnStartingEquipment(TNH_SupplyPoint __instance)
        {
            __instance.m_trackedObjects.Clear();
            if (__instance.M.ItemSpawnerMode == TNH_ItemSpawnerMode.On)
            {
                __instance.M.ItemSpawner.transform.position = __instance.SpawnPoints_Panels[0].position + Vector3.up * 0.8f;
                __instance.M.ItemSpawner.transform.rotation = __instance.SpawnPoints_Panels[0].rotation;
                __instance.M.ItemSpawner.SetActive(true);
            }

            for (int i = 0; i < __instance.SpawnPoint_Tables.Count; i++)
            {
                GameObject item = UnityEngine.Object.Instantiate(__instance.M.Prefab_MetalTable, __instance.SpawnPoint_Tables[i].position, __instance.SpawnPoint_Tables[i].rotation);
                __instance.m_trackedObjects.Add(item);
            }

            CustomCharacter character = LoadedTemplateManager.LoadedCharactersDict[__instance.M.C];
            if (character.HasPrimaryWeapon)
            {
                EquipmentGroup selectedGroup = character.PrimaryWeapon.PrimaryGroup;
                if (selectedGroup == null) selectedGroup = character.PrimaryWeapon.BackupGroup;

                if (selectedGroup != null)
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
                    GameObject item = UnityEngine.Object.Instantiate(selectedItem.GetGameObject(), __instance.SpawnPoint_Melee.position, __instance.SpawnPoint_Melee.rotation);
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
                    
                    List<GameObject> toSpawn = new List<GameObject>();
                    for (int i = 0; i < selectedGroup.ItemsToSpawn; i++)
                    {
                        FVRObject selectedItem = IM.OD[selectedGroup.GetObjects().GetRandom()];
                        var item = selectedItem.GetGameObject();
                        toSpawn.Add(item);
                        __instance.M.AddObjectToTrackedList(item);
                    }
                    
                    AnvilManager.Run(TNHTweakerUtils.InstantiateList(toSpawn, __instance.SpawnPoints_SmallItem[0].position));
                }
            }

            if (character.HasSecondaryItem)
            {
                EquipmentGroup selectedGroup = character.SecondaryItem.PrimaryGroup;
                if (selectedGroup == null) selectedGroup = character.SecondaryItem.BackupGroup;

                if (selectedGroup != null)
                {
                    selectedGroup = selectedGroup.GetSpawnedEquipmentGroups().GetRandom();
                    
                    List<GameObject> toSpawn = new List<GameObject>();
                    for (int i = 0; i < selectedGroup.ItemsToSpawn; i++)
                    {
                        FVRObject selectedItem = IM.OD[selectedGroup.GetObjects().GetRandom()];
                        var item = selectedItem.GetGameObject();
                        toSpawn.Add(item);
                        __instance.M.AddObjectToTrackedList(item);
                    }
                    
                    AnvilManager.Run(TNHTweakerUtils.InstantiateList(toSpawn, __instance.SpawnPoints_SmallItem[1].position));
                }
            }

            if (character.HasTertiaryItem)
            {
                EquipmentGroup selectedGroup = character.TertiaryItem.PrimaryGroup;
                if (selectedGroup == null) selectedGroup = character.TertiaryItem.BackupGroup;

                if (selectedGroup != null)
                {
                    selectedGroup = selectedGroup.GetSpawnedEquipmentGroups().GetRandom();
                    
                    List<GameObject> toSpawn = new List<GameObject>();
                    for (int i = 0; i < selectedGroup.ItemsToSpawn; i++)
                    {
                        FVRObject selectedItem = IM.OD[selectedGroup.GetObjects().GetRandom()];
                        var item = selectedItem.GetGameObject();
                        toSpawn.Add(item);
                        __instance.M.AddObjectToTrackedList(item);
                    }
                    
                    AnvilManager.Run(TNHTweakerUtils.InstantiateList(toSpawn, __instance.SpawnPoints_SmallItem[2].position));
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
                    GameObject item = UnityEngine.Object.Instantiate(selectedItem.GetGameObject(), __instance.SpawnPoint_Shield.position, __instance.SpawnPoint_Shield.rotation);
                    __instance.M.AddObjectToTrackedList(item);
                }
            }

            if (TNHTweaker.UnlimitedTokens.Value) __instance.M.AddTokens(999999, false);

            return false;
        }




        [HarmonyPatch(typeof(TNH_Manager), "SetPhase_Take")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool SetPhase_Take_Replacement(
            TNH_Manager __instance,
            int ___m_level,
            TNH_Progression.Level ___m_curLevel,
            TNH_PointSequence ___m_curPointSequence)
        {

            CustomCharacter character = LoadedTemplateManager.LoadedCharactersDict[__instance.C];
            Level level = character.GetCurrentLevel(___m_curLevel);

            spawnedBossIndexes.Clear();
            __instance.m_activeSupplyPointIndicies.Clear();
            preventOutfitFunctionality = LoadedTemplateManager.LoadedCharactersDict[__instance.C].ForceDisableOutfitFunctionality;


            //Clear the TNH radar
            if (__instance.RadarMode == TNHModifier_RadarMode.Standard)
            {
                __instance.TAHReticle.GetComponent<AIEntity>().LM_VisualOcclusionCheck = __instance.ReticleMask_Take;
            }
            else if (__instance.RadarMode == TNHModifier_RadarMode.Omnipresent)
            {
                __instance.TAHReticle.GetComponent<AIEntity>().LM_VisualOcclusionCheck = __instance.ReticleMask_Hold;
            }
            __instance.TAHReticle.DeRegisterTrackedType(TAH_ReticleContact.ContactType.Hold);
            __instance.TAHReticle.DeRegisterTrackedType(TAH_ReticleContact.ContactType.Supply);


            //Get the next hold point and configure it
            __instance.m_lastHoldIndex = __instance.m_curHoldIndex;
            __instance.m_curHoldIndex = GetNextHoldPointIndex(__instance, ___m_curPointSequence, ___m_level, __instance.m_curHoldIndex);
            __instance.m_curHoldPoint = __instance.HoldPoints[__instance.m_curHoldIndex];
            __instance.m_curHoldPoint.ConfigureAsSystemNode(___m_curLevel.TakeChallenge, ___m_curLevel.HoldChallenge, ___m_curLevel.NumOverrideTokensForHold);
            __instance.TAHReticle.RegisterTrackedObject(__instance.m_curHoldPoint.SpawnPoint_SystemNode, TAH_ReticleContact.ContactType.Hold);


            //Generate all of the supply points for this level
            List<int> supplyPointsIndexes = GetNextSupplyPointIndexes(__instance, ___m_curPointSequence, ___m_level, __instance.m_curHoldIndex);
            int numSupplyPoints = UnityEngine.Random.Range(level.MinSupplyPoints, level.MaxSupplyPoints + 1);
            numSupplyPoints = Mathf.Clamp(numSupplyPoints, 0, supplyPointsIndexes.Count);


            //Shuffle panel types
            level.PossiblePanelTypes.Shuffle();
            TNHTweakerLogger.Log("TNHTWEAKER -- Panel types for this hold:", TNHTweakerLogger.LogType.TNH);
            level.PossiblePanelTypes.ForEach(o => TNHTweakerLogger.Log(o.ToString(), TNHTweakerLogger.LogType.TNH));

            //Now spawn and setup all of the supply points
            //TODO this is one of the main code blocks for this method that requires it to be a full copy of the original method. This would be better fit as a transpiler patch
            TNHTweakerLogger.Log("TNHTWEAKER -- Spawning " + numSupplyPoints + " supply points", TNHTweakerLogger.LogType.TNH);
            for (int i = 0; i < numSupplyPoints; i++)
            {
                TNH_SupplyPoint supplyPoint = __instance.SupplyPoints[supplyPointsIndexes[i]];
                ConfigureSupplyPoint(supplyPoint, level, i);
                TAH_ReticleContact contact = __instance.TAHReticle.RegisterTrackedObject(supplyPoint.SpawnPoint_PlayerSpawn, TAH_ReticleContact.ContactType.Supply);
                supplyPoint.SetContact(contact);
                __instance.m_activeSupplyPointIndicies.Add(supplyPointsIndexes[i]);
            }


            //Go through and spawn the initial patrol
            if (__instance.UsesClassicPatrolBehavior)
            {
                if (__instance.m_level == 0)
                {
                    __instance.GenerateValidPatrol(__instance.m_curLevel.PatrolChallenge, __instance.m_curPointSequence.StartSupplyPointIndex, __instance.m_curHoldIndex, true);
                }
                else
                {
                    __instance.GenerateValidPatrol(__instance.m_curLevel.PatrolChallenge, __instance.m_curHoldIndex, __instance.m_curHoldIndex, false);
                }
            }
            else if (__instance.m_level == 0)
            {
                __instance.GenerateInitialTakeSentryPatrols(__instance.m_curLevel.PatrolChallenge, __instance.m_curPointSequence.StartSupplyPointIndex, -1, __instance.m_curHoldIndex, true);
            }
            else
            {
                __instance.GenerateInitialTakeSentryPatrols(__instance.m_curLevel.PatrolChallenge, -1, __instance.m_curHoldIndex, __instance.m_curHoldIndex, false);
            }



            if (__instance.BGAudioMode == TNH_BGAudioMode.Default)
            {
                __instance.FMODController.SwitchTo(0, 2f, false, false);
            }

            return false;
        }

        public static void ConfigureSupplyPoint(TNH_SupplyPoint supplyPoint, Level level, int supplyIndex)
        {
            TNHTweakerLogger.Log("TNHTWEAKER -- Configuring supply point : " + supplyIndex, TNHTweakerLogger.LogType.TNH);

            supplyPoint.T = level.SupplyChallenge.GetTakeChallenge();
            supplyPoint.m_isconfigured = true;

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

            for (int i = 0; i < toSpawn && i < point.SpawnPoints_Panels.Count; i++)
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
            panelTypes.Shuffle();

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

                else if (panelType == PanelType.MagDuplicator || panelType == PanelType.MagUpgrader || panelType == PanelType.MagPurchase)
                {
                    panel = point.M.SpawnMagDuplicator(point.SpawnPoints_Panels[i]);
                    panel.AddComponent(typeof(MagazinePanel));
                }

                else if (panelType == PanelType.Recycler)
                {
                    panel = point.M.SpawnGunRecycler(point.SpawnPoints_Panels[i]);
                }

                else if (panelType == PanelType.AmmoPurchase)
                {
                    panel = point.M.SpawnMagDuplicator(point.SpawnPoints_Panels[i]);
                    panel.AddComponent(typeof(AmmoPurchasePanel));
                }

                else if (panelType == PanelType.AddFullAuto)
                {
                    panel = point.M.SpawnMagDuplicator(point.SpawnPoints_Panels[i]);
                    panel.AddComponent(typeof(FullAutoPanel));
                }

                else if (panelType == PanelType.FireRateUp || panelType == PanelType.FireRateDown)
                {
                    panel = point.M.SpawnMagDuplicator(point.SpawnPoints_Panels[i]);
                    panel.AddComponent(typeof(FireRatePanel));
                }

                else
                {
                    panel = point.M.SpawnAmmoReloader(point.SpawnPoints_Panels[i]);
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
                AutoMeater turret = UnityEngine.Object.Instantiate<GameObject>(turretPrefab.GetGameObject(), pos, point.SpawnPoints_Turrets[i].rotation).GetComponent<AutoMeater>();
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
                GameObject box = UnityEngine.Object.Instantiate(point.M.Prefabs_ShatterableCrates[UnityEngine.Random.Range(0, point.M.Prefabs_ShatterableCrates.Count)], position, rotation);
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

                pointIndexes.Shuffle();
                pointIndexes.Shuffle();
                index = pointIndexes[0];
            }

            return index;
        }


        public static List<int> GetNextSupplyPointIndexes(TNH_Manager M, TNH_PointSequence pointSequence, int currLevel, int currHoldIndex)
        {
            List<int> indexes = new List<int>();

            if (currLevel == 0)
            {
                for (int i = 0; i < M.SafePosMatrix.Entries_SupplyPoints[pointSequence.StartSupplyPointIndex].SafePositions_SupplyPoints.Count; i++)
                {
                    if (M.SafePosMatrix.Entries_SupplyPoints[pointSequence.StartSupplyPointIndex].SafePositions_SupplyPoints[i])
                    {
                        indexes.Add(i);
                    }
                }
            }
            else
            {
                for (int i = 0; i < M.SafePosMatrix.Entries_HoldPoints[currHoldIndex].SafePositions_SupplyPoints.Count; i++)
                {
                    if (M.SafePosMatrix.Entries_HoldPoints[currHoldIndex].SafePositions_SupplyPoints[i])
                    {
                        indexes.Add(i);
                    }
                }
            }

            indexes.Shuffle();
            indexes.Shuffle();

            return indexes;
        }


        [HarmonyPatch(typeof(TNH_HoldPoint), "SpawnTakeEnemyGroup")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool SpawnTakeGroupReplacement(List<Transform> ___SpawnPoints_Sosigs_Defense, TNH_TakeChallenge ___T, TNH_Manager ___M, List<Sosig> ___m_activeSosigs)
        {
            ___SpawnPoints_Sosigs_Defense.Shuffle();
            ___SpawnPoints_Sosigs_Defense.Shuffle();

            for (int i = 0; i < ___T.NumGuards && i < ___SpawnPoints_Sosigs_Defense.Count; i++)
            {
                Transform transform = ___SpawnPoints_Sosigs_Defense[i];
                //Debug.Log("Take challenge sosig ID : " + ___T.GID);
                SosigEnemyTemplate template = IM.Instance.odicSosigObjsByID[___T.GID];
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
                AutoMeater turret = UnityEngine.Object.Instantiate<GameObject>(turretPrefab.GetGameObject(), pos, ___SpawnPoints_Turrets[i].rotation).GetComponent<AutoMeater>();
                ___m_activeTurrets.Add(turret);
            }

            return false;
        }


        #endregion


        #region During Hold Point

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


            if (__instance.M.TargetMode == TNHSetting_TargetMode.Simple)
            {
                __instance.M.EnqueueEncryptionLine(TNH_EncryptionType.Static);
                __instance.DeleteAllActiveWarpIns();
                SpawnEncryptionReplacement(__instance, currentPhase, true);
            }
            else
            {
                __instance.M.EnqueueEncryptionLine(currentPhase.Encryptions[0]);
                __instance.DeleteAllActiveWarpIns();
                SpawnEncryptionReplacement(__instance, currentPhase, false);
            }

            __instance.m_systemNode.SetNodeMode(TNH_HoldPointSystemNode.SystemNodeMode.Indentified);

            return false;
        }


        public static void SpawnEncryptionReplacement(TNH_HoldPoint holdPoint, Phase currentPhase, bool isSimple)
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

            List<FVRObject> encryptions;
            if (isSimple)
            {
                encryptions = new List<FVRObject>();
                encryptions.Add(holdPoint.M.GetEncryptionPrefab(TNH_EncryptionType.Static));
            }
            else
            {
                encryptions = currentPhase.Encryptions.Select(o => holdPoint.M.GetEncryptionPrefab(o)).ToList();
            }


            for (int i = 0; i < numTargets && i < holdPoint.m_validSpawnPoints.Count; i++)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate(encryptions[i % encryptions.Count].GetGameObject(), holdPoint.m_validSpawnPoints[i].position, holdPoint.m_validSpawnPoints[i].rotation);
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
                GameObject grenadeObject = UnityEngine.Object.Instantiate(IM.OD[grenadeType].GetGameObject(), randAttackVector.GrenadeVector.position, randAttackVector.GrenadeVector.rotation);

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


        #endregion


        #region Sosig Spawning

        /////////////////////////////
        //PATCHES FOR SPAWNING SOSIGS
        /////////////////////////////

        [HarmonyPatch(typeof(Sosig), "ClearSosig")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static void ClearSosig(Sosig __instance)
        {
            SosigLinkLootWrapper lootWrapper = __instance.GetComponentInChildren<SosigLinkLootWrapper>();
            if (lootWrapper != null)
            {
                lootWrapper.dontDrop = !lootWrapper.shouldDropOnCleanup;
            }
        }
        
        public static Sosig SpawnEnemy(SosigTemplate template, CustomCharacter character, Transform spawnLocation, TNHModifier_AIDifficulty difficulty, int IFF, bool isAssault, Vector3 pointOfInterest, bool allowAllWeapons)
        {
            return SpawnEnemy(template, character, spawnLocation.position, spawnLocation.rotation, difficulty, IFF, isAssault, pointOfInterest, allowAllWeapons);
        }

        public static Sosig SpawnEnemy(SosigTemplate template, CustomCharacter character, Vector3 spawnLocation, Quaternion spawnRotation, TNHModifier_AIDifficulty difficulty, int IFF, bool isAssault, Vector3 pointOfInterest, bool allowAllWeapons)
        {
            if (character.ForceAllAgentWeapons) allowAllWeapons = true;

            TNHTweakerLogger.Log("TNHTWEAKER -- Spawning sosig: " + template.SosigEnemyID, TNHTweakerLogger.LogType.TNH);

            //Create the sosig object
            GameObject sosigPrefab = UnityEngine.Object.Instantiate(IM.OD[template.SosigPrefabs.GetRandom<string>()].GetGameObject(), spawnLocation, spawnRotation);
            Sosig sosigComponent = sosigPrefab.GetComponentInChildren<Sosig>();

            //Fill out the sosigs config based on the difficulty
            SosigConfig config;

            if (difficulty == TNHModifier_AIDifficulty.Arcade && template.ConfigsEasy.Count > 0) config = template.ConfigsEasy.GetRandom<SosigConfig>();
            else if (template.Configs.Count > 0) config = template.Configs.GetRandom<SosigConfig>();
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
            if (template.WeaponOptions.Count > 0)
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
            if (outfitConfig.Chance_Headwear >= UnityEngine.Random.value)
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
                component.shouldDropOnCleanup = !character.DisableCleanupSosigDrops;
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
            if (tem.SosigEnemyID != SosigEnemyID.None)
            {
                if (tem.OutfitConfig.Count > 0 && LoadedTemplateManager.LoadedSosigsDict.ContainsKey(tem))
                {
                    OutfitConfig outfitConfig = LoadedTemplateManager.LoadedSosigsDict[tem].OutfitConfigs.GetRandom();


                    foreach (GameObject item in ___m_sosigPlayerBody.m_curClothes)
                    {
                        UnityEngine.Object.Destroy(item);
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
            SosigWeapon weapon = UnityEngine.Object.Instantiate(weaponPrefab, sosig.transform.position + Vector3.up * 0.1f, sosig.transform.rotation).GetComponent<SosigWeapon>();
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
                foreach (string clothing in options)
                {
                    GameObject clothingObject = UnityEngine.Object.Instantiate(IM.OD[clothing].GetGameObject(), link.transform.position, link.transform.rotation);
                    clothingObject.transform.SetParent(link.transform);
                    clothingObject.GetComponent<SosigWearable>().RegisterWearable(link);
                }
            }

            else
            {
                GameObject clothingObject = UnityEngine.Object.Instantiate(IM.OD[options.GetRandom<string>()].GetGameObject(), link.transform.position, link.transform.rotation);
                clothingObject.transform.SetParent(link.transform);
                clothingObject.GetComponent<SosigWearable>().RegisterWearable(link);
            }
        }


        public static void EquipSosigClothing(List<string> options, List<GameObject> playerClothing, Transform link, bool wearAll)
        {
            if (wearAll)
            {
                foreach (string clothing in options)
                {
                    GameObject clothingObject = UnityEngine.Object.Instantiate(IM.OD[clothing].GetGameObject(), link.position, link.rotation);

                    Component[] children = clothingObject.GetComponentsInChildren<Component>(true);
                    foreach (Component child in children)
                    {
                        child.gameObject.layer = LayerMask.NameToLayer("ExternalCamOnly");

                        if (!(child is Transform) && !(child is MeshFilter) && !(child is MeshRenderer))
                        {
                            UnityEngine.Object.Destroy(child);
                        }
                    }

                    playerClothing.Add(clothingObject);
                    clothingObject.transform.SetParent(link);
                }
            }

            else
            {
                GameObject clothingObject = UnityEngine.Object.Instantiate(IM.OD[options.GetRandom<string>()].GetGameObject(), link.position, link.rotation);

                Component[] children = clothingObject.GetComponentsInChildren<Component>(true);
                foreach (Component child in children)
                {
                    child.gameObject.layer = LayerMask.NameToLayer("ExternalCamOnly");

                    if (!(child is Transform) && !(child is MeshFilter) && !(child is MeshRenderer))
                    {
                        UnityEngine.Object.Destroy(child);
                    }
                }

                playerClothing.Add(clothingObject);
                clothingObject.transform.SetParent(link);
            }
        }


        #endregion


        #region Constructor and Secondary Panels

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
                        if (character.GlobalAmmoBlacklist == null || !character.GlobalAmmoBlacklist.Contains(objectID.ItemID))
                        {
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


        [HarmonyPatch(typeof(TNH_ObjectConstructor), "GetPoolEntry")]
        [HarmonyPrefix]
        public static bool GetPoolEntryPatch(ref EquipmentPoolDef.PoolEntry __result, int level, EquipmentPoolDef poolDef, EquipmentPoolDef.PoolEntry.PoolEntryType t, EquipmentPoolDef.PoolEntry prior)
        {

            //Collect all pools that could spawn based on level and type, and sum up their rarities
            List<EquipmentPoolDef.PoolEntry> validPools = new List<EquipmentPoolDef.PoolEntry>();
            float summedRarity = 0;
            foreach(EquipmentPoolDef.PoolEntry entry in poolDef.Entries)
            {
                if(entry.Type == t && entry.MinLevelAppears <= level && entry.MaxLevelAppears >= level)
                {
                    validPools.Add(entry);
                    summedRarity += entry.Rarity;
                }
            }

            //If we didn't find a single pool, we cry about it
            if(validPools.Count == 0)
            {
                TNHTweakerLogger.LogWarning("TNHTWEAKER -- No valid pool could spawn at constructor for type (" + t + ")");
                __result = null;
                return false;
            }

            //Go back through and remove pools that have already spawned, unless there is only one entry left
            validPools.Shuffle();
            for(int i = validPools.Count - 1; i >= 0 && validPools.Count > 1; i--)
            {
                if (SpawnedPools.Contains(validPools[i]))
                {
                    summedRarity -= validPools[i].Rarity;
                    validPools.RemoveAt(i);
                }
            }

            //Select a random value within the summed rarity, and select a pool based on that value
            float selectValue = UnityEngine.Random.Range(0, summedRarity);
            float currentSum = 0;
            foreach(EquipmentPoolDef.PoolEntry entry in validPools)
            {
                currentSum += entry.Rarity;
                if(selectValue <= currentSum)
                {
                    __result = entry;
                    SpawnedPools.Add(entry);
                    return false;
                }
            }


            TNHTweakerLogger.LogError("TNHTWEAKER -- Somehow escaped pool entry rarity selection! This is not good!");
            __result = poolDef.Entries[0];
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

            if (__instance.State == TNH_ObjectConstructor.ConstructorState.EntryList)
            {

                int cost = ___m_poolEntries[i].GetCost(__instance.M.EquipmentMode) + ___m_poolAddedCost[i];
                if (__instance.M.GetNumTokens() >= cost)
                {
                    __instance.SetState(TNH_ObjectConstructor.ConstructorState.Confirm, i);
                    SM.PlayCoreSound(FVRPooledAudioType.UIChirp, __instance.AudEvent_Select, __instance.transform.position);
                }
                else
                {
                    SM.PlayCoreSound(FVRPooledAudioType.UIChirp, __instance.AudEvent_Fail, __instance.transform.position);
                }
            }

            else if (__instance.State == TNH_ObjectConstructor.ConstructorState.Confirm)
            {

                if (i == 1)
                {
                    __instance.SetState(TNH_ObjectConstructor.ConstructorState.EntryList, 0);
                    ___m_selectedEntry = -1;
                    SM.PlayCoreSound(FVRPooledAudioType.UIChirp, __instance.AudEvent_Back, __instance.transform.position);
                }
                else if (i == 3)
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

                // This gathers all spawn points, so that multiple things can be spawned at the same time, on different spawnpoints.
                //TODO: I dont like this, but it should work.
                Dictionary<Transform, List<GameObject>> itemsToSpawn = new Dictionary<Transform, List<GameObject>>();
                itemsToSpawn.Add(constructor.SpawnPoint_Mag, new List<GameObject>());
                itemsToSpawn.Add(constructor.SpawnPoint_Ammo, new List<GameObject>());
                itemsToSpawn.Add(constructor.SpawnPoint_Grenade, new List<GameObject>());
                itemsToSpawn.Add(constructor.SpawnPoint_Melee, new List<GameObject>());
                itemsToSpawn.Add(constructor.SpawnPoint_Shield, new List<GameObject>());
                itemsToSpawn.Add(constructor.SpawnPoint_Object, new List<GameObject>());
                
                //This should only have one, and throw when trying to spawn more.
                itemsToSpawn.Add(constructor.SpawnPoint_Case, new List<GameObject>());
                
                foreach (var gunSpawnPoint in constructor.SpawnPoints_GunsSize)
                {
                    itemsToSpawn.Add(gunSpawnPoint, new List<GameObject>());
                }

                TNHTweakerLogger.Log("TNHTWEAKER -- Pool has " + selectedGroups.Count + " groups to spawn from", TNHTweakerLogger.LogType.TNH);
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
                            mainObject = FirearmUtils.GetAmmoContainerForEquipped(group.MinAmmoCapacity, group.MaxAmmoCapacity, character.GetMagazineBlacklist());
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
                            GameObject spawnedObject = UnityEngine.Object.Instantiate(gameObjectCallback.Result, primarySpawn.position + Vector3.up * objectDistancing * mainSpawnCount, primarySpawn.rotation);
                            TNHTweakerLogger.Log("TNHTWEAKER -- Normal item spawned", TNHTweakerLogger.LogType.TNH);
                        }

                        //Spawn any required objects
                        if (mainObject.RequiredSecondaryPieces != null)
                        {
                            for (int j = 0; j < mainObject.RequiredSecondaryPieces.Count; j++)
                            {
                                if(mainObject.RequiredSecondaryPieces[j] == null)
                                {
                                    TNHTweakerLogger.Log("TNHTWEAKER -- Null required object! Skipping", TNHTweakerLogger.LogType.TNH);
                                    continue;
                                }

                                TNHTweakerLogger.Log("TNHTWEAKER -- Spawning Required item", TNHTweakerLogger.LogType.TNH);
                                gameObjectCallback = mainObject.RequiredSecondaryPieces[j].GetGameObjectAsync();
                                yield return gameObjectCallback;
                                GameObject requiredItem = UnityEngine.Object.Instantiate(gameObjectCallback.Result, requiredSpawn.position + -requiredSpawn.right * 0.2f * requiredSpawnCount + Vector3.up * 0.2f * j, requiredSpawn.rotation);
                                requiredSpawnCount += 1;
                            }
                        }
                        

                        //Handle spawning for ammo objects if the main object has any
                        if (FirearmUtils.FVRObjectHasAmmoObject(mainObject))
                        {
                            Dictionary<string, MagazineBlacklistEntry> blacklist = character.GetMagazineBlacklist();
                            MagazineBlacklistEntry blacklistEntry = null;
                            if (blacklist.ContainsKey(mainObject.ItemID)) blacklistEntry = blacklist[mainObject.ItemID];

                            //Get lists of ammo objects for this firearm with filters and blacklists applied
                            List<FVRObject> compatibleMagazines = FirearmUtils.GetCompatibleMagazines(mainObject, group.MinAmmoCapacity, group.MaxAmmoCapacity, true, blacklistEntry);
                            List<FVRObject> compatibleRounds = FirearmUtils.GetCompatibleRounds(mainObject, character.ValidAmmoEras, character.ValidAmmoSets, character.GlobalAmmoBlacklist, blacklistEntry);
                            List<FVRObject> compatibleClips = mainObject.CompatibleClips;

                            TNHTweakerLogger.Log("TNHTWEAKER -- Compatible Mags: " + string.Join(",", compatibleMagazines.Select(o => o.ItemID).ToArray()), TNHTweakerLogger.LogType.TNH);
                            TNHTweakerLogger.Log("TNHTWEAKER -- Compatible Clips: " + string.Join(",", compatibleClips.Select(o => o.ItemID).ToArray()), TNHTweakerLogger.LogType.TNH);
                            TNHTweakerLogger.Log("TNHTWEAKER -- Compatible Rounds: " + string.Join(",", compatibleRounds.Select(o => o.ItemID).ToArray()), TNHTweakerLogger.LogType.TNH);

                            //If we are supposed to spawn magazines and clips, perform special logic for that
                            if (group.SpawnMagAndClip && compatibleMagazines.Count > 0 && compatibleClips.Count > 0 && group.NumMagsSpawned > 0 && group.NumClipsSpawned > 0)
                            {
                                TNHTweakerLogger.Log("TNHTWEAKER -- Spawning with both magazine and clips", TNHTweakerLogger.LogType.TNH);

                                FVRObject magazineObject = compatibleMagazines.GetRandom();
                                FVRObject clipObject = compatibleClips.GetRandom();
                                ammoSpawn = constructor.SpawnPoint_Mag;

                                gameObjectCallback = magazineObject.GetGameObjectAsync();
                                yield return gameObjectCallback;
                                GameObject spawnedMag = UnityEngine.Object.Instantiate(gameObjectCallback.Result, ammoSpawn.position + ammoSpawn.up * 0.05f * ammoSpawnCount, ammoSpawn.rotation);
                                ammoSpawnCount += 1;

                                gameObjectCallback = clipObject.GetGameObjectAsync();
                                yield return gameObjectCallback;
                                for (int i = 0; i < group.NumClipsSpawned; i++)
                                {
                                    GameObject spawnedClip = UnityEngine.Object.Instantiate(gameObjectCallback.Result, ammoSpawn.position + ammoSpawn.up * 0.05f * ammoSpawnCount, ammoSpawn.rotation);
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
                                else if (compatibleClips.Count > 0 && group.NumClipsSpawned > 0)
                                {
                                    ammoObject = compatibleClips.GetRandom();
                                    numSpawned = group.NumClipsSpawned;
                                    ammoSpawn = constructor.SpawnPoint_Mag;
                                }
                                else if (mainObject.CompatibleSpeedLoaders != null && mainObject.CompatibleSpeedLoaders.Count > 0 && group.NumClipsSpawned > 0)
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

                                TNHTweakerLogger.Log("TNHTWEAKER -- Spawning ammo object normally (" + ammoObject.ItemID + "), Count = " + numSpawned, TNHTweakerLogger.LogType.TNH);

                                gameObjectCallback = ammoObject.GetGameObjectAsync();
                                yield return gameObjectCallback;

                                for (int i = 0; i < numSpawned; i++)
                                {
                                    GameObject spawned = UnityEngine.Object.Instantiate(gameObjectCallback.Result, ammoSpawn.position + ammoSpawn.up * 0.05f * ammoSpawnCount, ammoSpawn.rotation);
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
                            GameObject spawnedSight = UnityEngine.Object.Instantiate(gameObjectCallback.Result, constructor.SpawnPoint_Object.position + -constructor.SpawnPoint_Object.right * 0.15f * objectSpawnCount, constructor.SpawnPoint_Object.rotation);

                            TNHTweakerLogger.Log("TNHTWEAKER -- Required sight spawned", TNHTweakerLogger.LogType.TNH);

                            for (int j = 0; j < sight.RequiredSecondaryPieces.Count; j++)
                            {
                                gameObjectCallback = sight.RequiredSecondaryPieces[j].GetGameObjectAsync();
                                yield return gameObjectCallback;
                                GameObject spawnedRequired = UnityEngine.Object.Instantiate(gameObjectCallback.Result, constructor.SpawnPoint_Object.position + -constructor.SpawnPoint_Object.right * 0.15f * objectSpawnCount + Vector3.up * 0.15f * j, constructor.SpawnPoint_Object.rotation);
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
                            GameObject bespokeObject = UnityEngine.Object.Instantiate(gameObjectCallback.Result, constructor.SpawnPoint_Object.position + -constructor.SpawnPoint_Object.right * 0.15f * objectSpawnCount, constructor.SpawnPoint_Object.rotation);
                            objectSpawnCount += 1;

                            TNHTweakerLogger.Log("TNHTWEAKER -- Bespoke item spawned", TNHTweakerLogger.LogType.TNH);
                        }
                    }
                }
            }

            constructor.allowEntry = true;
            yield break;
        }


        #endregion


        #region Misc Patches

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
            SpawnedPools.Clear();

            while (SpawnedConstructors.Count > 0)
            {
                try
                {
                    TNH_ObjectConstructor constructor = SpawnedConstructors[0].GetComponent<TNH_ObjectConstructor>();

                    if (constructor != null)
                    {
                        constructor.ClearCase();
                    }

                    UnityEngine.Object.Destroy(SpawnedConstructors[0]);
                }
                catch
                {
                    TNHTweakerLogger.LogWarning("TNHTWEAKER -- Failed to destroy constructor! It's likely that the constructor is already destroyed, so everything is probably just fine :)");
                }

                SpawnedConstructors.RemoveAt(0);
            }

            while (SpawnedPanels.Count > 0)
            {
                UnityEngine.Object.Destroy(SpawnedPanels[0]);
                SpawnedPanels.RemoveAt(0);
            }
        }

        [HarmonyPatch(typeof(Sosig), "BuffHealing_Invis")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool OverrideCloaking()
        {
            return !preventOutfitFunctionality;
        }


        [HarmonyPatch(typeof(TNH_ScoreDisplay), "ProcessHighScore")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool PreventScoring()
        {
            return false;
        }

        #endregion

    }
}
