using System.Collections.Generic;

namespace UnderMineControl.Loader.Core.Models
{
    public class ModRegistry
    {
        public List<Mod> Mods { get; set; }

        public string FileVersion { get; set; }

        public string UpdateUrl { get; set; }
    }
}
