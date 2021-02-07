using System;
using BepInEx;
using HarmonyLib;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using BepInEx.Configuration;
using System.Collections.Generic;

namespace xiaoye97
{
    [BepInPlugin("me.xiaoye97.plugin.Dyson.LDBTool", "LDBTool", "1.5")]
    public class LDBToolPlugin : BaseUnityPlugin
    {
        void Awake()
        {
            for (int i = 0; i <= (int)ProtoType.Vein; i++)
            {
                LDBTool.PreToAdd.Add((ProtoType)i, new List<Proto>());
                LDBTool.PostToAdd.Add((ProtoType)i, new List<Proto>());
                LDBTool.TotalDict.Add((ProtoType)i, new List<Proto>());
            }
        }

        void Start()
        {
            LDBTool.ShowProto = Config.Bind<bool>("config", "ShowProto", false, "是否开启数据显示");
            LDBTool.ShowProtoHotKey = Config.Bind<KeyCode>("config", "ShowProtoHotKey", KeyCode.F5, "呼出界面的快捷键");
            LDBTool.ShowItemProtoHotKey = Config.Bind<KeyCode>("config", "ShowItemProtoHotKey", KeyCode.I, "显示物品的Proto");
            LDBTool.ShowRecipeProtoHotKey = Config.Bind<KeyCode>("config", "ShowRecipeProtoHotKey", KeyCode.R, "显示配方的Proto");
            Harmony.CreateAndPatchAll(typeof(LDBTool));
        }

        void Update()
        {
            if (LDBTool.ShowProto.Value)
            {
                if (Input.GetKeyDown(LDBTool.ShowProtoHotKey.Value))
                {
                    ProtoDataUI.Show = !ProtoDataUI.Show;
                }
                if(SupportsHelper.SupportsRuntimeUnityEditor)
                {
                    if (Input.GetKeyDown(LDBTool.ShowItemProtoHotKey.Value))
                    {
                        LDBTool.TryShowItemProto();
                    }
                    if (Input.GetKeyDown(LDBTool.ShowRecipeProtoHotKey.Value))
                    {
                        LDBTool.TryShowRecipeProto();
                    }
                }
            }
        }

        void OnGUI()
        {
            if (LDBTool.ShowProto.Value && ProtoDataUI.Show)
            {
                ProtoDataUI.OnGUI();
            }
        }
    }

    public static class LDBTool
    {
        // 添加数据的Action
        public static Action PreAddDataAction, PostAddDataAction;
        // 修改数据的Action
        public static Action<Proto> EditDataAction;

        internal static Dictionary<ProtoType, List<Proto>> PreToAdd = new Dictionary<ProtoType, List<Proto>>();
        internal static Dictionary<ProtoType, List<Proto>> PostToAdd = new Dictionary<ProtoType, List<Proto>>();
        internal static Dictionary<ProtoType, List<Proto>> TotalDict = new Dictionary<ProtoType, List<Proto>>();
        internal static ConfigEntry<bool> ShowProto;
        internal static ConfigEntry<KeyCode> ShowProtoHotKey, ShowItemProtoHotKey, ShowRecipeProtoHotKey;
        private static bool Finshed;
        private static ConfigFile CustomConfig = new ConfigFile($"{Paths.ConfigPath}/LDBTool.CustomID.cfg", true);
        private static Dictionary<ProtoType, Dictionary<string, ConfigEntry<int>>> IDDict = new Dictionary<ProtoType, Dictionary<string, ConfigEntry<int>>>();
        private static UIItemTip lastTip;

        /// <summary>
        /// 通过配置文件绑定ID，允许玩家在冲突时自定义ID
        /// </summary>
        /// <param name="protoType"></param>
        /// <param name="proto"></param>
        private static void IdBind(ProtoType protoType, Proto proto)
        {
            var entry = CustomConfig.Bind<int>(protoType.ToString(), proto.Name, proto.ID);
            proto.ID = entry.Value;
            if (!IDDict.ContainsKey(protoType))
            {
                IDDict.Add(protoType, new Dictionary<string, ConfigEntry<int>>());
            }
            if (IDDict[protoType].ContainsKey(proto.Name))
            {
                Debug.LogError($"[LDBTool] Name {proto.Name} already exists.please check mod.");
                Debug.LogError($"[LDBTool] 姓名 {proto.Name} 已经存在.请检查Mod.");
            }
            else
            {
                IDDict[protoType].Add(proto.Name, entry);
            }
        }

