using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using SkySwordKill.Next;
using BepInEx.Configuration;

namespace RollSystem
{
    [BepInDependency("skyswordkill.plugin.Next", "0.2.9")]
    [BepInPlugin("me.xiaoye97.plugin.MiChangSheng.RollSystem", "抽奖系统", "1.3")]
    public class RollSystem : BaseUnityPlugin
    {
        public static RollSystem Inst;
        private ConfigEntry<KeyCode> HotKey;
        private static bool Show;
        private Rect winRect = new Rect(50, 50, 800, 420);

        private string[] SayConents = new string[] 
        {
            "系统: 欢迎宿主使用本系统。我是由高维空间生灵制造的超级抽奖系统，可以帮助宿主获得更强的力量。系统每年都会生成一张[普通抽奖券]给予宿主，宿主可以用于抽奖获得大奖，奖品包含万物，什么都有可能得到。另外，每个高维空间日，宿主签到都会获得由高纬度泄露出的一张[高级抽奖券]，每3次连续签到，宿主会获得一张[超级抽奖券]。高级和超级抽奖券会获得更好的奖励。",
            "系统: 签到成功，你已获得一张[高级抽奖券]。",
            "系统: 签到成功，你已获得一张[高级抽奖券]和一张[超级抽奖券]。",
            "系统: 你今天已经签到过了，请明天再来。",
            "系统: 你没有足够的抽奖券。",
            "系统: 如果你想要找到创造我的人进行反馈，可以通过以下途径联系\nbilibili:宵夜97\nQQ:1066666683\n觅长生官方交流群:游戏开始界面公告(我所有群都在)\n觅长生3DM交流群:103246254"
        };

        public string NowSay;
        private Texture2D Head => DataEx.GetAsset<Texture2D>("Assets/SystemGod.png");
        private Texture2D Tticket1 => DataEx.GetAsset<Texture2D>("Assets/Item Icon/90001.png");
        private Texture2D Tticket2 => DataEx.GetAsset<Texture2D>("Assets/Item Icon/90002.png");
        private Texture2D Tticket3 => DataEx.GetAsset<Texture2D>("Assets/Item Icon/90003.png");

        private RollLogic roll;

        /// <summary>
        /// 今天日期
        /// </summary>
        public int Today
        {
            get
            {
                var today = DateTime.Today;
                int day = 0;
                day += today.Year * 10000;
                day += today.Month * 100;
                day += today.Day;
                return day;
            }
        }

        /// <summary>
        /// 连续签到次数
        /// </summary>
        public int LianXuQianDaoCount
        {
            get
            {
                int lianXu = DialogAnalysis.GetInt("抽奖系统_连续签到次数");
                if (lianXu > 0)
                {
                    // 检查是否需要中断连续
                    if (Today > LastQianDaoDay + 1)
                    {
                        LianXuQianDaoCount = 0;
                        lianXu = 0;
                    }
                }
                return lianXu;
            }
            set
            {
                DialogAnalysis.SetInt("抽奖系统_连续签到次数", value);
            }
        }

        /// <summary>
        /// 最后签到日期
        /// </summary>
        public int LastQianDaoDay
        {
            get
            {
                string last = DialogAnalysis.GetStr("抽奖系统_最后签到日期");
                if (string.IsNullOrEmpty(last))
                {
                    return 0;
                }
                int day = int.Parse(last);
                return day;
            }
            set
            {
                DialogAnalysis.SetStr("抽奖系统_最后签到日期", value.ToString());
            }
        }

