using BepInEx.Configuration;
using System;
using System.Globalization;
using System.IO;

namespace KingdomMod {
    public class GlobalConfigs {
        public static ConfigFile ConfigFile;
        private static readonly ConfigFileWatcher _configFileWatcher = new();
        public static ConfigEntryWrapper<string> Language;
        public static ConfigEntryWrapper<string> StyleFile;
        public static ConfigEntryWrapper<int> GUIUpdatesPerSecond;
        public static ConfigEntryWrapper<float> GUIScalingSinglePlayer;
        public static ConfigEntryWrapper<float> GUIScalingCoop;

        public static void ConfigBind(ConfigFile config) {
            LogUtil.Message($"ConfigBind: {Path.GetFileName(config.ConfigFilePath)}");

            ConfigPrefabs.Initialize();

            ConfigFile = config;
            config.SaveOnConfigSet = true;
            config.Clear();

            Language = config.Bind("Global", "Language", "system", "");
            StyleFile = config.Bind("Global", "StyleFile", "KingdomMod.OverlayMap.Style.cfg", "");
            GUIUpdatesPerSecond = config.Bind("Global", "GUIUpdatesPerSecond", 10, "Increase to be more accurate, decrease to reduce performance impact");
            GUIScalingSinglePlayer = config.Bind("Global", "GUIScalingSinglePlayer", 1.0f, "Smaller: <1, bigger: >1");
            GUIScalingCoop = config.Bind("Global", "GUIScalingCoop", 0.5f, "Smaller: <1, bigger: >1");

            LogUtil.Message($"ConfigFilePath: {config.ConfigFilePath}");
            LogUtil.Message($"Language: {Language.Value}");
            LogUtil.Message($"StyleFile: {StyleFile.Value}");
            LogUtil.Message($"GUIUpdatesPerSecond: {GUIUpdatesPerSecond.Value}");

            LogUtil.Message($"Loaded config: {Path.GetFileName(ConfigFile.ConfigFilePath)}");

            OnLanguageChanged();
            OnStyleFileChanged();

            SetConfigDelegates();
            _configFileWatcher.Set(Path.GetFileName(config.ConfigFilePath), OnConfigFileChanged);
        }

        private static void OnConfigFileChanged(object source, FileSystemEventArgs e) {
            try {
                // LogMessage($"OnConfigFileChanged: {e.Name}, {e.ChangeType}");

                ConfigFile.Reload();
            } catch(Exception exception) {
                LogUtil.Message($"HResult: {exception.HResult:X}, {exception.Message}");
            }
        }

        public static void SetConfigDelegates() {
            Language.Entry.SettingChanged += (sender, args) => OnLanguageChanged();
            StyleFile.Entry.SettingChanged += (sender, args) => OnStyleFileChanged();
        }

        public static void OnLanguageChanged() {
            LogUtil.Message($"OnLanguageChanged: {Language.Entry.Value}");

            var lang = Language.Value;
            if(lang is "" or "system")
                lang = CultureInfo.CurrentCulture.Name;

            var bepInExDir = FileUtils.GetBepInExDir();
            var langFile = Path.Combine(bepInExDir, "config", $"KingdomMod.OverlayMap.Language.{lang}.cfg");
            LogUtil.Message($"Language file: {langFile}");

            if(!File.Exists(langFile)) {
                LogUtil.Warning($"Language file do not exist: {langFile}");
                lang = lang.Split('-')[0];
                var files = Directory.GetFiles(Path.Combine(bepInExDir, "config"), $"KingdomMod.OverlayMap.Language.{lang}*.cfg");
                foreach(var file in files) {
                    if(File.Exists(file)) {
                        langFile = file;
                        LogUtil.Warning($"Try to use the sub language file: {langFile}");
                        break;
                    }
                }
            }

            if(!File.Exists(langFile)) {
                LogUtil.Warning($"Language file do not exist: {langFile}");
                if(ConfigStrings.ConfigFile != null)
                    return;
                lang = "en-US";
                langFile = Path.Combine(bepInExDir, "config", $"KingdomMod.OverlayMap.Language.{lang}.cfg");
                LogUtil.Warning($"Try to use the default english language file: {langFile}");
            }

            if(ConfigStrings.ConfigFile != null) {
                if(Path.GetFileName(ConfigStrings.ConfigFile.ConfigFilePath) == Path.GetFileName(langFile)) {
                    LogUtil.Message("Attempt to load the same configuration file. Skip.");
                    return;
                }
            }

            LogUtil.Message($"Try to bind Language file: {Path.GetFileName(langFile)}");
            ConfigStrings.ConfigBind(new ConfigFile(langFile, true));
        }

        public static void OnStyleFileChanged() {
            LogUtil.Message($"OnStyleFileChanged: {StyleFile.Value}");

            var bepInExDir = FileUtils.GetBepInExDir();
            var styleFile = Path.Combine(bepInExDir, "config", StyleFile);
            LogUtil.Message($"Style file: {styleFile}");

            if(!File.Exists(styleFile)) {
                LogUtil.Warning($"Style file do not exist: {styleFile}");
                if(StyleConfigs.ConfigFile != null)
                    return;
                styleFile = Path.Combine(bepInExDir, "config", "KingdomMod.OverlayMap.Style.cfg");
                LogUtil.Warning($"Try to use the default style file: {styleFile}");
            }

            if(StyleConfigs.ConfigFile != null) {
                if(Path.GetFileName(StyleConfigs.ConfigFile.ConfigFilePath) == Path.GetFileName(styleFile)) {
                    LogUtil.Message("Attempt to load the same configuration file. Skip.");
                    return;
                }
            }

            LogUtil.Message($"Try to bind style file: {Path.GetFileName(styleFile)}");
            StyleConfigs.ConfigBind(new ConfigFile(styleFile, true));
        }
    }
}
