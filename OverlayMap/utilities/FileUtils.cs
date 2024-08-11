using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace KingdomMod {
    public static class FileUtils {

        public static string GetBepInExDir() {
            var baseDir = Assembly.GetExecutingAssembly().Location;
            var bepInExDir = Directory.GetParent(baseDir)?.Parent?.Parent?.FullName;

            bepInExDir ??= "BepInEx\\";
            return bepInExDir;
        }

        public static string GetFileHash(string filename) {
            int retry = 0;
            do {
                try {
                    using(var md5 = MD5.Create()) {
                        using(var stream = File.OpenRead(filename)) {
                            return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "");
                        }
                    }
                } catch(Exception e) {
                    if((uint) e.HResult != 0x80070020)
                        LogUtil.Message($"HResult: {e.HResult:X}, {e.Message}");
                }

                retry++;
                System.Threading.Thread.Sleep(10);
            } while(retry < 3);

            return "";
        }

        public static string GetEmbeddedResourceString(string res) {
            using var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(res);
            if(s != null) {
                using var reader = new StreamReader(s);
                return reader.ReadToEnd();
            }

            return string.Empty;
        }

        public static byte[] GetEmbeddedResourceBytes(string res) {
            using var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(res);

            if(s != null) {
                using var memoryStream = new MemoryStream();
                s.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }

            return [];
        }
    }
}
