using BepInEx;
using cakeslice;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using BepInEx.Configuration;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace me.xiaoye97.plugin.PCBS.CustomModel
{
    [BepInPlugin("me.xiaoye97.plugin.PCBS.CustomModel", "自定义模型", "1.0")]
    public class CustomModel : BaseUnityPlugin
    {
        public static CustomModel Ins;
        ConfigEntry<KeyCode> hotkey_open;
        ConfigEntry<KeyCode> hotkey_move;
        bool isRayMove = false;
        Rect windowRect = new Rect(50, 50, 600, 600);
        bool windowShow = false;
        int windowId = 0;
        bool openOutline = true;

        static string[] moveStepStrings = new string[] { "0.01", "0.1", "1", "10", "30" };
        static float[] moveSteps = new float[] { 0.01f, 0.1f, 1f, 10f, 30f };
        int nowSeletedMoveStep = 0;
        Vector3 tmpv3;

        public List<GameObject> models = new List<GameObject>();
        public List<Case> caseList = new List<Case>();
        public List<Object> resRefs = new List<Object>();
        public GameObject sceneGizmo;
        bool openSceneGizmo = false;

        #region GUI参数
        GUILayoutOption labelWidth = GUILayout.Width(60);
        GUILayoutOption labelWidth2 = GUILayout.Width(100);
        #endregion

        void Start()
        {
            Ins = this;
            windowId = Random.Range(1000000000, 2000000000);
            hotkey_open = Config.Bind("设置", "打开界面热键", KeyCode.F10);
            hotkey_move = Config.Bind("设置", "启用射线移动热键", KeyCode.F9);
            LoadModels();
            SceneManager.sceneLoaded += (scene, mode) => Invoke("OnSceneLoaded", 1f);
        }

        void OnSceneLoaded()
        {
            SaveMgr.Load();
        }

        //模型预制体列表
        public List<GameObject> Prefabs = new List<GameObject>();

        #region 资源加载
        /// <summary>
        /// 加载模型
        /// </summary>
        void LoadModels()
        {
            Logger.LogInfo("开始加载模型包");
            DirectoryInfo dir = new DirectoryInfo($"{Paths.GameRootPath}\\Models");
            if (!dir.Exists)
            {
                Logger.LogInfo("资源文件夹不存在，新建文件夹.");
                dir.Create();
                return;
            }

            List<FileInfo> modelFiles = new List<FileInfo>();
            foreach (var f in dir.GetFiles("*.res")) modelFiles.Add(f);
            foreach (var f in dir.GetFiles("*.model")) modelFiles.Add(f);
            foreach (var d in dir.GetDirectories())
            {
                foreach (var f in dir.GetFiles("*.model")) modelFiles.Add(f);
            }
            foreach (var f in modelFiles)
            {
                StartCoroutine(LoadAsset(f.FullName));
            }
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        IEnumerator LoadAsset(string path)
        {
            AssetBundleCreateRequest abcr = AssetBundle.LoadFromFileAsync(path);
            while (!abcr.isDone)
            {
                yield return new WaitForSeconds(0.2f);
            }
            yield return abcr;
            var res = abcr.assetBundle.LoadAllAssets();
            foreach (var r in res)
            {
                resRefs.Add(r);
                if (r is GameObject)
                {
                    Logger.LogInfo($"加载了{r.name}");
                    if (r.name.Contains("[hide]"))
                    {
                        if (r.name.Contains("SceneGizmoObject"))
                        {
                            sceneGizmo = GameObject.Instantiate(r as GameObject, transform);
                            sceneGizmo.SetActive(false);
                        }
                    }
                    else
                    {
                        Prefabs.Add(r as GameObject);
                    }
                }
            }
        }
        #endregion

        void Update()
        {
            if (Input.GetKeyDown(hotkey_open.Value))
            {
                windowShow = !windowShow;
                RefreshCaseList();
                var uiraycasters = GameObject.FindObjectsOfType<GraphicRaycaster>();
                foreach (var ui in uiraycasters)
                {
                    ui.enabled = !windowShow;
                }
            }
            if (Input.GetKeyDown(hotkey_move.Value))
            {
                isRayMove = !isRayMove;
            }
            if (Input.GetMouseButtonUp(0))
            {
                if (isRayMove) RayMove();
            }
            if (sceneGizmo != null)
            {
                if (sceneGizmo.activeSelf && !openSceneGizmo)
                    sceneGizmo.SetActive(false);
                else if (!sceneGizmo.activeSelf && openSceneGizmo)
                    sceneGizmo.SetActive(true);
            }
        }

        void OnGUI()
        {
            //剔除空引用
            for (int i = models.Count - 1; i >= 0; i--) if (models[i] == null) models.RemoveAt(i);
            if (nowModel >= models.Count) nowModel = 0;

            if (windowShow)
            {
                GUI.backgroundColor = Color.black;
                windowRect = GUILayout.Window(windowId, windowRect, WindowFunc, "自定义模型", GUILayout.Width(400));
            }
            else CloseLine();
        }

        Vector2 modelCreateSV, SceneModelSV;
        int nowModel;
        CustomOutline[] nowLines;
        Vector3 pastePos, pasteRot, PasteScl;
        void WindowFunc(int id)
        {
            GUI.backgroundColor = Color.black;
            GUILayout.BeginVertical("设置", GUI.skin.window);
            #region 设置
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("读取存档"))
            {
                foreach (var m in models)
                {
                    GameObject.Destroy(m);
                }
                SaveMgr.Load();
            }
            if (GUILayout.Button("保存存档"))
            {
                SaveMgr.Save();
            }
            GUI.backgroundColor = Color.grey;
            isRayMove = GUILayout.Toggle(isRayMove, $"启用射线移动({hotkey_move.Value})");
            openOutline = GUILayout.Toggle(openOutline, "启用模型高亮");
            openSceneGizmo = GUILayout.Toggle(openSceneGizmo, "启用方向指示器");
            GUI.backgroundColor = Color.black;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("调节刻度", labelWidth);
            GUI.backgroundColor = Color.grey;
            nowSeletedMoveStep = GUILayout.SelectionGrid(nowSeletedMoveStep, moveStepStrings, moveStepStrings.Length, GUILayout.Width(moveStepStrings.Length * 40));
            GUI.backgroundColor = Color.black;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("粘贴板", labelWidth);
            if (models.Count > 0 && nowModel < models.Count)
            {
                if (GUILayout.Button("↑", GUILayout.Width(20)))
                {
                    pastePos = models[nowModel].transform.position;
                    pasteRot = models[nowModel].transform.localEulerAngles;
                    PasteScl = models[nowModel].transform.localScale;
                }
                if (GUILayout.Button("↓", GUILayout.Width(20)))
                {
                    models[nowModel].transform.position = pastePos;
                    models[nowModel].transform.localEulerAngles = pasteRot;
                    models[nowModel].transform.localScale = PasteScl;
                }
            }

            GUILayout.Label("位置:" + pastePos.ToString());
            GUILayout.Label("旋转:" + pasteRot.ToString());
            GUILayout.Label("缩放:" + PasteScl.ToString());
            GUILayout.EndHorizontal();
            #endregion
            GUILayout.EndVertical();
            GUILayout.Space(8);

            GUILayout.BeginHorizontal();
            #region 生成模型窗口
            GUILayout.BeginHorizontal("生成模型", GUI.skin.window, GUILayout.Width(300));
            modelCreateSV = GUILayout.BeginScrollView(modelCreateSV, GUILayout.Height(200));
            for (int i = 0; i < Prefabs.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label((i + 1).ToString(), GUILayout.Width(20));
                if (GUILayout.Button(Prefabs[i].name))
                {
                    GameObject go = GameObject.Instantiate(Prefabs[i]);
                    var renders = go.GetComponentsInChildren<Renderer>();
                    foreach (var r in renders)
                    {
                        var line = r.gameObject.AddComponent<CustomOutline>();
                        line.enabled = false;
                    }
                    go.name = Prefabs[i].name;
                    go.transform.localScale = new Vector3(0.167f, 0.167f, 0.167f);
                    models.Add(go);
                    RefreshCaseList();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();
            #endregion
            GUILayout.Space(8);
            #region 场景内模型窗口
            GUILayout.BeginHorizontal("场景内模型", GUI.skin.window, GUILayout.Width(300));
            SceneModelSV = GUILayout.BeginScrollView(SceneModelSV, GUILayout.Height(200));
            for (int i = 0; i < models.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label((i + 1).ToString(), GUILayout.Width(20));
                if (nowModel == i)
                    GUI.color = Color.green;
                if (GUILayout.Button(models[i].name))
                {
                    CloseLine();
                    nowModel = i;
                }
                GUI.color = Color.white;
                //销毁物体
                if (GUILayout.Button("x", GUILayout.Width(20)))
                {
                    GameObject go = models[i];
                    models.Remove(go);
                    Destroy(go);
                    GUILayout.EndHorizontal();
                    if (nowModel == i && nowModel > 0)
                        nowModel--;
                    break;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();
            #endregion
            GUILayout.EndHorizontal();
            GUILayout.Space(8);

            ContrlGUI();
            GUI.DragWindow();
        }

        void ContrlGUI()
        {
            if (models.Count > 0)
            {
                CloseLine();
                ShowLine();

                GUILayout.BeginHorizontal();
                //机箱列表
                GUILayout.BeginVertical("机箱列表", GUI.skin.window);
                for (int i = 0; i < caseList.Count; i++)
                {
                    GUILayout.Label(caseList[i].gameObject.name);
                    if (GUILayout.Button("挂载"))
                    {
                        models[nowModel].transform.parent = caseList[i].gameObject.transform;
                        models[nowModel].transform.position = models[nowModel].transform.parent.position;
                        models[nowModel].transform.localEulerAngles = models[nowModel].transform.parent.localEulerAngles;
                    }
                }
                GUILayout.EndVertical();
                GUILayout.Space(8);
                //模型控制
                GUILayout.BeginVertical("模型控制", GUI.skin.window);
                ModelGUI();
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

        void ModelGUI()
        {
            if (models[nowModel].transform.parent != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("当前父物体:" + models[nowModel].transform.parent.name);
                if (GUILayout.Button("取消挂载"))
                    models[nowModel].transform.parent = null;
                GUILayout.EndHorizontal();
            }
            else
                GUILayout.Label("当前父物体:无");

            #region Transform
            GUILayout.BeginHorizontal();
            tmpv3 = models[nowModel].transform.position;
            GUILayout.Label("绝对位置", labelWidth);
            V3GUI();
            models[nowModel].transform.position = tmpv3;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            tmpv3 = models[nowModel].transform.localPosition;
            GUILayout.Label("相对位置", labelWidth);
            V3GUI();
            models[nowModel].transform.localPosition = tmpv3;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            tmpv3 = models[nowModel].transform.localEulerAngles;
            GUILayout.Label("旋转", labelWidth);
            V3GUI();
            models[nowModel].transform.localEulerAngles = tmpv3;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            tmpv3 = models[nowModel].transform.localScale;
            GUILayout.Label("缩放", labelWidth);
            V3GUI();
            models[nowModel].transform.localScale = tmpv3;
            GUILayout.EndHorizontal();
            #endregion
        }

        void V3GUI()
        {
            GUILayout.BeginHorizontal("", GUI.skin.box, labelWidth2);
            GUILayout.Label("x", GUILayout.Width(12));
            if (GUILayout.Button("-", GUILayout.Width(20))) tmpv3.x -= moveSteps[nowSeletedMoveStep];
            GUILayout.Label(tmpv3.x.ToString("f2"), GUILayout.Width(50));
            if (GUILayout.Button("+", GUILayout.Width(20))) tmpv3.x += moveSteps[nowSeletedMoveStep];
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("", GUI.skin.box, labelWidth2);
            GUILayout.Label("y", GUILayout.Width(12));
            if (GUILayout.Button("-", GUILayout.Width(20))) tmpv3.y -= moveSteps[nowSeletedMoveStep];
            GUILayout.Label(tmpv3.y.ToString("f2"), GUILayout.Width(50));
            if (GUILayout.Button("+", GUILayout.Width(20))) tmpv3.y += moveSteps[nowSeletedMoveStep];
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("", GUI.skin.box, labelWidth2);
            GUILayout.Label("z", GUILayout.Width(12));
            if (GUILayout.Button("-", GUILayout.Width(20))) tmpv3.z -= moveSteps[nowSeletedMoveStep];
            GUILayout.Label(tmpv3.z.ToString("f2"), GUILayout.Width(50));
            if (GUILayout.Button("+", GUILayout.Width(20))) tmpv3.z += moveSteps[nowSeletedMoveStep];
            GUILayout.EndHorizontal();
        }

        #region 功能函数
        public void RefreshCaseList()
        {
            caseList.Clear();
            var cases = GameObject.FindObjectsOfType<Case>();
            foreach (var c in cases)
            {
                caseList.Add(c);
            }
        }

        void RayMove()
        {
            if (models.Count > 0 && nowModel < models.Count)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    models[nowModel].transform.position = hit.point;
                }
            }
        }

        void CloseLine()
        {
            if (nowLines != null && nowLines.Length > 0)
            {
                foreach (var line in nowLines)
                {
                    if (line != null) line.enabled = false;
                }
            }
        }

        void ShowLine()
        {
            if (models.Count <= 0 || nowModel >= models.Count) return;
            nowLines = models[nowModel].GetComponentsInChildren<CustomOutline>();
            foreach (var line in nowLines)
            {
                if (line != null) line.enabled = openOutline;
            }
        }
        #endregion
    }
}
