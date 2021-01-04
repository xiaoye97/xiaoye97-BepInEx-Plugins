using BepInEx;
using UnityEngine;
using BepInEx.Configuration;

namespace GameSpeed
{
    [BepInPlugin("me.xiaoye97.plugin.MiChangSheng.GameSpeed", "游戏变速", "1.0")]
    public class GameSpeed : BaseUnityPlugin
    {
        ConfigEntry<float> SpeedConfig;
        ConfigEntry<KeyCode> KeyConfig;
        bool isChange;
        public bool IsChange
        {
            get { return isChange; }
            set
            {
                if(value)
                {
                    Time.timeScale = SpeedConfig.Value;
                }
                else
                {
                    Time.timeScale = 1;
                }
                isChange = value;
            }
        }
        void Start()
        {
            SpeedConfig = Config.Bind<float>("config", "Speed", 2f, "自定义变速倍数，变速范围0.2-5");
            SpeedConfig.Value = Mathf.Clamp(SpeedConfig.Value, 0.2f, 5);
            KeyConfig = Config.Bind<KeyCode>("config", "HotKey", KeyCode.B, "自定义热键");
        }

        void Update()
        {
            if(Input.GetKeyDown(KeyConfig.Value))
            {
                IsChange = !IsChange;
            }
        }
    }
}
