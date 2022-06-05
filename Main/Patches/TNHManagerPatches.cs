using FistVR;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Sodalite.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TNHTweaker.Objects.CharacterData;
using TNHTweaker.Objects.LootPools;
using TNHTweaker.ObjectWrappers;
using TNHTweaker.Utilities;
using UnityEngine;

namespace TNHTweaker.Patches
{
    public static class TNHManagerPatches
    {

        /// <summary>
        /// Before initializing the base character classes tables, initialize the extended character classes tables <br/><br/>
        /// Related Features: <br/>
        /// - <see href="https://github.com/devyndamonster/TakeAndHoldTweaker/issues/101"> Have starting equipment use our own EquipmentGroup loot system </see><br/>
        /// - <see href="https://github.com/devyndamonster/TakeAndHoldTweaker/issues/105"> Have object constructor use our own EquipmentGroup loot system </see><br/>
        /// </summary>
        [HarmonyPatch(typeof(TNH_Manager), "InitTables")]
        [HarmonyPrefix]
        public static bool InitTablesPatch(TNH_Manager __instance)
        {
            TNHManagerStateWrapper.Instance.GetCurrentCharacter().GenerateTables();

            return true;
        }



        /// <summary>
        /// Patches SetPhase_Take method to use the SupplyChallenges min and max boxes values <br/><br/>
        /// Related Features: <br/>
        /// - <see href="https://github.com/devyndamonster/TakeAndHoldTweaker/issues/106"> Allow you to set min and max boxes spawned at supply points </see><br/>
        /// </summary>
        [HarmonyPatch(typeof(TNH_Manager), "SetPhase_Take")]
        [HarmonyILManipulator]
        public static void SetPhaseTakeBoxCountPatch(ILContext ctx, MethodBase orig)
        {
            ILCursor cursor = new ILCursor(ctx);

            //Remove the two constant value for box counts
            cursor.GotoNext(MoveType.After,
                i => i.MatchLdcI4(1),
                i => i.MatchLdcI4(1),
                i => i.MatchLdcI4(1),
                i => i.MatchLdloc(16)
                );
            cursor.RemoveRange(2);

            //Now add calls to get min and max supply point boxes
            cursor.Emit(OpCodes.Ldsfld, AccessTools.Field(typeof(TNHManagerStateWrapper), "Instance"));
            cursor.Emit(OpCodes.Callvirt, AccessTools.Method(typeof(TNHManagerStateWrapper), "GetCurrentSupplyChallenge"));
            cursor.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(SupplyChallenge), "MinBoxesSpawned"));