        /// <summary>
        /// 在游戏数据加载之前添加数据
        /// </summary>
        /// <param name="protoType">要添加的Proto的类型</param>
        /// <param name="proto">要添加的Proto</param>
        public static void PreAddProto(ProtoType protoType, Proto proto)
        {
            if (!PreToAdd[protoType].Contains(proto))
            {
                IdBind(protoType, proto);
                PreToAdd[protoType].Add(proto);
                TotalDict[protoType].Add(proto);
            }
        }

        /// <summary>
        /// 在游戏数据加载之后添加数据
        /// </summary>
        /// <param name="protoType">要添加的Proto的类型</param>
        /// <param name="proto">要添加的Proto</param>
        public static void PostAddProto(ProtoType protoType, Proto proto)
        {
            if (!PostToAdd[protoType].Contains(proto))
            {
                IdBind(protoType, proto);
                PostToAdd[protoType].Add(proto);
                TotalDict[protoType].Add(proto);
            }
        }

        private static void AddProtos(Dictionary<ProtoType, List<Proto>> datas)
        {
            foreach (var kv in datas)
            {
                if (kv.Value.Count > 0)
                {
                    if (kv.Key == ProtoType.AdvisorTip) AddProtosToSet(LDB.advisorTips, kv.Value);
                    else if (kv.Key == ProtoType.Audio) AddProtosToSet(LDB.audios, kv.Value);
                    else if (kv.Key == ProtoType.EffectEmitter) AddProtosToSet(LDB.effectEmitters, kv.Value);
                    else if (kv.Key == ProtoType.Item) AddProtosToSet(LDB.items, kv.Value);
                    else if (kv.Key == ProtoType.Model) AddProtosToSet(LDB.models, kv.Value);
                    else if (kv.Key == ProtoType.Player) AddProtosToSet(LDB.players, kv.Value);
                    else if (kv.Key == ProtoType.Recipe) AddProtosToSet(LDB.recipes, kv.Value);
                    else if (kv.Key == ProtoType.String) AddProtosToSet(LDB.strings, kv.Value);
                    else if (kv.Key == ProtoType.Tech) AddProtosToSet(LDB.techs, kv.Value);
                    else if (kv.Key == ProtoType.Theme) AddProtosToSet(LDB.themes, kv.Value);
                    else if (kv.Key == ProtoType.Tutorial) AddProtosToSet(LDB.tutorial, kv.Value);
                    else if (kv.Key == ProtoType.Vege) AddProtosToSet(LDB.veges, kv.Value);
                    else if (kv.Key == ProtoType.Vein) AddProtosToSet(LDB.veins, kv.Value);
                }
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(VFPreload), "InvokeOnLoadWorkEnded")]
        private static void VFPreloadPrePatch()
        {
            if (Finshed) return;
            Debug.Log("[LDBTool]Pre Loading...");
            if (PreAddDataAction != null)
            {
                PreAddDataAction();
                PreAddDataAction = null;
            }
            AddProtos(PreToAdd);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(VFPreload), "InvokeOnLoadWorkEnded")]
        private static void VFPreloadPostPatch()
        {
            if (Finshed) return;
            Debug.Log("[LDBTool]Post Loading...");
            if (PostAddDataAction != null)
            {
                PostAddDataAction();
                PostAddDataAction = null;
            }
            AddProtos(PostToAdd);
            List<Proto> allProto = new List<Proto>();
            foreach (var p in LDB.advisorTips.dataArray) allProto.Add(p);
            foreach (var p in LDB.audios.dataArray) allProto.Add(p);
            foreach (var p in LDB.effectEmitters.dataArray) allProto.Add(p);
            foreach (var p in LDB.items.dataArray) allProto.Add(p);
            foreach (var p in LDB.models.dataArray) allProto.Add(p);
            foreach (var p in LDB.players.dataArray) allProto.Add(p);
            foreach (var p in LDB.recipes.dataArray) allProto.Add(p);
            foreach (var p in LDB.strings.dataArray) allProto.Add(p);
            foreach (var p in LDB.techs.dataArray) allProto.Add(p);
            foreach (var p in LDB.themes.dataArray) allProto.Add(p);
            foreach (var p in LDB.tutorial.dataArray) allProto.Add(p);
            foreach (var p in LDB.veges.dataArray) allProto.Add(p);
            foreach (var p in LDB.veins.dataArray) allProto.Add(p);
            if (EditDataAction != null)
            {
                foreach (var p in allProto)
                {
                    if (p != null)
                    {
                        try
                        {
                            EditDataAction(p);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"[LDBTool]Edit Error: ID:{p.ID} Type:{p.GetType().Name} {e.Message}");
                        }
                    }
                }
            }
            GameMain.iconSet.loaded = false;
            GameMain.iconSet.Create();
            Finshed = true;
            Debug.Log("[LDBTool]Done.");
        }

        /// <summary>
        /// 修复新物品不显示在合成菜单的问题
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPostfix, HarmonyPatch(typeof(GameHistoryData), "Import")]
        private static void HistoryPatch(GameHistoryData __instance)
        {
            foreach (var proto in TotalDict[ProtoType.Recipe])
            {
                var recipe = proto as RecipeProto;
                if (recipe.preTech != null)
                {
                    if (__instance.TechState(recipe.preTech.ID).unlocked)
                    {
                        if (!__instance.RecipeUnlocked(recipe.ID))
                        {
                            __instance.UnlockRecipe(recipe.ID);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 添加多个数据到数据表
        /// </summary>
        private static void AddProtosToSet<T>(ProtoSet<T> protoSet, List<Proto> protos) where T : Proto
        {
            var array = protoSet.dataArray;
            protoSet.Init(array.Length + protos.Count);
            for (int i = 0; i < array.Length; i++)
            {
                protoSet.dataArray[i] = array[i];
            }
            for (int i = 0; i < protos.Count; i++)
            {
                protoSet.dataArray[array.Length + i] = protos[i] as T;

                if (protos[i] is ItemProto)
                {
                    Traverse.Create(protos[i]).Property("index").SetValue(array.Length + i);
                }
                if (protos[i] is RecipeProto)
                {
                    RecipeProto proto = protos[i] as RecipeProto;
                    if (proto.preTech != null)
                    {
                        ArrayAddItem<int>(ref proto.preTech.UnlockRecipes, proto.ID);
                        ArrayAddItem<RecipeProto>(ref proto.preTech.unlockRecipeArray, proto);
                    }
                }
                Debug.Log($"[LDBTool]Add {protos[i].ID} {protos[i].Name.Translate()} to {protoSet.GetType().Name}.");
            }
            var dataIndices = new Dictionary<int, int>();
            for (int i = 0; i < protoSet.dataArray.Length; i++)
            {
                protoSet.dataArray[i].sid = protoSet.dataArray[i].SID;
                dataIndices[protoSet.dataArray[i].ID] = i;
            }
            Traverse.Create(protoSet).Field("dataIndices").SetValue(dataIndices);
            if (protoSet is ProtoSet<StringProto>)
            {
                var nameIndices = Traverse.Create(protoSet).Field("nameIndices").GetValue<Dictionary<string, int>>();
                for (int i = array.Length; i < protoSet.dataArray.Length; i++)
                {
                    nameIndices[protoSet.dataArray[i].Name] = i;
                }
                Traverse.Create(protoSet).Field("nameIndices").SetValue(nameIndices);
            }
        }

        /// <summary>
        /// 数组添加数据
        /// </summary>
        private static void ArrayAddItem<T>(ref T[] array, T item)
        {
            var list = array.ToList();
            list.Add(item);
            array = list.ToArray();
        }

        /// <summary>
        /// 在物品提示显示ID
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(UIItemTip), "SetTip")]
        private static void ItemTipPatch(UIItemTip __instance, int itemId)
        {
            if (ShowProto.Value)
            {
                Traverse.Create(__instance).Field("nameText").GetValue<Text>().text += $" {itemId}";
                lastTip = __instance;
            }
        }

        /// <summary>
        /// 尝试显示ItemProto，通过按键触发
        /// </summary>
        internal static void TryShowItemProto()
        {
            if (ShowProto.Value)
            {
                if (lastTip != null && lastTip.showingItemId != 0)
                {
                    var proto = LDB.items.Select(lastTip.showingItemId);
                    if (proto != null)
                    {
                        RUEHelper.ShowProto(proto);
                    }
                    else
                    {
                        var recipe = LDB.recipes.Select(-lastTip.showingItemId);
                        if (recipe != null)
                        {
                            foreach (var id in recipe.Results)
                            {
                                var item = LDB.items.Select(id);
                                RUEHelper.ShowProto(item);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 尝试显示RecipeProto，通过按键触发
        /// </summary>
        internal static void TryShowRecipeProto()
        {
            if (ShowProto.Value)
            {
                if (lastTip != null && lastTip.showingItemId != 0)
                {
                    var itemProto = LDB.items.Select(lastTip.showingItemId);
                    if (itemProto != null)
                    {
                        foreach (var proto in itemProto.recipes)
                        {
                            RUEHelper.ShowProto(proto);
                        }
                    }
                    else
                    {
                        var proto = LDB.recipes.Select(-lastTip.showingItemId);
                        RUEHelper.ShowProto(proto);
                    }
                }
            }
        }
    }
}