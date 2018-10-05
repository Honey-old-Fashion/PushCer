using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace VeryAnimation
{
    public class EditorSettings
    {
        private VeryAnimation va { get { return VeryAnimation.instance; } }

        public Language.LanguageType settingLanguageType { get; private set; }
        public bool settingComponentSaveSettings { get; private set; }
        public float settingBoneButtonSize { get; private set; }
        public Color settingBoneNormalColor { get; private set; }
        public Color settingBoneActiveColor { get; private set; }
        public bool settingBoneMuscleLimit { get; private set; }
        public enum SkeletonType
        {
            None,
            Line,
            Lines,
            Mesh,
        }
        private static readonly string[] SkeletonTypeString =
        {
            SkeletonType.None.ToString(),
            SkeletonType.Line.ToString(),
            SkeletonType.Lines.ToString(),
            SkeletonType.Mesh.ToString(),
        };
        public enum DummyPositionType
        {
            ScenePosition,
            TimelinePosition,
        }
        private static readonly GUIContent[] DummyPositionTypeString =
        {
            new GUIContent("Scene", "Scene Position"),
            new GUIContent("Timeline", "Timeline Position"),
        };
        public SkeletonType settingsSkeletonType { get; private set; }
        public Color settingSkeletonColor { get; private set; }
        public float settingIKTargetSize { get; private set; }
        public Color settingIKTargetNormalColor { get; private set; }
        public Color settingIKTargetActiveColor { get; private set; }
        public float settingEditorSliderSize { get; private set; }
        public bool settingHierarchyExpandSelectObject { get; private set; }
        public bool settingGenericMirrorName { get; private set; }
        public string settingGenericMirrorNameDifferentCharacters { get; private set; }
        public bool settingGenericMirrorNameIgnoreCharacter { get; private set; }
        public string settingGenericMirrorNameIgnoreCharacterString { get; private set; }
        public bool settingGenericMirrorPosition { get; private set; }
        public float settingGenericMirrorPositionThreshold { get; private set; }
        public bool settingBlendShapeMirrorName { get; private set; }
        public string settingBlendShapeMirrorNameDifferentCharacters { get; private set; }
        public Color settingDummyObjectColor { get; private set; }
        public DummyPositionType settingDummyPositionType { get; private set; }
        public Vector3 settingDummyObjectPosition { get; private set; }

        private bool componentFoldout;
        private bool boneFoldout;
        private bool skeletonFoldout;
        private bool ikFoldout;
        private bool editorFoldout;
        private bool hierarchyFoldout;
        private bool genericFoldout;
        private bool blendShapeFoldout;
        private bool dummyObjectFoldout;

        public EditorSettings()
        {
            settingLanguageType = (Language.LanguageType)EditorPrefs.GetInt("VeryAnimation_LanguageType", 0);
            settingComponentSaveSettings = EditorPrefs.GetBool("VeryAnimation_ComponentSaveSettings", true);
            settingBoneButtonSize = EditorPrefs.GetFloat("VeryAnimation_BoneButtonSize", 16f);
            settingBoneNormalColor = GetEditorPrefsColor("VeryAnimation_BoneNormalColor", Color.white);
            settingBoneActiveColor = GetEditorPrefsColor("VeryAnimation_BoneActiveColor", Color.yellow);
            settingBoneMuscleLimit = EditorPrefs.GetBool("VeryAnimation_BoneMuscleLimit", true);
            settingsSkeletonType = (SkeletonType)EditorPrefs.GetInt("VeryAnimation_SkeletonType", (int)SkeletonType.Lines);
            settingSkeletonColor = GetEditorPrefsColor("VeryAnimation_SkeletonColor", Color.green);
            settingIKTargetSize = EditorPrefs.GetFloat("VeryAnimation_IKTargetSize", 0.15f);
            settingIKTargetNormalColor = GetEditorPrefsColor("VeryAnimation_IKTargetNormalColor", new Color(1f, 1f, 1f, 0.5f));
            settingIKTargetActiveColor = GetEditorPrefsColor("VeryAnimation_IKTargetActiveColor", new Color(1f, 0.92f, 0.016f, 0.5f));
            settingEditorSliderSize = EditorPrefs.GetFloat("VeryAnimation_EditorSliderSize", 100f);
            settingHierarchyExpandSelectObject = EditorPrefs.GetBool("VeryAnimation_HierarchyExpandSelectObject", true);
            settingGenericMirrorName = EditorPrefs.GetBool("VeryAnimation_GenericMirrorName", true);
            settingGenericMirrorNameDifferentCharacters = EditorPrefs.GetString("VeryAnimation_GenericMirrorNameDifferentCharacters", "Left,Right,Hidari,Migi,L,R");
            settingGenericMirrorNameIgnoreCharacter = EditorPrefs.GetBool("VeryAnimation_GenericMirrorNameIgnoreCharacter", false);
            settingGenericMirrorNameIgnoreCharacterString = EditorPrefs.GetString("VeryAnimation_GenericMirrorNameIgnoreCharacterString", ".");
            settingGenericMirrorPosition = EditorPrefs.GetBool("VeryAnimation_GenericMirrorPosition", true);
            settingGenericMirrorPositionThreshold = EditorPrefs.GetFloat("VeryAnimation_GenericMirrorPositionThreshold", 0.01f);
            settingBlendShapeMirrorName = EditorPrefs.GetBool("VeryAnimation_BlendShapeMirrorName", true);
            settingBlendShapeMirrorNameDifferentCharacters = EditorPrefs.GetString("VeryAnimation_BlendShapeMirrorNameDifferentCharacters", "Left,Right,Hidari,Migi,L,R");
            settingDummyObjectColor = GetEditorPrefsColor("VeryAnimation_DummyObjectColor", Color.gray);
            settingDummyPositionType = (DummyPositionType)EditorPrefs.GetInt("VeryAnimation_DummyPositionType", (int)DummyPositionType.ScenePosition);
            settingDummyObjectPosition = GetEditorPrefsVector3("VeryAnimation_DummyObjectPosition", Vector3.zero);

            Language.SetLanguage(settingLanguageType);
        }
        public void Reset()
        {
            EditorPrefs.SetInt("VeryAnimation_LanguageType", (int)(settingLanguageType = (Language.LanguageType)0));
            EditorPrefs.SetBool("VeryAnimation_ComponentSaveSettings", settingComponentSaveSettings = true);
            EditorPrefs.SetFloat("VeryAnimation_BoneButtonSize", settingBoneButtonSize = 16f);
            SetEditorPrefsColor("VeryAnimation_BoneNormalColor", settingBoneNormalColor = Color.white);
            SetEditorPrefsColor("VeryAnimation_BoneActiveColor", settingBoneActiveColor = Color.yellow);
            EditorPrefs.SetBool("VeryAnimation_BoneMuscleLimit", settingBoneMuscleLimit = true);
            EditorPrefs.SetInt("VeryAnimation_SkeletonType", (int)(settingsSkeletonType = SkeletonType.Lines));
            SetEditorPrefsColor("VeryAnimation_SkeletonColor", settingSkeletonColor = Color.green);
            EditorPrefs.SetFloat("VeryAnimation_IKTargetSize", settingIKTargetSize = 0.15f);
            SetEditorPrefsColor("VeryAnimation_IKTargetNormalColor", settingIKTargetNormalColor = new Color(1f, 1f, 1f, 0.5f));
            SetEditorPrefsColor("VeryAnimation_IKTargetActiveColor", settingIKTargetActiveColor = new Color(1f, 0.92f, 0.016f, 0.5f));
            EditorPrefs.SetFloat("VeryAnimation_EditorSliderSize", settingEditorSliderSize = 100f);
            EditorPrefs.SetBool("VeryAnimation_HierarchyExpandSelectObject", settingHierarchyExpandSelectObject = true);
            EditorPrefs.SetBool("VeryAnimation_GenericMirrorName", settingGenericMirrorName = true);
            EditorPrefs.SetString("VeryAnimation_GenericMirrorNameDifferentCharacters", settingGenericMirrorNameDifferentCharacters = "Left,Right,Hidari,Migi,L,R");
            EditorPrefs.SetBool("VeryAnimation_GenericMirrorNameIgnoreCharacter", settingGenericMirrorNameIgnoreCharacter = false);
            EditorPrefs.SetString("VeryAnimation_GenericMirrorNameIgnoreCharacterString", settingGenericMirrorNameIgnoreCharacterString = ".");
            EditorPrefs.SetBool("VeryAnimation_GenericMirrorPosition", settingGenericMirrorPosition = true);
            EditorPrefs.SetFloat("VeryAnimation_GenericMirrorPositionThreshold", settingGenericMirrorPositionThreshold = 0.01f);
            EditorPrefs.SetBool("VeryAnimation_BlendShapeMirrorName", settingBlendShapeMirrorName = true);
            EditorPrefs.SetString("VeryAnimation_BlendShapeMirrorNameDifferentCharacters", settingBlendShapeMirrorNameDifferentCharacters = "Left,Right,Hidari,Migi,L,R");
            SetEditorPrefsColor("VeryAnimation_DummyObjectColor", settingDummyObjectColor = Color.gray);
            EditorPrefs.SetInt("VeryAnimation_DummyPositionType", (int)(settingDummyPositionType = DummyPositionType.ScenePosition));
            SetEditorPrefsVector3("VeryAnimation_DummyObjectPosition", settingDummyObjectPosition = Vector3.zero);

            Language.SetLanguage(settingLanguageType);
            va.UpdateBonesMirror();
            va.UpdateBlendShapeMirror();
            va.SetUpdateResampleAnimation();
            if (va.dummyObject != null)
                va.dummyObject.SetColor(settingDummyObjectColor);
            InternalEditorUtility.RepaintAllViews();
        }

        public void SettingsGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Language");
                    settingLanguageType = (Language.LanguageType)GUILayout.Toolbar((int)settingLanguageType, Language.LanguageTypeString);
                    EditorGUILayout.EndHorizontal();
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetInt("VeryAnimation_LanguageType", (int)(settingLanguageType));
                        Language.SetLanguage(settingLanguageType);
                        InternalEditorUtility.RepaintAllViews();
                    }
                }
                componentFoldout = EditorGUILayout.Foldout(componentFoldout, "Component", true);
                if (componentFoldout)
                {
                    EditorGUI.indentLevel++;
                    {
                        #region settingComponentSaveSettings
                        {
                            EditorGUI.BeginChangeCheck();
                            settingComponentSaveSettings = EditorGUILayout.Toggle(new GUIContent("Save Settings", "Save the 'VeryAnimationSaveSettings' component to the root game object."), settingComponentSaveSettings);
                            if (EditorGUI.EndChangeCheck())
                            {
                                EditorPrefs.SetBool("VeryAnimation_ComponentSaveSettings", settingComponentSaveSettings);
                            }
                        }
                        #endregion
                    }
                    EditorGUI.indentLevel--;
                }
                boneFoldout = EditorGUILayout.Foldout(boneFoldout, "Bone", true);
                if (boneFoldout)
                {
                    EditorGUI.indentLevel++;
                    {
                        #region Button Size
                        {
                            EditorGUI.BeginChangeCheck();
                            settingBoneButtonSize = EditorGUILayout.Slider("Button Size", settingBoneButtonSize, 1f, 32f);
                            if (EditorGUI.EndChangeCheck())
                            {
                                EditorPrefs.SetFloat("VeryAnimation_BoneButtonSize", settingBoneButtonSize);
                                InternalEditorUtility.RepaintAllViews();
                            }
                        }
                        #endregion
                        #region Button Normal Color
                        {
                            EditorGUI.BeginChangeCheck();
                            settingBoneNormalColor = EditorGUILayout.ColorField("Button Normal Color", settingBoneNormalColor);
                            if (EditorGUI.EndChangeCheck())
                            {
                                SetEditorPrefsColor("VeryAnimation_BoneNormalColor", settingBoneNormalColor);
                                InternalEditorUtility.RepaintAllViews();
                            }
                        }
                        #endregion
                        #region Button Active Color
                        {
                            EditorGUI.BeginChangeCheck();
                            settingBoneActiveColor = EditorGUILayout.ColorField("Button Active Color", settingBoneActiveColor);
                            if (EditorGUI.EndChangeCheck())
                            {
                                SetEditorPrefsColor("VeryAnimation_BoneActiveColor", settingBoneActiveColor);
                                InternalEditorUtility.RepaintAllViews();
                            }
                        }
                        #endregion
                        #region MuscleLimit
                        {
                            EditorGUI.BeginChangeCheck();
                            settingBoneMuscleLimit = EditorGUILayout.Toggle("Muscle Limit Gizmo", settingBoneMuscleLimit);
                            if (EditorGUI.EndChangeCheck())
                            {
                                EditorPrefs.SetBool("VeryAnimation_BoneMuscleLimit", settingBoneMuscleLimit);
                                InternalEditorUtility.RepaintAllViews();
                            }
                        }
                        #endregion
                    }
                    EditorGUI.indentLevel--;
                }
                skeletonFoldout = EditorGUILayout.Foldout(skeletonFoldout, "Skeleton", true);
                if (skeletonFoldout)
                {
                    EditorGUI.indentLevel++;
                    {
                        #region SkeletonType
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.PrefixLabel("Preview Type");
                            settingsSkeletonType = (SkeletonType)GUILayout.Toolbar((int)settingsSkeletonType, SkeletonTypeString);
                            EditorGUILayout.EndHorizontal();
                            if (EditorGUI.EndChangeCheck())
                            {
                                EditorPrefs.SetInt("VeryAnimation_SkeletonType", (int)settingsSkeletonType);
                                InternalEditorUtility.RepaintAllViews();
                            }
                        }
                        #endregion
                        #region Skeleton Color
                        {
                            EditorGUI.BeginChangeCheck();
                            settingSkeletonColor = EditorGUILayout.ColorField("Preview Color", settingSkeletonColor);
                            if (EditorGUI.EndChangeCheck())
                            {
                                SetEditorPrefsColor("VeryAnimation_SkeletonColor", settingSkeletonColor);
                                InternalEditorUtility.RepaintAllViews();
                            }
                        }
                        #endregion
                    }
                    EditorGUI.indentLevel--;
                }
                ikFoldout = EditorGUILayout.Foldout(ikFoldout, "IK", true);
                if (ikFoldout)
                {
                    EditorGUI.indentLevel++;
                    {
                        #region IK Target Size
                        {
                            EditorGUI.BeginChangeCheck();
                            settingIKTargetSize = EditorGUILayout.Slider("Button Size", settingIKTargetSize, 0.01f, 1f);
                            if (EditorGUI.EndChangeCheck())
                            {
                                EditorPrefs.SetFloat("VeryAnimation_IKTargetSize", settingIKTargetSize);
                                InternalEditorUtility.RepaintAllViews();
                            }
                        }
                        #endregion
                        #region IK Target Normal Color
                        {
                            EditorGUI.BeginChangeCheck();
                            settingIKTargetNormalColor = EditorGUILayout.ColorField("Button Normal Color", settingIKTargetNormalColor);
                            if (EditorGUI.EndChangeCheck())
                            {
                                SetEditorPrefsColor("VeryAnimation_IKTargetNormalColor", settingIKTargetNormalColor);
                                InternalEditorUtility.RepaintAllViews();
                            }
                        }
                        #endregion
                        #region IK Target Active Color
                        {
                            EditorGUI.BeginChangeCheck();
                            settingIKTargetActiveColor = EditorGUILayout.ColorField("Button Active Color", settingIKTargetActiveColor);
                            if (EditorGUI.EndChangeCheck())
                            {
                                SetEditorPrefsColor("VeryAnimation_IKTargetActiveColor", settingIKTargetActiveColor);
                                InternalEditorUtility.RepaintAllViews();
                            }
                        }
                        #endregion
                    }
                    EditorGUI.indentLevel--;
                }
                editorFoldout = EditorGUILayout.Foldout(editorFoldout, "Editor", true);
                if (editorFoldout)
                {
                    EditorGUI.indentLevel++;
                    {
                        #region SliderSize
                        {
                            EditorGUI.BeginChangeCheck();
                            settingEditorSliderSize = EditorGUILayout.Slider("Slider Size", settingEditorSliderSize, 50f, 500f);
                            if (EditorGUI.EndChangeCheck())
                            {
                                EditorPrefs.SetFloat("VeryAnimation_EditorSliderSize", settingEditorSliderSize);
                                InternalEditorUtility.RepaintAllViews();
                            }
                        }
                        #endregion
                    }
                    EditorGUI.indentLevel--;
                }
                hierarchyFoldout = EditorGUILayout.Foldout(hierarchyFoldout, "Hierarchy", true);
                if (hierarchyFoldout)
                {
                    EditorGUI.indentLevel++;
                    {
                        #region ExpandSelectObject
                        {
                            EditorGUI.BeginChangeCheck();
                            settingHierarchyExpandSelectObject = EditorGUILayout.Toggle(new GUIContent("Expand select object"), settingHierarchyExpandSelectObject);
                            if (EditorGUI.EndChangeCheck())
                            {
                                EditorPrefs.SetBool("VeryAnimation_HierarchyExpandSelectObject", settingHierarchyExpandSelectObject);
                            }
                        }
                        #endregion
                    }
                    EditorGUI.indentLevel--;
                }
                genericFoldout = EditorGUILayout.Foldout(genericFoldout, "Generic", true);
                if (genericFoldout)
                {
                    EditorGUI.indentLevel++;
                    {
                        EditorGUILayout.LabelField("Mirror");
                        EditorGUI.indentLevel++;
                        {
                            #region settingGenericMirrorName
                            {
                                EditorGUI.BeginChangeCheck();
                                settingGenericMirrorName = EditorGUILayout.Toggle(new GUIContent("Search by Name", "Search for mirror targets by name."), settingGenericMirrorName);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    EditorPrefs.SetBool("VeryAnimation_GenericMirrorName", settingGenericMirrorName);
                                    va.UpdateBonesMirror();
                                    InternalEditorUtility.RepaintAllViews();
                                }
                                if (settingGenericMirrorName)
                                {
                                    EditorGUI.indentLevel++;
                                    #region settingGenericMirrorNameDifferentCharacters
                                    {
                                        EditorGUI.BeginChangeCheck();
                                        settingGenericMirrorNameDifferentCharacters = EditorGUILayout.TextField(new GUIContent("Characters", "Different Characters"), settingGenericMirrorNameDifferentCharacters);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            EditorPrefs.SetString("VeryAnimation_GenericMirrorNameDifferentCharacters", settingGenericMirrorNameDifferentCharacters);
                                            va.UpdateBonesMirror();
                                            InternalEditorUtility.RepaintAllViews();
                                        }
                                    }
                                    #endregion
                                    #region settingGenericMirrorNameIgnoreCharacter
                                    {
                                        EditorGUILayout.BeginHorizontal();
                                        {
                                            EditorGUI.BeginChangeCheck();
                                            settingGenericMirrorNameIgnoreCharacter = EditorGUILayout.ToggleLeft("Ignore up to the specified character", settingGenericMirrorNameIgnoreCharacter);
                                            if (EditorGUI.EndChangeCheck())
                                            {
                                                EditorPrefs.SetBool("VeryAnimation_GenericMirrorNameIgnoreCharacter", settingGenericMirrorNameIgnoreCharacter);
                                                va.UpdateBonesMirror();
                                                InternalEditorUtility.RepaintAllViews();
                                            }
                                        }
                                        if (settingGenericMirrorNameIgnoreCharacter)
                                        {
                                            EditorGUI.BeginChangeCheck();
                                            settingGenericMirrorNameIgnoreCharacterString = EditorGUILayout.TextField(settingGenericMirrorNameIgnoreCharacterString, GUILayout.Width(100));
                                            if (EditorGUI.EndChangeCheck())
                                            {
                                                EditorPrefs.SetString("VeryAnimation_GenericMirrorNameIgnoreCharacterString", settingGenericMirrorNameIgnoreCharacterString);
                                                va.UpdateBonesMirror();
                                                InternalEditorUtility.RepaintAllViews();
                                            }
                                        }
                                        EditorGUILayout.EndHorizontal();
                                    }
                                    #endregion
                                    EditorGUI.indentLevel--;
                                }
                            }
                            #endregion
                            #region settingGenericMirrorPosition
                            {
                                EditorGUI.BeginChangeCheck();
                                settingGenericMirrorPosition = EditorGUILayout.Toggle(new GUIContent("Search by Position", "Search for mirror targets by position."), settingGenericMirrorPosition);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    EditorPrefs.SetBool("VeryAnimation_GenericMirrorPosition", settingGenericMirrorPosition);
                                    va.UpdateBonesMirror();
                                    InternalEditorUtility.RepaintAllViews();
                                }
                                if (settingGenericMirrorPosition)
                                {
                                    EditorGUI.indentLevel++;
                                    #region settingGenericMirrorPositionThreshold
                                    {
                                        EditorGUI.BeginChangeCheck();
                                        settingGenericMirrorPositionThreshold = EditorGUILayout.Slider("Threshold", settingGenericMirrorPositionThreshold, 0f, 1f);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            EditorPrefs.SetFloat("VeryAnimation_GenericMirrorPositionThreshold", settingGenericMirrorPositionThreshold);
                                            va.UpdateBonesMirror();
                                            InternalEditorUtility.RepaintAllViews();
                                        }
                                    }
                                    #endregion
                                    EditorGUI.indentLevel--;
                                }
                            }
                            #endregion
                        }
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }
                blendShapeFoldout = EditorGUILayout.Foldout(blendShapeFoldout, "Blend Shape", true);
                if (blendShapeFoldout)
                {
                    EditorGUI.indentLevel++;
                    {
                        EditorGUILayout.LabelField("Mirror");
                        EditorGUI.indentLevel++;
                        {
                            #region settingBlendShapeMirrorName
                            {
                                EditorGUI.BeginChangeCheck();
                                settingBlendShapeMirrorName = EditorGUILayout.Toggle(new GUIContent("Search by Name", "Search for mirror targets by name."), settingBlendShapeMirrorName);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    EditorPrefs.SetBool("VeryAnimation_BlendShapeMirrorName", settingBlendShapeMirrorName);
                                    va.UpdateBlendShapeMirror();
                                    InternalEditorUtility.RepaintAllViews();
                                }
                                if (settingBlendShapeMirrorName)
                                {
                                    EditorGUI.indentLevel++;
                                    #region settingBlendShapeMirrorNameDifferentCharacters
                                    {
                                        EditorGUI.BeginChangeCheck();
                                        settingBlendShapeMirrorNameDifferentCharacters = EditorGUILayout.TextField(new GUIContent("Characters", "Different Characters"), settingBlendShapeMirrorNameDifferentCharacters);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            EditorPrefs.SetString("VeryAnimation_BlendShapeMirrorNameDifferentCharacters", settingBlendShapeMirrorNameDifferentCharacters);
                                            va.UpdateBlendShapeMirror();
                                            InternalEditorUtility.RepaintAllViews();
                                        }
                                    }
                                    #endregion
                                    EditorGUI.indentLevel--;
                                }
                            }
                            #endregion
                        }
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }
                if (va.dummyObject != null)
                {
                    dummyObjectFoldout = EditorGUILayout.Foldout(dummyObjectFoldout, "Dummy Object", true);
                    if (dummyObjectFoldout)
                    {
                        EditorGUI.indentLevel++;
                        {
                            #region Color
                            {
                                EditorGUI.BeginChangeCheck();
                                settingDummyObjectColor = EditorGUILayout.ColorField("Color", settingDummyObjectColor);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    SetEditorPrefsColor("VeryAnimation_DummyObjectColor", settingDummyObjectColor);
                                    va.dummyObject.SetColor(settingDummyObjectColor);
                                    InternalEditorUtility.RepaintAllViews();
                                }
                            }
                            #endregion
                        }
                        {
                            #region settingDummyPositionType
                            {
                                EditorGUI.BeginChangeCheck();
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PrefixLabel("Position Type");
                                settingDummyPositionType = (DummyPositionType)GUILayout.Toolbar((int)settingDummyPositionType, DummyPositionTypeString);
                                EditorGUILayout.EndHorizontal();
                                if (EditorGUI.EndChangeCheck())
                                {
                                    EditorPrefs.SetInt("VeryAnimation_DummyPositionType", (int)settingDummyPositionType);
                                    va.SetUpdateResampleAnimation();
                                    va.SetSynchroIKtargetAll(true);
                                    InternalEditorUtility.RepaintAllViews();
                                }
                            }
                            #endregion
                            EditorGUI.indentLevel++;
                            {
                                #region Position
                                {
                                    EditorGUI.BeginChangeCheck();
                                    settingDummyObjectPosition = EditorGUILayout.Vector3Field(new GUIContent("Offset Position", "This is used to prevent objects from overlapping."), settingDummyObjectPosition);
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        SetEditorPrefsVector3("VeryAnimation_DummyObjectPosition", settingDummyObjectPosition);
                                        va.SetUpdateResampleAnimation();
                                        InternalEditorUtility.RepaintAllViews();
                                    }
                                }
                                #endregion
                            }
                            EditorGUI.indentLevel--;
                        }
                        EditorGUI.indentLevel--;
                    }
                }
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Reset"))
                    {
                        Reset();
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.EndHorizontal();
                }
                GUILayout.Space(4);
            }
            EditorGUILayout.EndVertical();
        }

        private Color GetEditorPrefsColor(string name, Color defcolor)
        {
            return new Color(EditorPrefs.GetFloat(name + "_r", defcolor.r),
                            EditorPrefs.GetFloat(name + "_g", defcolor.g),
                            EditorPrefs.GetFloat(name + "_b", defcolor.b),
                            EditorPrefs.GetFloat(name + "_a", defcolor.a));
        }
        private void SetEditorPrefsColor(string name, Color color)
        {
            EditorPrefs.SetFloat(name + "_r", color.r);
            EditorPrefs.SetFloat(name + "_g", color.g);
            EditorPrefs.SetFloat(name + "_b", color.b);
            EditorPrefs.SetFloat(name + "_a", color.a);
        }

        private Vector3 GetEditorPrefsVector3(string name, Vector3 defvec)
        {
            return new Vector3(EditorPrefs.GetFloat(name + "_x", defvec.x),
                            EditorPrefs.GetFloat(name + "_y", defvec.y),
                            EditorPrefs.GetFloat(name + "_z", defvec.z));
        }
        private void SetEditorPrefsVector3(string name, Vector3 vec)
        {
            EditorPrefs.SetFloat(name + "_x", vec.x);
            EditorPrefs.SetFloat(name + "_y", vec.y);
            EditorPrefs.SetFloat(name + "_z", vec.z);
        }
    }
}
