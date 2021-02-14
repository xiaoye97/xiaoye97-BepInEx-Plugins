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
    [BepInPlugin("me.xiaoye97.plugin.Dyson.LDBTool", "LDBTool", "1.8.0")]
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
            if (SupportsHelper.SupportsRuntimeUnityEditor)
            {
                ProtoDataUI.Skin = new RUESkin();
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
                if (SupportsHelper.SupportsRuntimeUnityEditor)
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
        private static ConfigFile CustomID = new ConfigFile($"{Paths.ConfigPath}/LDBTool/LDBTool.CustomID.cfg", true);
        private static ConfigFile CustomGridIndex = new ConfigFile($"{Paths.ConfigPath}/LDBTool/LDBTool.CustomGridIndex.cfg", true);
        private static ConfigFile CustomStringZHCN = new ConfigFile($"{Paths.ConfigPath}/LDBTool/LDBTool.CustomLocalization.ZHCN.cfg", true);
        private static ConfigFile CustomStringENUS = new ConfigFile($"{Paths.ConfigPath}/LDBTool/LDBTool.CustomLocalization.ENUS.cfg", true);
        private static ConfigFile CustomStringFRFR = new ConfigFile($"{Paths.ConfigPath}/LDBTool/LDBTool.CustomLocalization.FRFR.cfg", true);

        private static Dictionary<ProtoType, Dictionary<string, ConfigEntry<int>>> IDDict = new Dictionary<ProtoType, Dictionary<string, ConfigEntry<int>>>();
        private static Dictionary<ProtoType, Dictionary<string, ConfigEntry<int>>> GridIndexDict = new Dictionary<ProtoType, Dictionary<string, ConfigEntry<int>>>();
        private static Dictionary<ProtoType, Dictionary<string, ConfigEntry<string>>> ZHCNDict = new Dictionary<ProtoType, Dictionary<string, ConfigEntry<string>>>();
        private static Dictionary<ProtoType, Dictionary<string, ConfigEntry<string>>> ENUSDict = new Dictionary<ProtoType, Dictionary<string, ConfigEntry<string>>>();
        private static Dictionary<ProtoType, Dictionary<string, ConfigEntry<string>>> FRFRDict = new Dictionary<ProtoType, Dictionary<string, ConfigEntry<string>>>();

        private static UIItemTip lastTip;
        private static Dictionary<int, Dictionary<int, int>> BuildBarDict = new Dictionary<int, Dictionary<int, int>>();

        /// <summary>
        /// 设置建造快捷栏
        /// </summary>
        /// <param name="category">第几栏</param>
        /// <param name="index">第几个格子</param>
        /// <param name="itemId">物品ID</param>
        public static void SetBuildBar(int category, int index, int itemId)
        {
            if (category < 1 || category > 12)
            {
                Debug.LogWarning("[LDBTool]SetBuildBar Fail. category must be between 1 and 12.");
                return;
            }
            if (index < 1 || index > 12)
            {
                Debug.LogWarning("[LDBTool]SetBuildBar Fail. index must be between 1 and 12.");
                return;
            }
            if (Traverse.Create(typeof(UIBuildMenu)).Field("staticLoaded").GetValue<bool>() && Finshed) // 如果已经加载
            {
                var item = LDB.items.Select(itemId);
                if (item != null)
                {
                    var protos = Traverse.Create(typeof(UIBuildMenu)).Field("protos").GetValue<ItemProto[,]>();
                    protos[category, index] = item;
                    Debug.Log($"[LDBTool] Set build bar at {category},{index} ID:{item.ID} name:{item.Name.Translate()}");
                    Traverse.Create(typeof(UIBuildMenu)).Field("protos").SetValue(protos);
                }
                else
                {
                    Debug.LogWarning($"[LDBTool]SetBuildBar Fail. ItemProto with ID {itemId} not found.");
                    return;
                }
            }
            else
            {
                if (!BuildBarDict.ContainsKey(category))
                {
                    BuildBarDict.Add(category, new Dictionary<int, int>());
                }
                BuildBarDict[category][index] = itemId;
            }
        }

        /// <summary>
        /// 自动设置建造快捷栏
        /// </summary>
        private static void SetBuildBar()
        {
            var protos = Traverse.Create(typeof(UIBuildMenu)).Field("protos").GetValue<ItemProto[,]>();
            foreach (var kv in BuildBarDict)
            {
                foreach (var kv2 in kv.Value)
                {
                    var item = LDB.items.Select(kv2.Value);
                    if (item != null)
                    {
                        protos[kv.Key, kv2.Key] = item;
                        Debug.Log($"[LDBTool] Set build bar at {kv.Key},{kv2.Key} ID:{item.ID} name:{item.Name.Translate()}");
                    }
                    else
                    {
                        Debug.LogWarning($"[LDBTool]SetBuildBar Fail. ItemProto with ID {kv2.Value} not found.");
                    }
                }
            }
            Traverse.Create(typeof(UIBuildMenu)).Field("protos").SetValue(protos);
        }

        /// <summary>
        /// 用户配置数据绑定
        /// </summary>
        private static void Bind(ProtoType protoType, Proto proto)
        {
            IdBind(protoType, proto);
            GridIndexBind(protoType, proto);
            StringBind(protoType, proto);
        }

        /// <summary>
        /// 通过配置文件绑定ID，允许玩家在冲突时自定义ID
        /// </summary>
        private static void IdBind(ProtoType protoType, Proto proto)
        {
            var entry = CustomID.Bind<int>(protoType.ToString(), proto.Name, proto.ID);
            proto.ID = entry.Value;
            if (!IDDict.ContainsKey(protoType))
            {
                IDDict.Add(protoType, new Dictionary<string, ConfigEntry<int>>());
            }
            if (IDDict[protoType].ContainsKey(proto.Name))
            {
                Debug.LogError($"[LDBTool.CustomID]ID:{proto.ID} Name:{proto.Name} There is a conflict, please check.");
                Debug.LogError($"[LDBTool.CustomID]ID:{proto.ID} 姓名:{proto.Name} 存在冲突，请检查。");
            }
            else
            {
                IDDict[protoType].Add(proto.Name, entry);
            }
        }

        /// <summary>
        /// 通过配置文件绑定GridIndex，允许玩家在冲突时自定义GridIndex
        /// 在自定义ID之后执行
        /// </summary>
        private static void GridIndexBind(ProtoType protoType, Proto proto)
        {
            if (proto is ItemProto || proto is RecipeProto) // 只有物品和配方有GridIndex
            {
                ConfigEntry<int> entry = null;
                if (proto is ItemProto)
                {
                    var item = proto as ItemProto;
                    entry = CustomGridIndex.Bind<int>(protoType.ToString(), item.ID.ToString(), item.GridIndex, $"Item Name = {item.Name}");
                    item.GridIndex = entry.Value;
                }
                else if (proto is RecipeProto)
                {
                    var recipe = proto as RecipeProto;
                    entry = CustomGridIndex.Bind<int>(protoType.ToString(), recipe.ID.ToString(), recipe.GridIndex, $"Recipe Name = {recipe.Name}");
                    recipe.GridIndex = entry.Value;
                }
                if (entry != null)
                {
                    if (!GridIndexDict.ContainsKey(protoType))
                    {
                        GridIndexDict.Add(protoType, new Dictionary<string, ConfigEntry<int>>());
                    }
                    if (GridIndexDict[protoType].ContainsKey(proto.Name))
                    {
                        Debug.LogError($"[LDBTool.CustomGridIndex]ID:{proto.ID} Name:{proto.Name} There is a conflict, please check.");
                        Debug.LogError($"[LDBTool.CustomGridIndex]ID:{proto.ID} 姓名:{proto.Name} 存在冲突，请检查。");
                    }
                    else
                    {
                        GridIndexDict[protoType].Add(proto.Name, entry);
                    }
                }
            }
        }

        /// <summary>
        /// 通过配置文件绑定翻译文件，允许玩家在翻译缺失或翻译不准确时自定义翻译
        /// </summary>
        private static void StringBind(ProtoType protoType, Proto proto)
        {
            if (proto is StringProto)
            {
                var lang = proto as StringProto;
                ConfigEntry<string> zhcn, enus, frfr;
                zhcn = CustomStringZHCN.Bind<string>(protoType.ToString(), lang.ID.ToString(), lang.ZHCN, lang.Name);
                enus = CustomStringENUS.Bind<string>(protoType.ToString(), lang.ID.ToString(), lang.ENUS, lang.Name);
                frfr = CustomStringFRFR.Bind<string>(protoType.ToString(), lang.ID.ToString(), lang.FRFR, lang.Name);
                lang.ZHCN = zhcn.Value;
                lang.ENUS = enus.Value;
                lang.FRFR = frfr.Value;
                if (zhcn != null)
                {
                    if (!ZHCNDict.ContainsKey(protoType)) ZHCNDict.Add(protoType, new Dictionary<string, ConfigEntry<string>>());
                    if (ZHCNDict[protoType].ContainsKey(proto.Name))
                    {
                        Debug.LogError($"[LDBTool.CustomLocalization.ZHCN]ID:{proto.ID} Name:{proto.Name} There is a conflict, please check.");
                        Debug.LogError($"[LDBTool.CustomLocalization.ZHCN]ID:{proto.ID} 姓名:{proto.Name} 存在冲突，请检查。");
                    }
                    else ZHCNDict[protoType].Add(proto.Name, zhcn);
                }
                if (ENUSDict != null)
                {
                    if (!ENUSDict.ContainsKey(protoType)) ENUSDict.Add(protoType, new Dictionary<string, ConfigEntry<string>>());
                    if (ENUSDict[protoType].ContainsKey(proto.Name))
                    {
                        Debug.LogError($"[LDBTool.CustomLocalization.ENUS]ID:{proto.ID} Name:{proto.Name} There is a conflict, please check.");
                        Debug.LogError($"[LDBTool.CustomLocalization.ENUS]ID:{proto.ID} 姓名:{proto.Name} 存在冲突，请检查。");
                    }
                    else ENUSDict[protoType].Add(proto.Name, enus);
                }
                if (frfr != null)
                {
                    if (!FRFRDict.ContainsKey(protoType)) FRFRDict.Add(protoType, new Dictionary<string, ConfigEntry<string>>());
                    if (FRFRDict[protoType].ContainsKey(proto.Name))
                    {
                        Debug.LogError($"[LDBTool.CustomLocalization.FRFR]ID:{proto.ID} Name:{proto.Name} There is a conflict, please check.");
                        Debug.LogError($"[LDBTool.CustomLocalization.FRFR]ID:{proto.ID} 姓名:{proto.Name} 存在冲突，请检查。");
                    }
                    else FRFRDict[protoType].Add(proto.Name, frfr);
                }
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
                Bind(protoType, proto);
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
                Bind(protoType, proto);
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
            SetBuildBar();
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