namespace UnderMineControl.Loader.Core.FileManagement
{
    public enum FileType
    {
        /// <summary>
        /// Represents a file cached file we downloaded from some where
        /// </summary>
        CachedFile = 0,
        /// <summary>
        /// Represents a config file we handle and maintain
        /// </summary>
        ConfigFile = 1,
        /// <summary>
        /// Represents a file that we're currently working on or modifying
        /// </summary>
        WorkingFile = 2
    }
}
