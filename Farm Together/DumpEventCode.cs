using BepInEx;
using System.IO;
using UnityEngine;
using Logic.Events;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DumpEventCode
{
    [BepInPlugin("me.xiaoye97.plugin.DumpEventCode", "DumpEventCode", "1.0")]
    public class DumpEventCode : BaseUnityPlugin
    {
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.F10))
            {
                Dump();
            }
        }

        void Dump()
        {
            Logger.Log(BepInEx.Logging.LogLevel.Info, "开始Dump...");
            //收集全部物品信息
            List<ItemDefinition> itemList = new List<ItemDefinition>();
            foreach(var kv in ShopManager.ItemDictionary) itemList.Add(kv.Value);
            foreach (var kv in ShopManager.HouseItemDictionary) itemList.Add(kv.Value);
            foreach (var kv in ShopManager.HouseRoomMaterialDictionary) itemList.Add(kv.Value);
            foreach (var kv in ShopManager.RecipeDictionary) itemList.Add(kv.Value);
            foreach (var kv in CharacterManager.CharacterItemDictionary) itemList.Add(kv.Value);
            foreach (var kv in CharacterManager.FarmhandBodyDictionary) itemList.Add(kv.Value);
            foreach (var kv in PetManager.PetItemDictionary) itemList.Add(kv.Value);
            foreach (var kv in VehicleManager.VehicleItemDictionary) itemList.Add(kv.Value);
            //收集活动奖励信息
            List<ItemDefinition> eventItemList = new List<ItemDefinition>();
            foreach (var item in itemList) if (item.IsEventReward) eventItemList.Add(item);
            //收集已经开始的活动代码
            List<EventCode> startEventCodeList = new List<EventCode>();
            List<EventCode> noStartEventCodeList = new List<EventCode>(); //没开始的
            foreach (var item in eventItemList)
            {
                if(item.Enabled && IsEventStart(item.SeasonalEvent))
                {
                    startEventCodeList.Add(new EventCode(item));
                }
                else
                {
                    noStartEventCodeList.Add(new EventCode(item));
                }
            }

            var json = JsonConvert.SerializeObject(startEventCodeList);
            File.WriteAllText($"{Paths.PluginPath}\\EventCode.json", $"\"Rewards\":{json},");
            Logger.Log(BepInEx.Logging.LogLevel.Info, json);
            json = JsonConvert.SerializeObject(noStartEventCodeList);
            File.WriteAllText($"{Paths.PluginPath}\\NoSatrtEventCode.json", json);
        }

        bool IsEventStart(SeasonalEvents events)
        {
            var e = EventManager.GetEvent(events);
            return e.HasEverStarted(System.DateTime.UtcNow);
        }

        public class EventCode
        {
            public string id;

            public EventCode(ItemDefinition item)
            {
                id = item.FullId;
            }
        }
    }
}
