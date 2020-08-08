namespace UnderMineControl.Loader.Core.Models
{
    public class Dependency
    {
        public string Owner { get; set; }
        public string Repo { get; set; }
        public string Version { get; set; }
        public string[] Files { get; set; }
    }
}
