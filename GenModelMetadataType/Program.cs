﻿using System;
using System.IO;
using System.Reflection;

namespace GenModelMetadataType
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // 找到 DLL
            var assembly = GetAssembly(@"..\..\..\..\..\ASPNETCore5ModelMetadataType\WebApplication4\bin\Debug\net5.0", "WebApplication4");

            // 讀取 DLL

            // 產生內容

            Console.WriteLine("Hello World!");
        }

        private static Assembly GetAssembly(string path, string name)
        {
            string localPath = string.IsNullOrEmpty(path) ? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) : path;
            string assemblyFilePath = Path.Combine(localPath, $"{name}.dll");
                        
            if (!File.Exists(assemblyFilePath))
            {
                throw new Exception("Missing " + assemblyFilePath);
            }            

            return Assembly.LoadFrom(assemblyFilePath);
        }
    }
}