using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using System.ComponentModel;

namespace BlankMod
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInProcess("valheim.exe")]
    public class BlankMod : BaseUnityPlugin
    {
        public const string PluginGUID = "org.ssmvc.blank";
        public const string PluginName = "BlankMod";
        public const string PluginVersion = "1.0.0";

        private static Harmony _harmony;

        public void Awake()
        {
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
        }

        public void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }  
    }
}
