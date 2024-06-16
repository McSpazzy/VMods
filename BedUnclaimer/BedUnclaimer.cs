using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;

namespace BedUnclaimer
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInProcess("valheim.exe")]
    public class BedUnclaimer : BaseUnityPlugin
    {
        public const string PluginGUID = "org.ssmvc.bedunclaimer";
        public const string PluginName = "BedUnclaimer";
        public const string PluginVersion = "1.0.0";

        private static Harmony _harmony;
        public static ConfigEntry<bool> Enabled { get; set; }

        public void Awake()
        {
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
            Enabled = Config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod.");
        }

        public void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }

        [HarmonyPatch(typeof(Bed))]
        private static class BedPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(Bed.GetHoverText))]
            public static void GetHoverTextPostfix(Bed __instance, ref string __result)
            {
                if (!Enabled.Value || __instance.GetOwner() == 0L)
                {
                    return;
                }

                if (__instance.IsMine())
                {
                    __result += Localization.instance.Localize("\n[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>] UnClaim Bed"); 
                }
                else if (Player.m_localPlayer.GetPlayerID() == __instance.GetComponent<Piece>().GetCreator() || Player.m_debugMode)
                {
                    __result += Localization.instance.Localize($"\n[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>] UnClaim {__instance.GetOwnerName()}'s Bed");
                }
                   
            }

            [HarmonyPrefix]
            [HarmonyPatch(nameof(Bed.Interact))]
            public static bool InteractPrefix(Bed __instance, Humanoid human, bool repeat, bool alt)
            {
                if (!Enabled.Value || repeat || !alt || __instance.GetOwner() == 0L)
                    return true;

                if (__instance.IsMine())
                {
                    human.Message(MessageHud.MessageType.Center, $"UnClaimed bed.", 0, null);
                    __instance.SetOwner(0L, "");
                } 
                else if (Player.m_localPlayer.GetPlayerID() == __instance.GetComponent<Piece>().GetCreator() || Player.m_debugMode)
                {
                    System.Console.WriteLine($"UnClaimed {__instance.GetOwnerName()}'s bed.");
                    human.Message(MessageHud.MessageType.Center, $"UnClaimed {__instance.GetOwnerName()}'s bed.", 0, null);
                    __instance.SetOwner(0L, "");
                }

                return false;
            }
        }
    }
}
