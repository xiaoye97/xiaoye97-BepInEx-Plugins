using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using BepInEx.Configuration;

namespace AdvancedBuildDestruct
{
    [BepInPlugin("me.xiaoye97.plugin.Dyson.AdvancedBuildDestruct", "AdvancedBuildDestruct", "1.0.0")]
    public class AdvancedBuildDestruct : BaseUnityPlugin
    {
        public static ConfigEntry<float> FindBuildDistance;
        public static ConfigEntry<KeyCode> BuildKey, DestructKey;
        void Start()
        {
            FindBuildDistance = Config.Bind<float>("config", "FindBuildDistance", 10f, "建筑查询的距离");
            BuildKey = Config.Bind<KeyCode>("config", "BuildKey", KeyCode.LeftAlt, "进行连锁建造的按键");
            DestructKey = Config.Bind<KeyCode>("config", "DestructKey", KeyCode.LeftShift, "进行连锁拆除的按键");
            Harmony.CreateAndPatchAll(typeof(BuildPatch));
            Harmony.CreateAndPatchAll(typeof(DestructPatch));
        }
    }
}
