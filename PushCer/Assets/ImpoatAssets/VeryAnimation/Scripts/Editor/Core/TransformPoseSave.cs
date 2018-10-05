using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.Animations;
using System;
using System.Collections.Generic;

namespace VeryAnimation
{
    public class TransformPoseSave
    {
        public GameObject rootObject;
        public Vector3 startPosition;
        public Quaternion startRotation;
        public Vector3 startScale;
        
        public class SaveData
        {
            public SaveData()
            {
            }
            public SaveData(Transform t)
            {
                Save(t);
            }
            public void Save(Transform t)
            {
                localPosition = t.localPosition;
                localRotation = t.localRotation;
                localScale = t.localScale;
                position = t.position;
                rotation = t.rotation;
                scale = t.lossyScale;
                syncTransform = null;
            }
            public void LoadLocal(Transform t)
            {
                t.localPosition = localPosition;
                t.localRotation = localRotation;
                t.localScale = localScale;
                Assert.IsTrue(t != syncTransform);
                if (syncTransform != null)
                {
                    syncTransform.localPosition = localPosition;
                    syncTransform.localRotation = localRotation;
                    syncTransform.localScale = localScale;
                }
            }
            public void LoadWorld(Transform t)
            {
                t.SetPositionAndRotation(position, rotation);
                Assert.IsTrue(t != syncTransform);
                if (syncTransform != null)
                {
                    syncTransform.SetPositionAndRotation(position, rotation);
                }
            }

            public Vector3 localPosition;
            public Quaternion localRotation;
            public Vector3 localScale;
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
            public Transform syncTransform;
        }
        private Dictionary<Transform, SaveData> originalTransforms;
        private Dictionary<Transform, SaveData> bindTransforms;
        private Dictionary<Transform, SaveData> prefabTransforms;

        public TransformPoseSave(GameObject gameObject)
        {
            rootObject = gameObject;
            startPosition = gameObject.transform.position;
            startRotation = gameObject.transform.rotation;
            startScale = gameObject.transform.lossyScale;
            #region originalTransforms
            {
                originalTransforms = new Dictionary<Transform, SaveData>();
                Action<Transform, Transform> SaveTransform = null;
                SaveTransform = (t, root) =>
                {
                    if (!originalTransforms.ContainsKey(t))
                    {
                        var saveTransform = new SaveData(t);
                        originalTransforms.Add(t, saveTransform);
                    }
                    for (int i = 0; i < t.childCount; i++)
                        SaveTransform(t.GetChild(i), root);
                };
                SaveTransform(gameObject.transform, gameObject.transform);
            }
            #endregion
            #region saveTransforms
            {
                var bindPathTransforms = new Dictionary<string, SaveData>();
                var prefabPathTransforms = new Dictionary<string, SaveData>();
                var defaultPathTransforms = new Dictionary<string, SaveData>();
                {
                    var uAvatarSetupTool = new UAvatarSetupTool();
                    Action<Dictionary<string, SaveData>, Transform, Transform, bool> SaveTransform = null;
                    SaveTransform = (transforms, t, root, scaleOverwrite) =>
                    {
                        var path = AnimationUtility.CalculateTransformPath(t, root);
                        if (!transforms.ContainsKey(path))
                        {
                            var saveTransform = new SaveData(t);
                            transforms.Add(path, saveTransform);
                        }
                        else if (scaleOverwrite)
                        {
                            transforms[path].localScale = t.localScale;
                            transforms[path].scale = t.lossyScale;
                        }
                        for (int i = 0; i < t.childCount; i++)
                            SaveTransform(transforms, t.GetChild(i), root, scaleOverwrite);
                    };
                    {
                        List<GameObject> goList = new List<GameObject>();
                        Action<GameObject> AddList = null;
                        AddList = (obj) =>
                        {
                            goList.Add(obj);
                            for (int i = 0; i < obj.transform.childCount; i++)
                            {
                                AddList(obj.transform.GetChild(i).gameObject);
                            }
                        };

                        var prefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject) as GameObject;
                        if (prefab != null)
                        {
                            var go = GameObject.Instantiate<GameObject>(prefab);
                            AnimatorUtility.DeoptimizeTransformHierarchy(go);
                            go.hideFlags |= HideFlags.HideAndDontSave;
                            AddList(go);
                            #region BindPose
                            if (go.GetComponentInChildren<SkinnedMeshRenderer>() != null)
                            {
                                var go2 = GameObject.Instantiate<GameObject>(go);
                                go2.hideFlags |= HideFlags.HideAndDontSave;
                                AddList(go2);
                                if (uAvatarSetupTool.SampleBindPose(go2))
                                {
                                    {  //Root
                                        go2.transform.localPosition = gameObject.transform.localPosition;
                                        go2.transform.localRotation = gameObject.transform.localRotation;
                                        go2.transform.localScale = gameObject.transform.localScale;
                                    }
                                    SaveTransform(defaultPathTransforms, go2.transform, go2.transform, false);
                                    SaveTransform(bindPathTransforms, go2.transform, go2.transform, false);
                                }
                                GameObject.DestroyImmediate(go2);
                            }
                            #endregion
                            #region PrefabPose
                            {  //Root
                                go.transform.localPosition = gameObject.transform.localPosition;
                                go.transform.localRotation = gameObject.transform.localRotation;
                                go.transform.localScale = gameObject.transform.localScale;
                            }
                            SaveTransform(defaultPathTransforms, go.transform, go.transform, true);
                            SaveTransform(prefabPathTransforms, go.transform, go.transform, false);
                            #endregion
                            GameObject.DestroyImmediate(go);
                        }
                        else
                        {
                            #region BindPose
                            if (gameObject.GetComponentInChildren<SkinnedMeshRenderer>() != null)
                            {
                                var go = GameObject.Instantiate<GameObject>(gameObject);
                                go.hideFlags |= HideFlags.HideAndDontSave;
                                AddList(go);
                                if (uAvatarSetupTool.SampleBindPose(go))
                                {
                                    {  //Root
                                        go.transform.localPosition = gameObject.transform.localPosition;
                                        go.transform.localRotation = gameObject.transform.localRotation;
                                        go.transform.localScale = gameObject.transform.localScale;
                                    }
                                    SaveTransform(defaultPathTransforms, go.transform, go.transform, false);
                                    SaveTransform(bindPathTransforms, go.transform, go.transform, false);
                                }
                                GameObject.DestroyImmediate(go);
                            }
                            #endregion
                        }
                        foreach (var go in goList)
                        {
                            if (go != null)
                                GameObject.DestroyImmediate(go);
                        }
                    }
                    //GameObjectPose
                    SaveTransform(defaultPathTransforms, gameObject.transform, gameObject.transform, false);
                }
                bindTransforms = Paths2Transforms(bindPathTransforms, gameObject.transform);
                prefabTransforms = Paths2Transforms(prefabPathTransforms, gameObject.transform);
            }
            #endregion
        }

