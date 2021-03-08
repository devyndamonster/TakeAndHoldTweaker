using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNHTweaker.Utilities;
using UnityEngine;

namespace TNHTweaker
{
    public class SosigLinkLootWrapper : MonoBehaviour
    {

        void OnDestroy()
        {
            TNHTweakerLogger.Log("TNHTweaker -- Lootable link was destroyed!", TNHTweakerLogger.LogType.TNH);
        }

    }
}
