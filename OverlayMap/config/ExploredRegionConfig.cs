using BepInEx.Configuration;
using System.IO;

namespace KingdomMod {
    public class ExploredRegionConfig {
        public static ConfigFile ConfigFile;

        public static ConfigEntryWrapper<float> ExploredLeft;
        public static ConfigEntryWrapper<float> ExploredRight;
        public static ConfigEntryWrapper<float> Time;
        public static ConfigEntryWrapper<int> Days;

        public static void ConfigBind(string archiveFilename) {
            var bepInExDir = FileUtils.GetBepInExDir();
            var configFilePath = Path.Combine(bepInExDir, "config", "KingdomMod.OverlayMap.ExploredRegions.cfg");
            LogUtil.Message($"ExploredRegions file: {configFilePath}");

            ConfigFile = new ConfigFile(configFilePath, true);
            ConfigFile.SaveOnConfigSet = true;
            ConfigFile.Clear();

            ExploredLeft = ConfigFile.Bind(archiveFilename, "ExploredLeft", 0f);
            ExploredRight = ConfigFile.Bind(archiveFilename, "ExploredRight", 0f);
            Time = ConfigFile.Bind(archiveFilename, "Time", 0f);
            Days = ConfigFile.Bind(archiveFilename, "Days", 0);

            LogUtil.Message($"Loaded config: {Path.GetFileName(ConfigFile.ConfigFilePath)}");
        }
    }
}
