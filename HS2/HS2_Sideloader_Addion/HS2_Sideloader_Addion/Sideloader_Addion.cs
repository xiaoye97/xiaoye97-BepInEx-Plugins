using System;
using BepInEx;
using HarmonyLib;

namespace me.xiaoye97.plugin.HS2.SideLoader_Addion
{
    [BepInPlugin("_me.xiaoye97.plugin.HS2.SideLoader_Addion", "SideLoader扩展", "1.0")]
    public class SideLoader_Addion : BaseUnityPlugin
    {
        public static Harmony harmony;
        public static bool tmpFlag, loading;

        void Awake()
        {
            harmony = new Harmony("me.xiaoye97.plugin.HS2.SideLoader_Addion");
            harmony.Patch(
                AccessTools.Method(typeof(Sideloader.Sideloader), "LoadModsFromDirectories"),
                new HarmonyMethod(AccessTools.Method(typeof(SideLoader_Addion), "LoadDirPatchPre")),
                new HarmonyMethod(AccessTools.Method(typeof(SideLoader_Addion), "LoadDirPatchPost")));
        }

        public static bool LoadDirPatchPre()
        {
            loading = true;
            harmony.Patch(
                AccessTools.Method(typeof(String), "EndsWith", new Type[] { typeof(string), typeof(StringComparison) }),
                new HarmonyMethod(AccessTools.Method(typeof(SideLoader_Addion), "StringEndWithPre")), null);
            return true;
        }

        public static void LoadDirPatchPost()
        {
            loading = false;
        }

        public static bool StringEndWithPre(string value, String __instance, ref bool __result)
        {
            if (!loading) return true;
            if (value == ".zip" || value == ".zipmod")
            {
                if (!tmpFlag)
                {
                    tmpFlag = true;
                    if (__instance.EndsWith(".mod", StringComparison.OrdinalIgnoreCase))
                    {
                        __result = true;
                        tmpFlag = false;
                        return false;
                    }
                    tmpFlag = false;
                }
            }
            return true;
        }
    }
}
