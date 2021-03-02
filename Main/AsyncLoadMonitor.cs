using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.Utilities;
using UnityEngine;

namespace TNHTweaker
{
    public static class AsyncLoadMonitor
    {

        public static List<AnvilCallback<AssetBundle>> CallbackList = new List<AnvilCallback<AssetBundle>>();

        public static float GetProgress()
        {
            if (CallbackList.Count == 0) return 1;

            float totalStatus = 0;

            for(int i = 0; i < CallbackList.Count; i++)
            {
                if (CallbackList[i].IsCompleted)
                {
                    CallbackList.RemoveAt(i);
                    i -= 1;
                }

                else
                {
                    totalStatus += CallbackList[i].Progress;
                }
            }

            return totalStatus / CallbackList.Count;
        }
    }
}
