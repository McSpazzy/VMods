using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;

namespace NoEitr
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class NoEitr : BaseUnityPlugin
    {
        public const string PluginGUID = "org.ssmvc.noeitr";
        public const string PluginName = "NoEitr";
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

        [HarmonyPatch(typeof(Attack), "GetAttackEitr")]
        private static class GetAttackEitrPatch
        {
            public static void Postfix(ref float __result)
            {
                __result = 0;
            }
        }
    }
}