using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MCSDataHelper
{
    [BepInPlugin("me.xiaoye97.plugin.MiChangSheng.MCSDataHelper", "觅长生数据前置", "1.2")]
    public class DataHelper : BaseUnityPlugin
    {
        public static ConfigEntry<bool> DumpConfig;

        public static KBEngine.Avatar Player
        {
            get { return Tools.instance.getPlayer(); }
        }

        public static JSONObject ExSave = new JSONObject(JSONObject.Type.OBJECT);

        private static Dictionary<string, Texture2D> TexDict = new Dictionary<string, Texture2D>();
        private static Dictionary<string, Sprite> SpriteDict = new Dictionary<string, Sprite>();

        private void Awake()
        {
            DumpConfig = Config.Bind<bool>("config", "EnableDump", true, "是否开启数据转储");
            Harmony.CreateAndPatchAll(typeof(DataPatch));
        }

        /// <summary>
        /// 根据路径加载图片
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public static Texture2D GetTex(string path)
        {
            if (TexDict.ContainsKey(path)) return TexDict[path];
            if (File.Exists($"{BepInEx.Paths.GameRootPath}/{path}"))
            {
                FileStream fs = new FileStream($"{BepInEx.Paths.GameRootPath}/{path}", FileMode.Open, FileAccess.Read);
                byte[] thebytes = new byte[fs.Length];
                fs.Read(thebytes, 0, (int)fs.Length);
                Texture2D texture = new Texture2D(1, 1);
                texture.LoadImage(thebytes);
                TexDict.Add(path, texture);
                return texture;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 根据路径加载图片
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public static Sprite GetSprite(string path)
        {
            if (SpriteDict.ContainsKey(path)) return SpriteDict[path];
            var tex = GetTex(path);
            if (tex == null)
            {
                return null;
            }
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            SpriteDict.Add(path, sprite);
            return sprite;
        }

        /// <summary>
        /// 获取图标
        /// </summary>
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
            if (item.HasField("ModIcon"))
            {
                icon = DataHelper.GetTex(item["ModIcon"].str);
            }
            return icon;
        }

        #region 存档扩展

        public static void SetSaveData(string pluginId, string key, string data)
        {
            if (Player == null) return;
            if (!Player.TianFuID.HasField(pluginId)) Player.TianFuID.AddField(pluginId, new JSONObject(JSONObject.Type.OBJECT));
            if (Player.TianFuID[pluginId].HasField(key)) Player.TianFuID[pluginId].SetField(key, data);
            else Player.TianFuID[pluginId].AddField(key, data);
        }

        public static void SetSaveData(string pluginId, string key, int data)
        {
            if (Player == null) return;
            if (!Player.TianFuID.HasField(pluginId)) Player.TianFuID.AddField(pluginId, new JSONObject(JSONObject.Type.OBJECT));
            if (Player.TianFuID[pluginId].HasField(key)) Player.TianFuID[pluginId].SetField(key, data);
            else Player.TianFuID[pluginId].AddField(key, data);
        }

        public static void SetSaveData(string pluginId, string key, float data)
        {
            if (Player == null) return;
            if (!Player.TianFuID.HasField(pluginId)) Player.TianFuID.AddField(pluginId, new JSONObject(JSONObject.Type.OBJECT));
            if (Player.TianFuID[pluginId].HasField(key)) Player.TianFuID[pluginId].SetField(key, data);
            else Player.TianFuID[pluginId].AddField(key, data);
        }

        public static void SetSaveData(string pluginId, string key, bool data)
        {
            if (Player == null) return;
            if (!Player.TianFuID.HasField(pluginId)) Player.TianFuID.AddField(pluginId, new JSONObject(JSONObject.Type.OBJECT));
            if (Player.TianFuID[pluginId].HasField(key)) Player.TianFuID[pluginId].SetField(key, data);
            else Player.TianFuID[pluginId].AddField(key, data);
        }

        public static void SetSaveData(string pluginId, string key, JSONObject data)
        {
            if (Player == null) return;
            if (!Player.TianFuID.HasField(pluginId)) Player.TianFuID.AddField(pluginId, new JSONObject(JSONObject.Type.OBJECT));
            if (Player.TianFuID[pluginId].HasField(key)) Player.TianFuID[pluginId].SetField(key, data);
            else Player.TianFuID[pluginId].AddField(key, data);
        }

        public static JSONObject GetSaveData(string pluginId, string key)
        {
            if (Player == null) return null;
            if (Player.TianFuID.HasField(pluginId))
            {
                if (Player.TianFuID[pluginId].HasField(key)) return Player.TianFuID[pluginId][key];
            }
            return null;
        }

        #endregion 存档扩展
    }
}