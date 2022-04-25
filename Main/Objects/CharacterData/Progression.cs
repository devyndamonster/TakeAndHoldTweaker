using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TNHTweaker.Objects.CharacterData
{

    [CreateAssetMenu(menuName = "TNHTweaker/Progression", fileName = "NewProgression")]
    public class Progression : ScriptableObject
    {
        public List<Level> Levels = new List<Level>();
    }
}
