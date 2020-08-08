namespace UnderMineControl.Loader.Core.Models
{
    public class Mod
    {
        public string Guid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Url { get; set; }
        public string GameVersion { get; set; }
        public ModType Type { get; set; }
        public string ContactMethod { get; set; }

        public Dependency InstallSettings { get; set; }
    }
}
