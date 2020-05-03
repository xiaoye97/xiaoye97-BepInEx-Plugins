using System;
using BepInEx;
using AIChara;
using System.IO;
using System.Text;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace xiaoye97
{
    [BepInPlugin("me.xiaoye97.plugin.AIMaleExpression", "AI少女男性H表情辅助插件", "1.5")]
    public class AIMaleExpression : BaseUnityPlugin
    {
        private static HScene hscene;
        public static bool windowShow;
        private int windowId;
        private Rect windowRect = new Rect(10, 10, 900, 700);

        void Start()
        {
            new Harmony("me.xiaoye97.plugin.AIMaleExpression").PatchAll();
            LoadOverrideData();
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
                    GlobalMethod.setCameraMoveFlag(hscene.ctrlFlag.cameraCtrl, false);
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
        public static GlobalOverrideData nowOverrideData;
        public static Dictionary<string, string> nameDict = new Dictionary<string, string>() {
            { "openEye", "目開き"},
            { "openMouth", "口開き" },
            { "eyebrow", "眉" },
            { "eye", "目形" },
            { "mouth", "口舌形" },
            { "Neckbehaviour", "首挙動" },
            { "Eyebehaviour", "目挙動" },
            { "targetNeck", "首タゲ" },
            { "NeckRot", "首角度" },
            { "HeadRot", "頭角度" },
            { "targetEye", "目タゲ" },
            { "EyeRot", "目角度" }
        };
        public static Dictionary<string, bool> boolDict = new Dictionary<string, bool>() {
            { "openEye", true},
            { "openMouth", true },
            { "eyebrow", true },
            { "eye", true },
            { "mouth", true },
            { "Neckbehaviour", false },
            { "Eyebehaviour", false },
            { "targetNeck", false },
            { "NeckRot", false },
            { "HeadRot", false },
            { "targetEye", false },
            { "EyeRot", false }
        };

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
            if (GUILayout.Button("重写载入配置文件", GUILayout.Width(300)))
            {
                TryLoadConfigCSVToNow(true);
            }
            if (hscene.ctrlEyeNeckMale.Length > 0 && hscene.ctrlEyeNeckMale[0] != null)
            {
                lst = Traverse.Create(hscene.ctrlEyeNeckMale[0]).Field("lstEyeNeck").GetValue<List<HMotionEyeNeckMale.EyeNeck>>();
                if (lst != null)
                {
                    GUILayout.BeginHorizontal();
                    foreach (string k in nameDict.Keys)
                    {
                        boolDict[k] = GUILayout.Toggle(boolDict[k], nameDict[k]);
                    }
                    GUILayout.EndHorizontal();
                    if (lst.Count > 0)
                    {
                        svPos = GUILayout.BeginScrollView(svPos);
                        for (int i = 0; i < lst.Count; i++)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label($"{lst[i].anim}", GUILayout.Width(150));
                            foreach (string k in nameDict.Keys)
                            {
                                if (boolDict[k])
                                {
                                    GUITemplate(k, i);
                                }
                            }
                            GUILayout.EndHorizontal();
                        }
                        GUILayout.EndScrollView();
                    }
                }
            }
        }

        public static List<HMotionEyeNeckMale.EyeNeck> lst;

        void GUITemplate(string target, int index)
        {
            object tmpData = typeof(HMotionEyeNeckMale.EyeNeck).GetField(target).GetValue(lst[index]);
            if (tmpData is int)
            {
                GUIIntT(target, index);
            }
            else //Vector3
            {
                GUIV3T(target, index);
            }
        }

        void GUIIntT(string target, int index)
        {
            enTmp = lst[index];
            GUILayout.BeginHorizontal(GUILayout.Width(uiLabelWidth));
            int data = (int)typeof(HMotionEyeNeckMale.EyeNeck).GetField(target).GetValue(enTmp);
            GUILayout.Label($"{nameDict[target]} {data}");
            TypedReference r = __makeref(enTmp);
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                if (data > 0) typeof(HMotionEyeNeckMale.EyeNeck).GetField(target).SetValueDirect(r, data - 1);
            }
            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                typeof(HMotionEyeNeckMale.EyeNeck).GetField(target).SetValueDirect(r, data + 1);
            }
            lst[index] = enTmp;
            GUILayout.Label("", GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
        }

        void GUIV3T(string target, int index)
        {
            enTmp = lst[index];
            //0
            Vector3 data = ((Vector3[])typeof(HMotionEyeNeckMale.EyeNeck).GetField(target).GetValue(enTmp))[0];
            TypedReference r = __makeref(enTmp);
            GUILayout.BeginHorizontal(GUILayout.Width(uiLabelWidth * 3));
            GUILayout.Label($"{nameDict[target]}A");
            GUILayout.Label($"x{data.x}", GUILayout.Width(uiLabelWidth2));
            var x = GUILayout.HorizontalSlider(data.x, 0, 360, GUILayout.Width(uiSliderWidth));
            GUILayout.Label($"y{data.y}", GUILayout.Width(uiLabelWidth2));
            var y = GUILayout.HorizontalSlider(data.y, 0, 360, GUILayout.Width(uiSliderWidth));
            GUILayout.Label($"z{data.z}", GUILayout.Width(uiLabelWidth2));
            var z = GUILayout.HorizontalSlider(data.z, 0, 360, GUILayout.Width(uiSliderWidth));
            Vector3[] values = new Vector3[] { new Vector3(x, y, z), ((Vector3[])typeof(HMotionEyeNeckMale.EyeNeck).GetField(target).GetValue(enTmp))[1] };
            typeof(HMotionEyeNeckMale.EyeNeck).GetField(target).SetValueDirect(r, values);
            lst[index] = enTmp;
            GUILayout.Label("", GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            //1
            data = ((Vector3[])typeof(HMotionEyeNeckMale.EyeNeck).GetField(target).GetValue(enTmp))[1];
            r = __makeref(enTmp);
            GUILayout.BeginHorizontal(GUILayout.Width(uiLabelWidth * 3));
            GUILayout.Label($"{nameDict[target]}B");
            GUILayout.Label($"x{data.x}", GUILayout.Width(uiLabelWidth2));
            x = GUILayout.HorizontalSlider(data.x, 0, 360, GUILayout.Width(uiSliderWidth));
            GUILayout.Label($"y{data.y}", GUILayout.Width(uiLabelWidth2));
            y = GUILayout.HorizontalSlider(data.y, 0, 360, GUILayout.Width(uiSliderWidth));
            GUILayout.Label($"z{data.z}", GUILayout.Width(uiLabelWidth2));
            z = GUILayout.HorizontalSlider(data.z, 0, 360, GUILayout.Width(uiSliderWidth));
            values = new Vector3[] { ((Vector3[])typeof(HMotionEyeNeckMale.EyeNeck).GetField(target).GetValue(enTmp))[0], new Vector3(x, y, z) };
            typeof(HMotionEyeNeckMale.EyeNeck).GetField(target).SetValueDirect(r, values);
            lst[index] = enTmp;
            GUILayout.Label("", GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
        }

        #region 读写数据

        public static void LoadOverrideData()
        {
            if (File.Exists($"{Paths.PluginPath}/AIMaleExpression/GlobalOverride.csv"))
            {
                //Debug.Log("男性表情插件:读取全局复写配置");
                nowOverrideData = new GlobalOverrideData(File.ReadAllLines($"{Paths.PluginPath}/AIMaleExpression/GlobalOverride.csv"));
            }
            else
            {
                Debug.Log("男性表情插件:没有找到全局复写配置，创建新配置文件");
                if (!Directory.Exists($"{Paths.PluginPath}/AIMaleExpression/")) Directory.CreateDirectory($"{Paths.PluginPath}/AIMaleExpression/");
                File.WriteAllText($"{Paths.PluginPath}/AIMaleExpression/GlobalOverride.csv", "动画,list名列表(以|分割),目開き,口開き,眉,目形,口舌形,首挙動,目挙動,首タゲ,首角度,,,,,,頭角度,,,,,,目タゲ,目角度,,,,,\n");
            }
        }

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
            if (!Directory.Exists(Paths.PluginPath + "/AIMaleExpression/"))
            {
                Directory.CreateDirectory(Paths.PluginPath + "/AIMaleExpression/");
            }
            File.WriteAllText(Paths.PluginPath + "/AIMaleExpression/" + nowList + ".csv", sb.ToString());
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
                        lst[i - 1] = ParseEyeNeckFromValues(vs);
                    }
                }
                OverrideData();
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
            if (!Directory.Exists(Paths.PluginPath + "/AIMaleExpression/"))
            {
                Directory.CreateDirectory(Paths.PluginPath + "/AIMaleExpression/");
            }
            File.WriteAllText(Paths.PluginPath + "/AIMaleExpression/" + nowList + ".MonoBehaviour", sb.ToString());
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
                        lst[i - 3] = ParseEyeNeckFromValues(vs);
                    }
                }
                OverrideData();
            }
        }

        public static HMotionEyeNeckMale.EyeNeck ParseEyeNeckFromValues(string[] vs)
        {
            MaleNeckData neck = new MaleNeckData(vs);
            return neck.ToEyeNeck();
        }

        public static Vector3 ParseV3(string x, string y, string z)
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

            if (LocalDialog.GetOpenFileName(openFileName))//点击系统对话框框保存按钮
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
                if (hscene)
                {
                    if (windowShow) GlobalMethod.setCameraMoveFlag(hscene.ctrlFlag.cameraCtrl, false);
                    else GlobalMethod.setCameraMoveFlag(hscene.ctrlFlag.cameraCtrl, true);
                }
            }
        }

        public static void OverrideData()
        {
            if (lst != null)
            {
                for (int i = 0; i < lst.Count; i++)
                {
                    MaleNeckData n = new MaleNeckData(lst[i]);
                    lst[i] = nowOverrideData.OverrideMaleNeckData(n).ToEyeNeck();
                }
            }
        }

        /// <summary>
        /// 尝试读取配置到当前表情
        /// </summary>
        /// <returns></returns>
        public static bool TryLoadConfigCSVToNow(bool refresh, HMotionEyeNeckMale male = null)
        {
            if (Directory.Exists($"{Paths.PluginPath}/AIMaleExpression/"))
            {
                string[] vs = null;
                //先加载全局重写CSV
                LoadOverrideData();
                List<HMotionEyeNeckMale.EyeNeck> list = null;
                if (refresh) list = lst;
                else list = Traverse.Create(male).Field("lstEyeNeck").GetValue<List<HMotionEyeNeckMale.EyeNeck>>();
                //尝试加载姿势CSV
                if (File.Exists($"{Paths.PluginPath}/AIMaleExpression/{nowList}.csv"))
                {
                    var lines = File.ReadAllLines($"{Paths.PluginPath}/AIMaleExpression/{nowList}.csv");
                    if (lines != null)
                    {
                        if (!refresh) list.Clear();
                        for (int i = 1; i < lines.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(lines[i]))
                            {
                                vs = lines[i].Split(',');
                                if (refresh) lst[i - 1] = ParseEyeNeckFromValues(vs);
                                else list.Add(ParseEyeNeckFromValues(vs));
                            }
                        }
                    }
                    if (refresh) OverrideData();
                    Debug.Log("男性表情插件:已读取插件配置覆盖原表情数据");
                    return true;
                }
                //尝试加载MonoBehaviour文本
                else if (File.Exists($"{Paths.PluginPath}/AIMaleExpression/{nowList}.MonoBehaviour"))
                {
                    var lines = File.ReadAllLines($"{Paths.PluginPath}/AIMaleExpression/{nowList}.csv");
                    if (lines != null)
                    {
                        if (!refresh) list.Clear();
                        for (int i = 3; i < lines.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(lines[i]))
                            {
                                string line = lines[i].Replace("><", ",");
                                line = line.Replace("<", "");
                                line = line.Replace(">", "");
                                vs = line.Split(',');
                                if (refresh) lst[i - 3] = ParseEyeNeckFromValues(vs);
                                else list.Add(ParseEyeNeckFromValues(vs));
                            }
                        }
                    }
                    if (refresh) OverrideData();
                    Debug.Log("男性表情插件:已读取插件配置覆盖原表情数据");
                    return true;
                }
                if (refresh) OverrideData();
            }
            return false;
        }

        /// <summary>
        /// 加载啪啪啪动作前补丁
        /// </summary>
        [HarmonyPatch(typeof(HMotionEyeNeckMale), "Load")]
        class HPatch
        {
            public static bool Prefix(HMotionEyeNeckMale __instance, string _assetpath, string _file)
            {
                Debug.Log($"男性表情:加载AB资源 路径:{_assetpath} 名称:{_file}");
                nowABPath = _assetpath;
                nowList = _file;
                if (TryLoadConfigCSVToNow(false, __instance))
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 加载啪啪啪动作后补丁
        /// </summary>
        [HarmonyPatch(typeof(HMotionEyeNeckMale), "Load")]
        class OverrideDataPatch
        {
            public static void Postfix(HMotionEyeNeckMale __instance)
            {
                if (nowOverrideData != null)
                {
                    //检查全局覆盖
                    var lst = Traverse.Create(__instance).Field("lstEyeNeck").GetValue<List<HMotionEyeNeckMale.EyeNeck>>();
                    if (lst != null)
                    {
                        for (int i = 0; i < lst.Count; i++)
                        {
                            MaleNeckData n = new MaleNeckData(lst[i]);
                            lst[i] = nowOverrideData.OverrideMaleNeckData(n).ToEyeNeck();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 切换动画时补丁
        /// </summary>
        [HarmonyPatch(typeof(ChaControl), "setPlay")]
        class HAnimPatch
        {
            public static void Postfix(string _strAnmName)
            {
                nowAnim = _strAnmName;
            }
        }

        public class GlobalOverrideData
        {
            List<GlobalOverrideRowData> DataList = new List<GlobalOverrideRowData>();

            public GlobalOverrideData(string[] lines)
            {
                for (int i = 1; i < lines.Length; i++)
                {
                    if (!string.IsNullOrEmpty(lines[i]))
                    {
                        DataList.Add(new GlobalOverrideRowData(lines[i]));
                    }
                }
            }

            public MaleNeckData OverrideMaleNeckData(MaleNeckData neck)
            {
                //Debug.Log($"男性表情插件:动画{neck.anim}开始检查复写");
                foreach (var d in DataList)
                {
                    //Debug.Log($"男性表情插件:{d.anim}");
                    if (d.anim == neck.anim)
                    {
                        //Debug.Log($"男性表情插件:动画{neck.anim}在全局复写中存在，尝试进行复写");
                        return d.OverrideMaleNeckData(neck);
                    }
                }
                return neck;
            }
        }

        public class GlobalOverrideRowData
        {
            public string anim;
            public List<string> lists = new List<string>();
            public int openEye;
            public bool bOpenEye;
            public int openMouth;
            public bool bOpenMouth;
            public int eyebrow;
            public bool bEyeBrow;
            public int eye;
            public bool bEye;
            public int mouth;
            public bool bMouth;
            public int Neckbehaviour;
            public bool bNeckbehaviour;
            public int Eyebehaviour;
            public bool bEyebehaviour;
            public int targetNeck;
            public bool bTargetNeck;
            public Vector3[] NeckRot;
            public bool bNeckRot;
            public Vector3[] HeadRot;
            public bool bHeadRot;
            public int targetEye;
            public bool bTargetEye;
            public Vector3[] EyeRot;
            public bool bEyeRot;

            public GlobalOverrideRowData(string line)
            {
                //Debug.Log("男性表情插件:开始解析 " + line);
                string[] vs = line.Split(',');
                Vector3[] v;
                anim = vs[0];
                //Debug.Log($"男性表情插件:读取到动画{anim}的配置");
                string[] ls = vs[1].Split('|');
                foreach (string l in ls)
                {
                    if (!string.IsNullOrEmpty(l)) lists.Add(l);
                }
                if (!string.IsNullOrEmpty(vs[2]))
                {
                    openEye = int.Parse(vs[2]);
                    bOpenEye = true;
                }
                if (!string.IsNullOrEmpty(vs[3]))
                {
                    openMouth = int.Parse(vs[3]);
                    bOpenMouth = true;
                }
                if (!string.IsNullOrEmpty(vs[4]))
                {
                    eyebrow = int.Parse(vs[4]);
                    bEyeBrow = true;
                }
                if (!string.IsNullOrEmpty(vs[5]))
                {
                    eye = int.Parse(vs[5]);
                    bEye = true;
                }
                if (!string.IsNullOrEmpty(vs[6]))
                {
                    mouth = int.Parse(vs[6]);
                    bMouth = true;
                }
                if (!string.IsNullOrEmpty(vs[7]))
                {
                    Neckbehaviour = int.Parse(vs[7]);
                    bNeckbehaviour = true;
                }
                if (!string.IsNullOrEmpty(vs[8]))
                {
                    Eyebehaviour = int.Parse(vs[8]);
                    bEyebehaviour = true;
                }
                if (!string.IsNullOrEmpty(vs[9]))
                {
                    targetNeck = int.Parse(vs[9]);
                    bTargetNeck = true;
                }
                if (!string.IsNullOrEmpty(vs[10]))
                {
                    v = new Vector3[] { Vector3.zero, Vector3.zero };
                    v[0] = ParseV3(vs[10], vs[11], vs[12]);
                    v[1] = ParseV3(vs[13], vs[14], vs[15]);
                    NeckRot = v;
                    bNeckRot = true;
                }
                if (!string.IsNullOrEmpty(vs[16]))
                {
                    v = new Vector3[] { Vector3.zero, Vector3.zero };
                    v[0] = ParseV3(vs[16], vs[17], vs[18]);
                    v[1] = ParseV3(vs[19], vs[20], vs[21]);
                    HeadRot = v;
                    bHeadRot = true;
                }
                if (!string.IsNullOrEmpty(vs[22]))
                {
                    targetEye = int.Parse(vs[22]);
                    bTargetEye = true;
                }
                if (!string.IsNullOrEmpty(vs[23]))
                {
                    v = new Vector3[] { Vector3.zero, Vector3.zero };
                    v[0] = ParseV3(vs[23], vs[24], vs[25]);
                    v[1] = ParseV3(vs[26], vs[27], vs[28]);
                    EyeRot = v;
                    bEyeRot = true;
                }
            }

            public MaleNeckData OverrideMaleNeckData(MaleNeckData neck)
            {
                //Debug.Log("男性表情插件:GlobalOverrideRowData:lists.Count:" + lists.Count);
                //foreach (var l in lists)
                //{
                //    Debug.Log("男性表情插件:GlobalOverrideRowData:lists:" + l);
                //}
                //Debug.Log("男性表情插件:nowList:" + nowList);
                if (lists.Count == 0 || lists.Contains(nowList))
                {
                    //Debug.Log("男性表情插件:该姿势符合复写条件，开始复写");
                    if (bOpenEye) neck.openEye = openEye;
                    if (bOpenMouth) neck.openMouth = openMouth;
                    if (bEyeBrow) neck.eyebrow = eyebrow;
                    if (bEye) neck.eye = eye;
                    if (bMouth) neck.mouth = mouth;
                    if (bNeckbehaviour) neck.Neckbehaviour = Neckbehaviour;
                    if (bEyebehaviour) neck.Eyebehaviour = Eyebehaviour;
                    if (bTargetNeck) neck.targetNeck = targetNeck;
                    if (bNeckRot) neck.NeckRot = NeckRot;
                    if (bHeadRot) neck.HeadRot = HeadRot;
                    if (bTargetEye) neck.targetEye = targetEye;
                    if (bEyeRot) neck.EyeRot = EyeRot;
                }
                return neck;
            }
        }

        public class MaleNeckData
        {
            public string anim;
            public int openEye;
            public int openMouth;
            public int eyebrow;
            public int eye;
            public int mouth;
            public int Neckbehaviour;
            public int Eyebehaviour;
            public int targetNeck;
            public Vector3[] NeckRot;
            public Vector3[] HeadRot;
            public int targetEye;
            public Vector3[] EyeRot;

            //从原版中拷贝数据构造
            public MaleNeckData(HMotionEyeNeckMale.EyeNeck neck)
            {
                anim = neck.anim;
                openEye = neck.openEye;
                openMouth = neck.openMouth;
                eyebrow = neck.eyebrow;
                eye = neck.eye;
                mouth = neck.mouth;
                Neckbehaviour = neck.Neckbehaviour;
                Eyebehaviour = neck.Eyebehaviour;
                targetNeck = neck.targetNeck;
                NeckRot = neck.NeckRot;
                HeadRot = neck.HeadRot;
                targetEye = neck.targetEye;
                EyeRot = neck.EyeRot;
            }

            //从list的csv行数据中构造
            public MaleNeckData(string[] vs)
            {
                Vector3[] v;
                anim = vs[0];
                openEye = int.Parse(vs[1]);
                openMouth = int.Parse(vs[2]);
                eyebrow = int.Parse(vs[3]);
                eye = int.Parse(vs[4]);
                mouth = int.Parse(vs[5]);
                Neckbehaviour = int.Parse(vs[6]);
                Eyebehaviour = int.Parse(vs[7]);
                targetNeck = int.Parse(vs[8]);
                v = new Vector3[] { Vector3.zero, Vector3.zero };
                v[0] = ParseV3(vs[9], vs[10], vs[11]);
                v[1] = ParseV3(vs[12], vs[13], vs[14]);
                NeckRot = v;
                v = new Vector3[] { Vector3.zero, Vector3.zero };
                v[0] = ParseV3(vs[15], vs[16], vs[17]);
                v[1] = ParseV3(vs[18], vs[19], vs[20]);
                HeadRot = v;
                targetEye = int.Parse(vs[21]);
                v = new Vector3[] { Vector3.zero, Vector3.zero };
                v[0] = ParseV3(vs[22], vs[23], vs[24]);
                v[1] = ParseV3(vs[25], vs[26], vs[27]);
                EyeRot = v;
            }

            //转换为原版表情
            public HMotionEyeNeckMale.EyeNeck ToEyeNeck()
            {
                HMotionEyeNeckMale.EyeNeck neck = new HMotionEyeNeckMale.EyeNeck();
                neck.anim = anim;
                neck.openEye = openEye;
                neck.openMouth = openMouth;
                neck.eyebrow = eyebrow;
                neck.eye = eye;
                neck.mouth = mouth;
                neck.Neckbehaviour = Neckbehaviour;
                neck.Eyebehaviour = Eyebehaviour;
                neck.targetNeck = targetNeck;
                neck.NeckRot = NeckRot;
                neck.HeadRot = HeadRot;
                neck.targetEye = targetEye;
                neck.EyeRot = EyeRot;
                return neck;
            }
        }
    }
}
