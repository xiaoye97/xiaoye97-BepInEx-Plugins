using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace InGameWiki
{
    public static class ModTool
    {
        //品质颜色
        public static Color[] QualityColors = new Color[]
        {
            new Color(216f/255, 216f/255, 202f/255),
            new Color(179f/255, 217f/255, 81f/255),
            new Color(113f/255, 219f/255, 1),
            new Color(239f/255, 111f/255, 1),
            new Color(1, 157f/255, 67f/255),
            new Color(1, 116f/255, 77f/255)
        };

        //获取物品品质颜色 仅限物品JSON
        public static Color GetItemQualityColor(JSONObject item)
        {
            return QualityColors[item["quality"].I - 1];
        }

        //获取物品品质颜色 仅限物品JSON
        public static Color GetQualityColor(this JSONObject item)
        {
            return GetItemQualityColor(item);
        }

        //根据ID获取物品JSON 仅限物品ID
        public static JSONObject ItemJson(this int id)
        {
            if (id == 0) return null;
            return jsonData.instance.ItemJsonData[id.ToString()];
        }

        //获取图标
        public static Texture2D GetIcon(JSONObject item)
        {
            Texture2D icon;
            if ((int)item["ItemIcon"].n == 0)
            {
                icon = Resources.Load<Texture2D>("Item Icon/" + item["id"].ToString());
            }
            else
            {
                icon = Resources.Load<Texture2D>("Item Icon/" + (int)item["ItemIcon"].n);
            }
            if (icon == null)
            {
                icon = Resources.Load<Texture2D>("Item Icon/1");
            }
            return icon;
        }

        public static string UnCode64(this string code)
        {
            return Regex.Unescape(code);
        }
    }
}
