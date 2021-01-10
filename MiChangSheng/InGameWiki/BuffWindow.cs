using System;
using UnityEngine;
using System.Collections.Generic;

namespace InGameWiki
{
    public static class BuffWindow
    {
        public static bool ShowBuff;
        static Rect winRect = new Rect((Screen.width - 1200) / 2, (Screen.height - 700) / 2, 1200, 700);
        static Vector2 svPos;
        static int nowPage, maxPage, tmpPage, tmpShow;
        static string searchStr;
        static List<KeyValuePair<string, string>> buffData;
        static List<KeyValuePair<string, string>> showData = new List<KeyValuePair<string, string>>();

        public static void Init()
        {
            buffData = new List<KeyValuePair<string, string>>();

            JSONObject buffs = jsonData.instance._BuffJsonData;
            foreach (var buff in buffs.list)
            {
                buffData.Add(new KeyValuePair<string, string>($"名字: {buff["name"].str.UnCode64()}", $"描述: {buff["descr"].str.UnCode64()}"));
            }
        }

        public static void OnGUI()
        {
            if (ShowBuff)
            {
                winRect = GUILayout.Window(667, winRect, WindowFunc, "游戏百科·Buff数据表");
            }
        }

        static bool ContainsSearch(KeyValuePair<string, string> buff)
        {
            if (string.IsNullOrWhiteSpace(searchStr)) return true;
            string[] searchs = searchStr.Split(' ');
            bool result = true;
            foreach (var search in searchs)
            {
                if (!buff.Key.Contains(search) && !buff.Value.Contains(search))
                {
                    result = false;
                }
            }
            return result;
        }

        static void SearchBuffs()
        {
            if (string.IsNullOrWhiteSpace(searchStr))
            {
                maxPage = buffData.Count / 30;
                if (buffData.Count % 30 != 0) maxPage++;
                return;
            }
            showData.Clear();
            foreach (var buff in buffData)
            {
                if (ContainsSearch(buff))
                    showData.Add(buff);
            }
            maxPage = showData.Count / 30;
            if (showData.Count % 30 != 0) maxPage++;
        }

        static void WindowFunc(int id)
        {
            if (buffData == null)
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
            if (GUILayout.Button("关闭", GUILayout.Width(80))) ShowBuff = false;
            GUILayout.EndHorizontal();
            svPos = GUILayout.BeginScrollView(svPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            var show = string.IsNullOrWhiteSpace(searchStr) ? buffData : showData;
            foreach (var buff in show)
            {
                if (tmpPage < nowPage * 30)
                {
                    tmpPage++;
                    continue;
                }
                tmpShow++;
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label(buff.Key, GUILayout.Width(180));
                GUILayout.Label(buff.Value);
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
