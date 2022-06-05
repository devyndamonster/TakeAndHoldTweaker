using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TNHTweaker.Utilities
{
    public static class RandomUtils
    {
        /// <summary>
        /// Returns result of comparison between the given value, and a random value between 0 and 1
        /// </summary>
        /// <param name="value"> Chance value between 0 and 1. Larger values will return true more often </param>
        /// <returns> Randomly returns true or false, with chance of result based on input value </returns>
        public static bool Evaluate(float value)
        {
            return UnityEngine.Random.Range(0f, 1f) <= value;
        }
    }
}
