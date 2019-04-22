﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TypeGen.Cli.Models;
using TypeGen.Core.Business;
using TypeGen.Core.Utils;
using TypeGen.Core.Storage;
using TypeGen.Core.Validation;

namespace TypeGen.Cli.Business
{
    internal class ConfigProvider : IConfigProvider
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILogger _logger;

        public ConfigProvider(IFileSystem fileSystem,
            ILogger logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;
        }

        /// <summary>
        /// Creates a config object from a given config file
        /// </summary>
        /// <param name="configPath"></param>
        /// <param name="projectFolder"></param>
        /// <returns></returns>
        public TgConfig GetConfig(string configPath, string projectFolder)
        {
            Requires.NotNullOrEmpty(configPath, nameof(configPath));
            Requires.NotNullOrEmpty(projectFolder, nameof(projectFolder));
            
            if (!_fileSystem.FileExists(configPath))
            {
                if (_logger.LogVerbose) _logger.Log($"No config file found for project \"{projectFolder}\". Default configuration will be used.");

                TgConfig defaultConfig = new TgConfig()
                    .MergeWithDefaultParams()
                    .Normalize();

                UpdateConfigAssemblyPaths(defaultConfig, projectFolder);
                return defaultConfig;
            }

            if (_logger.LogVerbose) _logger.Log($"Reading the config file from \"{configPath}\"");

            string tgConfigJson = _fileSystem.ReadFile(configPath);
            TgConfig config = JsonConvert.DeserializeObject<TgConfig>(tgConfigJson)
                .MergeWithDefaultParams()
                .Normalize();

            UpdateConfigAssemblyPaths(config, projectFolder);

            return config;
        }

        private void UpdateConfigAssemblyPaths(TgConfig config, string projectFolder)
        {
            if (!string.IsNullOrEmpty(config.AssemblyPath))
            {
                config.AssemblyPath = GetAssemblyPath(config.AssemblyPath, projectFolder);
                _logger.Log("WARNING: assemblyPath config parameter is deprecated and can be removed in future versions. Please use 'assemblies' instead.");
            }

            config.Assemblies = config.Assemblies.Select(a => GetAssemblyPath(a, projectFolder)).ToArray();

            if (!config.Assemblies.Any())
            {
                config.Assemblies = new[] { GetAssemblyPath(null, projectFolder) };
            }
        }

        private string GetAssemblyPath(string configAssemblyPath, string projectFolder)
        {
            if (string.IsNullOrEmpty(configAssemblyPath))
            {
                if (_logger.LogVerbose) _logger.Log("Assembly path not found in the config file. Assembly file will be searched for recursively in the project's bin\\.");
                return GetDefaultAssemblyPath(projectFolder);
            }

            if (_logger.LogVerbose) _logger.Log($"Reading assembly path from the config file: '{configAssemblyPath}'");
            string assemblyPath = Path.Combine(projectFolder, configAssemblyPath);

            if (!_fileSystem.FileExists(assemblyPath))
            {
                throw new CliException($"The specified assembly: '{configAssemblyPath}' not found in the project folder");
            }

            return assemblyPath;
        }

        private string GetDefaultAssemblyPath(string projectFolder)
        {
            string projectFileName = _fileSystem.GetDirectoryFiles(projectFolder)
                .Select(FileSystemUtils.GetFileNameFromPath)
                .FirstOrDefault(n => n.EndsWith(".csproj") || n.EndsWith(".xproj"));

            if (projectFileName == null)
            {
                throw new CliException("Project file (*.csproj or *.xproj) not found in the project folder and no assembly path found in the config file");
            }

            string dllFileName = Path.ChangeExtension(projectFileName, "dll");
            string exeFileName = Path.ChangeExtension(projectFileName, "exe");
            string binPath = Path.Combine(projectFolder, "bin");

            IEnumerable<string> foundFiles = _fileSystem.GetFilesRecursive(binPath, dllFileName)
                .Concat(_fileSystem.GetFilesRecursive(binPath, exeFileName));

            if (foundFiles.Any())
            {
                string foundFile = foundFiles.First();
                if (_logger.LogVerbose) _logger.Log($"Using project assembly found in: {foundFile}");
                return foundFile;
            }

            throw new CliException($"None of: '{dllFileName}' or '{exeFileName}' found in the default assembly folder (the project's bin\\ folder; searched recursively). Please make sure your project is built.");
        }
    }
}
