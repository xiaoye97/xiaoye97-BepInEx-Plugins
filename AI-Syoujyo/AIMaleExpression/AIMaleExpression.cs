using System;
using BepInEx;
using AIChara;
using System.IO;
using System.Text;
using HarmonyLib;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace xiaoye97
{
    [BepInPlugin("me.xiaoye97.plugin.AIMaleExpression", "AI少女男性H表情辅助插件", "1.2")]
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
                }
                catch (Exception e)
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
        public bool showOpenEye = true, showOpenMouth = true, showEyebrow = true, showEye = true, showMouth = true;
        public bool showNeckbehaviour, showEyebehaviour, showTargetNeck, showNeckRotA, showNeckRotB, showHeadRotA, showHeadRotB, showTargetEye, showEyeRotA, showEyeRotB;
        void ListGUI()
        {
            if (!string.IsNullOrEmpty(nowABPath)) GUILayout.Label($"路径:{nowABPath}");
            if (!string.IsNullOrEmpty(nowList)) GUILayout.Label($"List:{nowList}");
            if (!string.IsNullOrEmpty(nowAnim)) GUILayout.Label($"当前动画:{nowAnim}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("保存当前数据到csv文件", GUILayout.Width(300)))
            {
                SaveNowToCSV();
            }
            if (GUILayout.Button("加载CSV文件覆盖当前数据(必须是与当前姿势一致的list)", GUILayout.Width(400)))
            {
                LoadCSVToNow();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("保存当前数据到MonoBehaviour文本", GUILayout.Width(300)))
            {
                SaveNowToMonoText();
            }
            if (GUILayout.Button("加载MonoBehaviour文本覆盖当前数据(必须是与当前姿势一致的list)", GUILayout.Width(400)))
            {
                LoadMonoTextToNow();
            }
            GUILayout.EndHorizontal();
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
                    if (lst.Count > 0)
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

        /// <summary>
        /// int数据模板
        /// </summary>
        void GUITemplate1(string label, string target, int index)
        {
            enTmp = lst[index];
            GUILayout.BeginHorizontal(GUILayout.Width(uiLabelWidth));
            int data = (int)typeof(HMotionEyeNeckMale.EyeNeck).GetField(target).GetValue(enTmp);
            GUILayout.Label($"{label} {data}");
            TypedReference r = __makeref(enTmp);
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                if (data > 0)
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

        /// <summary>
        /// Vector3数据模板
        /// </summary>
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

        #region 读写数据
        /// <summary>
        /// 保存当前数据到文件，文件路径Bepinex/list名+动画名+时间.csv
        /// </summary>
        void SaveNowToCSV()
        {
            Vector3 v;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Anim,目開き,口開き,眉,目形,口舌形,首挙動,目挙動,首タゲ,首角度,,,,,,頭角度,,,,,,目タゲ,目角度,,,,,");
            for (int i = 0; i < lst.Count; i++)
            {
                sb.Append(lst[i].anim + ",");
                sb.Append((int)typeof(HMotionEyeNeckMale.EyeNeck).GetField("openEye").GetValue(lst[i]) + ",");
                sb.Append((int)typeof(HMotionEyeNeckMale.EyeNeck).GetField("openMouth").GetValue(lst[i]) + ",");
                sb.Append((int)typeof(HMotionEyeNeckMale.EyeNeck).GetField("eyebrow").GetValue(lst[i]) + ",");
                sb.Append((int)typeof(HMotionEyeNeckMale.EyeNeck).GetField("eye").GetValue(lst[i]) + ",");
                sb.Append((int)typeof(HMotionEyeNeckMale.EyeNeck).GetField("mouth").GetValue(lst[i]) + ",");
                sb.Append((int)typeof(HMotionEyeNeckMale.EyeNeck).GetField("Neckbehaviour").GetValue(lst[i]) + ",");
                sb.Append((int)typeof(HMotionEyeNeckMale.EyeNeck).GetField("Eyebehaviour").GetValue(lst[i]) + ",");
                sb.Append((int)typeof(HMotionEyeNeckMale.EyeNeck).GetField("targetNeck").GetValue(lst[i]) + ",");
                v = ((Vector3[])typeof(HMotionEyeNeckMale.EyeNeck).GetField("NeckRot").GetValue(lst[i]))[0];
                sb.Append(v.x + "," + v.y + "," + v.z + ",");
                v = ((Vector3[])typeof(HMotionEyeNeckMale.EyeNeck).GetField("NeckRot").GetValue(lst[i]))[1];
                sb.Append(v.x + "," + v.y + "," + v.z + ",");
                v = ((Vector3[])typeof(HMotionEyeNeckMale.EyeNeck).GetField("HeadRot").GetValue(lst[i]))[0];
                sb.Append(v.x + "," + v.y + "," + v.z + ",");
                v = ((Vector3[])typeof(HMotionEyeNeckMale.EyeNeck).GetField("HeadRot").GetValue(lst[i]))[1];
                sb.Append(v.x + "," + v.y + "," + v.z + ",");
                sb.Append((int)typeof(HMotionEyeNeckMale.EyeNeck).GetField("targetEye").GetValue(lst[i]) + ",");
                v = ((Vector3[])typeof(HMotionEyeNeckMale.EyeNeck).GetField("EyeRot").GetValue(lst[i]))[0];
                sb.Append(v.x + "," + v.y + "," + v.z + ",");
                v = ((Vector3[])typeof(HMotionEyeNeckMale.EyeNeck).GetField("EyeRot").GetValue(lst[i]))[1];
                sb.Append(v.x + "," + v.y + "," + v.z + "\n");
            }
            File.WriteAllText(Paths.PluginPath + nowList + "+" + nowAnim + "+" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv", sb.ToString());
        }

        /// <summary>
        /// 从本地文件读取并加载，必须保证当前姿势与加载的姿势一致才能正确读取
        /// </summary>
        void LoadCSVToNow()
        {
            var lines = OpenDirectory("csv");
            if (lines != null)
            {
                for (int i = 1; i < lines.Length; i++)
                {
                    if (!string.IsNullOrEmpty(lines[i]))
                    {
                        string[] vs = lines[i].Split(',');
                        Vector3[] v;

                        HMotionEyeNeckMale.EyeNeck neck = new HMotionEyeNeckMale.EyeNeck();
                        neck.anim = vs[0];
                        neck.openEye = int.Parse(vs[1]);
                        neck.openMouth = int.Parse(vs[2]);
                        neck.eyebrow = int.Parse(vs[3]);
                        neck.eye = int.Parse(vs[4]);
                        neck.mouth = int.Parse(vs[5]);
                        neck.Neckbehaviour = int.Parse(vs[6]);
                        neck.Eyebehaviour = int.Parse(vs[7]);
                        neck.targetNeck = int.Parse(vs[8]);
                        v = new Vector3[] { Vector3.zero, Vector3.zero };
                        v[0] = ParseV3(vs[9], vs[10], vs[11]);
                        v[1] = ParseV3(vs[12], vs[13], vs[14]);
                        neck.NeckRot = v;
                        v = new Vector3[] { Vector3.zero, Vector3.zero };
                        v[0] = ParseV3(vs[15], vs[16], vs[17]);
                        v[1] = ParseV3(vs[18], vs[19], vs[20]);
                        neck.HeadRot = v;
                        neck.targetEye = int.Parse(vs[21]);
                        v = new Vector3[] { Vector3.zero, Vector3.zero };
                        v[0] = ParseV3(vs[22], vs[23], vs[24]);
                        v[1] = ParseV3(vs[25], vs[26], vs[27]);
                        neck.EyeRot = v;
                        lst[i - 1] = neck;
                    }
                }
            }
        }

        /// <summary>
        /// 保存当前数据到MonoBehaviour文本形式，与SB3U导出一致
        /// </summary>
        void SaveNowToMonoText()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<0><目開き><口開き><眉><目形><口舌形><首挙動><目挙動><首タゲ><首角度><><><><><><頭角度><><><><><><目タゲ><目角度><><><><><>");
            sb.AppendLine("<><><><><><><><><><ゆらぎ0.5以下><><><ゆらぎ0.5以上><><><ゆらぎ0.5以下><><><ゆらぎ0.5以上><><><><ゆらぎ0.5以下><><><ゆらぎ0.5以上><><>");
            sb.AppendLine("<><><><><><><><><><x><y><z><x><y><z><x><y><z><x><y><z><><x><y><z><x><y><z>");
            for (int i = 0; i < lst.Count; i++)
            {
                sb.Append($"<{lst[i].anim}>");
                SBAddData(sb, lst[i], "openEye");
                SBAddData(sb, lst[i], "openMouth");
                SBAddData(sb, lst[i], "eyebrow");
                SBAddData(sb, lst[i], "eye");
                SBAddData(sb, lst[i], "mouth");
                SBAddData(sb, lst[i], "Neckbehaviour");
                SBAddData(sb, lst[i], "Eyebehaviour");
                SBAddData(sb, lst[i], "targetNeck");
                SBAddData(sb, lst[i], "NeckRot", 0);
                SBAddData(sb, lst[i], "NeckRot", 1);
                SBAddData(sb, lst[i], "HeadRot", 0);
                SBAddData(sb, lst[i], "HeadRot", 1);
                SBAddData(sb, lst[i], "targetEye");
                SBAddData(sb, lst[i], "EyeRot", 0);
                SBAddData(sb, lst[i], "EyeRot", 1);
                sb.Append("\n");
            }
            File.WriteAllText(Paths.PluginPath + nowList + ".MonoBehaviour", sb.ToString());
        }

        //写入数据 - 普通int数据
        void SBAddData(StringBuilder sb, HMotionEyeNeckMale.EyeNeck neck, string fieldName)
        {
            sb.Append($"<{typeof(HMotionEyeNeckMale.EyeNeck).GetField(fieldName).GetValue(neck)}>");
        }

        //写入数据 - Vector3数据
        void SBAddData(StringBuilder sb, HMotionEyeNeckMale.EyeNeck neck, string fieldName, int index)
        {
            Vector3 v = ((Vector3[])typeof(HMotionEyeNeckMale.EyeNeck).GetField(fieldName).GetValue(neck))[index];
            if (v.x != 0) sb.Append($"<{v.x}>");
            else sb.Append("<>");
            if (v.y != 0) sb.Append($"<{v.y}>");
            else sb.Append("<>");
            if (v.z != 0) sb.Append($"<{v.z}>");
            else sb.Append("<>");
        }

        /// <summary>
        /// 从本地文件读取并加载，必须保证当前姿势与加载的姿势一致才能正确读取
        /// </summary>
        void LoadMonoTextToNow()
        {
            var lines = OpenDirectory("MonoBehaviour");
            if (lines != null)
            {
                for (int i = 3; i < lines.Length; i++)
                {
                    if (!string.IsNullOrEmpty(lines[i]))
                    {
                        string line = lines[i].Replace("><", ",");
                        line = line.Replace("<", "");
                        line = line.Replace(">", "");
                        string[] vs = line.Split(',');
                        Vector3[] v;

                        HMotionEyeNeckMale.EyeNeck neck = new HMotionEyeNeckMale.EyeNeck();
                        neck.anim = vs[0];
                        neck.openEye = int.Parse(vs[1]);
                        neck.openMouth = int.Parse(vs[2]);
                        neck.eyebrow = int.Parse(vs[3]);
                        neck.eye = int.Parse(vs[4]);
                        neck.mouth = int.Parse(vs[5]);
                        neck.Neckbehaviour = int.Parse(vs[6]);
                        neck.Eyebehaviour = int.Parse(vs[7]);
                        neck.targetNeck = int.Parse(vs[8]);
                        v = new Vector3[] { Vector3.zero, Vector3.zero };
                        v[0] = ParseV3(vs[9], vs[10], vs[11]);
                        v[1] = ParseV3(vs[12], vs[13], vs[14]);
                        neck.NeckRot = v;
                        v = new Vector3[] { Vector3.zero, Vector3.zero };
                        v[0] = ParseV3(vs[15], vs[16], vs[17]);
                        v[1] = ParseV3(vs[18], vs[19], vs[20]);
                        neck.HeadRot = v;
                        neck.targetEye = int.Parse(vs[21]);
                        v = new Vector3[] { Vector3.zero, Vector3.zero };
                        v[0] = ParseV3(vs[22], vs[23], vs[24]);
                        v[1] = ParseV3(vs[25], vs[26], vs[27]);
                        neck.EyeRot = v;
                        lst[i - 3] = neck;
                    }
                }
            }
        }

        Vector3 ParseV3(string x, string y, string z)
        {
            Vector3 v = Vector3.zero;
            if (string.IsNullOrEmpty(x)) v.x = 0;
            else v.x = float.Parse(x);
            if (string.IsNullOrEmpty(y)) v.y = 0;
            else v.x = float.Parse(y);
            if (string.IsNullOrEmpty(z)) v.z = 0;
            else v.x = float.Parse(z);
            return v;
        }
        /// <summary>
        /// 读取文件
        /// </summary>
        public string[] OpenDirectory(string type)
        {
            var openFileName = new OpenFileName();
            openFileName.structSize = Marshal.SizeOf(openFileName);
            openFileName.filter = "文件(*." + type + ")\0*." + type + "";
            openFileName.file = new string(new char[256]);
            openFileName.maxFile = openFileName.file.Length;
            openFileName.fileTitle = new string(new char[64]);
            openFileName.maxFileTitle = openFileName.fileTitle.Length;
            openFileName.initialDir = Application.streamingAssetsPath.Replace('/', '\\');//默认路径
            openFileName.title = "选择文件";
            openFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;

            if (LocalDialog.GetSaveFileName(openFileName))//点击系统对话框框保存按钮
            {
                var content = File.ReadAllLines(openFileName.file);
                return content;
            }
            return null;
        }
        #endregion

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F10))
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

    #region 文件选择框
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class OpenFileName
    {
        public int structSize = 0;
        public IntPtr dlgOwner = IntPtr.Zero;
        public IntPtr instance = IntPtr.Zero;
        public String filter = null;
        public String customFilter = null;
        public int maxCustFilter = 0;
        public int filterIndex = 0;
        public String file = null;
        public int maxFile = 0;
        public String fileTitle = null;
        public int maxFileTitle = 0;
        public String initialDir = null;
        public String title = null;
        public int flags = 0;
        public short fileOffset = 0;
        public short fileExtension = 0;
        public String defExt = null;
        public IntPtr custData = IntPtr.Zero;
        public IntPtr hook = IntPtr.Zero;
        public String templateName = null;
        public IntPtr reservedPtr = IntPtr.Zero;
        public int reservedInt = 0;
        public int flagsEx = 0;
    }

    public class LocalDialog
    {
        //链接指定系统函数       打开文件对话框
        [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
        public static bool GetOFN([In, Out] OpenFileName ofn)
        {
            return GetOpenFileName(ofn);//执行打开文件的操作
        }

        //链接指定系统函数        另存为对话框
        [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        public static extern bool GetSaveFileName([In, Out] OpenFileName ofn);
        public static bool GetSFN([In, Out] OpenFileName ofn)
        {
            return GetSaveFileName(ofn);//执行保存选中文件的操作
        }
    }
    #endregion
}
