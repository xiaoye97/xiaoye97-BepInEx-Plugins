using Oc;
using System;
using BepInEx;
using HarmonyLib;
using Oc.Item.UI;
using BepInEx.Logging;

namespace BigInventory
{
    [BepInPlugin("com.github.xiaoye97.plugin.Craftopia.BigInventory", "BigInventory", "1.2")]
    public class BigInventory : BaseUnityPlugin
    {
        public static int InventorySize = 64;
        public static ManualLogSource logger;

        private void Start()
        {
            logger = Logger;
            Harmony.CreateAndPatchAll(typeof(BigInventory));
            Logger.LogInfo("BigInventory Start");
        }

        [HarmonyPrefix, HarmonyPatch(typeof(OcItemUI_Cell_List), "InitOnAwake", new Type[] { typeof(int) })]
        public static bool GamePatch(OcItemUI_Cell_List __instance, ref int size)
        {
            OcItemUI_InventoryMng instUISS2 = UISceneSingleton<OcItemUI_InventoryMng>.InstUISS;
            if ((int)__instance.itemType < instUISS2.advancedExtendInventory.Length)
            {
                logger.LogInfo($"将{__instance.name} {__instance.itemType}的初始化尺寸从{size}设为{InventorySize}");
                size = InventorySize;
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(OcItemUI_Cell_List), "InitOnAwake", new Type[] { })]
        public static bool GamePatch2(OcItemUI_Cell_List __instance)
        {
            var _this = __instance;
            if (!_this.list.IsNullOrEmpty<OcItemUI_Cell>())
            {
                if (_this.list.Count < _this.Size)
                {
                    logger.LogInfo($"{__instance.name} {__instance.itemType}的list.Count {_this.list.Count} < Size {_this.Size}，进行补充");
                    for (int i = _this.list.Count; i < _this.Size; i++)
                    {
                        _this.list.Add(_this.InstCell(i));
                    }
                }
            }
            return true;
        }
    }
}