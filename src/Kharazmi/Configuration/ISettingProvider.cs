using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Functional;
using Kharazmi.Options;
using Microsoft.Extensions.Configuration;

namespace Kharazmi.Configuration
{
    public interface ISettingProvider
    {
        IConfigurationRoot Configuration { get; }
        event OptionsHandler<IOptions> UpdatedOptionHandler;
        IFrameworkSettings FrameworkSettings { get; }
        IAppSettings AppSettings { get; }
        TSettings? As<TSettings>() where TSettings : class, ISettings => AppSettings as TSettings;
        Microsoft.Extensions.Options.IOptions<T> GetOptions<T>() where T: class;
        Microsoft.Extensions.Options.IOptionsMonitor<T> GetOptionsMonitor<T>() where T: class;
        Microsoft.Extensions.Options.IOptionsSnapshot<T> GetOptionsSnapshot<T>() where T: class;
        T Get<T>() where T : class, IOptions;
        void AddOption<T>(T option) where T : class, IOptions;
        void UpdateOption<T>(Action<T> option) where T : class, IOptions;
        void UpdateOption<T>(T options) where T : class, IOptions;
        void ReloadSettings();
        Result SaveChanges();
        Task<Result> SaveChangesAsync(CancellationToken token = default);
        Result SaveChanges<TSettings>(Action<TSettings> settings) where TSettings : class, ISettings;

        Task<Result> SaveChangesAsync<TSettings>(Action<TSettings> settings, CancellationToken token = default)
            where TSettings : class, ISettings;

        IEnumerable<RawSetting> GetRawSettings();
        Task<Result> SaveRawSettingsAsync(RawSetting? rawSetting, CancellationToken token = default);
    }
}