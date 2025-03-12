using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Kharazmi.Guard;
using Kharazmi.Helpers;

namespace Kharazmi.Configuration
{
    public class BackupSourceFile
    {
        private readonly HashSet<FileInfo> _fileInfos;

        public BackupSourceFile(string providerName)
        {
            ProviderName = providerName;
            _fileInfos = new HashSet<FileInfo>(new BackupFileInfoComparer());
            OccurredOn = DateTimeHelper.DateTimeOffsetUtcNow.ToString("f");
            ChangedAt = DateTimeHelper.DateTimeOffsetUtcNow.ToString("f");
        }

        [ReadOnly(true), Bindable(false), Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public IReadOnlyList<FileInfo> FileInfos => _fileInfos.ToList();

        public string Default => _fileInfos.First(x => x.Name == ProviderName).Name;

        public IReadOnlyList<string> FileNames => _fileInfos.Select(x => x.Name).ToList();
        public string ProviderName { get; }

        [ReadOnly(true), Bindable(false)] public string OccurredOn { get; }
        [ReadOnly(true), Bindable(false)] public string ChangedAt { get; private set; }
     

        public BackupSourceFile UpdateDate()
        {
            ChangedAt = DateTimeHelper.DateTimeOffsetUtcNow.ToString("f");
            return this;
        }

        public BackupSourceFile AddOrUpdateFileInfos(List<FileInfo> fileInfos)
        {
            fileInfos.NotNull(nameof(fileInfos));
            foreach (var fileInfo in fileInfos)
                _fileInfos.Add(fileInfo);

            return this;
        }

        public BackupSourceFile RemoveFileInfo(FileInfo? value)
        {
            if (value is not null)
                _fileInfos.Remove(value);

            return this;
        }

        public BackupSourceFile RemoveFileInfos(List<FileInfo> fileInfos)
        {
            fileInfos.NotNull(nameof(fileInfos));
            foreach (var fileInfo in fileInfos)
                RemoveFileInfo(fileInfo);

            return this;
        }
    }
}