        public void SetRootTransform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Action<Dictionary<Transform, SaveData>> setRootTransform = (list) =>
            {
                if (!list.ContainsKey(rootObject.transform))
                    return;
                var save = list[rootObject.transform];
                save.localPosition = save.position = position;
                save.localRotation = save.rotation = rotation;
                save.localScale = save.scale = scale;
            };
            setRootTransform(originalTransforms);
            setRootTransform(bindTransforms);
            setRootTransform(prefabTransforms);
        }

        public void ChangeTransforms(GameObject gameObject)
        {
            var paths = new List<string>(originalTransforms.Count);
            var transforms = new List<Transform>(originalTransforms.Count);
            foreach (var pair in originalTransforms)
            {
                paths.Add(AnimationUtility.CalculateTransformPath(pair.Key, rootObject.transform));
                transforms.Add(pair.Key);
            }

            Action<Transform, Transform> SaveTransform = null;
            SaveTransform = (t, root) =>
            {
                var path = AnimationUtility.CalculateTransformPath(t, root);
                var index = paths.IndexOf(path);
                if (index >= 0)
                {
                    Action<Dictionary<Transform, SaveData>, Transform, Transform> ChangeTransform = (list, oldT, newT) =>
                    {
                        if (list != null && list.Count > 0)
                        {
                            SaveData saveData;
                            if (list.TryGetValue(oldT, out saveData))
                            {
                                list.Remove(oldT);
                                list.Add(newT, saveData);
                            }
                        }
                    };
                    ChangeTransform(originalTransforms, transforms[index], t);
                    ChangeTransform(bindTransforms, transforms[index], t);
                    ChangeTransform(prefabTransforms, transforms[index], t);
                }
                for (int i = 0; i < t.childCount; i++)
                    SaveTransform(t.GetChild(i), root);
            };
            SaveTransform(gameObject.transform, gameObject.transform);
            rootObject = gameObject;
        }

        public void SetSyncTransforms(GameObject gameObject)
        {
            var paths = new List<string>(originalTransforms.Count);
            var transforms = new List<Transform>(originalTransforms.Count);
            foreach (var pair in originalTransforms)
            {
                paths.Add(AnimationUtility.CalculateTransformPath(pair.Key, rootObject.transform));
                transforms.Add(pair.Key);
            }

            Action<Transform, Transform> SaveTransform = null;
            SaveTransform = (t, root) =>
            {
                var path = AnimationUtility.CalculateTransformPath(t, root);
                var index = paths.IndexOf(path);
                if (index >= 0)
                {
                    Action<Dictionary<Transform, SaveData>, Transform, Transform> SetSyncTransform = (list, oldT, newT) =>
                    {
                        if (list != null && list.Count > 0)
                        {
                            if (list.ContainsKey(oldT))
                            {
                                list[oldT].syncTransform = newT;
                            }
                        }
                    };
                    SetSyncTransform(originalTransforms, transforms[index], t);
                    SetSyncTransform(bindTransforms, transforms[index], t);
                    SetSyncTransform(prefabTransforms, transforms[index], t);
                }
                for (int i = 0; i < t.childCount; i++)
                    SaveTransform(t.GetChild(i), root);
            };
            SaveTransform(gameObject.transform, gameObject.transform);
        }
        public void ResetSyncTransforms()
        {
            Action<Dictionary<Transform, SaveData>> ResetSyncTransform = (list) =>
            {
                foreach (var pair in list)
                {
                    pair.Value.syncTransform = null;
                }
            };
            ResetSyncTransform(originalTransforms);
            ResetSyncTransform(bindTransforms);
            ResetSyncTransform(prefabTransforms);
        }

        public void ResetRootStartTransform()
        {
            rootObject.transform.position = startPosition;
            rootObject.transform.rotation = startRotation;
        }

        public bool IsEnableOriginalTransform()
        {
            return (originalTransforms != null && originalTransforms.Count > 0);
        }
        public bool ResetOriginalTransform()
        {
            if (IsEnableOriginalTransform())
            {
                foreach (var trans in originalTransforms)
                {
                    if (trans.Key != null)
                        trans.Value.LoadLocal(trans.Key);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        public SaveData GetOriginalTransform(Transform t)
        {
            if (originalTransforms != null)
            {
                SaveData data;
                if (originalTransforms.TryGetValue(t, out data))
                {
                    return data;
                }
            }
            Assert.IsTrue(false);
            return null;
        }

        public bool IsEnableBindTransform()
        {
            return (bindTransforms != null && bindTransforms.Count > 0);
        }
        public bool ResetBindTransform()
        {
            if (IsEnableBindTransform())
            {
                foreach (var trans in bindTransforms)
                {
                    if (trans.Key != null)
                        trans.Value.LoadLocal(trans.Key);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        public SaveData GetBindTransform(Transform t)
        {
            if (bindTransforms != null)
            {
                SaveData data;
                if (bindTransforms.TryGetValue(t, out data))
                {
                    return data;
                }
            }
            Assert.IsTrue(false);
            return null;
        }

        public bool IsEnablePrefabTransform()
        {
            return (prefabTransforms != null && prefabTransforms.Count > 0);
        }
        public bool ResetPrefabTransform()
        {
            if (IsEnablePrefabTransform())
            {
                foreach (var trans in prefabTransforms)
                {
                    if (trans.Key != null)
                        trans.Value.LoadLocal(trans.Key);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        public SaveData GetPrefabTransform(Transform t)
        {
            if (prefabTransforms != null)
            {
                SaveData data;
                if (prefabTransforms.TryGetValue(t, out data))
                {
                    return data;
                }
            }
            Assert.IsTrue(false);
            return null;
        }

        private Dictionary<Transform, SaveData> Paths2Transforms(Dictionary<string, SaveData> src, Transform transform)
        {
            var dst = new Dictionary<Transform, SaveData>(src.Count);
            Action<Transform, Transform> SaveTransform = null;
            SaveTransform = (t, root) =>
            {
                var path = AnimationUtility.CalculateTransformPath(t, root);
                if (src.ContainsKey(path))
                    dst.Add(t, src[path]);
                for (int i = 0; i < t.childCount; i++)
                    SaveTransform(t.GetChild(i), root);
            };
            SaveTransform(transform, transform);
            return dst;
        }
    }
}
