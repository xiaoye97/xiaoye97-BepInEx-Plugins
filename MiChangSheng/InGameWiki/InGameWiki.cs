using System;
using BepInEx;
using UnityEngine;
using BepInEx.Configuration;
using System.Collections.Generic;

namespace InGameWiki
{
    [BepInPlugin("me.xiaoye97.plugin.MiChangSheng.InGameWiki", "游戏百科", "1.0")]
    public class InGameWiki : BaseUnityPlugin
    {
        ConfigEntry<KeyCode> HotKey;
        public static ConfigEntry<bool> ShowJSON;
        private static bool isShow;
        public static bool IsShow
        {
            get { return isShow; }
            set
            {
                isShow = value;
            }
        }

        Rect winRect = new Rect((Screen.width - 1200) / 2, (Screen.height - 800) / 2, 1200, 800);

        void Start()
        {
            HotKey = Config.Bind<KeyCode>("config", "Hotkey", KeyCode.F8, "开关界面的快捷键");
            ShowJSON = Config.Bind<bool>("config", "ShowJson", false, "是否显示Json数据");
        }

        void Update()
        {
            if (Input.GetKeyDown(HotKey.Value))
            {
                IsShow = !IsShow;
            }
        }

        void OnGUI()
        {
            if (IsShow)
            {
                winRect = GUILayout.Window(666, winRect, WindowFunc, "游戏百科");
            }
            if (InfoWindow.ShowInfo)
            {
                InfoWindow.OnGUI();
            }
        }

        void WindowFunc(int id)
        {
            GUILayout.BeginVertical();
            ItemSearchUI.OnGUI();
            ItemDataUI.OnGUI();
            JsonUI.OnGUI();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }
    }
}
