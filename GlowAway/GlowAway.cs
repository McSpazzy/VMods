using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;

namespace GlowAway
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInProcess("valheim.exe")]
    public class GlowAway : BaseUnityPlugin
    {
        public const string PluginGUID = "org.ssmvc.glowaway";
        public const string PluginName = "GlowAway";
        public const string PluginVersion = "1.0.0";

        private static Harmony _harmony;
        public static ConfigEntry<bool> Enabled { get; set; }
        public static ConfigEntry<float> Scale { get; set; }
        public static ConfigEntry<bool> GlowEnabled { get; set; }
        public static ConfigEntry<float> IntensityMax { get; set; }

        public void Awake()
        {
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
            Enabled = Config.Bind<bool>("_Global", "isModEnabled", true, "Globally enable or disable this mod.");
            GlowEnabled = Config.Bind<bool>("Glow", "enableGlow", true, "Enable Glow");
            Scale = Config.Bind<float>("Glow", "scale", 1, "Glow Scale.");
            IntensityMax = Config.Bind<float>("Glow", "intensityMax", 10, "Max Intensity");
        }

        public void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }

        [HarmonyPatch(typeof(ParticleIntensityScaler))]
        private static class ParticleIntensityScalerPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(nameof(ParticleIntensityScaler.ApplyIntensity))]
            public static void ApplyIntensityPrefix(ParticleIntensityScaler __instance)
            {
                if (!Enabled.Value)
                {
                    return;
                }

                if (!GlowEnabled.Value)
                {
                    __instance.intensity = 0;
                }

                if (__instance.intensity > IntensityMax.Value)
                {
                    __instance.intensity = IntensityMax.Value;
                }

                __instance.scaleEmission = __instance.scaleEmission * Scale.Value;
            }
        }
    }
}
