using UnityEngine;
using UnityEngine.Profiling;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace VeryAnimation
{
    [Serializable]
    public class BlendPoseTree
    {
        private VeryAnimationWindow vaw { get { return VeryAnimationWindow.instance; } }
        private VeryAnimation va { get { return VeryAnimation.instance; } }
        private VeryAnimationEditorWindow vae { get { return VeryAnimationEditorWindow.instance; } }

        [SerializeField]
        private PoseTemplate poseL;
        [SerializeField]
        private PoseTemplate poseR;

        private bool IsPoseReady { get { return poseL != null && poseR != null; } }

        private class PoseIndexTable
        {
            public int[] muscleIndexes;
            public int[] transformIndexes;
        }
        private PoseIndexTable poseIndexTableL;
        private PoseIndexTable poseIndexTableR;

        public enum EditMode
        {
            Tree,
            Selection,
            Total
        }
        private EditMode editMode;

        private GUIContent[] EditModeString = new GUIContent[(int)EditMode.Total];

        private class BaseNode
        {
            public string name;
            public bool foldout;
            public BaseNode[] children;
        }
        private BaseNode rootNode;

        #region Humanoid
        private class HumanoidNode : BaseNode
        {
            public HumanBodyBones[] humanoidIndexes;
        }
        private HumanoidNode humanoidNode;

        [SerializeField]
        private bool humanoidEnablePosition = true;
        [SerializeField]
        private bool humanoidEnableRotation = true;
        #endregion

        #region Generic
        private class GenericNode : BaseNode
        {
            public int boneIndex;
        }
        private GenericNode genericNode;

        [SerializeField]
        private bool genericEnablePosition = true;
        [SerializeField]
        private bool genericEnableRotation = true;
        [SerializeField]
        private bool genericEnableScale = true;
        #endregion

        #region BlendShape
        private class BlendShapeNode : BaseNode
        {
            public SkinnedMeshRenderer renderer;
            public string[] blendShapeNames;
        }
        private BlendShapeNode blendShapeNode;

        [SerializeField]
        private bool blendShapeEnable = true;
        #endregion

        #region Values
        private Dictionary<BaseNode, int> blendPoseTreeTable;
        [SerializeField]
        private float[] blendPoseValues;
        #endregion

        public BlendPoseTree()
        {
            #region EditorPref
            EditorApplication.delayCall += () =>
            {
                editMode = (EditMode)EditorPrefs.GetInt("VeryAnimation_Editor_BlendPose_EditMode", 0);
            };
            #endregion

            UpdateEditModeString();
            Language.OnLanguageChanged += UpdateEditModeString;

            UpdateNode();
        }

        public void BlendPoseTreeToolbarGUI()
        {
            EditorGUI.BeginChangeCheck();
            var mode = (EditMode)GUILayout.Toolbar((int)editMode, EditModeString, EditorStyles.miniButton);
            if (EditorGUI.EndChangeCheck())
            {
                editMode = mode;
                EditorPrefs.SetInt("VeryAnimation_Editor_BlendPose_EditMode", (int)editMode);
            }
        }
        public void BlendPoseTreeGUI()
        {
            RowCount = 0;
            LabelWidth = Mathf.Min(VeryAnimationEditorWindow.instance.position.width / 2f, 400f);

            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                #region Top
                {
                    Action<int> SetPoseButton = (index) =>
                    {
                        if (GUILayout.Button(new GUIContent("", "Set current pose or load from template file"), vaw.guiStyleDropDown, GUILayout.Width(20f)))
                        {
                            Dictionary<string, PoseTemplate> poseTemplates = new Dictionary<string, PoseTemplate>();
                            {
                                var guids = AssetDatabase.FindAssets("t:posetemplate");
                                for (int i = 0; i < guids.Length; i++)
                                {
                                    var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                                    var poseTemplate = AssetDatabase.LoadAssetAtPath<PoseTemplate>(path);
                                    if (poseTemplate == null) continue;
                                    var name = path.Remove(0, "Assets/".Length);
                                    poseTemplates.Add(name, poseTemplate);
                                }
                            }

                            Action<PoseTemplate> MenuCallback = (poseTemplate) =>
                            {
                                Undo.RecordObject(vae, "Set Pose");
                                if (index == 0)
                                {
                                    poseL = poseTemplate;
                                }
                                else if (index == 1)
                                {
                                    poseR = poseTemplate;
                                }
                                UpdateNode();
                            };

                            GenericMenu menu = new GenericMenu();
                            {
                                menu.AddItem(new GUIContent("Current Pose"), false, () =>
                                {
                                    Undo.RecordObject(vae, "Set Pose");
                                    if (index == 0)
                                    {
                                        poseL = ScriptableObject.CreateInstance<PoseTemplate>();
                                        poseL.name = "Current";
                                        va.SavePoseTemplate(poseL);
                                    }
                                    else if (index == 1)
                                    {
                                        poseR = ScriptableObject.CreateInstance<PoseTemplate>();
                                        poseR.name = "Current";
                                        va.SavePoseTemplate(poseR);
                                    }
                                    UpdateNode();
                                });
                                menu.AddSeparator(string.Empty);
                            }
                            {
                                var enu = poseTemplates.GetEnumerator();
                                while (enu.MoveNext())
                                {
                                    var value = enu.Current.Value;
                                    menu.AddItem(new GUIContent(enu.Current.Key), false, () => { MenuCallback(value); });
                                }
                            }
                            menu.ShowAsContext();
                        }
                    };

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.BeginHorizontal(vaw.guiStyleAnimationRowOddStyle);
                        EditorGUILayout.LabelField("L", EditorStyles.boldLabel, GUILayout.Width(14f));
                        {
                            EditorGUI.BeginChangeCheck();
                            var pose = EditorGUILayout.ObjectField(poseL, typeof(PoseTemplate), false, GUILayout.Width(LabelWidth / 3f)) as PoseTemplate;
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(vae, "Set Pose");
                                poseL = pose;
                                UpdateNode();
                            }
                        }
                        SetPoseButton(0);
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.Space();
                    {
                        EditorGUI.BeginDisabledGroup(poseL == null || poseR == null);
                        EditorGUI.BeginChangeCheck();
                        var value = GUILayout.HorizontalSlider(blendPoseValues[0], 0f, 1f, GUILayout.Width(vaw.editorSettings.settingEditorSliderSize));
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(vae, "Change Slider");
                            if (editMode == EditMode.Tree)
                            {
                                SetChildrenValue(rootNode, value);
                            }
                            else if (editMode == EditMode.Selection)
                            {
                                blendPoseValues[blendPoseTreeTable[rootNode]] = value;
                                SetSelectionHumanoidValue(value);
                                SetSelectionGenericValue(value);
                                SetSelectionBlendShapeValue(value);
                            }
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    EditorGUILayout.Space();
                    {
                        EditorGUILayout.BeginHorizontal(vaw.guiStyleAnimationRowOddStyle);
                        {
                            EditorGUI.BeginChangeCheck();
                            var pose = EditorGUILayout.ObjectField(poseR, typeof(PoseTemplate), false, GUILayout.Width(LabelWidth / 3f)) as PoseTemplate;
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(vae, "Set Pose");
                                poseR = pose;
                                UpdateNode();
                            }
                        }
                        SetPoseButton(1);
                        EditorGUILayout.LabelField("R", EditorStyles.boldLabel, GUILayout.Width(14f));
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                #endregion

                GUILayout.Space(1);

                if (!IsPoseReady)
                {
                    EditorGUILayout.HelpBox(Language.GetText(Language.Help.BlendPoseNotPoseReady), MessageType.Info);
                }
                else if (editMode == EditMode.Tree)
                {
                    #region Humanoid
                    if (humanoidNode != null)
                    {
                        HumanoidTreeNodeGUI(humanoidNode);
                    }
                    #endregion

                    #region Generic
                    if (genericNode != null)
                    {
                        GenericTreeNodeGUI(genericNode);
                    }
                    #endregion

                    #region BlendShape
                    if (blendShapeNode != null)
                    {
                        BlendShapeTreeNodeGUI(blendShapeNode);
                    }
                    #endregion
                }
                else if (editMode == EditMode.Selection)
                {
                    #region Humanoid
                    if (humanoidNode != null)
                    {
                        EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                        EditorGUILayout.LabelField(new GUIContent("Humanoid"), GUILayout.Width(128f));
                        GUILayout.FlexibleSpace();
                        {
                            humanoidEnablePosition = EditorGUILayout.ToggleLeft(new GUIContent("P", "Position"), humanoidEnablePosition, GUILayout.Width(26f));
                            humanoidEnableRotation = EditorGUILayout.ToggleLeft(new GUIContent("R", "Rotation"), humanoidEnableRotation, GUILayout.Width(26f));
                            GUILayout.Space(30f);
                        }
                        {
                            EditorGUI.BeginChangeCheck();
                            var value = GUILayout.HorizontalSlider(blendPoseValues[blendPoseTreeTable[humanoidNode]], 0f, 1f, GUILayout.Width(vaw.editorSettings.settingEditorSliderSize));
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(vae, "Change Humanoid");
                                SetSelectionHumanoidValue(value);
                            }
                        }
                        if (GUILayout.Button("Reset", GUILayout.Width(44)))
                        {
                            Undo.RecordObject(vae, "Reset Humanoid");
                            SetSelectionHumanoidValue(0f);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    #endregion

                    #region Generic
                    if (genericNode != null)
                    {
                        EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                        EditorGUILayout.LabelField(new GUIContent("Transform"), GUILayout.Width(128f));
                        GUILayout.FlexibleSpace();
                        {
                            genericEnablePosition = EditorGUILayout.ToggleLeft(new GUIContent("P", "Position"), genericEnablePosition, GUILayout.Width(26f));
                            genericEnableRotation = EditorGUILayout.ToggleLeft(new GUIContent("R", "Rotation"), genericEnableRotation, GUILayout.Width(26f));
                            genericEnableScale = EditorGUILayout.ToggleLeft(new GUIContent("S", "Scale"), genericEnableScale, GUILayout.Width(26f));
                        }
                        {
                            EditorGUI.BeginChangeCheck();
                            var value = GUILayout.HorizontalSlider(blendPoseValues[blendPoseTreeTable[genericNode]], 0f, 1f, GUILayout.Width(vaw.editorSettings.settingEditorSliderSize));
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(vae, "Change Generic");
                                SetSelectionGenericValue(value);
                            }
                        }
                        if (GUILayout.Button("Reset", GUILayout.Width(44)))
                        {
                            Undo.RecordObject(vae, "Reset Generic");
                            SetSelectionGenericValue(0f);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    #endregion

                    #region BlendShape
                    if (blendShapeNode != null)
                    {
                        EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                        EditorGUILayout.LabelField(new GUIContent("Blend Shape"), GUILayout.Width(128f));
                        GUILayout.FlexibleSpace();
                        {
                            blendShapeEnable = EditorGUILayout.ToggleLeft(new GUIContent("", "BlendShape"), blendShapeEnable, GUILayout.Width(26f));
                            GUILayout.Space(30f * 2);
                        }
                        {
                            EditorGUI.BeginChangeCheck();
                            var value = GUILayout.HorizontalSlider(blendPoseValues[blendPoseTreeTable[blendShapeNode]], 0f, 1f, GUILayout.Width(vaw.editorSettings.settingEditorSliderSize));
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(vae, "Change BlendShape");
                                SetSelectionBlendShapeValue(value);
                            }
                        }
                        if (GUILayout.Button("Reset", GUILayout.Width(44)))
                        {
                            Undo.RecordObject(vae, "Reset BlendShape");
                            SetSelectionBlendShapeValue(0f);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    #endregion
                }
            }
            EditorGUILayout.EndVertical();
        }
        #region BlendPoseTreeGUI
        private int RowCount = 0;
        private float LabelWidth = 0;
        private const int IndentWidth = 15;
        private void HumanoidTreeNodeGUI(HumanoidNode mg)
        {
            EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
            EditorGUI.BeginChangeCheck();
            mg.foldout = EditorGUILayout.Foldout(mg.foldout, new GUIContent(mg.name, blendPoseValues[blendPoseTreeTable[mg]].ToString()), true);
            if (EditorGUI.EndChangeCheck())
            {
                if (Event.current.alt)
                    SetChildrenFoldout(mg, mg.foldout);
            }
            if (mg == humanoidNode)
            {
                {
                    EditorGUI.BeginChangeCheck();
                    var flag = EditorGUILayout.ToggleLeft(new GUIContent("P", "Position"), humanoidEnablePosition, GUILayout.Width(26f));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vae, "Change Enable Flag");
                        humanoidEnablePosition = flag;
                    }
                }
                {
                    EditorGUI.BeginChangeCheck();
                    var flag = EditorGUILayout.ToggleLeft(new GUIContent("R", "Rotation"), humanoidEnableRotation, GUILayout.Width(26f));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vae, "Change Enable Flag");
                        humanoidEnableRotation = flag;
                    }
                }
                GUILayout.Space(30f);
            }
            {
                EditorGUI.BeginChangeCheck();
                var value = GUILayout.HorizontalSlider(blendPoseValues[blendPoseTreeTable[mg]], 0f, 1f, GUILayout.Width(vaw.editorSettings.settingEditorSliderSize));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(vae, "Change Humanoid");
                    SetChildrenValue(mg, value);
                }
            }
            var nodeLevel = GetTreeLevel(mg, 0);
            GUILayout.Space(IndentWidth * nodeLevel);
            if (GUILayout.Button("Reset", GUILayout.Width(44)))
            {
                Undo.RecordObject(vae, "Reset Humanoid");
                SetChildrenValue(mg, 0f);
            }
            EditorGUILayout.EndHorizontal();
            if (mg.foldout)
            {
                EditorGUI.indentLevel++;
                if (mg.humanoidIndexes != null && mg.humanoidIndexes.Length > 0)
                {
                    for (int index = 0; index < mg.humanoidIndexes.Length; index++)
                    {
                        var hi = mg.humanoidIndexes[index];
                        var name = hi >= 0 ? hi.ToString() : "Root";
                        var valueIndex = blendPoseTreeTable[mg] + 1 + index;

                        EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                        EditorGUI.indentLevel++;
                        {
                            var rect = GUILayoutUtility.GetRect(new GUIContent(name), GUI.skin.button, GUILayout.Width(LabelWidth), GUILayout.Height(22));
                            {
                                rect.x += IndentWidth * EditorGUI.indentLevel;
                                rect.width -= IndentWidth * EditorGUI.indentLevel;
                                rect.height -= 4;
                            }
                            if (GUI.Button(rect, new GUIContent(name, blendPoseValues[valueIndex].ToString())))
                            {
                                #region SetAnimationWindowSynchroSelection
                                var bindings = new List<EditorCurveBinding>();
                                if (hi < 0)
                                {
                                    if (humanoidEnablePosition)
                                    {
                                        foreach (var binding in va.AnimationCurveBindingAnimatorRootT)
                                            bindings.Add(binding);
                                    }
                                    if (humanoidEnableRotation)
                                    {
                                        foreach (var binding in va.AnimationCurveBindingAnimatorRootQ)
                                            bindings.Add(binding);
                                    }
                                }
                                else
                                {
                                    if (humanoidEnableRotation)
                                    {
                                        for (int dof = 0; dof < 3; dof++)
                                        {
                                            var muscleIndex = HumanTrait.MuscleFromBone((int)hi, dof);
                                            if (muscleIndex < 0) continue;
                                            bindings.Add(va.AnimationCurveBindingAnimatorMuscle(muscleIndex));
                                        }
                                    }
                                    if (humanoidEnablePosition)
                                    {
                                        if (VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)hi] != null)
                                        {
                                            var tdofIndex = VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)hi].index;
                                            if (tdofIndex >= 0)
                                            {
                                                for (int dof = 0; dof < 3; dof++)
                                                {
                                                    bindings.Add(va.AnimationCurveBindingAnimatorTDOF(tdofIndex, dof));
                                                }
                                            }
                                        }
                                    }
                                }
                                if (bindings.Count > 0)
                                    va.SetAnimationWindowSynchroSelection(bindings.ToArray());
                                #endregion
                            }
                        }
                        GUILayoutUtility.GetRect(0f, 0f);
                        {
                            EditorGUI.BeginChangeCheck();
                            var value = GUILayout.HorizontalSlider(blendPoseValues[valueIndex], 0f, 1f, GUILayout.Width(vaw.editorSettings.settingEditorSliderSize));
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(vae, "Change Humanoid");
                                blendPoseValues[valueIndex] = value;
                                SetHumanoidValue(hi, value);
                            }
                        }
                        GUILayout.Space(IndentWidth * (nodeLevel - 1));
                        if (GUILayout.Button("Reset", GUILayout.Width(44)))
                        {
                            Undo.RecordObject(vae, "Reset Humanoid");
                            blendPoseValues[valueIndex] = 0f;
                            SetHumanoidValue(hi, 0f);
                        }
                        EditorGUI.indentLevel--;
                        EditorGUILayout.EndHorizontal();
                    }
                }
                if (mg.children != null && mg.children.Length > 0)
                {
                    foreach (var child in mg.children)
                    {
                        HumanoidTreeNodeGUI(child as HumanoidNode);
                    }
                }
                EditorGUI.indentLevel--;
            }
        }
        private void GenericTreeNodeGUI(GenericNode mg)
        {
            EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
            if (mg.children != null && mg.children.Length > 0)
            {
                EditorGUI.BeginChangeCheck();
                mg.foldout = EditorGUILayout.Foldout(mg.foldout, new GUIContent(mg.name, blendPoseValues[blendPoseTreeTable[mg]].ToString()), true);
                if (EditorGUI.EndChangeCheck())
                {
                    if (Event.current.alt)
                        SetChildrenFoldout(mg, mg.foldout);
                }
            }
            else
            {
                GUILayout.Space(16f);
                EditorGUILayout.LabelField(new GUIContent(mg.name, blendPoseValues[blendPoseTreeTable[mg]].ToString()));
            }
            if (mg == genericNode)
            {
                {
                    EditorGUI.BeginChangeCheck();
                    var flag = EditorGUILayout.ToggleLeft(new GUIContent("P", "Position"), genericEnablePosition, GUILayout.Width(26f));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vae, "Change Enable Flag");
                        genericEnablePosition = flag;
                    }
                }
                {
                    EditorGUI.BeginChangeCheck();
                    var flag = EditorGUILayout.ToggleLeft(new GUIContent("R", "Rotation"), genericEnableRotation, GUILayout.Width(26f));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vae, "Change Enable Flag");
                        genericEnableRotation = flag;
                    }
                }
                {
                    EditorGUI.BeginChangeCheck();
                    var flag = EditorGUILayout.ToggleLeft(new GUIContent("S", "Scale"), genericEnableScale, GUILayout.Width(26f));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vae, "Change Enable Flag");
                        genericEnableScale = flag;
                    }
                }
            }
            {
                EditorGUI.BeginChangeCheck();
                var value = GUILayout.HorizontalSlider(blendPoseValues[blendPoseTreeTable[mg]], 0f, 1f, GUILayout.Width(vaw.editorSettings.settingEditorSliderSize));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(vae, "Change Generic");
                    SetChildrenValue(mg, value);
                }
            }
            var nodeLevel = GetTreeLevel(mg, 0);
            GUILayout.Space(IndentWidth * nodeLevel);
            if (GUILayout.Button("Reset", GUILayout.Width(44)))
            {
                Undo.RecordObject(vae, "Reset Generic");
                SetChildrenValue(mg, 0f);
            }
            EditorGUILayout.EndHorizontal();
            if (mg.foldout)
            {
                EditorGUI.indentLevel++;
                if (mg.children != null && mg.children.Length > 0)
                {
                    foreach (var child in mg.children)
                    {
                        GenericTreeNodeGUI(child as GenericNode);
                    }
                }
                EditorGUI.indentLevel--;
            }
        }
        private void BlendShapeTreeNodeGUI(BlendShapeNode mg)
        {
            EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
            EditorGUI.BeginChangeCheck();
            mg.foldout = EditorGUILayout.Foldout(mg.foldout, new GUIContent(mg.name, blendPoseValues[blendPoseTreeTable[mg]].ToString()), true);
            if (EditorGUI.EndChangeCheck())
            {
                if (Event.current.alt)
                    SetChildrenFoldout(mg, mg.foldout);
            }
            if (mg == blendShapeNode)
            {
                {
                    EditorGUI.BeginChangeCheck();
                    var flag = EditorGUILayout.ToggleLeft(new GUIContent(" ", "BlendShape"), blendShapeEnable, GUILayout.Width(26f));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vae, "Change Enable Flag");
                        blendShapeEnable = flag;
                    }
                }
                GUILayout.Space(30f * 2);
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(mg.renderer, typeof(SkinnedMeshRenderer), false, GUILayout.Width(128f));
                EditorGUI.EndDisabledGroup();
            }
            {
                EditorGUI.BeginChangeCheck();
                var value = GUILayout.HorizontalSlider(blendPoseValues[blendPoseTreeTable[mg]], 0f, 1f, GUILayout.Width(vaw.editorSettings.settingEditorSliderSize));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(vae, "Change BlendShape");
                    SetChildrenValue(mg, value);
                }
            }
            var nodeLevel = GetTreeLevel(mg, 0);
            GUILayout.Space(IndentWidth * nodeLevel);
            if (GUILayout.Button("Reset", GUILayout.Width(44)))
            {
                Undo.RecordObject(vae, "Reset BlendShape");
                SetChildrenValue(mg, 0f);
            }
            EditorGUILayout.EndHorizontal();
            if (mg.foldout)
            {
                EditorGUI.indentLevel++;
                if (mg.blendShapeNames != null && mg.blendShapeNames.Length > 0)
                {
                    for (int index = 0; index < mg.blendShapeNames.Length; index++)
                    {
                        var valueIndex = blendPoseTreeTable[mg] + 1 + index;

                        EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                        EditorGUI.indentLevel++;
                        {
                            var rect = GUILayoutUtility.GetRect(new GUIContent(mg.blendShapeNames[index]), GUI.skin.button, GUILayout.Width(LabelWidth), GUILayout.Height(22));
                            {
                                rect.x += IndentWidth * EditorGUI.indentLevel;
                                rect.width -= IndentWidth * EditorGUI.indentLevel;
                                rect.height -= 4;
                            }
                            if (GUI.Button(rect, new GUIContent(mg.blendShapeNames[index], blendPoseValues[valueIndex].ToString())))
                            {
                                va.SetAnimationWindowSynchroSelection(new EditorCurveBinding[] { va.AnimationCurveBindingBlendShape(mg.renderer, mg.blendShapeNames[index]) });
                            }
                        }
                        GUILayoutUtility.GetRect(0f, 0f);
                        {
                            EditorGUI.BeginChangeCheck();
                            var value = GUILayout.HorizontalSlider(blendPoseValues[valueIndex], 0f, 1f, GUILayout.Width(vaw.editorSettings.settingEditorSliderSize));
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(vae, "Change BlendShape");
                                blendPoseValues[valueIndex] = value;
                                SetBlendShapeValue(mg.renderer, mg.blendShapeNames[index], value);
                            }
                        }
                        GUILayout.Space(IndentWidth * (nodeLevel - 1));
                        if (GUILayout.Button("Reset", GUILayout.Width(44)))
                        {
                            Undo.RecordObject(vae, "Reset BlendShape");
                            blendPoseValues[valueIndex] = 0f;
                            SetBlendShapeValue(mg.renderer, mg.blendShapeNames[index], 0f);
                        }
                        EditorGUI.indentLevel--;
                        EditorGUILayout.EndHorizontal();
                    }
                }
                if (mg.children != null && mg.children.Length > 0)
                {
                    foreach (var child in mg.children)
                    {
                        BlendShapeTreeNodeGUI(child as BlendShapeNode);
                    }
                }
                EditorGUI.indentLevel--;
            }
        }
        private int GetTreeLevel(BaseNode node, int level)
        {
            if (node.foldout)
            {
                if (node.children != null && node.children.Length > 0)
                {
                    int tmp = level;
                    foreach (var child in node.children)
                    {
                        tmp = Math.Max(tmp, GetTreeLevel(child, level + 1));
                    }
                    level = tmp;
                }
                else
                {
                    if (node is HumanoidNode)
                    {
                        var hnode = node as HumanoidNode;
                        if (hnode.humanoidIndexes != null && hnode.humanoidIndexes.Length > 0)
                            level++;
                    }
                    else if (node is GenericNode)
                    {
                    }
                    else if (node is BlendShapeNode)
                    {
                        var bnode = node as BlendShapeNode;
                        if (bnode.blendShapeNames != null && bnode.blendShapeNames.Length > 0)
                            level++;
                    }
                }
            }
            return level;
        }
        private void SetChildrenFoldout(BaseNode node, bool foldout)
        {
            node.foldout = foldout;
            if (node.children != null)
            {
                foreach (var child in node.children)
                {
                    SetChildrenFoldout(child, foldout);
                }
            }
        }
        private void SetHumanoidValue(HumanBodyBones humanoidIndex, float value)
        {
            if (poseL.isHuman && poseR.isHuman)
            {
                if (humanoidIndex < 0)
                {
                    #region Root
                    if (humanoidEnablePosition && poseL.haveRootT && poseR.haveRootT)
                    {
                        var blendValue = Vector3.Lerp(poseL.rootT, poseR.rootT, value);
                        if (va.GetAnimationValueAnimatorRootT() != blendValue)
                        {
                            va.SetAnimationValueAnimatorRootT(blendValue);
                        }
                    }
                    if (humanoidEnableRotation && poseL.haveRootQ && poseR.haveRootQ)
                    {
                        var blendValue = Quaternion.Slerp(poseL.rootQ, poseR.rootQ, value);
                        if (va.GetAnimationValueAnimatorRootQ() != blendValue)
                        {
                            va.SetAnimationValueAnimatorRootQ(blendValue);
                        }
                    }
                    #endregion
                }
                else
                {
                    #region Bone
                    if (humanoidEnableRotation && poseIndexTableL.muscleIndexes != null && poseIndexTableR.muscleIndexes != null)
                    {
                        for (int dof = 0; dof < 3; dof++)
                        {
                            var muscleIndex = HumanTrait.MuscleFromBone((int)humanoidIndex, dof);
                            if (muscleIndex < 0) continue;
                            float blendValue = 0f;
                            {
                                var miL = poseIndexTableL.muscleIndexes[muscleIndex];
                                var miR = poseIndexTableR.muscleIndexes[muscleIndex];
                                if (miL < 0 || miR < 0) continue;
                                blendValue = Mathf.Lerp(poseL.muscleValues[miL], poseR.muscleValues[miR], value);
                            }
                            if (va.GetAnimationValueAnimatorMuscle(muscleIndex) != blendValue)
                            {
                                va.SetAnimationValueAnimatorMuscle(muscleIndex, blendValue);
                            }
                        }
                    }
                    if (humanoidEnablePosition && poseL.tdofIndices != null && poseR.tdofIndices != null)
                    {
                        if (VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)humanoidIndex] != null)
                        {
                            var tdofIndex = VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)humanoidIndex].index;
                            if (tdofIndex >= 0 && (int)tdofIndex < poseL.tdofValues.Length && (int)tdofIndex < poseR.tdofValues.Length)
                            {
                                var blendValue = Vector3.Lerp(poseL.tdofValues[(int)tdofIndex], poseR.tdofValues[(int)tdofIndex], value);
                                if (va.GetAnimationValueAnimatorTDOF(tdofIndex) != blendValue)
                                {
                                    va.SetAnimationValueAnimatorTDOF(tdofIndex, blendValue);
                                }
                            }
                        }
                    }
                    #endregion
                }
            }
        }
        private void SetGenericValue(int boneIndex, float value)
        {
            if (boneIndex < 0) return;
            if (va.isHuman && va.humanoidConflict[boneIndex]) return;
            if (va.rootMotionBoneIndex >= 0 && boneIndex == 0) return;
            if (poseIndexTableL.transformIndexes == null || poseIndexTableR.transformIndexes == null) return;
            var miL = poseIndexTableL.transformIndexes[boneIndex];
            var miR = poseIndexTableR.transformIndexes[boneIndex];
            if (miL < 0 || miR < 0) return;

            if (!va.isHuman && va.rootMotionBoneIndex >= 0 && boneIndex == va.rootMotionBoneIndex)
            {
                #region Root
                if (genericEnablePosition && poseL.haveRootT && poseR.haveRootT)
                {
                    var blendValue = Vector3.Lerp(poseL.rootT, poseR.rootT, value);
                    if (va.GetAnimationValueAnimatorRootT() != blendValue)
                    {
                        va.SetAnimationValueAnimatorRootT(blendValue);
                    }
                }
                if (genericEnableRotation && poseL.haveRootQ && poseR.haveRootQ)
                {
                    var blendValue = Quaternion.Slerp(poseL.rootQ, poseR.rootQ, value);
                    if (va.GetAnimationValueAnimatorRootQ() != blendValue)
                    {
                        va.SetAnimationValueAnimatorRootQ(blendValue);
                    }
                }
                #endregion
            }
            else
            {
                #region Transform
                if (genericEnablePosition)
                {
                    var currentValue = va.GetAnimationValueTransformPosition(boneIndex);
                    var blendValue = Vector3.Lerp(poseL.transformValues[miL].position, poseR.transformValues[miR].position, value);
                    if (currentValue != blendValue)
                    {
                        va.SetAnimationValueTransformPosition(boneIndex, blendValue);
                    }
                }
                if (genericEnableRotation)
                {
                    var currentValue = va.GetAnimationValueTransformRotation(boneIndex);
                    var blendValue = Quaternion.Slerp(poseL.transformValues[miL].rotation, poseR.transformValues[miR].rotation, value);
                    if (currentValue != blendValue)
                    {
                        va.SetAnimationValueTransformRotation(boneIndex, blendValue);
                    }
                }
                if (genericEnableScale)
                {
                    var currentValue = va.GetAnimationValueTransformScale(boneIndex);
                    var blendValue = Vector3.Lerp(poseL.transformValues[miL].scale, poseR.transformValues[miR].scale, value);
                    if (currentValue != blendValue)
                    {
                        va.SetAnimationValueTransformScale(boneIndex, blendValue);
                    }
                }
                #endregion
            }
        }
        private void SetBlendShapeValue(SkinnedMeshRenderer renderer, string blendShapeName, float value)
        {
            if (blendShapeEnable && poseL.blendShapePaths != null && poseR.blendShapePaths != null)
            {
                var path = AnimationUtility.CalculateTransformPath(renderer.transform, va.editGameObject.transform);
                var indexL = EditorCommon.ArrayIndexOf(poseL.blendShapePaths, path);
                var indexR = EditorCommon.ArrayIndexOf(poseR.blendShapePaths, path);
                if (indexL >= 0 && indexR >= 0)
                {
                    var nameIndexL = EditorCommon.ArrayIndexOf(poseL.blendShapeValues[indexL].names, blendShapeName);
                    var nameIndexR = EditorCommon.ArrayIndexOf(poseR.blendShapeValues[indexR].names, blendShapeName);
                    if (nameIndexL >= 0 && nameIndexR >= 0)
                    {
                        var blendValue = Mathf.Lerp(poseL.blendShapeValues[indexL].weights[nameIndexL], poseR.blendShapeValues[indexR].weights[nameIndexR], value);
                        if (va.GetAnimationValueBlendShape(renderer, blendShapeName) != blendValue)
                        {
                            va.SetAnimationValueBlendShape(renderer, blendShapeName, blendValue);
                        }
                    }
                }
            }
        }
        private void SetChildrenValue(BaseNode node, float value)
        {
            var valueIndex = blendPoseTreeTable[node];
            blendPoseValues[valueIndex] = value;
            if (node.children != null)
            {
                foreach (var child in node.children)
                {
                    SetChildrenValue(child, value);
                }
            }

            if (node is HumanoidNode)
            {
                var hnode = node as HumanoidNode;
                if (hnode.humanoidIndexes != null && hnode.humanoidIndexes.Length > 0)
                {
                    int rootIndex = -1;
                    for (int i = 0; i < hnode.humanoidIndexes.Length; i++)
                    {
                        blendPoseValues[valueIndex + 1 + i] = value;
                        if (hnode.humanoidIndexes[i] < 0)
                        {
                            rootIndex = i;
                        }
                        else
                        {
                            SetHumanoidValue(hnode.humanoidIndexes[i], value);
                        }
                    }
                    if (rootIndex >= 0)
                    {
                        SetHumanoidValue(hnode.humanoidIndexes[rootIndex], value);
                    }
                }
            }
            else if (node is GenericNode)
            {
                var gnode = node as GenericNode;
                if (gnode.boneIndex > 0)
                {
                    SetGenericValue(gnode.boneIndex, value);
                }
            }
            else if (node is BlendShapeNode)
            {
                var bnode = node as BlendShapeNode;
                if (bnode.blendShapeNames != null && bnode.blendShapeNames.Length > 0)
                {
                    for (int i = 0; i < bnode.blendShapeNames.Length; i++)
                    {
                        blendPoseValues[valueIndex + 1 + i] = value;
                        SetBlendShapeValue(bnode.renderer, bnode.blendShapeNames[i], value);
                    }
                }
            }
            va.SetPoseAfter();
        }
        private void SetSelectionHumanoidValue(float value)
        {
            if (humanoidNode == null) return;
            blendPoseValues[blendPoseTreeTable[humanoidNode]] = value;
            foreach (var humanoidIndex in va.SelectionGameObjectsHumanoidIndex())
            {
                SetHumanoidValue(humanoidIndex, value);
            }
            if (va.SelectionGameObjectsIndexOf(vaw.gameObject) >= 0)
            {
                SetHumanoidValue((HumanBodyBones)(-1), value);
            }
            va.SetPoseAfter();
        }
        private void SetSelectionGenericValue(float value)
        {
            if (genericNode == null) return;
            blendPoseValues[blendPoseTreeTable[genericNode]] = value;
            foreach (var boneIndex in va.selectionBones)
            {
                SetGenericValue(boneIndex, value);
            }
            va.SetPoseAfter();
        }
        private void SetSelectionBlendShapeValue(float value)
        {
            if (blendShapeNode == null) return;
            blendPoseValues[blendPoseTreeTable[blendShapeNode]] = value;
            foreach (var boneIndex in va.selectionBones)
            {
                var renderer = va.editBones[boneIndex].GetComponentInChildren<SkinnedMeshRenderer>(true);
                if (renderer == null || renderer.sharedMesh == null || renderer.sharedMesh.blendShapeCount <= 0) continue;
                foreach (var child in blendShapeNode.children)
                {
                    if (!(child is BlendShapeNode)) continue;
                    var bnode = child as BlendShapeNode;
                    if (bnode.renderer != renderer) continue;
                    foreach (var name in bnode.blendShapeNames)
                    {
                        SetBlendShapeValue(renderer, name, value);
                    }
                }
            }
            va.SetPoseAfter();
        }
        #endregion

        private void UpdateNode()
        {
            #region Humanoid
            humanoidNode = null;
            if (IsPoseReady && va.isHuman)
            {
                var children = new List<HumanoidNode>();
                Func<HumanBodyBones[], HumanBodyBones[]> GetContainsList = (src) =>
                {
                    Func<PoseTemplate, HumanBodyBones, bool> IsHumanoidIndexContains = (pose, hi) =>
                    {
                        if (pose.isHuman)
                        {
                            if (hi < 0)
                            {
                                if (pose.haveRootT || pose.haveRootQ)
                                    return true;
                            }
                            else
                            {
                                for (int dof = 0; dof < 3; dof++)
                                {
                                    var muscleIndex = HumanTrait.MuscleFromBone((int)hi, dof);
                                    if (muscleIndex < 0) continue;
                                    if (EditorCommon.ArrayContains(pose.musclePropertyNames, va.musclePropertyName.PropertyNames[muscleIndex]))
                                        return true;
                                }
                                if (VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)hi] != null)
                                {
                                    var tdofIndex = VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)hi].index;
                                    if (tdofIndex >= 0)
                                    {
                                        if (EditorCommon.ArrayContains(pose.tdofIndices, tdofIndex))
                                            return true;
                                    }
                                }
                            }
                        }
                        return false;
                    };

                    var dst = new List<HumanBodyBones>();
                    foreach (var hi in src)
                    {
                        if (hi >= 0 && va.humanoidBones[(int)hi] == null && VeryAnimation.HumanVirtualBones[(int)hi] == null)
                            continue;
                        if (!IsHumanoidIndexContains(poseL, hi) || !IsHumanoidIndexContains(poseR, hi))
                            continue;
                        dst.Add(hi);
                    }
                    return dst.ToArray();
                };

                #region Head
                {
                    var node = new HumanoidNode()
                    {
                        name = "Head",
                        humanoidIndexes = GetContainsList(new HumanBodyBones[]
                        {
                            HumanBodyBones.Neck,
                            HumanBodyBones.Head,
                            HumanBodyBones.LeftEye,
                            HumanBodyBones.RightEye,
                            HumanBodyBones.Jaw,
                        }),
                    };
                    if (node.humanoidIndexes.Length > 0)
                        children.Add(node);
                }
                #endregion
                #region Body
                {
                    var node = new HumanoidNode()
                    {
                        name = "Body",
                        humanoidIndexes = GetContainsList(new HumanBodyBones[]
                        {
                            HumanBodyBones.Spine,
                            HumanBodyBones.Chest,
                            HumanBodyBones.UpperChest,
                        }),
                    };
                    if (node.humanoidIndexes.Length > 0)
                        children.Add(node);
                }
                #endregion
                #region Left Arm
                {
                    var node = new HumanoidNode()
                    {
                        name = "Left Arm",
                        humanoidIndexes = GetContainsList(new HumanBodyBones[]
                        {
                            HumanBodyBones.LeftShoulder,
                            HumanBodyBones.LeftUpperArm,
                            HumanBodyBones.LeftLowerArm,
                            HumanBodyBones.LeftHand,
                        }),
                    };
                    if (node.humanoidIndexes.Length > 0)
                        children.Add(node);
                }
                #endregion
                #region Right Arm
                {
                    var node = new HumanoidNode()
                    {
                        name = "Right Arm",
                        humanoidIndexes = GetContainsList(new HumanBodyBones[]
                        {
                            HumanBodyBones.RightShoulder,
                            HumanBodyBones.RightUpperArm,
                            HumanBodyBones.RightLowerArm,
                            HumanBodyBones.RightHand,
                        }),
                    };
                    if (node.humanoidIndexes.Length > 0)
                        children.Add(node);
                }
                #endregion
                #region Left Leg
                {
                    var node = new HumanoidNode()
                    {
                        name = "Left Leg",
                        humanoidIndexes = GetContainsList(new HumanBodyBones[]
                        {
                            HumanBodyBones.LeftUpperLeg,
                            HumanBodyBones.LeftLowerLeg,
                            HumanBodyBones.LeftFoot,
                            HumanBodyBones.LeftToes,
                        }),
                    };
                    if (node.humanoidIndexes.Length > 0)
                        children.Add(node);
                }
                #endregion
                #region Right Leg
                {
                    var node = new HumanoidNode()
                    {
                        name = "Right Leg",
                        humanoidIndexes = GetContainsList(new HumanBodyBones[]
                        {
                            HumanBodyBones.RightUpperLeg,
                            HumanBodyBones.RightLowerLeg,
                            HumanBodyBones.RightFoot,
                            HumanBodyBones.RightToes,
                        }),
                    };
                    if (node.humanoidIndexes.Length > 0)
                        children.Add(node);
                }
                #endregion
                #region Left Finger
                if (va.humanoidHasLeftHand)
                {
                    var node = new HumanoidNode()
                    {
                        name = "Left Finger",
                        children = new HumanoidNode[]
                        {
                            new HumanoidNode()
                            {
                                name = "Left Thumb",
                                humanoidIndexes = GetContainsList(new HumanBodyBones[]
                                {
                                    HumanBodyBones.LeftThumbProximal,
                                    HumanBodyBones.LeftThumbIntermediate,
                                    HumanBodyBones.LeftThumbDistal,
                                }),
                            },
                            new HumanoidNode()
                            {
                                name = "Left Index",
                                humanoidIndexes = GetContainsList(new HumanBodyBones[]
                                {
                                    HumanBodyBones.LeftIndexProximal,
                                    HumanBodyBones.LeftIndexIntermediate,
                                    HumanBodyBones.LeftIndexDistal,
                                }),
                            },
                            new HumanoidNode()
                            {
                                name = "Left Middle",
                                humanoidIndexes = GetContainsList(new HumanBodyBones[]
                                {
                                    HumanBodyBones.LeftMiddleProximal,
                                    HumanBodyBones.LeftMiddleIntermediate,
                                    HumanBodyBones.LeftMiddleDistal,
                                }),
                            },
                            new HumanoidNode()
                            {
                                name = "Left Ring",
                                humanoidIndexes = GetContainsList(new HumanBodyBones[]
                                {
                                    HumanBodyBones.LeftRingProximal,
                                    HumanBodyBones.LeftRingIntermediate,
                                    HumanBodyBones.LeftRingDistal,
                                }),
                            },
                            new HumanoidNode()
                            {
                                name = "Left Little",
                                humanoidIndexes = GetContainsList(new HumanBodyBones[]
                                {
                                    HumanBodyBones.LeftLittleProximal,
                                    HumanBodyBones.LeftLittleIntermediate,
                                    HumanBodyBones.LeftLittleDistal,
                                }),
                            },
                        },
                    };
                    children.Add(node);
                }
                #endregion
                #region Right Finger
                if (va.humanoidHasRightHand)
                {
                    var node = new HumanoidNode()
                    {
                        name = "Right Finger",
                        children = new HumanoidNode[]
                        {
                            new HumanoidNode()
                            {
                                name = "Right Thumb",
                                humanoidIndexes = GetContainsList(new HumanBodyBones[]
                                {
                                    HumanBodyBones.RightThumbProximal,
                                    HumanBodyBones.RightThumbIntermediate,
                                    HumanBodyBones.RightThumbDistal,
                                }),
                            },
                            new HumanoidNode()
                            {
                                name = "Right Index",
                                humanoidIndexes = GetContainsList(new HumanBodyBones[]
                                {
                                    HumanBodyBones.RightIndexProximal,
                                    HumanBodyBones.RightIndexIntermediate,
                                    HumanBodyBones.RightIndexDistal,
                                }),
                            },
                            new HumanoidNode()
                            {
                                name = "Right Middle",
                                humanoidIndexes = GetContainsList(new HumanBodyBones[]
                                {
                                    HumanBodyBones.RightMiddleProximal,
                                    HumanBodyBones.RightMiddleIntermediate,
                                    HumanBodyBones.RightMiddleDistal,
                                }),
                            },
                            new HumanoidNode()
                            {
                                name = "Right Ring",
                                humanoidIndexes = GetContainsList(new HumanBodyBones[]
                                {
                                    HumanBodyBones.RightRingProximal,
                                    HumanBodyBones.RightRingIntermediate,
                                    HumanBodyBones.RightRingDistal,
                                }),
                            },
                            new HumanoidNode()
                            {
                                name = "Right Little",
                                humanoidIndexes = GetContainsList(new HumanBodyBones[]
                                {
                                    HumanBodyBones.RightLittleProximal,
                                    HumanBodyBones.RightLittleIntermediate,
                                    HumanBodyBones.RightLittleDistal,
                                }),
                            },
                        },
                    };
                    children.Add(node);
                }
                #endregion

                humanoidNode = new HumanoidNode()
                {
                    name = "Humanoid",
                    children = children.ToArray(),
                    humanoidIndexes = GetContainsList(new HumanBodyBones[]
                    {
                        (HumanBodyBones)(-1),
                    }),
                };
            }
            #endregion

            #region Generic
            genericNode = null;
            if (IsPoseReady)
            {
                Func<Transform, GenericNode> AddBone = null;
                AddBone = (t) =>
                {
                    var boneIndex = va.BonesIndexOf(t.gameObject);
                    if (boneIndex < 0)
                        return null;
                    if (!EditorCommon.ArrayContains(poseL.transformPaths, va.bonePaths[boneIndex]))
                        return null;
                    if (!EditorCommon.ArrayContains(poseR.transformPaths, va.bonePaths[boneIndex]))
                        return null;

                    var children = new List<GenericNode>();
                    for (int i = 0; i < t.childCount; i++)
                    {
                        var child = AddBone(t.GetChild(i));
                        if (child != null)
                            children.Add(child);
                    }
                    var node = new GenericNode()
                    {
                        name = t.name,
                        children = children.ToArray(),
                        boneIndex = boneIndex,
                    };
                    return node;
                };

                {
                    var children = new List<GenericNode>();
                    for (int i = 0; i < vaw.gameObject.transform.childCount; i++)
                    {
                        var child = AddBone(vaw.gameObject.transform.GetChild(i));
                        if (child != null)
                            children.Add(child);
                    }
                    if (children.Count > 0)
                    {
                        genericNode = new GenericNode()
                        {
                            name = vaw.gameObject.name,
                            children = children.ToArray(),
                            boneIndex = va.BonesIndexOf(vaw.gameObject),
                        };
                    }
                }
            }
            #endregion

            #region BlendShape
            blendShapeNode = null;
            if (IsPoseReady)
            {
                var children = new List<BlendShapeNode>();
                if (poseL.blendShapePaths != null && poseR.blendShapePaths != null)
                {
                    foreach (var renderer in va.editGameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                    {
                        if (renderer == null || renderer.sharedMesh == null || renderer.sharedMesh.blendShapeCount <= 0) continue;
                        var path = AnimationUtility.CalculateTransformPath(renderer.transform, va.editGameObject.transform);
                        var indexL = EditorCommon.ArrayIndexOf(poseL.blendShapePaths, path);
                        var indexR = EditorCommon.ArrayIndexOf(poseR.blendShapePaths, path);
                        if (indexL < 0 || indexR < 0) continue;
                        var names = new List<string>();
                        for (int i = 0; i < poseL.blendShapeValues[indexL].names.Length; i++)
                        {
                            if (!EditorCommon.ArrayContains(poseR.blendShapeValues[indexR].names, poseL.blendShapeValues[indexL].names[i]))
                                continue;
                            if (!va.blendShapeWeightSave.IsHaveOriginalWeight(renderer, poseL.blendShapeValues[indexL].names[i]))
                                continue;
                            names.Add(poseL.blendShapeValues[indexL].names[i]);
                        }
                        if (names.Count > 0)
                        {
                            children.Add(new BlendShapeNode()
                            {
                                name = renderer.name,
                                renderer = renderer,
                                blendShapeNames = names.ToArray(),
                            });
                        }
                    }
                }
                if (children.Count > 0)
                {
                    blendShapeNode = new BlendShapeNode()
                    {
                        name = "Blend Shape",
                        children = children.ToArray(),
                    };
                }
            }
            #endregion

            #region Root
            {
                var rootChildren = new List<BaseNode>();
                if (humanoidNode != null) rootChildren.Add(humanoidNode);
                if (genericNode != null) rootChildren.Add(genericNode);
                if (blendShapeNode != null) rootChildren.Add(blendShapeNode);
                rootNode = new BaseNode()
                {
                    name = "Root",
                    children = rootChildren.ToArray(),
                };
            }
            #endregion

            #region Values
            {
                blendPoseTreeTable = new Dictionary<BaseNode, int>();
                int counter = 0;
                Action<BaseNode> AddTable = null;
                AddTable = (node) =>
                {
                    blendPoseTreeTable.Add(node, counter++);
                    if (node is HumanoidNode)
                    {
                        var hnode = node as HumanoidNode;
                        if (hnode.humanoidIndexes != null)
                        {
                            counter += hnode.humanoidIndexes.Length;
                        }
                    }
                    else if (node is GenericNode)
                    {
                    }
                    else if (node is BlendShapeNode)
                    {
                        var bnode = node as BlendShapeNode;
                        if (bnode.blendShapeNames != null)
                        {
                            counter += bnode.blendShapeNames.Length;
                        }
                    }
                    if (node.children != null)
                    {
                        foreach (var child in node.children)
                        {
                            AddTable(child);
                        }
                    }
                };
                AddTable(rootNode);

                blendPoseValues = new float[counter];
            }
            #endregion

            #region PoseIndexTable
            poseIndexTableL = poseIndexTableR = null;
            if (IsPoseReady)
            {
                Func<PoseTemplate, PoseIndexTable> CreatePoseIndexTable = (pose) =>
                {
                    var indexTable = new PoseIndexTable();

                    if (pose.musclePropertyNames != null && pose.muscleValues!= null)
                    {
                        indexTable.muscleIndexes = new int[va.musclePropertyName.PropertyNames.Length];
                        for (int i = 0; i < va.musclePropertyName.PropertyNames.Length; i++)
                            indexTable.muscleIndexes[i] = EditorCommon.ArrayIndexOf(pose.musclePropertyNames, va.musclePropertyName.PropertyNames[i]);
                    }
                    if (pose.transformPaths != null && pose.transformValues != null)
                    {
                        indexTable.transformIndexes = new int[va.bonePaths.Length];
                        for (int i = 0; i < va.bonePaths.Length; i++)
                            indexTable.transformIndexes[i] = EditorCommon.ArrayIndexOf(pose.transformPaths, va.bonePaths[i]);
                    }
                    return indexTable;
                };
                poseIndexTableL = CreatePoseIndexTable(poseL);
                poseIndexTableR = CreatePoseIndexTable(poseR);
            }
            #endregion
        }

        private void UpdateEditModeString()
        {
            for (int i = 0; i < (int)EditMode.Total; i++)
            {
                EditModeString[i] = Language.GetContent(Language.Help.BlendPoseModeTree + i);
            }
        }
    }
}
