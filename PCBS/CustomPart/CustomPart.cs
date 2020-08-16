using BepInEx;
using System.IO;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace CustomPart
{
    [BepInPlugin("me.xiaoye97.plugin.PCBS.CustomPart", "自定义配件", "1.0")]
    public class CustomPart : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource logger;
        void Start()
        {
            logger = Logger;
            new Harmony("me.xiaoye97.plugin.PCBS.CustomPart").PatchAll();
        }

        [HarmonyPatch(typeof(PartsDatabase), "ImportFromHTML")]
        class LoadAssetPatch
        {
            public static void Postfix(PartsDatabase __instance, TextAsset asset)
            {
                HTMLTableReader htmltableReader = new HTMLTableReader(asset);
                DirectoryInfo dir = new DirectoryInfo(Paths.GameRootPath + "/CustomPart/" + asset.name);
                if (!dir.Exists) dir.Create(); //创建文件夹
                else
                {
                    //获取文件
                    var files = dir.GetFiles("*.txt", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        var lines = File.ReadAllLines(file.FullName);
                        if (lines.Length > 0)
                        {
                            Dictionary<string, string> partData = new Dictionary<string, string>();
                            foreach (var line in lines)
                            {
                                //切割key value
                                var kv = line.Split('=');
                                if (kv.Length > 1 && !string.IsNullOrEmpty(kv[0]))
                                {
                                    partData.Add(kv[0], kv[1]);
                                }
                            }
                            //判断Part Type
                            if (!partData.ContainsKey("Part Type"))
                            {
                                logger.LogError(file.FullName + " 没有Part Type数据");
                            }
                            //判断InGame
                            if (!partData.ContainsKey("In Game"))
                            {
                                logger.LogError(file.FullName + " 没有In Game数据");
                            }
                            else
                            {
                                //判断InGame Yes 或者 No
                                if (partData["In Game"] == "Yes" || partData["In Game"] == "No")
                                {
                                    PartDesc partDesc = PartDesc.Create(partData["Part Type"], new GetString(htmltableReader.Describe));
                                    foreach (var key in partData.Keys)
                                    {
                                        try
                                        {
                                            //导入数据
                                            partDesc.ImportProp(key, partData[key], new GetString(htmltableReader.Describe));
                                        }
                                        catch (System.Exception e)
                                        {
                                            logger.LogError(htmltableReader.Describe() + ": " + e.ToString());
                                        }
                                    }
                                    //判断id
                                    if (partDesc.m_id == null)
                                    {
                                        logger.LogError(file.FullName + " 的ID为空");
                                    }
                                    if (__instance.m_parts.ContainsKey(partDesc.m_id))
                                    {
                                        logger.LogError(file.FullName + " 的ID已经存在");
                                    }
                                    __instance.m_parts[partDesc.m_id] = partDesc;
                                }
                                else
                                {
                                    logger.LogWarning(file.FullName + " 的In Game数据不为Yes 或者 No");
                                }
                            }
                        }

                    }
                }
            }
        }

        [HarmonyPatch(typeof(PartInstance), "FixForVersion")]
        class LoadSavePatch
        {
            public static void Postfix(PartInstance __instance, ref bool __result)
            {
                if(__result)
                {
                    PartsDatabase pdb = Traverse.Create<PartsDatabase>().Field("s_instance").GetValue<PartsDatabase>();
                    if (!pdb.m_parts.ContainsKey(__instance.GetPartId()))
                    {
                        logger.LogInfo(__instance.GetPartId() + "在数据库中不存在，将进行移除");
                        __result = false;
                    }
                }
            }
        }
    }
}
