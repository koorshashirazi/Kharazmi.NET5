using System;
using System.IO;

namespace Kharazmi.Watcher
{
    public class FilesWatcher
    {
        private readonly FileSystemWatcher _watcher = new();

        public event EventHandler<RenamedEventArgs>? OnRenamed;
        public event EventHandler<FileSystemEventArgs>? OnDeleted;
        public event EventHandler<FileSystemEventArgs>? OnCreated;
        public event EventHandler<FileSystemEventArgs>? OnModified;

        public FilesWatcher()
        {
        }

        public void Watch(string path, params string[] filterPattern)
        {
            _watcher.Path = path;
            _watcher.IncludeSubdirectories = true;
            _watcher.NotifyFilter = NotifyFilters.Attributes |
                                    NotifyFilters.CreationTime |
                                    NotifyFilters.DirectoryName |
                                    NotifyFilters.FileName |
                                    NotifyFilters.LastAccess |
                                    NotifyFilters.LastWrite |
                                    NotifyFilters.Security |
                                    NotifyFilters.Size;

            foreach (var filter in filterPattern)
            {
                _watcher.Filters.Add(filter);
            }
            
            _watcher.Changed += Changed;
            _watcher.Created += Created;
            _watcher.Deleted += Deleted;
            _watcher.Renamed += Renamed;
        }

        private void Renamed(object sender, RenamedEventArgs e)
        {
            OnRenamed?.Invoke(sender, e);
        }

        private void Deleted(object sender, FileSystemEventArgs e)
        {
            OnDeleted?.Invoke(sender, e);
        }

        private void Created(object sender, FileSystemEventArgs e)
        {
            OnCreated?.Invoke(sender, e);
        }

        private void Changed(object sender, FileSystemEventArgs e)
        {
            OnModified?.Invoke(sender, e);
        }
    }
}