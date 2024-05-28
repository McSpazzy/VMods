using BepInEx;
using HarmonyLib;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace HeatBar
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInProcess("valheim.exe")]
    public class HeatBar : BaseUnityPlugin
    {
        public const string PluginGUID = "org.ssmvc.heatbar";
        public const string PluginName = "HeatBar";
        public const string PluginVersion = "1.0.1";

        static Harmony _harmony;
        
        void Awake()
        {
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
        }

        void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }

        [HarmonyPatch(typeof(Character), "UpdateHeatDamage")]
        static class PlayerPatch
        {
            private static GuiBar _heatDisplay = null;

            public static void Postfix(Character __instance)
            {
                if (!__instance.IsPlayer())
                {
                    return;
                }

                if (_heatDisplay == null)
                {
                    Build();
                }
                else
                {
                    _heatDisplay.SetWidth(100);
                    _heatDisplay.SetMaxValue(0.7f);
                    _heatDisplay.SetValue(__instance.m_lavaHeatLevel);
                    _heatDisplay.gameObject.SetActive(_heatDisplay.m_value > 0 || _heatDisplay.m_delayTimer > -1f);
                }
            }

            private static void Build()
            {
                var hudI = Hud.instance;

                if (hudI.m_staggerProgress == null)
                {
                    return;
                }

                var objects = Resources.FindObjectsOfTypeAll<Transform>().Where(t => t.name == "heatbar").ToArray();
                foreach (var o in objects)
                {
                    if (o.name == "heatbar")
                    {
                        Destroy(o.gameObject);
                    }
                }

                _heatDisplay = Instantiate(hudI.m_staggerProgress, Hud.instance.m_rootObject.transform, true);
                _heatDisplay.transform.Translate(0, -30, 0);
                _heatDisplay.name = "heatbar";
                _heatDisplay.SetWidth(100);
                _heatDisplay.SetMaxValue(0.7f);

                var data = Assembly.GetExecutingAssembly().GetManifestResourceStream("HeatBar.image.png");
                if (data == null) return;
                var iconImage = _heatDisplay.transform.Find("StaggerIcon").gameObject.GetComponent<Image>();
                var imageData = new byte[data.Length];
                _ = data.Read(imageData, 0, imageData.Length);
                var texture = new Texture2D(1, 1);
                texture.LoadImage(imageData);
                iconImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            }
        }
    }
}
