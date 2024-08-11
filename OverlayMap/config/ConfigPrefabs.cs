using System.Collections.Generic;
using System.IO;

namespace KingdomMod {
    public class ConfigPrefabs {
        private static readonly List<ConfigPrefabStruct> _prefabs = new()
        {
                    new ConfigPrefabStruct
                    {
                        ResName = "KingdomMod.prefabs.KingdomMod.OverlayMap.Style.cfg",
                        FileName = "KingdomMod.OverlayMap.Style.cfg"
                    },
                    new ConfigPrefabStruct
                    {
                        ResName = "KingdomMod.prefabs.KingdomMod.OverlayMap.Language_en-US.cfg",
                        FileName = "KingdomMod.OverlayMap.Language.en-US.cfg"
                    },
                    new ConfigPrefabStruct
                    {
                        ResName = "KingdomMod.prefabs.KingdomMod.OverlayMap.Language_zh-CN.cfg",
                        FileName = "KingdomMod.OverlayMap.Language.zh-CN.cfg"
                    }
                };

        public static void Initialize() {
            var bepInExDir = FileUtils.GetBepInExDir();

            foreach(var prefab in _prefabs) {
                var configFile = Path.Combine(bepInExDir, "config", prefab.FileName);
                LogUtil.Message($"Config prefab file: {configFile}");

                if(!File.Exists(configFile)) {
                    LogUtil.Message($"Config prefab file do not exist: {configFile}");

                    File.WriteAllText(configFile, FileUtils.GetEmbeddedResourceString(prefab.ResName));
                }
            }
        }
    }
}
