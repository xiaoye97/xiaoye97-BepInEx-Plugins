using System;
using BepInEx;
using HarmonyLib;
using System.Linq;
using UnityEngine;
using BepInEx.Configuration;
using System.Collections.Generic;

namespace xiaoye97
{
    [BepInPlugin("me.xiaoye97.plugin.Dyson.LDBTool", "LDBTool", "1.1")]
    public class LDBTool : BaseUnityPlugin
    {
        private static Dictionary<ProtoType, List<Proto>> ReadyToAdd = new Dictionary<ProtoType, List<Proto>>();
        public static Action AddDataAction;
        public static Action<Proto> EditDataAction;
        public static ConfigEntry<bool> ShowProto;
        public static ConfigEntry<KeyCode> ShowProtoHotKey;

        /// <summary>
        /// 添加数据
        /// </summary>
        public static void AddProto(ProtoType protoType, Proto proto)
        {
            if (!ReadyToAdd[protoType].Contains(proto))
            {
                ReadyToAdd[protoType].Add(proto);
            }
        }

        void Awake()
        {
            for (int i = 0; i <= (int)ProtoType.Vein; i++)
            {
                ReadyToAdd.Add((ProtoType)i, new List<Proto>());
            }
        }

        void Start()
        {
            ShowProto = Config.Bind<bool>("config", "ShowProto", false, "是否开启数据显示");
            ShowProtoHotKey = Config.Bind<KeyCode>("config", "ShowProtoHotKey", KeyCode.F5, "呼出界面的快捷键");
            Harmony.CreateAndPatchAll(typeof(LDBTool));
        }

        void Update()
        {
            if (ShowProto.Value)
            {
                if (Input.GetKeyDown(ShowProtoHotKey.Value))
                {
                    ProtoDataUI.Show = !ProtoDataUI.Show;
                }
            }
        }

        void OnGUI()
        {
            if (ShowProto.Value && ProtoDataUI.Show)
            {
                ProtoDataUI.OnGUI();
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(VFPreload), "InvokeOnLoadWorkEnded")]
        public static void VFPreloadPatch()
        {
            Debug.Log("[LDBTool]Loading...");
            if (AddDataAction != null)
            {
                AddDataAction();
                AddDataAction = null;
            }
            foreach (var kv in ReadyToAdd)
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
        }

        /// <summary>
        /// 修复新物品不显示在合成菜单的问题
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPostfix, HarmonyPatch(typeof(GameHistoryData), "Import")]
        public static void HistoryPatch(GameHistoryData __instance)
        {
            foreach (var proto in ReadyToAdd[ProtoType.Recipe])
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
        public static void AddProtosToSet<T>(ProtoSet<T> protoSet, List<Proto> protos) where T : Proto
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
                if (protos[i] is RecipeProto)
                {
                    RecipeProto proto = protos[i] as RecipeProto;
                    if (proto.preTech != null)
                    {
                        ArrayAddItem<int>(ref proto.preTech.UnlockRecipes, proto.ID);
                        ArrayAddItem<RecipeProto>(ref proto.preTech.unlockRecipeArray, proto);
                    }
                }
                Debug.Log($"[LDBTool]Add {protos[i].ID} {protos[i].Name} to {protoSet.GetType().Name}.");
            }
            var dataIndices = new Dictionary<int, int>();
            for (int i = 0; i < protoSet.dataArray.Length; i++)
            {
                protoSet.dataArray[i].name = protoSet.dataArray[i].Name;
                protoSet.dataArray[i].sid = protoSet.dataArray[i].SID;
                dataIndices[protoSet.dataArray[i].ID] = i;
            }
            Traverse.Create(protoSet).Field("dataIndices").SetValue(dataIndices);
        }

        /// <summary>
        /// 数组添加数据
        /// </summary>
        public static void ArrayAddItem<T>(ref T[] array, T item)
        {
            var list = array.ToList();
            list.Add(item);
            array = list.ToArray();
        }
    }

    public enum ProtoType
    {
        AdvisorTip,
        Audio,
        EffectEmitter,
        Item,
        Model,
        Player,
        Recipe,
        String,
        Tech,
        Theme,
        Tutorial,
        Vege,
        Vein
    }
}