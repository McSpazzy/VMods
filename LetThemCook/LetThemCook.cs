using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Splatform;
using System.Reflection;

namespace LetThemCook
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInProcess("valheim.exe")]
    public class LetThemCook : BaseUnityPlugin
    {
        public const string PluginGUID = "org.ssmvc.letthemcook";
        public const string PluginName = "LetThemCook";
        public const string PluginVersion = "1.0.0";

        private static Harmony _harmony;

        public static ConfigEntry<bool> Enabled { get; set; }

        public void Awake()
        {
            Enabled = Config.Bind<bool>("_Global", "isModEnabled", true, "Globally enable or disable this mod.");
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
        }

        public void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }

        [HarmonyPatch(typeof(Piece))]
        public static class PiecePatch
        {
            [HarmonyPrefix, HarmonyPatch(nameof(Piece.SetCreator))]
            public static void SetCreator(Piece __instance, long uid, PlatformUserID platformUserID)
            {
                if (!Enabled.Value)
                {
                    return;
                }

                if (__instance && __instance.m_nview && __instance.m_nview.IsValid())
                {
                    if (__instance.TryGetComponent(out CookingStation cookingStation))
                    {
                        CookingStationManager.SetCanOvercookItems(cookingStation);
                    }
                }
            }
        }
    }
}
