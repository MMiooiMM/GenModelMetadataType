using GenModelMetadataType.Services;

namespace GenModelMetadataType
{
    public class App
    {
        private readonly IPathService pathService;
        private readonly IFileService fileService;
        private readonly string dbContextName;

        public App(IPathService pathService, IFileService fileService, string dbContextName)
        {
            this.pathService = pathService;
            this.fileService = fileService;
            this.dbContextName = dbContextName;
        }

        public void Run()
        {
            var (path, name) = pathService.GetAssemblyPathInfo();

            var assembly = fileService.GetAssembly(path, name);

            var types = fileService.GetEntityTypesFromAssembly(assembly, dbContextName);

            fileService.CreateFiles(types);
        }
    }
}