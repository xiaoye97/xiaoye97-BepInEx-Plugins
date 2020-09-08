using System;
using BepInEx;
using Oc.Item;
using Oc.Item.UI;
using System.Collections.Generic;

namespace BigInventory
{
    [BepInPlugin("me.xiaoye97.plugin.Craftopia.BigInventory", "大背包", "1.0")]
    public class BigInventory : BaseUnityPlugin
    {
        public static bool finished = false;

        void Update()
        {
            if (finished) return;
			if (SingletonMonoBehaviour<OcItemUI_InventoryMng>.Inst != null)
			{
				finished = true;
				OcItemUI_InventoryMng inst = SingletonMonoBehaviour<OcItemUI_InventoryMng>.Inst;
				List<OcItemUI_Cell_List> list = new List<OcItemUI_Cell_List>();
				foreach (OcItemStack stack in inst.GetStacksNotNull(true))
				{
					list.Add(inst.GetBelongList(stack));
				}
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] != null && list[i].Size != 64)
					{
						list[i].SetSize(64);
					}
				}
			}
		}
    }
}
