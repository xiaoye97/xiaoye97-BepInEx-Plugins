using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace AdvancedBuildDestruct
{
    public class DestructPatch
    {
        private static List<int> entityIdList = new List<int>();
        private static List<EntityData> cacheEntityList = new List<EntityData>();
        private static List<EntityData> entityList = new List<EntityData>();
        private static int cur = 0;

        /// <summary>
        /// 根据物品描述获取实体
        /// </summary>
        public static void FindEntitysByProto(PlanetFactory factory, ItemProto itemProto, int castObjId)
        {
            if (entityIdList.Contains(castObjId)) return;
            // Debug.Log($"开始查询 {itemProto.Name.Translate()} {castObjId}");
            cur = 0;
            entityIdList.Clear();
            entityList.Clear();
            cacheEntityList.Clear();
            EntityData start = EntityData.Null;
            // 将同一种物品加入缓存
            foreach (var entity in factory.entityPool)
            {
                if (entity.id == castObjId) start = entity;
                else if (itemProto.ID == entity.protoId)
                {
                    cacheEntityList.Add(entity);
                }
            }
            // 将初始物品加入结果池
            entityList.Add(start);
            entityIdList.Add(castObjId);
            // 搜索相连物品
            while (cur < entityList.Count)
            {
                Vector3 pos = entityList[cur].pos;
                for (int i = 0; i < cacheEntityList.Count; i++)
                {
                    if (Vector3.Distance(cacheEntityList[i].pos, pos) < AdvancedBuildDestruct.FindBuildDistance.Value)
                    {
                        entityList.Add(cacheEntityList[i]);
                        entityIdList.Add(cacheEntityList[i].id);
                    }
                }
                for (int i = cur; i < entityList.Count; i++)
                {
                    cacheEntityList.Remove(entityList[i]);
                }
                cur++;
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PlayerAction_Build), "DetermineDestructPreviews")]
        public static bool DetermineDestructPreviews(PlayerAction_Build __instance)
        {
            if (!Input.GetKey(AdvancedBuildDestruct.DestructKey.Value))
            {
                entityIdList.Clear();
                return true;
            }
            var _this = __instance;
            if (!VFInput.onGUI)
            {
                UICursor.SetCursor(ECursor.Delete);
            }
            _this.previewPose.position = Vector3.zero;
            _this.previewPose.rotation = Quaternion.identity;
            if (_this.castObjId > 0)
            {
                ItemProto itemProto = _this.GetItemProto(_this.castObjId);
                if (itemProto.prefabDesc.isBelt) return true; // 如果是传送带，则调用原版函数

                if (itemProto != null)
                {
                    _this.ClearBuildPreviews();
                    FindEntitysByProto(_this.factory, itemProto, _this.castObjId);
                    for (int i = 0; i < entityList.Count; i++)
                    {
                        _this.AddBuildPreview(new BuildPreview());
                    }
                    int index = 0;
                    foreach (var entity in entityList)
                    {
                        BuildPreview buildPreview = _this.buildPreviews[index];
                        buildPreview.item = itemProto;
                        buildPreview.desc = itemProto.prefabDesc;
                        buildPreview.lpos = entity.pos;
                        buildPreview.lrot = entity.rot;
                        buildPreview.objId = entity.id;
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
                            Pose objectPose2 = _this.GetObjectPose2(buildPreview.objId);
                            buildPreview.lpos2 = objectPose2.position;
                            buildPreview.lrot2 = objectPose2.rotation;
                        }
                        PlanetData planetData = _this.player.planetData;
                        Vector3 vector = _this.player.position;
                        if (planetData.type == EPlanetType.Gas)
                        {
                            vector = vector.normalized;
                            vector *= planetData.realRadius;
                        }
                        else
                        {
                            buildPreview.condition = EBuildCondition.Ok;
                            _this.cursorText = "拆除".Translate() + buildPreview.item.name + "\r\n" + "连锁拆除提示".Translate();
                        }
                        if (buildPreview.desc.multiLevel)
                        {
                            bool flag;
                            int num;
                            int num2;
                            _this.factory.ReadObjectConn(buildPreview.objId, 15, out flag, out num, out num2);
                            if (num != 0)
                            {
                                buildPreview.condition = EBuildCondition.Covered;
                                _this.cursorText = buildPreview.conditionText;
                            }
                        }
                        index++;
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
