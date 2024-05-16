using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace RemoveLava
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInProcess("valheim.exe")]
    public class RemoveLava : BaseUnityPlugin
    {
        public const string PluginGUID = "org.ssmvc.removelava";
        public const string PluginName = "RemoveLava";
        public const string PluginVersion = "1.0.2";

        static Harmony _harmony;

        void Awake()
        {
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);

            var removeLavaCommand = new Terminal.ConsoleCommand("removelava", "Remove Lava", new Terminal.ConsoleEvent(RemoveLavaFunc));
            var addLavaCommand = new Terminal.ConsoleCommand("floorislava", "Floor Is Lava", new Terminal.ConsoleEvent(FloorIsLavaFunc));
        }

        void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
        
        public static void FloorIsLavaFunc(Terminal.ConsoleEventArgs args)
        {
            if (args.Length < 2)
            {
                return;
            }

            var radius = Convert.ToInt32(args[1]);

            var list = new List<Heightmap>();
            Heightmap.FindHeightmap(Player.m_localPlayer.transform.position, radius, list);
            foreach (var heightmap in list)
            {
                SetHeightmapAlpha(heightmap, 1f);
            }
        }

        public static void RemoveLavaFunc(Terminal.ConsoleEventArgs args)
        {
            if (args.Length < 2)
            {
                return;
            }

            var radius = Convert.ToInt32(args[1]);

            var list = new List<Heightmap>();
            Heightmap.FindHeightmap(Player.m_localPlayer.transform.position, radius, list);
            foreach (var heightmap in list)
            {
                SetHeightmapAlpha(heightmap, 0f);
            }
        }

        public static void SetHeightmapAlpha(Heightmap heightmap, float value)
        {
            var comp = heightmap.GetAndCreateTerrainCompiler();

            var width = 65;
            System.Console.WriteLine($"Removing Lava From Heightmap {heightmap.transform.position}");
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < width; y++)
                {
                    var pixelColor = heightmap.GetPaintMask(x, y);
                    var num = x * width + y;
                    pixelColor.a = value;
                    comp.m_modifiedPaint[num] = true;
                    comp.m_paintMask[num] = pixelColor;
                }
            }

            comp.Save();
            heightmap.Poke(false);
        }
    }
}
