using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;
using Kharazmi.Extensions;

namespace Kharazmi.Configuration
{
    public class BackupSettings
    {
        private readonly ConcurrentDictionary<string, Type> _providerKeys;
        private readonly ConcurrentDictionary<Type, string> _providerTypes;
        private readonly ConcurrentDictionary<string, BackupSourceFile?> _backupSourceFiles;

        public BackupSettings()
        {
            _providerKeys = new ConcurrentDictionary<string, Type>();
            _providerTypes = new ConcurrentDictionary<Type, string>();
            _backupSourceFiles = new ConcurrentDictionary<string, BackupSourceFile?>();
        }

        public bool IsChanged { get; set; }

        [Newtonsoft.Json.JsonIgnore, JsonIgnore]
        public IReadOnlyDictionary<string, Type> ProviderKeys => _providerKeys;

        [Newtonsoft.Json.JsonIgnore, JsonIgnore]
        public IReadOnlyDictionary<Type, string> ProviderTypes => _providerTypes;

        public IReadOnlyList<string> ProviderNames => _providerKeys.Select(x => x.Key).ToList();

        public IReadOnlyDictionary<string, BackupSourceFile?> BackupSourceFiles => _backupSourceFiles;


        public BackupSettings SetChanged()
        {
            IsChanged = true;
            return this;
        }

        public BackupSettings SetAnyChanged()
        {
            IsChanged = false;
            return this;
        }

        public BackupSettings AddProvider(string providerName, Type providerType)
        {
            if (providerName.IsNotEmpty() && providerType.IsNull() == false)
            {
                _providerKeys.TryAdd(providerName, providerType);
                _providerTypes.TryAdd(providerType, providerName);
            }

            return this;
        }

        public BackupSettings AddBackupSourceFile([MaybeNull] BackupSourceFile? value)
        {
            if (value is not null && value.ProviderName.IsNotEmpty())
                _backupSourceFiles.AddOrUpdate(value.ProviderName, value, (_, oldValue) =>
                {
                    oldValue?.AddOrUpdateFileInfos(value.FileInfos.ToList());
                    oldValue?.UpdateDate();
                    return oldValue;
                });

            return this;
        }
    }
}