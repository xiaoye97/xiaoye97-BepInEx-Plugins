using System;
using BepInEx;
using System.IO;
using GUIPackage;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections;
using UnityEngine.UI;

namespace MCSDataHelper
{
    public static class DataPatch
    {
        /// <summary>
        /// Json数据的转储
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(jsonData), "init")]
        public static bool InitJsonPatch(string path)
        {
            if(DataHelper.DumpConfig.Value) //转储数据
            {
                if(!Directory.Exists($"{Paths.GameRootPath}/Dump"))
                {
                    Directory.CreateDirectory($"{Paths.GameRootPath}/Dump");
                }
                TextAsset textAsset = (TextAsset)Resources.Load(path);
                if(textAsset != null)
                {
                    string[] tmp = path.Split('/');
                    string fileName = tmp[tmp.Length - 1];
                    Debug.Log($"转储：{fileName}");
                    string text = textAsset.text.UnCode64();
                    File.WriteAllText($"{Paths.GameRootPath}/Dump/{fileName}.json", text);
                }
            }
            return true;
        }

		/// <summary>
		/// 递归查找json
		/// </summary>
		private static void AddJson(List<string> paths, string rootpath, string path)
        {
			DirectoryInfo dir = new DirectoryInfo(path);
			foreach(var file in dir.GetFiles())
            {
				if (rootpath.EndsWith(file.Name))
					paths.Add(file.FullName);
			}
			foreach(var d in dir.GetDirectories())
            {
				AddJson(paths, rootpath, d.FullName);
			}
		}

        /// <summary>
        /// Josn数据的加载
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(jsonData), "init")]
        public static void ItemJsonPatchPost(string path, ref JSONObject jsondata)
        {
            if(jsondata.Count == 0)
            {
                return;
            }
            if (!Directory.Exists($"{Paths.GameRootPath}/Mods"))
            {
                Directory.CreateDirectory($"{Paths.GameRootPath}/Mods");
            }
            //读取json列表
            List<string> jsonPathList = new List<string>();
			AddJson(jsonPathList, path, $"{Paths.GameRootPath}/Mods");
            if(jsonPathList.Count > 0)
            {
                foreach (var jsonPath in jsonPathList)
                {
                    var json = File.ReadAllText(jsonPath);
                    json = json.ToUnicode();
                    JSONObject jobj = new JSONObject(json, -2, false, false);
                    foreach (var j in jobj.list)
                    {
                        jsondata.AddField(j["id"].I.ToString(), j);
                    }
                }
                //File.WriteAllText(Paths.GameRootPath + "/ModLog.txt", jsondata.ToString());
            }
        }

        /// <summary>
        /// 修复Mod物品图标的加载
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(item), MethodType.Constructor, new Type[] { typeof(string), typeof(int), typeof(string), typeof(string), typeof(int), typeof(item.ItemType), typeof(int)})]
        public static void ItemIconPatch(item __instance)
        {
            //Mod物品的ID需大于100000，以免和游戏自带物品重复
            if (__instance.itemID < 100000) return;
            if (__instance.itemID.ItemJson().HasField("ModIcon"))
            {
                var tex = DataHelper.GetTex(__instance.itemID.ItemJson()["ModIcon"].str);
                __instance.itemIcon = tex;
            }
        }

		//TODO 完善类型判断
        [HarmonyPrefix, HarmonyPatch(typeof(Inventory2), "Show_Tooltip")]
        public static bool ShowToolTipPatch(Inventory2 __instance, item Item, int money, int moneyPercent)
        {
            Inventory2 _this = __instance;
			JSONObject jsonobject = jsonData.instance.ItemJsonData[Item.itemID.ToString()]; 
			string a = Tools.Code64(jsonobject["name"].ToString());
			TooltipItem component = _this.Tooltip.GetComponent<TooltipItem>();
			component.Clear();
			string text = Tools.instance.Code64ToString(Inventory2.getSkillBookDesc(jsonobject));
			//武器
			if ((int)jsonobject["type"].n == 0)
			{
				try
				{
					if (!jsonData.instance.EquipSeidJsonData[2].HasField(Item.itemID.ToString()))
                    {
						Debug.LogError($"[MCSDataHelper]id为{Item.itemID}的物品未填写装备seid，请参考d_items.py.equip_seid2进行填写");
					}
					int num = (int)jsonData.instance.EquipSeidJsonData[2][Item.itemID.ToString()]["value1"].n;
					JSONObject jsonobject2 = jsonData.instance.skillJsonData[num.ToString()];
					int itemCD = Inventory2.GetItemCD(Item);
                    component.Label7.text = itemCD + "回合";
					string text2 = "";
					foreach (JSONObject jsonobject3 in Inventory2.GetItemAttackType(Item.Seid, jsonobject2["AttackType"]).list)
					{
						text2 += Tools.getStr("xibieFight" + (int)jsonobject3.n);
					}
					component.Label8.text = text2;
					component.setCenterTextTitle("【冷却】", "【属性】", "");
					text = "[f28125]【主动】[-] [E0DDB4]" + text.Replace("主动：", "");
					Traverse.Create(_this).Field("showToolType").SetValue(1);
				}
				catch (Exception message)
				{
					TooltipItem component2 = _this.Tooltip.GetComponent<TooltipItem>();
					component2.Clear();
					component2.Label2.text = $"[bfba7d]武器 物品出错{Item.itemID}[-]";
					Debug.LogWarning($"武器 物品出错{Item.itemID}");
					Debug.LogWarning(message);
				}
			}
			//TODO 功法书和杂道书
			else if ((int)jsonobject["type"].n == 4 || (int)jsonobject["type"].n == 13)
			{
				try
				{
					int studiSkillTime = Tools.getStudiSkillTime((int)jsonobject["StuTime"].n, jsonobject["wuDao"]);
					string str = Tools.getStr("xiaohaoshijian").Replace("{Y}", string.Concat(Tools.DayToYear(studiSkillTime))).Replace("{M}", string.Concat(Tools.DayToMonth(studiSkillTime))).Replace("{D}", string.Concat(Tools.DayToDay(studiSkillTime))).Replace("消耗时间：", "");
					string text3 = "";
					foreach (object obj in component.TooltipHelp.transform.parent)
					{
						Transform transform = (Transform)obj;
						if (transform.gameObject.activeSelf)
						{
							UnityEngine.Object.Destroy(transform.gameObject);
						}
					}
					text3 = text3 + "[d3b068]领悟时间：[-][E0DDB4]" + str + "[-]\n";
					List<int> wudaoTypeList = new List<int>();
					List<int> wudaoLvList = new List<int>();
					item.GetWuDaoType(Item.itemID, wudaoTypeList, wudaoLvList);
					string str2 = item.StudyTiaoJian(wudaoTypeList, wudaoLvList);
					text3 = text3 + "[d3b068]领悟条件:[-][E0DDB4]" + str2 + "[-]\n";
					string str3 = item.StudyTiSheng(wudaoTypeList, "领悟后能够提升对");
					text3 = text3 + "[d3b068]悟道提升[-][E0DDB4]" + str3 + "[-]";
					if ((int)jsonobject["type"].n == 13)
					{
						int jsonobject4 = Tools.getJsonobject(Tools.instance.getPlayer().NaiYaoXin, Item.itemID.ToString() ?? "");
						text3 = string.Concat(new object[]
						{
					text3,
					"\n[d3b068]领悟次数[-][E0DDB4]",
					jsonobject4,
					"/",
					(int)jsonobject["CanUse"].n,
					"[-]"
						});
					}
					component.ShowSkillTime(text3);
					Traverse.Create(_this).Field("showToolType").SetValue(2);
				}
				catch (Exception message)
				{
					TooltipItem component2 = _this.Tooltip.GetComponent<TooltipItem>();
					component2.Clear();
					component2.Label2.text = "[bfba7d]暂无说明[-]";
					Debug.LogWarning("2 Mod物品出错" + Item.itemID.ToString());
					Debug.LogWarning(message);
				}
				
			}
			//TODO 技能书
			else if ((int)jsonobject["type"].n == 3 && a != "情报玉简")
			{
				try
				{
					string text4 = "";
					int studiSkillTime2 = Tools.getStudiSkillTime((int)jsonobject["StuTime"].n, jsonobject["wuDao"]);
					string str4 = Tools.getStr("xiaohaoshijian").Replace("{Y}", string.Concat(Tools.DayToYear(studiSkillTime2))).Replace("{M}", string.Concat(Tools.DayToMonth(studiSkillTime2))).Replace("{D}", string.Concat(Tools.DayToDay(studiSkillTime2))).Replace("消耗时间：", "");
					text4 = text4 + "[d3b068]领悟时间：[-][E0DDB4]" + str4 + "[-]\n";
					List<int> wudaoTypeList2 = new List<int>();
					List<int> wudaoLvList2 = new List<int>();
					item.GetWuDaoType(Item.itemID, wudaoTypeList2, wudaoLvList2);
					string str5 = item.StudyTiaoJian(wudaoTypeList2, wudaoLvList2);
					text4 = text4 + "[d3b068]领悟条件:[-][E0DDB4]" + str5 + "[-]\n";
					string str6 = item.StudyTiSheng(wudaoTypeList2, "领悟后能够提升对");
					text4 = text4 + "[d3b068]悟道提升[-][E0DDB4]" + str6 + "[-]";
					component.ShowSkillTime(text4);
					TooltipItem tooltipItem = component;
					int num2 = (int)float.Parse(jsonobject["desc"].str);
					JSONObject jsonobject5 = new JSONObject();
					foreach (KeyValuePair<string, JSONObject> keyValuePair in jsonData.instance.skillJsonData)
					{
						if ((int)keyValuePair.Value["Skill_ID"].n == num2 && (int)keyValuePair.Value["Skill_Lv"].n == Tools.instance.getPlayer().getLevelType())
						{
							jsonobject5 = keyValuePair.Value;
							break;
						}
					}
					foreach (object obj2 in component.LingQiGride.transform)
					{
						Transform transform2 = (Transform)obj2;
						if (transform2.gameObject.activeSelf)
						{
							UnityEngine.Object.Destroy(transform2.gameObject);
						}
					}
					int num3 = 0;
					foreach (JSONObject jsonobject6 in jsonobject5["skill_CastType"].list)
					{
						if (num3 > 0)
						{
							_this.CreatGameObjectToParent(tooltipItem.LingQiGride, tooltipItem.LingQifengexianImage);
						}
						for (int i = 0; i < (int)jsonobject5["skill_Cast"][num3].n; i++)
						{
							_this.CreatGameObjectToParent(tooltipItem.LingQiGride, tooltipItem.lingqiGridImage).GetComponent<Image>().sprite = tooltipItem.lingQiGrid[(int)jsonobject6.n];
						}
						num3++;
					}
					int num4 = 0;
					foreach (JSONObject jsonobject7 in jsonobject5["skill_SameCastNum"].list)
					{
						if (num3 > 0 || num4 > 0)
						{
							_this.CreatGameObjectToParent(tooltipItem.LingQiGride, tooltipItem.LingQifengexianImage);
						}
						for (int j = 0; j < (int)jsonobject7.n; j++)
						{
							_this.CreatGameObjectToParent(tooltipItem.LingQiGride, tooltipItem.lingqiGridImage).GetComponent<Image>().sprite = tooltipItem.lingQiGrid[tooltipItem.lingQiGrid.Count - 1];
						}
						num4++;
					}
					component.ShowSkillGride();
					Traverse.Create(_this).Field("showToolType").SetValue(3);
				}
				catch (Exception message)
				{
					TooltipItem component2 = _this.Tooltip.GetComponent<TooltipItem>();
					component2.Clear();
					component2.Label2.text = "[bfba7d]暂无说明[-]";
					Debug.LogWarning("3 Mod物品出错" + Item.itemID.ToString());
					Debug.LogWarning(message);
				}
				
			}
			//TODO 药材
			else if ((int)jsonobject["type"].n == 6)
			{
				try
				{
					KBEngine.Avatar player = Tools.instance.getPlayer();
					string liDanLeiXinStr = Tools.getLiDanLeiXinStr((int)jsonobject["yaoZhi2"].n);
					string liDanLeiXinStr2 = Tools.getLiDanLeiXinStr((int)jsonobject["yaoZhi3"].n);
					string liDanLeiXinStr3 = Tools.getLiDanLeiXinStr((int)jsonobject["yaoZhi1"].n);
					component.Label7.text = (player.GetHasZhuYaoShuXin(Item.itemID, jsonobject["quality"].I) ? liDanLeiXinStr : "未知");
					component.Label8.text = (player.GetHasFuYaoShuXin(Item.itemID, jsonobject["quality"].I) ? liDanLeiXinStr2 : "未知");
					component.Label9.text = (player.GetHasYaoYinShuXin(Item.itemID, jsonobject["quality"].I) ? liDanLeiXinStr3 : "未知");
					component.setCenterTextTitle("【主药】", "【辅药】", "【药引】");
					Traverse.Create(_this).Field("showToolType").SetValue(6);
				}
				catch (Exception message)
				{
					TooltipItem component2 = _this.Tooltip.GetComponent<TooltipItem>();
					component2.Clear();
					component2.Label2.text = "[bfba7d]暂无说明[-]";
					Debug.LogWarning("4 Mod物品出错" + Item.itemID.ToString());
					Debug.LogWarning(message);
				}
				
			}
			//TODO 丹炉和灵舟
			else if ((int)jsonobject["type"].n == 9 || (int)jsonobject["type"].n == 14)
			{
				try
				{
					if (!Item.Seid.HasField("NaiJiu"))
					{
						Item.Seid = Tools.CreateItemSeid(Item.itemID);
					}
					component.setCenterTextTitle("【耐久】", "", "");
					int num5 = (int)Item.Seid["NaiJiu"].n;
					int num6 = 100;
					if ((int)jsonobject["type"].n == 14)
					{
						num6 = (int)jsonData.instance.LingZhouPinJie[jsonobject["quality"].I.ToString()]["Naijiu"];
					}
					component.Label7.text = num5 + "/" + num6;
					Traverse.Create(_this).Field("showToolType").SetValue(9);
				}
				catch (Exception message)
				{
					TooltipItem component2 = _this.Tooltip.GetComponent<TooltipItem>();
					component2.Clear();
					component2.Label2.text = "[bfba7d]暂无说明[-]";
					Debug.LogWarning("5 Mod物品出错" + Item.itemID.ToString());
					Debug.LogWarning(message);
				}
			}
			//TODO 丹药
			else if ((int)jsonobject["type"].n == 5)
			{
				try
				{
					component.setCenterTextTitle("【耐药】", "【丹毒】", "");
					component.Label8.text = string.Concat((int)jsonobject["DanDu"].n);
					int jsonobject8 = Tools.getJsonobject(Tools.instance.getPlayer().NaiYaoXin, Item.itemID.ToString() ?? "");
					int itemCanUseNum = item.GetItemCanUseNum(jsonobject["id"].I);
					component.Label7.text = jsonobject8 + "/" + itemCanUseNum;
					component.ShowPlayerInfo();
					Traverse.Create(_this).Field("showToolType").SetValue(5);
				}
				catch (Exception message)
				{
					TooltipItem component2 = _this.Tooltip.GetComponent<TooltipItem>();
					component2.Clear();
					component2.Label2.text = "[bfba7d]暂无说明[-]";
					Debug.LogWarning("6 Mod物品出错" + Item.itemID.ToString());
					Debug.LogWarning(message);
				}
			}
			else
			{
				Traverse.Create(_this).Field("showToolType").SetValue(0);
			}
			Regex regex = new Regex("\\{STVar=\\d*\\}");
			MatchCollection matchCollection = Regex.Matches(text, "\\{STVar=\\d*\\}");
			foreach (var m in matchCollection)
			{
				int num7;
				if (int.TryParse(((Match)m).Value.Replace("{STVar=", "").Replace("}", ""), out num7))
				{
					int num8 = (int)Tools.instance.getPlayer().StaticValue.Value[num7];
					text = regex.Replace(text, num8.ToString());
				}
			}
			Regex.Matches(text, "【\\w*】");
			foreach (object obj3 in matchCollection)
			{
				Match match = (Match)obj3;
				text = text.Replace(match.Value, "[42E395]" + match.Value + "[-]");
			}
			component.Label1.text = "[e0ddb4]" + Inventory2.GetItemFirstDesc(Item.Seid, text);
			component.Label2.text = "[bfba7d]" + Inventory2.GetItemDesc(Item.Seid, Tools.instance.Code64ToString(jsonobject["desc2"].str));
			int num9 = Inventory2.GetItemQuality(Item, (int)jsonobject["quality"].n);
			List<string> tootipItemQualityColor = jsonData.instance.TootipItemQualityColor;
			string newValue = tootipItemQualityColor[num9 - 1] + Tools.getStr("shuzi" + num9) + Tools.getStr("jiecailiao");
			if ((int)jsonobject["type"].n == 0 || (int)jsonobject["type"].n == 1 || (int)jsonobject["type"].n == 2)
			{
				num9++;
				if (Item.Seid != null && Item.Seid.HasField("qualitydesc"))
				{
					newValue = tootipItemQualityColor[num9 - 1] + Item.Seid["qualitydesc"].str;
				}
				else
				{
					int num10 = (Item.Seid != null && Item.Seid.HasField("QPingZhi")) ? Item.Seid["QPingZhi"].I : ((int)jsonobject["typePinJie"].n);
					newValue = tootipItemQualityColor[num9 - 1] + Tools.getStr("EquipPingji" + (num9 - 1)) + ((num10 > 0) ? Tools.getStr("shangzhongxia" + num10) : "");
				}
			}
			else if ((int)jsonobject["type"].n == 3 || (int)jsonobject["type"].n == 4)
			{
				num9 *= 2;
				newValue = tootipItemQualityColor[num9 - 1] + Tools.getStr("pingjie" + (int)jsonobject["quality"].n) + Tools.getStr("shangzhongxia" + (int)jsonobject["typePinJie"].n);
			}
			else if ((int)jsonobject["type"].n == 5 || (int)jsonobject["type"].n == 9)
			{
				newValue = tootipItemQualityColor[num9 - 1] + Tools.getStr("shuzi" + num9) + Tools.getStr("pingdianyao");
			}
			else if ((int)jsonobject["type"].n == 6 || (int)jsonobject["type"].n == 7 || (int)jsonobject["type"].n == 8)
			{
				newValue = tootipItemQualityColor[num9 - 1] + Tools.getStr("shuzi" + num9) + Tools.getStr("jiecailiao");
				if ((int)jsonobject["type"].n == 8)
				{
					int i2 = jsonobject["WuWeiType"].I;
					string text5;
					if (i2 == 0)
					{
						text5 = "无";
					}
					else
					{
						text5 = Tools.Code64(jsonData.instance.LianQiWuWeiBiao[i2.ToString()]["desc"].str);
					}
					component.Label7.text = text5;
					int i3 = jsonobject["ShuXingType"].I;
					string text6;
					if (i3 == 0)
					{
						text6 = "无";
					}
					else
					{
						text6 = Tools.Code64(jsonData.instance.LianQiShuXinLeiBie[i3.ToString()]["desc"].str);
					}
					component.Label8.text = text6;
					component.setCenterTextTitle("【种类】", "【属性】", "");
				}
			}
			component.Label3.text = Tools.getStr("pingjieCell").Replace("{X}", newValue).Replace("[333333]品级：", "");
			component.Label4.text = ((jsonData.instance.TootipItemNameColor[num9 - 1] + Inventory2.GetItemName(Item, Tools.instance.Code64ToString(jsonobject["name"].str))) ?? "");
			component.Label5.text = Tools.getStr("ItemType" + (int)jsonobject["type"].n);
			if (money != 0)
			{
				int num11 = money;
				if (Item.Seid != null && Item.Seid.HasField("NaiJiu"))
				{
					num11 = (int)((float)num11 * ItemCellEX.getItemNaiJiuPrice(Item));
				}
				component.Label6.transform.parent.gameObject.SetActive(true);
				component.Label6.text = string.Concat(num11);
				if (moneyPercent > 0)
				{
					component.Label6.text = ((Tools.getStr("ItemColor" + 6) + num11) ?? "");
				}
				component.ShowMoney();
			}
			else
			{
				component.Label6.transform.parent.gameObject.SetActive(false);
			}
			component.icon.mainTexture = Item.itemIcon;
			component.pingZhi.mainTexture = Item.itemPingZhi;
			return false;
        }
    }
}
