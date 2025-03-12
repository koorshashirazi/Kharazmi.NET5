using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Extensions;
using Kharazmi.Functional;
using Kharazmi.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Kharazmi.Configuration
{
    internal class JsonBackupSettings : IJsonBackupSettings
    {
        private readonly ILogger<JsonBackupSettings>? _logger;
        private static readonly BackupSettings BackupSettings = new();

        public JsonBackupSettings(
            [AllowNull] ILogger<JsonBackupSettings>? logger,
            IHostEnvironment evn)
        {
            _logger = logger;
            _rootPath = evn.ContentRootPath;
        }

        private readonly string _rootPath;

        public event Action<string>? OnBackup;
        public event Action<string>? OnRestored;
        public event Action<string>? OnDeleted;

        public void TryAddProvider<TSettings>(string providerName) where TSettings : class, ISettings
            => BackupSettings.AddProvider(providerName, typeof(TSettings));

        public BackupSettings GetBackupSettings()
        {
            try
            {
                if (BackupSettings.BackupSourceFiles.Any() && !BackupSettings.IsChanged)
                    return BackupSettings;

                foreach (var providerName in BackupSettings.ProviderNames)
                {
                    var fileInfos = new DirectoryInfo(_rootPath).GetFiles("*.json", SearchOption.TopDirectoryOnly)
                        .Where(x => x.Name.EndsWith(providerName))
                        .ToList();

                    var backupSourceFile =
                        BackupSettings.BackupSourceFiles.FirstOrDefault(x => x.Key == providerName).Value ??
                        new BackupSourceFile(providerName); // Initialize new instance

                    backupSourceFile.AddOrUpdateFileInfos(fileInfos);

                    BackupSettings.AddBackupSourceFile(backupSourceFile);
                }

                BackupSettings.SetAnyChanged();

                return BackupSettings;
            }
            catch (Exception e)
            {
                _logger?.LogError("{Message}", e.Message);
                return new BackupSettings();
            }
        }

        public async Task<Result<BackupSettings>> BackupSettingsAsync(BackupModel? backupModel,
            CancellationToken token = default)
        {
            try
            {
                if (backupModel is null) return Result.FailAs<BackupSettings>("Invalid filename");
                var providerName = backupModel.ProviderName;

                if (providerName.IsEmpty())
                {
                    _logger?.LogError("Invalid config source path with source path {SourcePath}", providerName);
                    return Result.FailAs<BackupSettings>($"Invalid config source path  with source path {providerName}",
                        "BackupSettings");
                }

                var pathFile = Path.Combine(_rootPath, providerName);
                var jObject = await CreateJObjectOfAsync(pathFile, token);

                if (jObject is null)
                    return Result.FailAs<BackupSettings>($"Invalid json source path with path {providerName}");

                var settings = jObject.Serialize(Settings.GetJsonSettings);

                if (settings.IsEmpty())
                    return Result.FailAs<BackupSettings>($"Invalid json object with source path {providerName}");


                var sourceFileBak =$"{Guid.NewGuid():N}_{providerName}";
                var backupPath = Path.Combine(_rootPath, sourceFileBak);

                await File.WriteAllTextAsync(backupPath, settings, token).ConfigureAwait(false);
                BackupSettings.SetChanged();
                OnBackup?.Invoke(providerName);

                return Result.OkAs(GetBackupSettings());
            }
            catch (Exception e)
            {
                _logger?.LogError("{Message}", e.Message);
                return Result.FailAs<BackupSettings>("Backup settings is failed");
            }
        }

        public Result<BackupSettings> DeleteBackup(BackupModel? backupModel, CancellationToken token = default)
        {
            try
            {
                if (backupModel is null) return Result.FailAs<BackupSettings>("Invalid filename");

                var providerName = backupModel.ProviderName;
                if (providerName.IsEmpty())
                    return Result.FailAs<BackupSettings>($"Invalid source path {providerName}");

                var fileNames = backupModel.FileNames;
                if (fileNames is null || fileNames.Length == 0)
                    return Result.FailAs<BackupSettings>("Invalid filename");

                var backupSettings = GetBackupSettings();
                var backupSetting = backupSettings.BackupSourceFiles
                    .FirstOrDefault(x => x.Key == providerName);

                var backupSource = backupSetting.Value;
                if (backupSource is null)
                    return Result.FailAs<BackupSettings>(
                        $"Invalid source file path with source provider {providerName}");

                var sourceFileNames = fileNames.Where(x => x != backupSource.Default).ToList();

                foreach (var fileName in sourceFileNames)
                {
                    var file = backupSource.FileInfos.FirstOrDefault(x => x.Name == fileName);
                    if (file is null) continue;
                    if (file.Exists) file.Delete();
                    backupSource.RemoveFileInfo(file);
                }

                BackupSettings.SetChanged();
                OnDeleted?.Invoke(providerName);
                return Result.OkAs(BackupSettings);
            }
            catch (Exception e)
            {
                _logger?.LogError("{Message}", e.Message);
                return Result.FailAs<BackupSettings>("Delete backup settings is failed");
            }
        }

        public async Task<Result<BackupSettings>> RestoreSettingsAsync(BackupModel? backupModel,
            CancellationToken token = default)
        {
            try
            {
                if (backupModel is null) return Result.FailAs<BackupSettings>("Invalid filename");

                var providerName = backupModel.ProviderName;
                if (providerName.IsEmpty()) return Result.FailAs<BackupSettings>($"Invalid source path {providerName}");

                var fileName = backupModel.FileNames is not null ? backupModel.FileNames[0] : "";
                if (fileName.IsEmpty()) return Result.FailAs<BackupSettings>("Invalid filename");

                var backupSettings = GetBackupSettings();
                var backupSetting = backupSettings.BackupSourceFiles
                    .FirstOrDefault(x => x.Key == providerName);

                var backupSource = backupSetting.Value;
                if (backupSource is null)
                    return Result.FailAs<BackupSettings>(
                        $"Invalid source file path with source provider {providerName}");

                if (fileName == backupSource.Default)
                    return Result.OkAs<BackupSettings>($"Settings is set with provider source {providerName}");

                var file = backupSource.FileInfos.FirstOrDefault(x => x.Name == fileName);
                if (file is null)
                    return Result.FailAs<BackupSettings>(
                        $"Invalid source file path with source provider {providerName}");

                if (!file.Exists)
                    return Result.FailAs<BackupSettings>(
                        $"Invalid source file path with source provider {providerName}");


                var result = await WriteJsonFile(providerName, file.FullName, token);
                if (result.Failed)
                    return Result.MapToFail<BackupSettings>(result);

                if (file.Exists) file.Delete();

                backupSource.RemoveFileInfo(file);

                BackupSettings.SetChanged();
                OnRestored?.Invoke(providerName);
                return Result.OkAs(BackupSettings);
            }
            catch (Exception e)
            {
                _logger?.LogError("{Message}", e.Message);
                return Result.FailAs<BackupSettings>("Restore backup settings is failed");
            }
        }

        public Result<string> GetProviderPath(Type settingType)
        {
            BackupSettings.ProviderTypes.TryGetValue(settingType, out var sourceName);

            if (sourceName.IsEmpty())
            {
                _logger?.LogError("Invalid config source path with type {Settings} and source path {SourcePath}",
                    settingType.Name, sourceName);
                return Result.FailAs<string>("Invalid config source path");
            }

            return Result.OkAs(Path.Combine(_rootPath, sourceName));
        }

        public Result<string> GetProviderPath<TSettings>() where TSettings : class, ISettings
        {
            BackupSettings.ProviderTypes.TryGetValue(typeof(TSettings), out var sourceName);

            return sourceName.IsEmpty()
                ? Result.FailAs<string>("Invalid config source path")
                : Result.OkAs(Path.Combine(_rootPath, sourceName));
        }


        private async Task<JObject?> CreateJObjectOfAsync(string configSourcePath,
            CancellationToken token = default)
        {
            try
            {
                if (!File.Exists(configSourcePath)) return default;
                var currentJsonFile = await File.ReadAllTextAsync(configSourcePath, token);

                return currentJsonFile.IsNotEmpty()
                    ? currentJsonFile.Deserialize<JObject>(Settings.GetJsonSettings)
                    : default;
            }
            catch (Exception e)
            {
                _logger?.LogError("{Message}", e.Message);
                return default;
            }
        }


        private async Task<Result> WriteJsonFile(string providerName, string fromProviderName,
            CancellationToken token = default)
        {
            var jObject = await CreateJObjectOfAsync(fromProviderName, token);
            if (jObject.IsNull()) return Result.Fail($"Can't rewrite provider with name, {providerName}");

            var sourcePath = Path.Combine(_rootPath, providerName);

            await File.WriteAllTextAsync(sourcePath, jObject.Serialize(Settings.GetJsonSettings), token);

            _logger?.LogTrace(
                "ReWrite settings with provider name {SourceType}", providerName);

            return Result.Ok($"ReWrite settings with provider name {providerName}");
        }
    }
}