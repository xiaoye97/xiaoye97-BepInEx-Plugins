using System;
using BepInEx;
using HarmonyLib;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace xiaoye97
{
    [BepInPlugin("me.xiaoye97.plugin.Dyson.LDBTool", "LDBTool", "1.0")]
    public class LDBTool : BaseUnityPlugin
    {
        private static Dictionary<ProtoType, List<Proto>> ReadyToAdd = new Dictionary<ProtoType, List<Proto>>();
        public static Action AddDataAction;

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
            Harmony.CreateAndPatchAll(typeof(LDBTool));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(VFPreload), "InvokeOnLoadWorkEnded")]
        public static void VFPreloadPatch()
        {
            Debug.Log("[LDBTool]Loading...");
            if (AddDataAction != null)
            {
                AddDataAction();
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