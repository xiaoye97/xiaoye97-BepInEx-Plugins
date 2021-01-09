using UnityEngine;

namespace InGameWiki
{
    public static class InfoWindow
    {
        static bool showInfo;
        public static bool ShowInfo
        {
            get { return showInfo; }
            set
            {
                showInfo = value;
            }
        }
        static Rect winRect = new Rect((Screen.width - 200) / 2, (Screen.height - 100) / 2, 200, 100);

        public static void OnGUI()
        {
            if (ShowInfo)
            {
                winRect = GUILayout.Window(1997, winRect, WindowFunc, "关于");
            }
        }

        static void WindowFunc(int id)
        {
            if (GUILayout.Button("关闭"))
            {
                ShowInfo = false;
            }
            GUILayout.Label("插件名:游戏百科");
            GUILayout.Label("版本:1.0");
            GUILayout.Label("作者:xiaoye97");
            GUILayout.Label("Github:xiaoye97");
            GUILayout.Label("哔哩哔哩:宵夜97");
            GUILayout.Label("QQ:1066666683(夜空之下)");
            GUILayout.Label("如果有什么建议或者BUG反馈，可以在B站评论区或者觅长生官方3群找我反馈");
            GUI.DragWindow();
        }
    }
}
