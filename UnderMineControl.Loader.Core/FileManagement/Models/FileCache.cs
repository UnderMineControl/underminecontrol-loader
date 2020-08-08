using System;

namespace UnderMineControl.Loader.Core.FileManagement
{
    public class FileCache
    {
        public string Guid { get; set; }
        public string WebLocation { get; set; }
        public string OriginalName { get; set; }
        public DateTime Created { get; set; }
    }
}
