namespace GenModelMetadataType.Services
{
    public interface IFileService
    {
        /// <summary>
        /// 產生 Partial 程式碼檔案
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        void CreatePartialFiles(string path, string name);
    }
}