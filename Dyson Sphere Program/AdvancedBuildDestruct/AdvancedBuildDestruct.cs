using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using BepInEx.Configuration;

namespace AdvancedBuildDestruct
{
    [BepInPlugin("me.xiaoye97.plugin.Dyson.AdvancedBuildDestruct", "AdvancedBuildDestruct", "1.0.5")]
    public class AdvancedBuildDestruct : BaseUnityPlugin
    {
        public static ConfigEntry<float> FindBuildDistance;
        public static ConfigEntry<KeyCode> BuildKey, DestructKey;
        public static ConfigEntry<int> BuildExtraSpacing;

        public static bool buildKeyUp;
        public static float buildKeyCD;
        private UIBuildMenu UIBuildMenu;

        void Start()
        {
            FindBuildDistance = Config.Bind<float>("config", "FindBuildDistance", 10f, "拆除时建筑查询的距离");
            BuildExtraSpacing = Config.Bind<int>("config", "BuildExtraSpacing", 0, "建造额外间距");
            BuildKey = Config.Bind<KeyCode>("config", "BuildKey", KeyCode.LeftAlt, "进行连锁建造的按键");
            DestructKey = Config.Bind<KeyCode>("config", "DestructKey", KeyCode.LeftShift, "进行连锁拆除的按键");
            if (BuildExtraSpacing.Value < 0)
            {
                BuildExtraSpacing.Value = 0;
            }
            if (FindBuildDistance.Value < 0)
            {
                FindBuildDistance.Value = 0;
            }
            Harmony.CreateAndPatchAll(typeof(BuildPatch));
            Harmony.CreateAndPatchAll(typeof(DestructPatch));
            Harmony.CreateAndPatchAll(typeof(AdvancedBuildDestruct));
        }

        void Update()
        {
            if (buildKeyUp)
            {
                buildKeyCD -= Time.deltaTime;
                if (buildKeyCD < 0)
                {
                    buildKeyUp = false;
                }
            }
            if (Input.GetKeyUp(BuildKey.Value))
            {
                buildKeyUp = true;
                buildKeyCD = 1f;
            }
            if (BuildPatch.begin)
            {
                if (Input.GetKeyUp(KeyCode.Equals))
                {
                    BuildExtraSpacing.Value++;
                }
                if (Input.GetKeyUp(KeyCode.Minus))
                {
                    if (BuildExtraSpacing.Value > 0)
                    {
                        BuildExtraSpacing.Value--;
                        if (BuildExtraSpacing.Value < 0)
                        {
                            BuildExtraSpacing.Value = 0;
                        }
                    }
                }
            }
            if (UIBuildMenu == null)
            {
                if (UIRoot._instance != null && UIRoot._instance.uiGame != null && UIRoot._instance.uiGame.buildMenu != null)
                {
                    UIBuildMenu = UIRoot._instance.uiGame.buildMenu;
                }
            }
            else
            {
                if (UIBuildMenu.isRemoveMode)
                {
                    if (Input.GetKeyUp(KeyCode.Equals))
                    {
                        FindBuildDistance.Value++;
                    }
                    if (Input.GetKeyUp(KeyCode.Minus))
                    {
                        if (FindBuildDistance.Value > 0)
                        {
                            FindBuildDistance.Value--;
                        }
                    }
                }
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(UIGeneralTips), "_OnUpdate")]
        public static void TipsPatch(UIGeneralTips __instance)
        {
            if (UIGame.viewMode == EViewMode.Build)
            {
                int mode = __instance.gameData.mainPlayer.controller.cmd.mode;
                if (mode == -1) // 拆除模式
                {
                    __instance.modeText.text = "拆除模式".Translate() + $" {FindBuildDistance.Value.ToString("0")}";
                }
                else if (mode >= 0) // 建造模式
                {
                    if (BuildPatch.begin)
                    {
                        __instance.modeText.text = "建造模式".Translate() + $" {BuildExtraSpacing.Value}";
                    }
                }
            }
        }
    }
}
