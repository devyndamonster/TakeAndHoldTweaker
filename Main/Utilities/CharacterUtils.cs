using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TNHTweaker.Utilities
{
    public static class CharacterUtils
    {
        public static string GetGroupStringFromEnum(TNH_CharacterDef.CharacterGroup groupEnum)
        {
            switch (groupEnum)
            {
                case TNH_CharacterDef.CharacterGroup.DaringDefaults:
                    return "Daring Defaults";
                case TNH_CharacterDef.CharacterGroup.WienersThroughTime:
                    return "Weiners Through Time";
                case TNH_CharacterDef.CharacterGroup.MemetasticMeats:
                    return "Memetastic Meats";
                default: return "Misc";
            }
        }

        public static TNH_Char GetUniqueTNHCharValue()
        {
            return (TNH_Char)(111000 + TNHTweaker.CustomCharacterDict.Keys.Count());
        }
    }
}
