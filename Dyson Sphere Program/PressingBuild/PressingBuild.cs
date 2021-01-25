using System;
using BepInEx;
using HarmonyLib;
using System.Collections.Generic;

namespace PressingBuild
{
    [BepInPlugin("me.xiaoye97.plugin.Dyson.PressingBuild", "连续建造", "1.0")]
    public class PressingBuild : BaseUnityPlugin
    {
        void Start()
        {
            Harmony.CreateAndPatchAll(typeof(PressingBuild));
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PlayerAction_Build), "CreatePrebuilds")]
        public static bool BuildPtach(PlayerAction_Build __instance)
        {
            if (__instance.buildPreviews.Count > 0)
            {
                if (__instance.buildPreviews[0].item.ID == 2001 || __instance.buildPreviews[0].item.ID == 2002 || __instance.buildPreviews[0].item.ID == 2003)
                {
                    // 如果目标为传送带，则跳过
                    return true;
                }
                else
                {
                    //如果目标不为传送带，开启连续建造
                    CreatePrebuilds(__instance);
                    return false;
                }
            }
            return true;
        }

        public static bool ObjectIsBelt(PlayerAction_Build _this, int objId)
        {
            return Traverse.Create(_this).Method("ObjectIsBelt", objId).GetValue<bool>();
        }

        public static void CreatePrebuilds(PlayerAction_Build _this)
        {
            if (_this.waitConfirm && VFInput.rtsConfirm.pressing && _this.buildPreviews.Count > 0)
            {
                Traverse.Create(_this).Field("tmp_links").GetValue<List<int>>().Clear();
                var factory = Traverse.Create(_this).Field("factory").GetValue<PlanetFactory>();
                foreach (BuildPreview buildPreview in _this.buildPreviews)
                {
                    if (buildPreview.isConnNode)
                    {
                        buildPreview.lrot = Maths.SphericalRotation(buildPreview.lpos, 0f);
                    }
                    PrebuildData prebuild = default(PrebuildData);
                    prebuild.protoId = (short)buildPreview.item.ID;
                    prebuild.modelIndex = (short)buildPreview.desc.modelIndex;
                    prebuild.pos = _this.previewPose.position + _this.previewPose.rotation * buildPreview.lpos;
                    prebuild.pos2 = _this.previewPose.position + _this.previewPose.rotation * buildPreview.lpos2;
                    prebuild.rot = _this.previewPose.rotation * buildPreview.lrot;
                    prebuild.rot2 = _this.previewPose.rotation * buildPreview.lrot2;
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
                            buildPreview.objId = -factory.AddPrebuildDataWithComponents(prebuild);
                        }
                        else if (buildPreview.willCover)
                        {
                            int coverObjId = buildPreview.coverObjId;
                            bool flag2 = ObjectIsBelt(_this, coverObjId);
                            if (flag2)
                            {
                                for (int j = 0; j < 4; j++)
                                {
                                    bool flag3;
                                    int num2;
                                    int num3;
                                    factory.ReadObjectConn(coverObjId, j, out flag3, out num2, out num3);
                                    int num4 = num2;
                                    if (num4 != 0 && ObjectIsBelt(_this, num4))
                                    {
                                        bool flag4 = false;
                                        for (int k = 0; k < 2; k++)
                                        {
                                            factory.ReadObjectConn(num4, k, out flag3, out num2, out num3);
                                            if (num2 != 0)
                                            {
                                                bool flag5 = ObjectIsBelt(_this, num2);
                                                bool flag6 = Traverse.Create(_this).Method("ObjectIsInserter", num2).GetValue<bool>();
                                                if (!flag5 && !flag6)
                                                {
                                                    flag4 = true;
                                                    break;
                                                }
                                            }
                                        }
                                        if (flag4)
                                        {
                                            Traverse.Create(_this).Field("tmp_links").GetValue<List<int>>().Add(num4);
                                        }
                                    }
                                }
                            }
                            if (buildPreview.coverObjId > 0)
                            {
                                Array.Copy(factory.entityConnPool, buildPreview.coverObjId * 16, Traverse.Create(_this).Field("tmp_conn").GetValue<int[]>(), 0, 16);
                                for (int l = 0; l < 16; l++)
                                {
                                    bool flag7;
                                    int num5;
                                    int otherSlotId;
                                    factory.ReadObjectConn(buildPreview.coverObjId, l, out flag7, out num5, out otherSlotId);
                                    if (num5 > 0)
                                    {
                                        factory.ApplyEntityDisconnection(num5, buildPreview.coverObjId, otherSlotId, l);
                                    }
                                }
                                Array.Clear(factory.entityConnPool, buildPreview.coverObjId * 16, 16);
                            }
                            else
                            {
                                Array.Copy(factory.prebuildConnPool, -buildPreview.coverObjId * 16, Traverse.Create(_this).Field("tmp_conn").GetValue<int[]>(), 0, 16);
                                Array.Clear(factory.prebuildConnPool, -buildPreview.coverObjId * 16, 16);
                            }
                            buildPreview.objId = -factory.AddPrebuildDataWithComponents(prebuild);
                            if (buildPreview.objId > 0)
                            {
                                Array.Copy(Traverse.Create(_this).Field("tmp_conn").GetValue<int[]>(), 0, factory.entityConnPool, buildPreview.objId * 16, 16);
                            }
                            else
                            {
                                Array.Copy(Traverse.Create(_this).Field("tmp_conn").GetValue<int[]>(), 0, factory.prebuildConnPool, -buildPreview.objId * 16, 16);
                            }
                            factory.EnsureObjectConn(buildPreview.objId);
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
                            factory.WriteObjectConn(buildPreview2.objId, buildPreview2.outputFromSlot, true, buildPreview2.outputObjId, buildPreview2.outputToSlot);
                        }
                        else if (buildPreview2.output != null)
                        {
                            factory.WriteObjectConn(buildPreview2.objId, buildPreview2.outputFromSlot, true, buildPreview2.output.objId, buildPreview2.outputToSlot);
                        }
                        if (buildPreview2.inputObjId != 0)
                        {
                            factory.WriteObjectConn(buildPreview2.objId, buildPreview2.inputToSlot, false, buildPreview2.inputObjId, buildPreview2.inputFromSlot);
                        }
                        else if (buildPreview2.input != null)
                        {
                            factory.WriteObjectConn(buildPreview2.objId, buildPreview2.inputToSlot, false, buildPreview2.input.objId, buildPreview2.inputFromSlot);
                        }
                    }
                }
                foreach (BuildPreview buildPreview3 in _this.buildPreviews)
                {
                    if (buildPreview3.coverObjId != 0 && buildPreview3.willCover && buildPreview3.objId != 0 && ObjectIsBelt(_this, buildPreview3.objId))
                    {
                        bool flag8;
                        int num6;
                        int num7;
                        factory.ReadObjectConn(buildPreview3.objId, 0, out flag8, out num6, out num7);
                        if (num6 != 0 && flag8 && ObjectIsBelt(_this, buildPreview3.objId))
                        {
                            int num8;
                            factory.ReadObjectConn(num6, 0, out flag8, out num8, out num7);
                            if (num8 == buildPreview3.objId)
                            {
                                factory.ClearObjectConn(num6, 0);
                            }
                        }
                    }
                }
                foreach (BuildPreview buildPreview4 in _this.buildPreviews)
                {
                    if (buildPreview4.coverObjId != 0 && buildPreview4.willCover)
                    {
                        _this.DoDestructObject(buildPreview4.coverObjId);
                    }
                    foreach (int objId in Traverse.Create(_this).Field("tmp_links").GetValue<List<int>>())
                    {
                        _this.DoDestructObject(objId);
                    }
                }
                _this.AfterPrebuild();
            }
        }
    }
}
