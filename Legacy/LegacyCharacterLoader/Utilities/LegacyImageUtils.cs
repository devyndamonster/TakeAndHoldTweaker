using Deli.VFS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using TNHTweaker.Utilities;
using System.IO;

namespace LegacyCharacterLoader.Utilities
{
    public static class LegacyImageUtils
    {
        public static Sprite LoadSpriteFromFileHandle(IFileHandle file)
        {
            Texture2D spriteTexture = LoadTextureFromFileHandle(file);
            return ImageUtils.LoadSpriteFromTexture(spriteTexture);
        }

        public static Texture2D LoadTextureFromFileHandle(IFileHandle file)
        {
            Stream fileStream = file.OpenRead();
            MemoryStream mem = new MemoryStream();

            CopyStream(fileStream, mem);
            byte[] fileData = mem.ToArray();

            Texture2D tex2D = new Texture2D(2, 2);
            tex2D.LoadImage(fileData);

            return tex2D;
        }

        private static void CopyStream(Stream input, Stream output)
        {
            byte[] b = new byte[32768];
            int r;
            while ((r = input.Read(b, 0, b.Length)) > 0)
                output.Write(b, 0, r);
        }
    }
}
