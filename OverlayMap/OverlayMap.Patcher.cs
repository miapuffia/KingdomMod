using System;
using HarmonyLib;

namespace KingdomMod
{
    public partial class OverlayMap
    {
        public class Patcher
        {
            private static OverlayMap _overlayMap;

            public static void PatchAll(OverlayMap overlayMap)
            {
                try
                {
                    _overlayMap = overlayMap;
                    var harmony = new Harmony("KingdomMod.OverlayMap.Patcher");
                    harmony.PatchAll();
                }
                catch (Exception ex)
                {
                    LogUtil.Error($"[Patcher] => {ex}");
                }
            }

            [HarmonyPatch(typeof(NetworkBigBoss), nameof(NetworkBigBoss.Client_OnCaughtUp))]
            public class DebugIsDebugBuildPatcher
            {
                public static void Postfix()
                {
                    LogUtil.Message("NetworkBigBoss.Client_OnCaughtUp.");
                    _overlayMap.OnGameStart();
                }
            }
        }
    }
}