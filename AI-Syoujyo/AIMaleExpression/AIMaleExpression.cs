using System;
using BepInEx;
using AIChara;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace xiaoye97
{
    [BepInPlugin("me.xiaoye97.plugin.AIMaleExpression", "AI少女男性H表情辅助插件", "1.0")]
    public class AIMaleExpression : BaseUnityPlugin
    {
        private HScene hscene;
        public static bool windowShow; 
        private int windowId;
        private Rect windowRect = new Rect(10, 10, 900, 700);

        void Start()
        {
            new Harmony("me.xiaoye97.plugin.AIMaleExpression").PatchAll();
            windowId = UnityEngine.Random.Range(1000000000, 2000000000);
        }

        void OnGUI()
        {
            if (!windowShow) return;
            GUI.backgroundColor = Color.black;
            windowRect = GUILayout.Window(windowId, windowRect, WindowFunc, "男性啪啪啪表情实时预览");
        }

        void WindowFunc(int id)
        {
            if (hscene == null)
            {
                var obj = GameObject.Find("CommonSpace/HSceneSet/HScene");
                if (obj) hscene = obj.GetComponent<HScene>();
                GUILayout.Label("没有开始H");
            }
            else
            {
                try
                {
                    ListGUI();
                }catch(Exception e)
                {
                    Logger.LogWarning(e.Message);
                }
            }
            GUI.DragWindow();
        }

        public static string nowABPath, nowList, nowAnim; //当前包路径，list，动画
        public static readonly float uiLabelWidth = 110, uiLabelWidth2 = 40, uiSliderWidth = 80;
        public Vector2 svPos;
        private HMotionEyeNeckMale.EyeNeck enTmp;
        public bool showOpenEye =true, showOpenMouth = true, showEyebrow = true, showEye = true, showMouth = true;
        public bool showNeckbehaviour, showEyebehaviour, showTargetNeck, showNeckRotA, showNeckRotB, showHeadRotA, showHeadRotB, showTargetEye, showEyeRotA, showEyeRotB;
        void ListGUI()
        {
            if (!string.IsNullOrEmpty(nowABPath)) GUILayout.Label($"路径:{nowABPath}");
            if (!string.IsNullOrEmpty(nowList)) GUILayout.Label($"List:{nowList}");
            if (!string.IsNullOrEmpty(nowAnim)) GUILayout.Label($"当前动画:{nowAnim}");
            if (hscene.ctrlEyeNeckMale.Length > 0 && hscene.ctrlEyeNeckMale[0] != null)
            {
                lst = Traverse.Create(hscene.ctrlEyeNeckMale[0]).Field("lstEyeNeck").GetValue<List<HMotionEyeNeckMale.EyeNeck>>();
                if (lst != null)
                {
                    GUILayout.BeginHorizontal();
                    showOpenEye = GUILayout.Toggle(showOpenEye, "目開き");
                    showOpenMouth = GUILayout.Toggle(showOpenMouth, "口開き");
                    showEyebrow = GUILayout.Toggle(showEyebrow, "眉");
                    showEye = GUILayout.Toggle(showEye, "目形");
                    showMouth = GUILayout.Toggle(showMouth, "口舌形");
                    showNeckbehaviour = GUILayout.Toggle(showNeckbehaviour, "首挙動");
                    showEyebehaviour = GUILayout.Toggle(showEyebehaviour, "目挙動");
                    showTargetNeck = GUILayout.Toggle(showTargetNeck, "首タゲ");
                    showNeckRotA = GUILayout.Toggle(showNeckRotA, "首角度A");
                    showNeckRotB = GUILayout.Toggle(showNeckRotB, "首角度B");
                    showHeadRotA = GUILayout.Toggle(showHeadRotA, "頭角度A");
                    showHeadRotB = GUILayout.Toggle(showHeadRotB, "頭角度B");
                    showTargetEye = GUILayout.Toggle(showTargetEye, "目タゲ");
                    showEyeRotA = GUILayout.Toggle(showEyeRotA, "目角度A");
                    showEyeRotB = GUILayout.Toggle(showEyeRotB, "目角度B");
                    GUILayout.EndHorizontal();
                    if(lst.Count>0)
                    {
                        svPos = GUILayout.BeginScrollView(svPos);
                        for (int i = 0; i < lst.Count; i++)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label($"{lst[i].anim}", GUILayout.Width(150));
                            if (showOpenEye) GUITemplate1("目開き", "openEye", i);
                            if (showOpenMouth) GUITemplate1("口開き", "openMouth", i);
                            if (showEyebrow) GUITemplate1("眉", "eyebrow", i);
                            if (showEye) GUITemplate1("目形", "eye", i);
                            if (showMouth) GUITemplate1("口舌形", "mouth", i);
                            if (showNeckbehaviour) GUITemplate1("首挙動", "Neckbehaviour", i);
                            if (showEyebehaviour) GUITemplate1("目挙動", "Eyebehaviour", i);
                            if (showTargetNeck) GUITemplate1("首タゲ", "targetNeck", i);
                            if (showNeckRotA) GUITemplate2("首タゲA", "NeckRot", i, 0);
                            if (showNeckRotB) GUITemplate2("首タゲB", "NeckRot", i, 1);
                            if (showHeadRotA) GUITemplate2("頭角度A", "HeadRot", i, 0);
                            if (showHeadRotB) GUITemplate2("頭角度B", "HeadRot", i, 1);
                            if (showTargetEye) GUITemplate1("目タゲ", "targetEye", i);
                            if (showEyeRotA) GUITemplate2("目角度A", "EyeRot", i, 0);
                            if (showEyeRotB) GUITemplate2("目角度B", "EyeRot", i, 1);
                            GUILayout.EndHorizontal();
                        }
                        GUILayout.EndScrollView();
                    }
                }
            }
        }

        List<HMotionEyeNeckMale.EyeNeck> lst;

        void GUITemplate1(string label, string target, int index)
        {
            enTmp = lst[index];
            GUILayout.BeginHorizontal(GUILayout.Width(uiLabelWidth));
            int data = (int)typeof(HMotionEyeNeckMale.EyeNeck).GetField(target).GetValue(enTmp);
            GUILayout.Label($"{label} {data}");
            TypedReference r = __makeref(enTmp);
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                if(data > 0)
                {
                    typeof(HMotionEyeNeckMale.EyeNeck).GetField(target).SetValueDirect(r, data - 1);
                }
            }
            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                typeof(HMotionEyeNeckMale.EyeNeck).GetField(target).SetValueDirect(r, data + 1);
            }
            lst[index] = enTmp;
            GUILayout.Label("", GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
        }

        void GUITemplate2(string label, string target, int index, int aorb)
        {
            enTmp = lst[index];
            Vector3 data = ((Vector3[])typeof(HMotionEyeNeckMale.EyeNeck).GetField(target).GetValue(enTmp))[aorb];
            TypedReference r = __makeref(enTmp);
            GUILayout.BeginHorizontal(GUILayout.Width(uiLabelWidth * 3));
            GUILayout.Label($"{label}");
            GUILayout.Label($"x{data.x}", GUILayout.Width(uiLabelWidth2));
            var x = GUILayout.HorizontalSlider(data.x, 0, 360, GUILayout.Width(uiSliderWidth));
            GUILayout.Label($"y{data.y}", GUILayout.Width(uiLabelWidth2));
            var y = GUILayout.HorizontalSlider(data.y, 0, 360, GUILayout.Width(uiSliderWidth));
            GUILayout.Label($"z{data.z}", GUILayout.Width(uiLabelWidth2));
            var z = GUILayout.HorizontalSlider(data.z, 0, 360, GUILayout.Width(uiSliderWidth));
            Vector3[] values = null;
            switch (aorb)
            {
                case 0:
                    values = new Vector3[] { new Vector3(x, y, z), ((Vector3[])typeof(HMotionEyeNeckMale.EyeNeck).GetField(target).GetValue(enTmp))[1] };
                    break;
                case 1:
                    values = new Vector3[] { ((Vector3[])typeof(HMotionEyeNeckMale.EyeNeck).GetField(target).GetValue(enTmp))[0], new Vector3(x, y, z) };
                    break;
            }
            typeof(HMotionEyeNeckMale.EyeNeck).GetField(target).SetValueDirect(r, values);
            lst[index] = enTmp;
            GUILayout.Label("", GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
        }

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.F10))
            {
                windowShow = !windowShow;
            }
        }

        [HarmonyPatch(typeof(HMotionEyeNeckMale), "Load")]
        class HPatch
        {
            public static bool Prefix(string _assetpath, string _file)
            {
                Debug.Log($"男性表情:加载AB资源 路径:{_assetpath} 名称:{_file}");
                nowABPath = _assetpath;
                nowList = _file;
                return true;
            }
        }

        [HarmonyPatch(typeof(ChaControl), "setPlay")]
        class HAnimPatch
        {
            public static void Postfix(string _strAnmName)
            {
                nowAnim = _strAnmName;
            }
        }
    }
}
