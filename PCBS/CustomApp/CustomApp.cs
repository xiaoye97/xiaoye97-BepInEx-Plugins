using System;
using BepInEx;
using System.IO;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace xiaoye97
{
    [BepInPlugin("me.xiaoye97.plugin.CustomApp", "装机模拟器自定义APP", "1.0")]
    [BepInProcess("PCBS.exe")]
    public class CustomApp : BaseUnityPlugin
    {
        public static List<OSProgramDesc> ProgramList = new List<OSProgramDesc>();

        void Start()
        {
            SearchApps();
            new Harmony("me.xiaoye97.plugin.CustomApp").PatchAll();
        }

        public void Log(string msg)
        {
            Logger.Log(BepInEx.Logging.LogLevel.Info, msg);
        }

        private void SearchApps()
        {
            if (!Directory.Exists($"{Paths.PluginPath}\\CustomApp")) return;
            DirectoryInfo rootPath = new DirectoryInfo($"{Paths.PluginPath}\\CustomApp");
            if(!rootPath.Exists)
            {
                Log("根目录不存在");
                return;
            }
            foreach(var appdir in rootPath.GetDirectories())
            {
                if(File.Exists($"{appdir.FullName}\\app.json"))
                {
                    var desc = JsonUtility.FromJson<CustomAppDesc>(File.ReadAllText($"{appdir.FullName}\\app.json"));
                    LoadApp(appdir.FullName, desc);
                }
            }
        }

        /// <summary>
        /// 加载App
        /// </summary>
        public void LoadApp(string appDir, CustomAppDesc appDesc)
        {
            if (!string.IsNullOrEmpty(appDesc.DLLName))
            {
                Log($"加载 {appDesc.AppID} 的程序集引用{appDesc.DLLName}");
                AppDomain.CurrentDomain.Load(appDesc.DLLName);
            }
            var desc = gameObject.AddComponent<OSProgramDesc>();
            AssetBundle ab = AssetBundle.LoadFromFile($"{appDir}\\{appDesc.ABName}");
            desc.m_id = appDesc.AppID;
            desc.m_icon = ab.LoadAsset<Sprite>(appDesc.IconName);
            desc.m_panel = ab.LoadAsset<GameObject>(appDesc.PrefabName);
            desc.m_minWidth = appDesc.MinWidth;
            desc.m_minHeight = appDesc.MinHeight;
            desc.m_initWidth = appDesc.InitWidth;
            desc.m_initHeight = appDesc.InitHeight;
            desc.m_installTime = appDesc.InstallTime;
            desc.m_removeTime = appDesc.RemoveTime;
            desc.m_resizable = appDesc.Resizable;
            ProgramList.Add(desc);
        }

        [Serializable]
        public class CustomAppDesc
        {
            /// <summary>
            /// App的ID(名字)
            /// </summary>
            public string AppID;

            /// <summary>
            /// AB包名称
            /// </summary>
            public string ABName;

            /// <summary>
            /// 需要引用的DLL名称(没有就留空)
            /// </summary>
            public string DLLName;

            /// <summary>
            /// APP图标的文件名
            /// </summary>
            public string IconName;

            /// <summary>
            /// 程序界面预制体
            /// </summary>
            public string PrefabName;

            /// <summary>
            /// 最小宽度
            /// </summary>
            public float MinWidth = 100;

            /// <summary>
            /// 最小高度
            /// </summary>
            public float MinHeight = 100;

            /// <summary>
            /// 实例化时默认宽度
            /// </summary>
            public float InitWidth = 100;

            /// <summary>
            /// 实例化时默认高度
            /// </summary>
            public float InitHeight = 100;

            /// <summary>
            /// 安装所需时间
            /// </summary>
            public int InstallTime = 10;

            /// <summary>
            /// 卸载所需时间
            /// </summary>
            public int RemoveTime = 10;

            /// <summary>
            /// 是否可以改变窗口大小
            /// </summary>
            public bool Resizable = true;
        }

        #region 补丁
        [HarmonyPatch(typeof(PartsDatabase), "Load")]
        class DataBaseLoadPatch
        {
            public static void Postfix(PartsDatabase __instance)
            {
                List<OSProgramDesc> list = new List<OSProgramDesc>();
                foreach(var prog in PartsDatabase.GetAllPrograms())
                {
                    list.Add(prog);
                }
                foreach(var prog in ProgramList)
                {
                    list.Add(prog);
                }
                Traverse.Create(__instance).Field<OSProgramDesc[]>("m_programs").Value = list.ToArray();
            }
        }

        [HarmonyPatch(typeof(CareerStatus), "IsProgramAvailableForInstall")]
        class InstallAppPatch
        {
            public static bool Prefix(string progId, ref bool __result)
            {
                foreach (var prog in ProgramList)
                {
                    if(prog.m_id == progId)
                    {
                        __result = true;
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(OSProgramDesc), "m_uiName", MethodType.Getter)]
        class LocalizedPatch
        {
            public static bool Prefix(OSProgramDesc __instance, ref string __result)
            {
                foreach (var prog in ProgramList)
                {
                    if (prog.m_id == __instance.m_id)
                    {
                        __result = __instance.m_id;
                        return false;
                    }
                }
                return true;
            }
        }
        #endregion
    }
}
