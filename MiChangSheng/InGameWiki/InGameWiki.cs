using System;
using BepInEx;
using GUIPackage;
using HarmonyLib;
using UnityEngine;
using BepInEx.Configuration;
using System.Collections.Generic;

namespace InGameWiki
{
    [BepInPlugin("me.xiaoye97.plugin.MiChangSheng.InGameWiki", "游戏百科", "1.1")]
    public class InGameWiki : BaseUnityPlugin
    {
        ConfigEntry<KeyCode> HotKey, CheckKey;
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
            CheckKey = Config.Bind<KeyCode>("config", "Checkkey", KeyCode.F7, "对着物品按下快捷键，打开百科对应的物品页");
            ShowJSON = Config.Bind<bool>("config", "ShowJson", false, "是否显示Json数据");
            Harmony.CreateAndPatchAll(typeof(InGameWiki));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Inventory2), "Show_Tooltip")]
        public static void TooltipPatch(item Item)
        {
            ItemSearchUI.TooltipItem = Item;
        }

        void Update()
        {
            if (Input.GetKeyDown(HotKey.Value))
            {
                IsShow = !IsShow;
            }
            if(Input.GetKeyDown(CheckKey.Value))
            {
                if(ItemSearchUI.TooltipItem != null)
                {
                    ItemSearchUI.SelectItem = ItemSearchUI.TooltipItem.itemID.ItemJson();
                    IsShow = true;
                }
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
            if(BuffWindow.ShowBuff)
            {
                BuffWindow.OnGUI();
            }
        }

        void WindowFunc(int id)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal(GUI.skin.box);
            if (GUILayout.Button("关于", GUILayout.Width(80)))
            {
                InfoWindow.ShowInfo = true;
            }
            if (GUILayout.Button("Buff数据表", GUILayout.Width(100)))
            {
                IsShow = false;
                BuffWindow.ShowBuff = true;
            }
            GUILayout.Label(" ");
            if (GUILayout.Button("关闭", GUILayout.Width(80)))
            {
                IsShow = false;
            }
            GUILayout.EndHorizontal();
            ItemSearchUI.OnGUI();
            ItemDataUI.OnGUI();
            JsonUI.OnGUI();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }
    }
}
