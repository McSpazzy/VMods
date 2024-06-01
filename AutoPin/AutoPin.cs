using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace AutoPin
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInProcess("valheim.exe")]
    public class AutoPin : BaseUnityPlugin
    {
        public const string PluginGUID = "org.ssmvc.autopin";
        public const string PluginName = "AutoPin";
        public const string PluginVersion = "1.0.1";

        private static Harmony _harmony;
        public static ConfigEntry<bool> Enabled { get; set; }

        private ConfigEntry<KeyboardShortcut> AddPointKey { get; set; }
        private ConfigEntry<Minimap.PinType> AddPointType { get; set; }
        private ConfigEntry<string> AddPointName { get; set; }
        private ConfigEntry<bool> AddDay { get; set; }

        public void Awake()
        {
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
            Enabled = Config.Bind<bool>("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

            AddPointKey = Config.Bind("General", "ShortcutKey", new KeyboardShortcut(KeyCode.Quote), "Shortcut Key");
            AddPointType = Config.Bind("General", "Icon", Minimap.PinType.Icon3, "Pin Type");
            AddPointName = Config.Bind("General", "Text", "AutoPin", "Text to use for AutoPin");
            AddDay = Config.Bind("General", "AppendDay", true, "Append day to pin");
        }

        public void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }

        private void Update()
        {
            if (!Enabled.Value)
            {
                return;
            }

            if (AddPointKey.Value.IsDown())
            {
                if (!Minimap.instance)
                {
                    return;
                }

                var pinName = AddPointName.Value;
                if (AddDay.Value)
                {
                    pinName += $"\r\n$hud_mapday {EnvMan.instance.GetDay(ZNet.instance.GetTimeSeconds())}";
                }
                Minimap.instance.AddPin(Player.m_localPlayer.transform.position, AddPointType.Value, pinName, true, false, Player.m_localPlayer.GetPlayerID(), "AutoPin");
            }
        }
    }
}
