using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using BepInEx.Configuration;
using System.Collections.Generic;
using RuntimeUnityEditor.Core.Utils;
using RuntimeUnityEditor.Core.Inspector;
using RuntimeUnityEditor.Core.ObjectTree;
using RuntimeUnityEditor.Core.Inspector.Entries;

namespace RuntimeUnityEditorAddion
{
    [BepInPlugin("me.xiaoye97.plugin.Unity.RuntimeUnityEditorAddion", "运行时Unity编辑器扩展", "1.2")]
    [BepInDependency("RuntimeUnityEditor", BepInDependency.DependencyFlags.HardDependency)]
    public class RuntimeUnityEditorAddion : BaseUnityPlugin
    {
        public static ConfigEntry<bool> filterUnload, disableSort, showColor, showTexture;
        void Start()
        {
            filterUnload = Config.Bind<bool>("Setting", "Filter Unload", true);
            disableSort = Config.Bind<bool>("Setting", "Diasble Sort", true);
            showColor = Config.Bind<bool>("Setting", "Show Color", true);
            showTexture = Config.Bind<bool>("Setting", "Show Texture", true);
            new Harmony("me.xiaoye97.plugin.Unity.RuntimeUnityEditorAddion").PatchAll();
        }

        #region 过滤未加载的物体
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
        #endregion

        #region 禁止按字母排序
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
        #endregion

        #region 显示颜色&显示贴图
        public static Color cacheColor, backColor;
        public static Texture2D cacheTexture;
        private static bool startShowColor = false, startShowTexture = false;
        public static RenderTexture cacheRenderTexture;
        [HarmonyPatch(typeof(Inspector), "DrawSingleContentEntry")]
        class InspectorPatch
        {
            public static bool Prefix(ICacheEntry entry)
            {
                switch(entry.TypeName())
                {
                    case "UnityEngine.Color":
                        ColorFix(entry);
                        break;
                    case "UnityEngine.Texture2D":
                        Texture2DFix(entry);
                        break;
                    case "UnityEngine.Sprite":
                        SpriteFix(entry);
                        break;
                    default: 
                        break;
                }
                return true;
            }

            public static void ColorFix(ICacheEntry entry)
            {
                cacheColor = (Color)entry.GetValue();
                backColor = GUI.contentColor;
                startShowColor = true;
            }

            public static void Texture2DFix(ICacheEntry entry)
            {
                cacheTexture = (Texture2D)entry.GetValue();
                if(cacheTexture != null)
                {
                    startShowTexture = true;
                }
            }

            public static void SpriteFix(ICacheEntry entry)
            {
                Sprite sprite = (Sprite)entry.GetValue();
                if(sprite != null)
                {
                    cacheTexture = sprite.texture;
                    if (cacheTexture != null)
                    {
                        startShowTexture = true;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(GUILayout), "Label", new Type[] {typeof(string), typeof(GUILayoutOption[]) })]
        class InspectorPatch2
        {
            public static bool Prefix(ref GUILayoutOption[] options)
            {
                if(showColor.Value)
                {
                    if(startShowColor)
                    {
                        startShowColor = false;
                        GUI.contentColor = cacheColor;
                        GUILayout.Label("██", GUILayout.Width(18));
                        options = new GUILayoutOption[]
                        {
                            GUILayout.Width(148f),
                            GUILayout.MaxWidth(148f)
                        };
                        GUI.contentColor = backColor;
                    }
                }

                if(showTexture.Value)
                {
                    if(startShowTexture)
                    {
                        startShowTexture = false;
                        GUILayout.Label(cacheTexture, GUILayout.Width(48), GUILayout.Height(48));
                        options = new GUILayoutOption[]
                        {
                            GUILayout.Width(118f),
                            GUILayout.MaxWidth(118f)
                        };
                    }
                }
                return true;
            }
        }
        #endregion
    }
}
