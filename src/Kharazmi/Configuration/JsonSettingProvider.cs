using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.Functional;
using Kharazmi.Json;
using Kharazmi.Options;
using Kharazmi.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

namespace Kharazmi.Configuration
{
    public delegate void OptionsHandler<in TOptions>(ISettingProvider sender, TOptions options, Type optionType);

    internal class JsonSettingProvider : ISettingProvider
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<JsonSettingProvider>? _logger;
        private readonly IJsonBackupSettings _backupSettings;
        private readonly IHostEnvironment _evn;
        private readonly object _lock = new();

        // ReSharper disable once InconsistentNaming
        private readonly FrameworkSettings _FrameworkSettings;

        // ReSharper disable once InconsistentNaming
        private readonly AppSettings _AppSettings;

        public JsonSettingProvider(
            IServiceProvider sp,
            IConfigurationRoot configs,
            [AllowNull] ILogger<JsonSettingProvider>? logger,
            IJsonBackupSettings backupSettings,
            IHostEnvironment evn)
        {
            Configuration = configs;
            _FrameworkSettings = SettingsContainer.Instance.GetFrameworkSettings();
            _AppSettings = SettingsContainer.Instance.GetAppSettings();
            _sp = sp;
            _logger = logger;
            _backupSettings = backupSettings;
            _evn = evn;

            _backupSettings.OnRestored += OnRestored;
        }

        public event OptionsHandler<IOptions>? UpdatedOptionHandler;

        public IConfigurationRoot Configuration { get; }

        public IFrameworkSettings FrameworkSettings => _FrameworkSettings;

        public IAppSettings AppSettings => _AppSettings;

        public IOptions<T> GetOptions<T>() where T : class
            => _sp.GetRequiredService<IOptions<T>>();

        public IOptionsMonitor<T> GetOptionsMonitor<T>() where T : class
            => _sp.GetRequiredService<IOptionsMonitor<T>>();

        public IOptionsSnapshot<T> GetOptionsSnapshot<T>() where T : class
            => _sp.GetRequiredService<IOptionsSnapshot<T>>();

        internal JsonSettingProvider Init(string pathJsonFile)
        {
            CreateFrameworkJsonFile();
            _backupSettings.TryAddProvider<FrameworkSettings>(FrameworkConstants.JsonFileName);
            _backupSettings.TryAddProvider<AppSettings>(pathJsonFile);
            return this;
        }

        private void CreateFrameworkJsonFile()
        {
            var result = _backupSettings.GetProviderPath<FrameworkSettings>();
            if (result.Failed) return;

            var jsonFile = result.Value;
            if (jsonFile.IsEmpty()) return;

            if (File.Exists(jsonFile)) return;
            _logger?.LogWarning("Json file {JsonFile} not exists", jsonFile);

            JObject jObject = JObject.FromObject(new {FrameworkSettings = new FrameworkSettings()});

            File.WriteAllText(jsonFile, jObject.Serialize());
            _logger?.LogTrace("Create json file {JsonFile}", jsonFile);
        }

