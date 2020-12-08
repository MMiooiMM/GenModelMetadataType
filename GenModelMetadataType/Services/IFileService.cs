using System;
using System.Collections.Generic;
using System.Reflection;

namespace GenModelMetadataType.Services
{
    public interface IFileService
    {
        /// <summary>
        /// 取得 Assembly
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public Assembly GetAssembly(string path, string name);

        /// <summary>
        /// 從 Assembly 取得所有 Entity 的 Type
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public IEnumerable<Type> GetEntityTypesFromAssembly(Assembly assembly, string dbContextName);

        /// <summary>
        /// 新增檔案
        /// </summary>
        /// <param name="types"></param>
        public void CreateFiles(IEnumerable<Type> types);
    }
}