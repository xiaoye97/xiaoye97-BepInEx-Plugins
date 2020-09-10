using Oc;
using BepInEx;
using HarmonyLib;

namespace MoreSkillPoints
{
    [BepInPlugin("me.xiaoye97.plugin.Craftopia.MoreSkillPoints", "MoreSkillPoints", "1.0")]
    public class MoreSkillPoints : BaseUnityPlugin
    {
        public static int PointMul = 2;

        void Start()
        {
            Traverse.Create(typeof(OcDefine)).Field("INCREASE_SKILLPOINT_BY_LEVEL_UP").SetValue(PointMul);
        }
    }
}
