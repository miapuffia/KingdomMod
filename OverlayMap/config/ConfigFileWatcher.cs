using System;
using System.IO;

namespace KingdomMod {
    public class ConfigFileWatcher {
        private readonly FileSystemWatcher _watcher = new();
        private string _configFileHash;
        private FileSystemEventHandler _changedEventHandler;

        public void Set(string fileName, FileSystemEventHandler changed) {
            _watcher.Path = Path.Combine(FileUtils.GetBepInExDir(), "config");
            _watcher.NotifyFilter = NotifyFilters.LastWrite;
            _watcher.Filter = fileName ?? "*.cfg";
            _watcher.Changed += OnConfigFileChanged;
            _watcher.IncludeSubdirectories = false;
            _watcher.EnableRaisingEvents = true;
            _changedEventHandler = changed;
        }

        private void OnConfigFileChanged(object source, FileSystemEventArgs e) {
            try {
                var hash = FileUtils.GetFileHash(e.FullPath);
                if(hash == "")
                    return;
                if(hash == _configFileHash)
                    return;
                _configFileHash = hash;
                _changedEventHandler?.Invoke(source, e);
            } catch(Exception exception) {
                LogUtil.Message($"HResult: {exception.HResult:X}, {exception.Message}");
            }
        }
    }
}
