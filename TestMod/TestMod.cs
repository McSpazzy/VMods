using BepInEx;
using HarmonyLib;
using System.Reflection;

namespace TestMod
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInProcess("valheim.exe")]
    public class TestMod : BaseUnityPlugin
    {
        public const string PluginGUID = "org.ssmvc.testmod";
        public const string PluginName = "TestMod";
        public const string PluginVersion = "1.0.0";

        private static Harmony _harmony;

        public void Awake()
        {
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
        }

        public void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }
}
