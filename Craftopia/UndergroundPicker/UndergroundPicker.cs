using Oc;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using BepInEx.Configuration;

namespace UndergroundPicker
{
    [BepInPlugin("me.xiaoye97.plugin.Craftopia.UndergroundPicker", "UndergroundPicker", "1.0")]
    public class UndergroundPicker : BaseUnityPlugin
    {
        public static OcInstallObjMng Inst;
        private ConfigEntry<float> checkTime;
        private float cd;

        void Start()
        {
            checkTime = Config.Bind<float>("Setting", "CheckTime", 2, "每隔多长时间触发一次检测(秒)");
            cd = checkTime.Value;
        }

        void Update()
        {
            if (cd > 0) cd -= Time.deltaTime;
            else
            {
                cd = checkTime.Value;
                TryPickup();
            }
        }

        void TryPickup()
        {
            if(Inst == null)
            {
                if (SingletonMonoBehaviour<OcInstallObjMng>.Inst == null) return;
                else Inst = SingletonMonoBehaviour<OcInstallObjMng>.Inst;
            }
            var pickers = Inst.transform.GetComponentsInChildren<OcPicker>();
            foreach (var picker in pickers)
            {
                if (picker.transform.position.y < -1)
                {
                    Traverse.Create(picker).Field("_PickupEventCmp").GetValue<OcPickupEvent>().PickupHold();
                }
            }
        }
    }
}
