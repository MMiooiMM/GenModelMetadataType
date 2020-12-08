using GenModelMetadataType.Services;

namespace GenModelMetadataType
{
    public class App
    {
        private readonly IPathService pathService;
        private readonly IFileService fileService;

        public App(IPathService pathService, IFileService fileService)
        {
            this.pathService = pathService;
            this.fileService = fileService;
        }

        public void Run()
        {
            var (path, name) = pathService.GetAssemblyPathInfo();

            fileService.CreatePartialFiles(path, name);
        }
    }
}