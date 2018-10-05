//#define Enable_Profiler

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEditor;

namespace VeryAnimation
{
    [Serializable]
    public class VeryAnimationEditorWindow : EditorWindow
    {
        public static VeryAnimationEditorWindow instance;

        private VeryAnimationWindow vaw { get { return VeryAnimationWindow.instance; } }
        private VeryAnimation va { get { return VeryAnimation.instance; } }

        #region GUI
        private bool editorPoseFoldout = true;
        private bool editorBlendPoseFoldout = true;
        private bool editorMuscleFoldout = true;
        private bool editorBlendShapeFoldout = false;
        private bool editorSelectionFoldout = true;

        private bool editorPoseHelp;
        private bool editorBlendPoseGroupHelp;
        private bool editorMuscleGroupHelp;
        private bool editorBlendShapeGroupHelp;
        private bool editorSelectionHelp;
        #endregion

        #region Strings
        private GUIContent[] RootCorrectionModeString = new GUIContent[(int)VeryAnimation.RootCorrectionMode.Total];
        #endregion

        #region Core
        [SerializeField]
        private BlendPoseTree blendPoseTree;
        [SerializeField]
        private MuscleGroupTree muscleGroupTree;
        [SerializeField]
        private BlendShapeTree blendShapeTree;
        #endregion

        private Vector2 editorScrollPosition;

        private const int QuickSaveSize = 3;
        private PoseTemplate[] quickSaves;

        private string poseSaveDefaultDirectory;

        void OnEnable()
        {
            if (vaw == null || va == null) return;

            instance = this;

            blendPoseTree = new BlendPoseTree();
            muscleGroupTree = new MuscleGroupTree();
            blendShapeTree = new BlendShapeTree();

            #region EditorPref
            {
                editorPoseFoldout = EditorPrefs.GetBool("VeryAnimation_Editor_Pose", true);
                editorBlendPoseFoldout = EditorPrefs.GetBool("VeryAnimation_Editor_BlendPose", false);
                editorMuscleFoldout = EditorPrefs.GetBool("VeryAnimation_Editor_Muscle", true);
                editorBlendShapeFoldout = EditorPrefs.GetBool("VeryAnimation_Editor_BlendShape", false);
                editorSelectionFoldout = EditorPrefs.GetBool("VeryAnimation_Editor_Selection", true);

                va.clampMuscle = EditorPrefs.GetBool("VeryAnimation_ClampMuscle", false);
                va.autoFootIK = EditorPrefs.GetBool("VeryAnimation_AutoFootIK", false);
                va.mirrorEnable = EditorPrefs.GetBool("VeryAnimation_MirrorEnable", false);
                va.rootCorrectionMode = (VeryAnimation.RootCorrectionMode)EditorPrefs.GetInt("VeryAnimation_RootCorrectionMode", (int)VeryAnimation.RootCorrectionMode.Single);
                muscleGroupTree.muscleGroupMode = (MuscleGroupTree.MuscleGroupMode)EditorPrefs.GetInt("VeryAnimation_MuscleGroupMode", 0);
            }
            #endregion

            poseSaveDefaultDirectory = Application.dataPath;

            UpdateRootCorrectionModeString();
            Language.OnLanguageChanged += UpdateRootCorrectionModeString;

            titleContent = new GUIContent("VA Editor");
        }
        void OnDisable()
        {
            if (vaw == null || va == null) return;
            
            #region EditorPref
            {
                EditorPrefs.SetBool("VeryAnimation_ClampMuscle", va.clampMuscle);
                EditorPrefs.SetBool("VeryAnimation_AutoFootIK", va.autoFootIK);
                EditorPrefs.SetBool("VeryAnimation_MirrorEnable", va.mirrorEnable);
                EditorPrefs.SetInt("VeryAnimation_RootCorrectionMode", (int)va.rootCorrectionMode);
                EditorPrefs.SetInt("VeryAnimation_MuscleGroupMode", (int)muscleGroupTree.muscleGroupMode);
            }
            #endregion

            instance = null;

            if (vaw != null)
            {
                vaw.Release();
            }
        }
        void OnDestroy()
        {
            if (vaw != null)
            {
                vaw.Release();
            }
        }

        void OnInspectorUpdate()
        {
            if (vaw == null || va == null || va.isEditError)
            {
                Close();
                return;
            }
        }

