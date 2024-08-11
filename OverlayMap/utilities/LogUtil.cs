namespace KingdomMod {
    internal static class LogUtil {
        public static void Error(object o) => OverlayMapPlugin.Instance.LogSource.LogError(o);
        public static void Info(object o) => OverlayMapPlugin.Instance.LogSource.LogInfo(o);
        public static void Message(object o) => OverlayMapPlugin.Instance.LogSource.LogMessage(o);
        public static void Warning(object o) => OverlayMapPlugin.Instance.LogSource.LogWarning(o);
    }
}
