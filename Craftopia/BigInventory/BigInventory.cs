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
            Debug.Log($"设置背包{__instance.name}的大小到{InventorySize}");
            __instance.SetSize(InventorySize);
        }
    }
}