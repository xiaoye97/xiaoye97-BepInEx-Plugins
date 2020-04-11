using RoR2;
using RoR2.UI;
using BepInEx;
using HarmonyLib;

namespace xiaoye97
{
    [BepInPlugin("me.xiaoye97.plugin.ShowItemDescriptionInTab", "在背包中显示物品描述", "1.0")]
    public class ShowItemDescriptionInTab : BaseUnityPlugin
    {
        void Start()
        {
            new Harmony("me.xiaoye97.plugin.ShowItemDescriptionInTab").PatchAll();
        }

        [HarmonyPatch(typeof(ItemIcon), "SetItemIndex")]
        class DescPatch
        {
            public static void Postfix(ItemIcon __instance)
            {
                if(__instance.tooltipProvider)
                {
                    __instance.tooltipProvider.bodyToken = ItemCatalog.GetItemDef(Traverse.Create(__instance).Field("itemIndex").GetValue<ItemIndex>()).descriptionToken;
                }
            }
        }
    }
}
