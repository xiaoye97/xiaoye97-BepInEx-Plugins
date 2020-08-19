using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using BepInEx.Configuration;
using System.Collections.Generic;
using RuntimeUnityEditor.Core.Utils;
using RuntimeUnityEditor.Core.ObjectTree;

namespace RuntimeUnityEditorAddion
{
    [BepInPlugin("me.xiaoye97.plugin.Unity.RuntimeUnityEditorAddion", "运行时Unity编辑器扩展", "1.0")]
    public class RuntimeUnityEditorAddion : BaseUnityPlugin
    {
        public static ConfigEntry<bool> filterUnload;
        public static ConfigEntry<bool> disableSort;
        void Start()
        {
            filterUnload = Config.Bind<bool>("Setting", "Filter Unload", true);
            disableSort = Config.Bind<bool>("Setting", "Diasble Sort", true);
            new Harmony("me.xiaoye97.plugin.Unity.RuntimeUnityEditorAddion").PatchAll();
        }

        [HarmonyPatch(typeof(GameObjectSearcher), "FindAllRootGameObjects")]
        class FindPatch
        {
            public static void Postfix(ref IEnumerable<GameObject> __result)
            {
                if (!filterUnload.Value) return;
                List<GameObject> objs = new List<GameObject>();
                foreach (var obj in __result)
                {
                    if (obj.scene.isLoaded)
                    {
                        objs.Add(obj);
                    }
                }
                __result = objs;
            }
        }

        [HarmonyPatch(typeof(GameObjectSearcher), "Refresh")]
        class RefreshPatch
        {
            public static bool Prefix(GameObjectSearcher __instance, bool full, Predicate<GameObject> objectFilter)
            {
                if (!disableSort.Value) return true;
                var _searchResults = Traverse.Create(__instance).Field("_searchResults").GetValue<List<GameObject>>();
                var _cachedRootGameObjects = Traverse.Create(__instance).Field("_cachedRootGameObjects").GetValue<List<GameObject>>();
                if (_searchResults != null)
                {
                    return false;
                }
                if (_cachedRootGameObjects == null || full)
                {
                    _cachedRootGameObjects = GameObjectSearcher.FindAllRootGameObjects().ToList();
                    full = true;
                }
                else
                {
                    _cachedRootGameObjects.RemoveAll((GameObject o) => o == null);
                }
                if (UnityFeatureHelper.SupportsScenes && !full)
                {
                    List<GameObject> list = UnityFeatureHelper.GetSceneGameObjects().Except(_cachedRootGameObjects).ToList<GameObject>();
                    if (list.Count > 0)
                    {
                        _cachedRootGameObjects.AddRange(list);
                    }
                }
                if (objectFilter != null)
                {
                    _cachedRootGameObjects.RemoveAll(objectFilter);
                }
                Traverse.Create(__instance).Field("_cachedRootGameObjects").SetValue(_cachedRootGameObjects);
                return false;
            }
        }
    }
}
