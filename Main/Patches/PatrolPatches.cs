using FistVR;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.ObjectTemplates;
using TNHTweaker.Utilities;
using UnityEngine;

namespace TNHTweaker.Patches
{
    public static class PatrolPatches
    {
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

            while (TNHTweaker.SpawnedBossIndexes.Contains(index) && attempts < patrols.Count)
            {
                attempts += 1;
                index += 1;
                if (index >= patrols.Count) index = 0;
            }

            if (TNHTweaker.SpawnedBossIndexes.Contains(index)) return -1;

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


        //TODO To choose spawn based on IFF, we need to basically generate spawn points on our own in this method!
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
                    TNHTweaker.SpawnedBossIndexes.Add(patrolIndex);
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

            foreach (Transform spawnPoint in instance.HoldPoints[HoldPointStart].SpawnPoints_Sosigs_Defense)
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

        [HarmonyPatch(typeof(Sosig), "BuffHealing_Invis")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool OverrideCloaking()
        {
            return !TNHTweaker.PreventOutfitFunctionality;
        }


    }
}
