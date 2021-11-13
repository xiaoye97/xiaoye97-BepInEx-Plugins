using HarmonyLib;

namespace RollSystem
{
    public class RollSystemPatch
    {
        [HarmonyPrefix, HarmonyPatch(typeof(KBEngine.Avatar), "AddTime")]
        public static bool AddTimePatch(KBEngine.Avatar __instance, int addday, int addMonth, int Addyear)
        {
            System.DateTime nowTime = __instance.worldTimeMag.getNowTime();
            System.DateTime dateTime = nowTime.AddYears(Addyear).AddMonths(addMonth).AddDays(addday);
            int year = dateTime.Year - nowTime.Year;
            if (year > 0)
            {
                Tools.instance.getPlayer().addItem(90001, year, null);
            }
            return true;
        }
    }
}
