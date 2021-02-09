using System;
using BepInEx;
using xiaoye97;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using System.Collections.Generic;

namespace SuperBelt
{
    [BepInDependency("me.xiaoye97.plugin.Dyson.LDBTool", "1.7.0")]
    [BepInPlugin("me.xiaoye97.plugin.Dyson.SuperBelt", "SuperBelt", "1.3.0")]
    public class SuperBelt : BaseUnityPlugin
    {
        Sprite belt4Icon, belt5Icon;
        Color belt4Color = new Color(129 / 255f, 103 / 255f, 246 / 255f);
        Color belt5Color = new Color(1, 65 / 255f, 63 / 255f);
        public static ConfigEntry<bool> CanUpgrade3to4, CanUpgrade4to5;
        void Start()
        {
            CanUpgrade3to4 = Config.Bind<bool>("config", "CanUpgrade3to4", true);
            CanUpgrade4to5 = Config.Bind<bool>("config", "CanUpgrade4to5", true);
            LDBTool.PreAddDataAction += AddTranslate;
            LDBTool.PostAddDataAction += AddBeltData;
            LDBTool.EditDataAction += Edit;
            SetBuildBar();
            var ab = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("SuperBelt.belt"));
            belt4Icon = ab.LoadAsset<Sprite>("belt-4");
            belt5Icon = ab.LoadAsset<Sprite>("belt-5");
            Harmony.CreateAndPatchAll(typeof(SuperBelt));
        }

        /// <summary>
        /// 修改传送带升级
        /// </summary>
        void Edit(Proto proto)
        {
            if (proto is ItemProto)
            {
                var item = proto as ItemProto;
                if (item.prefabDesc.isBelt)
                {
                    if (CanUpgrade3to4.Value && CanUpgrade3to4.Value)
                    {
                        item.Upgrades = new int[] { 2001, 2002, 2003, 2004, 2005 };
                    }
                    else if (CanUpgrade3to4.Value)
                    {
                        item.Upgrades = new int[] { 2001, 2002, 2003, 2004 };
                    }
                    else if (CanUpgrade4to5.Value && item.ID == 2004)
                    {
                        item.Upgrades = new int[] { 2001, 2002, 2003, 2004, 2005 };
                    }
                }
            }
        }

        /// <summary>
        /// 添加翻译
        /// </summary>
        void AddTranslate()
        {
            // 添加翻译
            StringProto nameString = new StringProto();
            nameString.ID = 10001;
            nameString.Name = "超级传送带MKI";
            nameString.name = "超级传送带MKI";
            nameString.ZHCN = "超级传送带 MK.I";
            nameString.ENUS = "Super Belt MK.I";
            nameString.FRFR = "Super Belt MK.I";
            StringProto descString = new StringProto();
            descString.ID = 10002;
            descString.Name = "超级传送带MKI描述";
            descString.name = "超级传送带MKI描述";
            descString.ZHCN = "比极速传送带更强力！";
            descString.ENUS = "It can transport items at 60/s!";
            descString.FRFR = "It can transport items at 60/s!";
            StringProto nameString2 = new StringProto();
            nameString2.ID = 10003;
            nameString2.Name = "超级传送带MKII";
            nameString2.name = "超级传送带MKII";
            nameString2.ZHCN = "超级传送带 MK.II";
            nameString2.ENUS = "Super Belt MK.II";
            nameString2.FRFR = "Super Belt MK.II";
            StringProto descString2 = new StringProto();
            descString2.ID = 10004;
            descString2.Name = "超级传送带MKII描述";
            descString2.name = "超级传送带MKII描述";
            descString2.ZHCN = "要不要来跑一圈赤道?";
            descString2.ENUS = "It can transport items at 120/s!";
            descString2.FRFR = "It can transport items at 120/s!";
            LDBTool.PreAddProto(ProtoType.String, nameString);
            LDBTool.PreAddProto(ProtoType.String, descString);
            LDBTool.PreAddProto(ProtoType.String, nameString2);
            LDBTool.PreAddProto(ProtoType.String, descString2);
        }

