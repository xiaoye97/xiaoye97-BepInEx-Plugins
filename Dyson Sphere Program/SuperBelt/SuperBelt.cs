using System;
using BepInEx;
using xiaoye97;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace SuperBelt
{
    [BepInDependency("me.xiaoye97.plugin.Dyson.LDBTool", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("me.xiaoye97.plugin.Dyson.SuperBelt", "SuperBelt", "1.2")]
    public class SuperBelt : BaseUnityPlugin
    {
        Sprite belt4Icon, belt5Icon;
        Color belt4Color = new Color(129 / 255f, 103 / 255f, 246 / 255f);
        Color belt5Color = new Color(1, 65 / 255f, 63 / 255f);

        void Start()
        {
            LDBTool.PreAddDataAction += AddTranslate;
            LDBTool.PostAddDataAction += AddBeltData;
            var ab = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("SuperBelt.belt"));
            belt4Icon = ab.LoadAsset<Sprite>("belt-4");
            belt5Icon = ab.LoadAsset<Sprite>("belt-5");
            Harmony.CreateAndPatchAll(typeof(SuperBelt));
        }

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
            descString.ZHCN = "比急速传送带更强力的设备，有效升级你的工厂！";
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
            descString2.ZHCN = "终极传送带！";
            descString2.ENUS = "It can transport items at 120/s!";
            descString2.FRFR = "It can transport items at 120/s!";
            LDBTool.PreAddProto(ProtoType.String, nameString);
            LDBTool.PreAddProto(ProtoType.String, descString);
            LDBTool.PreAddProto(ProtoType.String, nameString2);
            LDBTool.PreAddProto(ProtoType.String, descString2);
        }

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
            belt4.BuildIndex = 351; // 不要和现有序号重复
            belt4.GridIndex = belt4r.GridIndex;
            belt4.handcraft = belt4r;
            belt4.maincraft = belt4r;
            belt4.handcrafts = new List<RecipeProto>() { belt4r };
            belt4.recipes = new List<RecipeProto>() { belt4r }; // 设置有哪些配方可以合成此物品(用于UI显示)
            belt4.prefabDesc = belt3.prefabDesc.Copy();
            belt4.prefabDesc.modelIndex = belt4.ModelIndex;
            belt4.prefabDesc.beltSpeed = 10;
            belt4.prefabDesc.beltPrototype = 2004;

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
            belt5.BuildIndex = 352; // 不要和现有序号重复
            belt5.GridIndex = belt5r.GridIndex;
            belt5.handcraft = belt5r;
            belt5.maincraft = belt5r;
            belt5.handcrafts = new List<RecipeProto>() { belt5r };
            belt5.recipes = new List<RecipeProto>() { belt5r }; // 设置有哪些配方可以合成此物品(用于UI显示)
            belt5.prefabDesc = belt3.prefabDesc.Copy();
            belt5.prefabDesc.modelIndex = belt5.ModelIndex;
            belt5.prefabDesc.beltSpeed = 20;
            belt5.prefabDesc.beltPrototype = 2005;

            LDBTool.PostAddProto(ProtoType.Recipe, belt4r);
            LDBTool.PostAddProto(ProtoType.Item, belt4);
            LDBTool.PostAddProto(ProtoType.Recipe, belt5r);
            LDBTool.PostAddProto(ProtoType.Item, belt5);
            AddMatAndMesh();
        }

        #region Belt Color
        public void AddMatAndMesh()
        {
            Debug.Log("[SuperBelt]Add mat and mesh...");
            Configs inst = Traverse.Create(typeof(Configs)).Field("instance").GetValue<Configs>();
            var builtin = inst.m_builtin;
            var oriMat = Instantiate(builtin.beltMat[8]);
            List<Material> mats = new List<Material>(builtin.beltMat);
            oriMat.color = belt4Color;
            for (int i = 0; i < 4; i++)
            {
                mats.Add(Instantiate(oriMat));
            }
            oriMat.color = belt5Color;
            for (int i = 0; i < 4; i++)
            {
                mats.Add(Instantiate(oriMat));
            }
            builtin.beltMat = mats.ToArray();
            List<Mesh> meshs = new List<Mesh>(builtin.beltMesh);
            var oriMesh = Instantiate(builtin.beltMesh[0]);
            for (int i = 0; i < 8; i++)
            {
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
            _this.beltPool[beltId].modelIndex = Traverse.Create(_this).Field("beltRenderingBatch").GetValue<BeltRenderingBatch[]>()[num4].AddNode(tmpBeltAnchors);
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
    }
}
