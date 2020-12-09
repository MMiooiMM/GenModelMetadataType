namespace GenModelMetadataType.Services
{
    public interface IPathService
    {
        /// <summary>
        /// 取得 Assembly 檔案訊息
        /// </summary>
        /// <returns></returns>
        public (string path, string name) GetAssemblyFileInfo();
    }
}