            cursor.Emit(OpCodes.Ldsfld, AccessTools.Field(typeof(TNHManagerStateWrapper), "Instance"));
            cursor.Emit(OpCodes.Callvirt, AccessTools.Method(typeof(TNHManagerStateWrapper), "GetCurrentSupplyChallenge"));
            cursor.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(SupplyChallenge), "MaxBoxesSpawned"));
        }


        /// <summary>
        /// Patches GenerateValidPatrol method to call our own GeneratePatrol method <br/><br/>
        /// Related Features: <br/>
        /// - <see href="https://github.com/devyndamonster/TakeAndHoldTweaker/issues/111"> Allow multiple types of sosigs to spawn in patrols </see><br/>
        /// </summary>
        [HarmonyPatch(typeof(TNH_Manager), "GenerateValidPatrol")]
        [HarmonyILManipulator]
        public static void GenerateValidPatrolPatch(ILContext ctx, MethodBase orig)
        {
            ILCursor cursor = new ILCursor(ctx);

            //Remove call to generate the patrol
            PatchUtils.RemoveStartToEnd(
                cursor,
                new Func<Instruction, bool>[]
                {
                    i => i.MatchLdloc(0),
                    i => i.MatchLdfld(AccessTools.Field(typeof(TNH_PatrolChallenge.Patrol), "LType"))
                },
                new Func<Instruction, bool>[]
                {
                    i => i.MatchLdfld(AccessTools.Field(typeof(TNH_PatrolChallenge.Patrol), "IFFUsed")),
                    i => true,
                    i => i.MatchStloc(11)
                }
            );

            //Get random patrol index and store it
            cursor.Emit(OpCodes.Call, ((Func<TNH_Manager, int>)GetRandomPatrolIndex).Method);
            cursor.Emit(OpCodes.Stloc, 8);

            //Set patrol from this patrol index
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(TNH_PatrolChallenge), "Patrols"));
            cursor.Emit(OpCodes.Ldloc, 8);
            cursor.Emit(OpCodes.Callvirt, AccessTools.Method(typeof(List<TNH_PatrolChallenge.Patrol>), "get_Item", new Type[] {typeof(int)}));
            cursor.Emit(OpCodes.Stloc_0);

            //Now generate the patrol with our own method
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloc, 8);
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit(OpCodes.Ldc_I4_0);
            cursor.Emit(OpCodes.Callvirt, AccessTools.Method(typeof(List<int>), "get_Item", new Type[] { typeof(int) }));
            cursor.Emit(OpCodes.Call, ((Func<TNH_Manager, int, int, TNH_Manager.SosigPatrolSquad>)GeneratePatrol).Method);
            cursor.Emit(OpCodes.Stloc, 11);
        }


        public static int GetRandomPatrolIndex(TNH_Manager manager)
        {
            return TNHManagerStateWrapper.Instance.GetCurrentLevel().Patrols.GetRandomIndex();
        }


        public static TNH_Manager.SosigPatrolSquad GeneratePatrol(TNH_Manager manager, int patrolIndex, int holdPointStart)
        {
            TNH_Manager.SosigPatrolSquad sosigPatrolSquad = new TNH_Manager.SosigPatrolSquad();
            Patrol patrol = TNHManagerStateWrapper.Instance.GetCurrentLevel().Patrols[patrolIndex];
            manager.HoldPoints[holdPointStart].SpawnPoints_Sosigs_Defense.Shuffle();

            foreach (TNH_HoldPoint patrolPoint in GetPatrolPoints(manager, holdPointStart))
            {
                sosigPatrolSquad.PatrolPoints.Add(patrolPoint.SpawnPoints_Sosigs_Defense.GetRandom().position);
            }

            for(int sosigIndex = 0; sosigIndex < patrol.PatrolSize; sosigIndex++)
            {
                Transform spawnPoint = manager.HoldPoints[holdPointStart].SpawnPoints_Sosigs_Defense[sosigIndex];
                SosigEnemyID sosigID = sosigIndex == 0 ? patrol.LeaderType : patrol.EnemyTypes.GetRandom();
                bool allowAllWeapons = sosigIndex == 0;
                SosigEnemyTemplate sosigTemplate = IM.Instance.odicSosigObjsByID[sosigID];

                Sosig spawnedSosig = manager.SpawnEnemy(
                    sosigTemplate,
                    spawnPoint.position,
                    spawnPoint.rotation,
                    patrol.IFFUsed,
                    true,
                    sosigPatrolSquad.PatrolPoints[0],
                    allowAllWeapons
                );

                if (sosigIndex == 0 && RandomUtils.Evaluate(0.35f))
                {
                    spawnedSosig.Links[1].RegisterSpawnOnDestroy(manager.Prefab_HealthPickupMinor);
                }

                spawnedSosig.SetAssaultSpeed(Sosig.SosigMoveSpeed.Walking);
                sosigPatrolSquad.Squad.Add(spawnedSosig);
            }

            return sosigPatrolSquad;
        }

        private static List<TNH_HoldPoint> GetPatrolPoints(TNH_Manager manager, int holdPointStart)
        {
            List<TNH_HoldPoint> patrolPoints = new List<TNH_HoldPoint>();
            patrolPoints.Add(manager.HoldPoints[holdPointStart]);

            List<TNH_HoldPoint> holdPoints = new List<TNH_HoldPoint>(manager.HoldPoints);
            holdPoints.RemoveAt(holdPointStart);

            holdPoints.Shuffle();
            patrolPoints.AddRange(holdPoints.Take(5));

            return patrolPoints;
        }

    }
}