        private void Start()
        {
            Inst = this;
            HotKey = Config.Bind<KeyCode>("config", "HotKey", KeyCode.R, "系统面板快捷键");
            SetSay(0);
            roll = new RollLogic();
            Harmony.CreateAndPatchAll(typeof(RollSystemPatch));
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
            if (Input.GetKeyDown(KeyCode.T))
            {
                PlayerEx.Player.addItem(RollLogic.Ticket1ID, 1000, null);
                PlayerEx.Player.addItem(RollLogic.Ticket1ID + 1, 1000, null);
                PlayerEx.Player.addItem(RollLogic.Ticket1ID + 2, 1000, null);
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

        public void SetSay(int say)
        {
            NowSay = SayConents[say];
        }

        /// <summary>
        /// 每日签到
        /// </summary>
        public void DayQianDao()
        {
            int lianXu = RollSystem.Inst.LianXuQianDaoCount;
            RollSystem.Inst.LastQianDaoDay = RollSystem.Inst.Today;
            RollSystem.Inst.LianXuQianDaoCount = lianXu + 1;
            PlayerEx.Player.addItem(RollLogic.Ticket1ID + 1, 1, null);
            RollSystem.Inst.SetSay(1);
            if (RollSystem.Inst.LianXuQianDaoCount % 3 == 0)
            {
                PlayerEx.Player.addItem(RollLogic.Ticket1ID + 2, 1, null);
                RollSystem.Inst.SetSay(2);
            }
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
            if (PlayerEx.Player == null)
                GUILayout.Label("系统: 未检测到宿主灵魂，系统当前处于待机状态。");
            else
                GUILayout.Label(NowSay, GUILayout.ExpandHeight(true));
            GUILayout.Space(1);
            // 对话按钮
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("你是谁?", GUILayout.Width(120), GUILayout.Height(40)))
            {
                SetSay(0);
            }
            if (GUILayout.Button("我要签到", GUILayout.Width(120), GUILayout.Height(40)))
            {
                if (PlayerEx.Player != null)
                {
                    if (Today == LastQianDaoDay)
                    {
                        SetSay(3);
                    }
                    else
                    {
                        DayQianDao();
                    }
                }
            }
            if (GUILayout.Button("连续签到天数", GUILayout.Width(120), GUILayout.Height(40)))
            {
                if (PlayerEx.Player != null)
                {
                    NowSay = $"系统: 你现在已经连续签到了{LianXuQianDaoCount}天。";
                }
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
            if (PlayerEx.Player != null)
            {
                GUILayout.BeginHorizontal();
                if (roll.startRoll) //滚动抽奖界面
                {
                    GUILayout.BeginVertical();
                    //转盘
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(90);
                    for (int i = 0; i < 5; i++)
                    {
                        int size = 128 - Mathf.Abs(i - 2) * 10;
                        JSONObject item = roll.ItemGrids[i].ItemJson();
                        var itemData = JSONClass._ItemJsonData.DataDict[roll.ItemGrids[i]];
                        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(size), GUILayout.Height(size));
                        GUILayout.Label(GetIcon(item), GUILayout.Width(size), GUILayout.Height(size));
                        GUI.contentColor = DataEx.QualityColors[itemData.quality - 1];
                        GUILayout.Label(itemData.name);
                        GUI.contentColor = Color.white;
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndHorizontal();
                    // 按钮
                    GUILayout.BeginHorizontal();
                    if (roll.endRoll)
                    {
                        if (GUILayout.Button("确定", GUILayout.ExpandHeight(true)))
                        {
                            roll.startRoll = false;
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
                        roll.StartRoll(0);
                    }
                    if (GUILayout.Button("普通十连", GUILayout.Height(50)))
                    {
                        roll.StartRoll(0, 10);
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
                        roll.StartRoll(1);
                    }
                    if (GUILayout.Button("高级十连", GUILayout.Height(50)))
                    {
                        roll.StartRoll(1, 10);
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
                        roll.StartRoll(2);
                    }
                    if (GUILayout.Button("超级十连", GUILayout.Height(50)))
                    {
                        roll.StartRoll(2, 10);
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

        public Texture2D GetIcon(JSONObject item)
        {
            Texture2D texture2D;
            if (item["ItemIcon"].I == 0)
            {
                texture2D = ResManager.inst.LoadTexture2D("Item Icon/" + item["id"].ToString());
            }
            else
            {
                texture2D = ResManager.inst.LoadTexture2D("Item Icon/" + (item["ItemIcon"].I).ToString());
            }
            if (texture2D == null)
            {
                texture2D = ResManager.inst.LoadTexture2D("Item Icon/1");
            }
            return texture2D;
        }
    }
}