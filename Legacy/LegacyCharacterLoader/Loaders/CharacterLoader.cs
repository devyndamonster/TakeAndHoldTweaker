using Deli;
using Deli.Setup;
using Deli.VFS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegacyCharacterLoader.Loaders
{
    public class CharacterLoader
    {
        public void LoadAsset(SetupStage stage, Mod mod, IHandle handle)
        {
            if (handle is not IFileHandle file)
            {
                throw new ArgumentException("Could not load character! Character should point to a folder holding the character.json and thumb.png");
            }


        }
    }
}
