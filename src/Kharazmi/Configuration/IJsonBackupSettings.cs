using System;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Functional;

namespace Kharazmi.Configuration
{
    public interface IJsonBackupSettings
    {
        event Action<string>? OnBackup;
        event Action<string>? OnRestored;
        event Action<string>? OnDeleted;
        void TryAddProvider<TSettings>(string providerName) where TSettings : class, ISettings;

        BackupSettings GetBackupSettings();
        Task<Result<BackupSettings>> BackupSettingsAsync(BackupModel? backupModel, CancellationToken token = default);
        Result<BackupSettings> DeleteBackup(BackupModel? backupModel, CancellationToken token = default);
        Task<Result<BackupSettings>> RestoreSettingsAsync(BackupModel? backupModel, CancellationToken token = default);

        Result<string> GetProviderPath(Type settingType);
        Result<string> GetProviderPath<TSettings>() where TSettings : class, ISettings;
    }
}