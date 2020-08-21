using BepInEx;
using HarmonyLib;
using UnityEngine;
using BepInEx.Configuration;

namespace me.xiaoye97.plugin.PCBS.HideCable
{
    [BepInPlugin("me.xiaoye97.plugin.PCBS.HideCable", "隐藏电源线", "1.0")]
    public class HideCable : BaseUnityPlugin
    {
        public static ConfigEntry<bool> isHide;
        void Start()
        {
            isHide = Config.Bind("设置", "是否隐藏电源线", true);
            isHide.SettingChanged += (o, e) =>
            {
                var cables = GameObject.FindObjectsOfType<CableInstance>();
                foreach (var cable in cables)
                {
                    var r = cable.GetComponent<Renderer>();
                    if (r != null) r.enabled = !isHide.Value;
                }
            };
            new Harmony("me.xiaoye97.plugin.PCBS.HideCable").PatchAll();
        }


        [HarmonyPatch(typeof(CableInstance), "Start")]
        class CablePatch
        {
            public static void Postfix(CableInstance __instance)
            {
                var r = __instance.GetComponent<Renderer>();
                if (r != null) r.enabled = !isHide.Value;
            }
        }
    }
}