        void OnGUI()
        {
            if (va == null || !va.edit || va.isError || !vaw.guiStyleReady)
                return;

#if Enable_Profiler
            Profiler.BeginSample("****VeryAnimationEditorWindow.OnGUI");
#endif
            Event e = Event.current;

            #region Event
            switch (e.type)
            {
            case EventType.KeyDown:
                if (focusedWindow == this)
                    va.HotKeys();
                break;
            case EventType.MouseUp:
                SceneView.RepaintAll();
                break;
            }
            va.Commands();
            #endregion

            #region ToolBar
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                {
                    EditorGUI.BeginChangeCheck();
                    editorPoseFoldout = GUILayout.Toggle(editorPoseFoldout, "Pose", EditorStyles.toolbarButton);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetBool("VeryAnimation_Editor_Pose", editorPoseFoldout);
                    }
                }
                {
                    EditorGUI.BeginChangeCheck();
                    editorBlendPoseFoldout = GUILayout.Toggle(editorBlendPoseFoldout, "Blend Pose", EditorStyles.toolbarButton);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetBool("VeryAnimation_Editor_BlendPose", editorBlendPoseFoldout);
                    }
                }
                if (va.isHuman)
                {
                    EditorGUI.BeginChangeCheck();
                    editorMuscleFoldout = GUILayout.Toggle(editorMuscleFoldout, "Muscle Group", EditorStyles.toolbarButton);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetBool("VeryAnimation_Editor_Muscle", editorMuscleFoldout);
                    }
                }
                if (blendShapeTree.IsHaveBlendShapeNodes())
                {
                    EditorGUI.BeginChangeCheck();
                    editorBlendShapeFoldout = GUILayout.Toggle(editorBlendShapeFoldout, "Blend Shape", EditorStyles.toolbarButton);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetBool("VeryAnimation_Editor_BlendShape", editorBlendShapeFoldout);
                    }
                }
                {
                    EditorGUI.BeginChangeCheck();
                    editorSelectionFoldout = GUILayout.Toggle(editorSelectionFoldout, "Selection", EditorStyles.toolbarButton);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetBool("VeryAnimation_Editor_Selection", editorSelectionFoldout);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            #endregion

            if (va.isHuman)
                HumanoidEditorGUI();
            else
                GenericEditorGUI();

#if Enable_Profiler
            Profiler.EndSample();
#endif
        }

        private void HumanoidEditorGUI()
        {
            Event e = Event.current;
            #region Tools
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Options", EditorStyles.miniLabel, GUILayout.Width(48f));
                    {
                        EditorGUI.BeginChangeCheck();
                        var flag = GUILayout.Toggle(va.clampMuscle, Language.GetContent(Language.Help.EditorOptionsClamp), va.clampMuscle ? vaw.guiStyleActiveMiniButton : EditorStyles.miniButton);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(this, "Change Clamp");
                            va.clampMuscle = flag;
                            va.SetUpdateSelectionIKtarget();
                        }
                    }
                    {
#if UNITY_2017_1_OR_NEWER
                        if (va.uAw_2017_1.GetLinkedWithTimeline())
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            GUILayout.Toggle(true, Language.GetContent(Language.Help.EditorOptionsFootIK), vaw.guiStyleActiveMiniButton);
                            EditorGUI.EndDisabledGroup();
                        }
                        else
