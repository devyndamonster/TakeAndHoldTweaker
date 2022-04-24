using FistVR;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace TNHTweaker.Main.Patches
{
    public class TNHMenuPatches
    {

        [HarmonyPatch(typeof(TNH_UIManager), "Start")]
        [HarmonyPrefix]
        public static bool InitTNH(List<TNH_UIManager.CharacterCategory> ___Categories, TNH_CharacterDatabase ___CharDatabase, TNH_UIManager __instance)
        {
            TNHTweakerLogger.Log("Start method of TNH_UIManager just got called!", TNHTweakerLogger.LogType.General);

            //Add menu wrapper to UI
















            /*
            

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
            */

            return true;
        }
    }
}
