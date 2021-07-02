using BepInEx;
using GUIPackage;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MCSDataHelper
{
    public static class DataPatch
    {
        /// <summary>
        /// Json数据的转储
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(jsonData), "init")]
        public static bool InitJsonPatch(string path)
        {
            if (DataHelper.DumpConfig.Value) // 转储数据
            {
                if (!Directory.Exists($"{Paths.GameRootPath}/Dump"))
                {
                    Directory.CreateDirectory($"{Paths.GameRootPath}/Dump");
                }
                TextAsset textAsset = (TextAsset)Resources.Load(path);
                if (textAsset != null)
                {
                    string[] tmp = path.Split('/');
                    string fileName = tmp[tmp.Length - 1];
                    Debug.Log($"转储：{fileName}");
                    string text = textAsset.text.UnCode64();
                    File.WriteAllText($"{Paths.GameRootPath}/Dump/{fileName}.json", text);
                }
            }
            return true;
        }

        /// <summary>
        /// 递归查找json
        /// </summary>
        private static void AddJson(List<string> paths, string rootpath, string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            foreach (var file in dir.GetFiles())
            {
                if (rootpath.EndsWith(file.Name))
                    paths.Add(file.FullName);
            }
            foreach (var d in dir.GetDirectories())
            {
                AddJson(paths, rootpath, d.FullName);
            }
        }

        /// <summary>
        /// Josn数据的加载
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(jsonData), "init")]
        public static void ItemJsonPatchPost(string path, ref JSONObject jsondata)
        {
            if (jsondata.Count == 0)
            {
                return;
            }
            if (!Directory.Exists($"{Paths.GameRootPath}/Mods"))
            {
                Directory.CreateDirectory($"{Paths.GameRootPath}/Mods");
            }
            // 读取json列表
            List<string> jsonPathList = new List<string>();
            AddJson(jsonPathList, path, $"{Paths.GameRootPath}/Mods");
            if (jsonPathList.Count > 0)
            {
                foreach (var jsonPath in jsonPathList)
                {
                    var json = File.ReadAllText(jsonPath);
                    json = json.ToUnicode();
                    JSONObject jobj = new JSONObject(json, -2, false, false);
                    foreach (var j in jobj.list)
                    {
                        jsondata.AddField(j["id"].I.ToString(), j);
                    }
                }
                //File.WriteAllText(Paths.GameRootPath + "/ModLog.txt", jsondata.ToString());
            }
        }

        /// <summary>
        /// 修复Mod物品图标的加载
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(item), MethodType.Constructor, new Type[] { typeof(string), typeof(int), typeof(string), typeof(string), typeof(int), typeof(item.ItemType), typeof(int) })]
        public static void ItemIconPatch(item __instance)
        {
            // Mod物品的ID不能大于100000，大于100000的部分为请教功法占用，有特殊处理
            if (__instance.itemID >= 100000) return;
            if (__instance.itemID.ItemJson().HasField("ModIcon"))
            {
                var tex = DataHelper.GetTex(__instance.itemID.ItemJson()["ModIcon"].str);
                __instance.itemIcon = tex;
            }
        }
    }
}