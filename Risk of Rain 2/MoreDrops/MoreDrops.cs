using RoR2;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using BepInEx.Configuration;
using UnityEngine.Networking;

namespace xiaoye97
{
    [BepInPlugin("me.xiaoye97.plugin.MoreDrops", "更多掉落物", "1.0")]
    public class MoreDrops : BaseUnityPlugin
    {
        public static ConfigEntry<int> configDropCount;
        public static ConfigEntry<bool> configRandom;
        void Start()
        {
            configDropCount = Config.Bind("Config", "DropCount", 2, "掉落数量");
            configRandom = Config.Bind("Config", "RandomDrop", true, "是否随机掉落");
            new Harmony("me.xiaoye97.plugin.MoreDrops").PatchAll();
        }

        [HarmonyPatch(typeof(ChestBehavior), "ItemDrop")]
        class DropPatch
        {
            public static bool Prefix(ChestBehavior __instance)
            {
                if (!NetworkServer.active) return false;
                PickupIndex dropPickup = Traverse.Create(__instance).Field("dropPickup").GetValue<PickupIndex>();
                if(dropPickup == PickupIndex.none) return false;
                for (int i = 0; i < configDropCount.Value; i++)
                {
                    dropPickup = Traverse.Create(__instance).Field("dropPickup").GetValue<PickupIndex>();
                    PickupDropletController.CreatePickupDroplet(dropPickup, __instance.dropTransform.position + Vector3.up * 1.5f, Vector3.up * __instance.dropUpVelocityStrength + __instance.dropTransform.forward * __instance.dropForwardVelocityStrength);
                    if(configRandom.Value) __instance.RollItem();
                }
                Traverse.Create(__instance).Field("dropPickup").SetValue(PickupIndex.none);
                return false;
            }
        }
    }
}
