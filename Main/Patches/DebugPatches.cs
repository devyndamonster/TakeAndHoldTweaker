using FistVR;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.Utilities;

namespace TNHTweaker
{
    public class DebugPatches
    {
		/*
		[HarmonyPatch(typeof(ObjectTable))] // Specify target method with HarmonyPatch attribute
		[HarmonyPatch("Initialize")]
		[HarmonyPatch(new Type[] { typeof(ObjectTableDef), typeof(FVRObject.ObjectCategory), typeof(List<FVRObject.OTagEra>), typeof(List<FVRObject.OTagSet>), typeof(List<FVRObject.OTagFirearmSize>), typeof(List<FVRObject.OTagFirearmAction>), typeof(List<FVRObject.OTagFirearmFiringMode>), typeof(List<FVRObject.OTagFirearmFiringMode>), typeof(List<FVRObject.OTagFirearmFeedOption>), typeof(List<FVRObject.OTagFirearmMount>), typeof(List<FVRObject.OTagFirearmRoundPower>), typeof(List<FVRObject.OTagAttachmentFeature>), typeof(List<FVRObject.OTagMeleeStyle>), typeof(List<FVRObject.OTagMeleeHandedness>), typeof(List<FVRObject.OTagFirearmMount>), typeof(List<FVRObject.OTagPowerupType>), typeof(List<FVRObject.OTagThrownType>), typeof(List<FVRObject.OTagThrownDamageType>), typeof(int), typeof(int), typeof(int), typeof(bool)})]
		[HarmonyPrefix]
		public static bool Initialize(ObjectTable __instance, ObjectTableDef Def, FVRObject.ObjectCategory category, List<FVRObject.OTagEra> eras, List<FVRObject.OTagSet> sets, List<FVRObject.OTagFirearmSize> sizes, List<FVRObject.OTagFirearmAction> actions, List<FVRObject.OTagFirearmFiringMode> modes, List<FVRObject.OTagFirearmFiringMode> excludeModes, List<FVRObject.OTagFirearmFeedOption> feedoptions, List<FVRObject.OTagFirearmMount> mountsavailable, List<FVRObject.OTagFirearmRoundPower> roundPowers, List<FVRObject.OTagAttachmentFeature> features, List<FVRObject.OTagMeleeStyle> meleeStyles, List<FVRObject.OTagMeleeHandedness> meleeHandedness, List<FVRObject.OTagFirearmMount> mounttype, List<FVRObject.OTagPowerupType> powerupTypes, List<FVRObject.OTagThrownType> thrownTypes, List<FVRObject.OTagThrownDamageType> thrownDamageTypes, int minCapacity, int maxCapacity, int requiredExactCapacity, bool isBlanked)
		{
			__instance.MinCapacity = minCapacity;
			__instance.MaxCapacity = maxCapacity;
			if (isBlanked)
			{
				TNHTweakerLogger.Log("Table is blanked, not populating!", TNHTweakerLogger.LogType.TNH);
				return false;
			}
			if (Def.UseIDListOverride)
			{
				TNHTweakerLogger.Log("Using IDOverride! Will only add IDs manually", TNHTweakerLogger.LogType.TNH);

				for (int i = 0; i < Def.IDOverride.Count; i++)
				{
					__instance.Objs.Add(IM.OD[Def.IDOverride[i]]);
				}
				return false;
			}

			TNHTweakerLogger.Log("Not using IDOverride, table will populate automatically", TNHTweakerLogger.LogType.TNH);

			__instance.Objs = new List<FVRObject>(ManagerSingleton<IM>.Instance.odicTagCategory[category]);

			TNHTweakerLogger.Log("Aquired all objects from Category (" + category + "), Listing them below", TNHTweakerLogger.LogType.TNH);
			TNHTweakerLogger.Log(string.Join("\n", __instance.Objs.Select(o => o.ItemID).ToArray()), TNHTweakerLogger.LogType.TNH);

			TNHTweakerLogger.Log("Going through and removing items that do not match desired tags", TNHTweakerLogger.LogType.TNH);

			for (int j = __instance.Objs.Count - 1; j >= 0; j--)
			{
				FVRObject fvrobject = __instance.Objs[j];
				TNHTweakerLogger.Log("Looking at item (" + fvrobject.ItemID + ")", TNHTweakerLogger.LogType.TNH);

				if (!fvrobject.OSple)
				{
					TNHTweakerLogger.Log("OSple is false, removing", TNHTweakerLogger.LogType.TNH);
					__instance.Objs.RemoveAt(j);
				}
				else if (minCapacity > -1 && fvrobject.MaxCapacityRelated < minCapacity)
				{
					TNHTweakerLogger.Log("Magazines not big enough, removing", TNHTweakerLogger.LogType.TNH);
					__instance.Objs.RemoveAt(j);
				}
				else if (maxCapacity > -1 && fvrobject.MinCapacityRelated > maxCapacity)
				{
					TNHTweakerLogger.Log("Magazines not small enough, removing", TNHTweakerLogger.LogType.TNH);
					__instance.Objs.RemoveAt(j);
				}
				else if (requiredExactCapacity > -1 && !__instance.DoesGunMatchExactCapacity(fvrobject))
				{
					TNHTweakerLogger.Log("Not exact capacity, removing", TNHTweakerLogger.LogType.TNH);
					__instance.Objs.RemoveAt(j);
				}
				else if (eras != null && eras.Count > 0 && !eras.Contains(fvrobject.TagEra))
				{
					TNHTweakerLogger.Log("Wrong era, removing", TNHTweakerLogger.LogType.TNH);
					__instance.Objs.RemoveAt(j);
				}
				else if (sets != null && sets.Count > 0 && !sets.Contains(fvrobject.TagSet))
				{
					TNHTweakerLogger.Log("Wrong set, removing", TNHTweakerLogger.LogType.TNH);
					__instance.Objs.RemoveAt(j);
				}
				else if (sizes != null && sizes.Count > 0 && !sizes.Contains(fvrobject.TagFirearmSize))
				{
					TNHTweakerLogger.Log("Wrong size, removing", TNHTweakerLogger.LogType.TNH);
					__instance.Objs.RemoveAt(j);
				}
				else if (actions != null && actions.Count > 0 && !actions.Contains(fvrobject.TagFirearmAction))
				{
					TNHTweakerLogger.Log("Wrong actions, removing", TNHTweakerLogger.LogType.TNH);
					__instance.Objs.RemoveAt(j);
				}
				else if (roundPowers != null && roundPowers.Count > 0 && !roundPowers.Contains(fvrobject.TagFirearmRoundPower))
				{
					TNHTweakerLogger.Log("Wrong round power, removing", TNHTweakerLogger.LogType.TNH);
					__instance.Objs.RemoveAt(j);
				}
				else
				{
					if (modes != null && modes.Count > 0)
					{
						bool flag = false;
						for (int k = 0; k < modes.Count; k++)
						{
							if (!fvrobject.TagFirearmFiringModes.Contains(modes[k]))
							{
								flag = true;
								break;
							}
						}
						if (flag)
						{
							TNHTweakerLogger.Log("Wrong firing modes, removing", TNHTweakerLogger.LogType.TNH);
							__instance.Objs.RemoveAt(j);
							break;
						}
					}
					if (excludeModes != null)
					{
						bool flag2 = false;
						for (int l = 0; l < excludeModes.Count; l++)
						{
							if (fvrobject.TagFirearmFiringModes.Contains(excludeModes[l]))
							{
								flag2 = true;
								break;
							}
						}
						if (flag2)
						{
							TNHTweakerLogger.Log("Excluded firing modes, removing", TNHTweakerLogger.LogType.TNH);
							__instance.Objs.RemoveAt(j);
							break;
						}
					}
					if (feedoptions != null)
					{
						bool flag3 = false;
						for (int m = 0; m < feedoptions.Count; m++)
						{
							if (!fvrobject.TagFirearmFeedOption.Contains(feedoptions[m]))
							{
								flag3 = true;
								break;
							}
						}
						if (flag3)
						{
							TNHTweakerLogger.Log("Wrong feed options, removing", TNHTweakerLogger.LogType.TNH);
							__instance.Objs.RemoveAt(j);
							break;
						}
					}
					if (mountsavailable != null)
					{
						bool flag4 = false;
						for (int n = 0; n < mountsavailable.Count; n++)
						{
							if (!fvrobject.TagFirearmMounts.Contains(mountsavailable[n]))
							{
								flag4 = true;
								break;
							}
						}
						if (flag4)
						{
							TNHTweakerLogger.Log("Wrong mounts, removing", TNHTweakerLogger.LogType.TNH);
							__instance.Objs.RemoveAt(j);
							break;
						}
					}
					if (powerupTypes != null && powerupTypes.Count > 0 && !powerupTypes.Contains(fvrobject.TagPowerupType))
					{
						TNHTweakerLogger.Log("Wrong powerup type, removing", TNHTweakerLogger.LogType.TNH);
						__instance.Objs.RemoveAt(j);
					}
					else if (thrownTypes != null && thrownTypes.Count > 0 && !thrownTypes.Contains(fvrobject.TagThrownType))
					{
						TNHTweakerLogger.Log("Wrong thrown type, removing", TNHTweakerLogger.LogType.TNH);
						__instance.Objs.RemoveAt(j);
					}
					else if (thrownTypes != null && thrownTypes.Count > 0 && !thrownTypes.Contains(fvrobject.TagThrownType))
					{
						__instance.Objs.RemoveAt(j);
					}
					else if (meleeStyles != null && meleeStyles.Count > 0 && !meleeStyles.Contains(fvrobject.TagMeleeStyle))
					{
						TNHTweakerLogger.Log("Wrong melee style, removing", TNHTweakerLogger.LogType.TNH);
						__instance.Objs.RemoveAt(j);
					}
					else if (meleeHandedness != null && meleeHandedness.Count > 0 && !meleeHandedness.Contains(fvrobject.TagMeleeHandedness))
					{
						TNHTweakerLogger.Log("Wrong melee handedness, removing", TNHTweakerLogger.LogType.TNH);
						__instance.Objs.RemoveAt(j);
					}
					else if (mounttype != null && mounttype.Count > 0 && !mounttype.Contains(fvrobject.TagAttachmentMount))
					{
						TNHTweakerLogger.Log("Wrong mount type, removing", TNHTweakerLogger.LogType.TNH);
						__instance.Objs.RemoveAt(j);
					}
					else if (features != null && features.Count > 0 && !features.Contains(fvrobject.TagAttachmentFeature))
					{
						TNHTweakerLogger.Log("Wrong features, removing", TNHTweakerLogger.LogType.TNH);
						__instance.Objs.RemoveAt(j);
					}

                    else
                    {
						TNHTweakerLogger.Log("Keeping item!", TNHTweakerLogger.LogType.TNH);
					}
				}
			}

			return false;
		}
		*/

	}
}
