using System;
using UnityEngine;
using System.Collections.Generic;

namespace InGameWiki
{
    public static class SkillWindow
    {
        public static bool ShowSkill;
        static Rect winRect = new Rect((Screen.width - 1200) / 2, (Screen.height - 700) / 2, 1200, 700);
        static Vector2 svPos;
        static int nowPage, maxPage, tmpPage, tmpShow;
        static string searchStr;
        static List<string> nameList = new List<string>();
        static List<string> descList = new List<string>();
        static List<int> idList = new List<int>();
        static List<int> showList = new List<int>();

        public static void Init()
        {
            JSONObject buffs = jsonData.instance._skillJsonData;
            foreach (var buff in buffs.list)
            {
                idList.Add(buff["id"].I);
                nameList.Add(buff["name"].str.UnCode64());
                descList.Add(buff["descr"].str.UnCode64());
            }
        }

        public static void OnGUI()
        {
            if (ShowSkill)
            {
                winRect = GUILayout.Window(667, winRect, WindowFunc, "游戏百科·技能数据表");
            }
        }

        static bool ContainsSearch(string str)
        {
            if (string.IsNullOrWhiteSpace(searchStr)) return true;
            string[] searchs = searchStr.Split(' ');
            bool result = true;
            foreach (var search in searchs)
            {
                if (!str.Contains(search))
                {
                    result = false;
                }
            }
            return result;
        }

        static void SearchBuffs()
        {
            showList.Clear();
            if (string.IsNullOrWhiteSpace(searchStr))
            {
                for (int i = 0; i < idList.Count; i++)
                {
                    showList.Add(i);
                }
                maxPage = idList.Count / 30;
                if (idList.Count % 30 != 0) maxPage++;
                return;
            }
            for (int i = 0; i < idList.Count; i++)
            {
                if (ContainsSearch(idList[i].ToString()) || ContainsSearch(nameList[i]) || ContainsSearch(descList[i]))
                    showList.Add(i);
            }
            maxPage = showList.Count / 30;
            if (showList.Count % 30 != 0) maxPage++;
        }

        static void WindowFunc(int id)
        {
            if (idList.Count == 0)
            {
                Init();
            }
            SearchBuffs();
            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label("关键词", GUILayout.Width(45));
            searchStr = GUILayout.TextField(searchStr, GUILayout.Width(100));
            if (GUILayout.Button("上一页", GUILayout.Width(80))) nowPage--;
            GUILayout.Label($"第{nowPage + 1}页 共{maxPage}页", GUILayout.Width(88));
            if (GUILayout.Button("下一页", GUILayout.Width(80))) nowPage++;
            if (nowPage < 0) nowPage = maxPage - 1;
            if (nowPage >= maxPage) nowPage = 0;
            tmpPage = 0;
            tmpShow = 0;
            GUILayout.Label(" ");
            if (GUILayout.Button("关闭", GUILayout.Width(80))) ShowSkill = false;
            GUILayout.EndHorizontal();
            svPos = GUILayout.BeginScrollView(svPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            foreach (var buff in showList)
            {
                if (tmpPage < nowPage * 30)
                {
                    tmpPage++;
                    continue;
                }
                tmpShow++;
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label($"skillid: {idList[buff]}", GUILayout.Width(90));
                GUILayout.Label($"名字: {nameList[buff]}", GUILayout.Width(180));
                GUILayout.Label($"描述: {descList[buff]}");
                GUILayout.EndHorizontal();
                if (tmpShow >= 30)
                {
                    break;
                }
            }
            GUILayout.EndScrollView();
            GUI.DragWindow();
        }
    }
}
