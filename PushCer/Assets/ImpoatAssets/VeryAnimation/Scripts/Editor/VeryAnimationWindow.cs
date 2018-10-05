//#define Enable_Profiler
//#define Enable_MemoryLeakCheck

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
#if UNITY_2017_1_OR_NEWER
using UnityEngine.Playables;
using UnityEngine.Timeline;
#endif

namespace VeryAnimation
{
    [Serializable]
    public class VeryAnimationWindow : EditorWindow
    {
        public static VeryAnimationWindow instance;

        public static readonly string version = "1.1.1p4";

        [MenuItem("Window/Very Animation/Main %#v")]
        public static void Open()
        {
            if (instance == null)
            {
                EditorWindow window = null;
                foreach (var w in Resources.FindObjectsOfTypeAll<EditorWindow>())
                {
                    if (w.GetType().Name == "InspectorWindow")
                    {
                        window = w;
                        break;
                    }
                }
                if (window != null)
                    GetWindow<VeryAnimationWindow>(window.GetType());
                else
                    GetWindow<VeryAnimationWindow>();
            }
            else
            {
                if (instance.va != null)
                {
                    instance.SetGameObject();
                    if (!instance.va.isError && !instance.va.edit)
                    {
                        instance.Initialize();
                    }
                }
            }
        }

        public GameObject gameObject { get; private set; }
        public Animator animator { get; private set; }
        public Animation animation { get; private set; }
        public AnimationClip playingAnimationClip { get; private set; }
        public float playingAnimationTime { get; private set; }

        #region Core
        [SerializeField]
        private VeryAnimation va;
        public EditorSettings editorSettings { get; private set; }
        #endregion

        #region Reflection
        public UEditorWindow uEditorWindow { get; private set; }
        public USceneView uSceneView { get; private set; }
        public UEditorGUIUtility uEditorGUIUtility { get; private set; }
        public USnapSettings uSnapSettings { get; private set; }
        public UDisc uDisc { get; private set; }
        public UMuscleClipEditorUtilities uMuscleClipQualityInfo { get; private set; }
        public UAnimationUtility uAnimationUtility { get; private set; }
        #endregion

        #region Editor
        public bool initialized { get; private set; }
        private int undoGroupID = -1;
        private int beforeErrorCode;
        private bool handleTransformUpdate = true;
        private Vector3 handlePosition;
        private Quaternion handleRotation;
        private Vector3 handleScale;

        private int[] muscleRotationHandleIds;
        [NonSerialized]
        public int[] muscleRotationSliderIds;

        private Vector3[] skeletonLines;
        private EditorCommon.ArrowMesh arrowMesh;

        public enum RepaintGUI
        {
            None,
            Edit,
            All,
        }
        private RepaintGUI repaintGUI;
        public void SetRepaintGUI(RepaintGUI type)
        {
            if (repaintGUI < type)
                repaintGUI = type;
        }

        private int beforeSelectedTab;
        #endregion

        #region SelectionRect
        private struct SelectionRect
        {
            public void Reset()
            {
                Enable = false;
                start = Vector2.zero;
                end = Vector2.zero;
                distance = 0f;
                if (calcList == null) calcList = new List<GameObject>();
                else calcList.Clear();
                if (virtualCalcList == null) virtualCalcList = new List<HumanBodyBones>();
                else virtualCalcList.Clear();
                if (animatorIKCalcList == null) animatorIKCalcList = new List<AnimatorIKCore.IKTarget>();
                else animatorIKCalcList.Clear();
                if (originalIKCalcList == null) originalIKCalcList = new List<int>();
                else originalIKCalcList.Clear();
                beforeSelection = null;
                virtualBeforeSelection = null;
                beforeAnimatorIKSelection = null;
                beforeOriginalIKSelection = null;
            }
            public void SetStart(Vector2 add)
            {
                Enable = true;
                start = add;
                end = add;
                distance = 0f;
            }
            public void SetEnd(Vector2 add)
            {
                distance += Vector2.Distance(end, add);
                end = add;
            }
            public bool Enable { get; private set; }
            public Vector2 min { get { return Vector2.Min(start, end); } }
            public Vector2 max { get { return Vector2.Max(start, end); } }
            public Rect rect { get { return new Rect(min.x, min.y, max.x - min.x, max.y - min.y); } }

            public Vector2 start { get; private set; }
            public Vector2 end { get; private set; }
            public float distance { get; private set; }

            public List<GameObject> calcList;
            public List<HumanBodyBones> virtualCalcList;
            public List<AnimatorIKCore.IKTarget> animatorIKCalcList;
            public List<int> originalIKCalcList;
            public GameObject[] beforeSelection;
            public HumanBodyBones[] virtualBeforeSelection;
            public AnimatorIKCore.IKTarget[] beforeAnimatorIKSelection;
            public int[] beforeOriginalIKSelection;
        }
        private SelectionRect selectionRect;
        #endregion

        #region DisableEditor
        public class CustomAssetModificationProcessor : UnityEditor.AssetModificationProcessor
        {
            private static bool enable = true;

            static string[] OnWillSaveAssets(string[] paths)
            {
                if (enable)
                {
                    foreach (var w in Resources.FindObjectsOfTypeAll<VeryAnimationWindow>())
                    {
                        if (w.initialized)
                        {
                            w.Release();
                            Debug.Log("<color=blue>[Very Animation]</color>Editing ended : OnWillSaveAssets");
                        }
                    }
                }
                return paths;
            }

            public static void Pause()
            {
                enable = false;
            }
            public static void Resume()
            {
                enable = true;
            }
        }

        [InitializeOnLoadMethod]
        static void InitializeOnLoadMethod()
        {
            foreach (var w in Resources.FindObjectsOfTypeAll<VeryAnimationWindow>())
            {
                if (w.initialized)
                {
                    w.Release();
                    Debug.Log("<color=blue>[Very Animation]</color>Editing ended : InitializeOnLoadMethod");
                }
            }
        }

        static void CloseOtherWindows()
        {
            if (VeryAnimationControlWindow.instance != null)
                VeryAnimationControlWindow.instance.Close();
            if (VeryAnimationEditorWindow.instance != null)
                VeryAnimationEditorWindow.instance.Close();
        }

#if UNITY_2017_2_OR_NEWER
        private void OnPlayModeStateChanged(PlayModeStateChange mode)
        {
            foreach (var w in Resources.FindObjectsOfTypeAll<VeryAnimationWindow>())
            {
                if (w.initialized)
                {
                    w.Release();
                    Debug.Log("<color=blue>[Very Animation]</color>Editing ended : OnPlayModeStateChanged");
                }
            }
        }
        private void OnPauseStateChanged(PauseState mode)
        {
            foreach (var w in Resources.FindObjectsOfTypeAll<VeryAnimationWindow>())
            {
                if (w.initialized)
                {
                    w.Release();
                    Debug.Log("<color=blue>[Very Animation]</color>Editing ended : OnPauseStateChanged");
                }
            }
        }
#else
        private void OnPlaymodeStateChanged()
        {
            foreach (var w in Resources.FindObjectsOfTypeAll<VeryAnimationWindow>())
            {
                if (w.initialized)
                {
                    w.Release();
                    Debug.Log("<color=blue>[Very Animation]</color>Editing ended : OnPlaymodeStateChanged");
                }
            }
        }
#endif
        #endregion

        #region Texture
        private Texture2D circleNormalTex;
        private Texture2D circleActiveTex;
        private Texture2D circle3NormalTex;
        private Texture2D circle3ActiveTex;
        private Texture2D diamondNormalTex;
        private Texture2D diamondActiveTex;
        private Texture2D circleDotNormalTex;
        private Texture2D circleDotActiveTex;
        private Texture2D redLightTex;
        private Texture2D orangeLightTex;
        private Texture2D greenLightTex;
        private Texture2D lightRimTex;

        private void TextureReady()
        {
            circleNormalTex = EditorCommon.LoadTexture2DAssetAtPath("Assets/VeryAnimation/Textures/Editor/Circle_normal.psd");
            circleActiveTex = EditorCommon.LoadTexture2DAssetAtPath("Assets/VeryAnimation/Textures/Editor/Circle_active.psd");
            circle3NormalTex = EditorCommon.LoadTexture2DAssetAtPath("Assets/VeryAnimation/Textures/Editor/Circle3_normal.psd");
            circle3ActiveTex = EditorCommon.LoadTexture2DAssetAtPath("Assets/VeryAnimation/Textures/Editor/Circle3_active.psd");
            diamondNormalTex = EditorCommon.LoadTexture2DAssetAtPath("Assets/VeryAnimation/Textures/Editor/Diamond_normal.psd");
            diamondActiveTex = EditorCommon.LoadTexture2DAssetAtPath("Assets/VeryAnimation/Textures/Editor/Diamond_active.psd");
            circleDotNormalTex = EditorCommon.LoadTexture2DAssetAtPath("Assets/VeryAnimation/Textures/Editor/CircleDot_normal.psd");
            circleDotActiveTex = EditorCommon.LoadTexture2DAssetAtPath("Assets/VeryAnimation/Textures/Editor/CircleDot_active.psd");
            redLightTex = EditorGUIUtility.IconContent("lightMeter/redLight").image as Texture2D;
            orangeLightTex = EditorGUIUtility.IconContent("lightMeter/orangeLight").image as Texture2D;
            greenLightTex = EditorGUIUtility.IconContent("lightMeter/greenLight").image as Texture2D;
            lightRimTex = EditorGUIUtility.IconContent("lightMeter/lightRim").image as Texture2D;
        }
        #endregion

        #region GUIStyle
        public bool guiStyleReady { get; private set; }
        public GUIStyle guiStyleBoldButton { get; private set; }
        public GUIStyle guiStyleBoldActiveButton { get; private set; }
        public GUIStyle guiStyleActiveButton { get; private set; }
        public GUIStyle guiStyleActiveMiniButton { get; private set; }
        public GUIStyle guiStyleCircleButton { get; private set; }
        public GUIStyle guiStyleCircle3Button { get; private set; }
        public GUIStyle guiStyleDiamondButton { get; private set; }
        public GUIStyle guiStyleCircleDotButton { get; private set; }
        public GUIStyle guiStyleCenterAlignLabel { get; private set; }
        public GUIStyle guiStyleCenterAlignItalicLabel { get; private set; }
        public GUIStyle guiStyleCenterAlignYellowLabel { get; private set; }
        public GUIStyle guiStyleBoldFoldout { get; private set; }
        public GUIStyle guiStyleDropDown { get; private set; }
        public GUIStyle guiStyleToolbarBoldButton { get; private set; }
        public GUIStyle guiStyleAnimationRowEvenStyle { get; private set; }
        public GUIStyle guiStyleAnimationRowOddStyle { get; private set; }
        public GUIStyle guiStyleMiddleRightMiniLabel { get; private set; }
        public GUIStyle guiStyleMiddleRightGreyMiniLabel { get; private set; }

        private void GUIStyleReady()
        {
            if (guiStyleBoldButton == null || guiStyleBoldButton.normal.background == null)
            {
                guiStyleBoldButton = new GUIStyle(GUI.skin.button);
                guiStyleBoldButton.fontStyle = FontStyle.Bold;
            }
            if (guiStyleBoldActiveButton == null || guiStyleBoldActiveButton.normal.background == null)
            {
                guiStyleBoldActiveButton = new GUIStyle(GUI.skin.button);
                guiStyleBoldActiveButton.fontStyle = FontStyle.Bold;
                guiStyleBoldActiveButton.normal = guiStyleBoldActiveButton.active;
            }
            if (guiStyleActiveButton == null || guiStyleActiveButton.normal.background == null)
            {
                guiStyleActiveButton = new GUIStyle(GUI.skin.button);
                guiStyleActiveButton.normal = guiStyleActiveButton.active;
            }
            if (guiStyleActiveMiniButton == null || guiStyleActiveMiniButton.normal.background == null)
            {
                guiStyleActiveMiniButton = new GUIStyle(EditorStyles.miniButton);
                guiStyleActiveMiniButton.normal = guiStyleActiveMiniButton.active;
            }
            if (guiStyleCircleButton == null || guiStyleCircleButton.normal.background != circleNormalTex || guiStyleCircleButton.active.background != circleActiveTex)
            {
                guiStyleCircleButton = new GUIStyle(GUI.skin.button);
                guiStyleCircleButton.normal.background = circleNormalTex;
                guiStyleCircleButton.active.background = circleActiveTex;
                guiStyleCircleButton.border = new RectOffset(0, 0, 0, 0);
                guiStyleCircleButton.margin = new RectOffset(0, 0, 0, 0);
                guiStyleCircleButton.overflow = new RectOffset(0, 0, 0, 0);
                guiStyleCircleButton.padding = new RectOffset(0, 0, 0, 0);
                guiStyleCircleButton.imagePosition = ImagePosition.ImageOnly;
            }
            if (guiStyleCircle3Button == null || guiStyleCircle3Button.normal.background != circle3NormalTex || guiStyleCircle3Button.active.background != circle3ActiveTex)
            {
                guiStyleCircle3Button = new GUIStyle(GUI.skin.button);
                guiStyleCircle3Button.normal.background = circle3NormalTex;
                guiStyleCircle3Button.active.background = circle3ActiveTex;
                guiStyleCircle3Button.border = new RectOffset(0, 0, 0, 0);
                guiStyleCircle3Button.margin = new RectOffset(0, 0, 0, 0);
                guiStyleCircle3Button.overflow = new RectOffset(0, 0, 0, 0);
                guiStyleCircle3Button.padding = new RectOffset(0, 0, 0, 0);
                guiStyleCircleButton.imagePosition = ImagePosition.ImageOnly;
            }
            if (guiStyleDiamondButton == null || guiStyleDiamondButton.normal.background != diamondNormalTex || guiStyleDiamondButton.active.background != diamondActiveTex)
            {
                guiStyleDiamondButton = new GUIStyle(GUI.skin.button);
                guiStyleDiamondButton.normal.background = diamondNormalTex;
                guiStyleDiamondButton.active.background = diamondActiveTex;
                guiStyleDiamondButton.border = new RectOffset(0, 0, 0, 0);
                guiStyleDiamondButton.margin = new RectOffset(0, 0, 0, 0);
                guiStyleDiamondButton.overflow = new RectOffset(0, 0, 0, 0);
                guiStyleDiamondButton.padding = new RectOffset(0, 0, 0, 0);
                guiStyleCircleButton.imagePosition = ImagePosition.ImageOnly;
            }
            if (guiStyleCircleDotButton == null || guiStyleCircleDotButton.normal.background != circleDotNormalTex || guiStyleCircleDotButton.active.background != circleDotActiveTex)
            {
                guiStyleCircleDotButton = new GUIStyle(GUI.skin.button);
                guiStyleCircleDotButton.normal.background = circleDotNormalTex;
                guiStyleCircleDotButton.active.background = circleDotActiveTex;
                guiStyleCircleDotButton.border = new RectOffset(0, 0, 0, 0);
                guiStyleCircleDotButton.margin = new RectOffset(0, 0, 0, 0);
                guiStyleCircleDotButton.overflow = new RectOffset(0, 0, 0, 0);
                guiStyleCircleDotButton.padding = new RectOffset(0, 0, 0, 0);
                guiStyleCircleButton.imagePosition = ImagePosition.ImageOnly;
            }
            if (guiStyleCenterAlignLabel == null)
            {
                guiStyleCenterAlignLabel = new GUIStyle(EditorStyles.label);
                guiStyleCenterAlignLabel.alignment = TextAnchor.MiddleCenter;
            }
            if (guiStyleCenterAlignItalicLabel == null)
            {
                guiStyleCenterAlignItalicLabel = new GUIStyle(EditorStyles.label);
                guiStyleCenterAlignItalicLabel.alignment = TextAnchor.MiddleCenter;
                guiStyleCenterAlignItalicLabel.fontStyle = FontStyle.Italic;
            }
            if (guiStyleCenterAlignYellowLabel == null)
            {
                guiStyleCenterAlignYellowLabel = new GUIStyle(EditorStyles.label);
                guiStyleCenterAlignYellowLabel.alignment = TextAnchor.MiddleCenter;
                guiStyleCenterAlignYellowLabel.normal.textColor = Color.yellow;
            }
            if (guiStyleBoldFoldout == null || guiStyleBoldFoldout.normal.background == null)
            {
                guiStyleBoldFoldout = new GUIStyle(EditorStyles.foldout);
                guiStyleBoldFoldout.fontStyle = FontStyle.Bold;
            }
            if (guiStyleDropDown == null || guiStyleDropDown.normal.background == null)
            {
                guiStyleDropDown = new GUIStyle("DropDown");
                guiStyleDropDown.alignment = TextAnchor.MiddleCenter;
            }
            if (guiStyleToolbarBoldButton == null || guiStyleToolbarBoldButton.normal.background == null)
            {
                guiStyleToolbarBoldButton = new GUIStyle(EditorStyles.toolbarButton);
                guiStyleToolbarBoldButton.fontStyle = FontStyle.Bold;
            }
            if (guiStyleAnimationRowEvenStyle == null || guiStyleAnimationRowEvenStyle.normal.background == null)
            {
                var s = new GUIStyle("AnimationRowEven");
                guiStyleAnimationRowEvenStyle = new GUIStyle(GUI.skin.box);
                guiStyleAnimationRowEvenStyle.normal.background = s.normal.background;
                guiStyleAnimationRowEvenStyle.border = new RectOffset(0, 0, 0, 0);
                guiStyleAnimationRowEvenStyle.margin = new RectOffset(0, 0, 0, 0);
                guiStyleAnimationRowEvenStyle.overflow = new RectOffset(0, 0, 0, 0);
                guiStyleAnimationRowEvenStyle.padding = new RectOffset(0, 0, 0, 0);
            }
            if (guiStyleAnimationRowOddStyle == null || guiStyleAnimationRowOddStyle.normal.background == null)
            {
                var s = new GUIStyle("AnimationRowOdd");
                guiStyleAnimationRowOddStyle = new GUIStyle(GUI.skin.box);
                guiStyleAnimationRowOddStyle.normal.background = s.normal.background;
                guiStyleAnimationRowOddStyle.border = new RectOffset(0, 0, 0, 0);
                guiStyleAnimationRowOddStyle.margin = new RectOffset(0, 0, 0, 0);
                guiStyleAnimationRowOddStyle.overflow = new RectOffset(0, 0, 0, 0);
                guiStyleAnimationRowOddStyle.padding = new RectOffset(0, 0, 0, 0);
            }
            if (guiStyleMiddleRightMiniLabel == null)
            {
                guiStyleMiddleRightMiniLabel = new GUIStyle(EditorStyles.miniLabel);
                guiStyleMiddleRightMiniLabel.alignment = TextAnchor.MiddleRight;
            }
            if (guiStyleMiddleRightGreyMiniLabel == null)
            {
                guiStyleMiddleRightGreyMiniLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                guiStyleMiddleRightGreyMiniLabel.alignment = TextAnchor.MiddleRight;
            }
            guiStyleReady = true;
        }
        private void GUIStyleClear()
        {
            guiStyleBoldButton = null;
            guiStyleBoldActiveButton = null;
            guiStyleActiveButton = null;
            guiStyleActiveMiniButton = null;
            guiStyleCircleButton = null;
            guiStyleCircle3Button = null;
            guiStyleDiamondButton = null;
            guiStyleCircleDotButton = null;
            guiStyleCenterAlignLabel = null;
            guiStyleCenterAlignItalicLabel = null;
            guiStyleBoldFoldout = null;
            guiStyleDropDown = null;
            guiStyleToolbarBoldButton = null;
            guiStyleAnimationRowEvenStyle = null;
            guiStyleAnimationRowOddStyle = null;
            guiStyleMiddleRightMiniLabel = null;
            guiStyleMiddleRightGreyMiniLabel = null;
            guiStyleReady = false;
        }
        #endregion

