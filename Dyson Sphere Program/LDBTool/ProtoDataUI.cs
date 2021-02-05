using System;
using UnityEngine;
using System.Collections.Generic;

namespace xiaoye97
{
    /// <summary>
    /// 用来显示Proto数据，方便开发
    /// </summary>
    public static class ProtoDataUI
    {
        public static bool Show;
        private static Rect winRect = new Rect(0, 0, 500, 800);
        private static int selectType;
        private static string[] protoTypeNames = Enum.GetNames(typeof(ProtoType));

        public static void OnGUI()
        {
            winRect = GUILayout.Window(3562532, winRect, WindowFunc, "ProtoData");
        }

        public static void WindowFunc(int id)
        {
            GUILayout.BeginVertical();
            selectType = GUILayout.SelectionGrid(selectType, protoTypeNames, 13);

            ProtoType type = (ProtoType)selectType;
            if (type == ProtoType.AdvisorTip) LDB.advisorTips.ShowSet();
            else if (type == ProtoType.Audio) LDB.audios.ShowSet();
            else if (type == ProtoType.EffectEmitter) LDB.effectEmitters.ShowSet();
            else if (type == ProtoType.Item) LDB.items.ShowSet();
            else if (type == ProtoType.Model) LDB.models.ShowSet();
            else if (type == ProtoType.Player) LDB.players.ShowSet();
            else if (type == ProtoType.Recipe) LDB.recipes.ShowSet();
            else if (type == ProtoType.String) LDB.strings.ShowSet();
            else if (type == ProtoType.Tech) LDB.techs.ShowSet();
            else if (type == ProtoType.Theme) LDB.themes.ShowSet();
            else if (type == ProtoType.Tutorial) LDB.tutorial.ShowSet();
            else if (type == ProtoType.Vege) LDB.veges.ShowSet();
            else if (type == ProtoType.Vein) LDB.veins.ShowSet();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }
    }

    public static class ProtoSetEx
    {
        private static Vector2 sv;
        private static Dictionary<Type, int> selectPages = new Dictionary<Type, int>()
        {
            {typeof(AdvisorTipProtoSet) , 0 },
            {typeof(AudioProtoSet) , 0 },
            {typeof(EffectEmitterProtoSet) , 0 },
            {typeof(ItemProtoSet) , 0 },
            {typeof(ModelProtoSet) , 0 },
            {typeof(PlayerProtoSet) , 0 },
            {typeof(RecipeProtoSet) , 0 },
            {typeof(StringProtoSet) , 0 },
            {typeof(TechProtoSet) , 0 },
            {typeof(ThemeProtoSet) , 0 },
            {typeof(TutorialProtoSet) , 0 },
            {typeof(VegeProtoSet) , 0 },
            {typeof(VeinProtoSet) , 0 }
        };

        public static void ShowSet<T>(this ProtoSet<T> protoSet) where T : Proto
        {
            if (protoSet.dataArray.Length > 100)
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label($"Page {selectPages[protoSet.GetType()] + 1} / {protoSet.dataArray.Length / 100 + 1}", GUILayout.Width(80));
                if (GUILayout.Button("-", GUILayout.Width(20))) selectPages[protoSet.GetType()]--;
                if (GUILayout.Button("+", GUILayout.Width(20))) selectPages[protoSet.GetType()]++;
                if (selectPages[protoSet.GetType()] < 0) selectPages[protoSet.GetType()] = protoSet.dataArray.Length / 100;
                else if (selectPages[protoSet.GetType()] > protoSet.dataArray.Length / 100) selectPages[protoSet.GetType()] = 0;
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label($"index", GUILayout.Width(40));
            GUILayout.Label($"ID", GUILayout.Width(40));
            GUILayout.Label($"Name");
            GUILayout.Label($"TranslateName");
            if (SupportsHelper.SupportsRuntimeUnityEditor)
            {
                GUILayout.Label($"Show Data", GUILayout.Width(100));
            }
            GUILayout.EndHorizontal();
            sv = GUILayout.BeginScrollView(sv, GUI.skin.box);
            for (int i = selectPages[protoSet.GetType()] * 100; i < Mathf.Min(selectPages[protoSet.GetType()] * 100 + 100, protoSet.dataArray.Length); i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{i}", GUILayout.Width(40));
                if (protoSet.dataArray[i] != null)
                {
                    GUILayout.Label($"{protoSet.dataArray[i].ID}", GUILayout.Width(40));
                    GUILayout.Label($"{protoSet.dataArray[i].Name}");
                    GUILayout.Label($"{protoSet.dataArray[i].name.Translate()}");
                    if (SupportsHelper.SupportsRuntimeUnityEditor)
                    {
                        if (GUILayout.Button($"Show Data", GUILayout.Width(100)))
                        {
                            ShowItem item = new ShowItem(protoSet.dataArray[i], $"{protoSet.dataArray[i].GetType().Name} {protoSet.dataArray[i].name.Translate()}");
                            RUEHelper.ShowData(item);
                        }
                    }
                }
                else
                {
                    GUILayout.Label("null");
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }
    }
}
