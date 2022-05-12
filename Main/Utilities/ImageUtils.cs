using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TNHTweaker.Utilities
{
    public static class ImageUtils
    {
        public static Sprite LoadSpriteFromPath(string filePath)
        {
            Texture2D spriteTexture = LoadTextureFromPath(filePath);
            Sprite sprite = LoadSpriteFromTexture(spriteTexture);
            sprite.name = Path.GetFileNameWithoutExtension(filePath);
            return sprite;
        }

        public static Sprite LoadSpriteFromTexture(Texture2D spriteTexture, float pixelsPerUnit = 100f)
        {
            return Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height), new Vector2(0, 0), pixelsPerUnit);
        }

        public static Texture2D LoadTextureFromPath(string filePath)
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);

            return tex;
        }

    }
}
