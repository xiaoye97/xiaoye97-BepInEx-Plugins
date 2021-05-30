using BepInEx;
using HarmonyLib;
using UnityEngine;
using BepInEx.Configuration;

namespace xiaoye97
{
    [BepInPlugin("me.xiaoye97.plugin.Dyson.BiggerBuildRange", "BiggerBuildRange", "1.0.0")]
    public class BiggerBuildRange : BaseUnityPlugin
    {
        private static ConfigEntry<int> RangeConfig;

        private void Start()
        {
            RangeConfig = Config.Bind<int>("config", "BuildRange", 50, "建造范围");
            Harmony.CreateAndPatchAll(typeof(BiggerBuildRange));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BuildTool_Click), "_OnInit")]
        public static void ChangeBuildRange(BuildTool_Click __instance)
        {
            __instance.dotsSnapped = new Vector3[RangeConfig.Value];
        }
    }
}