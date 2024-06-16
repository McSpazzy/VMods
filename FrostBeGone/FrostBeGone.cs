using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace FrostBeGone
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInProcess("valheim.exe")]
    public class FrostBeGone : BaseUnityPlugin
    {
        public const string PluginGUID = "org.ssmvc.frostbegone";
        public const string PluginName = "FrostBeGone";
        public const string PluginVersion = "1.0.0";

        private static Harmony _harmony;
        public static ConfigEntry<bool> Enabled { get; set; }

        public void Awake()
        {
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
            Enabled = Config.Bind<bool>("_Global", "isModEnabled", true, "Globally enable or disable this mod.");
        }

        public void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }

        [HarmonyPatch(typeof(EffectList))]
        private static class EffectListPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(EffectList.Create))]
            public static void CreatePostfix(EffectList __instance, Vector3 basePos, Quaternion baseRot, Transform baseParent, float scale, int variant, GameObject[] __result)
            {
                if (!Enabled.Value)
                {
                    return;
                }

                foreach (var effect in __result.Where(e => e.name.StartsWith("vfx_Frost")))
                {
                    if (!ReferenceEquals(baseParent, Player.m_localPlayer.transform))
                    {
                        effect.SetActive(false);
                    }
                }
            }
        }
    }
}