        /// <summary>
        /// 添加传送带数据
        /// </summary>
        void AddBeltData()
        {
            // 因为部分数据可以复用，所以直接从现有库中读取
            var belt3 = LDB.items.Select(2003);
            var belt3r = LDB.recipes.Select(92);
            var preTech = LDB.techs.Select(1605);

            ItemProto belt4 = belt3.Copy();
            RecipeProto belt4r = belt3r.Copy();
            ItemProto belt5 = belt3.Copy();
            RecipeProto belt5r = belt3r.Copy();
            // MK.I
            Traverse.Create(belt4).Field("_iconSprite").SetValue(belt4Icon);
            Traverse.Create(belt4r).Field("_iconSprite").SetValue(belt4Icon);
            belt4r.ID = 201;
            belt4r.Name = "超级传送带MKI";
            belt4r.name = "超级传送带MKI".Translate();
            belt4r.Description = "超级传送带MKI描述";
            belt4r.description = "超级传送带MKI描述".Translate();
            belt4r.Items = new int[] { 2003, 1205, 1406 }; // 合成材料
            belt4r.Results = new int[] { 2004 }; // 合成结果
            belt4r.GridIndex = 2501; // 在合成表中的位置，第2页，第5排，第1个
            belt4r.preTech = preTech;
            belt4r.SID = belt4r.GridIndex.ToString();
            belt4r.sid = belt4r.GridIndex.ToString();
            belt4.Name = "超级传送带MKI";
            belt4.name = "超级传送带MKI".Translate();
            belt4.Description = "超级传送带MKI描述";
            belt4.description = "超级传送带MKI描述".Translate();
            belt4.ID = 2004;
            belt4.makes = new List<RecipeProto>();
            belt4.BuildIndex = 304; // 不要和现有序号重复
            belt4.GridIndex = belt4r.GridIndex;
            belt4.handcraft = belt4r;
            belt4.maincraft = belt4r;
            belt4.handcrafts = new List<RecipeProto>() { belt4r };
            belt4.recipes = new List<RecipeProto>() { belt4r }; // 设置有哪些配方可以合成此物品(用于UI显示)
            belt4.prefabDesc = belt3.prefabDesc.Copy();
            belt4.prefabDesc.modelIndex = belt4.ModelIndex;
            belt4.prefabDesc.beltSpeed = 10;
            belt4.prefabDesc.beltPrototype = 2004;
            belt4.prefabDesc.isBelt = true;
            belt4.Grade = 4;

            // MK.II
            Traverse.Create(belt5).Field("_iconSprite").SetValue(belt5Icon);
            Traverse.Create(belt5r).Field("_iconSprite").SetValue(belt5Icon);
            belt5r.ID = 202;
            belt5r.Name = "超级传送带MKII";
            belt5r.name = "超级传送带MKII".Translate();
            belt5r.Description = "超级传送带MKII描述";
            belt5r.description = "超级传送带MKII描述".Translate();
            belt5r.Items = new int[] { 2004, 1205, 1210 }; // 合成材料
            belt5r.Results = new int[] { 2005 }; // 合成结果
            belt5r.GridIndex = 2502; // 在合成表中的位置，第2页，第5排，第2个
            belt5r.preTech = preTech;
            belt5r.SID = belt5r.GridIndex.ToString();
            belt5r.sid = belt5r.GridIndex.ToString();
            belt5.Name = "超级传送带MKII";
            belt5.name = "超级传送带MKII".Translate();
            belt5.Description = "超级传送带MKII描述";
            belt5.description = "超级传送带MKII描述".Translate();
            belt5.ID = 2005;
            belt5.makes = new List<RecipeProto>();
            belt5.BuildIndex = 305; // 不要和现有序号重复
            belt5.GridIndex = belt5r.GridIndex;
            belt5.handcraft = belt5r;
            belt5.maincraft = belt5r;
            belt5.handcrafts = new List<RecipeProto>() { belt5r };
            belt5.recipes = new List<RecipeProto>() { belt5r }; // 设置有哪些配方可以合成此物品(用于UI显示)
            belt5.prefabDesc = belt3.prefabDesc.Copy();
            belt5.prefabDesc.modelIndex = belt5.ModelIndex;
            belt5.prefabDesc.beltSpeed = 20;
            belt5.prefabDesc.beltPrototype = 2005;
            belt5.prefabDesc.isBelt = true;
            belt5.Grade = 5;

            LDBTool.PostAddProto(ProtoType.Recipe, belt4r);
            LDBTool.PostAddProto(ProtoType.Item, belt4);
            LDBTool.PostAddProto(ProtoType.Recipe, belt5r);
            LDBTool.PostAddProto(ProtoType.Item, belt5);
            AddMatAndMesh();
        }

