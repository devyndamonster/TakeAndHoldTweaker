using Deli;
using Deli.Setup;
using Deli.VFS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegacyCharacterLoader.Loaders
{
    public class SosigLoader
    {
        public void LoadAsset(SetupStage stage, Mod mod, IHandle handle)
        {
            if (handle is not IFileHandle file)
            {
                throw new ArgumentException("Could not load sosig! Make sure you're pointing to a sosig template json file in the manifest");
            }
        }
    }
}
