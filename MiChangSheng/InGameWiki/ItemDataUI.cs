using System;
using UnityEngine;
using System.Collections.Generic;

namespace InGameWiki
{
    public static class ItemDataUI
    {
        static Vector2 svPos, exSvPos, ex2SvPos;
        public static void OnGUI()
        {
            GUILayout.BeginHorizontal("物品详情", GUI.skin.window, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            if (ItemSearchUI.SelectItem != null)
            {
                GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(120));
                Texture2D icon = ModTool.GetIcon(ItemSearchUI.SelectItem);
                if (icon != null)
                {
                    GUILayout.Label(icon);
                }
                else
                {
                    GUILayout.Label("此物品无图标");
                }
                if (InGameWiki.ShowJSON.Value)
                {
                    if (GUILayout.Button("显示JSON数据"))
                    {
                        JsonUI.Json = ItemSearchUI.SelectItem;
                    }
                }
                GUILayout.EndVertical();
            }
            svPos = GUILayout.BeginScrollView(svPos, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            if (ItemSearchUI.SelectItem == null)
            {
                GUILayout.Label("当前未选中物品");
            }
            else
            {
                GUI.contentColor = ItemSearchUI.SelectItem.GetQualityColor();
                GUILayout.Label($"物品名称: {ItemSearchUI.SelectItem["name"].str.UnCode64()}");
                GUI.contentColor = Color.white;
                GUILayout.Label($"ID: {ItemSearchUI.SelectItem["id"].I}");
                GUILayout.Label($"品质: {ItemSearchUI.SelectItem["quality"].I}品");
                GUILayout.Label($"类别: {Tools.getStr("ItemType" + (int)ItemSearchUI.SelectItem["type"].n)}");
                GUILayout.Label($"简介: {ItemSearchUI.SelectItem["desc"].str.UnCode64()}");
                GUILayout.Label($"描述: {ItemSearchUI.SelectItem["desc2"].str.UnCode64()}");
                GUILayout.Label($"最大堆叠: { ItemSearchUI.SelectItem["maxNum"].I }");
                GUILayout.Label($"价格: { ItemSearchUI.SelectItem["price"].I }");
                switch ((int)ItemSearchUI.SelectItem["type"].n)
                {
                    case 6: //药材
                        GUILayout.Label("【药性】");
                        GUILayout.Label($"用于主药: {Tools.getLiDanLeiXinStr((int)ItemSearchUI.SelectItem["yaoZhi2"].n)}");
                        GUILayout.Label($"用于辅药: {Tools.getLiDanLeiXinStr((int)ItemSearchUI.SelectItem["yaoZhi3"].n)}");
                        GUILayout.Label($"用于药引: {Tools.getLiDanLeiXinStr((int)ItemSearchUI.SelectItem["yaoZhi1"].n)}");
                        break;
                }
            }
            GUILayout.EndScrollView();
            //联动数据1
            if (ItemSearchUI.SelectItem != null)
            {
                switch ((int)ItemSearchUI.SelectItem["type"].n)
                {
                    case 5: //丹药
                        DanYaoUI(ItemSearchUI.SelectItem);
                        break;
                    case 6: //药材
                        YaoCaiUI(ItemSearchUI.SelectItem);
                        break;
                }
            }
            //联动数据 - 采集地数据
            if (ItemSearchUI.SelectItem != null)
            {
                //6药材 8材料
                if ((int)ItemSearchUI.SelectItem["type"].n == 6 || (int)ItemSearchUI.SelectItem["type"].n == 8)
                {
                    CaiJiDiUI(ItemSearchUI.SelectItem);
                }
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 联动数据用的Label，带跳转按钮
        /// </summary>
        static void LianDongLabel(string label, JSONObject item)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(60));
            GUI.contentColor = item.GetQualityColor();
            if (GUILayout.Button(item["name"].str.UnCode64(), GUILayout.Width(100)))
            {
                ItemSearchUI.SelectItem = item;
            }
            GUI.contentColor = Color.white;
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 联动数据 - 丹方数据UI
        /// </summary>
        static void DanFangDataUI(JSONObject df)
        {
            JSONObject danYao = df["ItemID"].I.ItemJson();
            JSONObject yaoYin = df["value1"].I.ItemJson();
            JSONObject zhuYao1 = df["value2"].I.ItemJson();
            JSONObject zhuYao2 = df["value3"].I.ItemJson();
            JSONObject fuYao1 = df["value4"].I.ItemJson();
            JSONObject fuYao2 = df["value5"].I.ItemJson();
            GUI.contentColor = danYao.GetQualityColor();
            GUILayout.BeginVertical($"联动数据: 丹方 {df["name"].str.UnCode64()}", GUI.skin.window);
            GUI.contentColor = Color.white;
            LianDongLabel("丹药", danYao);
            GUILayout.Label($"用途: {danYao["desc"].str.UnCode64()}");
            if (df["value1"].I != 0) LianDongLabel($"药引  {df["num1"].I}x", yaoYin);
            if (df["value2"].I != 0) LianDongLabel($"主药  {df["num2"].I}x", zhuYao1);
            if (df["value3"].I != 0) LianDongLabel($"主药2 {df["num3"].I}x", zhuYao2);
            if (df["value4"].I != 0) LianDongLabel($"辅药  {df["num4"].I}x", fuYao1);
            if (df["value5"].I != 0) LianDongLabel($"辅药2 {df["num5"].I}x", fuYao2);
            GUILayout.Label($"花费时间: {df["castTime"].I}天");
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 丹药UI
        /// </summary>
        static void DanYaoUI(JSONObject item)
        {
            int id = item["id"].I;
            List<JSONObject> dfList = new List<JSONObject>();
            foreach (var df in jsonData.instance.LianDanDanFangBiao.list)
            {
                if (df["ItemID"].I == id)
                {
                    dfList.Add(df);
                }
            }
            if (dfList.Count > 0)
            {
                exSvPos = GUILayout.BeginScrollView(exSvPos, GUILayout.ExpandHeight(true), GUILayout.Width(300));
                foreach (var df in dfList)
                {
                    DanFangDataUI(df);
                }
                GUILayout.EndScrollView();
            }
        }

        /// <summary>
        /// 药材UI
        /// </summary>
        static void YaoCaiUI(JSONObject item)
        {
            int id = item["id"].I;
            List<JSONObject> dfList = new List<JSONObject>();
            foreach (var df in jsonData.instance.LianDanDanFangBiao.list)
            {
                for (int i = 1; i <= 5; i++)
                {
                    if (df[$"value{i}"].I == id)
                    {
                        dfList.Add(df);
                        continue;
                    }
                }
            }
            if (dfList.Count > 0)
            {
                exSvPos = GUILayout.BeginScrollView(exSvPos, GUILayout.ExpandHeight(true), GUILayout.Width(300));
                foreach (var df in dfList)
                {
                    DanFangDataUI(df);
                }
                GUILayout.EndScrollView();
            }
        }

        /// <summary>
        /// 联动数据 - 采集地UI
        /// </summary>
        static void CaiJiDiDataUI(JSONObject caijidi)
        {
            List<JSONObject> items = new List<JSONObject>();
            for (int i = 1; i <= 8; i++)
            {
                if (caijidi[$"value{i}"].I != 0)
                {
                    items.Add(caijidi[$"value{i}"].I.ItemJson());
                }
            }
            if (items.Count > 0)
            {
                GUILayout.BeginVertical($"联动数据: 采集地 {caijidi["name"].str.UnCode64()}", GUI.skin.window);
                foreach (var item in items)
                {
                    LianDongLabel("采集物", item);
                }
                GUILayout.EndVertical();
            }
        }

        /// <summary>
        /// 采集地UI
        /// </summary>
        static void CaiJiDiUI(JSONObject item)
        {
            int id = item["id"].I;
            List<JSONObject> caijidiList = new List<JSONObject>();
            foreach (var caijidi in jsonData.instance.CaiYaoDiaoLuo.list)
            {
                for (int i = 1; i <= 8; i++)
                {
                    if (caijidi[$"value{i}"].I == id)
                    {
                        caijidiList.Add(caijidi);
                        continue;
                    }
                }
            }
            if (caijidiList.Count > 0)
            {
                ex2SvPos = GUILayout.BeginScrollView(ex2SvPos, GUILayout.ExpandHeight(true), GUILayout.Width(300));
                foreach (var caijidi in caijidiList)
                {
                    CaiJiDiDataUI(caijidi);
                }
                GUILayout.EndScrollView();
            }
        }
    }
}