        #region GUI
        private bool guiAnimationFoldout;
        private bool guiAnimationLoopFoldout;
        private bool guiAnimationWarningFoldout = true;
        private bool guiToolsFoldout;
        private bool guiSettingsFoldout;
        private bool guiHelpFoldout;
        private bool guiPreviewFoldout;

        private bool guiAnimationHelp;
        private bool guiToolsHelp;
        private bool guiSettingsHelp;
        private bool guiHelpHelp;
        private bool guiPreviewHelp;
        #endregion

        #region MemoryLeakCheck
#if Enable_MemoryLeakCheck
        private List<UnityEngine.Object> memoryLeakDontSaveList;
#endif
        #endregion

        private void OnEnable()
        {
            instance = this;

            {
                if (va == null)
                    va = new VeryAnimation();
                va.OnEnable();

                editorSettings = new EditorSettings();

                uEditorWindow = new UEditorWindow();
                uSceneView = new USceneView();
                uSnapSettings = new USnapSettings();
                uDisc = new UDisc();
#if UNITY_2018_1_OR_NEWER
                uEditorGUIUtility = new UEditorGUIUtility_2018_1();
                uMuscleClipQualityInfo = new UMuscleClipEditorUtilities_2018_1();
                uAnimationUtility = new UAnimationUtility_2018_1();
#else
                uEditorGUIUtility = new UEditorGUIUtility();
                uMuscleClipQualityInfo = new UMuscleClipEditorUtilities();
                uAnimationUtility = new UAnimationUtility();
#endif

                TextureReady();
                GUIStyleClear();
            }

            titleContent = new GUIContent("VeryAnimation");
            minSize = new Vector2(320, minSize.y);

            InternalEditorUtility.RepaintAllViews();
        }
        private void OnDisable()
        {
            if (va != null)
                va.OnDisable();
        }
        private void OnDestroy()
        {
            if (va != null)
                va.OnDestroy();
            instance = null;
        }

        private void OnSelectionChange()
        {
            if (!initialized || va.isEditError) return;

            va.SelectGameObjectEvent();
            Repaint();
        }
        private void OnFocus()
        {
            instance = this;    //Measures against the problem that OnEnable may not come when repeating Shift + Space.
            va.OnFocus();
        }
        private void OnLostFocus()
        {
            if (!initialized || va.isEditError) return;

            if (!uEditorWindow.HasFocus(this))
            {
                Release();
                return;
            }
        }

        private void OnGUI()
        {
            if (va == null || va.uAw == null)
                return;

#if Enable_Profiler
            Profiler.BeginSample(string.Format("****VeryAnimationWindow.OnGUI {0}", Event.current));
#endif

            GUIStyleReady();

            Event e = Event.current;

            if (va.uAw.instance == null)
            {
                #region Animation Window is not open
                EditorGUILayout.HelpBox(Language.GetText(Language.Help.AnimationWindowisnotopen), MessageType.Error);
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Open Animation Window"))
                    {
#if UNITY_2018_2_OR_NEWER
                        EditorApplication.ExecuteMenuItem("Window/Animation/Animation");
#else
                        EditorApplication.ExecuteMenuItem("Window/Animation");
#endif
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.EndHorizontal();
                }
                #endregion
            }
            else if (!va.uAw.HasFocus())
            {
                #region Animation Window is not focus
                EditorGUILayout.HelpBox(Language.GetText(Language.Help.AnimationWindowisnotfocus), MessageType.Error);
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Focus Animation Window"))
                    {
                        va.uAw.instance.Focus();
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.EndHorizontal();
                }
                #endregion
            }
            else if (gameObject == null || (animator == null && animation == null))
            {
                #region Selection Error
#if UNITY_2017_1_OR_NEWER
                if (va.uAw_2017_1.GetLinkedWithTimeline())
                    EditorGUILayout.LabelField(Language.GetText(Language.Help.TheSequenceEditortowhichAnimationislinkedisnotenabled), EditorStyles.centeredGreyMiniLabel, GUILayout.Height(48));
                else
#endif
                    EditorGUILayout.LabelField(Language.GetText(Language.Help.Noanimatableobjectselected), EditorStyles.centeredGreyMiniLabel, GUILayout.Height(48));
                #endregion
            }
            else if (!va.edit)
            {
                #region Ready
                var clip = playingAnimationClip != null ? playingAnimationClip : va.uAw.GetSelectionAnimationClip();

                #region Animation
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
#if UNITY_2017_1_OR_NEWER
                    if (va.uAw_2017_1.GetLinkedWithTimeline())
                    {
                        EditorGUILayout.LabelField("Linked with Sequence Editor", EditorStyles.centeredGreyMiniLabel);
                        var currentDirector = va.uAw_2017_1.GetTimelineCurrentDirector();
                        if (currentDirector != null)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Playable Director", GUILayout.Width(120));
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.ObjectField(currentDirector, typeof(PlayableDirector), false);
                            EditorGUI.EndDisabledGroup();
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    else
#endif
                    {
                        if (EditorApplication.isPlaying)
                        {
                            if (animator != null)
                                EditorGUILayout.LabelField("Linked with Animator Controller", EditorStyles.centeredGreyMiniLabel);
                            else if (animation != null)
                                EditorGUILayout.LabelField("Linked with Animation Component", EditorStyles.centeredGreyMiniLabel);
                        }
                        else
                        {
                            EditorGUILayout.LabelField("Linked with Animation Window", EditorStyles.centeredGreyMiniLabel);
                        }
                    }
                    {
                        #region Animatable
                        if (animator != null)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Animator", GUILayout.Width(120));
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.ObjectField(animator, typeof(Animator), false);
                            EditorGUI.EndDisabledGroup();
                            EditorGUILayout.EndHorizontal();
                        }
                        else if (animation != null)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Animation", GUILayout.Width(120));
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.ObjectField(animation, typeof(Animation), false);
                            EditorGUI.EndDisabledGroup();
                            EditorGUILayout.EndHorizontal();
                        }
                        #endregion

                        #region Animation Clip
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Animation Clip", GUILayout.Width(120));
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.ObjectField(clip, typeof(AnimationClip), false);
                            EditorGUI.EndDisabledGroup();
                            if (clip != null)
                            {
                                if ((clip.hideFlags & HideFlags.NotEditable) != HideFlags.None)
                                {
                                    EditorGUILayout.LabelField("(Read-Only)", GUILayout.Width(78));
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                            if (playingAnimationClip != null)
                            {
                                EditorGUI.indentLevel++;
                                EditorGUI.BeginDisabledGroup(true);
                                EditorGUILayout.Slider("Time", playingAnimationTime, 0f, playingAnimationClip.length);
                                EditorGUI.EndDisabledGroup();
                                EditorGUI.indentLevel--;
                            }
                        }
                        #endregion
                    }
                    EditorGUILayout.EndVertical();
                }
                #endregion

                EditorGUI.BeginDisabledGroup(va.isError);
                if (GUILayout.Button("Edit Animation", guiStyleBoldButton, GUILayout.Height(32)))
                {
                    Initialize();
                }
                EditorGUI.EndDisabledGroup();

                #region Error
                if (va.uAw.GetSelectionAnimationClip() == null)
                {
                    EditorGUILayout.HelpBox(Language.GetText(Language.Help.AnimationClipisnotselectedinAnimationWindow), MessageType.Error);
                }
                if (!gameObject.activeInHierarchy)
                {
                    EditorGUILayout.HelpBox(Language.GetText(Language.Help.GameObjectisnotActive), MessageType.Error);
                }
                if (animator != null && !animator.hasTransformHierarchy)
                {
                    EditorGUILayout.HelpBox(Language.GetText(Language.Help.Editingonoptimizedtransformhierarchyisnotsupported), MessageType.Error);
                }
                if (animation != null && animation.GetClipCount() == 0)
                {
                    EditorGUILayout.HelpBox(Language.GetText(Language.Help.ThereisnoAnimationClipinAnimationComponent), MessageType.Error);
                }
                if (animation != null && Application.isPlaying)
                {
                    EditorGUILayout.HelpBox(Language.GetText(Language.Help.EditingLegacywhileplayingisnotsupported), MessageType.Error);
                }
#if UNITY_2017_1_OR_NEWER
                if (!va.uAw_2017_1.GetLinkedWithTimeline())
#endif
                {
                    if (animator != null && animator.runtimeAnimatorController == null)
                    {
                        EditorGUILayout.HelpBox(Language.GetText(Language.Help.AnimatorControllerisnotfound), MessageType.Error);
                    }
                    if (animator != null && animator.runtimeAnimatorController != null && animator.runtimeAnimatorController.animationClips.Length == 0)
                    {
                        EditorGUILayout.HelpBox(Language.GetText(Language.Help.ThereisnoAnimationClipinAnimatorController), MessageType.Error);
                    }
                    if (animator != null && animator.runtimeAnimatorController != null && (animator.runtimeAnimatorController.hideFlags & (HideFlags.DontSave | HideFlags.NotEditable)) != 0)
                    {
                        EditorGUILayout.HelpBox(Language.GetText(Language.Help.AnimatorControllerisnoteditable), MessageType.Error);
                    }
                }
#if UNITY_2017_1_OR_NEWER
                else
                {
                    if (!va.uAw_2017_1.GetLinkedWithTimelineEditable())
                    {
                        EditorGUILayout.HelpBox(Language.GetText(Language.Help.TheAnimationTracktowhichAnimationislinkedisnotenabled), MessageType.Error);
                    }
                    if (Application.isPlaying)
                    {
                        EditorGUILayout.HelpBox(Language.GetText(Language.Help.EditingTimelinewhileplayingisnotsupported), MessageType.Error);
                    }
                    {
                        var currentDirector = va.uAw_2017_1.GetTimelineCurrentDirector();
                        if (currentDirector != null && !currentDirector.gameObject.activeInHierarchy)
                        {
                            EditorGUILayout.HelpBox(Language.GetText(Language.Help.TimelineGameObjectisnotActive), MessageType.Error);
                        }
                        if (currentDirector != null && !currentDirector.enabled)
                        {
                            EditorGUILayout.HelpBox(Language.GetText(Language.Help.TimelinePlayableDirectorisnotEnable), MessageType.Error);
                        }
                    }
                }
#endif
                #endregion

                #region Warning
                if (gameObject != null && gameObject.activeInHierarchy && animator != null && animator.isHuman && animator.hasTransformHierarchy && va.uAvatar.GetHasTDoF(animator.avatar))
                {
                    #region TDOF
                    if (!animator.isInitialized)
                        animator.Rebind();
                    {
                        var checkList = new HumanBodyBones[]
                        {
                            HumanBodyBones.Chest,
                            HumanBodyBones.Neck,
                            HumanBodyBones.LeftShoulder,
                            HumanBodyBones.RightShoulder,
                            HumanBodyBones.UpperChest,
                        };
                        foreach (var hi in checkList)
                        {
                            if (animator.GetBoneTransform(hi) != null) continue;
                            EditorGUILayout.HelpBox(string.Format(Language.GetText(Language.Help.TranslationDoFisdisabled), hi, hi), MessageType.Warning);
                        }
                    }
                    if (animator.GetBoneTransform(HumanBodyBones.Chest) == null)
                    {
                        var checkList = new HumanBodyBones[]
                        {
                            HumanBodyBones.Neck,
                            HumanBodyBones.LeftShoulder,
                            HumanBodyBones.RightShoulder,
                            HumanBodyBones.UpperChest,
                        };
                        foreach (var hi in checkList)
                        {
                            if (animator.GetBoneTransform(hi) != null)
                                EditorGUILayout.HelpBox(string.Format(Language.GetText(Language.Help.TranslationDoFisdisabled), "Chest", hi), MessageType.Warning);
                        }
                    }
                    else if (animator.GetBoneTransform(HumanBodyBones.UpperChest) == null)
                    {
                        var checkList = new HumanBodyBones[]
                        {
                            HumanBodyBones.Neck,
                            HumanBodyBones.LeftShoulder,
                            HumanBodyBones.RightShoulder,
                        };
                        foreach (var hi in checkList)
                        {
                            if (animator.GetBoneTransform(hi) != null)
                                EditorGUILayout.HelpBox(string.Format(Language.GetText(Language.Help.TranslationDoFisdisabled), "UpperChest", hi), MessageType.Warning);
                        }
                    }
                    #endregion
                }
                if (animator != null && animator.isHuman)
                {
                    #region Animator IK
                    const float ErrorThreshold = 0.1f;

                    if (!animator.isInitialized)
                        animator.Rebind();

                    #region Root
                    {
                        var scale = gameObject.transform.lossyScale;
                        if (Mathf.Abs(scale.x - scale.y) >= ErrorThreshold || Mathf.Abs(scale.x - scale.z) >= ErrorThreshold || Mathf.Abs(scale.y - scale.z) >= ErrorThreshold)
                        {
                            EditorGUILayout.HelpBox(string.Format(Language.GetText(Language.Help.Roottransformscaleratiodoesnotmatch), gameObject.name, scale), MessageType.Warning);
                        }
                    }
                    #endregion
                    #endregion
                }
                #endregion
                #endregion
            }
            else if (!va.isEditError)
            {
                #region Editing
                #region Toolbar
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                    {
                        EditorGUI.BeginChangeCheck();
                        guiAnimationFoldout = GUILayout.Toggle(guiAnimationFoldout, "Animation", EditorStyles.toolbarButton);
                        if (EditorGUI.EndChangeCheck())
                        {
                            EditorPrefs.SetBool("VeryAnimation_Main_Animation", guiAnimationFoldout);
                        }
                    }
                    {
                        EditorGUI.BeginChangeCheck();
                        guiToolsFoldout = GUILayout.Toggle(guiToolsFoldout, "Tools", EditorStyles.toolbarButton);
                        if (EditorGUI.EndChangeCheck())
                        {
                            EditorPrefs.SetBool("VeryAnimation_Main_Tools", guiToolsFoldout);
                        }
                    }
                    {
                        EditorGUI.BeginChangeCheck();
                        guiSettingsFoldout = GUILayout.Toggle(guiSettingsFoldout, "Settings", EditorStyles.toolbarButton);
                        if (EditorGUI.EndChangeCheck())
                        {
                            EditorPrefs.SetBool("VeryAnimation_Main_Settings", guiSettingsFoldout);
                        }
                    }
                    {
                        EditorGUI.BeginChangeCheck();
                        guiHelpFoldout = GUILayout.Toggle(guiHelpFoldout, "Help", EditorStyles.toolbarButton);
                        if (EditorGUI.EndChangeCheck())
                        {
                            EditorPrefs.SetBool("VeryAnimation_Main_Help", guiHelpFoldout);
                        }
                    }
                    {
                        EditorGUI.BeginChangeCheck();
                        guiPreviewFoldout = GUILayout.Toggle(guiPreviewFoldout, "Preview", EditorStyles.toolbarButton);
                        if (EditorGUI.EndChangeCheck())
                        {
                            EditorPrefs.SetBool("VeryAnimation_Main_Preview", guiPreviewFoldout);
                        }
                    }
                    EditorGUILayout.Space();
                    #region Edit
                    if (GUILayout.Button("Exit", guiStyleToolbarBoldButton, GUILayout.Width(48)))
                    {
                        Release();
                        return;
                    }
                    #endregion
                    EditorGUILayout.EndHorizontal();
                }
                #endregion

