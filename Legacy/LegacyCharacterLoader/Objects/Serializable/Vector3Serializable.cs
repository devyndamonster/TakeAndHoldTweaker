using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Valve.Newtonsoft.Json;

namespace LegacyCharacterLoader.Objects.Serializable
{
    public class Vector3Serializable
    {
        public float x;
        public float y;
        public float z;

        [JsonIgnore]
        private Vector3 v;

        public Vector3Serializable() { }

        public Vector3Serializable(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
            this.v = v;
        }

        public Vector3 GetVector3()
        {
            v = new Vector3(x, y, z);
            return v;
        }
    }
}
