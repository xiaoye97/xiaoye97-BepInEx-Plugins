using System;
using UnityEngine;
using System.Collections.Generic;

namespace InGameWiki
{
    public static class ItemSearchUI
    {
        static string searchStr;
        static string[] qualityStrs = new string[] { "全部", "一品", "二品", "三品", "四品", "五品", "六品" };
        static string[] typeStrs = new string[] { "全部", "武器", "防具", "饰品", "技能书", "功法书", "丹药", "药材", "任务道具", "材料", "丹炉", "丹方", "药渣", "书籍", "书籍", "灵舟", "秘药", "其他" };
        static int selectQuality, selectType, nowPage, maxPage, tmpPage, tmpShow;
        static List<KeyValuePair<string, JSONObject>> ItemList = new List<KeyValuePair<string, JSONObject>>();
        static Vector2 svPos;
        public static JSONObject SelectItem;

        /// <summary>
        /// 物品是否为目标品质
        /// </summary>
        static bool IsTargetQuality(JSONObject itemdata)
        {
            if (selectQuality > 0)
            {
                if (selectQuality != itemdata["quality"].I)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 物品是否为目标种类
        /// </summary>
        static bool IsTargetType(JSONObject itemdata)
        {
            if (selectType > 0)
            {
                if (selectType - 1 != itemdata["type"].I)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 物品是否包含关键词
        /// </summary>
        static bool ContainsSearch(JSONObject itemdata)
        {
            if (!IsTargetQuality(itemdata)) return false;
            if (!IsTargetType(itemdata)) return false;
            if (string.IsNullOrWhiteSpace(searchStr)) return true;
            string[] searchs = searchStr.Split(' ');
            bool result = true;
            foreach (var search in searchs)
            {
                if (!itemdata["name"].str.UnCode64().Contains(search) && !itemdata["desc"].str.UnCode64().Contains(search) && !itemdata["id"].I.ToString().Contains(search))
                {
                    result = false;
                }
            }
            return result;
        }

        /// <summary>
        /// 根据关键词搜索物品
        /// </summary>
        static void SearchItems()
        {
            ItemList.Clear();
            foreach (var kv in jsonData.instance.ItemJsonData)
            {
                if (ContainsSearch(kv.Value))
                {
                    ItemList.Add(new KeyValuePair<string, JSONObject>(kv.Key, kv.Value));
                }
            }
        }

        public static void OnGUI()
        {
            GUILayout.BeginVertical("物品搜索", GUI.skin.window, GUILayout.Height(300), GUILayout.ExpandWidth(true));
            #region 过滤及翻页
            GUILayout.BeginHorizontal(GUI.skin.box);
            //种类
            GUILayout.Label("种类", GUILayout.Width(30));
            selectType = GUILayout.SelectionGrid(selectType, typeStrs, 18);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUI.skin.box);
            //品级
            GUILayout.Label("品级", GUILayout.Width(30));
            selectQuality = GUILayout.SelectionGrid(selectQuality, qualityStrs, 7, GUILayout.Width(350));
            GUILayout.Space(20);

            //关键词
            GUILayout.Label("关键词", GUILayout.Width(45));
            searchStr = GUILayout.TextField(searchStr, GUILayout.Width(100));
            GUILayout.Space(20);

            //页数
            SearchItems();
            maxPage = ItemList.Count / 30;
            if (ItemList.Count % 30 != 0) maxPage++;
            GUILayout.Label($"  第{nowPage + 1}页 共{maxPage}页", GUILayout.Width(100));
            if (GUILayout.Button("上一页", GUILayout.Width(50))) nowPage--;
            if (GUILayout.Button("下一页", GUILayout.Width(50))) nowPage++;
            if (nowPage < 0) nowPage = maxPage - 1;
            if (nowPage >= maxPage) nowPage = 0;
            tmpPage = 0;
            tmpShow = 0;
            if (GUILayout.Button("关于", GUILayout.Width(80)))
            {
                InfoWindow.ShowInfo = true;
            }
            GUILayout.EndHorizontal();
            #endregion

            //列表
            svPos = GUILayout.BeginScrollView(svPos);
            foreach (var kv in ItemList)
            {
                if (tmpPage < nowPage * 30)
                {
                    tmpPage++;
                    continue;
                }
                tmpShow++;
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label($"{nowPage * 30 + tmpShow}", GUILayout.Width(40));
                GUI.contentColor = ModTool.GetItemQualityColor(kv.Value);
                if (GUILayout.Button(Tools.instance.Code64ToString(kv.Value["name"].str), GUILayout.Width(100)))
                {
                    SelectItem = kv.Value;
                }
                GUI.contentColor = Color.white;
                GUILayout.Label($"{Tools.instance.Code64ToString(kv.Value["desc"].str)}");
                GUILayout.EndHorizontal();
                if (tmpShow >= 30)
                {
                    break;
                }
            }
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }
    }
}
