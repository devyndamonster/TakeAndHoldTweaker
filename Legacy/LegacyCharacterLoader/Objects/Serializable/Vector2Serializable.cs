using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Valve.Newtonsoft.Json;

namespace LegacyCharacterLoader.Objects.Serializable
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
}
