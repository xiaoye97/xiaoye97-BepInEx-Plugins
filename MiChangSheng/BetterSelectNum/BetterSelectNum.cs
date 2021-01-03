using System;
using BepInEx;
using HarmonyLib;
using GUIPackage;
using BepInEx.Configuration;

namespace BetterSelectNum
{
    [BepInPlugin("me.xiaoye97.plugin.MiChangSheng.BetterSelectNum", "更好的数量选择", "1.0")]
    public class BetterSelectNum : BaseUnityPlugin
    {
        public static ConfigEntry<bool> MaxMode;
        void Start()
        {
            MaxMode = Config.Bind<bool>("config", "MaxMode", true, "最大数量模式，为true时每次选择物品会从最大数量开始，为false时会从1开始");
            Harmony.CreateAndPatchAll(typeof(BetterSelectNum));
        }

        /// <summary>
        /// 数量选择优化，数量为1时，较少会变成最大值，数量为最大值时，增加会变成1
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(UI_chaifen), "InputOnChenge")]
        public static bool SelectNumFix(UI_chaifen __instance)
        {
            try
            {
                int num = int.Parse(__instance.inputNum.value);
                if (num < 1)
                {
                    __instance.inputNum.value = string.Concat(__instance.Item.itemNum);
                }
                else if (__instance.Item.itemNum < num)
                {
                    __instance.inputNum.value = "1";
                }
            }
            catch (Exception)
            {
                __instance.inputNum.value = "1";
            }
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ItemCellEX), "setUI_chifen")]
        public static bool ItemNumFix1(ItemCellEX __instance)
        {
            if(MaxMode.Value)
            {
                var item = __instance.inventory.dragedItem.Clone();
                selectNum.instence.gameObject.GetComponent<UI_chaifen>().Item = item;
                selectNum.instence.gameObject.GetComponent<UI_chaifen>().inputNum.value = string.Concat(item.itemNum);
                return false;
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(itemcellShopEx), "EXsetUI_chifen")]
        public static bool ItemNumFix2(itemcellShopEx __instance)
        {
            if (MaxMode.Value)
            {
                var item = __instance.inventory.inventory[int.Parse(__instance.name)].Clone();
                selectNum.instence.gameObject.GetComponent<UI_chaifen>().Item = item;
                selectNum.instence.gameObject.GetComponent<UI_chaifen>().Item.itemNum = 9999;
                selectNum.instence.gameObject.GetComponent<UI_chaifen>().inputNum.value = string.Concat(9999);
                return false;
            }
            return true;
        }
    }
}