                #region Animation
                if (guiAnimationFoldout)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    guiAnimationFoldout = EditorGUILayout.Foldout(guiAnimationFoldout, "Animation", true, guiStyleBoldFoldout);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetBool("VeryAnimation_Main_Animation", guiAnimationFoldout);
                    }
                    {
                        EditorGUILayout.Space();
                        if (GUILayout.Button("?", guiAnimationHelp ? guiStyleActiveButton : GUI.skin.button, GUILayout.Width(16), GUILayout.Height(14)))
                        {
                            guiAnimationHelp = !guiAnimationHelp;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    {
                        if (guiAnimationHelp)
                        {
                            EditorGUILayout.HelpBox(Language.GetText(Language.Help.HelpAnimation), MessageType.Info);
                        }

                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        {
                            #region Animatable
                            if (animator != null)
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("Animator");
                                EditorGUI.BeginDisabledGroup(true);
                                EditorGUILayout.ObjectField(animator, typeof(Animator), false);
                                EditorGUI.EndDisabledGroup();
                                EditorGUILayout.EndHorizontal();
                            }
                            else if (animation != null)
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("Animation");
                                EditorGUI.BeginDisabledGroup(true);
                                EditorGUILayout.ObjectField(animation, typeof(Animation), false);
                                EditorGUI.EndDisabledGroup();
                                EditorGUILayout.EndHorizontal();
                            }
                            #endregion

                            #region Animation Clip
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("Animation Clip");
                                EditorGUI.BeginDisabledGroup(true);
                                EditorGUILayout.ObjectField(va.currentClip, typeof(AnimationClip), false);
                                EditorGUI.EndDisabledGroup();
                                if (va.currentClip != null)
                                {
                                    if ((va.currentClip.hideFlags & HideFlags.NotEditable) != HideFlags.None)
                                    {
                                        EditorGUILayout.LabelField("(Read-Only)", GUILayout.Width(78));
                                    }
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                            #endregion

                            if (va.currentClip != null)
                            {
                                AnimationClipSettings animationClipSettings = AnimationUtility.GetAnimationClipSettings(va.currentClip);
                                bool hasMotionCurves = uAnimationUtility.HasMotionCurves(va.currentClip);
                                bool hasRootCurves = uAnimationUtility.HasRootCurves(va.currentClip);
                                EditorGUI.indentLevel++;
                                if ((va.currentClip.hideFlags & HideFlags.NotEditable) != 0)
                                {
                                    EditorGUILayout.BeginHorizontal(GUI.skin.box);
                                    EditorGUILayout.HelpBox(Language.GetText(Language.Help.AnimationclipisReadOnly), MessageType.Warning);
                                    {
                                        EditorGUILayout.BeginVertical();
                                        EditorGUILayout.Space();
                                        if (GUILayout.Button("Duplicate and Replace"))
                                        {
                                            va.DuplicateAndReplace();
                                        }
                                        EditorGUILayout.Space();
                                        EditorGUILayout.EndVertical();
                                    }
                                    EditorGUILayout.EndHorizontal();
                                }
                                else
                                {
                                    if (animator != null && animator.isHuman && va.currentClip.isHumanMotion)
                                        EditorGUILayout.LabelField("Humanoid Motion");
                                    else if (!va.currentClip.legacy)
                                        EditorGUILayout.LabelField("Generic Motion");
                                    else
                                        EditorGUILayout.LabelField("Legacy Motion");
                                    #region Loop
                                    if (va.currentClip.isLooping && !va.currentClip.legacy)
                                    {
                                        guiAnimationLoopFoldout = EditorGUILayout.Foldout(guiAnimationLoopFoldout, "Loop", true);
                                        EditorGUI.indentLevel++;
                                        if (guiAnimationLoopFoldout)
                                        {
                                            if (animator != null && !animator.isInitialized)
                                                animator.Rebind();
                                            var info = uMuscleClipQualityInfo.GetMuscleClipQualityInfo(va.currentClip, 0f, va.currentClip.length);
                                            var hasRootCurve = va.uAnimationUtility.HasRootCurves(va.currentClip) || va.uAnimationUtility.HasMotionCurves(va.currentClip);
                                            {
                                                EditorGUILayout.BeginHorizontal();
                                                if (animationClipSettings.loopBlend)
                                                    EditorGUILayout.LabelField(new GUIContent("Loop Pose", "Loop Blend"), GUILayout.Width(160f));
                                                else
                                                    EditorGUILayout.LabelField("Loop", GUILayout.Width(160f));
                                                GUILayout.FlexibleSpace();
                                                if (hasRootCurve && va.currentClip.isHumanMotion)
                                                {
                                                    EditorGUILayout.LabelField("loop match", guiStyleMiddleRightMiniLabel, GUILayout.Width(90f));
                                                    var rect = EditorGUILayout.GetControlRect(false, 16f, GUILayout.Width(16f));
                                                    if (animationClipSettings.loopBlend)
                                                    {
                                                        if (info.loop < 0.33f)
                                                            GUI.DrawTexture(rect, redLightTex);
                                                        else if (info.loop < 0.66f)
                                                            GUI.DrawTexture(rect, orangeLightTex);
                                                        else
                                                            GUI.DrawTexture(rect, greenLightTex);
                                                    }
                                                    else
                                                    {
                                                        if (info.loop < 0.66f)
                                                            GUI.DrawTexture(rect, redLightTex);
                                                        else if (info.loop < 0.99f)
                                                            GUI.DrawTexture(rect, orangeLightTex);
                                                        else
                                                            GUI.DrawTexture(rect, greenLightTex);
                                                    }
                                                    GUI.DrawTexture(rect, lightRimTex);
                                                }
                                                EditorGUILayout.EndHorizontal();
                                            }
                                            if (hasRootCurve)
                                            {
                                                Action<string, float, bool> LoopMatchGUI = (name, value, bake) =>
                                                {
                                                    EditorGUILayout.BeginHorizontal();
                                                    EditorGUILayout.LabelField(name, GUILayout.Width(160f));
                                                    EditorGUILayout.Space();
                                                    if (bake)
                                                    {
                                                        EditorGUILayout.LabelField("loop match", guiStyleMiddleRightMiniLabel, GUILayout.Width(90f));
                                                        var rect = EditorGUILayout.GetControlRect(false, 16f, GUILayout.Width(16f));
                                                        if (animationClipSettings.loopBlend)
                                                        {
                                                            if (value < 0.33f)
                                                                GUI.DrawTexture(rect, redLightTex);
                                                            else if (value < 0.66f)
                                                                GUI.DrawTexture(rect, orangeLightTex);
                                                            else
                                                                GUI.DrawTexture(rect, greenLightTex);
                                                        }
                                                        else
                                                        {
                                                            if (value < 0.66f)
                                                                GUI.DrawTexture(rect, redLightTex);
                                                            else if (value < 0.99f)
                                                                GUI.DrawTexture(rect, orangeLightTex);
                                                            else
                                                                GUI.DrawTexture(rect, greenLightTex);
                                                        }
                                                        GUI.DrawTexture(rect, lightRimTex);
                                                    }
                                                    else
                                                    {
                                                        EditorGUILayout.LabelField("root motion", guiStyleMiddleRightMiniLabel, GUILayout.Width(90f));
                                                    }
                                                    EditorGUILayout.EndHorizontal();
                                                };
                                                LoopMatchGUI("Loop Orientation", info.loopOrientation, animationClipSettings.loopBlendOrientation);
                                                LoopMatchGUI("Loop Position (Y)", info.loopPositionY, animationClipSettings.loopBlendPositionY);
                                                LoopMatchGUI("Loop Position (XZ)", info.loopPositionXZ, animationClipSettings.loopBlendPositionXZ);
                                            }
                                        }
                                        EditorGUI.indentLevel--;
                                    }
                                    #endregion
                                    #region Warning
                                    {
                                        int count = 0;
                                        {
                                            if (animationClipSettings.loopTime && animationClipSettings.loopBlend) count++;
                                            if (!animationClipSettings.keepOriginalPositionY && animationClipSettings.heightFromFeet && !va.IsHaveAnimationCurveAnimatorIkT(VeryAnimation.AnimatorIKIndex.LeftFoot) && !va.IsHaveAnimationCurveAnimatorIkT(VeryAnimation.AnimatorIKIndex.RightFoot)) count++;
                                            if (hasRootCurves && !hasMotionCurves &&
                                                (!animationClipSettings.keepOriginalOrientation || !animationClipSettings.keepOriginalPositionXZ || !animationClipSettings.keepOriginalPositionY)) count++;
                                        }
                                        if (count > 0)
                                        {
                                            guiAnimationWarningFoldout = EditorGUILayout.Foldout(guiAnimationWarningFoldout, "Warning", true);
                                            EditorGUI.indentLevel++;
                                            if (guiAnimationWarningFoldout)
                                            {
                                                if (animationClipSettings.loopTime && animationClipSettings.loopBlend)
                                                {
                                                    EditorGUILayout.HelpBox(Language.GetText(Language.Help.AnimationClipSettingsLoopPoseisenabled), MessageType.Warning);
                                                }
                                                if (!animationClipSettings.keepOriginalPositionY && animationClipSettings.heightFromFeet && !va.IsHaveAnimationCurveAnimatorIkT(VeryAnimation.AnimatorIKIndex.LeftFoot) && !va.IsHaveAnimationCurveAnimatorIkT(VeryAnimation.AnimatorIKIndex.RightFoot))
                                                {
                                                    EditorGUILayout.HelpBox(Language.GetText(Language.Help.AnimationClipSettingsRootTransformPositionYisFeet), MessageType.Warning);
                                                }
                                                if (hasRootCurves && !hasMotionCurves &&
                                                    (!animationClipSettings.keepOriginalOrientation || !animationClipSettings.keepOriginalPositionXZ || !animationClipSettings.keepOriginalPositionY))
                                                {
                                                    EditorGUILayout.HelpBox(Language.GetText(Language.Help.AnimationClipSettingsBasedUponisnotOriginal), MessageType.Warning);
                                                }
                                            }
                                            EditorGUI.indentLevel--;
                                        }
                                    }
                                    #endregion
                                    #region Error
                                    {
                                        int count = 0;
                                        {
                                            if (animationClipSettings.cycleOffset != 0f) count++;
                                            if (animationClipSettings.mirror) count++;
#if UNITY_2017_1_OR_NEWER
                                            if (va.uAw_2017_1.GetLinkedWithTimeline())
                                            {
                                                var timelineFrameRate = va.uAw_2017_1.GetTimelineFrameRate();
                                                if (va.currentClip != null && va.currentClip.frameRate != timelineFrameRate)
                                                    count++;
                                            }
#endif
                                        }
                                        if (count > 0)
                                        {
                                            EditorGUILayout.LabelField("Error");
                                            EditorGUI.indentLevel++;
                                            {
                                                if (animationClipSettings.cycleOffset != 0f)
                                                {
                                                    EditorGUILayout.HelpBox(Language.GetText(Language.Help.AnimationClipSettingsCycleOffsetisnot0), MessageType.Error);
                                                }
                                                if (animationClipSettings.mirror)
                                                {
                                                    EditorGUILayout.HelpBox(Language.GetText(Language.Help.AnimationClipSettingsMirrorisenabled), MessageType.Error);
                                                }
#if UNITY_2017_1_OR_NEWER
                                                if (va.uAw_2017_1.GetLinkedWithTimeline())
                                                {
                                                    var timelineFrameRate = va.uAw_2017_1.GetTimelineFrameRate();
                                                    if (va.currentClip != null && va.currentClip.frameRate != timelineFrameRate)
                                                    {
                                                        EditorGUILayout.HelpBox(string.Format(Language.GetText(Language.Help.AnimationClipSettingsFramerateofTimelineandAnimationClipdonotmatch), va.currentClip.frameRate, timelineFrameRate), MessageType.Error);
                                                    }
                                                }
#endif
                                            }
                                            EditorGUI.indentLevel--;
                                        }
                                    }
                                    #endregion
                                }
                                EditorGUI.indentLevel--;
                            }
                        }
                        GUILayout.Space(3);
                        EditorGUILayout.EndVertical();
                    }
                }
                #endregion

                #region Tools
                if (guiToolsFoldout)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    guiToolsFoldout = EditorGUILayout.Foldout(guiToolsFoldout, "Tools", true, guiStyleBoldFoldout);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetBool("VeryAnimation_Main_Tools", guiToolsFoldout);
                    }
                    {
                        EditorGUILayout.Space();
                        if (GUILayout.Button("?", guiToolsHelp ? guiStyleActiveButton : GUI.skin.button, GUILayout.Width(16), GUILayout.Height(14)))
                        {
                            guiToolsHelp = !guiToolsHelp;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    if (guiToolsHelp)
                    {
                        EditorGUILayout.HelpBox(Language.GetText(Language.Help.HelpTools), MessageType.Info);
                    }

                    va.ToolsGUI();
                }
                #endregion

                #region Settings
                if (guiSettingsFoldout)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    guiSettingsFoldout = EditorGUILayout.Foldout(guiSettingsFoldout, "Settings", true, guiStyleBoldFoldout);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetBool("VeryAnimation_Main_Settings", guiSettingsFoldout);
                    }
                    {
                        EditorGUILayout.Space();
                        if (GUILayout.Button("?", guiSettingsHelp ? guiStyleActiveButton : GUI.skin.button, GUILayout.Width(16), GUILayout.Height(14)))
                        {
                            guiSettingsHelp = !guiSettingsHelp;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    {
                        if (guiSettingsHelp)
                        {
                            EditorGUILayout.HelpBox(Language.GetText(Language.Help.HelpSettings), MessageType.Info);
                        }

                        editorSettings.SettingsGUI();
                    }
                }
                #endregion

                #region Help
                if (guiHelpFoldout)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    guiHelpFoldout = EditorGUILayout.Foldout(guiHelpFoldout, "Help", true, guiStyleBoldFoldout);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetBool("VeryAnimation_Main_Help", guiHelpFoldout);
                    }
                    {
                        EditorGUILayout.Space();
                        if (GUILayout.Button("?", guiHelpHelp ? guiStyleActiveButton : GUI.skin.button, GUILayout.Width(16), GUILayout.Height(14)))
                        {
                            guiHelpHelp = !guiHelpHelp;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    {
                        if (guiHelpHelp)
                        {
                            EditorGUILayout.HelpBox(Language.GetText(Language.Help.HelpHelp), MessageType.Info);
                        }

                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        {
                            EditorGUILayout.LabelField("Version", version);
                            EditorGUILayout.LabelField("Hotkeys");
                            EditorGUI.indentLevel++;
                            {
                                EditorGUILayout.LabelField("Esc", "[Editor] Exit edit");
                                EditorGUILayout.LabelField("O", "[Editor] Change Clamp");
                                EditorGUILayout.LabelField("J", "[Editor] Change Foot IK");
                                EditorGUILayout.LabelField("M", "[Editor] Change Mirror");
                                EditorGUILayout.LabelField("L", "[Editor] Change Root Correction Mode");
                                EditorGUILayout.LabelField("I", "[Editor] Change selection bone IK");
                                EditorGUILayout.LabelField("Page Down", "[AnimationWindow] Next animation clip");
                                EditorGUILayout.LabelField("Page Up", "[AnimationWindow] Previous animation clip");
                                EditorGUILayout.LabelField("F5", "[AnimationWindow] Force refresh");
                                EditorGUILayout.LabelField("Space / Ctrl + Space", "[AnimationWindow] Change playing");
                                EditorGUILayout.LabelField("C", "[AnimationWindow] Switch between curves and dope sheet");
                                EditorGUILayout.LabelField("K", "[AnimationWindow] Add keyframe");
                                EditorGUILayout.LabelField(",", "[AnimationWindow] Move to next frame");
                                EditorGUILayout.LabelField(".", "[AnimationWindow] Move to previous frame");
                                EditorGUILayout.LabelField("Alt + ,", "[AnimationWindow] Move to next keyframe");
                                EditorGUILayout.LabelField("Alt + .", "[AnimationWindow] Move to previous keyframe");
                                EditorGUILayout.LabelField("Shift + ,", "[AnimationWindow] Move to first keyframe");
                                EditorGUILayout.LabelField("Shift + .", "[AnimationWindow] Move to last keyframe");
                                EditorGUILayout.LabelField("H", "[Hierarchy] Hide select bones");
                                EditorGUILayout.LabelField("Shift + H", "[Hierarchy] Show select bones");
                                EditorGUILayout.LabelField("P", "[Preview] Change playing");
                                EditorGUILayout.LabelField("Ctrl + Keypad Plus", "[IK] Add IK - Level / Direction");
                                EditorGUILayout.LabelField("Ctrl + Keypad Minus", "[IK] Sub IK - Level / Direction");
                            }
                            EditorGUI.indentLevel--;
                            EditorGUILayout.LabelField("Icons");
                            EditorGUI.indentLevel++;
                            {
                                Action<string, Texture2D> IconGUI = (s, t) =>
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField(s, GUILayout.Width(146));
                                    var rect = EditorGUILayout.GetControlRect();
                                    rect.width = rect.height;
                                    GUI.DrawTexture(rect, t);
                                    EditorGUILayout.EndHorizontal();
                                };
                                IconGUI("Humanoid / Normal", circleNormalTex);
                                IconGUI("Root", circle3NormalTex);
                                IconGUI("Non Humanoid", diamondNormalTex);
                                IconGUI("Humanoid Virtual", circleDotNormalTex);
                            }
                            EditorGUI.indentLevel--;
                        }
                        EditorGUILayout.EndVertical();
                    }
                }
                #endregion

                #region Preview
                if (guiPreviewFoldout)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    guiPreviewFoldout = EditorGUILayout.Foldout(guiPreviewFoldout, "Preview", true, guiStyleBoldFoldout);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetBool("VeryAnimation_Main_Preview", guiPreviewFoldout);
                    }
                    {
                        EditorGUILayout.Space();
                        if (GUILayout.Button("?", guiPreviewHelp ? guiStyleActiveButton : GUI.skin.button, GUILayout.Width(16), GUILayout.Height(14)))
                        {
                            guiPreviewHelp = !guiPreviewHelp;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    {
                        if (guiPreviewHelp)
                        {
                            EditorGUILayout.HelpBox(Language.GetText(Language.Help.HelpPreview), MessageType.Info);
                        }
                        else
                        {
                            GUILayout.Space(2f);
                        }

                        {
                            va.PreviewGUI();
                        }
                    }
                }
                #endregion
                #endregion
            }

