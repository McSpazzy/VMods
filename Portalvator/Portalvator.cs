using BepInEx;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace Portalvator
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInProcess("valheim.exe")]
    public class Portalvator : BaseUnityPlugin
    {
        public const string PluginGUID = "org.ssmvc.portalvator";
        public const string PluginName = "Portalvator";
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

        [HarmonyPatch(typeof(Player))]
        public static class PlayerPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(Player.TeleportTo))]
            public static void TeleportToPostfix(Player __instance, Vector3 pos, Quaternion rot, ref bool distantTeleport, ref bool __result)
            {
                if (!__result || !distantTeleport || !ZNetScene.instance.IsAreaReady(pos))
                {
                    return;
                }

                var currentZone = ZoneSystem.instance.GetZone(__instance.transform.position);
                var targetZone = ZoneSystem.instance.GetZone(pos);
                var zoneAdjacent = Mathf.Abs(currentZone.x - targetZone.x) <= 1 && Mathf.Abs(currentZone.y - targetZone.y) <= 1;

                if (zoneAdjacent)
                {
                    __instance.m_distantTeleport = false;
                }
            }
        }
    }
}
