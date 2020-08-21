using System;
using BepInEx;
using System.IO;
using cakeslice;
using hg.LitJson;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace me.xiaoye97.plugin.PCBS.CustomModel
{
    public static class SaveMgr
    {
        public static SaveData saveData = new SaveData();

        public static void Load()
        {
            Debug.Log($"自定义模型:读取存档Save_{SceneManager.GetActiveScene().name}.json ...");
            //读取存档
            if (File.Exists($"{Paths.GameRootPath}\\Models\\Save_{SceneManager.GetActiveScene().name}.json"))
            {
                string jsonstr = File.ReadAllText($"{Paths.GameRootPath}\\Models\\Save_{SceneManager.GetActiveScene().name}.json");
                //Debug.Log("读取到的json\n" + jsonstr);
                saveData = JsonMapper.ToObject<SaveData>(jsonstr);
            }

            //刷新机箱列表
            CustomModel.Ins.RefreshCaseList();
            //生成物体
            foreach (var data in saveData.modelDatas)
            {
                bool done = false;
                for(int i = 0; i < CustomModel.Ins.Prefabs.Count && !done; i++)
                {
                    if (data.modelName == CustomModel.Ins.Prefabs[i].name)
                    {
                        var go = GameObject.Instantiate(CustomModel.Ins.Prefabs[i]);
                        //添加outline
                        var renders = go.GetComponentsInChildren<Renderer>();
                        foreach (var r in renders)
                        {
                            var line = r.gameObject.AddComponent<CustomOutline>();
                            line.enabled = false;
                        }
                        go.name = CustomModel.Ins.Prefabs[i].name;
                        Debug.Log($"自定义模型:生成了{go.name} 预定的父物体:{data.computerName}");
                        foreach (var c in CustomModel.Ins.caseList)
                        {
                            if (data.computerName == c.gameObject.name)
                            {
                                Debug.Log($"将{go.name}绑定到机箱{data.computerName}");
                                go.transform.parent = c.transform;
                                go.transform.position = c.transform.position + data.pos.ToVector3();
                                go.transform.localEulerAngles = c.transform.localEulerAngles + data.rot.ToVector3();
                                go.transform.localScale = data.scl.ToVector3();
                                done = true;
                                break;
                            }
                        }
                        if(!done)
                        {
                            go.transform.localScale = data.scl.ToVector3();
                            go.transform.position = data.pos.ToVector3();
                            go.transform.localEulerAngles = data.rot.ToVector3();
                            done = true;
                        }
                        CustomModel.Ins.models.Add(go);
                    }
                }
            }
        }

        public static void Save()
        {
            if (!Directory.Exists($"{Paths.GameRootPath}\\Models"))
            {
                Directory.CreateDirectory($"{Paths.GameRootPath}\\Models");
            }
            saveData.modelDatas.Clear();
            foreach (var m in CustomModel.Ins.models)
            {
                saveData.modelDatas.Add(new ModelData(m));
            }
            string datastr = JsonMapper.ToJson(saveData);
            //Debug.Log("将要保存的数据:\n" + datastr);
            File.WriteAllText($"{Paths.GameRootPath}\\Models\\Save_{SceneManager.GetActiveScene().name}.json", datastr);
        }
    }

    [Serializable]
    public class SaveData
    {
        public List<ModelData> modelDatas = new List<ModelData>();
    }

    [Serializable]
    public class ModelData
    {
        public string modelName;
        public string computerName = "无";
        public V3 pos;
        public V3 rot;
        public V3 scl;

        public ModelData()
        {

        }

        public ModelData(GameObject gameObject)
        {
            modelName = gameObject.name;
            if (gameObject.transform.parent != null)
            {
                var c = gameObject.transform.parent.GetComponent<Case>();
                if (c != null) computerName = c.name;
                pos = new V3(gameObject.transform.position - gameObject.transform.parent.position);
                rot = new V3(gameObject.transform.localEulerAngles - gameObject.transform.parent.localEulerAngles);
            }
            else
            {
                pos = new V3(gameObject.transform.position);
                rot = new V3(gameObject.transform.localEulerAngles);
            }
            scl = new V3(gameObject.transform.localScale);
        }
    }

    [Serializable]
    public class V3
    {
        public float x;
        public float y;
        public float z;

        public V3()
        {

        }

        public V3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public V3(Vector3 v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }
}