        /// <summary>
        /// 设置快捷栏
        /// </summary>
        void SetBuildBar()
        {
            LDBTool.SetBuildBar(3, 4, 2004);
            LDBTool.SetBuildBar(3, 5, 2005);
            LDBTool.SetBuildBar(3, 6, 2011);
            LDBTool.SetBuildBar(3, 7, 2012);
            LDBTool.SetBuildBar(3, 8, 2013);
            LDBTool.SetBuildBar(3, 9, 2020);
        }

        #region Belt Color
        public void AddMatAndMesh()
        {
            Debug.Log("[SuperBelt]Add mat and mesh...");
            Configs inst = Traverse.Create(typeof(Configs)).Field("instance").GetValue<Configs>();
            var builtin = inst.m_builtin;

            List<Material> mats = new List<Material>(builtin.beltMat);
            for (int i = 0; i < 4; i++)
            {
                var oriMat = Instantiate(builtin.beltMat[8 + i]);
                oriMat.color = belt4Color;
                mats.Add(Instantiate(oriMat));
            }
            for (int i = 0; i < 4; i++)
            {
                var oriMat = Instantiate(builtin.beltMat[8 + i]);
                oriMat.color = belt5Color;
                mats.Add(Instantiate(oriMat));
            }
            builtin.beltMat = mats.ToArray();
            List<Mesh> meshs = new List<Mesh>(builtin.beltMesh);

            for (int i = 0; i < 4; i++)
            {
                var oriMesh = Instantiate(builtin.beltMesh[8 + i]);
                meshs.Add(Instantiate(oriMesh));
            }
            for (int i = 0; i < 4; i++)
            {
                var oriMesh = Instantiate(builtin.beltMesh[8 + i]);
                meshs.Add(Instantiate(oriMesh));
            }
            builtin.beltMesh = meshs.ToArray();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(CargoTraffic), "AlterBeltRenderer")]
        public static bool ColorPath1(CargoTraffic __instance, int beltId)
        {
            var _this = __instance;
            if (beltId == 0)
            {
                return false;
            }
            _this.RemoveBeltRenderer(beltId);
            CargoPath cargoPath = _this.GetCargoPath(_this.beltPool[beltId].segPathId);
            if (cargoPath == null)
            {
                return false;
            }
            BeltComponent beltComponent = _this.beltPool[beltId];
            double num = (double)((float)beltComponent.segIndex - 0.5f);
            double num2 = (double)((float)(beltComponent.segIndex + beltComponent.segLength) - 0.5f);
            if (cargoPath.closed)
            {
                if (num < 4.0)
                {
                    num = 4.0;
                }
                if (num2 + 9.0 + 1.0 >= (double)cargoPath.pathLength)
                {
                    num2 = (double)(cargoPath.pathLength - 5);
                }
            }
            else
            {
                if (num < 4.0)
                {
                    num = 4.0;
                }
                if (num2 + 5.0 >= (double)cargoPath.pathLength)
                {
                    num2 = (double)(cargoPath.pathLength - 5 - 1);
                }
            }
            int num3 = 1;
            int num4 = 0;
            if (beltComponent.mainInputId > 0 && beltComponent.outputId > 0)
            {
                Vector3 pos = _this.factory.entityPool[_this.beltPool[beltComponent.mainInputId].entityId].pos;
                Vector3 pos2 = _this.factory.entityPool[beltComponent.entityId].pos;
                Vector3 pos3 = _this.factory.entityPool[_this.beltPool[beltComponent.outputId].entityId].pos;
                float num5 = Vector3.Angle(pos - pos2, pos3 - pos2);
                if (num5 > 165f)
                {
                    num3 = 1;
                    num4 = 0;
                }
                else if (num5 > 135f)
                {
                    num3 = 2;
                    num4 = 1;
                }
                else if (num5 > 100f)
                {
                    num3 = 4;
                    num4 = 2;
                }
                else
                {
                    num3 = 8;
                    num4 = 3;
                }
                if (beltComponent.segIndex + beltComponent.segLength == cargoPath.pathLength && cargoPath.outputPath != null)
                {
                    num3 = 8;
                    num4 = 3;
                }
            }
            if (beltComponent.speed <= 1) { }
            else if (beltComponent.speed <= 2) num4 += 4;
            else if (beltComponent.speed <= 5) num4 += 8;
            else if (beltComponent.speed <= 10) num4 += 12;
            else num4 += 16;
            var tmpBeltAnchors = new BeltAnchor[9];
            for (int i = 0; i <= num3; i++)
            {
                double num6 = num + (num2 - num) / (double)num3 * (double)i;
                int num7 = (int)(Math.Floor(num6) + 1E-06);
                double num8 = num6 - (double)num7;
                int num9 = (num8 >= 1E-05) ? (num7 + 1) : num7;
                tmpBeltAnchors[i].t = (float)num6;
                if (num9 == num7)
                {
                    tmpBeltAnchors[i].pos = cargoPath.pointPos[num7];
                    tmpBeltAnchors[i].rot = cargoPath.pointRot[num7];
                }
                else
                {
                    tmpBeltAnchors[i].pos = Vector3.Lerp(cargoPath.pointPos[num7], cargoPath.pointPos[num9], (float)num8);
                    tmpBeltAnchors[i].rot = Quaternion.Slerp(cargoPath.pointRot[num7], cargoPath.pointRot[num9], (float)num8);
                }
            }
            _this.beltPool[beltId].modelBatchIndex = num4 + 1;
            _this.beltPool[beltId].modelIndex = _this.beltRenderingBatch[num4].AddNode(tmpBeltAnchors);
            return false;
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(CargoTraffic), "CreateRenderingBatches")]
        public static IEnumerable<CodeInstruction> ColorPatch2(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            codes[9].operand = 20;
            codes[39].operand = 20;
            return codes.AsEnumerable();
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(CargoTraffic), "Draw")]
        public static IEnumerable<CodeInstruction> ColorPatch3(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            codes[14].operand = 20;
            return codes.AsEnumerable();
        }
        #endregion

        #region Speed bug fix
        [HarmonyPrefix, HarmonyPatch(typeof(CargoPath), "Update")]
        public static bool CargoPathPatch(CargoPath __instance)
        {
            var _this = __instance;
            if (_this.outputPath != null)
            {
                int Sign = _this.bufferLength - 5 - 1;
                if (_this.buffer[Sign] == 250)
                {
                    int cargoId = (int)(_this.buffer[Sign + 1] - 1 + (_this.buffer[Sign + 2] - 1) * 100) + (int)(_this.buffer[Sign + 3] - 1) * 10000 + (int)(_this.buffer[Sign + 4] - 1) * 1000000;
                    if (_this.closed) // 线路闭合
                    {
                        if (_this.outputPath.TryInsertCargoNoSqueeze(_this.outputIndex, cargoId))
                        {
                            Array.Clear(_this.buffer, Sign - 4, 10);
                            _this.updateLen = _this.bufferLength;
                        }
                    }
                    else if (_this.outputPath.TryInsertCargo(_this.outputIndex, cargoId))
                    {
                        Array.Clear(_this.buffer, Sign - 4, 10);
                        _this.updateLen = _this.bufferLength;
                    }
                }
            }
            else if (_this.bufferLength <= 10) return false;
            if (!_this.closed)
            {
                int Rear = _this.bufferLength - 1;
                if (_this.buffer[Rear] != 255 && _this.buffer[Rear] != 0)
                {
                    Debug.Log($"传送带末尾异常! {_this.id} {Rear}");
                    // 清空异常数据
                    for (int i = Rear; i >= 0; i--)
                    {
                        if (_this.buffer[i] == 246)
                        {
                            _this.buffer[i] = 0;
                            break;
                        }
                        _this.buffer[i] = 0;
                    }
                    _this.updateLen = _this.bufferLength;
                }
            }
            for (int j = _this.updateLen - 1; j >= 0; j--)
            {
                if (_this.buffer[j] == 0) break;
                _this.updateLen--;
            }
            if (_this.updateLen == 0) return false;
            int len = _this.updateLen;
            for (int k = _this.chunkCount - 1; k >= 0; k--)
            {
                int begin = _this.chunks[k * 3];
                int speed = _this.chunks[k * 3 + 2];
                if (begin < len)
                {
                    if (_this.buffer[begin] != 0)
                    {
                        for (int l = begin - 5; l < begin + 4; l++)
                        {
                            if (l >= 0)
                            {
                                if (_this.buffer[l] == 250)
                                {
                                    if (l < begin) begin = l + 5 + 1;
                                    else begin = l - 4;
                                    break;
                                }
                            }
                        }
                    }
                    if (speed > 10) // 如果速度大于10，则进行长度判断处理,防止越界
                    {
                        for (int i = 10; i <= speed; i++)
                        {
                            if (begin + i + 10 >= _this.bufferLength) // 即将离开传送带尽头
                            {
                                speed = i;
                                break;
                            }
                            else
                            {
                                if (_this.buffer[begin + i] != 0) // 速度范围内不为空
                                {
                                    speed = i;
                                    break;
                                }
                            }
                        }
                        if (speed < 10)
                        {
                            speed = 10; // 如果速度减速到安全速度以内，设定为安全速度
                        }
                    }
                    int m = 0;
                    while (m < speed)
                    {
                        int num8 = len - begin;
                        if (num8 < 10) // 移动结束
                        {
                            break;
                        }
                        int num9 = 0;
                        for (int n = 0; n < speed - m; n++)
                        {
                            if (_this.buffer[len - 1 - n] != 0) break;
                            num9++;
                        }
                        if (num9 > 0)
                        {
                            Array.Copy(_this.buffer, begin, _this.buffer, begin + num9, num8 - num9);
                            Array.Clear(_this.buffer, begin, num9);
                            m += num9;
                        }
                        for (int num11 = len - 1; num11 >= 0; num11--)
                        {
                            if (_this.buffer[num11] == 0) break;
                            len--;
                        }
                    }
                    int num12 = begin + ((m != 0) ? m : 1);
                    if (len > num12)
                    {
                        len = num12;
                    }
                }
            }
            return false;
        }
        #endregion

        #region Belt determine more fix
        public static bool StartDetermine;
        [HarmonyPrefix, HarmonyPatch(typeof(PlayerAction_Build), "DetermineMoreChainTargets")]
        public static bool BatchPre()
        {
            StartDetermine = true;
            return true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerAction_Build), "DetermineMoreChainTargets")]
        public static void BatchPost(PlayerAction_Build __instance)
        {
            StartDetermine = false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PlayerAction_Build), "GetPrefabDesc")]
        public static bool GetPrefabDescPatch(PlayerAction_Build __instance, int objId, ref PrefabDesc __result)
        {
            if (StartDetermine)
            {
                var _this = __instance;
                var item = Traverse.Create(_this).Method("GetItemProto", objId).GetValue<ItemProto>();
                if (item == null) __result = null;
                else __result = item.prefabDesc;
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion
    }
}
