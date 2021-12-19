using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using BepInEx.Configuration;

namespace AdvancedBuildDestruct
{
    [BepInPlugin("me.xiaoye97.plugin.Dyson.AdvancedBuildDestruct", "AdvancedBuildDestruct", "1.0.5")]
    public class AdvancedBuildDestruct : BaseUnityPlugin
    {
        Harmony harmony;

        public static ConfigEntry<float> FindBuildDistance;
        public static ConfigEntry<KeyCode> BuildKey, DestructKey;
        public static ConfigEntry<int> BuildExtraSpacing;

        public static bool buildKeyUp;
        public static float buildKeyCD;
        private UIBuildMenu UIBuildMenu;

        public static List<UIKeyTipNode> allTips;
        public static UIKeyTipNode tipBuildToggle;
        public static UIKeyTipNode tipBuildPlus;
        public static UIKeyTipNode tipBuildMinus;
        public static UIKeyTipNode tipDestructPlus;
        public static UIKeyTipNode tipDestructMinus;

        private static PlayerController _pc;
        internal static PlayerController pc
        {
            get
            {
                if (_pc == null)
                {
                    var go = GameObject.Find("Player (Icarus)");
                    _pc = go.GetComponent<PlayerController>();
                }
                return _pc;
            }
        }

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

            harmony = new Harmony("me.xiaoye97.plugin.Dyson.AdvancedBuildDestruct");
            try
            {
                harmony.PatchAll(typeof(BuildPatch));
                harmony.PatchAll(typeof(DestructPatch));
                harmony.PatchAll(typeof(AdvancedBuildDestruct));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
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

        internal void OnDestroy()
        {
            harmony.UnpatchSelf();  // For ScriptEngine hot-reloading
            allTips.Remove(tipBuildToggle);
            allTips.Remove(tipBuildPlus);
            allTips.Remove(tipBuildMinus);
            allTips.Remove(tipDestructPlus);
            allTips.Remove(tipDestructMinus);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(UIKeyTips), "UpdateTipDesiredState")]
        public static void UpdateTipDesiredStatePatch(UIKeyTips __instance, ref List<UIKeyTipNode> ___allTips)
        {
            if (!tipBuildToggle)
            {
                allTips = ___allTips;
                tipBuildToggle = __instance.RegisterTip("ALT", "Toggle repeated build");
                tipBuildPlus = __instance.RegisterTip("+", "Increase build gap");
                tipBuildMinus = __instance.RegisterTip("-", "Decrease build gap");
                tipDestructPlus = __instance.RegisterTip("+", "Increase area");
                tipDestructMinus = __instance.RegisterTip("-", "Decrease area");
            }
            int mode = pc.cmd.mode;
            tipBuildToggle.desired= UIGame.viewMode == EViewMode.Build && mode >= 0;
            tipBuildPlus.desired= UIGame.viewMode == EViewMode.Build && mode >= 0 && BuildPatch.begin;
            tipBuildMinus.desired= UIGame.viewMode == EViewMode.Build && mode >= 0 && BuildPatch.begin;
            tipDestructPlus.desired= UIGame.viewMode == EViewMode.Build && mode == -1;
            tipDestructMinus.desired= UIGame.viewMode == EViewMode.Build && mode == -1;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(UIGeneralTips), "_OnUpdate")]
        public static void TipsPatch(ref Text ___modeText)
        {
            if (UIGame.viewMode == EViewMode.Build)
            {
                int mode = pc.cmd.mode;
                if (mode == -1) // 拆除模式
                {
                    ___modeText.text += $" - Area {FindBuildDistance.Value.ToString("0")}";
                }
                else if (mode >= 0) // 建造模式
                {
                    if (BuildPatch.begin)
                    {
                        ___modeText.text += $" - Spacing {BuildExtraSpacing.Value}";
                    }
                }
            }
        }
    }
}
