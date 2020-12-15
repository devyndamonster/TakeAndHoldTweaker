using System.Collections.Generic;

namespace TNHTweaker.Extentions
{
    public static class Extensions
    {

        public static T GetRandom<T>(this List<T> list)
        {
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

    }

}
