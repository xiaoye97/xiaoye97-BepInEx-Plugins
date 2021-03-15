using System;
using BepInEx;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System.Collections.Generic;

namespace DysonSphereBlueprint
{
    [BepInPlugin("me.xiaoye97.plugin.Dyson.DysonSphereBlueprint", "DysonSphereBlueprint", "1.3")]
    public class DysonSphereBlueprint : BaseUnityPlugin
    {
        public static string BPDir;
        private UIDysonPanel dysonPanel;
        public List<string> BPPathList = new List<string>();
        public List<string> BPFileNameList = new List<string>();
        private GameObject UIPrefab, ItemPrefab;
        private Button openBtn, saveBtn;
        private InputField fileNameInput;
        private RectTransform dsyonEditorTop, BPUI, contentRT, BPPanel;
        private VerticalLayoutGroup VLG;
        private Vector3 targetPos = new Vector3(-480, 290, 0);

        private void Start()
        {
            Init();
            if (!Directory.Exists(BPDir))
            {
                Directory.CreateDirectory(BPDir);
            }
        }

        private void Init()
        {
            BPDir = $"{Paths.GameRootPath}/DSBlueprints";
            var ab = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("DysonSphereBlueprint.dspbpui"));
            UIPrefab = ab.LoadAsset<GameObject>("BPUI");
            ItemPrefab = ab.LoadAsset<GameObject>("ImportItem");
        }

        private void Update()
        {
            if (GameMain.instance != null && GameMain.mainPlayer != null)
            {
                if (dysonPanel == null)
                {
                    dysonPanel = GameObject.FindObjectOfType<UIDysonPanel>();
                }
                if (dysonPanel != null && dysonPanel.active)
                {
                    if (dsyonEditorTop == null)
                    {
                        dsyonEditorTop = UIRoot.instance.transform.Find("Always on Top/Overlay Canvas - Top/Dyson Editor Top") as RectTransform;
                    }
                    if (BPUI == null)
                    {
                        if (dsyonEditorTop != null)
                        {
                            BPUI = GameObject.Instantiate(UIPrefab, dsyonEditorTop).transform as RectTransform;
                            BPUI.anchorMin = new Vector2(0, 1);
                            BPUI.anchorMax = new Vector2(0, 1);
                            BPUI.localPosition = targetPos;
                            openBtn = BPUI.Find("BPOpenBtn").GetComponent<Button>();
                            openBtn.onClick.AddListener(OnOpenButtonClick);
                            BPPanel = BPUI.Find("BPPanel") as RectTransform;
                            saveBtn = BPPanel.Find("SaveBtn").GetComponent<Button>();
                            saveBtn.onClick.AddListener(OnSaveButtonClick);
                            fileNameInput = BPPanel.Find("InputField").GetComponent<InputField>();
                            contentRT = BPPanel.Find("Scroll View/Viewport/Content").transform as RectTransform;
                            VLG = contentRT.GetComponent<VerticalLayoutGroup>();
                            BPPanel.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        if (BPUI.localPosition != targetPos)
                        {
                            BPUI.localPosition = targetPos;
                        }
                    }
                }
            }
        }

        #region UI

        public void OnOpenButtonClick()
        {
            BPPanel.gameObject.SetActive(!BPPanel.gameObject.activeSelf);
            if (BPPanel.gameObject.activeInHierarchy)
            {
                RefreshBPFiles();
            }
        }

        public void OnSaveButtonClick()
        {
            if (!Directory.Exists(BPDir))
            {
                Directory.CreateDirectory(BPDir);
            }
            SaveNowDysonSphere($"{BPDir}/{fileNameInput.text}.dsbp");
            RefreshBPFiles();
        }

        public void RefreshFileUI()
        {
            List<Transform> childs = new List<Transform>();
            for (int i = 0; i < contentRT.childCount; i++)
            {
                childs.Add(contentRT.GetChild(i));
            }
            for (int i = childs.Count - 1; i >= 0; i--)
            {
                GameObject.Destroy(childs[i].gameObject);
            }
            float height = 0;
            for (int i = 0; i < BPFileNameList.Count; i++)
            {
                var item = GameObject.Instantiate(ItemPrefab, contentRT);
                height += (item.transform as RectTransform).sizeDelta.y;
                height += VLG.spacing;
                item.transform.Find("Text").GetComponent<Text>().text = $"{BPFileNameList[i]}";
                string path = BPPathList[i];
                item.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    LoadDysonSphereBP(path);
                });
            }
            contentRT.sizeDelta = new Vector2(contentRT.sizeDelta.x, height);
            contentRT.anchoredPosition = new Vector2(contentRT.anchoredPosition.x, 0);
        }

        #endregion UI

        private void RefreshBPFiles()
        {
            BPPathList.Clear();
            BPFileNameList.Clear();
            DirectoryInfo dir = new DirectoryInfo(BPDir);
            if (dir.Exists)
            {
                foreach (var file in dir.GetFiles("*.dsbp"))
                {
                    BPPathList.Add(file.FullName);
                    BPFileNameList.Add(file.Name);
                }
            }
            else
            {
                dir.Create();
            }
            RefreshFileUI();
        }

        private void SaveNowDysonSphere(string path)
        {
            if (GameMain.instance != null && GameMain.mainPlayer != null)
            {
                if (GameMain.localStar != null)
                {
                    var dyson = GameMain.data.dysonSpheres[GameMain.localStar.index];
                    if (dyson != null)
                    {
                        try
                        {
                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
                                {
                                    dyson.Export(binaryWriter);
                                    using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                                    {
                                        memoryStream.WriteTo(fileStream);
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.LogError(e.Message);
                        }
                    }
                    else
                    {
                        Logger.LogWarning("当前星系没有戴森球");
                    }
                }
                else
                {
                    Logger.LogWarning("当前未在星系中");
                }
            }
            else
            {
                Logger.LogWarning("当前未进入游戏");
            }
        }

        private void LoadDysonSphereBP(string path)
        {
            if (GameMain.instance != null && GameMain.mainPlayer != null)
            {
                if (GameMain.localStar != null)
                {
                    var dyson = GameMain.data.dysonSpheres[GameMain.localStar.index];
                    if (dyson != null)
                    {
                        try
                        {
                            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                            {
                                using (BinaryReader binaryReader = new BinaryReader(fileStream))
                                {
                                    dyson.Import(binaryReader);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.LogError(e.Message);
                        }
                    }
                    else
                    {
                        Logger.LogWarning("当前星系没有戴森球");
                    }
                }
                else
                {
                    Logger.LogWarning("当前未在星系中");
                }
            }
            else
            {
                Logger.LogWarning("当前未进入游戏");
            }
        }
    }
}