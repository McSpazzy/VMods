using BepInEx.Logging;
using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Xml.Linq;

namespace RemoveLava
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInProcess("valheim.exe")]
    public class RemoveLava : BaseUnityPlugin
    {
        public const string PluginGUID = "org.ssmvc.removelava";
        public const string PluginName = "RemoveLava";
        public const string PluginVersion = "1.0.1";

        static ManualLogSource _logger;
        static Harmony _harmony;

        void Awake()
        {
            _logger = Logger;
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
                var comp = heightmap.GetAndCreateTerrainCompiler();

                var width = 65;
                System.Console.WriteLine($"Adding Lava To Heightmap {heightmap.transform.position}");
                var modified = typeof(TerrainComp).GetField("m_modifiedPaint", BindingFlags.NonPublic | BindingFlags.Instance);
                var mask = typeof(TerrainComp).GetField("m_paintMask", BindingFlags.NonPublic | BindingFlags.Instance);

                var modifiedArray = (bool[])modified?.GetValue(comp);
                var maskArray = (Color[])mask?.GetValue(comp);

                if (modifiedArray == null || maskArray == null)
                {
                    continue;
                }

                for (var x = 0; x < width; x++)
                {
                    for (var y = 0; y < width; y++)
                    {
                        var pixelColor = heightmap.GetPaintMask(x, y);
                        pixelColor.a = 1f;
                        var num = x * width + y;
                        modifiedArray[num] = true;
                        maskArray[num] = pixelColor;
                    }
                }

                var saveFunc = typeof(TerrainComp).GetMethod("Save", BindingFlags.NonPublic | BindingFlags.Instance);
                saveFunc?.Invoke(comp, null);
                heightmap.Poke(false);
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
                var comp = heightmap.GetAndCreateTerrainCompiler();

                var width = 65;
                System.Console.WriteLine($"Removing Lava From Heightmap {heightmap.transform.position}");
                var modified = typeof(TerrainComp).GetField("m_modifiedPaint", BindingFlags.NonPublic | BindingFlags.Instance);
                var mask = typeof(TerrainComp).GetField("m_paintMask", BindingFlags.NonPublic | BindingFlags.Instance);

                var modifiedArray = (bool[])modified?.GetValue(comp);
                var maskArray = (Color[])mask?.GetValue(comp);

                if (modifiedArray == null || maskArray == null)
                {
                    continue;
                }

                for (var x = 0; x < width; x++)
                {
                    for (var y = 0; y < width; y++)
                    {
                        var pixelColor = heightmap.GetPaintMask(x, y);
                        pixelColor.a = 0f;
                        var num = x * width + y;
                        modifiedArray[num] = true;
                        maskArray[num] = pixelColor;
                    }
                }

                var saveFunc = typeof(TerrainComp).GetMethod("Save", BindingFlags.NonPublic | BindingFlags.Instance);
                saveFunc?.Invoke(comp, null);
                heightmap.Poke(false);
            }
        }
    }
}
