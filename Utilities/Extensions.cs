using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TNHTweaker.Utilities
{  
    public static class Extensions
    {

        public static T GetRandom<T>(this List<T> list)
        {
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        public static bool ContainsNull<T>(this List<T> list)
        {
            for(int i = 0; i < list.Count; i++)
            {
                if (list[i] == null) return true;
            }
            return false;
        }

    }

}
