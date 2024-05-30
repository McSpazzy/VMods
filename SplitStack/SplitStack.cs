using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace SplitStack
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInProcess("valheim.exe")]
    public class SplitStack : BaseUnityPlugin
    {
        public const string PluginGUID = "org.ssmvc.splitstack";
        public const string PluginName = "SplitStack";
        public const string PluginVersion = "1.0.0";

        private static Harmony _harmony;

        private static bool _splitNStacks = false;
        private static bool _splitStacksN = false;

        public static ConfigEntry<bool> Enabled { get; set; }
        public static ConfigEntry<KeyCode> SplitNStacksKey { get; set; }
        public static ConfigEntry<KeyCode> SplitStacksNKey { get; set; }

        public void Awake()
        {
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
            Enabled = Config.Bind<bool>("_Global", "isModEnabled", true, "Globally enable or disable this mod.");
            SplitNStacksKey = Config.Bind<KeyCode>("Keys", "Split N Stacks Key", KeyCode.LeftControl, "Key for split modifier count. n Stacks.");
            SplitStacksNKey = Config.Bind<KeyCode>("Keys", "Split Stacks N Key", KeyCode.LeftAlt, "Key for split modifier quantity. Stacks of n quantity.");
        }

        public void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }

        [HarmonyPatch(typeof(InventoryGui), "OnSelectedItem")]
        private static class OnSelectedItemPatch
        {
            public static bool Prefix(InventoryGui __instance, InventoryGrid grid, ItemDrop.ItemData item, Vector2i pos, InventoryGrid.Modifier mod)
            {
                if (!Enabled.Value)
                {
                    return true;
                }

                var localPlayer = Player.m_localPlayer;
                if (localPlayer.IsTeleporting())
                {
                    return false;
                }

                if (mod != InventoryGrid.Modifier.Split || item.m_stack <= 1)
                {
                    return true;
                }

                _splitNStacks = ZInput.GetKey(SplitNStacksKey.Value);
                _splitStacksN = ZInput.GetKey(SplitStacksNKey.Value);

                if (!_splitNStacks && !_splitStacksN)
                {
                    return true;
                }

                var emptySlots = grid.GetInventory().GetEmptySlots();

                if (emptySlots == 0)
                {
                    __instance.SetupDragItem(item, grid.GetInventory(), item.m_stack);
                    return false;
                }

                if (_splitNStacks)
                {
                    __instance.m_splitSlider.maxValue = Math.Min(item.m_stack, emptySlots + 1);
                    __instance.m_splitSlider.minValue = 1;
                }

                if (_splitStacksN)
                {
                    __instance.m_splitSlider.maxValue = item.m_stack;
                    if (emptySlots >= item.m_stack - 1)
                    {
                        __instance.m_splitSlider.minValue = 1;
                    }
                    else
                    {
                        __instance.m_splitSlider.minValue = (int)(item.m_stack / (emptySlots + 1));
                    }
                }

                __instance.m_splitSlider.value = __instance.m_splitSlider.minValue;
                __instance.m_splitIcon.sprite = item.GetIcon();
                __instance.m_splitIconName.text = Localization.instance.Localize(item.m_shared.m_name);
                __instance.m_splitItem = item;
                __instance.m_splitInventory = grid.GetInventory();
                __instance.OnSplitSliderChanged(__instance.m_splitSlider.value);
                __instance.m_splitPanel.gameObject.SetActive(true);

                return false;
            }
        }

        [HarmonyPatch(typeof(InventoryGui), "OnSplitOk")]
        private static class OnSplitOkPatch
        {
            public static bool Prefix(InventoryGui __instance)
            {
                if (!_splitNStacks && !_splitStacksN)
                {
                    return true;
                }

                var val = (int) __instance.m_splitSlider.value;

                // Split into stacks of N
                if (_splitStacksN)
                {
                    if (val == __instance.m_splitItem.m_stack) // Selected value same as stack size
                    {
                        Close(__instance);
                        return false;
                    }

                    while (__instance.m_splitItem.m_stack > val && __instance.m_splitInventory.GetEmptySlots() > 0)
                    {
                        var spot = __instance.m_splitInventory.FindEmptySlot(true);
                        __instance.m_splitInventory.AddItem(__instance.m_splitItem.Clone(), val, spot.x, spot.y);
                        __instance.m_splitItem.m_stack -= val;
                    }
                }

                // Split into N stacks of
                if (_splitNStacks)
                {
                    if (val == 1) // Selected value is one stack
                    {
                        Close(__instance);
                        return false;
                    }

                    var stackAmount = __instance.m_splitItem.m_stack / val;
                    var diff = __instance.m_splitItem.m_stack - stackAmount * val;

                    __instance.m_splitItem.m_stack = stackAmount + diff;

                    for (var i = 0; i < val - 1; i++)
                    {
                        var spot = __instance.m_splitInventory.FindEmptySlot(true);
                        __instance.m_splitInventory.AddItem(__instance.m_splitItem.Clone(), stackAmount, spot.x, spot.y);
                    }
                }

                if (__instance.m_currentContainer)
                {
                    __instance.m_currentContainer.Save();
                }

                Close(__instance);
                return false;
            }

            private static void Close(InventoryGui __instance)
            {
                _splitNStacks = false;
                _splitStacksN = false;
                __instance.m_splitItem = null;
                __instance.m_splitInventory = null;
                __instance.m_splitPanel.gameObject.SetActive(false);
            }
        }

        [HarmonyPatch(typeof(InventoryGui), "OnSplitSliderChanged")]
        private static class OnSplitSliderChangedPatch
        {
            public static bool Prefix(InventoryGui __instance, float value)
            {
                if (!_splitNStacks && !_splitStacksN)
                {
                    return true;
                }

                if (_splitStacksN)
                {
                    __instance.m_splitAmount.text = $"Split into stacks of {value}.";
                    if (__instance.m_splitSlider.minValue > 1)
                    {
                        __instance.m_splitAmount.text += $" Min {__instance.m_splitSlider.minValue}";
                    }
                }
                else if (_splitNStacks)
                {
                    __instance.m_splitAmount.text = $"Split into {value}/{__instance.m_splitSlider.maxValue} stacks.";
                }

                return false;
            }
        }
    }
}
