using BepInEx;
using HarmonyLib;
using Oc;
using Oc.Item.UI;

namespace BigInventory
{
    [BepInPlugin("com.github.xiaoye97.plugin.Craftopia.BigInventory", "BigInventory", "1.0")]
    public class BigInventory : BaseUnityPlugin
    {
        public static int InventorySize = 64;

        private void Start()
        {
            Harmony.CreateAndPatchAll(typeof(BigInventory));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(OcItemUI_Cell_List_Inventory), "ConvertFromSaveData")]
        public static void GamePatch(OcItemUI_Cell_List_Inventory __instance)
        {
            OcItemUI_InventoryMng instUISS2 = UISceneSingleton<OcItemUI_InventoryMng>.InstUISS;
            if ((int)__instance.itemType < instUISS2.advancedExtendInventory.Length)
            {
                if (__instance.Size < InventorySize)
                {
                    Debug.Log($"将背包{__instance.name}({__instance.itemType})的大小从到{__instance.Size}增加到{InventorySize}");
                    instUISS2.IncreaseListSize(__instance.itemType, InventorySize - __instance.Size, true);
                }
                else
                {
                    Debug.Log($"背包{__instance.name}({__instance.itemType})的大小不需要变化");
                }
            }
        }
    }
}