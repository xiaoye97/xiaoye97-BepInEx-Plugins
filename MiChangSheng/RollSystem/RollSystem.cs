using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using MCSDataHelper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RollSystem
{
    [BepInDependency("me.xiaoye97.plugin.MiChangSheng.MCSDataHelper", "1.1")]
    [BepInPlugin("me.xiaoye97.plugin.MiChangSheng.RollSystem", "抽奖系统", "1.1")]
    public class RollSystem : BaseUnityPlugin
    {
        public const string pluginId = "me.xiaoye97.plugin.MiChangSheng.RollSystem";

        private KBEngine.Avatar Player
        {
            get { return Tools.instance.getPlayer(); }
        }

        private ConfigEntry<KeyCode> HotKey;
        private static bool Show;
        private Rect winRect = new Rect(50, 50, 800, 420);

        // 奖池
        private List<int> RewardPool = new List<int>();

        // 物品字典
        private Dictionary<int, List<int>> ItemDict = new Dictionary<int, List<int>>();

        private List<int[]> RollRate = new List<int[]>
        {
            new int[] { 40, 25, 20, 10, 4, 1 }, // 普通抽奖券概率
            new int[] { 0, 0, 50, 30, 15, 5 }, // 高级抽奖券概率
            new int[] { 0, 0, 0, 0, 70, 30 }  // 超级抽奖券概率
        };

        private static int Tticket1ID = 90001;

        // 奖励可能的数量
        private int[] RewardCounts = new int[] { 1, 3, 5, 10, 50, 100, 1000 };

        // 奖励数量增长的概率
        private int[] RewardCountRate = new int[] { 70, 60, 40, 30, 20, 10 };

        private string[] SayConents = new string[] {
            "系统: 欢迎宿主使用本系统。我是由高维空间生灵制造的超级抽奖系统，可以帮助宿主获得更强的力量。系统每年都会生成一张[普通抽奖券]给予宿主，宿主可以用于抽奖获得大奖，奖品包含万物，什么都有可能得到。另外，每个高维空间日，宿主签到都会获得由高纬度泄露出的一张[高级抽奖券]，每3次连续签到，宿主会获得一张[超级抽奖券]。高级和超级抽奖券会获得更好的奖励。",
            "系统: 签到成功，你已获得一张[高级抽奖券]。",
            "系统: 签到成功，你已获得一张[高级抽奖券]和一张[超级抽奖券]。",
            "系统: 你今天已经签到过了，请明天再来。",
            "系统: 你没有足够的抽奖券。",
            "系统: 如果你想要找到创造我的人进行反馈，可以通过以下途径联系\nbilibili:宵夜97\nQQ:1066666683 夜空之下\n觅长生官方交流3群:1045590922"
        };

        private string NowSay;
        private Texture2D Head, Tticket1, Tticket2, Tticket3;

        private bool startRoll, endRoll;
        private List<int> ItemGrids = new List<int>();

        private void Start()
        {
            HotKey = Config.Bind<KeyCode>("config", "HotKey", KeyCode.R, "系统面板快捷键");
            Head = DataHelper.GetTex($"Mods/RollSystemMod/Texture/SystemGod.png");
            Tticket1 = DataHelper.GetTex($"Mods/RollSystemMod/Texture/RaffleTticket1.png");
            Tticket2 = DataHelper.GetTex($"Mods/RollSystemMod/Texture/RaffleTticket2.png");
            Tticket3 = DataHelper.GetTex($"Mods/RollSystemMod/Texture/RaffleTticket3.png");
            NowSay = SayConents[0];
            Harmony.CreateAndPatchAll(typeof(RollSystem));
        }

        private void Update()
        {
            if (Input.GetKeyDown(HotKey.Value))
            {
                Show = !Show;
                if (Show)
                {
                    NowSay = SayConents[0];
                }
            }
        }

        private void OnGUI()
        {
            if (Show)
            {
                GUI.backgroundColor = Color.black;
                winRect = GUILayout.Window(7777777, winRect, WindowFunc, "超级抽奖系统");
                GUI.backgroundColor = Color.white;
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(KBEngine.Avatar), "AddTime")]
        public static bool AddTimePatch(KBEngine.Avatar __instance, int addday, int addMonth, int Addyear)
        {
            System.DateTime nowTime = __instance.worldTimeMag.getNowTime();
            System.DateTime dateTime = nowTime.AddYears(Addyear).AddMonths(addMonth).AddDays(addday);
            int year = dateTime.Year - nowTime.Year;
            if (year > 0)
            {
                Tools.instance.getPlayer().addItem(Tticket1ID, null, year);
            }
            return true;
        }

        private void WindowFunc(int id)
        {
            GUILayout.BeginVertical();

            #region 对话部分

            // 对话部分
            GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Height(116));
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(112), GUILayout.Height(112));
            GUILayout.Label(Head);
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            if (Player == null)
                GUILayout.Label("系统: 未检测到宿主灵魂，系统当前处于待机状态。");
            else
                GUILayout.Label(NowSay, GUILayout.ExpandHeight(true));
            GUILayout.Space(1);
            // 对话按钮
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("你是谁?", GUILayout.Width(120), GUILayout.Height(40)))
            {
                NowSay = SayConents[0];
            }
            if (GUILayout.Button("我要签到", GUILayout.Width(120), GUILayout.Height(40)))
            {
                JSONObject dayQianDao = DataHelper.GetSaveData(pluginId, "dayQianDao");
                if (dayQianDao == null) dayQianDao = new JSONObject(JSONObject.Type.OBJECT);
                if (dayQianDao.HasField(System.DateTime.Today.ToString("d")))
                {
                    NowSay = SayConents[3];
                }
                else
                {
                    DayQianDao();
                }
            }
            if (GUILayout.Button("连续签到天数", GUILayout.Width(120), GUILayout.Height(40)))
            {
                var json = DataHelper.GetSaveData(pluginId, "LianXuCount");
                if (json == null)
                {
                    CalcLianXuCount();
                    json = DataHelper.GetSaveData(pluginId, "LianXuCount");
                }
                int day = json.I;
                NowSay = $"系统: 你现在已经连续签到了{day}天。";
            }
            if (GUILayout.Button("系统反馈", GUILayout.Width(120), GUILayout.Height(40)))
            {
                NowSay = SayConents[5];
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            #endregion 对话部分

            // 主体部分
            if (Player != null)
            {
                GUILayout.BeginHorizontal();
                if (startRoll) //滚动抽奖界面
                {
                    GUILayout.BeginVertical();
                    //转盘
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(90);
                    for (int i = 0; i < 5; i++)
                    {
                        int size = 128 - Mathf.Abs(i - 2) * 10;
                        JSONObject item = ItemGrids[i].ItemJson();
                        var itemData = JSONClass._ItemJsonData.DataDict[ItemGrids[i]];
                        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(size), GUILayout.Height(size));
                        GUILayout.Label(DataHelper.GetIcon(item), GUILayout.Width(size), GUILayout.Height(size));
                        GUI.contentColor = itemData.GetQualityColor();
                        GUILayout.Label(itemData.name);
                        GUI.contentColor = Color.white;
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndHorizontal();
                    // 按钮
                    GUILayout.BeginHorizontal();
                    if (endRoll)
                    {
                        if (GUILayout.Button("确定", GUILayout.ExpandHeight(true)))
                        {
                            startRoll = false;
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                }
                else // 准备抽奖界面
                {
                    #region 准备抽奖界面

                    GUILayout.BeginVertical(GUI.skin.box);
                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    GUILayout.Space(50);
                    GUILayout.Label(Tticket1);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(40);
                    GUI.contentColor = DataEx.QualityColors[0];
                    if (GUILayout.Button("普通抽奖", GUILayout.Height(50)))
                    {
                        StartRoll(0);
                    }
                    GUI.contentColor = Color.white;
                    GUILayout.EndVertical();

                    GUILayout.Space(20);

                    GUILayout.BeginVertical(GUI.skin.box);
                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    GUILayout.Space(50);
                    GUILayout.Label(Tticket2);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(40);
                    GUI.contentColor = DataEx.QualityColors[2];
                    if (GUILayout.Button("高级抽奖", GUILayout.Height(50)))
                    {
                        StartRoll(1);
                    }
                    GUI.contentColor = Color.white;
                    GUILayout.EndVertical();

                    GUILayout.Space(20);

                    GUILayout.BeginVertical(GUI.skin.box);
                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    GUILayout.Space(50);
                    GUILayout.Label(Tticket3);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(40);
                    GUI.contentColor = DataEx.QualityColors[4];
                    if (GUILayout.Button("超级抽奖", GUILayout.Height(50)))
                    {
                        StartRoll(2);
                    }
                    GUI.contentColor = Color.white;
                    GUILayout.EndVertical();

                    #endregion 准备抽奖界面
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        /// <summary>
        /// 计算连续签到次数
        /// </summary>
        private void CalcLianXuCount()
        {
            JSONObject dayQianDao = DataHelper.GetSaveData(pluginId, "dayQianDao");
            if (dayQianDao == null)
            {
                DataHelper.SetSaveData(pluginId, "LianXuCount", 0);
            }
            else
            {
                int count = 0;
                List<System.DateTime> SignedDays = new List<System.DateTime>();
                List<System.DateTime> tmpDays = new List<System.DateTime>();
                foreach (var dayStr in dayQianDao.keys)
                {
                    var day = System.DateTime.Parse(dayStr);
                    SignedDays.Add(day);
                }
                SignedDays.Sort();
                if (SignedDays[SignedDays.Count - 1].AddDays(1) == System.DateTime.Today || SignedDays[SignedDays.Count - 1] == System.DateTime.Today)
                {
                    System.DateTime lastDay = System.DateTime.Today;
                    while (SignedDays.Count > 0)
                    {
                        var day = SignedDays[SignedDays.Count - 1];
                        SignedDays.RemoveAt(SignedDays.Count - 1);
                        if (tmpDays.Count == 0)
                        {
                            count++;
                        }
                        else
                        {
                            if (day.AddDays(1) == lastDay)
                            {
                                count++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        tmpDays.Add(day);
                        lastDay = day;
                    }
                    DataHelper.SetSaveData(pluginId, "LianXuCount", count);
                }
                else
                {
                    DataHelper.SetSaveData(pluginId, "LianXuCount", 0);
                }
            }
        }

        /// <summary>
        /// 每日签到
        /// </summary>
        private void DayQianDao()
        {
            JSONObject dayQianDao = DataHelper.GetSaveData(pluginId, "dayQianDao");
            string today = System.DateTime.Today.ToString("d");
            if (dayQianDao == null)
            {
                dayQianDao = new JSONObject(JSONObject.Type.OBJECT);
            }
            if (!dayQianDao.HasField(today))
            {
                dayQianDao.AddField(today, true);
                DataHelper.SetSaveData(pluginId, "dayQianDao", dayQianDao);
                CalcLianXuCount();
                Player.addItem(Tticket1ID + 1, null, 1);
                NowSay = SayConents[1];
                if (DataHelper.GetSaveData(pluginId, "LianXuCount").I % 3 == 0)
                {
                    Player.addItem(Tticket1ID + 2, null, 1);
                    NowSay = SayConents[2];
                }
            }
        }

        /// <summary>
        /// 开始抽奖
        /// </summary>
        /// <param name="level">抽奖级别</param>
        private void StartRoll(int level)
        {
            if (Player.hasItem(Tticket1ID + level))
            {
                Player.removeItem(Tticket1ID + level);
                RefreshItemDict();
                FullRewardPool(RollRate[level]);
                startRoll = true;
                endRoll = false;
                StartCoroutine("LoopItemGrid");
            }
            else // 没有抽奖券
            {
                NowSay = SayConents[4];
            }
        }

        private IEnumerator LoopItemGrid()
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
            Player.addItem(id, Tools.CreateItemSeid(id), count);
            var item = JSONClass._ItemJsonData.DataDict[id];
            NowSay = $"系统: 恭喜你获得{item.quality}品 {item.name} {count}个。";
            endRoll = true;
            yield break;
        }

        /// <summary>
        /// 过滤物品。如果此物品不能加进任何奖池，则返回false
        /// </summary>
        /// <param name="id">物品</param>
        /// <returns>是否能加入奖池</returns>
        private bool FilterItem(JSONClass._ItemJsonData item)
        {
            int t = item.type;
            // 排除任务道具、药渣、其他
            if (t == 7 || t == 11 || t == 16)
            {
                return false;
            }
            if (item.desc.Contains("此物品已被删除") || item.name.Contains("情报"))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 刷新物品字典
        /// </summary>
        private void RefreshItemDict()
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
        private void FullRewardPool(int[] rate)
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
        private int RollItem(int level)
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
        private int RollCount(int id)
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