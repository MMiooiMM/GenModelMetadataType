using System.IO;
using System.Linq;

namespace GenModelMetadataType.Services
{
    public class PathService : IPathService
    {
        public (string path, string name) GetAssemblyPathInfo()
        {
            string path = GetProjectRootDirectory();

            return (GenerateBinDirectory(path), GetLastPath(path));
        }

        #region private function

        /// <summary>
        /// 取得專案根目錄的完整路徑
        /// </summary>
        /// <returns></returns>
        private string GetProjectRootDirectory()
        {
            var path = Directory.GetCurrentDirectory();

            while (!CheckIfContainBinDirectory(path) || !CheckIfContainCsprojFile(path))
            {
                path = GetUpperDirectory(path);
            }

            return path;
        }

        /// <summary>
        /// 取得路徑中上一層資料夾的完整路徑
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string GetUpperDirectory(string path)
        {
            return string.Join('\\', path.Split('\\')[0..^1]);
        }

        /// <summary>
        /// 檢查是否包含 bin 資料夾
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool CheckIfContainBinDirectory(string path)
        {
            return Directory.GetDirectories(path).Any(d => GetLastPath(d) == "bin");
        }

        /// <summary>
        /// 檢查是否包含 Csproj 檔案
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool CheckIfContainCsprojFile(string path)
        {
            return Directory.GetFiles(path).Any(d => d.Contains(".csproj"));
        }

        /// <summary>
        /// 產生 bin 內容資料夾
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string GenerateBinDirectory(string path)
        {
            return $"{path}\\bin\\Debug\\net5.0";
        }

        /// <summary>
        /// 取得路徑中最後一個資料夾名稱
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string GetLastPath(string path)
        {
            return path.Split('\\')[^1];
        }

        #endregion
    }
}