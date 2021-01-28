using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace BatchDestruct
{
    [BepInPlugin("me.xiaoye97.plugin.Dyson.BatchDestruct", "BatchDestruct", "1.0")]
    public class BatchDestruct : BaseUnityPlugin
    {
        void Start()
        {
            Harmony.CreateAndPatchAll(typeof(BatchDestruct));
        }

        /// <summary>
        /// 根据传送带id获取传送带线路
        /// </summary>
        public static CargoPath GetPathByBeltId(PlanetFactory factory, int beltId)
        {
            foreach (var path in factory.cargoTraffic.pathPool)
            {
                if (path == null) continue;
                if (path.belts.Contains(beltId))
                {
                    return path;
                }
            }
            return null;
        }

        /// <summary>
        /// 根据线路获取实体
        /// </summary>
        public static List<EntityData> GetEntitysByPath(PlanetFactory factory, CargoPath path)
        {
            List<EntityData> entityList = new List<EntityData>();
            foreach (var entity in factory.entityPool)
            {
                if (path.belts.Contains(entity.beltId))
                {
                    entityList.Add(entity);
                }
            }
            return entityList;
        }

        /// <summary>
        /// 根据物品描述获取实体
        /// </summary>
        public static List<EntityData> GetEntitysByProto(PlanetFactory factory, ItemProto itemProto)
        {
            List<EntityData> entityList = new List<EntityData>();
            foreach (var entity in factory.entityPool)
            {
                if (entity.id != 0)
                {
                    if (entity.protoId == itemProto.ID)
                    {
                        if ((entity.pos - GameMain.data.mainPlayer.position).sqrMagnitude <= GameMain.data.mainPlayer.mecha.buildArea * GameMain.data.mainPlayer.mecha.buildArea)
                        {
                            entityList.Add(entity);
                        }
                    }
                }
            }
            return entityList;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PlayerAction_Build), "DetermineDestructPreviews")]
        public static bool DetermineDestructPreviewsPatch(PlayerAction_Build __instance)
        {
            if (!Input.GetKey(KeyCode.LeftShift)) return true;
            var _this = __instance;
            PlanetFactory factory = Traverse.Create(_this).Field("factory").GetValue<PlanetFactory>();
            if (!VFInput.onGUI)
            {
                UICursor.SetCursor(ECursor.Delete);
            }
            _this.previewPose.position = Vector3.zero;
            _this.previewPose.rotation = Quaternion.identity;
            if (_this.castObjId != 0)
            {
                ItemProto itemProto = Traverse.Create(_this).Method("GetItemProto", _this.castObjId).GetValue<ItemProto>();
                if (itemProto != null)
                {
                    _this.ClearBuildPreviews();
                    List<EntityData> entityList;
                    if (factory.entityPool[_this.castObjId].beltId != 0)
                    {
                        var path = GetPathByBeltId(factory, factory.entityPool[_this.castObjId].beltId);
                        entityList = GetEntitysByPath(factory, path);
                    }
                    else
                    {
                        entityList = GetEntitysByProto(factory, itemProto);
                    }
                    for (int i = 0; i < entityList.Count; i++)
                    {
                        _this.AddBuildPreview(new BuildPreview());
                    }
                    for (int i = 0; i < _this.buildPreviews.Count; i++)
                    {
                        BuildPreview buildPreview = _this.buildPreviews[i];
                        ItemProto proto = Traverse.Create(_this).Method("GetItemProto", entityList[i].id).GetValue<ItemProto>();
                        buildPreview.item = proto;
                        buildPreview.desc = proto.prefabDesc;
                        buildPreview.lpos = entityList[i].pos;
                        buildPreview.lrot = entityList[i].rot;
                        buildPreview.objId = entityList[i].id;
                        if (buildPreview.desc.lodCount > 0 && buildPreview.desc.lodMeshes[0] != null)
                        {
                            buildPreview.needModel = true;
                        }
                        else
                        {
                            buildPreview.needModel = false;
                        }
                        buildPreview.isConnNode = true;
                        bool isInserter = buildPreview.desc.isInserter;
                        if (isInserter)
                        {
                            Pose objectPose2 = Traverse.Create(_this).Method("GetObjectPose2", buildPreview.objId).GetValue<Pose>();
                            buildPreview.lpos2 = objectPose2.position;
                            buildPreview.lrot2 = objectPose2.rotation;
                        }
                        if ((buildPreview.lpos - _this.player.position).sqrMagnitude > _this.player.mecha.buildArea * _this.player.mecha.buildArea)
                        {
                            buildPreview.condition = EBuildCondition.OutOfReach;
                            _this.cursorText = "目标超出范围".Translate();
                            _this.cursorWarning = true;
                        }
                        else
                        {
                            buildPreview.condition = EBuildCondition.Ok;
                            _this.cursorText = "拆除".Translate() + buildPreview.item.name;
                        }
                        if (buildPreview.desc.multiLevel)
                        {
                            bool flag;
                            int num;
                            int num2;
                            factory.ReadObjectConn(buildPreview.objId, 15, out flag, out num, out num2);
                            if (num != 0)
                            {
                                buildPreview.condition = EBuildCondition.Covered;
                                _this.cursorText = buildPreview.conditionText;
                            }
                        }
                    }
                }
                else
                {
                    _this.ClearBuildPreviews();
                }
            }
            else
            {
                _this.ClearBuildPreviews();
            }
            return false;
        }
    }
}
