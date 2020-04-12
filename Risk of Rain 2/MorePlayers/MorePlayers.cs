using RoR2;
using BepInEx;
using HarmonyLib;
using BepInEx.Configuration;

namespace xiaoye97
{
    [BepInPlugin("me.xiaoye97.plugin.MorePlayers", "更多玩家", "1.0")]
    public class MorePlayers : BaseUnityPlugin
    {
        public static ConfigEntry<int> playerCountConfig;

        void Awake()
        {
            playerCountConfig = Config.Bind("Config", "PlayerCount", 16, "玩家数量");
            if(playerCountConfig.Value >= 4)
            {
                Traverse.Create<RoR2Application>().Field("maxPlayers").SetValue(playerCountConfig.Value);
                Traverse.Create<RoR2Application>().Field("hardMaxPlayers").SetValue(playerCountConfig.Value);
                Traverse.Create<RoR2Application>().Field("maxLocalPlayers").SetValue(playerCountConfig.Value);
                Logger.Log(BepInEx.Logging.LogLevel.Info, "玩家数量上限设定为"+playerCountConfig.Value);
            }
            else Logger.Log(BepInEx.Logging.LogLevel.Warning, "玩家数量上限不能设置到4以下");
        }
    }
}