#if Enable_Profiler
            Profiler.EndSample();
#endif
        }

        private void OnPreSceneGUI(SceneView sceneView)
        {
            if (va.isEditError || !guiStyleReady) return;
            if (sceneView != SceneView.lastActiveSceneView) return;

            if (sceneView == EditorWindow.focusedWindow)
            {
                va.Commands();
            }
        }
        private void OnSceneGUI(SceneView sceneView)
        {
            if (va.isEditError || !guiStyleReady) return;
            if (sceneView != SceneView.lastActiveSceneView) return;

#if Enable_Profiler
            Profiler.BeginSample(string.Format("****VeryAnimationWindow.OnSceneGUI {0}", Event.current));
#endif

            Handles.matrix = Matrix4x4.identity;
            Event e = Event.current;
            var showGizmo = IsShowSceneGizmo();
            bool repaintScene = false;
            var controlID = GUIUtility.GetControlID(FocusType.Passive);

            #region Event
            switch (e.type)
            {
            case EventType.Layout:
                HandleUtility.AddDefaultControl(controlID);
                break;
            case EventType.KeyDown:
                if (focusedWindow is SceneView)
                    va.HotKeys();
                break;
            case EventType.KeyUp:
                break;
            case EventType.MouseMove:
                handleTransformUpdate = true;
                selectionRect.Reset();
                break;
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    handleTransformUpdate = true;
                }
                if (!e.alt && e.button == 0)
                {
                    selectionRect.Reset();
                    selectionRect.SetStart(e.mousePosition);
                    if (e.control || e.shift)
                    {
                        selectionRect.beforeSelection = va.selectionGameObjects != null ? va.selectionGameObjects.ToArray() : null;
                        selectionRect.virtualBeforeSelection = va.selectionHumanVirtualBones != null ? va.selectionHumanVirtualBones.ToArray() : null;
                        selectionRect.beforeAnimatorIKSelection = va.isHuman ? va.animatorIK.ikTargetSelect : null;
                        selectionRect.beforeOriginalIKSelection = va.originalIK.ikTargetSelect;
                    }
                }
                SetRepaintGUI(RepaintGUI.Edit);
                repaintScene = true;
                break;
            case EventType.MouseDrag:
                if (GUIUtility.hotControl != 0)
                    handleTransformUpdate = false;
                else
                    handleTransformUpdate = true;
                if (selectionRect.Enable)
                {
                    if (GUIUtility.hotControl == 0)
                    {
                        selectionRect.SetEnd(e.mousePosition);
                        #region Selection
                        {
                            var rect = selectionRect.rect;
                            #region Now
                            #region Bone
                            {
                                selectionRect.calcList.Clear();
                                for (int i = 0; i < va.bones.Length; i++)
                                {
                                    if (!va.IsShowBone(i) || (va.isHuman && i == 0)) continue;
                                    if (rect.Contains(HandleUtility.WorldToGUIPoint(va.editBones[i].transform.position)))
                                    {
                                        selectionRect.calcList.Add(va.bones[i]);
                                    }
                                }
                            }
                            #endregion
                            #region VirtualBone
                            {
                                selectionRect.virtualCalcList.Clear();
                                if (va.isHuman)
                                {
                                    if (va.IsShowBone(va.rootMotionBoneIndex))
                                    {
                                        if (rect.Contains(HandleUtility.WorldToGUIPoint(va.humanWorldRootPositionCache)))
                                        {
                                            selectionRect.calcList.Add(gameObject);
                                        }
                                    }
                                    for (int i = 0; i < VeryAnimation.HumanVirtualBones.Length; i++)
                                    {
                                        if (!va.IsShowVirtualBone((HumanBodyBones)i)) continue;

                                        if (rect.Contains(HandleUtility.WorldToGUIPoint(va.GetHumanVirtualBonePosition((HumanBodyBones)i))))
                                        {
                                            selectionRect.virtualCalcList.Add((HumanBodyBones)i);
                                        }
                                    }
                                }
                            }
                            #endregion
                            #region AnimatorIK
                            {
                                selectionRect.animatorIKCalcList.Clear();
                                if (va.isHuman && selectionRect.calcList.Count == 0 && selectionRect.virtualCalcList.Count == 0)
                                {
                                    for (int i = 0; i < va.animatorIK.ikData.Length; i++)
                                    {
                                        var data = va.animatorIK.ikData[i];
                                        if (!data.enable) continue;
                                        var guiPoint = HandleUtility.WorldToGUIPoint(data.worldPosition);
                                        if (!selectionRect.rect.Contains(guiPoint)) continue;
                                        selectionRect.animatorIKCalcList.Add((AnimatorIKCore.IKTarget)i);
                                    }
                                }
                            }
                            #endregion
                            #region OriginalIK
                            {
                                selectionRect.originalIKCalcList.Clear();
                                if (selectionRect.calcList.Count == 0 && selectionRect.virtualCalcList.Count == 0 && selectionRect.animatorIKCalcList.Count == 0)
                                {
                                    for (int i = 0; i < va.originalIK.ikData.Count; i++)
                                    {
                                        var data = va.originalIK.ikData[i];
                                        if (!data.enable) continue;
                                        var guiPoint = HandleUtility.WorldToGUIPoint(data.worldPosition);
                                        if (!selectionRect.rect.Contains(guiPoint)) continue;
                                        selectionRect.originalIKCalcList.Add(i);
                                    }
                                }
                            }
                            #endregion
                            #endregion
                            #region Before
                            #region Bone
                            if ((e.control || e.shift) && selectionRect.beforeSelection != null)
                            {
                                if (e.shift)
                                {
                                    foreach (var go in selectionRect.beforeSelection)
                                    {
                                        if (go == null) continue;
                                        if (!selectionRect.calcList.Contains(go))
                                            selectionRect.calcList.Add(go);
                                    }
                                }
                                else if (e.control)
                                {
                                    foreach (var go in selectionRect.beforeSelection)
                                    {
                                        if (go == null) continue;
                                        Vector3 pos;
                                        if (va.isHuman && go == gameObject)
                                        {
                                            pos = va.humanWorldRootPositionCache;
                                        }
                                        else
                                        {
                                            var boneIndex = va.BonesIndexOf(go);
                                            if (boneIndex >= 0)
                                                pos = va.editBones[boneIndex].transform.position;
                                            else
                                                pos = go.transform.position;
                                        }
                                        if (!rect.Contains(HandleUtility.WorldToGUIPoint(pos)))
                                        {
                                            if (!selectionRect.calcList.Contains(go))
                                                selectionRect.calcList.Add(go.gameObject);
                                        }
                                        else
                                        {
                                            selectionRect.calcList.Remove(go.gameObject);
                                        }
                                    }
                                }
                            }
                            #endregion
                            #region VirtualBone
                            if (va.isHuman)
                            {
                                if ((e.control || e.shift) && selectionRect.virtualBeforeSelection != null)
                                {
                                    if (e.shift)
                                    {
                                        foreach (var go in selectionRect.virtualBeforeSelection)
                                        {
                                            if (!selectionRect.virtualCalcList.Contains(go))
                                                selectionRect.virtualCalcList.Add(go);
                                        }
                                    }
                                    else if (e.control)
                                    {
                                        foreach (var go in selectionRect.virtualBeforeSelection)
                                        {
                                            if (!rect.Contains(HandleUtility.WorldToGUIPoint(va.GetHumanVirtualBonePosition(go))))
                                            {
                                                if (!selectionRect.virtualCalcList.Contains(go))
                                                    selectionRect.virtualCalcList.Add(go);
                                            }
                                            else
                                            {
                                                selectionRect.virtualCalcList.Remove(go);
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                            #region AnimatorIK
                            if (va.isHuman)
                            {
                                if ((e.control || e.shift) && selectionRect.beforeAnimatorIKSelection != null)
                                {
                                    if (e.shift)
                                    {
                                        foreach (var target in selectionRect.beforeAnimatorIKSelection)
                                        {
                                            if (!selectionRect.animatorIKCalcList.Contains(target))
                                                selectionRect.animatorIKCalcList.Add(target);
                                        }
                                    }
                                    else if (e.control)
                                    {
                                        foreach (var target in selectionRect.beforeAnimatorIKSelection)
                                        {
                                            Vector3 pos = va.animatorIK.ikData[(int)target].worldPosition;
                                            if (!rect.Contains(HandleUtility.WorldToGUIPoint(pos)))
                                            {
                                                if (!selectionRect.animatorIKCalcList.Contains(target))
                                                    selectionRect.animatorIKCalcList.Add(target);
                                            }
                                            else
                                            {
                                                selectionRect.animatorIKCalcList.Remove(target);
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                            #region OriginalIK
                            {
                                if ((e.control || e.shift) && selectionRect.beforeOriginalIKSelection != null)
                                {
                                    if (e.shift)
                                    {
                                        foreach (var target in selectionRect.beforeOriginalIKSelection)
                                        {
                                            if (!selectionRect.originalIKCalcList.Contains(target))
                                                selectionRect.originalIKCalcList.Add(target);
                                        }
                                    }
                                    else if (e.control)
                                    {
                                        foreach (var target in selectionRect.beforeOriginalIKSelection)
                                        {
                                            Vector3 pos = va.originalIK.ikData[target].worldPosition;
                                            if (!rect.Contains(HandleUtility.WorldToGUIPoint(pos)))
                                            {
                                                if (!selectionRect.originalIKCalcList.Contains(target))
                                                    selectionRect.originalIKCalcList.Add(target);
                                            }
                                            else
                                            {
                                                selectionRect.originalIKCalcList.Remove(target);
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                            #endregion
                            {
                                bool selectionChange = false;
                                #region IsChanged
                                {
                                    #region Bone
                                    {
                                        if (va.selectionGameObjects == null || va.selectionGameObjects.Count != selectionRect.calcList.Count)
                                            selectionChange = true;
                                        else if (va.selectionGameObjects != null)
                                        {
                                            for (int i = 0; i < va.selectionGameObjects.Count; i++)
                                            {
                                                if (va.selectionGameObjects[i] != selectionRect.calcList[i])
                                                {
                                                    selectionChange = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                    #region VirtualBone
                                    if (va.isHuman)
                                    {
                                        if (va.selectionHumanVirtualBones == null || va.selectionHumanVirtualBones.Count != selectionRect.virtualCalcList.Count)
                                            selectionChange = true;
                                        else if (va.selectionHumanVirtualBones != null)
                                        {
                                            for (int i = 0; i < va.selectionHumanVirtualBones.Count; i++)
                                            {
                                                if (va.selectionHumanVirtualBones[i] != selectionRect.virtualCalcList[i])
                                                {
                                                    selectionChange = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                    #region AnimatorIK
                                    if (va.isHuman)
                                    {
                                        if (va.animatorIK.ikTargetSelect == null || va.animatorIK.ikTargetSelect.Length != selectionRect.animatorIKCalcList.Count)
                                            selectionChange = true;
                                        else if (va.animatorIK.ikTargetSelect != null)
                                        {
                                            for (int i = 0; i < va.animatorIK.ikTargetSelect.Length; i++)
                                            {
                                                if (va.animatorIK.ikTargetSelect[i] != selectionRect.animatorIKCalcList[i])
                                                {
                                                    selectionChange = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                    #region OriginalIK
                                    {
                                        if (va.originalIK.ikTargetSelect == null || va.originalIK.ikTargetSelect.Length != selectionRect.originalIKCalcList.Count)
                                            selectionChange = true;
                                        else if (va.originalIK.ikTargetSelect != null)
                                        {
                                            for (int i = 0; i < va.originalIK.ikTargetSelect.Length; i++)
                                            {
                                                if (va.originalIK.ikTargetSelect[i] != selectionRect.originalIKCalcList[i])
                                                {
                                                    selectionChange = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                #endregion
                                if (selectionChange)
                                {
                                    va.SelectGameObjectMouseDrag(selectionRect.calcList.ToArray(), selectionRect.virtualCalcList.ToArray(), selectionRect.animatorIKCalcList.ToArray(), selectionRect.originalIKCalcList.ToArray());
                                    VeryAnimationControlWindow.ForceSelectionChange();
                                }
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        selectionRect.Reset();
                    }
                }
                if (e.button == 0 && GUIUtility.hotControl != 0)
                    SetRepaintGUI(RepaintGUI.Edit);
                repaintScene = true;
                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl < 0)
                    GUIUtility.hotControl = 0;
                else if (GUIUtility.hotControl == 0 && selectionRect.Enable && selectionRect.distance < 10f)
                {
                    #region SelectMesh
                    {
                        GameObject go = null;
                        var animatorIKTarget = AnimatorIKCore.IKTarget.None;
                        int originalIKTarget = -1;
                        {
                            var worldRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                            float lengthSqMin = float.MaxValue;
                            var renderers = va.editGameObject.GetComponentsInChildren<Renderer>();
                            foreach (var renderer in renderers)
                            {
                                if (renderer == null || !renderer.enabled) continue;
                                if (renderer is SkinnedMeshRenderer)
                                {
                                    #region SkinnedMeshRenderer
                                    var skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
                                    if (skinnedMeshRenderer.sharedMesh != null)
                                    {
                                        var worldToLocalMatrix = Matrix4x4.TRS(renderer.transform.position, renderer.transform.rotation, Vector3.one).inverse;
                                        var localRay = new Ray(worldToLocalMatrix.MultiplyPoint3x4(worldRay.origin), worldToLocalMatrix.MultiplyVector(worldRay.direction));
                                        Mesh mesh = new Mesh();
                                        mesh.hideFlags |= HideFlags.DontSave;
                                        skinnedMeshRenderer.BakeMesh(mesh);
                                        var vertices = mesh.vertices;
                                        BoneWeight[] boneWeights = null;
                                        Transform[] boneTransforms = null;
                                        var indices = mesh.triangles;
                                        for (int i = 0; i < indices.Length; i += 3)
                                        {
                                            Vector3 posP;
                                            if (!EditorCommon.Ray_Triangle(ref localRay, ref vertices[indices[i + 0]], ref vertices[indices[i + 1]], ref vertices[indices[i + 2]], out posP)) continue;
                                            var lengthSq = (posP - localRay.origin).sqrMagnitude;
                                            if (lengthSq > lengthSqMin)
                                                continue;

                                            if (boneWeights == null)
                                                boneWeights = skinnedMeshRenderer.sharedMesh.boneWeights;
                                            if (boneTransforms == null)
                                                boneTransforms = skinnedMeshRenderer.bones;

                                            Transform bone = null;
                                            {
                                                Dictionary<int, float> bonePoints = new Dictionary<int, float>();
                                                Action<int, float> AddBonePoint = (boneIndex, boneWeight) =>
                                                {
                                                    if (boneWeight <= 0f || boneIndex < 0 || boneIndex >= boneTransforms.Length)
                                                        return;
                                                    var t = boneTransforms[boneIndex];
                                                    var point = Vector2.Distance(HandleUtility.WorldToGUIPoint(t.position), e.mousePosition);
                                                    point = point + (point * (1f - boneWeight));
                                                    if (!bonePoints.ContainsKey(boneIndex))
                                                        bonePoints.Add(boneIndex, point);
                                                    else
                                                        bonePoints[boneIndex] = Mathf.Min(bonePoints[boneIndex], point);
                                                };
                                                for (int v = 0; v < 3; v++)
                                                {
                                                    var index = indices[i + v];
                                                    if (index >= boneWeights.Length) continue;
                                                    AddBonePoint(boneWeights[index].boneIndex0, boneWeights[index].weight0);
                                                    AddBonePoint(boneWeights[index].boneIndex1, boneWeights[index].weight1);
                                                    AddBonePoint(boneWeights[index].boneIndex2, boneWeights[index].weight2);
                                                    AddBonePoint(boneWeights[index].boneIndex3, boneWeights[index].weight3);
                                                }
                                                foreach (var pair in bonePoints.OrderBy((x) => x.Value))
                                                {
                                                    bone = boneTransforms[pair.Key];
                                                    break;
                                                }
                                            }
                                            if (bone != null)
                                            {
                                                int boneIndex = va.EditBonesIndexOf(bone.gameObject);
                                                var animatorIKTargetSub = AnimatorIKCore.IKTarget.None;
                                                int originalIKTargetSub = -1;
                                                while (boneIndex < 0 || !va.IsShowBone(boneIndex))
                                                {
                                                    #region IKTarget
                                                    if (va.isHuman)
                                                    {
                                                        var target = va.animatorIK.IsIKBone(va.boneIndex2humanoidIndex[boneIndex]);
                                                        if (target != AnimatorIKCore.IKTarget.None)
                                                        {
                                                            animatorIKTargetSub = target;
                                                            originalIKTargetSub = -1;
                                                            break;
                                                        }
                                                    }
                                                    {
                                                        var target = va.originalIK.IsIKBone(boneIndex);
                                                        if (target >= 0)
                                                        {
                                                            animatorIKTargetSub = AnimatorIKCore.IKTarget.None;
                                                            originalIKTargetSub = target;
                                                            break;
                                                        }
                                                    }
                                                    #endregion
                                                    boneIndex = va.parentBoneIndexes[boneIndex];
                                                    if (boneIndex == va.rootMotionBoneIndex)
                                                    {
                                                        if (!va.IsShowBone(boneIndex))
                                                            boneIndex = -1;
                                                        break;
                                                    }
                                                }
                                                if (boneIndex >= 0)
                                                {
                                                    lengthSqMin = lengthSq;
                                                    go = va.bones[boneIndex];
                                                    animatorIKTarget = animatorIKTargetSub;
                                                    originalIKTarget = originalIKTargetSub;
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                else if (renderer is MeshRenderer)
                                {
                                    #region MeshRenderer
                                    var worldToLocalMatrix = renderer.transform.worldToLocalMatrix;
                                    var localRay = new Ray(worldToLocalMatrix.MultiplyPoint3x4(worldRay.origin), worldToLocalMatrix.MultiplyVector(worldRay.direction));
                                    var meshFilter = renderer.GetComponent<MeshFilter>();
                                    if (meshFilter != null && meshFilter.sharedMesh != null)
                                    {
                                        var vertices = meshFilter.sharedMesh.vertices;
                                        var indices = meshFilter.sharedMesh.triangles;
                                        for (int i = 0; i < indices.Length; i += 3)
                                        {
                                            Vector3 posP;
                                            if (!EditorCommon.Ray_Triangle(ref localRay, ref vertices[indices[i + 0]], ref vertices[indices[i + 1]], ref vertices[indices[i + 2]], out posP)) continue;
                                            posP = renderer.transform.localToWorldMatrix.MultiplyPoint3x4(posP);
                                            var lengthSq = (posP - worldRay.origin).sqrMagnitude;
                                            if (lengthSq > lengthSqMin)
                                                continue;

                                            var bone = renderer.transform;
                                            {
                                                int boneIndex = va.EditBonesIndexOf(bone.gameObject);
                                                var animatorIKTargetSub = AnimatorIKCore.IKTarget.None;
                                                int originalIKTargetSub = -1;
                                                while (boneIndex < 0 || !va.IsShowBone(boneIndex))
                                                {
                                                    #region IKTarget
                                                    if (va.isHuman)
                                                    {
                                                        var target = va.animatorIK.IsIKBone(va.boneIndex2humanoidIndex[boneIndex]);
                                                        if (target != AnimatorIKCore.IKTarget.None)
                                                        {
                                                            animatorIKTargetSub = target;
                                                            originalIKTargetSub = -1;
                                                            break;
                                                        }
                                                    }
                                                    {
                                                        var target = va.originalIK.IsIKBone(boneIndex);
                                                        if (target >= 0)
                                                        {
                                                            animatorIKTargetSub = AnimatorIKCore.IKTarget.None;
                                                            originalIKTargetSub = target;
                                                            break;
                                                        }
                                                    }
                                                    #endregion
                                                    boneIndex = va.parentBoneIndexes[boneIndex];
                                                    if (boneIndex <= va.rootMotionBoneIndex)
                                                    {
                                                        if (!va.IsShowBone(boneIndex))
                                                            boneIndex = -1;
                                                        break;
                                                    }
                                                }
                                                if (boneIndex >= 0)
                                                {
                                                    lengthSqMin = lengthSq;
                                                    go = va.bones[boneIndex];
                                                    animatorIKTarget = animatorIKTargetSub;
                                                    originalIKTarget = originalIKTargetSub;
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                }
                            }
                        }
                        if (animatorIKTarget != AnimatorIKCore.IKTarget.None)
                        {
                            va.SelectAnimatorIKTargetPlusKey(animatorIKTarget);
                        }
                        else if (originalIKTarget >= 0)
                        {
                            va.SelectOriginalIKTargetPlusKey(originalIKTarget);
                        }
                        else
                        {
                            va.SelectGameObjectPlusKey(go);
                        }
                    }
                    #endregion
                }
                if (e.button == 0)
                {
                    handleTransformUpdate = true;
                }
                selectionRect.Reset();
                SetRepaintGUI(RepaintGUI.Edit);
                repaintScene = true;
                break;
            }
            #endregion

            #region SelectionRect
            if (selectionRect.Enable && selectionRect.rect.width > 0f && selectionRect.rect.height > 0f)
            {
                Handles.BeginGUI();
                GUI.Box(selectionRect.rect, "", "SelectionRect");
                Handles.EndGUI();
            }
            #endregion

            #region Tools
            if (showGizmo)
            {
                #region Handle
                {
                    bool genericHandle = false;
                    if (va.isHuman)
                    {
                        #region Humanoid
                        var humanoidIndex = va.SelectionGameObjectHumanoidIndex();
                        if (va.selectionActiveBone == va.rootMotionBoneIndex)
                        {
                            #region Root
                            if (handleTransformUpdate)
                            {
                                handlePosition = va.humanWorldRootPositionCache;
                                handleRotation = Tools.pivotRotation == PivotRotation.Local ? va.humanWorldRootRotationCache : Tools.handleRotation;
                            }
                            va.EnableCustomTools(Tool.None);
                            var currentTool = va.CurrentTool();
                            if (currentTool == Tool.Move)
                            {
                                EditorGUI.BeginChangeCheck();
                                var position = Handles.PositionHandle(handlePosition, handleRotation);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    va.SetAnimationValueAnimatorRootT(va.GetHumanLocalRootPosition(position));
                                    handlePosition = position;
                                }
                            }
                            else if (currentTool == Tool.Rotate)
                            {
                                EditorGUI.BeginChangeCheck();
                                var rotation = Handles.RotationHandle(handleRotation, handlePosition);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    if (Tools.pivotRotation == PivotRotation.Local)
                                    {
                                        va.SetAnimationValueAnimatorRootQ(va.GetHumanLocalRootRotation(rotation));
                                    }
                                    else
                                    {
                                        float angle;
                                        Vector3 axis;
                                        (Quaternion.Inverse(handleRotation) * rotation).ToAngleAxis(out angle, out axis);
                                        var bodyRotation = va.humanWorldRootRotationCache;
                                        bodyRotation = bodyRotation * Quaternion.Inverse(bodyRotation) * Quaternion.AngleAxis(angle, handleRotation * axis) * bodyRotation;
                                        va.SetAnimationValueAnimatorRootQ(va.GetHumanLocalRootRotation(bodyRotation));
                                    }
                                    Tools.handleRotation = handleRotation = rotation;
                                }
                            }
                            #endregion
                        }
                        else if (humanoidIndex == HumanBodyBones.Hips)
                        {
                            va.EnableCustomTools(Tool.None);
                        }
                        else if (humanoidIndex > HumanBodyBones.Hips)
                        {
                            #region Muscle
                            va.EnableCustomTools(Tool.None);
                            var currentTool = va.CurrentTool();
                            #region handleTransformUpdate
                            if (handleTransformUpdate)
                            {
                                if (va.editHumanoidBones[(int)humanoidIndex] != null)
                                {
                                    handlePosition = va.editHumanoidBones[(int)humanoidIndex].transform.position;
                                    handleRotation = Tools.pivotRotation == PivotRotation.Local ? va.editHumanoidBones[(int)humanoidIndex].transform.rotation : Tools.handleRotation;
                                    if (Tools.pivotMode == PivotMode.Center)
                                    {
                                        Bounds bounds;
                                        if (va.GetSelectionBounds(out bounds))
                                        {
                                            handlePosition = bounds.center;
                                        }
                                    }
                                }
                                else
                                {
                                    handlePosition = va.GetHumanVirtualBonePosition(humanoidIndex);
                                    handleRotation = Tools.pivotRotation == PivotRotation.Local ? va.GetHumanVirtualBoneRotation(humanoidIndex) : Tools.handleRotation;
                                }
                            }
                            #endregion
                            #region CenterLine
                            if (Tools.pivotMode == PivotMode.Center && (currentTool == Tool.Move || currentTool == Tool.Rotate))
                            {
                                var saveColor = Handles.color;
                                Handles.color = editorSettings.settingBoneActiveColor;
                                Vector3 pos2;
                                if (va.editHumanoidBones[(int)humanoidIndex] != null)
                                    pos2 = va.editHumanoidBones[(int)humanoidIndex].transform.position;
                                else
                                    pos2 = va.GetHumanVirtualBonePosition(humanoidIndex);
                                Handles.DrawLine(handlePosition, pos2);
                                Handles.color = saveColor;
                            }
                            #endregion
                            Action<Action<HumanBodyBones, Quaternion>> SetCenterRotationAction = (action) =>
                            {
                                if (va.selectionActiveBone < 0) return;
                                Vector3 center = va.GetSelectionOriginalBoundsCenter();
                                var activeVec = va.boneSaveTransforms[va.selectionActiveBone].position - center;
                                if (activeVec.sqrMagnitude > 0f)
                                    activeVec.Normalize();
                                var activeParentOffset = va.parentBoneIndexes[va.selectionActiveBone] >= 0 ? Quaternion.Inverse(va.boneSaveTransforms[va.parentBoneIndexes[va.selectionActiveBone]].rotation) * va.editBones[va.parentBoneIndexes[va.selectionActiveBone]].transform.rotation : Quaternion.identity;
                                activeVec = activeParentOffset * activeVec;
                                var activeUp = Quaternion.FromToRotation(activeParentOffset * Vector3.forward, activeVec) * (activeParentOffset * Vector3.up);
                                var activeRight = Quaternion.FromToRotation(activeParentOffset * Vector3.forward, activeVec) * (activeParentOffset * Vector3.right);
                                var activeUpRot = activeVec.sqrMagnitude > 0f ? Quaternion.LookRotation(activeVec, activeUp) : Quaternion.identity;
                                var activeRightRot = activeVec.sqrMagnitude > 0f ? Quaternion.LookRotation(activeVec, activeRight) : Quaternion.identity;
                                foreach (var hi in va.SelectionGameObjectsHumanoidIndex())
                                {
                                    var boneIndex = va.humanoidIndex2boneIndex[(int)hi];
                                    if (boneIndex < 0) continue;
                                    var t = va.editBones[boneIndex].transform;
                                    Quaternion different = Quaternion.identity;
                                    if (boneIndex != va.selectionActiveBone && activeVec.sqrMagnitude > 0f)
                                    {
                                        var vec = va.boneSaveTransforms[boneIndex].position - center;
                                        if (vec.sqrMagnitude > 0f)
                                        {
                                            vec.Normalize();
                                            vec = activeParentOffset * vec;
                                            if (Mathf.Abs(Vector3.Dot(vec, activeUp)) > 0.99f)
                                                different = Quaternion.LookRotation(vec, activeRight) * Quaternion.Inverse(activeRightRot);
                                            else
                                                different = Quaternion.LookRotation(vec, activeUp) * Quaternion.Inverse(activeUpRot);
                                        }
                                    }
                                    action(hi, different);
                                }
                            };
                            if (currentTool == Tool.Move)
                            {
                                #region Move
                                EditorGUI.BeginChangeCheck();
                                var position = Handles.PositionHandle(handlePosition, handleRotation);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    Action<HumanBodyBones, Vector3> ChangeTDOF = (hi, move) =>
                                    {
                                        if (va.editHumanoidBones[(int)hi] == null)
                                            return;
                                        if (VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)hi] == null)
                                            return;
                                        Quaternion rotation;
                                        {
                                            var parentHi = VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)hi].parent;
                                            while (va.editHumanoidBones[(int)parentHi] == null)
                                                parentHi = VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)parentHi].parent;
                                            var post = va.uAvatar.GetPostRotation(va.editAnimator.avatar, (int)parentHi);
                                            rotation = va.editHumanoidBones[(int)parentHi].transform.rotation * post;
                                        }
                                        var localAdd = Quaternion.Inverse(rotation) * move;
                                        {
                                            var mat = (va.editGameObject.transform.worldToLocalMatrix * va.editHumanoidBones[(int)humanoidIndex].transform.localToWorldMatrix).inverse;
                                            Vector3 lposition, lscale;
                                            Quaternion lrotation;
                                            EditorCommon.GetTRS(mat, out lposition, out lrotation, out lscale);
                                            localAdd = Vector3.Scale(localAdd, lscale);
                                        }
                                        if (va.editAnimator.humanScale > 0f)
                                            localAdd *= 1f / va.editAnimator.humanScale;
                                        else
                                            localAdd = Vector3.zero;
                                        va.SetAnimationValueAnimatorTDOF(VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)hi].index, va.GetAnimationValueAnimatorTDOF(VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)hi].index) + localAdd);
                                    };
                                    var offset = position - handlePosition;
                                    #region suppress power
                                    {
                                        var maxLevel = va.GetSelectionGameObjectsMaxLevel();
                                        if (maxLevel > 1)
                                        {
                                            var rate = 1f / maxLevel;
                                            offset *= rate;
                                        }
                                    }
                                    #endregion
                                    if (Tools.pivotMode == PivotMode.Center)
                                    {
                                        #region Center
                                        SetCenterRotationAction((hi, different) =>
                                        {
                                            ChangeTDOF(hi, different * offset);
                                        });
                                        #endregion
                                    }
                                    else
                                    {
                                        #region Pivot
                                        foreach (var hi in va.SelectionGameObjectsHumanoidIndex())
                                        {
                                            ChangeTDOF(hi, offset);
                                        }
                                        #endregion
                                    }
                                    handlePosition = position;
                                }
                                #endregion
                            }
                            else if (currentTool == Tool.Rotate)
                            {
                                #region Rotate
                                Action<Quaternion> CalcRotation = (afterRot) =>
                                {
                                    {
                                        float angle;
                                        Vector3 axis;
                                        (Quaternion.Inverse(handleRotation) * afterRot).ToAngleAxis(out angle, out axis);
                                        #region suppress power
                                        {
                                            var maxLevel = va.GetSelectionGameObjectsMaxLevel();
                                            if (maxLevel > 1)
                                            {
                                                var rate = 1f / maxLevel;
                                                angle *= rate;
                                            }
                                        }
                                        #endregion
                                        if (Tools.pivotMode == PivotMode.Center)
                                        {
                                            #region Center
                                            SetCenterRotationAction((hi, different) =>
                                            {
                                                if (va.editHumanoidBones[(int)hi] == null)
                                                    return;
                                                var handleRotationSub = different * handleRotation;
                                                va.editHumanoidBones[(int)hi].transform.Rotate(handleRotationSub * axis, angle, Space.World);
                                            });
                                            #endregion
                                        }
                                        else
                                        {
                                            #region Pivot
                                            foreach (var hi in va.SelectionGameObjectsHumanoidIndex())
                                            {
                                                if (va.editHumanoidBones[(int)hi] == null)
                                                    continue;
                                                va.editHumanoidBones[(int)hi].transform.Rotate(handleRotation * axis, angle, Space.World);
                                            }
                                            #endregion
                                        }
                                    }
                                    HumanPose hpAfter = new HumanPose();
                                    va.GetHumanPose(ref hpAfter);
                                    if (va.editHumanoidBones[(int)HumanBodyBones.Neck] == null)
                                    {
                                        if (va.IsSelectionGameObjectsHumanoidIndexContains(HumanBodyBones.Head))
                                        {
                                            for (int dof = 0; dof < 3; dof++)
                                            {
                                                var muscleIndex = HumanTrait.MuscleFromBone((int)HumanBodyBones.Neck, dof);
                                                if (muscleIndex < 0) continue;
                                                va.SetAnimationValueAnimatorMuscle(muscleIndex, 0f);
                                            }
                                        }
                                    }
                                    foreach (var muscleIndex in va.SelectionGameObjectsMuscleIndex(-1))
                                    {
                                        var hi = (HumanBodyBones)HumanTrait.BoneFromMuscle(muscleIndex);
                                        if (va.editHumanoidBones[(int)hi] == null)
                                            continue;
                                        var muscle = hpAfter.muscles[muscleIndex];
                                        if (va.clampMuscle)
                                            muscle = Mathf.Clamp(muscle, -1f, 1f);
                                        va.SetAnimationValueAnimatorMuscle(muscleIndex, muscle);
                                    }
                                };
                                {
                                    if (muscleRotationHandleIds == null || muscleRotationHandleIds.Length != 3)
                                        muscleRotationHandleIds = new int[3];
                                    for (int i = 0; i < muscleRotationHandleIds.Length; i++)
                                        muscleRotationHandleIds[i] = -1;
                                }
                                if (Tools.pivotRotation == PivotRotation.Local && Tools.pivotMode == PivotMode.Pivot)
                                {
                                    #region LocalPivot
                                    Color saveColor = Handles.color;
                                    float handleSize = HandleUtility.GetHandleSize(handlePosition);
                                    {
                                        Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                                        EditorGUI.BeginChangeCheck();
                                        var rotation = Handles.FreeRotateHandle(handleRotation, handlePosition, handleSize);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            CalcRotation(rotation);
                                            Tools.handleRotation = handleRotation = rotation;
                                        }
                                    }
                                    {
                                        EditorGUI.BeginChangeCheck();
                                        int rotDofMode = -1;
                                        float rotDofDist = 0f;
                                        Quaternion rotDofHandleRotation = Quaternion.identity;
                                        Quaternion rotDofAfterRotation = Quaternion.identity;
                                        #region MuscleRotationHandle
                                        Transform t = null;
                                        if (va.selectionActiveBone >= 0)
                                            t = va.editBones[va.selectionActiveBone].transform;
                                        Quaternion preRotation = va.uAvatar.GetPreRotation(va.editAnimator.avatar, (int)humanoidIndex);
                                        var snapRotation = uSnapSettings.rotation;
                                        {
                                            if (HumanTrait.MuscleFromBone((int)humanoidIndex, 0) >= 0)
                                            {
                                                Handles.color = Handles.xAxisColor;
                                                EditorGUI.BeginChangeCheck();
                                                Quaternion hRotation;
                                                if (t != null)
                                                    hRotation = va.uAvatar.GetZYPostQ(va.editAnimator.avatar, (int)humanoidIndex, t.parent.rotation, t.rotation);
                                                else
                                                    hRotation = va.GetHumanVirtualBoneRotation(humanoidIndex);
                                                var rotDofDistSave = uDisc.GetRotationDist();
                                                var rotation = Handles.Disc(hRotation, handlePosition, hRotation * Vector3.right, handleSize, true, snapRotation);
                                                muscleRotationHandleIds[0] = uEditorGUIUtility.GetLastControlID();
                                                if (EditorGUI.EndChangeCheck())
                                                {
                                                    rotDofMode = 0;
                                                    rotDofDist = uDisc.GetRotationDist() - rotDofDistSave;
                                                    rotDofHandleRotation = hRotation;
                                                    rotDofAfterRotation = rotation;
                                                }
                                            }
                                            if (HumanTrait.MuscleFromBone((int)humanoidIndex, 1) >= 0)
                                            {
                                                Handles.color = Handles.yAxisColor;
                                                EditorGUI.BeginChangeCheck();
                                                Quaternion hRotation;
                                                if (t != null)
                                                    hRotation = t.parent.rotation * preRotation;
                                                else
                                                    hRotation = va.GetHumanVirtualBoneParentRotation(humanoidIndex);
                                                var rotDofDistSave = uDisc.GetRotationDist();
                                                var rotation = Handles.Disc(hRotation, handlePosition, hRotation * Vector3.up, handleSize, true, snapRotation);
                                                muscleRotationHandleIds[1] = uEditorGUIUtility.GetLastControlID();
                                                if (EditorGUI.EndChangeCheck())
                                                {
                                                    rotDofMode = 1;
                                                    rotDofDist = uDisc.GetRotationDist() - rotDofDistSave;
                                                    rotDofHandleRotation = hRotation;
                                                    rotDofAfterRotation = rotation;
                                                }
                                            }
                                            if (HumanTrait.MuscleFromBone((int)humanoidIndex, 2) >= 0)
                                            {
                                                Handles.color = Handles.zAxisColor;
                                                EditorGUI.BeginChangeCheck();
                                                Quaternion hRotation;
                                                if (t != null)
                                                    hRotation = t.parent.rotation * preRotation;
                                                else
                                                    hRotation = va.GetHumanVirtualBoneParentRotation(humanoidIndex);
                                                var rotDofDistSave = uDisc.GetRotationDist();
                                                var rotation = Handles.Disc(hRotation, handlePosition, hRotation * Vector3.forward, handleSize, true, snapRotation);
                                                muscleRotationHandleIds[2] = uEditorGUIUtility.GetLastControlID();
                                                if (EditorGUI.EndChangeCheck())
                                                {
                                                    rotDofMode = 2;
                                                    rotDofDist = uDisc.GetRotationDist() - rotDofDistSave;
                                                    rotDofHandleRotation = hRotation;
                                                    rotDofAfterRotation = rotation;
                                                }
                                            }
                                        }
                                        #endregion
                                        if (rotDofMode >= 0 && rotDofMode <= 2)
                                        {
                                            #region suppress power
                                            {
                                                var maxLevel = va.GetSelectionGameObjectsMaxLevel();
                                                if (maxLevel > 1)
                                                {
                                                    var rate = 1f / maxLevel;
                                                    rotDofDist *= rate;
                                                }
                                            }
                                            #endregion
                                            foreach (var hi in va.SelectionGameObjectsHumanoidIndex())
                                            {
                                                var muscleIndex = HumanTrait.MuscleFromBone((int)hi, rotDofMode);
                                                var muscle = va.GetAnimationValueAnimatorMuscle(muscleIndex);
                                                {
                                                    var muscleLimit = va.humanoidMuscleLimit[(int)hi];
                                                    var value = muscleLimit.max[rotDofMode] - muscleLimit.min[rotDofMode];
                                                    if (value > 0f)
                                                    {
                                                        var add = rotDofDist / (value / 2f);
                                                        Vector3 limitSign;
                                                        if (va.editHumanoidBones[(int)hi] != null)
                                                            limitSign = va.uAvatar.GetLimitSign(va.editAnimator.avatar, (int)hi);
                                                        else
                                                            limitSign = va.GetHumanVirtualBoneLimitSign(hi);
                                                        muscle -= add * limitSign[rotDofMode];
                                                    }
                                                }
                                                if (va.clampMuscle)
                                                    muscle = Mathf.Clamp(muscle, -1f, 1f);
                                                va.SetAnimationValueAnimatorMuscle(muscleIndex, muscle);
                                            }
                                        }
                                    }
                                    if (va.editHumanoidBones[(int)humanoidIndex] != null)
                                    {
                                        Handles.color = Handles.centerColor;
                                        EditorGUI.BeginChangeCheck();
                                        var rotation = Handles.Disc(handleRotation, handlePosition, Camera.current.transform.forward, handleSize * 1.1f, false, 0f);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            CalcRotation(rotation);
                                            Tools.handleRotation = handleRotation = rotation;
                                        }
                                    }
                                    Handles.color = saveColor;
                                    #endregion
                                }
                                else
                                {
                                    #region Other
                                    if (va.editHumanoidBones[(int)humanoidIndex] != null)
                                    {
                                        EditorGUI.BeginChangeCheck();
                                        var rotation = Handles.RotationHandle(handleRotation, handlePosition);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            CalcRotation(rotation);
                                            Tools.handleRotation = handleRotation = rotation;
                                        }
                                    }
                                    #endregion
                                }

                                #endregion
                            }
                            #endregion
                        }
                        else if (va.selectionActiveBone >= 0)
                        {
                            genericHandle = true;
                        }
                        else
                        {
                            va.EnableCustomTools(Tool.None);
                        }
                        #endregion
                    }
                    else if (va.selectionActiveBone >= 0)
                    {
                        #region Generic
                        if (va.selectionActiveBone == va.rootMotionBoneIndex)
                        {
                            #region Root
                            if (handleTransformUpdate)
                            {
                                handlePosition = va.editBones[va.selectionActiveBone].transform.position;
                                handleRotation = Tools.pivotRotation == PivotRotation.Local && va.selectionActiveBone >= 0 ? va.editBones[va.selectionActiveBone].transform.rotation : Tools.handleRotation;
                            }
                            va.EnableCustomTools(Tool.None);
                            var currentTool = va.CurrentTool();
                            if (currentTool == Tool.Move)
                            {
                                EditorGUI.BeginChangeCheck();
                                var position = Handles.PositionHandle(handlePosition, handleRotation);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    var offset = position - handlePosition;
                                    {
                                        var t = va.editBones[va.selectionActiveBone].transform;
                                        t.Translate(offset, Space.World);
                                        va.SetAnimationValueAnimatorRootT(t.localPosition);
                                    }
                                    handlePosition = position;
                                }
                            }
                            else if (currentTool == Tool.Rotate)
                            {
                                EditorGUI.BeginChangeCheck();
                                var rotation = Handles.RotationHandle(handleRotation, handlePosition);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    float angle;
                                    Vector3 axis;
                                    (Quaternion.Inverse(handleRotation) * rotation).ToAngleAxis(out angle, out axis);
                                    {
                                        var t = va.editBones[va.selectionActiveBone].transform;
                                        t.Rotate(handleRotation * axis, angle, Space.World);
                                        va.SetAnimationValueAnimatorRootQ(t.localRotation);
                                    }
                                    Tools.handleRotation = handleRotation = rotation;
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            genericHandle = true;
                        }
                        #endregion
                    }
                    else
                    {
                        va.EnableCustomTools(Tool.None);
                    }
                    if (genericHandle && va.selectionActiveBone >= 0)
                    {
                        #region GenericHandle
                        va.EnableCustomTools(Tool.None);
                        var currentTool = va.CurrentTool();
                        #region handleTransformUpdate
                        if (handleTransformUpdate)
                        {
                            handlePosition = va.editBones[va.selectionActiveBone].transform.position;
                            handleRotation = Tools.pivotRotation == PivotRotation.Local && va.selectionActiveBone >= 0 ? va.editBones[va.selectionActiveBone].transform.rotation : Tools.handleRotation;
                            if (Tools.pivotMode == PivotMode.Center && va.selectionActiveBone >= 0)
                            {
                                Bounds bounds;
                                if (va.GetSelectionBounds(out bounds))
                                {
                                    handlePosition = bounds.center;
                                    if (Tools.pivotRotation == PivotRotation.Local)
                                    {
                                        var activeVec = va.editBones[va.selectionActiveBone].transform.position - bounds.center;
                                        if (activeVec.sqrMagnitude > 0f)
                                        {
                                            activeVec.Normalize();
                                            handleRotation = Quaternion.LookRotation(activeVec, va.editBones[va.selectionActiveBone].transform.up);
                                        }
                                    }
                                }
                            }
                            handleScale = Vector3.one;
                        }
                        #endregion
                        #region CenterLine
                        if (Tools.pivotMode == PivotMode.Center && (currentTool == Tool.Move || currentTool == Tool.Rotate))
                        {
                            var saveColor = Handles.color;
                            Handles.color = editorSettings.settingBoneActiveColor;
                            Handles.DrawLine(handlePosition, va.editBones[va.selectionActiveBone].transform.position);
                            Handles.color = saveColor;
                        }
                        #endregion
                        Action<Action<int, Quaternion>> SetCenterRotationAction = (action) =>
                        {
                            Vector3 center = va.GetSelectionOriginalBoundsCenter();
                            var activeVec = va.boneSaveTransforms[va.selectionActiveBone].position - center;
                            if (activeVec.sqrMagnitude > 0f)
                                activeVec.Normalize();
                            var activeParentOffset = va.parentBoneIndexes[va.selectionActiveBone] >= 0 ? Quaternion.Inverse(va.boneSaveTransforms[va.parentBoneIndexes[va.selectionActiveBone]].rotation) * va.editBones[va.parentBoneIndexes[va.selectionActiveBone]].transform.rotation : Quaternion.identity;
                            activeVec = activeParentOffset * activeVec;
                            var activeUp = Quaternion.FromToRotation(activeParentOffset * Vector3.forward, activeVec) * (activeParentOffset * Vector3.up);
                            var activeRight = Quaternion.FromToRotation(activeParentOffset * Vector3.forward, activeVec) * (activeParentOffset * Vector3.right);
                            var activeUpRot = activeVec.sqrMagnitude > 0f ? Quaternion.LookRotation(activeVec, activeUp) : Quaternion.identity;
                            var activeRightRot = activeVec.sqrMagnitude > 0f ? Quaternion.LookRotation(activeVec, activeRight) : Quaternion.identity;
                            foreach (var boneIndex in va.SelectionGameObjectsOtherHumanoidBoneIndex())
                            {
                                var t = va.editBones[boneIndex].transform;
                                Quaternion different = Quaternion.identity;
                                if (boneIndex != va.selectionActiveBone && activeVec.sqrMagnitude > 0f)
                                {
                                    var vec = va.boneSaveTransforms[boneIndex].position - center;
                                    if (vec.sqrMagnitude > 0f)
                                    {
                                        vec.Normalize();
                                        vec = activeParentOffset * vec;
                                        if (Mathf.Abs(Vector3.Dot(vec, activeUp)) > 0.99f)
                                            different = Quaternion.LookRotation(vec, activeRight) * Quaternion.Inverse(activeRightRot);
                                        else
                                            different = Quaternion.LookRotation(vec, activeUp) * Quaternion.Inverse(activeUpRot);
                                    }
                                }
                                action(boneIndex, different);
                            }
                        };
                        if (currentTool == Tool.Move)
                        {
                            #region Move
                            EditorGUI.BeginChangeCheck();
                            var position = Handles.PositionHandle(handlePosition, handleRotation);
                            if (EditorGUI.EndChangeCheck())
                            {
                                var offset = position - handlePosition;
                                #region suppress power
                                {
                                    var maxLevel = va.GetSelectionGameObjectsMaxLevel();
                                    if (maxLevel > 1)
                                    {
                                        var rate = 1f / maxLevel;
                                        offset *= rate;
                                    }
                                }
                                #endregion
                                if (Tools.pivotMode == PivotMode.Center)
                                {
                                    #region Center
                                    SetCenterRotationAction((boneIndex, different) =>
                                    {
                                        var t = va.editBones[boneIndex].transform;
                                        t.Translate(different * offset, Space.World);
                                        va.SetAnimationValueTransformPosition(boneIndex, t.localPosition);
                                    });
                                    #endregion
                                }
                                else
                                {
                                    #region Pivot
                                    foreach (var boneIndex in va.SelectionGameObjectsOtherHumanoidBoneIndex())
                                    {
                                        var t = va.editBones[boneIndex].transform;
                                        t.Translate(offset, Space.World);
                                        va.SetAnimationValueTransformPosition(boneIndex, t.localPosition);
                                    }
                                    #endregion
                                }
                                handlePosition = position;
                            }
                            #endregion
                        }
                        else if (currentTool == Tool.Rotate)
                        {
                            #region Rotate
                            EditorGUI.BeginChangeCheck();
                            var rotation = Handles.RotationHandle(handleRotation, handlePosition);
                            if (EditorGUI.EndChangeCheck())
                            {
                                float angle;
                                Vector3 axis;
                                (Quaternion.Inverse(handleRotation) * rotation).ToAngleAxis(out angle, out axis);
                                #region suppress power
                                {
                                    var maxLevel = va.GetSelectionGameObjectsMaxLevel();
                                    if (maxLevel > 1)
                                    {
                                        var rate = 1f / maxLevel;
                                        angle *= rate;
                                    }
                                }
                                #endregion
                                if (Tools.pivotMode == PivotMode.Center)
                                {
                                    #region Center
                                    SetCenterRotationAction((boneIndex, different) =>
                                    {
                                        var t = va.editBones[boneIndex].transform;
                                        var handleRotationSub = different * handleRotation;
                                        t.Rotate(handleRotationSub * axis, angle, Space.World);
                                        va.SetAnimationValueTransformRotation(boneIndex, t.localRotation);
                                    });
                                    #endregion
                                }
                                else
                                {
                                    #region Pivot
                                    foreach (var boneIndex in va.SelectionGameObjectsOtherHumanoidBoneIndex())
                                    {
                                        var t = va.editBones[boneIndex].transform;
                                        t.Rotate(handleRotation * axis, angle, Space.World);
                                        va.SetAnimationValueTransformRotation(boneIndex, t.localRotation);
                                    }
                                    #endregion
                                }
                                Tools.handleRotation = handleRotation = rotation;
                            }
                            #endregion
                        }
                        else if (currentTool == Tool.Scale)
                        {
                            #region Scale
                            if (Tools.pivotRotation == PivotRotation.Local)
                            {
                                EditorGUI.BeginChangeCheck();
                                var scale = Handles.ScaleHandle(handleScale, handlePosition, handleRotation, HandleUtility.GetHandleSize(handlePosition));
                                if (EditorGUI.EndChangeCheck())
                                {
                                    var offset = scale - handleScale;
                                    #region suppress power
                                    {
                                        var maxLevel = va.GetSelectionGameObjectsMaxLevel();
                                        if (maxLevel > 1)
                                        {
                                            var rate = 1f / maxLevel;
                                            offset *= rate;
                                        }
                                    }
                                    #endregion
                                    foreach (var boneIndex in va.SelectionGameObjectsOtherHumanoidBoneIndex())
                                    {
                                        var t = va.editBones[boneIndex].transform;
                                        t.localScale += offset;
                                        va.SetAnimationValueTransformScale(boneIndex, t.localScale);
                                    }
                                    handleScale = scale;
                                }
                            }
                            #endregion
                        }
                        #endregion
                    }
                }
                #endregion

                #region IK
                va.IKHandleGUI();
                va.IKTargetGUI();
                #endregion

                if (e.type == EventType.Repaint)
                {
                    #region Skeleton
                    if (editorSettings.settingsSkeletonType != EditorSettings.SkeletonType.None && va.skeletonShowBoneList != null)
                    {
                        foreach (var boneIndex in va.skeletonShowBoneList)
                        {
                            DrawArrow(va.editBones[va.parentBoneIndexes[boneIndex]].transform.position, va.editBones[boneIndex].transform.position);
                        }
                    }
                    #endregion

                    #region MuscleLimit
                    if (va.isHuman && editorSettings.settingBoneMuscleLimit && va.clampMuscle &&
                        Tools.pivotMode == PivotMode.Pivot &&
                        muscleRotationHandleIds != null && muscleRotationHandleIds.Length == 3 &&
                        muscleRotationSliderIds != null && muscleRotationSliderIds.Length == 3)
                    {
                        var humanoidIndex = (int)va.SelectionGameObjectHumanoidIndex();
                        if (humanoidIndex >= 0 && va.CurrentTool() == Tool.Rotate)
                        {
                            Transform t = null;
                            if (va.editHumanoidBones[humanoidIndex] != null)
                                t = va.editHumanoidBones[humanoidIndex].transform;
                            Avatar avatar = va.editAnimator.avatar;
                            int index1 = HumanTrait.MuscleFromBone(humanoidIndex, 0);
                            int index2 = HumanTrait.MuscleFromBone(humanoidIndex, 1);
                            int index3 = HumanTrait.MuscleFromBone(humanoidIndex, 2);
                            float axisLength = HandleUtility.GetHandleSize(handlePosition);
                            Quaternion quaternion1, quaternion2;
                            {
                                Quaternion preRotation = va.uAvatar.GetPreRotation(avatar, humanoidIndex);
                                Quaternion postRotation = va.uAvatar.GetPostRotation(avatar, humanoidIndex);
                                quaternion1 = t != null ? t.parent.rotation * preRotation : va.GetHumanVirtualBoneParentRotation((HumanBodyBones)humanoidIndex);
                                quaternion2 = t != null ? t.rotation * postRotation : va.GetHumanVirtualBoneRotation((HumanBodyBones)humanoidIndex);
                            }
                            Quaternion zyRoll = va.uAvatar.GetZYRoll(avatar, humanoidIndex, Vector3.zero);
                            Vector3 limitSign = t != null ? va.uAvatar.GetLimitSign(avatar, humanoidIndex) : va.GetHumanVirtualBoneLimitSign((HumanBodyBones)humanoidIndex);
                            //X
                            Vector3 normalX = Vector3.zero, fromX = Vector3.zero, lineX = Vector3.zero;
                            float angleX = 0f, radiusX = 0f;
                            if (index1 != -1)
                            {
                                Quaternion zyPostQ = t != null ? va.uAvatar.GetZYPostQ(avatar, humanoidIndex, t.parent.rotation, t.rotation) : quaternion1;
                                float angle = va.humanoidMuscleLimit[humanoidIndex].min.x;
                                float num = va.humanoidMuscleLimit[humanoidIndex].max.x;
                                float length = axisLength;
                                if (va.musclePropertyName.Names[index1].StartsWith("Left") || va.musclePropertyName.Names[index1].StartsWith("Right")) //why?
                                {
                                    angle *= 0.5f;
                                    num *= 0.5f;
                                }
                                Vector3 vector3_3 = zyPostQ * Vector3.forward;
                                Vector3 vector3_5 = quaternion2 * Vector3.right * limitSign.x;
                                Vector3 from = Quaternion.AngleAxis(angle, vector3_5) * vector3_3;

                                normalX = vector3_5;
                                fromX = from;
                                angleX = num - angle;
                                radiusX = length;
                                Vector3 lineVec = Quaternion.AngleAxis(Mathf.Lerp(angle, num, (va.GetAnimationValueAnimatorMuscle(index1) + 1f) / 2f), vector3_5) * vector3_3;
                                lineX = handlePosition + lineVec * length;
                            }
                            //Y
                            Vector3 normalY = Vector3.zero, fromY = Vector3.zero, lineY = Vector3.zero;
                            float angleY = 0f, radiusY = 0f;
                            if (index2 != -1)
                            {
                                float angle = va.humanoidMuscleLimit[humanoidIndex].min.y;
                                float num = va.humanoidMuscleLimit[humanoidIndex].max.y;
                                float length = axisLength;
                                Vector3 vector3_2 = quaternion1 * Vector3.up * limitSign.y;
                                Vector3 vector3_3 = quaternion1 * zyRoll * Vector3.right;
                                Vector3 from = Quaternion.AngleAxis(angle, vector3_2) * vector3_3;

                                normalY = vector3_2;
                                fromY = from;
                                angleY = num - angle;
                                radiusY = length;
                                Vector3 lineVec = Quaternion.AngleAxis(Mathf.Lerp(angle, num, (va.GetAnimationValueAnimatorMuscle(index2) + 1f) / 2f), vector3_2) * vector3_3;
                                lineY = handlePosition + lineVec * length;
                            }
                            //Z
                            Vector3 normalZ = Vector3.zero, fromZ = Vector3.zero, lineZ = Vector3.zero;
                            float angleZ = 0f, radiusZ = 0f;
                            if (index3 != -1)
                            {
                                float angle = va.humanoidMuscleLimit[humanoidIndex].min.z;
                                float num = va.humanoidMuscleLimit[humanoidIndex].max.z;
                                float length = axisLength;
                                Vector3 vector3_7 = quaternion1 * Vector3.forward * limitSign.z;
                                Vector3 vector3_8 = quaternion1 * zyRoll * Vector3.right;
                                Vector3 from = Quaternion.AngleAxis(angle, vector3_7) * vector3_8;

                                normalZ = vector3_7;
                                fromZ = from;
                                angleZ = num - angle;
                                radiusZ = length;
                                Vector3 lineVec = Quaternion.AngleAxis(Mathf.Lerp(angle, num, (va.GetAnimationValueAnimatorMuscle(index3) + 1f) / 2f), vector3_7) * vector3_8;
                                lineZ = handlePosition + lineVec * length;
                            }
                            if (GUIUtility.hotControl == muscleRotationHandleIds[0])
                            {
                                #region DrawY
                                if (index2 != -1)
                                {
                                    Color color = muscleRotationHandleIds[1] == GUIUtility.hotControl || muscleRotationSliderIds[1] == GUIUtility.hotControl ? new Color(1f, 1f, 1f, 0.5f) : new Color(1, 1, 1, 0.2f);
                                    Handles.color = Handles.yAxisColor * color;
                                    Handles.DrawSolidArc(handlePosition, normalY, fromY, angleY, radiusY);
                                    Handles.color = new Color(1f, 0f, 1f, Handles.color.a);
                                    Handles.DrawLine(handlePosition, lineY);
                                }
                                #endregion
                                #region DrawZ
                                if (index3 != -1)
                                {
                                    Color color = muscleRotationHandleIds[2] == GUIUtility.hotControl || muscleRotationSliderIds[2] == GUIUtility.hotControl ? new Color(1f, 1f, 1f, 0.5f) : new Color(1, 1, 1, 0.2f);
                                    Handles.color = Handles.zAxisColor * color;
                                    Handles.DrawSolidArc(handlePosition, normalZ, fromZ, angleZ, radiusZ);
                                    Handles.color = new Color(1f, 1f, 0f, Handles.color.a);
                                    Handles.DrawLine(handlePosition, lineZ);
                                }
                                #endregion
                                #region DrawX
                                if (index1 != -1)
                                {
                                    Color color = muscleRotationHandleIds[0] == GUIUtility.hotControl || muscleRotationSliderIds[0] == GUIUtility.hotControl ? new Color(1f, 1f, 1f, 0.5f) : new Color(1, 1, 1, 0.2f);
                                    Handles.color = Handles.xAxisColor * color;
                                    Handles.DrawSolidArc(handlePosition, normalX, fromX, angleX, radiusX);
                                    Handles.color = new Color(0f, 1f, 1f, Handles.color.a);
                                    Handles.DrawLine(handlePosition, lineX);
                                }
                                #endregion
                            }
                            else if (GUIUtility.hotControl == muscleRotationHandleIds[1])
                            {
                                #region DrawX
                                if (index1 != -1)
                                {
                                    Color color = muscleRotationHandleIds[0] == GUIUtility.hotControl || muscleRotationSliderIds[0] == GUIUtility.hotControl ? new Color(1f, 1f, 1f, 0.5f) : new Color(1, 1, 1, 0.2f);
                                    Handles.color = Handles.xAxisColor * color;
                                    Handles.DrawSolidArc(handlePosition, normalX, fromX, angleX, radiusX);
                                    Handles.color = new Color(0f, 1f, 1f, Handles.color.a);
                                    Handles.DrawLine(handlePosition, lineX);
                                }
                                #endregion
                                #region DrawZ
                                if (index3 != -1)
                                {
                                    Color color = muscleRotationHandleIds[2] == GUIUtility.hotControl || muscleRotationSliderIds[2] == GUIUtility.hotControl ? new Color(1f, 1f, 1f, 0.5f) : new Color(1, 1, 1, 0.2f);
                                    Handles.color = Handles.zAxisColor * color;
                                    Handles.DrawSolidArc(handlePosition, normalZ, fromZ, angleZ, radiusZ);
                                    Handles.color = new Color(1f, 1f, 0f, Handles.color.a);
                                    Handles.DrawLine(handlePosition, lineZ);
                                }
                                #endregion
                                #region DrawY
                                if (index2 != -1)
                                {
                                    Color color = muscleRotationHandleIds[1] == GUIUtility.hotControl || muscleRotationSliderIds[1] == GUIUtility.hotControl ? new Color(1f, 1f, 1f, 0.5f) : new Color(1, 1, 1, 0.2f);
                                    Handles.color = Handles.yAxisColor * color;
                                    Handles.DrawSolidArc(handlePosition, normalY, fromY, angleY, radiusY);
                                    Handles.color = new Color(1f, 0f, 1f, Handles.color.a);
                                    Handles.DrawLine(handlePosition, lineY);
                                }
                                #endregion
                            }
                            else
                            {
                                #region DrawX
                                if (index1 != -1)
                                {
                                    Color color = muscleRotationHandleIds[0] == GUIUtility.hotControl || muscleRotationSliderIds[0] == GUIUtility.hotControl ? new Color(1f, 1f, 1f, 0.5f) : new Color(1, 1, 1, 0.2f);
                                    Handles.color = Handles.xAxisColor * color;
                                    Handles.DrawSolidArc(handlePosition, normalX, fromX, angleX, radiusX);
                                    Handles.color = new Color(0f, 1f, 1f, Handles.color.a);
                                    Handles.DrawLine(handlePosition, lineX);
                                }
                                #endregion
                                #region DrawY
                                if (index2 != -1)
                                {
                                    Color color = muscleRotationHandleIds[1] == GUIUtility.hotControl || muscleRotationSliderIds[1] == GUIUtility.hotControl ? new Color(1f, 1f, 1f, 0.5f) : new Color(1, 1, 1, 0.2f);
                                    Handles.color = Handles.yAxisColor * color;
                                    Handles.DrawSolidArc(handlePosition, normalY, fromY, angleY, radiusY);
                                    Handles.color = new Color(1f, 0f, 1f, Handles.color.a);
                                    Handles.DrawLine(handlePosition, lineY);
                                }
                                #endregion
                                #region DrawZ
                                if (index3 != -1)
                                {
                                    Color color = muscleRotationHandleIds[2] == GUIUtility.hotControl || muscleRotationSliderIds[2] == GUIUtility.hotControl ? new Color(1f, 1f, 1f, 0.5f) : new Color(1, 1, 1, 0.2f);
                                    Handles.color = Handles.zAxisColor * color;
                                    Handles.DrawSolidArc(handlePosition, normalZ, fromZ, angleZ, radiusZ);
                                    Handles.color = new Color(1f, 1f, 0f, Handles.color.a);
                                    Handles.DrawLine(handlePosition, lineZ);
                                }
                                #endregion
                            }
                        }
                    }
                    #endregion
                }

                #region Bones
                {
                    var bkColor = GUI.color;
                    Handles.BeginGUI();

                    #region Bones
                    for (int i = 0; i < va.bones.Length; i++)
                    {
                        if (i == va.rootMotionBoneIndex) continue;
                        if (!va.IsShowBone(i)) continue;

                        var pos = HandleUtility.WorldToGUIPoint(va.editBones[i].transform.position);
                        var rect = new Rect(pos.x + 2 - editorSettings.settingBoneButtonSize / 2f, pos.y - 2 - editorSettings.settingBoneButtonSize / 2f, editorSettings.settingBoneButtonSize, editorSettings.settingBoneButtonSize);     //Why is it shifted. I do not know the cause 2
                        bool selected = va.SelectionGameObjectsIndexOf(va.bones[i]) >= 0;
                        GUIStyle guiStyle = guiStyleCircleButton;
                        if (va.isHuman)
                        {
                            if (i == va.rootMotionBoneIndex)
                                guiStyle = guiStyleCircle3Button;
                            else if (va.boneIndex2humanoidIndex[i] < 0)
                                guiStyle = guiStyleDiamondButton;
                        }
                        else
                        {
                            if (va.rootMotionBoneIndex >= 0)
                            {
                                if (i == va.rootMotionBoneIndex)
                                    guiStyle = guiStyleCircle3Button;
                            }
                            else
                            {
                                if (i == 0)
                                    guiStyle = guiStyleCircle3Button;
                            }
                        }
                        if (selected) GUI.color = editorSettings.settingBoneActiveColor;
                        else GUI.color = editorSettings.settingBoneNormalColor;

                        if (GUI.Button(rect, "", guiStyle))
                        {
                            va.SelectGameObjectPlusKey(va.bones[i]);
                        }
                    }
                    #endregion

                    if (va.isHuman)
                    {
                        #region Virtual
                        {
                            for (int i = 0; i < VeryAnimation.HumanVirtualBones.Length; i++)
                            {
                                if (!va.IsShowVirtualBone((HumanBodyBones)i)) continue;

                                var pos = HandleUtility.WorldToGUIPoint(va.GetHumanVirtualBonePosition((HumanBodyBones)i));
                                var rect = new Rect(pos.x - editorSettings.settingBoneButtonSize / 2f, pos.y - editorSettings.settingBoneButtonSize / 2f, editorSettings.settingBoneButtonSize, editorSettings.settingBoneButtonSize);
                                bool selected = va.SelectionGameObjectsContains((HumanBodyBones)i);
                                if (selected) GUI.color = editorSettings.settingBoneActiveColor;
                                else GUI.color = editorSettings.settingBoneNormalColor;
                                GUIStyle guiStyle = guiStyleCircleDotButton;
                                if (GUI.Button(rect, "", guiStyle))
                                {
                                    va.SelectVirtualBonePlusKey((HumanBodyBones)i);
                                    VeryAnimationControlWindow.ForceSelectionChange();
                                }
                            }
                        }
                        #endregion

                        #region Root
                        if (va.IsShowBone(va.rootMotionBoneIndex))
                        {
                            var pos = HandleUtility.WorldToGUIPoint(va.humanWorldRootPositionCache);
                            var rect = new Rect(pos.x - editorSettings.settingBoneButtonSize / 2f, pos.y - editorSettings.settingBoneButtonSize / 2f, editorSettings.settingBoneButtonSize, editorSettings.settingBoneButtonSize);
                            bool selected = va.SelectionGameObjectsIndexOf(gameObject) >= 0;
                            if (selected) GUI.color = editorSettings.settingBoneActiveColor;
                            else GUI.color = editorSettings.settingBoneNormalColor;
                            if (GUI.Button(rect, "", guiStyleCircle3Button))
                            {
                                va.SelectGameObjectPlusKey(gameObject);
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region Root
                        if (va.IsShowBone(va.rootMotionBoneIndex))
                        {
                            var pos = HandleUtility.WorldToGUIPoint(va.editBones[va.rootMotionBoneIndex].transform.position);
                            var rect = new Rect(pos.x - editorSettings.settingBoneButtonSize / 2f, pos.y - editorSettings.settingBoneButtonSize / 2f, editorSettings.settingBoneButtonSize, editorSettings.settingBoneButtonSize);
                            bool selected = va.SelectionGameObjectsIndexOf(va.bones[va.rootMotionBoneIndex]) >= 0;
                            if (selected) GUI.color = editorSettings.settingBoneActiveColor;
                            else GUI.color = editorSettings.settingBoneNormalColor;
                            if (GUI.Button(rect, "", guiStyleCircle3Button))
                            {
                                va.SelectGameObjectPlusKey(va.bones[va.rootMotionBoneIndex]);
                            }
                        }
                        #endregion
                    }

                    Handles.EndGUI();
                    GUI.color = bkColor;
                }
                #endregion
            }
            #endregion

            if (repaintScene)
            {
                sceneView.Repaint();
            }

#if Enable_Profiler
            Profiler.EndSample();
#endif
        }

        private void DrawArrow(Vector3 posA, Vector3 posB)
        {
            if (editorSettings.settingsSkeletonType == EditorSettings.SkeletonType.Line)
            {
                Handles.color = editorSettings.settingSkeletonColor;
                Handles.DrawLine(posA, posB);
            }
            else if (editorSettings.settingsSkeletonType == EditorSettings.SkeletonType.Lines)
            {
                Handles.color = editorSettings.settingSkeletonColor;
                var vec = posB - posA;
                var cam = UnityEditor.SceneView.currentDrawingSceneView.camera.transform.forward;
                var cross = Vector3.Cross(vec, cam);
                cross.Normalize();
                vec.Normalize();
                float radius = HandleUtility.GetHandleSize(posA) * (editorSettings.settingBoneButtonSize / 200f);
                if (skeletonLines == null || skeletonLines.Length != 5)
                    skeletonLines = new Vector3[5];
                skeletonLines[0] = posA;
                skeletonLines[1] = posA + cross * radius + vec * radius;
                skeletonLines[2] = posB;
                skeletonLines[3] = posA - cross * radius + vec * radius;
                skeletonLines[4] = skeletonLines[0];
                Handles.DrawPolyLine(skeletonLines);
            }
            else if (editorSettings.settingsSkeletonType == EditorSettings.SkeletonType.Mesh)
            {
                if (arrowMesh == null)
                    arrowMesh = new EditorCommon.ArrowMesh();
                arrowMesh.material.color = editorSettings.settingSkeletonColor;
                arrowMesh.material.SetPass(0);
                var vec = posB - posA;
                var length = vec.magnitude;
                Quaternion qat = posB != posA ? Quaternion.LookRotation(vec) : Quaternion.identity;
                Matrix4x4 mat = Matrix4x4.TRS(posA, qat, new Vector3(length, length, length));
                Graphics.DrawMeshNow(arrowMesh.mesh, mat);
            }
        }

        private void OnInspectorUpdate()
        {
#if Enable_Profiler
            Profiler.BeginSample("****VeryAnimationWindow.OnInspectorUpdate");
#endif
            if (va.isEditError)
            {
                SetGameObject();

                #region Repaint
                if (beforeErrorCode != va.getErrorCode)
                {
                    Repaint();
                    beforeErrorCode = va.getErrorCode;
                }
                #endregion

                #region PlayingAnimation
                if (UpdatePlayingAnimation())
                {
                    Repaint();
                }
                #endregion

                return;
            }
            else
            {
                va.OnInspectorUpdate();

                #region Repaint
                switch (repaintGUI)
                {
                case RepaintGUI.Edit:
                    Repaint();
                    VeryAnimationEditorWindow.ForceRepaint();
                    break;
                case RepaintGUI.All:
                    Repaint();
                    VeryAnimationEditorWindow.ForceRepaint();
                    VeryAnimationControlWindow.ForceRepaint();
                    SceneView.RepaintAll();
                    break;
                }
                repaintGUI = RepaintGUI.None;
                #endregion
            }

#if Enable_Profiler
            Profiler.EndSample();
#endif
        }
        private void Update()
        {
            if (initialized)
            {
                if (va.isEditError)
                {
                    Release();
#if UNITY_2017_1_OR_NEWER
                    if (va.uAw_2017_1.GetLinkedWithTimeline() && va.uAw_2017_1.GetActiveRootGameObject() != null)
                    {
                        SetGameObject();
                        if (!va.isError)
                            Initialize();
                    }
#endif
                }
            }
            else
            {
                return;
            }

#if Enable_Profiler
            Profiler.BeginSample("****VeryAnimationWindow.Update");
#endif

            va.Update();

#if Enable_Profiler
            Profiler.EndSample();
#endif
        }

        public void Initialize()
        {
            Release();

            #region MemoryLeakCheck
#if Enable_MemoryLeakCheck
            memoryLeakDontSaveList = new List<UnityEngine.Object>();
            foreach (var obj in Resources.FindObjectsOfTypeAll<UnityEngine.Object>())
            {
                if ((obj.hideFlags & HideFlags.DontSave) == 0) continue;
                memoryLeakDontSaveList.Add(obj);
            }
#endif
            #endregion

            if (EditorApplication.isPlaying)
            {
                if (!EditorApplication.isPaused)
                    EditorApplication.isPaused = true;
                AnimatorStateSave.SaveAllState();
            }
            else
            {
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }

            UpdatePlayingAnimation();

            Selection.activeObject = null;

            undoGroupID = Undo.GetCurrentGroup();
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Very Animation Edit");
            Undo.RecordObject(this, "Very Animation Edit");

            va.Initialize();

            #region VeryAnimationEditorWindow
            if (VeryAnimationEditorWindow.instance == null)
            {
                var hew = EditorWindow.CreateInstance<VeryAnimationEditorWindow>();
                hew.ShowUtility();
            }
            #endregion
            #region VeryAnimationControlWindow
            if (VeryAnimationControlWindow.instance == null)
            {
                EditorWindow window = null;
                foreach (var w in Resources.FindObjectsOfTypeAll<EditorWindow>())
                {
                    if (w.GetType().Name == "SceneHierarchyWindow")
                    {
                        window = w;
                        break;
                    }
                }
                if (window != null)
                    GetWindow<VeryAnimationControlWindow>(window.GetType());
                else
                    GetWindow<VeryAnimationControlWindow>();
            }
            if (VeryAnimationControlWindow.instance != null)
                VeryAnimationControlWindow.instance.Initialize();
            #endregion

            #region SaveSettings
            {
                #region EditorPref
                {
                    guiAnimationFoldout = EditorPrefs.GetBool("VeryAnimation_Main_Animation", true);
                    guiToolsFoldout = EditorPrefs.GetBool("VeryAnimation_Main_Tools", false);
                    guiSettingsFoldout = EditorPrefs.GetBool("VeryAnimation_Main_Settings", false);
                    guiHelpFoldout = EditorPrefs.GetBool("VeryAnimation_Main_Help", false);
                    guiPreviewFoldout = EditorPrefs.GetBool("VeryAnimation_Main_Preview", true);
                }
                #endregion
                var saveSettings = gameObject.GetComponent<VeryAnimationSaveSettings>();
                if (saveSettings != null)
                {
                    #region bones
                    if (saveSettings.bonePaths != null && saveSettings.bonePaths.Length > 0)
                    {
                        if (saveSettings.showBones != null && saveSettings.showBones.Length > 0)
                        {
                            for (int i = 0; i < va.boneShowFlags.Length; i++)
                                va.boneShowFlags[i] = false;
                            foreach (var index in saveSettings.showBones)
                            {
                                var boneIndex = va.GetBoneIndexFromPath(saveSettings.bonePaths[index]);
                                if (boneIndex < 0) continue;
                                va.boneShowFlags[boneIndex] = true;
                            }
                        }
                        if (saveSettings.foldoutBones != null && saveSettings.foldoutBones.Length > 0)
                        {
                            if (VeryAnimationControlWindow.instance != null)
                            {
                                VeryAnimationControlWindow.instance.CollapseAll();
                                foreach (var index in saveSettings.foldoutBones)
                                {
                                    var boneIndex = va.GetBoneIndexFromPath(saveSettings.bonePaths[index]);
                                    if (boneIndex < 0) continue;
                                    VeryAnimationControlWindow.instance.SetExpand(va.bones[boneIndex], true);
                                }
                            }
                        }
                    }
                    #endregion
                }
                va.LoadSaveSettings(saveSettings);
            }
            va.OnBoneShowFlagsUpdated.Invoke();
            #endregion

            SceneView.onSceneGUIDelegate += OnSceneGUI;
            {
                var del = uSceneView.GetOnPreSceneGUIDelegate();
                del += OnPreSceneGUI;
                uSceneView.SetOnPreSceneGUIDelegate(del);
            }
#if UNITY_2017_2_OR_NEWER
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.pauseStateChanged += OnPauseStateChanged;
#else
            EditorApplication.playmodeStateChanged += OnPlaymodeStateChanged;
#endif
            initialized = true;

            OnSelectionChange();

            if (!uEditorWindow.HasFocus(this))
            {
                beforeSelectedTab = uEditorWindow.GetSelectedTab(this);
                Focus();
            }
            else
            {
                beforeSelectedTab = -1;
            }
            VeryAnimationEditorWindow.instance.Focus();

            InternalEditorUtility.RepaintAllViews();
            EditorApplication.delayCall += () =>
            {
                InternalEditorUtility.RepaintAllViews();
            };
        }
        public void Release()
        {
            if (instance == null || va == null || !initialized) return;
            initialized = false;

            Undo.SetCurrentGroupName("Very Animation Edit");
            Undo.RecordObject(this, "Very Animation Edit");

            #region SaveSettings
            if (gameObject != null)
            {
                var saveSettings = gameObject.GetComponent<VeryAnimationSaveSettings>();
                if (editorSettings.settingComponentSaveSettings)
                {
                    if (saveSettings == null)
                    {
                        //saveSettings = Undo.AddComponent<VeryAnimationSaveSettings>(gameObject);  Unexplained cause, Unity is crash depending on data, so change to the following.
                        saveSettings = gameObject.AddComponent<VeryAnimationSaveSettings>();
                        InternalEditorUtility.SetIsInspectorExpanded(saveSettings, false);
                        Undo.RegisterCreatedObjectUndo(saveSettings, "Very Animation Edit");
                    }
                    Undo.RecordObject(saveSettings, "Very Animation Edit");
                    #region bones
                    {
                        saveSettings.bonePaths = new string[va.bonePaths.Length];
                        va.bonePaths.CopyTo(saveSettings.bonePaths, 0);
                        {
                            var list = new List<int>();
                            if (va.boneShowFlags != null && va.bones != null && va.boneShowFlags.Length == va.bones.Length)
                            {
                                for (int i = 0; i < va.boneShowFlags.Length; i++)
                                {
                                    if (va.bones[i] == null || !va.boneShowFlags[i]) continue;
                                    list.Add(i);
                                }
                            }
                            saveSettings.showBones = list.ToArray();
                        }
                        {
                            var list = new List<int>();
                            if (VeryAnimationControlWindow.instance != null)
                            {
                                VeryAnimationControlWindow.instance.ActionAllExpand((go) =>
                                {
                                    var boneIndex = va.BonesIndexOf(go);
                                    if (boneIndex >= 0)
                                        list.Add(boneIndex);
                                });
                            }
                            saveSettings.foldoutBones = list.ToArray();
                        }
                    }
                    #endregion
                    va.SaveSaveSettings(saveSettings);
                }
                else
                {
                    if (saveSettings != null)
                        Undo.DestroyObjectImmediate(saveSettings);
                }
            }
            #endregion

            if (uSceneView != null)
            {
                var del = uSceneView.GetOnPreSceneGUIDelegate();
                del -= OnPreSceneGUI;
                uSceneView.SetOnPreSceneGUIDelegate(del);
            }
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
#if UNITY_2017_2_OR_NEWER
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.pauseStateChanged -= OnPauseStateChanged;
#else
            EditorApplication.playmodeStateChanged -= OnPlaymodeStateChanged;
#endif
            Language.OnLanguageChanged = null;
            #region Editor
            handleTransformUpdate = true;
            arrowMesh = null;
            #endregion

            va.Release();

            if (undoGroupID >= 0)
            {
                Undo.CollapseUndoOperations(undoGroupID);
                undoGroupID = -1;
            }

            AnimatorStateSave.LoadAllState();
            if (EditorApplication.isPlaying && EditorApplication.isPaused)  //Is there a bug that will not be updated while pausing? Therefore, it forcibly updates it.
            {
                if (gameObject != null)
                {
                    foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>())
                    {
                        if (renderer == null) continue;
                        renderer.enabled = !renderer.enabled;
                        renderer.enabled = !renderer.enabled;
                    }
                }
            }

            if (beforeSelectedTab >= 0 && beforeSelectedTab < uEditorWindow.GetNumTabs(this))
            {
                uEditorWindow.SetSelectedTab(this, beforeSelectedTab);
            }

            EditorApplication.delayCall += () =>
            {
                if (va == null || va.isEditError)
                    CloseOtherWindows();
            };

            GC.Collect();

            #region MemoryLeakCheck
#if Enable_MemoryLeakCheck
            if (memoryLeakDontSaveList != null)
            {
                EditorApplication.delayCall += () =>
                {
                    EditorApplication.delayCall += () =>
                    {
                        foreach (var obj in Resources.FindObjectsOfTypeAll<UnityEngine.Object>())
                        {
                            if ((obj.hideFlags & HideFlags.DontSave) == 0) continue;
                            if (!memoryLeakDontSaveList.Contains(obj))
                                Debug.LogWarningFormat("Memory Leak = Type({0}), Name({1})", obj.GetType(), obj.name);

                        }
                        memoryLeakDontSaveList = null;
                    };
                };
            }
#endif
            #endregion

            InternalEditorUtility.RepaintAllViews();
        }

        public bool IsShowSceneGizmo()
        {
            if (va.uAw.GetPlaying()) return false;
            if (focusedWindow == va.uAw.instance) return false;
#if UNITY_2017_1_OR_NEWER
            if (focusedWindow == va.uAw_2017_1.uTimelineWindow.instance) return false;
#endif
            return true;
        }

        public bool UpdatePlayingAnimation()
        {
            AnimationClip clip;
            float time;
            if (va.GetPlayingAnimationInfo(out clip, out time))
            {
                if (playingAnimationClip != clip || playingAnimationTime != time)
                {
                    playingAnimationClip = clip;
                    playingAnimationTime = time;
                    return true;
                }
            }
            else if (playingAnimationClip != null || playingAnimationTime != 0f)
            {
                playingAnimationClip = null;
                playingAnimationTime = 0f;
                return true;
            }
            return false;
        }

        private void SetGameObject()
        {
            bool updated = false;
            var go = va.uAw != null ? va.uAw.GetActiveRootGameObject() : null;
            if (go != gameObject)
            {
                gameObject = go;
                updated = true;
            }
            var ap = va.uAw != null ? va.uAw.GetActiveAnimationPlayer() : null;
            if (ap is Animator)
            {
                var apa = ap as Animator;
                if (animator != apa)
                {
                    animator = apa;
                    updated = true;
                }
                animation = null;
            }
            else if (ap is Animation)
            {
                var apa = ap as Animation;
                if (animation != apa)
                {
                    animation = apa;
                    updated = true;
                }
                animator = null;
            }
            else
            {
                if (animation != null)
                {
                    animation = null;
                    updated = true;
                }
                if (animator != null)
                {
                    animator = null;
                    updated = true;
                }
            }
            if (updated)
                Repaint();
        }
    }
}
