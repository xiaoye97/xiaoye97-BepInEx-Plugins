using RoR2;
using System;
using BepInEx;
using HarmonyLib;
using RoR2.Stats;
using BepInEx.Harmony;
using UnityEngine.Networking;

namespace xiaoye97
{
    [BepInPlugin("me.xiaoye97.plugin.R2UnlockAll", "解锁全部", "1.0")]
    public class R2UnlockAll : BaseUnityPlugin
    {
        public Harmony harmony;
        void Start()
        {
            HarmonyWrapper.PatchAll(typeof(UnlockPatch));
        }

        class UnlockPatch
        {
            public static bool Patch1(ref bool __result)
            {
                __result = true;
                return false;
            }

            public static bool Patch2(ref bool __result)
            {
                if (!NetworkServer.active) __result = false;
                else __result = true;
                return false;
            }

            [HarmonyPatch(typeof(UserProfile), "HasUnlockable", new Type[] { typeof(string) })]
            [HarmonyPrefix]
            public static bool HasUnlockable_String(ref bool __result) => Patch1(ref __result);

            [HarmonyPatch(typeof(UserProfile), "HasUnlockable", new Type[] { typeof(UnlockableDef) })]
            [HarmonyPrefix]
            public static bool HasUnlockable_UnlockableDef(ref bool __result) => Patch1(ref __result);

            [HarmonyPatch(typeof(UserProfile), "HasSurvivorUnlocked")]
            [HarmonyPrefix]
            public static bool HasSurvivorUnlocked(ref bool __result) => Patch1(ref __result);

            [HarmonyPatch(typeof(UserProfile), "HasDiscoveredPickup")]
            [HarmonyPrefix]
            public static bool HasDiscoveredPickup(ref bool __result) => Patch1(ref __result);

            [HarmonyPatch(typeof(UserProfile), "HasAchievement")]
            [HarmonyPrefix]
            public static bool HasAchievement(ref bool __result) => Patch1(ref __result);

            [HarmonyPatch(typeof(UserProfile), "CanSeeAchievement")]
            [HarmonyPrefix]
            public static bool CanSeeAchievement(ref bool __result) => Patch1(ref __result);

            [HarmonyPatch(typeof(StatSheet), "HasUnlockable")]
            [HarmonyPrefix]
            public static bool HasUnlockable(ref bool __result) => Patch1(ref __result);

            [HarmonyPatch(typeof(Run), "IsUnlockableUnlocked")]
            [HarmonyPrefix]
            public static bool IsUnlockableUnlocked(ref bool __result) => Patch2(ref __result);

            [HarmonyPatch(typeof(Run), "DoesEveryoneHaveThisUnlockableUnlocked")]
            [HarmonyPrefix]
            public static bool DoesEveryoneHaveThisUnlockableUnlocked(ref bool __result) => Patch2(ref __result);

            [HarmonyPatch(typeof(PreGameController), "AnyUserHasUnlockable")]
            [HarmonyPrefix]
            public static bool AnyUserHasUnlockable(ref bool __result) => Patch1(ref __result);
        }
    }
}
