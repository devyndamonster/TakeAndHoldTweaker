using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.Objects.CharacterData;
using TNHTweaker.Objects.LootPools;
using UnityEngine;

namespace TNHTweaker.ObjectConverters
{
	public static class ProgressionConverter
	{
		public static Progression ConvertProgressionFromVanilla(TNH_Progression from)
		{
			Progression progression = ScriptableObject.CreateInstance<Progression>();

			progression.Levels = from.Levels.Select(o => LevelConverter.ConvertLevelFromVanilla(o)).ToList();

			return progression;
		}


		public static TNH_Progression ConvertProgressionToVanilla(Progression from)
		{
			TNH_Progression progression = ScriptableObject.CreateInstance<TNH_Progression>();

			progression.Levels = from.Levels.Select(o => LevelConverter.ConvertLevelToVanilla(o)).ToList();

			return progression;
		}
	}
}
