using Deli.Newtonsoft.Json;
using FistVR;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using TNHTweaker.ObjectTemplates;
using TNHTweaker.Utilities;
using UnityEngine.Networking;
using Steamworks;

namespace TNHTweaker.Main.Patches
{
    public static class HighScorePatches
    {

        private static string[] equipment = { "Spawnlock", "Limited" };
        private static string[] health = { "Standard", "One-Hit" };
        private static string[] length = { "5-Hold", "Endless", "3-Hold" };


        [HarmonyPatch(typeof(TNH_Manager), "DelayedInit")]
        [HarmonyPrefix]
        public static bool StartOfGamePatch(TNH_Manager __instance)
        {
            //Clear all entries from the tracked stats
            TNHTweaker.HoldActions.Clear();
            TNHTweaker.HoldStats.Clear();

            return true;
        }


        [HarmonyPatch(typeof(TNH_Manager), "InitPlayerPosition")]
        [HarmonyPrefix]
        public static bool TrackPlayerSpawnPatch(TNH_Manager __instance)
        {
            TNHTweaker.HoldActions[0].Add($"Spawned At Supply {__instance.m_curPointSequence.StartSupplyPointIndex}");

            return true;
        }


        [HarmonyPatch(typeof(TNH_Manager), "HoldPointCompleted")]
        [HarmonyPrefix]
        public static bool TrackHoldCompletion(TNH_Manager __instance)
        {
            TNHTweaker.HoldStats.Add(new HoldStats()
            {
                SosigsKilled = __instance.Stats[3],
                MeleeKills = __instance.Stats[5],
                Headshots = __instance.Stats[4],
                TokensSpent = __instance.Stats[8],
                GunsRecycled = 0,
                AmmoSpent = 0
            });

            __instance.Stats[3] = 0;
            __instance.Stats[5] = 0;
            __instance.Stats[4] = 0;
            __instance.Stats[8] = 0;

            return true;
        }


        [HarmonyPatch(typeof(TNH_Manager), "SetLevel")]
        [HarmonyPrefix]
        public static bool TrackNextLevel(TNH_Manager __instance)
        {
            TNHTweaker.HoldActions.Add(new List<string>());

            return true;
        }


        [HarmonyPatch(typeof(TNH_HoldPoint), "BeginHoldChallenge")]
        [HarmonyPrefix]
        public static bool TrackHoldStart(TNH_HoldPoint __instance)
        {
            TNHTweaker.HoldActions[__instance.M.m_level].Add($"Entered Hold {__instance.M.HoldPoints.IndexOf(__instance)}");

            return true;
        }


        [HarmonyPatch(typeof(TNH_GunRecycler), "Button_Recycler")]
        [HarmonyPrefix]
        public static bool TrackRecyclePatch(TNH_GunRecycler __instance)
        {
            if(__instance.m_detectedFirearms.Count > 0 && __instance.m_detectedFirearms[0] != null)
            {
                TNHTweaker.HoldActions[__instance.M.m_level].Add($"Recycled {__instance.m_detectedFirearms[0].ObjectWrapper.DisplayName}");
            }

            return true;
        }


        [HarmonyPatch(typeof(TNH_SupplyPoint), "TestVisited")]
        [HarmonyPrefix]
        public static bool TrackSupplyVisitsPatch(TNH_SupplyPoint __instance, ref bool __result)
        {
            if (!__instance.m_isconfigured)
            {
                __result = false;
                return false;
            }

            bool flag = __instance.TestVolumeBool(__instance.Bounds, GM.CurrentPlayerBody.transform.position);
            if (flag)
            {
                if (!__instance.m_hasBeenVisited && __instance.m_contact != null)
                {
                    __instance.m_contact.SetVisited(true);
                    TNHTweaker.HoldActions[__instance.M.m_level].Add($"Entered Supply {__instance.M.SupplyPoints.IndexOf(__instance)}");
                }
                __instance.m_hasBeenVisited = true;
            }
            __result = flag;

            return false;
        }



        [HarmonyPatch(typeof(TNH_ScoreDisplay), "SubmitScoreAndGoToBoard")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        public static bool PreventScoring(TNH_ScoreDisplay __instance, int score)
        {

            GM.Omni.OmniFlags.AddScore(__instance.m_curSequenceID, score);

            __instance.m_hasCurrentScore = true;
            __instance.m_currentScore = score;


            foreach(int i in GM.TNH_Manager.Stats)
            {
                TNHTweakerLogger.Log($"Stat : {i}", TNHTweakerLogger.LogType.TNH);
            }

            TNHTweakerLogger.Log("Name: " + SteamFriends.GetPersonaName(), TNHTweakerLogger.LogType.General);

            AnvilManager.Instance.StartCoroutine(PostScore(score));

            __instance.RedrawHighScoreDisplay(__instance.m_curSequenceID);

            GM.Omni.SaveToFile();

            return false;
        }


        public static ScoreEntry GetScoreEntry(TNH_Manager instance, int score)
        {
            ScoreEntry entry = new ScoreEntry();

            entry.Name = SteamFriends.GetPersonaName();
            entry.Score = score;
            entry.Character = instance.C.DisplayName;
            entry.Map = instance.LevelName;
            entry.EquipmentMode = equipment[(int)GM.TNHOptions.EquipmentModeSetting];
            entry.HealthMode = health[(int)GM.TNHOptions.HealthModeSetting];
            entry.GameLength = length[(int)GM.TNHOptions.ProgressionTypeSetting];
            entry.HoldActions = JsonConvert.SerializeObject(TNHTweaker.HoldActions);
            entry.HoldStats = JsonConvert.SerializeObject(TNHTweaker.HoldStats);

            return entry;
        }


        public static UnityWebRequest GetScoresAPIWebRequest(int score)
        {
            string url = "https://tnh-dashboard.azure-api.net/v1/api/scores";

            UnityWebRequest www = new UnityWebRequest(url, "Put");

            ScoreEntry entry = GetScoreEntry(GM.TNH_Manager, score);

            string data = JsonConvert.SerializeObject(entry);

            TNHTweakerLogger.Log("Sending score entry: " + data, TNHTweakerLogger.LogType.TNH);

            www.SetRequestHeader(Globals.Accept, "*/*");
            www.SetRequestHeader(Globals.Content_Type, Globals.ApplicationJson);
            www.downloadHandler = new DownloadHandlerBuffer();

            if (!string.IsNullOrEmpty(data))
            {
                byte[] payload = Encoding.UTF8.GetBytes(data);
                UploadHandler handler = new UploadHandlerRaw(payload);
                handler.contentType = "application/json";
                www.uploadHandler = handler;
            }

            return www;
        }

        public static IEnumerator PostScore(int score)
        {
            using (UnityWebRequest www = GetScoresAPIWebRequest(score))
            {
                yield return www.Send();

                TNHTweakerLogger.Log("Sent!", TNHTweakerLogger.LogType.TNH);
            }
        }


    }
}
