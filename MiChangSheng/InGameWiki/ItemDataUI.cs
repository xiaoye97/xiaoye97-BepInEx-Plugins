using System;
using UnityEngine;
using MCSDataHelper;
using System.Collections.Generic;

namespace InGameWiki
{
    public static class ItemDataUI
    {
        private static Vector2 svPos, exSvPos, ex2SvPos;

        public static void OnGUI()
        {
            GUILayout.BeginHorizontal("物品详情", GUI.skin.window, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            if (ItemSearchUI.SelectItem != null)
            {
                GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(120));
                Texture2D icon = DataHelper.GetIcon(ItemSearchUI.SelectItemJson);
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
                        JsonUI.Json = ItemSearchUI.SelectItemJson;
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
                GUILayout.Label($"物品名称: {ItemSearchUI.SelectItem.name}");
                GUI.contentColor = Color.white;
                GUILayout.Label($"ID: {ItemSearchUI.SelectItem.id}");
                GUILayout.Label($"品质: {ItemSearchUI.SelectItem.quality}品");
                GUILayout.Label($"类别: {Tools.getStr("ItemType" + ItemSearchUI.SelectItem.type)}");
                GUILayout.Label($"简介: {ItemSearchUI.SelectItem.desc}");
                GUILayout.Label($"描述: {ItemSearchUI.SelectItem.desc2}");
                GUILayout.Label($"最大堆叠: { ItemSearchUI.SelectItem.maxNum }");
                GUILayout.Label($"价格: { ItemSearchUI.SelectItem.price }");
                switch (ItemSearchUI.SelectItem.type)
                {
                    case 6: // 药材
                        GUILayout.Label("【药性】");
                        GUILayout.Label($"用于主药: {Tools.getLiDanLeiXinStr(ItemSearchUI.SelectItem.yaoZhi2)}");
                        GUILayout.Label($"用于辅药: {Tools.getLiDanLeiXinStr(ItemSearchUI.SelectItem.yaoZhi3)}");
                        GUILayout.Label($"用于药引: {Tools.getLiDanLeiXinStr(ItemSearchUI.SelectItem.yaoZhi1)}");
                        break;
                }
            }
            GUILayout.EndScrollView();
            // 联动数据1
            if (ItemSearchUI.SelectItem != null)
            {
                switch (ItemSearchUI.SelectItem.type)
                {
                    case 5: // 丹药
                        DanYaoUI(ItemSearchUI.SelectItem);
                        break;

                    case 6: // 药材
                        YaoCaiUI(ItemSearchUI.SelectItem);
                        break;
                }
            }
            //联动数据 - 采集地数据
            if (ItemSearchUI.SelectItem != null)
            {
                //6药材 8材料
                if (ItemSearchUI.SelectItem.type == 6 || ItemSearchUI.SelectItem.type == 8)
                {
                    CaiJiDiUI(ItemSearchUI.SelectItem);
                }
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 联动数据用的Label，带跳转按钮
        /// </summary>
        private static void LianDongLabel(string label, JSONClass._ItemJsonData item)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(60));
            GUI.contentColor = item.GetQualityColor();
            if (GUILayout.Button(item.name, GUILayout.Width(100)))
            {
                ItemSearchUI.SelectItem = item;
            }
            GUI.contentColor = Color.white;
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 联动数据 - 丹方数据UI
        /// </summary>
        private static void DanFangDataUI(JSONObject df)
        {
            JSONClass._ItemJsonData danYao = JSONClass._ItemJsonData.DataDict[df["ItemID"].I];
            GUI.contentColor = danYao.GetQualityColor();
            GUILayout.BeginVertical($"联动数据: 丹方 {df["name"].Str}", GUI.skin.window);
            GUI.contentColor = Color.white;
            LianDongLabel("丹药", danYao);
            GUILayout.Label($"用途: {danYao.desc}");
            if (df["value1"].I != 0)
            {
                LianDongLabel($"药引  {df["num1"].I}x", JSONClass._ItemJsonData.DataDict[df["value1"].I]);
            }
            if (df["value2"].I != 0)
            {
                LianDongLabel($"主药  {df["num2"].I}x", JSONClass._ItemJsonData.DataDict[df["value2"].I]);
            }
            if (df["value3"].I != 0)
            {
                LianDongLabel($"主药2 {df["num3"].I}x", JSONClass._ItemJsonData.DataDict[df["value3"].I]);
            }
            if (df["value4"].I != 0)
            {
                LianDongLabel($"辅药  {df["num4"].I}x", JSONClass._ItemJsonData.DataDict[df["value4"].I]);
            }
            if (df["value5"].I != 0)
            {
                LianDongLabel($"辅药2 {df["num5"].I}x", JSONClass._ItemJsonData.DataDict[df["value5"].I]);
            }
            GUILayout.Label($"花费时间: {df["castTime"].I}天");
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 丹药UI
        /// </summary>
        private static void DanYaoUI(JSONClass._ItemJsonData item)
        {
            List<JSONObject> dfList = new List<JSONObject>();
            foreach (var df in jsonData.instance.LianDanDanFangBiao.list)
            {
                if (df["ItemID"].I == item.id)
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
        private static void YaoCaiUI(JSONClass._ItemJsonData item)
        {
            List<JSONObject> dfList = new List<JSONObject>();
            foreach (var df in jsonData.instance.LianDanDanFangBiao.list)
            {
                for (int i = 1; i <= 5; i++)
                {
                    if (df[$"value{i}"].I == item.id)
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
        private static void CaiJiDiDataUI(JSONObject caijidi)
        {
            List<JSONClass._ItemJsonData> items = new List<JSONClass._ItemJsonData>();
            for (int i = 1; i <= 8; i++)
            {
                if (caijidi[$"value{i}"].I != 0)
                {
                    items.Add(JSONClass._ItemJsonData.DataDict[caijidi[$"value{i}"].I]);
                }
            }
            if (items.Count > 0)
            {
                GUILayout.BeginVertical($"联动数据: 采集地 {caijidi["name"].Str}", GUI.skin.window);
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
        private static void CaiJiDiUI(JSONClass._ItemJsonData item)
        {
            List<JSONObject> caijidiList = new List<JSONObject>();
            foreach (var caijidi in jsonData.instance.CaiYaoDiaoLuo.list)
            {
                for (int i = 1; i <= 8; i++)
                {
                    if (caijidi[$"value{i}"].I == item.id)
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