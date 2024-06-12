using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
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
        public const string PluginVersion = "1.0.3";

        private static Harmony _harmony;
        public static ConfigEntry<bool> Enabled { get; set; }

        public static ConfigEntry<bool> EnableBubbleColor { get; set; }
        public static ConfigEntry<Color> BubbleColor { get; set; }
        public static ConfigEntry<bool> BubbleColorSelfOnly { get; set; }

        public static ConfigEntry<bool> ShowBubblePercent { get; set; }
        public static ConfigEntry<bool> ShowBubbleHitPoints { get; set; }
        public static ConfigEntry<bool> ShowBubbleHits { get; set; }

        public static ConfigEntry<float> ShaderVelocity { get; set; }
        public static ConfigEntry<float> ShaderRefraction { get; set; }
        public static ConfigEntry<float> ShaderGlossiness { get; set; }
        public static ConfigEntry<float> ShaderMetallic { get; set; }
        public static ConfigEntry<bool> ShaderTexture { get; set; }

        private static Texture _defaultTexture;

        public void Awake()
        {
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
            Enabled = base.Config.Bind<bool>("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

            EnableBubbleColor = base.Config.Bind<bool>("Color", "Enable Bubble Color", true, "Enable or disable the bubble color");
            BubbleColor = base.Config.Bind<Color>("Color", "Bubble Color", new Color(1, 0, 0, 0.5f));
            
            ShowBubblePercent = base.Config.Bind<bool>("Misc", "Show Bubble Percent", true, "Show remaining bubble integrity");
            ShowBubbleHitPoints = base.Config.Bind<bool>("Misc", "Show Bubble HitPoints", false, "Show remaining bubble hit points");
            ShowBubbleHits = base.Config.Bind<bool>("Misc", "Show Bubble Hits", true, "Show bubble damage taken");
            BubbleColorSelfOnly = base.Config.Bind<bool>("Misc", "Self Only", false, "Do not apply bubble colors to other player/creature bubbles");

            ShaderVelocity = base.Config.Bind<float>("Shader", "ShaderVelocity", 5f, "Wavy Speed");
            ShaderRefraction = base.Config.Bind<float>("Shader", "ShaderRefraction", 0.1f, "Bubble Distortion");
            ShaderGlossiness = base.Config.Bind<float>("Shader", "ShaderGlossiness", 0.8f, "Bubble Glossiness");
            ShaderMetallic = base.Config.Bind<float>("Shader", "ShaderMetallic", 1f, "Bubble Metallic");
            ShaderTexture = base.Config.Bind<bool>("Shader", "DisableTexture", false, "Makes bubble more smoother");

            Config.SettingChanged += UpdateBubble;
        }

        private void UpdateBubble(object sender, SettingChangedEventArgs e)
        {
            if (!Enabled.Value || !EnableBubbleColor.Value)
            {
                return;
            }

            var existingBubble = Player.m_localPlayer.transform.Find("vfx_StaffShield(Clone)");
            if (existingBubble)
            {
                RecolorBubble(existingBubble.gameObject);
            }
        }

        public void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
        
        private static void RecolorBubble(GameObject effect)
        {
            var renderer = effect.GetComponentInChildren<MeshRenderer>();
            if (renderer == null)
            {
                return;
            }

            renderer.material.mainTexture = _defaultTexture ?? (_defaultTexture = renderer.material.mainTexture);
            renderer.material.color = BubbleColor.Value;
            if (ShaderTexture.Value)
            {
                renderer.material.mainTexture = new Texture2D(1, 1);
            }

            renderer.material.SetFloat("_WaveVel", ShaderVelocity.Value);
            renderer.material.SetFloat("_RefractionIntensity", ShaderRefraction.Value);
            renderer.material.SetFloat("_Glossiness", ShaderGlossiness.Value);
            renderer.material.SetFloat("_Metallic", ShaderMetallic.Value);
        }
        
        [HarmonyPatch(typeof(SE_Shield))]
        private static class ShieldPatch
        {
            [HarmonyPatch("OnDamaged")]
            [HarmonyPrefix]
            public static void OnDamagedPrefix(SE_Shield __instance, HitData hit, Character attacker)
            {
                if (Enabled.Value && ShowBubbleHits.Value && hit.GetTotalDamage() > 0)
                    DamageText.instance.ShowText(HitData.DamageModifier.Normal, hit.m_point, hit.GetTotalDamage());
            }

            [HarmonyPatch("Setup")]
            [HarmonyPostfix]
            public static void SetupPostfix(SE_Shield __instance, Character character)
            {
                if (!Enabled.Value || !EnableBubbleColor.Value)
                    return;

                if (BubbleColorSelfOnly.Value && !ReferenceEquals(character, Player.m_localPlayer))
                    return;

                foreach (var effect in __instance.m_startEffectInstances)
                {
                    if (effect.name.StartsWith("vfx_StaffShield"))
                    {
                        RecolorBubble(effect);
                    }
                }
            }

            // I made this but then never used it. maybe in the future.
            public static void LoadTexture(string path, ref Material material)
            {
                var imageData = File.ReadAllBytes(path);
                var texture = new Texture2D(1, 1);
                texture.LoadImage(imageData);
                material.mainTexture = texture;
            }
        }

        [HarmonyPatch(typeof(StatusEffect), "GetIconText")]
        private static class StatusEffectPatch
        {
            public static void Postfix(StatusEffect __instance, ref string __result)
            {
                if (Enabled.Value && __instance is SE_Shield derivedInstance)
                {
                    if (ShowBubblePercent.Value && ShowBubbleHitPoints.Value)
                        __result += "\r\n";

                    if (ShowBubblePercent.Value)
                        __result += $" ({100 - (derivedInstance.m_damage / derivedInstance.m_totalAbsorbDamage) * 100f:##}%)";

                    if (ShowBubbleHitPoints.Value)
                        __result += $" ({derivedInstance.m_totalAbsorbDamage - derivedInstance.m_damage:####})";
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
        public static Color GetRainbowColor(float time, float duration, float alpha)
        {
            var hue = (time % duration) / duration;
            var color = Color.HSVToRGB(hue, 1f, 1f);
            color.a = alpha;
            return color;
        }

        public void Update()
        {
            var rendererMag = GetComponentInChildren<MeshRenderer>();
            rendererMag.material.color = GetRainbowColor(Time.time, 1, Alpha);
        }
    }
}
