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

        private static void Main(string[] args)
        {
            var assembly = GetAssembly(@"..\..\..\..\..\ASPNETCore5ModelMetadataType\WebApplication4\bin\Debug\net5.0", "WebApplication4");

            var types = GetEntityTypesFromAssembly(assembly);

            CreateFiles(types);

            Console.WriteLine("Done.");
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

        /// <summary>
        /// 新增檔案
        /// </summary>
        /// <param name="types"></param>
        private static void CreateFiles(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                var fileName = $"{type.Name}.Partial.cs";
                var fileContent = GeneratePartialCodeContent(type);

                using StreamWriter sw = new StreamWriter($"{Environment.CurrentDirectory}\\{fileName}");
                sw.Write(fileContent);
            }
        }

        /// <summary>
        /// 產生 Partial Code 內容
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string GeneratePartialCodeContent(Type type)
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
        private static string GetTypeAliasOrName(Type type)
        {
            return typeAlias.TryGetValue(type, out string alias) ? alias : type.Name;
        }
    }
}