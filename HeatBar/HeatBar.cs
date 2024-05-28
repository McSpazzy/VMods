using BepInEx;
using HarmonyLib;
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
        public const string PluginVersion = "1.0.0";

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
            private static GuiBar HeatDisplay = null;
            private static Transform HeatTransform = null;

            public static void Postfix(Character __instance)
            {

                if (!__instance.IsPlayer())
                {
                    return;
                }

                var hudI = Hud.instance;

                if (HeatDisplay == null)
                {
                    HeatTransform = Hud.instance.m_rootObject.transform.Find("heatbar");
                    if (HeatTransform != null)
                    {
                        HeatDisplay = HeatTransform.gameObject.GetComponent<GuiBar>();
                    }
                    else
                    {
                        HeatDisplay = Instantiate(hudI.m_staggerProgress, Hud.instance.m_rootObject.transform, true);
                        HeatDisplay.transform.Translate(0, -30, 0);
                        HeatDisplay.name = "heatbar";
                        HeatDisplay.SetWidth(100);
                        HeatDisplay.SetMaxValue(0.7f);

                        var data = Assembly.GetExecutingAssembly().GetManifestResourceStream("HeatBar.image.png");
                        if (data != null)
                        {
                            var iconImage = HeatDisplay.transform.Find("StaggerIcon").gameObject.GetComponent<Image>();
                            var imageData = new byte[data.Length];
                            _ = data.Read(imageData, 0, imageData.Length);
                            var texture = new Texture2D(1, 1);
                            texture.LoadImage(imageData);
                            iconImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero); ;
                        }
                    }
                }

                if (HeatDisplay != null)
                {
                    HeatDisplay.SetValue(__instance.m_lavaHeatLevel);
                    HeatDisplay.gameObject.SetActive(HeatDisplay.m_value > 0 || HeatDisplay.m_delayTimer > -1f);
                }
            }
        }
    }
}
