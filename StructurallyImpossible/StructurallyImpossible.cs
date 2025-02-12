using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;

namespace StructurallyImpossible
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInProcess("valheim.exe")]
    public class StructurallyImpossible : BaseUnityPlugin
    {
        public const string PluginGUID = "org.ssmvc.structurallyimpossible";
        public const string PluginName = "StructurallyImpossible";
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

        [HarmonyPatch(typeof(Piece))]
        public static class PiecePatch
        {
            [HarmonyPrefix, HarmonyPatch(nameof(Piece.SetCreator))]
            public static void SetCreator(Piece __instance, long uid)
            {
                if (!Enabled.Value)
                {
                    return;
                }

                if (__instance && __instance.m_nview && __instance.m_nview.IsValid() && !__instance.TryGetComponent(out Plant _))
                {
                    if (__instance.TryGetComponent(out WearNTear wearNTear))
                    {
                        WearNTearManager.SetNoSupportWear(wearNTear);
                    }
                }
            }
        }
    }
}
