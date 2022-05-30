using FistVR;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TNHTweaker.ObjectConverters;
using TNHTweaker.Objects.CharacterData;
using TNHTweaker.ObjectWrappers;
using TNHTweaker.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace TNHTweaker.Patches
{
    public static class TNHMenuPatches
    {

        /// <summary>
        /// Before UI Manager starts, run initialization logic <br/><br/>
        /// Related Features: <br/>
        /// - <see href="https://github.com/devyndamonster/TakeAndHoldTweaker/issues/102"> Add support for unlimited number of character pages in TNH Menu </see><br/>
        /// - <see href="https://github.com/devyndamonster/TakeAndHoldTweaker/issues/103"> Allow for vanilla characters to be converted into our own character class </see><br/>
        /// </summary>
        [HarmonyPatch(typeof(TNH_UIManager), "Start")]
        [HarmonyPrefix]
        public static bool InitTNHPatch(TNH_UIManager __instance)
        {
            TNHTweakerLogger.Log("Initializing TNH UI", TNHTweakerLogger.LogType.General);

            ConvertDefaultCharacters(__instance.CharDatabase);

            //Add menu wrapper to UI
            TNHMenuUIWrapper menuWrapper = __instance.gameObject.AddComponent<TNHMenuUIWrapper>();

            return true;
        }


        /// <summary>
        /// Overrides logic that displays a new character category with our own method call <br/><br/>
        /// Related Features: <br/>
        /// - <see href="https://github.com/devyndamonster/TakeAndHoldTweaker/issues/102"> Add support for unlimited number of character pages in TNH Menu </see><br/>
        /// </summary>
        [HarmonyPatch(typeof(TNH_UIManager), "SetSelectedCategory")]
        [HarmonyILManipulator]
        public static void CategoryRefreshPatch(ILContext ctx, MethodBase orig)
        {
            ILCursor c = new ILCursor(ctx);

            //Match to the beginning of the for loop for UI refresh
            c.GotoNext(
                i => i.MatchLdcI4(0),
                i => i.MatchStloc(0),
                i => true,
                i => i.MatchLdloc(0),
                i => i.MatchLdarg(0)
            );
            int startOfLoopIndex = c.Index;

            //Match to the end of the for loop for UI refresh
            c.GotoNext(
                i => i.MatchStloc(0),
                i => i.MatchLdloc(0),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(AccessTools.Field(typeof(TNH_UIManager), "LBL_CharacterName")),
                i => i.MatchCallvirt(AccessTools.Method(typeof(List<Text>), "get_Count"))
            );
            int endOfLoopIndex = c.Index;

            //Remove all of the for loop
            //+7 to index is still strange, but the desired range is 63 and the range is off by 7
            c.Index = startOfLoopIndex;
            int loopRange = (endOfLoopIndex - startOfLoopIndex) + 7;
            c.RemoveRange(loopRange);

            //Now add our new call for refreshing the UI
            c.Emit(OpCodes.Ldsfld, AccessTools.Field(typeof(TNHMenuUIWrapper), "Instance"));
            c.Emit(OpCodes.Callvirt, AccessTools.Method(typeof(TNHMenuUIWrapper), "DisplayNewCategory"));
            c.Emit(OpCodes.Nop);
        }

        private static void ConvertDefaultCharacters(TNH_CharacterDatabase charDatabase)
        {
            foreach (TNH_CharacterDef character in charDatabase.Characters)
            {
                if (!TNHTweaker.CustomCharacterDict.ContainsKey(character))
                {
                    Character customCharacter = CharacterConverter.ConvertCharacterFromVanilla(character);
                    TNHTweaker.CustomCharacterDict[character] = customCharacter;
                    TNHTweaker.BaseCharacterDict[customCharacter] = character;
                }
            }
        }
    }
}
