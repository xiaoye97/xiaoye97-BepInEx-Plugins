using Oc;
using BepInEx;
using Oc.Item.UI;
using HarmonyLib;

namespace BigInventory
{
    [BepInPlugin("me.xiaoye97.plugin.Craftopia.BigInventory", "大背包", "1.1")]
    public class BigInventory : BaseUnityPlugin
    {
        public static int InventorySize = 64;

        void Start()
        {
            new Harmony("me.xiaoye97.plugin.Craftopia.BigInventory").PatchAll();
        }

        [HarmonyPatch(typeof(OcGameMng), "OnGameSceneSetUpFinish")]
        class InventoryPatch
        {
            public static void Postfix()
            {
                FixInventory();
            }
        }

        public static void FixInventory()
        {
            var inst = SingletonMonoBehaviour<OcItemUI_InventoryMng>.Inst;
            if (inst != null)
            {
                Traverse.Create(inst).Field("equipmentList").Method("SetSize", 64).GetValue();
                Traverse.Create(inst).Field("consumptionList").Method("SetSize", 64).GetValue();
                Traverse.Create(inst).Field("materialList").Method("SetSize", 64).GetValue();
                Traverse.Create(inst).Field("relicList").Method("SetSize", 64).GetValue();
                Traverse.Create(inst).Field("buildingList").Method("SetSize", 64).GetValue();
            }
        }
    }
}
