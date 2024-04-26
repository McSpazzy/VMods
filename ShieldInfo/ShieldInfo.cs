using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace ShieldInfo
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class ShieldInfo : BaseUnityPlugin
    {
        public const string PluginGUID = "org.ssmvc.shieldinfo";
        public const string PluginName = "ShieldInfo";
        public const string PluginVersion = "1.0.0";

        static ManualLogSource _logger;
        Harmony _harmony;

        void Awake()
        {
            _logger = Logger;
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
        }

        void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }

        [HarmonyPatch(typeof(ShieldGenerator), "GetHoverText")]
        private static class ShieldShowTimer
        {
            public static void Postfix(ShieldGenerator __instance, ref string __result, ref float ___m_lastFuel)
            {
                var domeColor = ShieldDomeImageEffect.GetDomeColor(___m_lastFuel);
               __result += $"Power Remaining: <color=#{ColorUtility.ToHtmlStringRGB(domeColor)}><b>{__instance.GetFuelRatio() * 100:0.00}%</b></color>";
            }
        }
    }
}
