using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ColorBubble
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInProcess("valheim.exe")]
    public class ColorBubble : BaseUnityPlugin
    {
        public const string PluginGUID = "org.ssmvc.colorbubble";
        public const string PluginName = "ColorBubble";
        public const string PluginVersion = "1.0.0";

        private static Harmony _harmony;

        public static ConfigEntry<Color> BubbleColor { get; set; }
        public static ConfigEntry<bool> Enabled { get; set; }

        public void Awake()
        {
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
            BubbleColor = base.Config.Bind<Color>("General", "Bubble Color", new Color(1, 0, 0, 0.5f));
            Enabled = base.Config.Bind<bool>("General", "Enabled", true, "Enable mod");
        }

        public void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }

        [HarmonyPatch(typeof(EffectList), "Create")]
        private static class ColorTheBubble
        {
            public static void Postfix(GameObject[] __result)
            {
                if (!Enabled.Value)
                {
                    return;
                }

                foreach (var r in __result)
                {
                    if (r.name.Contains("vfx_StaffShield"))
                    {
                        var renderer = r.GetComponentInChildren<MeshRenderer>();
                        renderer.material.color = BubbleColor.Value;

                        // Rainbow still in testing
                        //var rot = r.AddComponent<RotateColor>();
                        //rot.Alpha = BubbleColor.Value.a;
                    }
                }
            }
        }

        // I made this but then never used it. maybe in the future.
        public static List<Vector3> GetCirclePoints3D(int n, float radius)
        {
            var points = new List<Vector3>();
            for (var i = 0; i < n; i++)
            {
                var angle = 2 * Mathf.PI * i / n;
                var x = radius * Mathf.Cos(angle);
                var z = radius * Mathf.Sin(angle);
                points.Add(new Vector3(x, 0, z));
            }
            return points;
        }

        // I made this but then never used it. maybe in the future.
        public static Color[] GenerateRainbowColors(int n)
        {
            var colors = new Color[n];
            var hueStep = 1.0f / n;
    
            for (var i = 0; i < n; i++)
            {
                var hue = i * hueStep;
                colors[i] = Color.HSVToRGB(hue, 1.0f, 1.0f);
            }
            return colors;
        }
    }

    // I made this but then never used it. maybe in the future.
    public class RotateColor : MonoBehaviour
    {
        public float Alpha = 0.5f;
        static Color GetRainbowColor(float time, float duration, float alpha)
        {
            var hue = (time % duration) / duration;
            var color = Color.HSVToRGB(hue, 1f, 1f);
            color.a = alpha;
            return color;
        }

        void Update()
        {
            var rendererMag = GetComponentInChildren<MeshRenderer>();
            rendererMag.material.color = GetRainbowColor(Time.time, 1, Alpha);
        }
    }
}
