using Deli.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TNHTweaker.ObjectTemplates
{
    public class Vector2Serializable
    {
        public float x;
        public float y;

        [JsonIgnore]
        private Vector2 v;

        public Vector2Serializable() { }

        public Vector2Serializable(Vector2 v)
        {
            x = v.x;
            y = v.y;
            this.v = v;
        }

        public Vector2 GetVector2()
        {
            v = new Vector2(x, y);
            return v;
        }
    }

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
