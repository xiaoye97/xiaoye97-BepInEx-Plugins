using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RollSystem
{
    public class RollLogic
    {
        // 奖池
        public List<int> RewardPool = new List<int>();

        // 物品字典
        public Dictionary<int, List<int>> ItemDict = new Dictionary<int, List<int>>();

        public List<int[]> RollRate = new List<int[]>
        {
            new int[] { 50, 20, 15, 10, 4, 1 }, // 普通抽奖券概率
            new int[] { 0, 0, 50, 30, 15, 5 }, // 高级抽奖券概率
            new int[] { 0, 0, 0, 0, 70, 30 }  // 超级抽奖券概率
        };

        /// <summary>
        /// 特定排除的物品
        /// </summary>
        public List<int> FilterItemIDList = new List<int>()
        {
            1, 117, 218, 304, 4536, 12025, 10016, 10101
        };

        public static int Ticket1ID = 90001;

        // 奖励可能的数量
        public int[] RewardCounts = new int[] { 1, 3, 5, 10, 50, 100, 1000 };

        // 奖励数量增长的概率
        public int[] RewardCountRate = new int[] { 70, 60, 40, 30, 20, 10 };

        public bool startRoll, endRoll;
        public List<int> ItemGrids = new List<int>();

        /// <summary>
        /// 开始抽奖
        /// </summary>
        /// <param name="level">抽奖级别</param>
        public void StartRoll(int level, int count = 1)
        {
            if (PlayerEx.Player.getItemNum(Ticket1ID + level) >= count)
            {
                // 单抽
                if (count == 1)
                {
                    PlayerEx.Player.removeItem(Ticket1ID + level);
                    RefreshItemDict();
                    FullRewardPool(RollRate[level]);
                    startRoll = true;
                    endRoll = false;
                    RollSystem.Inst.StartCoroutine(LoopItemGrid());
                }
                // 多抽，不播放动画
                else
                {
                    string resultMsg = "系统: 恭喜你获得 ";
                    int maxLevel = 0;
                    List<JSONClass._ItemJsonData> rewards = new List<JSONClass._ItemJsonData>();
                    for (int i = 0; i < count; i++)
                    {
                        PlayerEx.Player.removeItem(Ticket1ID + level);
                        RefreshItemDict();
                        FullRewardPool(RollRate[level]);
                        int id = RewardPool[Random.Range(0, RewardPool.Count)];
                        var item = JSONClass._ItemJsonData.DataDict[id];
                        rewards.Add(item);
                        if (item.quality > maxLevel)
                        {
                            maxLevel = item.quality;
                        }
                        int itemCount = RollCount(id);
                        PlayerEx.Player.addItem(id, itemCount, Tools.CreateItemSeid(id));
                        resultMsg += $"{item.quality}品{item.name}{itemCount}个 ";
                    }
                    // 检查是否触发保底
                    var ticket = JSONClass._ItemJsonData.DataDict[Ticket1ID + level];
                    // 中奖的最高等级小于等于抽奖券等级则触发保底
                    if (maxLevel <= ticket.quality)
                    {
                        if (level == 0)
                        {
                            PlayerEx.Player.addItem(90002, 1, Tools.CreateItemSeid(90002));
                            resultMsg += $"\n哎呀，看来你的运气不太好啊，额外送你一张{JSONClass._ItemJsonData.DataDict[90002].name}作为安慰吧。";
                        }
                        else if (level == 1)
                        {
                            PlayerEx.Player.addItem(90003, 1, Tools.CreateItemSeid(90003));
                            resultMsg += $"\n哎呀，看来你的运气不太好啊，额外送你一张{JSONClass._ItemJsonData.DataDict[90003].name}作为安慰吧。";
                        }
                        else if (level == 2)
                        {
                            PlayerEx.Player.addItem(90003, 3, Tools.CreateItemSeid(90003));
                            resultMsg += $"\n哎呀，看来你的运气不太好啊，额外送你3张{JSONClass._ItemJsonData.DataDict[90003].name}作为安慰吧。";
                        }
                    }
                    RollSystem.Inst.NowSay = resultMsg;
                }
            }
            else // 没有抽奖券
            {
                RollSystem.Inst.SetSay(4);
            }
        }

        public IEnumerator LoopItemGrid()
        {
            float cur = 0;
            float speed = 3f; // 初速度
            float a = 5f; // 加速度
            List<int> tmp = new List<int>();
            foreach (var t in RewardPool)
            {
                tmp.Add(t);
            }
            ItemGrids.Clear();
            for (int i = 0; i < 5; i++)
            {
                int index = Random.Range(0, tmp.Count);
                ItemGrids.Add(tmp[index]);
                tmp.RemoveAt(index);
            }
            while (speed > 0)
            {
                if (cur > 1)
                {
                    ItemGrids.RemoveAt(0);
                    int index = Random.Range(0, tmp.Count);
                    ItemGrids.Add(tmp[index]);
                    tmp.RemoveAt(index);
                    if (tmp.Count <= 0)
                    {
                        foreach (var t in RewardPool) tmp.Add(t);
                    }
                    cur -= 1;
                }
                cur += speed * Time.deltaTime;
                speed += a * Time.deltaTime;
                a -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            int id = ItemGrids[2];
            int count = RollCount(id);
            PlayerEx.Player.addItem(id, count, Tools.CreateItemSeid(id));
            var item = JSONClass._ItemJsonData.DataDict[id];
            RollSystem.Inst.NowSay = $"系统: 恭喜你获得{item.quality}品 {item.name} {count}个。";
            endRoll = true;
            yield break;
        }

        /// <summary>
        /// 过滤物品。如果此物品不能加进任何奖池，则返回false
        /// </summary>
        /// <param name="id">物品</param>
        /// <returns>是否能加入奖池</returns>
        public bool FilterItem(JSONClass._ItemJsonData item)
        {
            int t = item.type;
            // 排除任务道具、药渣、其他
            if (t == 7 || t == 11 || t == 16)
            {
                return false;
            }
            // 排除被标记删除的物品
            if (item.desc.Contains("删除"))
            {
                return false;
            }
            // 排除名字带情报的物品
            if (item.name.Contains("情报"))
            {
                return false;
            }
            // 特殊指定排除的物品
            if (FilterItemIDList.Contains(item.id))
            {
                return false;
            }
            // 排除请教的功法技能
            if (item.id > 100000)
            {
                return false;
            }
            // 排除炼器模板
            if (item.id >= 18001 && item.id <= 18010)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 刷新物品字典
        /// </summary>
        public void RefreshItemDict()
        {
            ItemDict.Clear();
            for (int i = 0; i < 6; i++)
            {
                ItemDict.Add(i, new List<int>());
            }
            foreach (var item in JSONClass._ItemJsonData.DataList)
            {
                if (FilterItem(item))
                {
                    ItemDict[item.quality - 1].Add(item.id);
                }
            }
        }

        /// <summary>
        /// 填充奖池
        /// </summary>
        /// <param name="rate"></param>
        public void FullRewardPool(int[] rate)
        {
            RewardPool.Clear();
            for (int q = 0; q < 6; q++)
            {
                int max = ItemDict[q].Count;
                for (int n = 0; n < rate[q]; n++)
                {
                    int id = ItemDict[q][Random.Range(0, max)];
                    RewardPool.Add(id);
                }
            }
        }

        /// <summary>
        /// 抽奖
        /// </summary>
        /// <param name="level">抽奖等级</param>
        /// <returns>抽到的物品</returns>
        public int RollItem(int level)
        {
            RefreshItemDict();
            FullRewardPool(RollRate[level]);
            int itemid = RewardPool[Random.Range(0, RewardPool.Count)];
            return itemid;
        }

        /// <summary>
        /// 随机奖励数量
        /// </summary>
        /// <param name="id">物品ID</param>
        /// <returns></returns>
        public int RollCount(int id)
        {
            var item = JSONClass._ItemJsonData.DataDict[id];
            int max = item.maxNum;
            int quality = item.quality - 1;
            if (max == 1) return 1;
            int cur = -1;
            bool flag = true;
            while (flag)
            {
                cur++;
                flag = Random.Range(0, 100) < RewardCountRate[quality];
            }
            cur = Mathf.Clamp(cur, 0, RewardCounts.Length - 1);
            int count = RewardCounts[cur];
            count = Mathf.Clamp(count, 1, max);
            return count;
        }
    }
}
