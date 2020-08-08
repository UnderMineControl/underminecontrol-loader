using System.Net;

namespace UnderMineControl.Loader.Core.FileManagement
{
    public class FileResult
    {
        public string FileName { get; set; }
        public string Path { get; set; }
        public HttpStatusCode Code { get; set; }
        public bool Worked => (int)Code >= 200 && (int)Code < 300;
        public FileCache Cached { get; set; }
    }
}