#endif
                        {
                            EditorGUI.BeginChangeCheck();
                            var flag = GUILayout.Toggle(va.autoFootIK, Language.GetContent(Language.Help.EditorOptionsFootIK), va.autoFootIK ? vaw.guiStyleActiveMiniButton : EditorStyles.miniButton);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(this, "Change Foot IK");
                                va.autoFootIK = flag;
                            }
                        }
                    }
                    {
                        EditorGUI.BeginChangeCheck();
                        var flag = GUILayout.Toggle(va.mirrorEnable, Language.GetContent(Language.Help.EditorOptionsMirror), va.mirrorEnable ? vaw.guiStyleActiveMiniButton : EditorStyles.miniButton);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(this, "Change Mirror");
                            va.mirrorEnable = flag;
                            va.SetUpdateResampleAnimation();
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(Language.GetContent(Language.Help.EditorRootCorrection), EditorStyles.miniLabel, GUILayout.Width(88f));
                    {
                        EditorGUI.BeginChangeCheck();
                        var mode = (VeryAnimation.RootCorrectionMode)GUILayout.Toolbar((int)va.rootCorrectionMode, RootCorrectionModeString, EditorStyles.miniButton);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(this, "Change Root Correction Mode");
                            va.rootCorrectionMode = mode;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            #endregion

            editorScrollPosition = EditorGUILayout.BeginScrollView(editorScrollPosition);

            EditorGUI_PoseGUI();

            EditorGUI_BlendPoseGUI();

            EditorGUI_MuscleGroupGUI();

            EditorGUI_BlendShapeGUI();

            EditorGUI_SelectionGUI();

            EditorGUILayout.EndScrollView();
        }
        private void GenericEditorGUI()
        {
            Event e = Event.current;
            #region Tools
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Options", GUILayout.Width(52f));
                    {
                        EditorGUI.BeginChangeCheck();
                        var flag = GUILayout.Toggle(va.mirrorEnable, Language.GetContent(Language.Help.EditorOptionsMirror), va.mirrorEnable ? vaw.guiStyleActiveButton : GUI.skin.button);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(this, "Change Mirror");
                            va.mirrorEnable = flag;
                            va.SetUpdateResampleAnimation();
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            #endregion

            editorScrollPosition = EditorGUILayout.BeginScrollView(editorScrollPosition);

            EditorGUI_PoseGUI();

            EditorGUI_BlendPoseGUI();

            EditorGUI_BlendShapeGUI();

            EditorGUI_SelectionGUI();

            EditorGUILayout.EndScrollView();
        }
        private void EditorGUI_PoseGUI()
        {
            {
                if (editorPoseFoldout)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUI.BeginChangeCheck();
                        editorPoseFoldout = EditorGUILayout.Foldout(editorPoseFoldout, "Pose", true, vaw.guiStyleBoldFoldout);
                        if (EditorGUI.EndChangeCheck())
                        {
                            EditorPrefs.SetBool("VeryAnimation_Editor_Pose", editorPoseFoldout);
                        }
                    }
                    EditorGUILayout.Space();
                    if (GUILayout.Button("?", editorPoseHelp ? vaw.guiStyleActiveButton : GUI.skin.button, GUILayout.Width(16), GUILayout.Height(14)))
                    {
                        editorPoseHelp = !editorPoseHelp;
                    }
                    EditorGUILayout.EndHorizontal();

                    if (editorPoseHelp)
                    {
                        EditorGUILayout.HelpBox(Language.GetText(Language.Help.HelpPose), MessageType.Info);
                    }

                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    {
                        EditorGUILayout.BeginHorizontal();
                        #region Reset
                        if (va.isHuman)
                        {
                            if (GUILayout.Button(Language.GetContent(Language.Help.EditorPoseHumanoidReset)))
                            {
                                Undo.RecordObject(this, "Reset Pose");
                                va.SetPoseHumanoidDefault();
                            }
                        }
                        #endregion
                        #region Bind or Start
                        {
                            if (va.transformPoseSave.IsEnableBindTransform())
                            {
                                if (GUILayout.Button(Language.GetContent(Language.Help.EditorPoseBind)))
                                {
                                    Undo.RecordObject(this, "Bind Pose");
                                    va.SetPoseBind();
                                }
                            }
                            else
                            {
                                if (GUILayout.Button(Language.GetContent(Language.Help.EditorPoseStart)))
                                {
                                    Undo.RecordObject(this, "Edit Start Pose");
                                    va.SetPoseEditStart();
                                }
                            }
                        }
                        #endregion
                        #region Prefab
                        {
                            if (va.transformPoseSave.IsEnablePrefabTransform())
                            {
                                if (GUILayout.Button(Language.GetContent(Language.Help.EditorPosePrefab)))
                                {
                                    Undo.RecordObject(this, "Prefab Pose");
                                    va.SetPosePrefab();
                                }
                            }
                        }
                        #endregion
                        #region Mirror
                        if (GUILayout.Button(Language.GetContent(Language.Help.EditorPoseMirror)))
                        {
                            Undo.RecordObject(this, "Mirror Pose");
                            va.SetPoseMirror();
                        }
                        #endregion
                        #region Template
                        if (GUILayout.Button(Language.GetContent(Language.Help.EditorPoseTemplate), vaw.guiStyleDropDown))
                        {
                            Dictionary<string, string> poseTemplates = new Dictionary<string, string>();
                            {
                                var guids = AssetDatabase.FindAssets("t:posetemplate");
                                for (int i = 0; i < guids.Length; i++)
                                {
                                    var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                                    var name = path.Remove(0, "Assets/".Length);
                                    poseTemplates.Add(name, path);
                                }
                            }
                            
                            GenericMenu menu = new GenericMenu();
                            {
                                var enu = poseTemplates.GetEnumerator();
                                while (enu.MoveNext())
                                {
                                    var value = enu.Current.Value;
                                    menu.AddItem(new GUIContent(enu.Current.Key), false, () =>
                                    {
                                        var poseTemplate = AssetDatabase.LoadAssetAtPath<PoseTemplate>(value);
                                        if (poseTemplate != null)
                                        {
                                            Undo.RecordObject(this, "Template Pose");
                                            Undo.RegisterCompleteObjectUndo(va.currentClip, "Template Pose");
                                            va.LoadPoseTemplate(poseTemplate, true);
                                        }
                                        else
                                        {
                                            Debug.LogErrorFormat(Language.GetText(Language.Help.LogFailedLoadPoseError), value);
                                        }
                                    });
                                }
                            }
                            menu.ShowAsContext();
                        }
                        #endregion
                        EditorGUILayout.Space();
                        #region Save as
                        if (GUILayout.Button(Language.GetContent(Language.Help.EditorPoseSaveAs)))
                        {
                            string path = EditorUtility.SaveFilePanel("Save as Pose Template", poseSaveDefaultDirectory, string.Format("{0}.asset", va.currentClip.name), "asset");
                            if (!string.IsNullOrEmpty(path))
                            {
                                if (!path.StartsWith(Application.dataPath))
                                {
                                    EditorCommon.SaveInsideAssetsFolderDisplayDialog();
                                }
                                else
                                {
                                    poseSaveDefaultDirectory = Path.GetDirectoryName(path);
                                    path = path.Replace(Application.dataPath, "Assets");
                                    var poseTemplate = ScriptableObject.CreateInstance<PoseTemplate>();
                                    va.SavePoseTemplate(poseTemplate);
                                    AssetDatabase.CreateAsset(poseTemplate, path);
                                    Focus();
                                }
                            }
                        }
                        #endregion
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(4);
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("Quick Load", GUILayout.Width(70));
                            Action<int> QuickLoad = (index) =>
                            {
                                EditorGUI.BeginDisabledGroup(quickSaves == null || index >= quickSaves.Length || quickSaves[index] == null);
                                if (GUILayout.Button((index + 1).ToString()))
                                {
                                    Undo.RecordObject(this, "Quick Load");
                                    Undo.RegisterCompleteObjectUndo(va.currentClip, "Quick Load");
                                    va.LoadPoseTemplate(quickSaves[index], false);
                                }
                                EditorGUI.EndDisabledGroup();
                            };
                            for (int i = 0; i < QuickSaveSize; i++)
                            {
                                QuickLoad(i);
                            }
                            EditorGUILayout.Space();
                            EditorGUILayout.LabelField("Quick Save", GUILayout.Width(70));
                            Action<int> QuickSave = (index) =>
                            {
                                if (GUILayout.Button((index + 1).ToString()))
                                {
                                    Undo.RecordObject(this, "Quick Save");
                                    if (quickSaves == null || quickSaves.Length != QuickSaveSize)
                                        quickSaves = new PoseTemplate[QuickSaveSize];
                                    {
                                        quickSaves[index] = ScriptableObject.CreateInstance<PoseTemplate>();
                                        va.SavePoseTemplate(quickSaves[index]);
                                    }
                                }
                            };
                            for (int i = 0; i < QuickSaveSize; i++)
                            {
                                QuickSave(i);
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    GUILayout.Space(3);
                    EditorGUILayout.EndVertical();
                }
            }
        }

        private void EditorGUI_BlendPoseGUI()
        {
            if (editorBlendPoseFoldout)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginChangeCheck();
                    editorBlendPoseFoldout = EditorGUILayout.Foldout(editorBlendPoseFoldout, "Blend Pose", true, vaw.guiStyleBoldFoldout);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetBool("VeryAnimation_Editor_BlendPose", editorBlendPoseFoldout);
                    }
                }
                {
                    EditorGUILayout.Space();
                    blendPoseTree.BlendPoseTreeToolbarGUI();
                    if (GUILayout.Button("?", editorBlendPoseGroupHelp ? vaw.guiStyleActiveButton : GUI.skin.button, GUILayout.Width(16), GUILayout.Height(14)))
                    {
                        editorBlendPoseGroupHelp = !editorBlendPoseGroupHelp;
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (editorBlendPoseGroupHelp)
                {
                    EditorGUILayout.HelpBox(Language.GetText(Language.Help.HelpBlendPose), MessageType.Info);
                }

                blendPoseTree.BlendPoseTreeGUI();
            }
        }

        private void EditorGUI_MuscleGroupGUI()
        {
            if (editorMuscleFoldout)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginChangeCheck();
                    editorMuscleFoldout = EditorGUILayout.Foldout(editorMuscleFoldout, "Muscle Group", true, vaw.guiStyleBoldFoldout);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetBool("VeryAnimation_Editor_Muscle", editorMuscleFoldout);
                    }
                }
                {
                    EditorGUILayout.Space();
                    muscleGroupTree.MuscleGroupToolbarGUI();
                    if (GUILayout.Button("?", editorMuscleGroupHelp ? vaw.guiStyleActiveButton : GUI.skin.button, GUILayout.Width(16), GUILayout.Height(14)))
                    {
                        editorMuscleGroupHelp = !editorMuscleGroupHelp;
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (editorMuscleGroupHelp)
                {
                    EditorGUILayout.HelpBox(Language.GetText(Language.Help.HelpMuscleGroup), MessageType.Info);
                }

                muscleGroupTree.MuscleGroupTreeGUI();
            }
        }

        private void EditorGUI_BlendShapeGUI()
        {
            if (!blendShapeTree.IsHaveBlendShapeNodes())
                return;

            if (editorBlendShapeFoldout)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginChangeCheck();
                    editorBlendShapeFoldout = EditorGUILayout.Foldout(editorBlendShapeFoldout, "Blend Shape", true, vaw.guiStyleBoldFoldout);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetBool("VeryAnimation_Editor_BlendShape", editorBlendShapeFoldout);
                    }
                }
                {
                    EditorGUILayout.Space();
                    if (GUILayout.Button("?", editorBlendShapeGroupHelp ? vaw.guiStyleActiveButton : GUI.skin.button, GUILayout.Width(16), GUILayout.Height(14)))
                    {
                        editorBlendShapeGroupHelp = !editorBlendShapeGroupHelp;
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (editorBlendShapeGroupHelp)
                {
                    EditorGUILayout.HelpBox(Language.GetText(Language.Help.HelpBlendShape), MessageType.Info);
                }

                blendShapeTree.BlendShapeTreeGUI();
            }
        }
        private void EditorGUI_SelectionGUI()
        {
            if (editorSelectionFoldout)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginChangeCheck();
                    editorSelectionFoldout = EditorGUILayout.Foldout(editorSelectionFoldout, "Selection", true, vaw.guiStyleBoldFoldout);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetBool("VeryAnimation_Editor_Selection", editorSelectionFoldout);
                    }
                }
                if (va.selectionActiveGameObject != null)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField(va.selectionActiveGameObject, typeof(GameObject), false);
                    EditorGUI.EndDisabledGroup();
                }
                else if (va.animatorIK.ikActiveTarget != AnimatorIKCore.IKTarget.None && va.animatorIK.ikData[(int)va.animatorIK.ikActiveTarget].enable)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.LabelField("Animator IK: " + AnimatorIKCore.IKTargetStrings[(int)va.animatorIK.ikActiveTarget]);
                    EditorGUI.EndDisabledGroup();
                }
                else if (va.originalIK.ikActiveTarget >= 0 && va.originalIK.ikData[va.originalIK.ikActiveTarget].enable)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.LabelField("Original IK: " + va.originalIK.ikData[va.originalIK.ikActiveTarget].name);
                    EditorGUI.EndDisabledGroup();
                }
                else if (va.selectionHumanVirtualBones != null && va.selectionHumanVirtualBones.Count > 0)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.LabelField("Virtual: " + va.selectionHumanVirtualBones[0].ToString());
                    EditorGUI.EndDisabledGroup();
                }
                {
                    EditorGUILayout.Space();
                    if (GUILayout.Button("?", editorSelectionHelp ? vaw.guiStyleActiveButton : GUI.skin.button, GUILayout.Width(16), GUILayout.Height(14)))
                    {
                        editorSelectionHelp = !editorSelectionHelp;
                    }
                }
                EditorGUILayout.EndHorizontal();
                {
                    if (editorSelectionHelp)
                    {
                        EditorGUILayout.HelpBox(Language.GetText(Language.Help.HelpSelection), MessageType.Info);
                    }

                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    {
                        var humanoidIndex = va.SelectionGameObjectHumanoidIndex();
                        var boneIndex = va.selectionActiveBone;
                        if (va.isHuman && (humanoidIndex >= 0 || boneIndex == va.rootMotionBoneIndex))
                        {
                            #region Humanoid
                            if (humanoidIndex == HumanBodyBones.Hips)
                            {
                                EditorGUILayout.LabelField(Language.GetText(Language.Help.SelectionHip), EditorStyles.centeredGreyMiniLabel);
                            }
                            else if (humanoidIndex > HumanBodyBones.Hips || va.selectionActiveGameObject == vaw.gameObject)
                            {
                                EditorGUILayout.BeginHorizontal();
                                #region Mirror
                                var mirrorIndex = humanoidIndex >= 0 && va.humanoidIndex2boneIndex[(int)humanoidIndex] >= 0 ? va.mirrorBoneIndexes[va.humanoidIndex2boneIndex[(int)humanoidIndex]] : -1;
                                if (GUILayout.Button(Language.GetContentFormat(Language.Help.SelectionMirror, (mirrorIndex >= 0 ? string.Format("From '{0}'", va.bones[mirrorIndex].name) : "From self")), GUILayout.Width(100)))
                                {
                                    va.SelectionHumanoidMirror();
                                }
                                #endregion
                                EditorGUILayout.Space();
                                #region Reset
                                if (GUILayout.Button("Reset All", GUILayout.Width(100)))
                                {
                                    va.SelectionHumanoidResetAll();
                                }
                                #endregion
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.Space();
                            }
                            int RowCount = 0;
                            if (boneIndex == va.rootMotionBoneIndex)
                            {
                                #region Root
                                {
                                    EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                                    if (GUILayout.Button("RootT", GUILayout.Width(50)))
                                    {
                                        va.lastTool = Tool.Move;
                                        va.SelectGameObject(vaw.gameObject);
                                    }
                                    EditorGUI.BeginChangeCheck();
                                    var rootT = EditorGUILayout.Vector3Field("", va.GetAnimationValueAnimatorRootT());
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        va.SetAnimationValueAnimatorRootT(rootT);
                                    }
                                    if (GUILayout.Button("Reset", GUILayout.Width(44)))
                                    {
                                        if (va.IsHaveAnimationCurveAnimatorRootT())
                                        {
                                            va.SetAnimationValueAnimatorRootT(new Vector3(0, 1, 0));
                                        }
                                    }
                                    EditorGUILayout.EndHorizontal();
                                }
                                {
                                    EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                                    if (GUILayout.Button("RootQ", GUILayout.Width(50)))
                                    {
                                        va.lastTool = Tool.Rotate;
                                        va.SelectGameObject(vaw.gameObject);
                                    }
                                    EditorGUI.BeginChangeCheck();
                                    var rootQ = EditorGUILayout.Vector3Field("", va.GetAnimationValueAnimatorRootQ().eulerAngles);
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        va.SetAnimationValueAnimatorRootQ(Quaternion.Euler(rootQ));
                                    }
                                    if (GUILayout.Button("Reset", GUILayout.Width(44)))
                                    {
                                        if (va.IsHaveAnimationCurveAnimatorRootQ())
                                        {
                                            va.SetAnimationValueAnimatorRootQ(Quaternion.identity);
                                        }
                                    }
                                    EditorGUILayout.EndHorizontal();
                                }
                                #endregion
                            }
                            else if (humanoidIndex > HumanBodyBones.Hips)
                            {
                                #region Muscle
                                if (vaw.muscleRotationSliderIds == null || vaw.muscleRotationSliderIds.Length != 3)
                                    vaw.muscleRotationSliderIds = new int[3];
                                for (int i = 0; i < vaw.muscleRotationSliderIds.Length; i++)
                                    vaw.muscleRotationSliderIds[i] = -1;
                                for (int i = 0; i < 3; i++)
                                {
                                    var muscleIndex = HumanTrait.MuscleFromBone((int)humanoidIndex, i);
                                    if (muscleIndex < 0) continue;
                                    var muscleValue = va.GetAnimationValueAnimatorMuscle(muscleIndex);
                                    EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                                    if (GUILayout.Button(new GUIContent(va.musclePropertyName.Names[muscleIndex], muscleValue.ToString())))
                                    {
                                        va.lastTool = Tool.Rotate;
                                        va.SelectHumanoidBones(new HumanBodyBones[] { humanoidIndex });
                                        va.SetAnimationWindowSynchroSelection(new EditorCurveBinding[] { va.AnimationCurveBindingAnimatorMuscle(muscleIndex) });
                                    }
                                    GUILayout.Space(9);
                                    {
                                        var saveBackgroundColor = GUI.backgroundColor;
                                        switch (i)
                                        {
                                        case 0: GUI.backgroundColor = Handles.xAxisColor; break;
                                        case 1: GUI.backgroundColor = Handles.yAxisColor; break;
                                        case 2: GUI.backgroundColor = Handles.zAxisColor; break;
                                        }
                                        EditorGUI.BeginChangeCheck();
                                        muscleValue = GUILayout.HorizontalSlider(muscleValue, -1f, 1f, GUILayout.Width(vaw.editorSettings.settingEditorSliderSize));
                                        vaw.muscleRotationSliderIds[i] = vaw.uEditorGUIUtility.GetLastControlID();
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            foreach (var mi in va.SelectionGameObjectsMuscleIndex(i))
                                            {
                                                va.SetAnimationValueAnimatorMuscle(mi, muscleValue);
                                            }
                                        }
                                        GUI.backgroundColor = saveBackgroundColor;
                                    }
                                    if (GUILayout.Button("Reset", GUILayout.Width(44)))
                                    {
                                        foreach (var mi in va.SelectionGameObjectsMuscleIndex(i))
                                        {
                                            if (va.IsHaveAnimationCurveAnimatorMuscle(mi))
                                            {
                                                va.SetAnimationValueAnimatorMuscle(mi, 0f);
                                            }
                                        }
                                    }
                                    EditorGUILayout.EndHorizontal();
                                }
                                #endregion

                                #region TDOF
                                if (va.humanoidHasTDoF && VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)humanoidIndex] != null)
                                {
                                    EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                                    if (GUILayout.Button("TDOF", GUILayout.Width(50)))
                                    {
                                        va.lastTool = Tool.Move;
                                        va.SelectHumanoidBones(new HumanBodyBones[] { humanoidIndex });
                                    }
                                    EditorGUI.BeginChangeCheck();
                                    var tdof = EditorGUILayout.Vector3Field("", va.GetAnimationValueAnimatorTDOF(VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)humanoidIndex].index));
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        foreach (var hi in va.SelectionGameObjectsHumanoidIndex())
                                        {
                                            if (VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)hi] == null) continue;
                                            va.SetAnimationValueAnimatorTDOF(VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)hi].index, tdof);
                                        }
                                    }
                                    if (GUILayout.Button("Reset", GUILayout.Width(44)))
                                    {
                                        foreach (var hi in va.SelectionGameObjectsHumanoidIndex())
                                        {
                                            if (VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)hi] == null) continue;
                                            if (va.IsHaveAnimationCurveAnimatorTDOF(VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)hi].index))
                                            {
                                                va.SetAnimationValueAnimatorTDOF(VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)hi].index, Vector3.zero);
                                            }
                                        }
                                    }
                                    EditorGUILayout.EndHorizontal();
                                }
                                #endregion
                            }
                            #endregion
                        }
                        else if (boneIndex >= 0)
                        {
                            #region Generic
                            if (va.isHuman && va.humanoidConflict[boneIndex])
                            {
                                EditorGUILayout.LabelField(Language.GetText(Language.Help.SelectionHumanoidConflict), EditorStyles.centeredGreyMiniLabel);
                            }
                            else
                            {
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    #region Mirror
                                    if (GUILayout.Button(Language.GetContentFormat(Language.Help.SelectionMirror, (va.mirrorBoneIndexes[boneIndex] >= 0 ? string.Format("From '{0}'", va.bones[va.mirrorBoneIndexes[boneIndex]].name) : "From self")), GUILayout.Width(100)))
                                    {
                                        va.SelectionGenericMirror();
                                    }
                                    #endregion
                                    EditorGUILayout.Space();
                                    #region Reset
                                    if (GUILayout.Button("Reset All", GUILayout.Width(100)))
                                    {
                                        va.SelectionGenericResetAll();
                                    }
                                    #endregion
                                    EditorGUILayout.EndHorizontal();
                                }
                                EditorGUILayout.Space();
                                int RowCount = 0;
                                if (boneIndex == va.rootMotionBoneIndex)
                                {
                                    #region Root
                                    {
                                        EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                                        if (GUILayout.Button("RootT", GUILayout.Width(50)))
                                        {
                                            va.lastTool = Tool.Move;
                                            va.SelectGameObject(va.bones[va.rootMotionBoneIndex]);
                                        }
                                        EditorGUI.BeginChangeCheck();
                                        var rootT = EditorGUILayout.Vector3Field("", va.GetAnimationValueAnimatorRootT());
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            va.SetAnimationValueAnimatorRootT(rootT);
                                        }
                                        if (GUILayout.Button("Reset", GUILayout.Width(44)))
                                        {
                                            if (va.IsHaveAnimationCurveAnimatorRootT())
                                            {
                                                va.SetAnimationValueAnimatorRootT(va.boneSaveTransforms[boneIndex].localPosition);
                                            }
                                        }
                                        EditorGUILayout.EndHorizontal();
                                    }
                                    {
                                        EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                                        if (GUILayout.Button("RootQ", GUILayout.Width(50)))
                                        {
                                            va.lastTool = Tool.Rotate;
                                            va.SelectGameObject(va.bones[va.rootMotionBoneIndex]);
                                        }
                                        EditorGUI.BeginChangeCheck();
                                        var rootQ = EditorGUILayout.Vector3Field("", va.GetAnimationValueAnimatorRootQ().eulerAngles);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            va.SetAnimationValueAnimatorRootQ(Quaternion.Euler(rootQ));
                                        }
                                        if (GUILayout.Button("Reset", GUILayout.Width(44)))
                                        {
                                            if (va.IsHaveAnimationCurveAnimatorRootQ())
                                            {
                                                va.SetAnimationValueAnimatorRootQ(va.boneSaveTransforms[boneIndex].localRotation);
                                            }
                                        }
                                        EditorGUILayout.EndHorizontal();
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region Position
                                    {
                                        EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                                        if (GUILayout.Button("Position", GUILayout.Width(58)))
                                        {
                                            va.lastTool = Tool.Move;
                                            va.SelectGameObject(va.bones[boneIndex]);
                                        }
                                        EditorGUI.BeginChangeCheck();
                                        {
                                            var localPosition = EditorGUILayout.Vector3Field("", va.GetAnimationValueTransformPosition(boneIndex));
                                            if (EditorGUI.EndChangeCheck())
                                            {
                                                foreach (var bi in va.SelectionGameObjectsOtherHumanoidBoneIndex())
                                                {
                                                    va.SetAnimationValueTransformPosition(bi, localPosition);
                                                }
                                            }
                                        }
                                        if (GUILayout.Button("Reset", GUILayout.Width(44)))
                                        {
                                            foreach (var bi in va.SelectionGameObjectsOtherHumanoidBoneIndex())
                                            {
                                                if (va.IsHaveAnimationCurveTransformPosition(bi))
                                                {
                                                    va.SetAnimationValueTransformPosition(bi, va.boneSaveTransforms[bi].localPosition);
                                                }
                                            }
                                        }
                                        EditorGUILayout.EndHorizontal();
                                    }
                                    #endregion
                                    #region Rotation
                                    {
                                        EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                                        if (GUILayout.Button("Rotation", GUILayout.Width(58)))
                                        {
                                            va.lastTool = Tool.Rotate;
                                            va.SelectGameObject(va.bones[boneIndex]);
                                        }
                                        EditorGUI.BeginChangeCheck();
                                        {
                                            var localEulerAngles = EditorGUILayout.Vector3Field("", va.GetAnimationValueTransformRotation(boneIndex).eulerAngles);
                                            if (EditorGUI.EndChangeCheck())
                                            {
                                                foreach (var bi in va.SelectionGameObjectsOtherHumanoidBoneIndex())
                                                {
                                                    va.SetAnimationValueTransformRotation(bi, Quaternion.Euler(localEulerAngles));
                                                }
                                            }
                                        }
                                        if (GUILayout.Button("Reset", GUILayout.Width(44)))
                                        {
                                            foreach (var bi in va.SelectionGameObjectsOtherHumanoidBoneIndex())
                                            {
                                                if (va.IsHaveAnimationCurveTransformRotation(bi) != URotationCurveInterpolation.Mode.Undefined)
                                                {
                                                    va.SetAnimationValueTransformRotation(bi, va.boneSaveTransforms[bi].localRotation);
                                                }
                                            }
                                        }
                                        EditorGUILayout.EndHorizontal();
                                    }
                                    #endregion
                                    #region Scale
                                    {
                                        EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                                        if (GUILayout.Button("Scale", GUILayout.Width(58)))
                                        {
                                            va.lastTool = Tool.Scale;
                                            va.SelectGameObject(va.bones[boneIndex]);
                                        }
                                        EditorGUI.BeginChangeCheck();
                                        {
                                            var localScale = EditorGUILayout.Vector3Field("", va.GetAnimationValueTransformScale(boneIndex));
                                            if (EditorGUI.EndChangeCheck())
                                            {
                                                foreach (var bi in va.SelectionGameObjectsOtherHumanoidBoneIndex())
                                                {
                                                    va.SetAnimationValueTransformScale(bi, localScale);
                                                }
                                            }
                                        }
                                        if (GUILayout.Button("Reset", GUILayout.Width(44)))
                                        {
                                            foreach (var bi in va.SelectionGameObjectsOtherHumanoidBoneIndex())
                                            {
                                                if (va.IsHaveAnimationCurveTransformScale(bi))
                                                {
                                                    va.SetAnimationValueTransformScale(boneIndex, va.boneSaveTransforms[bi].localScale);
                                                }
                                            }
                                        }
                                        EditorGUILayout.EndHorizontal();
                                    }
                                    #endregion
                                }
                            }
                            #endregion
                        }
                        else if (va.animatorIK.ikActiveTarget != AnimatorIKCore.IKTarget.None)
                        {
                            va.animatorIK.SelectionGUI();
                        }
                        else if (va.originalIK.ikActiveTarget >= 0)
                        {
                            va.originalIK.SelectionGUI();
                        }
                        else
                        {
                            EditorGUILayout.LabelField(Language.GetText(Language.Help.SelectionNothingisselected), EditorStyles.centeredGreyMiniLabel);
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
            }
        }

        private void UpdateRootCorrectionModeString()
        {
            for (int i = 0; i < (int)VeryAnimation.RootCorrectionMode.Total; i++)
            {
                RootCorrectionModeString[i] = Language.GetContent(Language.Help.EditorRootCorrectionDisable + i);
            }
        }

        public static void ForceRepaint()
        {
            if (instance == null) return;
            instance.Repaint();
        }
    }
}
