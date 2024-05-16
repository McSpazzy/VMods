using BepInEx;
using HarmonyLib;
using System.Reflection;

namespace NoEitr
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class NoEitr : BaseUnityPlugin
    {
        public const string PluginGUID = "org.ssmvc.noeitr";
        public const string PluginName = "NoEitr";
        public const string PluginVersion = "1.0.1";

        Harmony _harmony;

        void Awake()
        {
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
        }

        void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }

        [HarmonyPatch(typeof(Humanoid), "GetCurrentWeapon")]
        private static class GetCurrentWeaponPatch
        {
            public static void Postfix(Character __instance, ref ItemDrop.ItemData __result)
            {
                if (__instance.IsPlayer() && __result != null)
                {
                    if (__result.m_shared.m_attack.m_reloadEitrDrain > 0)
                    {
                        __result.m_shared.m_attack.m_reloadEitrDrain = 0;
                    }

                    if (__result.m_shared.m_attack.m_attackEitr > 0)
                    {
                        __result.m_shared.m_attack.m_attackEitr = 0;
                    }
                }
            }
        }
    }
}
