using BepInEx;
using UnityEngine;
using BepInEx.Configuration;
using System.Collections.Generic;
using System.IO;

namespace MCSLiHui
{
    [BepInPlugin("me.xiaoye97.plugin.MiChangSheng.MCSLiHui", "自定义立绘", "1.1")]
    public class MCSLiHui : BaseUnityPlugin
    {
        private bool show;
        private ConfigEntry<KeyCode> Hotkey;
        private Rect winRect = new Rect(100, 100, 500, 300);
        private static List<string> hideScenes = new List<string>() { "MainMenu", "LoadingScreen" };
        private string facePlayerInput = "10001";
        private string faceNPCInput = "10001";

        private void Start()
        {
            Hotkey = Config.Bind<KeyCode>("config", "Hotkey", KeyCode.F8, "开启界面热键");
            DirectoryInfo dir = new DirectoryInfo($"{Application.dataPath}/../ModRes/Effect/Prefab/gameEntity/Avater");
            if (!dir.Exists)
            {
                dir.Create();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(Hotkey.Value))
            {
                show = !show;
            }
        }

        private void OnGUI()
        {
            if (show)
            {
                if (PlayerEx.Player != null && !hideScenes.Contains(SceneEx.NowSceneName))
                {
                    winRect = GUILayout.Window(654321, winRect, WindowFunc, "自定义立绘Mod");
                }
            }
        }

        public void WindowFunc(int id)
        {
            GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            InfoGUI();
            PlayerGUI();
            NPCGUI();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        public void PlayerGUI()
        {
            GUILayout.BeginVertical("玩家立绘", GUI.skin.window);
            GUILayout.Label($"玩家当前立绘:{PlayerEx.Player.Face.I}");
            GUILayout.BeginHorizontal();
            GUILayout.Label("设置立绘编号", GUILayout.Width(200));
            facePlayerInput = GUILayout.TextField(facePlayerInput);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("修改立绘"))
            {
                PlayerEx.Player.Face = new JSONObject(int.Parse(facePlayerInput));
                if (UIHeadPanel.Inst != null)
                {
                    UIHeadPanel.Inst.Face.setFace();
                }
            }
            GUILayout.EndVertical();
        }

        public void NPCGUI()
        {
            GUILayout.BeginVertical("NPC立绘", GUI.skin.window);
            if (UINPCJiaoHu.Inst.JiaoHuPop.gameObject.activeInHierarchy)
            {
                GUILayout.Label($"当前交互NPC:{UINPCJiaoHu.Inst.NowJiaoHuNPC.Name}");
                GUILayout.Label($"NPC当前立绘:{UINPCJiaoHu.Inst.NowJiaoHuNPC.Face}");
                GUILayout.BeginHorizontal();
                GUILayout.Label("设置NPC立绘编号", GUILayout.Width(200));
                faceNPCInput = GUILayout.TextField(faceNPCInput);
                GUILayout.EndHorizontal();
                if (GUILayout.Button("修改立绘"))
                {
                    jsonData.instance.AvatarJsonData[UINPCJiaoHu.Inst.NowJiaoHuNPC.ID.ToString()].SetField("face", int.Parse(faceNPCInput));
                    NpcJieSuanManager.inst.isUpDateNpcList = true;
                    UINPCJiaoHu.Inst.JiaoHuPop.RefreshUI();
                }
            }
            else
            {
                GUILayout.Label("点击NPC打开交互弹窗以获取NPC数据");
            }
            GUILayout.EndVertical();
        }

        public void InfoGUI()
        {
            GUILayout.BeginVertical("说明(视频教程关注B站 宵夜97)", GUI.skin.window);
            GUILayout.Label("放入立绘流程:");
            GUILayout.Label("1.在steam右键游戏->管理->浏览本地文件");
            GUILayout.Label("2.打开ModRes/Effect/Prefab/gameEntity/Avater文件夹(Mod会创建此文件夹)");
            GUILayout.Label("3.根据自定义的立绘编号(10000以上)在Avater下建立文件夹，如编号为10001，则建的文件夹名为Avater10001");
            GUILayout.Label("4.将PNG格式立绘图片放入建好的文件夹，并将文件名改为编号");
            GUILayout.Label("注1:官方的立绘尺寸为1255x1408，在准备立绘时，要裁切成这个尺寸，并且脸的位置和模板人物对齐");
            GUILayout.Label("注2:立绘模板在觅长生/觅长生test_Data/Res/Effect/Prefab/gameEntity/Avater/Avater10001/10001.png");
            GUILayout.EndVertical();
        }
    }
}