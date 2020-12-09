using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;

namespace GenModelMetadataType.Services
{
    public class FileService : IFileService
    {
        private static readonly Dictionary<Type, string> typeAlias = new Dictionary<Type, string>
        {
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(object), "object" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(string), "string" },
            { typeof(uint), "uint" },
            { typeof(ulong), "ulong" },
            { typeof(void), "void" }
        };

        private readonly ILogger<FileService> logger;

        public FileService(ILogger<FileService> logger)
        {
            this.logger = logger;
        }

        public void CreatePartialFiles(string path, string name)
        {
            var assembly = GetAssembly(path, name);

            var types = GetEntityTypesFromAssembly(assembly);

            CreateFiles(types);
        }

        #region private function

        /// <summary>
        /// 取得 Assembly
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private Assembly GetAssembly(string path, string name)
        {
            string localPath = string.IsNullOrEmpty(path)
                ? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                : path;
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
        private IEnumerable<Type> GetEntityTypesFromAssembly(Assembly assembly)
        {
            return GetDbContextTypeFromAssembly(assembly)
                .GetProperties()
                .Where(prop => CheckIfDbSetGenericType(prop.PropertyType))
                .Select(type => type.PropertyType.GetGenericArguments()[0]);
        }

        /// <summary>
        /// 新增檔案
        /// </summary>
        /// <param name="types"></param>
        private void CreateFiles(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                var fileName = $"{type.Name}.Partial.cs";
                var fileContent = GeneratePartialCodeContent(type);

                using StreamWriter sw = new StreamWriter($"{Directory.GetCurrentDirectory()}\\{fileName}");
                sw.Write(fileContent);

                logger.LogInformation($"create {fileName}.");
            }
        }

        /// <summary>
        /// 從 Assembly 取得 DbContext 的 Type
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private Type GetDbContextTypeFromAssembly(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes().FirstOrDefault();
            }
            catch (ReflectionTypeLoadException e)
            {
                string dbContextFullName = "Microsoft.EntityFrameworkCore.DbContext";

                var dbContextType = e.Types.Where(t => t.BaseType.FullName.Contains(dbContextFullName));

                var args = Environment.GetCommandLineArgs();
                if (args.Length > 1)
                {
                    var specifyDbContextName = args[1];
                    dbContextType.Where(t => t.FullName.Equals(specifyDbContextName));
                }

                return dbContextType.FirstOrDefault();
            }
        }

        /// <summary>
        /// 檢查是否為 DbSet 的泛型類別
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool CheckIfDbSetGenericType(Type type)
        {
            return type.IsGenericType && GetFullName(type).Contains("DbSet");
        }

        /// <summary>
        /// 取得 Type 的完整名稱
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetFullName(Type type)
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

        /// <summary>
        /// 產生 Partial Code 內容
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GeneratePartialCodeContent(Type type)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("using Microsoft.AspNetCore.Mvc;");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.ComponentModel.DataAnnotations;");
            sb.AppendLine();
            sb.AppendLine("#nullable disable");
            sb.AppendLine();
            sb.AppendLine($"namespace {type.Namespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    [ModelMetadataType(typeof({type.Name}Metadata))]");
            sb.AppendLine($"    public partial class {type.Name}");
            sb.AppendLine("    {");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine($"    internal class {type.Name}Metadata");
            sb.AppendLine("    {");

            foreach (var prop in type.GetProperties())
            {
                if (prop.GetGetMethod().IsVirtual)
                {
                    continue;
                }
                sb.AppendLine("        // [Required]");
                sb.AppendLine($"        public {GetTypeAliasOrName(prop.PropertyType)} {prop.Name} {{ get; set; }}");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        /// <summary>
        /// 取得原始型別的別名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetTypeAliasOrName(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                var t = type.GetGenericArguments()[0];
                return (typeAlias.TryGetValue(t, out string alias) ? alias : t.Name) + "?";
            }
            else
            {
                return typeAlias.TryGetValue(type, out string alias) ? alias : type.Name;
            }
        }

        #endregion private function
    }
}