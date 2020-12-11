// <auto-generated />

using System;
using System.Reflection;
using System.Resources;
using JetBrains.Annotations;

#nullable enable

namespace GenModelMetadataType.Properties
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    internal static class Resources
    {
        private static readonly ResourceManager _resourceManager
            = new ResourceManager("GenModelMetadataType.Properties.Resources", typeof(Resources).Assembly);

        /// <summary>
        ///     Missing {assemblyFilePath}!
        /// </summary>
        public static string AssemblyFileNotFound([CanBeNull] object? assemblyFilePath)
            => string.Format(
                GetString("AssemblyFileNotFound", nameof(assemblyFilePath)),
                assemblyFilePath);

        /// <summary>
        ///     Build failed. Use dotnet build to see the errors.
        /// </summary>
        public static string BuildFailed
            => GetString("BuildFailed");

        /// <summary>
        ///     Build started...
        /// </summary>
        public static string BuildStarted
            => GetString("BuildStarted");

        /// <summary>
        ///     Build succeeded.
        /// </summary>
        public static string BuildSucceeded
            => GetString("BuildSucceeded");

        /// <summary>
        ///     The name of the DbContext class to generate.
        /// </summary>
        public static string ContextOptionDescription
            => GetString("ContextOptionDescription");

        /// <summary>
        ///     Create {file}.
        /// </summary>
        public static string CreateFile([CanBeNull] object? file)
            => string.Format(
                GetString("CreateFile", nameof(file)),
                file);

        /// <summary>
        ///     Unable to retrieve project metadata. Ensure it's an SDK-style project. If you're using a custom BaseIntermediateOutputPath or MSBuildProjectExtensionsPath values, Use the --msbuildprojectextensionspath option.
        /// </summary>
        public static string GetMetadataFailed
            => GetString("GetMetadataFailed");

        /// <summary>
        ///     More than one project was found in the current working directory. Use the --project option.
        /// </summary>
        public static string MultipleProjects
            => GetString("MultipleProjects");

        /// <summary>
        ///     More than one project was found in directory '{projectDir}'. Specify one using its file name.
        /// </summary>
        public static string MultipleProjectsInDirectory([CanBeNull] object? projectDir)
            => string.Format(
                GetString("MultipleProjectsInDirectory", nameof(projectDir)),
                projectDir);

        /// <summary>
        ///     No project was found. Change the current working directory or use the --project option.
        /// </summary>
        public static string NoProject
            => GetString("NoProject");

        /// <summary>
        ///     No project was found in directory '{projectDir}'.
        /// </summary>
        public static string NoProjectInDirectory([CanBeNull] object? projectDir)
            => string.Format(
                GetString("NoProjectInDirectory", nameof(projectDir)),
                projectDir);

        /// <summary>
        ///     The directory to put partial class files in. Paths are relative to the project directory.
        /// </summary>
        public static string OutputOptionDescription
            => GetString("OutputOptionDescription");

        /// <summary>
        ///     Relative path to the project folder of the target project. Default value is the current folder.
        /// </summary>
        public static string ProjectOptionDescription
            => GetString("ProjectOptionDescription");

        /// <summary>
        ///     Unhandled exception!
        /// </summary>
        public static string UnhandledException
            => GetString("UnhandledException");

        /// <summary>
        ///     Worker starting at: {time}.
        /// </summary>
        public static string WorkerStarted([CanBeNull] object? time)
            => string.Format(
                GetString("WorkerStarted", nameof(time)),
                time);

        /// <summary>
        ///     Worker stopped at: {time}.
        /// </summary>
        public static string WorkerStopped([CanBeNull] object? time)
            => string.Format(
                GetString("WorkerStopped", nameof(time)),
                time);

        private static string GetString(string name, params string[] formatterNames)
        {
            var value = _resourceManager.GetString(name)!;
            for (var i = 0; i < formatterNames.Length; i++)
            {
                value = value.Replace("{" + formatterNames[i] + "}", "{" + i + "}");
            }

            return value;
        }
    }
}

