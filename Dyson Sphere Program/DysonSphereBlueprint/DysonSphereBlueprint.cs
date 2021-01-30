using System;
using BepInEx;
using System.IO;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace DysonSphereBlueprint
{
    [BepInPlugin("me.xiaoye97.plugin.Dyson.DysonSphereBlueprint", "DysonSphereBlueprint", "1.0")]
    public class DysonSphereBlueprint : BaseUnityPlugin
    {
        public static string BPDir;
        private bool show;
        public bool Show
        {
            get { return show; }
            set
            {
                show = value;
                if (show)
                {
                    RefreshBPFiles();
                }
            }
        }
        public static GUISkin DSPSkin;
        private UIDysonPanel dysonPanel;
        private Rect startButtonRect = new Rect(300, 0, 120, 36);
        private Rect winRect = new Rect(300, 36, 400, 500);

        public List<string> BPPathList = new List<string>();
        public List<string> BPFileNameList = new List<string>();

        void Start()
        {
            Init();
        }

        void Init()
        {
            BPDir = $"{Paths.GameRootPath}/DSBlueprints";
            var ab = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("DysonSphereBlueprint.skin"));
            DSPSkin = ab.LoadAsset<GUISkin>("DSPSkin");
        }

        void OnGUI()
        {
            if (GameMain.instance != null && GameMain.mainPlayer != null)
            {
                if (dysonPanel == null)
                {
                    dysonPanel = GameObject.FindObjectOfType<UIDysonPanel>();
                }
                if (dysonPanel != null && dysonPanel.active)
                {
                    GUI.skin = DSPSkin;
                    if (GUI.Button(startButtonRect, "蓝图"))
                    {
                        Show = !Show;
                    }
                    if (Show)
                    {
                        winRect = GUILayout.Window(666, winRect, WindowFunc, "");
                    }
                }
                else
                {
                    Show = false;
                }
            }
        }

        void RefreshBPFiles()
        {
            BPPathList.Clear();
            BPFileNameList.Clear();
            DirectoryInfo dir = new DirectoryInfo(BPDir);
            foreach (var file in dir.GetFiles("*.dsbp"))
            {
                BPPathList.Add(file.FullName);
                BPFileNameList.Add(file.Name);
            }
        }

        Vector2 sv;
        string SaveName = "NewBlueprint";
        void WindowFunc(int id)
        {
            GUILayout.BeginVertical();
            GUILayout.Label("文件名");
            SaveName = GUILayout.TextField(SaveName);
            if (GUILayout.Button("保存当前戴森球"))
            {
                if (!Directory.Exists(BPDir))
                {
                    Directory.CreateDirectory(BPDir);
                }
                SaveNowDysonSphere($"{BPDir}/{SaveName}.dsbp");
                RefreshBPFiles();
            }
            GUILayout.BeginVertical("载入戴森球蓝图", GUI.skin.box);
            sv = GUILayout.BeginScrollView(sv);
            for (int i = 0; i < BPFileNameList.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{BPFileNameList[i]}");
                if (GUILayout.Button("导入", GUILayout.Width(50)))
                {
                    LoadDysonSphereBP(BPPathList[i]);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndVertical();
        }

        void SaveNowDysonSphere(string path)
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
                                    dyson.ExportBP(binaryWriter);
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

        void LoadDysonSphereBP(string path)
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
                                    dyson.ImportBP(binaryReader);
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

    public static class DSPEx
    {
        #region DysonSphere
        public static void ExportBP(this DysonSphere _this, BinaryWriter w)
        {
            w.Write(2);
            w.Write(_this.randSeed);
            w.Write(_this.layerCount);
            w.Write(_this.layersIdBased.Length);
            for (int i = 1; i < _this.layersIdBased.Length; i++)
            {
                if (_this.layersIdBased[i] != null && _this.layersIdBased[i].id == i)
                {
                    w.Write(i);
                    _this.layersIdBased[i].ExportBP(w);
                }
                else
                {
                    w.Write(0);
                }
            }
        }

        public static void ImportBP(this DysonSphere _this, BinaryReader r)
        {
            int num = r.ReadInt32();
            if (num == 0)
            {
                _this.ResetNew();
                return;
            }
            _this.randSeed = r.ReadInt32();
            _this.layerCount = r.ReadInt32();
            int num3 = r.ReadInt32();
            for (int i = 1; i < num3; i++)
            {
                int num4 = r.ReadInt32();
                if (num4 != 0)
                {
                    Assert.True(i == num4);
                    if (i != num4)
                    {
                        throw new Exception("dyson layerId doesn't match! (1)");
                    }
                    DysonSphereLayer dysonSphereLayer = new DysonSphereLayer(_this);
                    dysonSphereLayer.Init();
                    if (i < _this.layersIdBased.Length)
                    {
                        _this.layersIdBased[i] = dysonSphereLayer;
                        _this.layersSorted[i - 1] = dysonSphereLayer;
                    }
                    dysonSphereLayer.ImportBP(r);
                    Assert.True(i == dysonSphereLayer.id);
                    if (i != dysonSphereLayer.id)
                    {
                        throw new Exception("dyson layerId doesn't match! (2)");
                    }
                }
            }
            _this.LayerSort();
            if (_this.autoNodeCount == 0)
            {
                _this.PickAutoNode();
                _this.PickAutoNode();
                _this.PickAutoNode();
                _this.PickAutoNode();
            }
        }
        #endregion

        #region DysonSphereLayer
        public static void ExportBP(this DysonSphereLayer _this, BinaryWriter w)
        {
            w.Write(0);
            w.Write(_this.id);
            w.Write(_this.orbitRadius);
            w.Write(_this.orbitRotation.x);
            w.Write(_this.orbitRotation.y);
            w.Write(_this.orbitRotation.z);
            w.Write(_this.orbitRotation.w);
            w.Write(_this.orbitAngularSpeed);
            w.Write(_this.currentAngle);
            w.Write(_this.currentRotation.x);
            w.Write(_this.currentRotation.y);
            w.Write(_this.currentRotation.z);
            w.Write(_this.currentRotation.w);
            w.Write(_this.nextRotation.x);
            w.Write(_this.nextRotation.y);
            w.Write(_this.nextRotation.z);
            w.Write(_this.nextRotation.w);
            w.Write(_this.gridMode);
            w.Write(_this.nodeCapacity);
            w.Write(_this.nodeCursor);
            w.Write(_this.nodeRecycleCursor);
            for (int i = 1; i < _this.nodeCursor; i++)
            {
                if (_this.nodePool[i] != null && _this.nodePool[i].id == i)
                {
                    w.Write(i);
                    _this.nodePool[i].ExportBP(w);
                }
                else
                {
                    w.Write(0);
                }
            }
            for (int j = 0; j < _this.nodeRecycleCursor; j++)
            {
                w.Write(_this.nodeRecycle[j]);
            }
            w.Write(_this.frameCapacity);
            w.Write(_this.frameCursor);
            w.Write(_this.frameRecycleCursor);
            for (int k = 1; k < _this.frameCursor; k++)
            {
                if (_this.framePool[k] != null && _this.framePool[k].id == k)
                {
                    w.Write(k);
                    _this.framePool[k].ExportBP(w);
                }
                else
                {
                    w.Write(0);
                }
            }
            for (int l = 0; l < _this.frameRecycleCursor; l++)
            {
                w.Write(_this.frameRecycle[l]);
            }
            w.Write(_this.shellCapacity);
            w.Write(_this.shellCursor);
            w.Write(_this.shellRecycleCursor);
            for (int m = 1; m < _this.shellCursor; m++)
            {
                if (_this.shellPool[m] != null && _this.shellPool[m].id == m)
                {
                    w.Write(m);
                    _this.shellPool[m].ExportBP(w);
                }
                else
                {
                    w.Write(0);
                }
            }
            for (int n = 0; n < _this.shellRecycleCursor; n++)
            {
                w.Write(_this.shellRecycle[n]);
            }
        }

        public static void ImportBP(this DysonSphereLayer _this, BinaryReader r)
        {
            r.ReadInt32();
            _this.id = r.ReadInt32();
            _this.orbitRadius = r.ReadSingle();
            _this.orbitRotation.x = r.ReadSingle();
            _this.orbitRotation.y = r.ReadSingle();
            _this.orbitRotation.z = r.ReadSingle();
            _this.orbitRotation.w = r.ReadSingle();
            _this.orbitAngularSpeed = r.ReadSingle();
            _this.currentAngle = r.ReadSingle();
            _this.currentRotation.x = r.ReadSingle();
            _this.currentRotation.y = r.ReadSingle();
            _this.currentRotation.z = r.ReadSingle();
            _this.currentRotation.w = r.ReadSingle();
            _this.nextRotation.x = r.ReadSingle();
            _this.nextRotation.y = r.ReadSingle();
            _this.nextRotation.z = r.ReadSingle();
            _this.nextRotation.w = r.ReadSingle();
            _this.gridMode = r.ReadInt32();
            _this.ResetNew();
            int num = r.ReadInt32();
            Traverse.Create(_this).Method("SetNodeCapacity", num).GetValue();
            _this.nodeCursor = r.ReadInt32();
            _this.nodeRecycleCursor = r.ReadInt32();
            for (int i = 1; i < _this.nodeCursor; i++)
            {
                int num2 = r.ReadInt32();
                if (num2 != 0)
                {
                    Assert.True(num2 == i);
                    DysonNode dysonNode = new DysonNode();
                    dysonNode.ImportBP(r);
                    Assert.True(dysonNode.id == i);
                    if (dysonNode.id != i || num2 != i)
                    {
                        throw new Exception($"node id doesn't match! {dysonNode.id} and {num2}");
                    }
                    _this.nodePool[i] = dysonNode;
                }
            }
            for (int j = 0; j < _this.nodeRecycleCursor; j++)
            {
                _this.nodeRecycle[j] = r.ReadInt32();
            }
            num = r.ReadInt32();
            Traverse.Create(_this).Method("SetFrameCapacity", num).GetValue();
            _this.frameCursor = r.ReadInt32();
            _this.frameRecycleCursor = r.ReadInt32();
            for (int k = 1; k < _this.frameCursor; k++)
            {
                int num3 = r.ReadInt32();
                if (num3 != 0)
                {
                    Assert.True(num3 == k);
                    DysonFrame dysonFrame = new DysonFrame();
                    dysonFrame.ImportBP(r, _this.dysonSphere);
                    Assert.True(dysonFrame.id == k);
                    if (dysonFrame.id != k || num3 != k)
                    {
                        throw new Exception("frame id doesn't match!");
                    }
                    _this.framePool[k] = dysonFrame;
                }
            }
            for (int l = 0; l < _this.frameRecycleCursor; l++)
            {
                _this.frameRecycle[l] = r.ReadInt32();
            }
            num = r.ReadInt32();
            Traverse.Create(_this).Method("SetShellCapacity", num).GetValue();
            _this.shellCursor = r.ReadInt32();
            _this.shellRecycleCursor = r.ReadInt32();
            for (int m = 1; m < _this.shellCursor; m++)
            {
                int num4 = r.ReadInt32();
                if (num4 != 0)
                {
                    Assert.True(num4 == m);
                    DysonShell dysonShell = new DysonShell(_this);
                    dysonShell.ImportBP(r, _this.dysonSphere);
                    Assert.True(dysonShell.id == m);
                    if (dysonShell.id != m || num4 != m)
                    {
                        throw new Exception("shell id doesn't match!");
                    }
                    _this.shellPool[m] = dysonShell;
                }
            }
            for (int n = 0; n < _this.shellRecycleCursor; n++)
            {
                _this.shellRecycle[n] = r.ReadInt32();
            }
            for (int num5 = 1; num5 < _this.nodeCursor; num5++)
            {
                if (_this.nodePool[num5] != null && _this.nodePool[num5].id == num5)
                {
                    _this.nodePool[num5].RecalcSpReq();
                    _this.nodePool[num5].RecalcCpReq();
                }
            }
        }
        #endregion

        #region DysonNode
        public static void ExportBP(this DysonNode _this, BinaryWriter w)
        {
            w.Write(_this.id);
            w.Write(_this.protoId);
            w.Write(_this.layerId);
            w.Write(_this.use);
            w.Write(_this.reserved);
            w.Write(_this.pos.x);
            w.Write(_this.pos.y);
            w.Write(_this.pos.z);
            w.Write(_this.spMax);
            w.Write(_this.rid);
        }

        public static void ImportBP(this DysonNode _this, BinaryReader r)
        {
            _this.SetEmpty();
            _this.id = r.ReadInt32();
            _this.protoId = r.ReadInt32();
            _this.layerId = r.ReadInt32();
            _this.use = r.ReadBoolean();
            _this.reserved = r.ReadBoolean();
            _this.pos.x = r.ReadSingle();
            _this.pos.y = r.ReadSingle();
            _this.pos.z = r.ReadSingle();
            _this.spMax = r.ReadInt32();
            _this.rid = r.ReadInt32();
            _this.RecalcSpReq();
            _this.RecalcCpReq();
        }
        #endregion

        #region DysonFrame
        public static void ExportBP(this DysonFrame _this, BinaryWriter w)
        {
            w.Write(0);
            w.Write(_this.id);
            w.Write(_this.protoId);
            w.Write(_this.layerId);
            w.Write(_this.reserved);
            w.Write(_this.nodeA.id);
            w.Write(_this.nodeB.id);
            w.Write(_this.euler);
            w.Write(_this.spMax);
        }

        public static void ImportBP(this DysonFrame _this, BinaryReader r, DysonSphere dysonSphere)
        {
            _this.SetEmpty();
            r.ReadInt32();
            _this.id = r.ReadInt32();
            _this.protoId = r.ReadInt32();
            _this.layerId = r.ReadInt32();
            _this.reserved = r.ReadBoolean();
            int nodeId = r.ReadInt32();
            int nodeId2 = r.ReadInt32();
            _this.euler = r.ReadBoolean();
            _this.spMax = r.ReadInt32();
            _this.nodeA = dysonSphere.FindNode(_this.layerId, nodeId);
            _this.nodeB = dysonSphere.FindNode(_this.layerId, nodeId2);
            Assert.NotNull(_this.nodeA);
            Assert.NotNull(_this.nodeB);
            if (_this.nodeA != null && !_this.nodeA.frames.Contains(_this))
            {
                _this.nodeA.frames.Add(_this);
            }
            if (_this.nodeB != null && !_this.nodeB.frames.Contains(_this))
            {
                _this.nodeB.frames.Add(_this);
            }
            _this.spMax = _this.segCount * 10;
            _this.GetSegments();
        }
        #endregion

        #region DysonShell
        public static void ExportBP(this DysonShell _this, BinaryWriter w)
        {
            w.Write(0);
            w.Write(_this.id);
            w.Write(_this.protoId);
            w.Write(_this.layerId);
            w.Write(_this.randSeed);
            w.Write(_this.polygon.Count);
            for (int i = 0; i < _this.polygon.Count; i++)
            {
                w.Write(_this.polygon[i].x);
                w.Write(_this.polygon[i].y);
                w.Write(_this.polygon[i].z);
            }
            w.Write(_this.nodes.Count);
            for (int j = 0; j < _this.nodes.Count; j++)
            {
                w.Write(_this.nodes[j].id);
            }
            w.Write(_this.vertexCount);
            w.Write(_this.triangleCount);
            int num = _this.verts.Length;
            w.Write(num);
            for (int k = 0; k < num; k++)
            {
                w.Write(_this.verts[k].x);
                w.Write(_this.verts[k].y);
                w.Write(_this.verts[k].z);
            }
            num = _this.pqArr.Length;
            w.Write(num);
            for (int l = 0; l < num; l++)
            {
                w.Write(_this.pqArr[l].x);
                w.Write(_this.pqArr[l].y);
            }
            num = _this.tris.Length;
            w.Write(num);
            for (int m = 0; m < num; m++)
            {
                w.Write(_this.tris[m]);
            }
            num = _this.vAdjs.Length;
            w.Write(num);
            for (int n = 0; n < num; n++)
            {
                w.Write(_this.vAdjs[n]);
            }
            num = _this.vertAttr.Length;
            w.Write(num);
            for (int num2 = 0; num2 < num; num2++)
            {
                w.Write(_this.vertAttr[num2]);
            }
            num = _this.vertsq.Length;
            w.Write(num);
            for (int num3 = 0; num3 < num; num3++)
            {
                w.Write(_this.vertsq[num3]);
            }
            num = _this.vertsqOffset.Length;
            w.Write(num);
            for (int num4 = 0; num4 < num; num4++)
            {
                w.Write(_this.vertsqOffset[num4]);
            }
            num = _this.nodecps.Length;
            w.Write(num);
            num = _this.vertcps.Length;
            w.Write(num);
            num = _this.vertRecycle.Length;
            w.Write(num);
            w.Write(_this.vertRecycleCursor);
            for (int num7 = 0; num7 < _this.vertRecycleCursor; num7++)
            {
                w.Write(_this.vertRecycle[num7]);
            }
        }

        public static void ImportBP(this DysonShell _this, BinaryReader r, DysonSphere dysonSphere)
        {
            _this.SetEmpty();
            r.ReadInt32();
            _this.id = r.ReadInt32();
            _this.protoId = r.ReadInt32();
            _this.layerId = r.ReadInt32();
            _this.randSeed = r.ReadInt32();
            int num = r.ReadInt32();
            for (int i = 0; i < num; i++)
            {
                Vector3 item = default(Vector3);
                item.x = r.ReadSingle();
                item.y = r.ReadSingle();
                item.z = r.ReadSingle();
                _this.polygon.Add(item);
            }
            int num2 = r.ReadInt32();
            for (int j = 0; j < num2; j++)
            {
                int num3 = r.ReadInt32();
                DysonNode dysonNode = dysonSphere.FindNode(_this.layerId, num3);
                Assert.NotNull(dysonNode);
                if (dysonNode != null)
                {
                    _this.nodeIndexMap[num3] = _this.nodes.Count;
                    _this.nodes.Add(dysonNode);
                    if (!dysonNode.shells.Contains(_this))
                    {
                        dysonNode.shells.Add(_this);
                    }
                }
            }
            Assert.True(_this.nodeIndexMap.Count == _this.nodes.Count);
            int count = _this.nodes.Count;
            for (int k = 0; k < count; k++)
            {
                int index = k;
                int index2 = (k + 1) % count;
                DysonFrame dysonFrame = DysonNode.FrameBetween(_this.nodes[index], _this.nodes[index2]);
                Assert.NotNull(dysonFrame);
                _this.frames.Add(dysonFrame);
            }
            _this.vertexCount = r.ReadInt32();
            _this.triangleCount = r.ReadInt32();
            int num4 = r.ReadInt32();
            _this.verts = new Vector3[num4];
            for (int l = 0; l < num4; l++)
            {
                _this.verts[l].x = r.ReadSingle();
                _this.verts[l].y = r.ReadSingle();
                _this.verts[l].z = r.ReadSingle();
            }
            num4 = r.ReadInt32();
            _this.pqArr = new IntVector2[num4];
            for (int m = 0; m < num4; m++)
            {
                _this.pqArr[m].x = r.ReadInt32();
                _this.pqArr[m].y = r.ReadInt32();
            }
            num4 = r.ReadInt32();
            _this.tris = new int[num4];
            for (int n = 0; n < num4; n++)
            {
                _this.tris[n] = r.ReadInt32();
            }
            num4 = r.ReadInt32();
            _this.vAdjs = new int[num4];
            for (int num5 = 0; num5 < num4; num5++)
            {
                _this.vAdjs[num5] = r.ReadInt32();
            }
            num4 = r.ReadInt32();
            _this.vertAttr = new int[num4];
            for (int num6 = 0; num6 < num4; num6++)
            {
                _this.vertAttr[num6] = r.ReadInt32();
            }
            Assert.True(_this.vertAttr.Length == _this.verts.Length);
            num4 = r.ReadInt32();
            _this.vertsq = new int[num4];
            for (int num7 = 0; num7 < num4; num7++)
            {
                _this.vertsq[num7] = r.ReadInt32();
            }
            Assert.True(_this.vertsq.Length == _this.verts.Length);
            num4 = r.ReadInt32();
            _this.vertsqOffset = new int[num4];
            for (int num8 = 0; num8 < num4; num8++)
            {
                _this.vertsqOffset[num8] = r.ReadInt32();
            }
            Assert.True(_this.vertsqOffset.Length == _this.nodes.Count + 1);
            num4 = r.ReadInt32();
            _this.nodecps = new int[num4];
            Assert.True(_this.nodecps.Length == _this.nodes.Count + 1);
            num4 = r.ReadInt32();
            _this.vertcps = new uint[num4];
            num4 = r.ReadInt32();
            _this.vertRecycleCursor = r.ReadInt32();
            _this.vertRecycle = new int[num4];
            for (int num11 = 0; num11 < _this.vertRecycleCursor; num11++)
            {
                _this.vertRecycle[num11] = r.ReadInt32();
            }
            Assert.True(_this.vertRecycle.Length == _this.verts.Length);
            _this.GenerateGeometryOnImport();
        }
        #endregion
    }
}
