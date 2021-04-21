using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using PCBS.FileExplorer;
using System.Collections.Generic;

namespace VideoWallpaper
{
    [BepInPlugin("me.xiaoye97.plugin.PCBS.VideoWallpaper", "VideoWallpaper", "1.0")]
    public class VideoWallpaper : BaseUnityPlugin
    {
        private void Start()
        {
            Harmony.CreateAndPatchAll(typeof(VideoWallpaper));
        }

        private void Update()
        {
            foreach (var os in playerDict.Keys)
            {
                if (os != null)
                {
                    // 如果为空，说明已经被删除，移除引用
                    if (displayDict[os] == null)
                    {
                        playerDict.Remove(os);
                        displayDict.Remove(os);
                        initDict.Remove(os);
                        break;
                    }
                    displayDict[os].texture = playerDict[os].texture;
                    if (!initDict[os])
                    {
                        var rt = playerDict[os].transform as RectTransform;
                        rt.sizeDelta = new Vector2(1280, 720);
                        rt.anchorMin = Vector2.zero;
                        rt.anchorMax = Vector2.one;
                        rt.offsetMin = Vector2.zero;
                        rt.offsetMax = Vector2.zero;
                        initDict[os] = true;
                    }
                }
            }
        }

        private static Dictionary<OS, VideoPlayer> playerDict = new Dictionary<OS, VideoPlayer>();
        private static Dictionary<OS, RawImage> displayDict = new Dictionary<OS, RawImage>();
        private static Dictionary<OS, bool> initDict = new Dictionary<OS, bool>();

        /// <summary>
        /// 设置视频壁纸
        /// </summary>
        public static void SetVideoWallpaper(OS os, string path)
        {
            VideoPlayer player = null;
            RawImage display = null;
            GameObject videoGO = null;
            if (os.m_desktop.transform.childCount > 0)
            {
                GameObject.Destroy(os.m_desktop.transform.GetChild(0).gameObject);
            }
            videoGO = new GameObject("Video");
            videoGO.transform.SetParent(os.m_desktop.transform);
            videoGO.transform.localPosition = Vector3.zero;
            videoGO.transform.localScale = Vector3.one;
            videoGO.transform.localEulerAngles = Vector3.zero;
            player = videoGO.AddComponent<VideoPlayer>();
            display = videoGO.AddComponent<RawImage>();
            player.enabled = true;
            player.isLooping = true;
            player.source = VideoSource.Url;
            player.url = $"file://{path}";
            player.audioOutputMode = VideoAudioOutputMode.Direct;
            player.SetDirectAudioMute(0, true);
            player.Play();
            player.aspectRatio = VideoAspectRatio.Stretch;
            display.texture = player.texture;
            if (playerDict.ContainsKey(os))
            {
                playerDict[os] = player;
            }
            else
            {
                playerDict.Add(os, player);
            }
            if (displayDict.ContainsKey(os))
            {
                displayDict[os] = display;
            }
            else
            {
                displayDict.Add(os, display);
            }
            if (initDict.ContainsKey(os))
            {
                initDict[os] = false;
            }
            else
            {
                initDict.Add(os, false);
            }
            os.m_computer.m_software.m_wallpaper = path;
        }

        /// <summary>
        /// 壁纸文件过滤修改
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SelectWallpaperApp), "Start")]
        public static void FileFilterPatch(SelectWallpaperApp __instance)
        {
            __instance.m_fileExplorer.m_fileFilters = new string[] { "*jpg", "*png", "*mp4" };
        }

        /// <summary>
        /// 选择文件后的处理
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SelectWallpaperApp), "OnShowFilePreview")]
        public static bool FileSelectPatch(SelectWallpaperApp __instance)
        {
            var _this = __instance;
            if (_this.m_fileExplorer)
            {
                string path = _this.m_fileExplorer.m_selectedFile.GetFullPath();
                if (path.EndsWith("jpg") || path.EndsWith("png"))
                {
                    // 是图片，交给游戏处理
                    return true;
                }
                _this.FreeTextureMemory();
                _this.m_customWallpaperPreview.sprite = _this.transform.parent.GetComponentInParent<OS>().m_defaultDesktop;
                if (!_this.m_customWallpaperPreview.gameObject.activeSelf)
                {
                    _this.m_customWallpaperPreview.gameObject.SetActive(true);
                }
                if (_this.m_currentFilePath)
                {
                    _this.m_currentFilePath.text = _this.m_fileExplorer.GetSelectedFileName();
                }
                _this.m_uploadToWorkshopButton.gameObject.SetActive(_this.IsSteamWorkshopActive);
            }
            return false;
        }

        /// <summary>
        /// 设置壁纸的修改
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(OS), "SetWallpaper")]
        public static bool SetWallpaperPatch(OS __instance, ref string wallpaper, bool preserveAspect)
        {
            if (__instance.GetWallPaperSprite(wallpaper).texture.width != 8)
            {
                // 是图片，销毁视频并交给游戏处理
                if (__instance.m_desktop.transform.childCount > 0)
                {
                    GameObject.Destroy(__instance.m_desktop.transform.GetChild(0).gameObject);
                }
                return true;
            }
            if (Tools.FileExists(wallpaper))
            {
                SetVideoWallpaper(__instance, wallpaper);
            }
            else
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 设置自定义壁纸的修改
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(OS), "SetCustomWallpaper")]
        public static bool SetCustomWallpaperPatch(OS __instance, string path)
        {
            if (path.EndsWith("jpg") || path.EndsWith("png") || string.IsNullOrEmpty(path))
            {
                // 是图片，交给游戏处理
                return true;
            }
            SetVideoWallpaper(__instance, path);
            return false;
        }
    }
}