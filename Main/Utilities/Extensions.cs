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

        public static bool ContainsNull<T>(this List<T> list)
        {
            for(int i = 0; i < list.Count; i++)
            {
                if (list[i] == null) return true;
            }
            return false;
        }
        
        public static Bounds GetMaxBounds(this GameObject g) {
            var b = new Bounds(g.transform.position, Vector3.zero);
            foreach (Renderer r in g.GetComponentsInChildren<Renderer>()) {
                b.Encapsulate(r.bounds);
            }
            return b;
        }
    }

}
