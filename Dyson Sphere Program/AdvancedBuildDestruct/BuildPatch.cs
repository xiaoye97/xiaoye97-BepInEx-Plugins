using System;
using HarmonyLib;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace AdvancedBuildDestruct
{
    public class BuildPatch
    {
        [HarmonyPrefix, HarmonyPatch(typeof(PlayerAction_Build), "DetermineBuildPreviews")]
        public static bool DetermineBuildPreviewsPrePatch(PlayerAction_Build __instance)
        {
            var _this = __instance;
            CommandState cmd = _this.controller.cmd;
            if (_this.player.planetData.type != EPlanetType.Gas)
            {
                if (_this.handPrefabDesc != null)
                {
                    if (cmd.mode == 1)
                    {
                        if (_this.cursorValid)
                        {
                            if (_this.handPrefabDesc.minerType == EMinerType.None)
                            {
                                if (!_this.handPrefabDesc.multiLevel)
                                {
                                    if (!_this.multiLevelCovering)
                                    {
                                        DetermineBuildPreviews(_this);
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            begin = false;
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PlayerAction_Build), "UpdatePreviews")]
        public static bool UpdatePreviewsPrePatch(PlayerAction_Build __instance)
        {
            var _this = __instance;
            if (begin)
            {
                UpdatePreviews(_this);
                return false;
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PlayerAction_Build), "CreatePrebuilds")]
        public static bool CreatePrebuildsPrePatch(PlayerAction_Build __instance)
        {
            var _this = __instance;
            if (begin)
            {
                CreatePrebuilds(_this);
                return false;
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PlayerAction_Build), "CheckBuildConditions")]
        public static bool CheckBuildConditionsPrePatch(PlayerAction_Build __instance)
        {
            var _this = __instance;
            if (begin)
            {
                _this.previewPose.position = Vector3.zero;
                _this.previewPose.rotation = Quaternion.identity;
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PlayerAction_Build), "CheckBuildConditions")]
        public static void CheckBuildConditionsPostPatch(PlayerAction_Build __instance, ref bool __result)
        {
            var _this = __instance;
            if (begin)
            {
                bool flag = true;
                foreach (var pre in _this.buildPreviews)
                {
                    if (pre.condition != EBuildCondition.Ok)
                    {
                        flag = false;
                    }
                }
                __result = flag;
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerAction_Build), "OnExitBuildMode")]
        public static void OnExitBuildModePostPatch()
        {
            if (begin)
            {
                begin = false;
            }
        }

        public static bool begin;
        static Vector3 beginPos;
        static Vector3[] snaps = new Vector3[256];

        public static void DetermineBuildPreviews(PlayerAction_Build _this)
        {
            if (AdvancedBuildDestruct.buildKeyUp)
            {
                AdvancedBuildDestruct.buildKeyUp = false;
                beginPos = _this.groundSnappedPos;
                begin = !begin;
            }
            _this.waitConfirm = _this.cursorValid;
            int path = 0;
            int count = 0;
            if (begin)
            {
                count = _this.planetAux.SnapLineNonAlloc(beginPos, _this.groundSnappedPos, ref path, snaps);
            }
            if (VFInput._tabKey.onDown)
            {
                _this.modelOffset++;
            }
            if (VFInput._rotate.onDown)
            {
                _this.yaw += 90f;
                _this.yaw = Mathf.Repeat(_this.yaw, 360f);
                _this.yaw = Mathf.Round(_this.yaw / 90f) * 90f;
            }
            if (VFInput._counterRotate.onDown)
            {
                _this.yaw -= 90f;
                _this.yaw = Mathf.Repeat(_this.yaw, 360f);
                _this.yaw = Mathf.Round(_this.yaw / 90f) * 90f;
            }
            if (_this.handPrefabDesc.minerType != EMinerType.Vein)
            {
                _this.yaw = Mathf.Round(_this.yaw / 90f) * 90f;
            }
            _this.multiLevelCovering = false;
            _this.previewPose.position = _this.groundSnappedPos;
            _this.previewPose.rotation = Maths.SphericalRotation(_this.previewPose.position, _this.yaw);

            _this.ClearBuildPreviews();
            if (begin)
            {
                List<Vector3> posList = new List<Vector3>();
                List<ColliderData> cList = new List<ColliderData>();
                for (int i = 0; i < count; i++)
                {
                    var desc = _this.handPrefabDesc;
                    if (i > 0)
                    {
                        // 电力设备
                        // Debug.Log("判断电力设备");
                        float x = posList[posList.Count - 1].x - snaps[i].x;
                        float y = posList[posList.Count - 1].y - snaps[i].y;
                        float z = posList[posList.Count - 1].z - snaps[i].z;
                        float dis = x * x + y * y + z * z;
                        if (dis < 12.25f) continue;
                        if (desc.isPowerNode && !desc.isAccumulator)
                        {
                            if (desc.windForcedPower && dis < 110.25f) continue; // 风力发电机
                            if (_this.handItem.ModelIndex == 73 && dis < 110.25f) continue; // 射线接收站
                        }

                        // 物流站
                        // Debug.Log("判断物流站");
                        if (desc.isStation)
                        {
                            float dis2 = (desc.isStellarStation) ? 841f : 225f;
                            if ((posList[posList.Count - 1] - snaps[i]).sqrMagnitude < dis2)
                            {
                                continue;
                            }
                        }
                        // 发射器
                        // Debug.Log("判断发射器");
                        if (desc.isEjector)
                        {
                            if (dis < 110.25f) continue;
                        }
                        // 建造碰撞器
                        // Debug.Log("判断碰撞器");
                        if (desc.hasBuildCollider)
                        {
                            ColliderData c = desc.buildCollider;
                            c.pos = snaps[i] + Maths.SphericalRotation(snaps[i], _this.yaw) * c.pos;
                            c.q = Maths.SphericalRotation(snaps[i], _this.yaw) * c.q;
                            if (_this.handItem.BuildMode == 1)
                            {
                                float mul = 1f;
                                if (_this.handItem.ModelIndex == 64) mul = 1.05f;
                                if (_this.handItem.ModelIndex == 54 || _this.handItem.ModelIndex == 118)
                                {
                                    if (!CheckBox(cList[cList.Count - 1], c))
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        if (dis < 16) i++;
                                    }
                                }
                                else if (!CheckBox(cList[cList.Count - 1], c, mul))
                                {
                                    // Debug.Log("因碰撞器跳过");
                                    continue;
                                }
                            }
                        }
                    }
                    i += AdvancedBuildDestruct.BuildExtraSpacing.Value;
                    _this.AddBuildPreview(BuildPreview.CreateSingle(_this.handItem, _this.handPrefabDesc, true));
                    posList.Add(snaps[i]);
                    if (desc.hasBuildCollider)
                    {
                        ColliderData c = desc.buildCollider;
                        c.pos = snaps[i] + Maths.SphericalRotation(snaps[i], _this.yaw) * c.pos;
                        c.q = Maths.SphericalRotation(snaps[i], _this.yaw) * c.q;
                        cList.Add(c);
                    }
                }
                for (int i = 0; i < _this.buildPreviews.Count; i++)
                {
                    BuildPreview buildPreview = _this.buildPreviews[i];
                    buildPreview.ResetInfos();
                    buildPreview.item = _this.handItem;
                    buildPreview.desc = _this.handPrefabDesc;
                    buildPreview.recipeId = _this.copyRecipeId;
                    buildPreview.filterId = _this.copyFilterId;
                    buildPreview.lpos = posList[i];
                    buildPreview.lrot = Maths.SphericalRotation(posList[i], _this.yaw);
                }
            }
            else
            {
                _this.AddBuildPreview(BuildPreview.CreateSingle(_this.handItem, _this.handPrefabDesc, true));
                BuildPreview buildPreview = _this.buildPreviews[0];
                buildPreview.ResetInfos();
                buildPreview.item = _this.handItem;
                buildPreview.desc = _this.handPrefabDesc;
                buildPreview.recipeId = _this.copyRecipeId;
                buildPreview.filterId = _this.copyFilterId;
            }
        }

        public static void UpdatePreviews(PlayerAction_Build _this)
        {
            int num = 0;
            int pointCount = _this.connGraph.pointCount;
            _this.connRenderer.ClearXSigns();
            _this.connRenderer.ClearUpgradeArrows();
            for (int i = 0; i < _this.buildPreviews.Count; i++)
            {
                BuildPreview buildPreview = _this.buildPreviews[i];
                if (buildPreview.needModel)
                {
                    _this.CreatePreviewModel(buildPreview);
                    int previewIndex = buildPreview.previewIndex;
                    if (previewIndex >= 0)
                    {
                        _this.previewRenderers[previewIndex].transform.localPosition = buildPreview.lpos;
                        _this.previewRenderers[previewIndex].transform.localRotation = buildPreview.lrot;
                        bool isInserter = buildPreview.desc.isInserter;
                        Material material;
                        if (isInserter)
                        {
                            Material original;
                            if (_this.upgrading)
                            {
                                original = Configs.builtin.previewUpgradeMat_Inserter;
                            }
                            else if (_this.destructing)
                            {
                                original = Configs.builtin.previewDestructMat_Inserter;
                            }
                            else
                            {
                                original = ((buildPreview.condition != EBuildCondition.Ok) ? Configs.builtin.previewErrorMat_Inserter : Configs.builtin.previewOkMat_Inserter);
                            }
                            material = UnityEngine.Object.Instantiate<Material>(original);
                            bool t;
                            bool t2;
                            _this.GetInserterT1T2(buildPreview.objId, out t, out t2);
                            if (buildPreview.outputObjId != 0 && !_this.ObjectIsBelt(buildPreview.outputObjId) && !_this.ObjectIsInserter(buildPreview.outputObjId))
                            {
                                t2 = true;
                            }
                            if (buildPreview.inputObjId != 0 && !_this.ObjectIsBelt(buildPreview.inputObjId) && !_this.ObjectIsInserter(buildPreview.inputObjId))
                            {
                                t = true;
                            }
                            material.SetVector("_Position1", _this.Vector3BoolToVector4(Vector3.zero, t));
                            material.SetVector("_Position2", _this.Vector3BoolToVector4(Quaternion.Inverse(buildPreview.lrot) * (buildPreview.lpos2 - buildPreview.lpos), t2));
                            material.SetVector("_Rotation1", _this.QuaternionToVector4(Quaternion.identity));
                            material.SetVector("_Rotation2", _this.QuaternionToVector4(Quaternion.Inverse(buildPreview.lrot) * buildPreview.lrot2));
                            _this.previewRenderers[previewIndex].enabled = (buildPreview.condition != EBuildCondition.NeedConn);
                        }
                        else
                        {
                            _this.previewRenderers[previewIndex].enabled = true;
                            Material original2;
                            if (_this.upgrading)
                            {
                                original2 = Configs.builtin.previewUpgradeMat;
                            }
                            else if (_this.destructing)
                            {
                                original2 = Configs.builtin.previewDestructMat;
                            }
                            else
                            {
                                original2 = ((buildPreview.condition != EBuildCondition.Ok) ? Configs.builtin.previewErrorMat : Configs.builtin.previewOkMat);
                            }
                            material = UnityEngine.Object.Instantiate<Material>(original2);
                        }
                        _this.previewRenderers[previewIndex].sharedMaterial = material;
                    }
                }
                else if (buildPreview.previewIndex >= 0)
                {
                    _this.FreePreviewModel(buildPreview);
                }
                if (buildPreview.isConnNode)
                {
                    if (_this.upgrading)
                    {
                        if (_this.upgradeLevel == 1)
                        {
                            _this.connRenderer.AddUpgradeArrow(buildPreview.lpos);
                        }
                        else if (_this.upgradeLevel == -1)
                        {
                            _this.connRenderer.AddDowngradeArrow(buildPreview.lpos);
                        }
                    }
                    else if (_this.destructing)
                    {
                        _this.connRenderer.AddXSign(buildPreview.lpos, buildPreview.lrot);
                    }
                    else
                    {
                        uint num2 = 4U;
                        if (buildPreview.condition != EBuildCondition.Ok)
                        {
                            num2 = 0U;
                        }
                        if (num < pointCount)
                        {
                            _this.connGraph.points[num] = buildPreview.lpos;
                            _this.connGraph.colors[num] = num2;
                        }
                        else
                        {
                            _this.connGraph.AddPoint(buildPreview.lpos, num2);
                        }
                        num++;
                    }
                }
            }
            _this.connGraph.SetPointCount(num);
            if (num > 0)
            {
                _this.showConnGraph = true;
            }
        }

        public static void CreatePrebuilds(PlayerAction_Build _this)
        {
            if (_this.waitConfirm && VFInput._buildConfirm.onDown && _this.buildPreviews.Count > 0)
            {
                _this.tmp_links.Clear();
                foreach (BuildPreview buildPreview in _this.buildPreviews)
                {
                    if (buildPreview.isConnNode)
                    {
                        buildPreview.lrot = Maths.SphericalRotation(buildPreview.lpos, 0f);
                    }
                    PrebuildData prebuild = default(PrebuildData);
                    prebuild.protoId = (short)buildPreview.item.ID;
                    prebuild.modelIndex = (short)buildPreview.desc.modelIndex;
                    prebuild.pos = buildPreview.lpos;
                    prebuild.pos2 = buildPreview.lpos2;
                    prebuild.rot = buildPreview.lrot;
                    prebuild.rot2 = buildPreview.lrot2;
                    prebuild.pickOffset = (short)buildPreview.inputOffset;
                    prebuild.insertOffset = (short)buildPreview.outputOffset;
                    prebuild.recipeId = buildPreview.recipeId;
                    prebuild.filterId = buildPreview.filterId;
                    prebuild.InitRefArray(buildPreview.refCount);
                    for (int i = 0; i < buildPreview.refCount; i++)
                    {
                        prebuild.refArr[i] = buildPreview.refArr[i];
                    }
                    bool flag = true;
                    if (buildPreview.coverObjId == 0 || buildPreview.willCover)
                    {
                        int id = buildPreview.item.ID;
                        int num = 1;
                        if (_this.player.inhandItemId == id && _this.player.inhandItemCount > 0)
                        {
                            _this.player.UseHandItems(1);
                        }
                        else
                        {
                            _this.player.package.TakeTailItems(ref id, ref num, false);
                        }
                        flag = (num == 1);
                    }
                    if (flag)
                    {
                        if (buildPreview.coverObjId == 0)
                        {
                            buildPreview.objId = -_this.factory.AddPrebuildDataWithComponents(prebuild);
                        }
                        else if (buildPreview.willCover)
                        {
                            int coverObjId = buildPreview.coverObjId;
                            bool flag2 = _this.ObjectIsBelt(coverObjId);
                            if (flag2)
                            {
                                for (int j = 0; j < 4; j++)
                                {
                                    bool flag3;
                                    int num2;
                                    int num3;
                                    _this.factory.ReadObjectConn(coverObjId, j, out flag3, out num2, out num3);
                                    int num4 = num2;
                                    if (num4 != 0 && _this.ObjectIsBelt(num4))
                                    {
                                        bool flag4 = false;
                                        for (int k = 0; k < 2; k++)
                                        {
                                            _this.factory.ReadObjectConn(num4, k, out flag3, out num2, out num3);
                                            if (num2 != 0)
                                            {
                                                bool flag5 = _this.ObjectIsBelt(num2);
                                                bool flag6 = _this.ObjectIsInserter(num2);
                                                if (!flag5 && !flag6)
                                                {
                                                    flag4 = true;
                                                    break;
                                                }
                                            }
                                        }
                                        if (flag4)
                                        {
                                            _this.tmp_links.Add(num4);
                                        }
                                    }
                                }
                            }
                            if (buildPreview.coverObjId > 0)
                            {
                                Array.Copy(_this.factory.entityConnPool, buildPreview.coverObjId * 16, _this.tmp_conn, 0, 16);
                                for (int l = 0; l < 16; l++)
                                {
                                    bool flag7;
                                    int num5;
                                    int otherSlotId;
                                    _this.factory.ReadObjectConn(buildPreview.coverObjId, l, out flag7, out num5, out otherSlotId);
                                    if (num5 > 0)
                                    {
                                        _this.factory.ApplyEntityDisconnection(num5, buildPreview.coverObjId, otherSlotId, l);
                                    }
                                }
                                Array.Clear(_this.factory.entityConnPool, buildPreview.coverObjId * 16, 16);
                            }
                            else
                            {
                                Array.Copy(_this.factory.prebuildConnPool, -buildPreview.coverObjId * 16, _this.tmp_conn, 0, 16);
                                Array.Clear(_this.factory.prebuildConnPool, -buildPreview.coverObjId * 16, 16);
                            }
                            buildPreview.objId = -_this.factory.AddPrebuildDataWithComponents(prebuild);
                            if (buildPreview.objId > 0)
                            {
                                Array.Copy(_this.tmp_conn, 0, _this.factory.entityConnPool, buildPreview.objId * 16, 16);
                            }
                            else
                            {
                                Array.Copy(_this.tmp_conn, 0, _this.factory.prebuildConnPool, -buildPreview.objId * 16, 16);
                            }
                            _this.factory.EnsureObjectConn(buildPreview.objId);
                        }
                        else
                        {
                            buildPreview.objId = buildPreview.coverObjId;
                        }
                    }
                    else
                    {
                        Assert.CannotBeReached();
                        UIRealtimeTip.Popup("物品不足".Translate(), true, 1);
                    }
                }
                foreach (BuildPreview buildPreview2 in _this.buildPreviews)
                {
                    if (buildPreview2.objId != 0)
                    {
                        if (buildPreview2.outputObjId != 0)
                        {
                            _this.factory.WriteObjectConn(buildPreview2.objId, buildPreview2.outputFromSlot, true, buildPreview2.outputObjId, buildPreview2.outputToSlot);
                        }
                        else if (buildPreview2.output != null)
                        {
                            _this.factory.WriteObjectConn(buildPreview2.objId, buildPreview2.outputFromSlot, true, buildPreview2.output.objId, buildPreview2.outputToSlot);
                        }
                        if (buildPreview2.inputObjId != 0)
                        {
                            _this.factory.WriteObjectConn(buildPreview2.objId, buildPreview2.inputToSlot, false, buildPreview2.inputObjId, buildPreview2.inputFromSlot);
                        }
                        else if (buildPreview2.input != null)
                        {
                            _this.factory.WriteObjectConn(buildPreview2.objId, buildPreview2.inputToSlot, false, buildPreview2.input.objId, buildPreview2.inputFromSlot);
                        }
                    }
                }
                foreach (BuildPreview buildPreview3 in _this.buildPreviews)
                {
                    if (buildPreview3.coverObjId != 0 && buildPreview3.willCover && buildPreview3.objId != 0 && _this.ObjectIsBelt(buildPreview3.objId))
                    {
                        bool flag8;
                        int num6;
                        int num7;
                        _this.factory.ReadObjectConn(buildPreview3.objId, 0, out flag8, out num6, out num7);
                        if (num6 != 0 && flag8 && _this.ObjectIsBelt(buildPreview3.objId))
                        {
                            int num8;
                            _this.factory.ReadObjectConn(num6, 0, out flag8, out num8, out num7);
                            if (num8 == buildPreview3.objId)
                            {
                                _this.factory.ClearObjectConn(num6, 0);
                            }
                        }
                    }
                }
                int num9 = 0;
                foreach (BuildPreview buildPreview4 in _this.buildPreviews)
                {
                    if (buildPreview4.coverObjId != 0 && buildPreview4.willCover)
                    {
                        _this.DoDestructObject(buildPreview4.coverObjId, out num9);
                    }
                    foreach (int objId in _this.tmp_links)
                    {
                        _this.DoDestructObject(objId, out num9);
                    }
                }
                _this.AfterPrebuild();
                begin = false;
            }
        }

        //[HarmonyTranspiler, HarmonyPatch(typeof(PlayerAction_Build), "CheckBuildConditions")]
        //public static IEnumerable<CodeInstruction> CheckPatch(IEnumerable<CodeInstruction> instructions)
        //{
        //    var codes = instructions.ToList();
        //    codes[1881].operand = 200f;
        //    return codes.AsEnumerable();
        //}

        /// <summary>
        /// 检查碰撞，如果撞到，则返回false
        /// </summary>
        public static bool CheckBox(ColliderData c1, ColliderData c2, float mul = 1f)
        {
            c1.ext *= mul;
            c2.ext *= mul;
            if (c1.ContainsInBox(c2.pos)) return false;
            Quaternion r = c2.q;
            if (c1.ContainsInBox(c2.pos + r * new Vector3(c2.ext.x, c2.ext.y, c2.ext.z))) return false;
            if (c1.ContainsInBox(c2.pos + r * new Vector3(c2.ext.x, c2.ext.y, -c2.ext.z))) return false;
            if (c1.ContainsInBox(c2.pos + r * new Vector3(c2.ext.x, -c2.ext.y, c2.ext.z))) return false;
            if (c1.ContainsInBox(c2.pos + r * new Vector3(c2.ext.x, -c2.ext.y, -c2.ext.z))) return false;
            if (c1.ContainsInBox(c2.pos + r * new Vector3(-c2.ext.x, c2.ext.y, c2.ext.z))) return false;
            if (c1.ContainsInBox(c2.pos + r * new Vector3(-c2.ext.x, c2.ext.y, -c2.ext.z))) return false;
            if (c1.ContainsInBox(c2.pos + r * new Vector3(-c2.ext.x, -c2.ext.y, c2.ext.z))) return false;
            if (c1.ContainsInBox(c2.pos + r * new Vector3(-c2.ext.x, -c2.ext.y, -c2.ext.z))) return false;

            if (c2.ContainsInBox(c1.pos)) return false;
            r = c1.q;
            if (c2.ContainsInBox(c1.pos + r * new Vector3(c1.ext.x, c1.ext.y, c1.ext.z))) return false;
            if (c2.ContainsInBox(c1.pos + r * new Vector3(c1.ext.x, c1.ext.y, -c1.ext.z))) return false;
            if (c2.ContainsInBox(c1.pos + r * new Vector3(c1.ext.x, -c1.ext.y, c1.ext.z))) return false;
            if (c2.ContainsInBox(c1.pos + r * new Vector3(c1.ext.x, -c1.ext.y, -c1.ext.z))) return false;
            if (c2.ContainsInBox(c1.pos + r * new Vector3(-c1.ext.x, c1.ext.y, c1.ext.z))) return false;
            if (c2.ContainsInBox(c1.pos + r * new Vector3(-c1.ext.x, c1.ext.y, -c1.ext.z))) return false;
            if (c2.ContainsInBox(c1.pos + r * new Vector3(-c1.ext.x, -c1.ext.y, c1.ext.z))) return false;
            if (c2.ContainsInBox(c1.pos + r * new Vector3(-c1.ext.x, -c1.ext.y, -c1.ext.z))) return false;
            return true;
        }
    }
}
