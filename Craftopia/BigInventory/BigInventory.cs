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

        [HarmonyPostfix, HarmonyPatch(typeof(OcItemUI_Cell_List_Inventory), "ConvertFromSaveData")]
        public static void GamePatch(OcItemUI_Cell_List_Inventory __instance)
        {
            OcItemUI_InventoryMng instUISS2 = UISceneSingleton<OcItemUI_InventoryMng>.InstUISS;
            if ((int)__instance.itemType < instUISS2.advancedExtendInventory.Length)
            {
                if (__instance.Size < InventorySize)
                {
                    logger.LogInfo($"将背包{__instance.name}({__instance.itemType})的大小从到{__instance.Size}增加到{InventorySize}");
                    instUISS2.IncreaseListSize(__instance.itemType, InventorySize - __instance.Size, false);
                }
                else
                {
                    logger.LogInfo($"背包{__instance.name}({__instance.itemType})的大小不需要变化");
                }
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(OcItemUI_Cell_List), "InitOnAwake", new Type[] { typeof(int) })]
        public static bool GamePatch2(OcItemUI_Cell_List __instance, ref int size)
        {
            OcItemUI_InventoryMng instUISS2 = UISceneSingleton<OcItemUI_InventoryMng>.InstUISS;
            if ((int)__instance.itemType < instUISS2.advancedExtendInventory.Length)
            {
                logger.LogInfo($"将{__instance.name} {__instance.itemType}的初始化尺寸从{size}设为{InventorySize}");
                size = InventorySize;
            }
            return true;
        }
    }
}