        public T Get<T>() where T : class, IOptions
        {
            try
            {
                Monitor.Enter(_lock);
                if (typeof(IConfigurePluginOption).IsAssignableFrom(typeof(T)))
                {
                    var option = _FrameworkSettings.TryGetOption<T>();
                    if (option is not null && option.Enable) return option;

                    option = GetOrCreateOption<T>();
                    AddOption(option);
                    return option;
                }

                var appOption = _AppSettings.TryGetOption<T>();
                if (appOption is not null) return appOption;

                appOption = GetOrCreateOption<T>();
                AddOption(appOption);
                return appOption;
            }
            catch (Exception e)
            {
                _logger.LogError("{Message}", e.Message);
                return typeof(T).CreateInstance<T>();
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        public IEnumerable<RawSetting> GetRawSettings()
        {
            ReLoadSettings(_AppSettings.GetType());
            ReLoadSettings(_FrameworkSettings.GetType());

            yield return new RawSetting
            {
                JsonData = _AppSettings.Serialize(Settings.GetJsonSettings),
                ProviderName = _backupSettings.GetBackupSettings().ProviderTypes
                    .FirstOrDefault(x => x.Key == _AppSettings.GetType()).Value
            };

            yield return new RawSetting
            {
                JsonData = FrameworkSettings.Serialize(Settings.GetJsonSettings),
                ProviderName = _backupSettings.GetBackupSettings().ProviderTypes
                    .FirstOrDefault(x => x.Key == typeof(FrameworkSettings)).Value
            };
        }

        public void AddOption<T>(T option) where T : class, IOptions
        {
            try
            {
                Monitor.Enter(_lock);
                if (typeof(IConfigurePluginOption).IsAssignableFrom(typeof(T)))
                {
                    _FrameworkSettings.AddOption(option);
                    return;
                }

                if (typeof(IAppSettings).IsAssignableFrom(typeof(T)))
                    _AppSettings.AddOption(option);
            }
            catch (Exception e)
            {
                _logger.LogError("{Message}", e.Message);
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        public void UpdateOption<T>(Action<T> option) where T : class, IOptions
        {
            try
            {
                Monitor.Enter(_lock);
                var options = Get<T>();
                option.Invoke(options);
                UpdateOption(options);
            }
            catch (Exception e)
            {
                _logger.LogError("{Message}", e.Message);
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        public void UpdateOption<T>(T options) where T : class, IOptions
        {
            try
            {
                Monitor.Enter(_lock);
                if (typeof(IConfigurePluginOption).IsAssignableFrom(typeof(T)))
                {
                    _FrameworkSettings.UpdateOption(options);
                    OnUpdatedOptions(options);
                    ClearOptionsChanges(options);
                    return;
                }

                _AppSettings.UpdateOption(options);
                OnUpdatedOptions(options);
                ClearOptionsChanges(options);
            }
            catch (Exception e)
            {
                _logger.LogError("{Message}", e.Message);
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        public void ReloadSettings()
        {
            Configuration.Reload();

            // var hooks = _sp.GetServices<IConfigurePlugin>();
            //
            // foreach (var hook in hooks)
            //     hook.Configure(this);
        }

        public Result SaveChanges()
            => AsyncHelper.RunSync(() => SaveChangesAsync());

        public async Task<Result> SaveChangesAsync(CancellationToken token = default)
        {
            try
            {
                Result result1 = Result.Ok();
                Result result2 = Result.Ok();
                if (_FrameworkSettings.IsDirty())
                {
                    result1 = await ReWriteSettingsAsync(_FrameworkSettings, token);
                    _FrameworkSettings.Clear();
                    _FrameworkSettings.MakeDirty(false);
                }

                if (_AppSettings.IsDirty())
                {
                    result2 = await ReWriteSettingsAsync(_AppSettings, token);
                    _AppSettings.Clear();
                    _AppSettings.MakeDirty(false);
                }

                Configuration.Reload();

                await Task.Delay(TimeSpan.FromSeconds(1), token);

                var result = Result.Combine(result1, result2);
                if (result.Failed)
                    _logger.LogError(MessageTemplate.SaveSettingFailed, MessageEventName.SettingsProvider,
                        nameof(JsonSettingProvider), result.Description);
                else
                    _logger.LogTrace(MessageTemplate.SaveSettingSucceed, MessageEventName.SettingsProvider,
                        nameof(JsonSettingProvider));
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError("{Message}", e.Message);
                return Result.Fail("Unable to save changes");
            }
        }

        public Result SaveChanges<TSettings>(Action<TSettings> settings) where TSettings : class, ISettings
            => AsyncHelper.RunSync(() => SaveChangesAsync(settings));

        public async Task<Result> SaveChangesAsync<TSettings>(Action<TSettings> setup,
            CancellationToken token = default) where TSettings : class, ISettings
        {
            if (typeof(IFrameworkSettings).IsAssignableFrom(typeof(TSettings)))
            {
                var settings = _FrameworkSettings;
                setup.Invoke((TSettings) (settings as IFrameworkSettings));
                UpdateFrameworkSettings(settings);
                return await SaveChangesAsync(token);
            }

            var appSettings = _AppSettings;
            setup.Invoke((TSettings) (appSettings as ISettings));
            UpdateAppSettings(appSettings);
            return await SaveChangesAsync(token);
        }

        public async Task<Result> SaveRawSettingsAsync(RawSetting? rawSetting, CancellationToken token = default)
        {
            if (rawSetting is null) return Result.Fail("Update app settings failed");

            var providerName = rawSetting.ProviderName;
            if (providerName.IsEmpty())
                return Result.Fail("Update app settings failed");

            var json = rawSetting.JsonData;
            if (json.IsEmpty()) return Result.Fail("Update app settings failed");

            try
            {
                var bkSettings = _backupSettings.GetBackupSettings();
                var exist = bkSettings.ProviderKeys.TryGetValue(providerName, out var settingsType);

                if (exist && typeof(IFrameworkSettings).IsAssignableFrom(settingsType))
                {
                    var frameworkSettings = json.Deserialize<FrameworkSettings>(Settings.GetJsonSettings);
                    return await ReWriteSettingsAsync(frameworkSettings, token);
                }

                if (exist && typeof(IAppSettings).IsAssignableFrom(settingsType))
                {
                    var appSettings = json.Deserialize<AppSettings>(Settings.GetJsonSettings);
                    return await ReWriteSettingsAsync(appSettings, token);
                }

                return Result.Ok();
            }
            catch (Exception e)
            {
                _logger?.LogError("{Message}", e.Message);
                return Result.Fail("Update app settings failed");
            }
        }

        private T GetOrCreateOption<T>() where T : class, IOptions
        {
            var option = typeof(IConfigurePluginOption).IsAssignableFrom(typeof(T))
                ? Configuration.GetSection($"{nameof(Kharazmi.Configuration.FrameworkSettings)}:{typeof(T).Name}")
                    .Get<T>()
                : Configuration.GetSection($"{nameof(Kharazmi.Configuration.AppSettings)}:{typeof(T).Name}").Get<T>();

            return option.IsNull() ? typeof(T).CreateInstance<T>() : option;
        }

        private void OnUpdatedOptions<TOptions>(TOptions options) where TOptions : class, IOptions
            => UpdatedOptionHandler?.Invoke(this, options, typeof(TOptions));

        private void OnRestored(string providerName)
        {
            _backupSettings.GetBackupSettings().ProviderKeys.TryGetValue(providerName, out var settingsType);
            ReLoadSettings(settingsType);
        }

        private void ReLoadSettings(Type? settingsType)
        {
            if (settingsType.IsNull())
                return;

            var result = _backupSettings.GetProviderPath(settingsType);

            if (result.Failed)
            {
                _logger.LogError("{Message}", result.Description);
                return;
            }

            var configSourcePath = result.Value;
            if (configSourcePath.IsEmpty()) return;

            if (!File.Exists(configSourcePath)) return;

            var json = File.ReadAllText(configSourcePath);

            if (typeof(IFrameworkSettings).IsAssignableFrom(settingsType))
            {
                var settingsObj = json.Deserialize<Dictionary<string, FrameworkSettings>>(Settings.GetJsonSettings);
                if (settingsObj.IsNull()) return;

                settingsObj.TryGetValue(settingsType.Name, out var frameworkSettings);
                if (frameworkSettings.IsNull()) return;

                UpdateFrameworkSettings(frameworkSettings);
            }

            if (typeof(IAppSettings).IsAssignableFrom(settingsType))
            {
                var jsonObj = json.Deserialize<JObject>(Settings.GetJsonSettings);

                var settingsToken = jsonObj?[settingsType.Name];
                if (settingsToken.IsNull()) return;

                var appSettings = JObject.FromObject(settingsToken).ToObject<AppSettings>();
                if (appSettings.IsNull()) return;

                UpdateAppSettings(appSettings);
            }
        }

        private void UpdateAppSettings(AppSettings appSettings)
        {
            foreach (var (key, value) in appSettings)
            {
                _AppSettings.Remove(key);
                _AppSettings.TryAdd(key, value);
            }
        }

        private void UpdateFrameworkSettings(FrameworkSettings frameworkSettings)
        {
            foreach (var (key, value) in frameworkSettings)
            {
                _FrameworkSettings.Remove(key);
                _FrameworkSettings.TryAdd(key, value);
            }
        }

        private static void ClearOptionsChanges<T>(T options) where T : class, IOptions
        {
            options.MakeDirty(false);
            if (options is not IChildOptions childOptions) return;

            foreach (var childOption in childOptions.GetChildOptions(childOptions.GetChildType()))
                childOption.MakeDirty(false);
        }

        private async Task<Result> ReWriteSettingsAsync<TSettings>(TSettings? settings,
            CancellationToken token = default) where TSettings : class, ISettings
        {
            if (settings is null)
                return Result.Fail("Update app settings failed. Invalid settings type");

            var type = typeof(TSettings);

            _backupSettings.GetBackupSettings().ProviderTypes.TryGetValue(type, out var sourceName);

            if (sourceName.IsEmpty())
            {
                _logger?.LogError("Invalid config source path with type {Settings} and source path {SourcePath}",
                    typeof(TSettings).Name, sourceName);
                return Result.Fail(
                    $"Invalid config source path with type {typeof(TSettings).Name} and source path {sourceName}");
            }

            var configSourcePath = Path.Combine(_evn.ContentRootPath, sourceName);

            var jsonObj = await ReadJsonFileAsync(configSourcePath, token);
            if (jsonObj.IsNull())
            {
                _logger?.LogTrace(
                    "Can't read json file with source type {SourceType}", typeof(TSettings).Name);
                return Result.Fail("Update app settings failed. Invalid settings type");
            }

            var settingSection = jsonObj[type.Name];
            if (settingSection.IsNull())
            {
                _logger?.LogTrace("Can't find section with type {TypeName}", type.Name);
                return Result.Fail("Update app settings failed. Invalid settings type");
            }

            var settingsJObject = JObject.FromObject(settingSection);
            if (settingsJObject.IsNull())
            {
                _logger?.LogTrace("Can't find section with type {TypeName}", type.Name);
                return Result.Fail("Update app settings failed. Invalid settings type");
            }

            foreach (var (key, value) in settings)
            {
                settingsJObject.Remove(key);
                settingsJObject.Add(key, JObject.FromObject(value));
            }

            jsonObj.Remove(type.Name);
            jsonObj.Add(type.Name, JObject.FromObject(settingsJObject));

            await File.WriteAllTextAsync(configSourcePath, jsonObj.Serialize(Settings.GetJsonSettings), token);

            _logger?.LogTrace(
                "ReWrite settings with source type {SourceType}", typeof(TSettings).Name);

            return Result.Ok($"ReWrite settings with source type {typeof(TSettings).Name}");
        }

        private async Task<JObject?> ReadJsonFileAsync(string configSourcePath,
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

        private void SubscribeTo(IConfigurationSection section, IOptions option)
        {
            ChangeToken.OnChange(section.GetReloadToken, () =>
            {
                // TODO
            });
        }
    }
}