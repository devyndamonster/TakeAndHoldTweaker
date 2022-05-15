using LegacyCharacterLoader.Objects.CharacterData;
using LegacyCharacterLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.Objects.CharacterData;
using UnityEngine;

namespace LegacyCharacterLoader.LegacyConverters
{
    public static class LegacyProgressionConverter
    {
		public static Progression ConvertProgressionFromLegacy(List<LegacyLevel> from)
		{
			LogConversionStart(from);
			Progression progression = ScriptableObject.CreateInstance<Progression>();

			progression.Levels = from.Select(o => LegacyLevelConverter.ConvertLevelFromLegacy(o)).ToList();

			LogConversionEnd(progression);
			return progression;
		}

		private static void LogConversionStart(List<LegacyLevel> from)
		{
			LegacyLogger.Log($"- Starting conversion of legacy progression -", LegacyLogger.LogType.Loading);
		}

		private static void LogConversionEnd(Progression to)
		{
			LegacyLogger.Log($"- Finished conversion of legacy progression -", LegacyLogger.LogType.Loading);
		}
	}
}
