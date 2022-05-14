using LegacyCharacterLoader.Objects.CharacterData;
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
			Progression progression = ScriptableObject.CreateInstance<Progression>();

			progression.Levels = from.Select(o => LegacyLevelConverter.ConvertLevelFromLegacy(o)).ToList();

			return progression;
		}
	}
}
