using BepInEx;
using AIProject;
using HarmonyLib;

namespace AI0CD
{
    [BepInPlugin("me.xiaoye97.plugin.AI0CD", "采集无CD", "1.0")]
    public class AI0CD : BaseUnityPlugin
    {
        void Start()
        {
            new Harmony("me.xiaoye97.plugin.AI0CD").PatchAll();
        }

        [HarmonyPatch(typeof(EnvironmentProfile), "SearchCoolTimeDuration", MethodType.Getter)]
        class CDPatch
        {
            public static bool Prefix(ref float __result)
            {
                __result = 0;
                return false;
            }
        }
    }
}
