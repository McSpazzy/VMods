using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;

namespace Crowned
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInProcess("valheim.exe")]
    public class Crowned : BaseUnityPlugin
    {
        public const string PluginGUID = "org.ssmvc.crowned";
        public const string PluginName = "Crowned";
        public const string PluginVersion = "1.0.1";

        private static Harmony _harmony;
        public static ConfigEntry<bool> Enabled { get; set; }
        public static ConfigEntry<bool> DisableCrownEffect { get; set; }
        public static ConfigEntry<string> DisabledText { get; set; }
        
        private volatile bool _refreshCrownMode;

        public void Awake()
        {
            Enabled = base.Config.Bind<bool>("_Global", "isModEnabled", true, "Globally enable or disable this mod.");
            DisableCrownEffect = base.Config.Bind<bool>("Crown", "disableCrownEffect", true, "Disable the crown's monster fear effect.");
            DisabledText = base.Config.Bind<string>("Crown", "disabledText", "Ceremonial", "Text to show if crown effect is disabled.");
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
            Config.SettingChanged += OnSettingChanged;
            SetCrownMode();
        }

        private void OnSettingChanged(object sender, SettingChangedEventArgs args)
        {
            if (args.ChangedSetting == Enabled || args.ChangedSetting == DisableCrownEffect)
            {
                _refreshCrownMode = true;
            }
        }

        public void Update()
        {
            if (!_refreshCrownMode) return;
            _refreshCrownMode = false;
            SetCrownMode();
        }

        private static void SetCrownMode()
        {
            var player = Player.m_localPlayer;
            if (!player) return;
            player.SetCrownMode(player.GetSEMan().GetStatusEffects().OfType<SE_Crowned>().Any());
        }

        public void OnDestroy()
        {
            Config.SettingChanged -= OnSettingChanged;
            _harmony?.UnpatchSelf();
            SetCrownMode();
        }
    }

    [HarmonyPatch(typeof(Player))]
    static class PlayerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Player.SetCrownMode))]
        static void SetCrownModePrefix(ref bool crownMode)
        {
            if (Crowned.Enabled.Value && Crowned.DisableCrownEffect.Value)
            {
                crownMode = false;
            }
        }
    }

    [HarmonyPatch(typeof(StatusEffect), "GetIconText")]
    static class StatusEffectPatch
    {
        public static void Postfix(StatusEffect __instance, ref string __result)
        {
            if (Crowned.Enabled.Value && Crowned.DisableCrownEffect.Value && __instance is SE_Crowned)
            {
                __result = Crowned.DisabledText.Value;
            }
        }
    }
}
