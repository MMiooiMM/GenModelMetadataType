using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GenModelMetadataType
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // 找到 DLL
            var assembly = GetAssembly(@"..\..\..\..\..\ASPNETCore5ModelMetadataType\WebApplication4\bin\Debug\net5.0", "WebApplication4");

            // 讀取 DLL 並取得 entity
            var types = GetEntityTypesFromAssembly(assembly);

            // 產生內容
            Console.WriteLine("Hello World!");
        }

        /// <summary>
        /// 取得 Assembly
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static Assembly GetAssembly(string path, string name)
        {
            string localPath = string.IsNullOrEmpty(path) ? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) : path;
            string assemblyFilePath = Path.Combine(localPath, $"{name}.dll");

            if (!File.Exists(assemblyFilePath))
            {
                throw new Exception($"Missing {assemblyFilePath}");
            }

            return Assembly.LoadFrom(assemblyFilePath);
        }

        /// <summary>
        /// 從 Assembly 取得所有 Entity 的 Type
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private static IEnumerable<Type> GetEntityTypesFromAssembly(Assembly assembly)
        {
            return GetDbContextTypeFromAssembly(assembly)
                .GetProperties()
                .Where(prop => CheckIfDbSetGenericType(prop.PropertyType))
                .Select(type => type.PropertyType.GetGenericArguments()[0]);
        }

        /// <summary>
        /// 從 Assembly 取得 DbContext 的 Type
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private static Type GetDbContextTypeFromAssembly(Assembly assembly)
        {
            string dbContextFullName = "Microsoft.EntityFrameworkCore.DbContext";

            try
            {
                return assembly.GetTypes().FirstOrDefault();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.FirstOrDefault(t => t.BaseType.FullName.Contains(dbContextFullName) && t.IsPublic);
            }
        }

        /// <summary>
        /// 檢查是否為 DbSet 的泛型類別
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool CheckIfDbSetGenericType(Type type)
        {
            return type.IsGenericType && GetFullName(type).Contains("DbSet");
        }

        /// <summary>
        /// 取得 Type 的完整名稱
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string GetFullName(Type type)
        {
            if (!type.IsGenericType) return type.Name;

            StringBuilder sb = new StringBuilder();

            sb.Append(type.Name.Substring(0, type.Name.LastIndexOf("`")));
            sb.Append(type.GetGenericArguments().Aggregate("<",
                delegate (string aggregate, Type type)
                {
                    return aggregate + (aggregate == "<" ? "" : ",") + GetFullName(type);
                }));
            sb.Append(">");

            return sb.ToString();
        }       
    }
}