using System.Text.RegularExpressions;
using UnityEngine;

namespace MCSDataHelper
{
    public static class DataEx
    {
        /// <summary>
        /// 根据ID获取物品JSON 仅限物品ID
        /// </summary>
        public static JSONObject ItemJson(this int id)
        {
            if (id == 0) return null;
            if (jsonData.instance.ItemJsonData.ContainsKey(id.ToString()))
            {
                return jsonData.instance.ItemJsonData[id.ToString()];
            }
            return null;
        }

        /// <summary>
        /// 转换编码，将字符串中的中文Unicode转换为中文
        /// </summary>
        public static string UnCode64(this string code)
        {
            return Regex.Unescape(code);
        }

        /// <summary>
        /// 转换编码，将字符串中的中文部分转换为Unicode
        /// </summary>
        public static string ToUnicode(this string str)
        {
            str = Regex.Replace(str, @"([\u4e00-\u9fa5])", (m) =>
            {
                foreach (var c in m.Groups)
                {
                    return string.Format("\\u{0:x4}", (int)c.ToString()[0]);
                }
                return "";
            });
            return str;
        }

        /// <summary>
        /// 品质颜色
        /// </summary>
        public static Color[] QualityColors = new Color[]
        {
            new Color(216f/255, 216f/255, 202f/255),
            new Color(179f/255, 217f/255, 81f/255),
            new Color(113f/255, 219f/255, 1),
            new Color(239f/255, 111f/255, 1),
            new Color(1, 157f/255, 67f/255),
            new Color(1, 116f/255, 77f/255)
        };

        /// <summary>
        /// 获取物品品质颜色 仅限物品JSON
        /// </summary>
        public static Color GetItemQualityColor(JSONObject item)
        {
            return QualityColors[item["quality"].I - 1];
        }

        /// <summary>
        /// 获取物品品质颜色 仅限物品JSON
        /// </summary>
        public static Color GetQualityColor(this JSONObject item)
        {
            return GetItemQualityColor(item);
        }

        /// <summary>
        /// 获取物品品质颜色
        /// </summary>
        public static Color GetQualityColor(this JSONClass._ItemJsonData item)
        {
            return QualityColors[item.quality - 1];
        }
    }
}