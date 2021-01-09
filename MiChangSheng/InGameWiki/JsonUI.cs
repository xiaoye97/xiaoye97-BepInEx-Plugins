/*
 JSON显示默认关闭，仅开发时使用
 */
using System;
using UnityEngine;
using System.Text.RegularExpressions;

namespace InGameWiki
{
    public static class JsonUI
    {
        static JSONObject json;
        static string jsonStr;
        public static JSONObject Json
        {
            get { return json; }
            set
            {
                json = value.Clone();
                jsonStr = Code64ToString(json.ToString());
            }
        }
        static Vector2 listPos, jsonPos;

        static string Code64ToString(string str)
        {
            str = Regex.Replace(str, @"(\\u[0-9a-z]{4})", (m) =>
            {
                foreach (var c in m.Groups)
                {
                    return Regex.Unescape(c.ToString());
                }
                return "";
            });
            return str;
        }

        public static void OnGUI()
        {
            if (!InGameWiki.ShowJSON.Value) return;
            GUILayout.BeginHorizontal("JSON", GUI.skin.window, GUILayout.ExpandWidth(true), GUILayout.Height(150));
            listPos = GUILayout.BeginScrollView(listPos, GUI.skin.box, GUILayout.ExpandHeight(true), GUILayout.Width(150));
            //JSON列表
            if (GUILayout.Button("物品表")) Json = jsonData.instance._ItemJsonData;
            if (GUILayout.Button("丹方表")) Json = jsonData.instance.LianDanDanFangBiao;
            if (GUILayout.Button("采药掉落")) Json = jsonData.instance.CaiYaoDiaoLuo;
            if (GUILayout.Button("采药收益")) Json = jsonData.instance.CaiYaoShoYi;
            if (GUILayout.Button("材料能量表")) Json = jsonData.instance.CaiLiaoNengLiangBiao;
            if (GUILayout.Button("支路随机采集")) Json = jsonData.instance.AllMapCaiJiBiao;
            GUILayout.EndScrollView();

            jsonPos = GUILayout.BeginScrollView(jsonPos, GUI.skin.box, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            if (Json == null)
            {
                GUILayout.Label("当前未选择JSON");
            }
            else
            {
                GUILayout.Label(jsonStr);
            }
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();
        }
    }
}
