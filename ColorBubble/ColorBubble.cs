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
        public const string PluginVersion = "1.0.2";

        private static Harmony _harmony;
        public static ConfigEntry<bool> Enabled { get; set; }

        public static ConfigEntry<bool> EnableBubbleColor { get; set; }
        public static ConfigEntry<Color> BubbleColor { get; set; }

        public static ConfigEntry<bool> ShowBubblePercent { get; set; }
        public static ConfigEntry<bool> ShowBubbleHits { get; set; }

        public static ConfigEntry<float> ShaderVelocity { get; set; }
        public static ConfigEntry<float> ShaderRefraction { get; set; }
        public static ConfigEntry<float> ShaderGlossiness { get; set; }
        public static ConfigEntry<float> ShaderMetallic { get; set; }

        public void Awake()
        {
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
            Enabled = base.Config.Bind<bool>("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

            EnableBubbleColor = base.Config.Bind<bool>("Color", "Enable Bubble Color", true, "Enable or disable the bubble color");
            BubbleColor = base.Config.Bind<Color>("Color", "Bubble Color", new Color(1, 0, 0, 0.5f));

            ShowBubblePercent = base.Config.Bind<bool>("Misc", "Show Bubble Percent", true, "Show remaining bubble integrity");
            ShowBubbleHits = base.Config.Bind<bool>("Misc", "Show Bubble Hits", true, "Show bubble damage taken");

            ShaderVelocity = base.Config.Bind<float>("Shader", "ShaderVelocity", 5f, "Wavy Speed");
            ShaderRefraction = base.Config.Bind<float>("Shader", "ShaderRefraction", 0.1f, "Bubble Distortion");
            ShaderGlossiness = base.Config.Bind<float>("Shader", "ShaderGlossiness", 0.8f, "Bubble Glossiness");
            ShaderMetallic = base.Config.Bind<float>("Shader", "ShaderMetallic", 1f, "Bubble Metallic");
        }

        public void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }

        [HarmonyPatch(typeof(SE_Shield))]
        private static class ShieldPatch
        {
            [HarmonyPatch("Setup")]
            [HarmonyPostfix]
            static void SetupPostfix(SE_Shield __instance, Character character)
            {
                // System.Console.WriteLine("Shield SETUP");
            }

            [HarmonyPatch("OnDamaged")]
            [HarmonyPrefix]
            static void OnDamagedPrefix(SE_Shield __instance, HitData hit, Character attacker)
            {
                // System.Console.WriteLine($"Shield HIT {__instance.m_damage} {__instance.m_absorbDamage} {__instance.m_totalAbsorbDamage}" );
                if (Enabled.Value && ShowBubbleHits.Value)
                {
                    if (hit.GetTotalDamage() > 0)
                    {
                        DamageText.instance.ShowText(HitData.DamageModifier.Normal, hit.m_point, hit.GetTotalDamage());
                    }
                }
            }
        }

        [HarmonyPatch(typeof(StatusEffect))]
        private static class StatusEffectPatch
        {
            [HarmonyPatch("GetIconText")]
            [HarmonyPostfix]
            static void GetIconTextPostfix(StatusEffect __instance, ref string __result)
            {
                if (Enabled.Value && ShowBubblePercent.Value && __instance is SE_Shield derivedInstance)
                {
                    var perc = (derivedInstance.m_damage / derivedInstance.m_totalAbsorbDamage) * 100f;
                    __result += $" ({100-perc:##}%)";
                }
            }
        }

        [HarmonyPatch(typeof(EffectList), "Create")]
        private static class ColorTheBubble
        {
            public static void Postfix(GameObject[] __result)
            {
                if (!Enabled.Value || !EnableBubbleColor.Value)
                {
                    return;
                }

                foreach (var r in __result)
                {
                    if (r.name.Contains("vfx_StaffShield"))
                    {
                        var renderer = r.GetComponentInChildren<MeshRenderer>();
                        renderer.material.color = BubbleColor.Value;

                        renderer.material.SetFloat("_WaveVel", ShaderVelocity.Value);
                        renderer.material.SetFloat("_RefractionIntensity", ShaderRefraction.Value);
                        renderer.material.SetFloat("_Glossiness", ShaderGlossiness.Value);
                        renderer.material.SetFloat("_Metallic", ShaderMetallic.Value);

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
