//#define Enable_Profiler

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditorInternal;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;

namespace VeryAnimation
{
    [Serializable]
    public partial class VeryAnimation
    {
        public static VeryAnimation instance;

        private VeryAnimationWindow vaw { get { return VeryAnimationWindow.instance; } }

        #region Reflection
        public UAnimationWindow uAw { get; private set; }
        public UAvatar uAvatar { get; private set; }
        public UAnimator uAnimator { get; private set; }
        public UAnimatorControllerTool uAnimatorControllerTool { get; private set; }
        public UParameterControllerEditor uParameterControllerEditor { get; private set; }
        public UAnimationUtility uAnimationUtility { get; private set; }
        public UAnimationWindowUtility uAnimationWindowUtility { get; private set; }
        public UCurveUtility uCurveUtility { get; private set; }
        public URotationCurveInterpolation uRotationCurveInterpolation { get; private set; }
        public USceneView uSceneView { get; private set; }

        public UAvatarPreview uAvatarPreview { get; private set; }
        public UAnimationClipEditor uAnimationClipEditor { get; private set; }

#if UNITY_2017_1_OR_NEWER
        public UAnimationWindow_2017_1 uAw_2017_1 { get; private set; }
#endif
#if UNITY_2018_1_OR_NEWER
        public UAnimationUtility_2018_1 uAnimationUtility_2018_1 { get; private set; }
#endif
        #endregion

        #region Core
        public TransformPoseSave transformPoseSave { get; private set; }
        public BlendShapeWeightSave blendShapeWeightSave { get; private set; }
        public MusclePropertyName musclePropertyName { get; private set; }
        public AnimatorIKCore animatorIK;
        public OriginalIKCore originalIK;
        #endregion

        public bool edit;
#if UNITY_2017_1_OR_NEWER
        private bool linkedWithTimeline;
#endif
        #region Selection
        public List<GameObject> selectionGameObjects { get; private set; }
        public List<int> selectionBones { get; private set; }
        public GameObject selectionActiveGameObject { get { return selectionGameObjects != null && selectionGameObjects.Count > 0 ? selectionGameObjects[0] : null; } }
        public int selectionActiveBone { get { return selectionBones != null && selectionBones.Count > 0 ? selectionBones[0] : -1; } }
        public List<HumanBodyBones> selectionHumanVirtualBones { get; private set; }
        #endregion

        #region Cache
        public Renderer[] renderers { get; private set; }
        public bool isHuman { get; private set; }
        public bool animatorApplyRootMotion { get; private set; }
        public Avatar animatorAvatar { get; private set; }
        public Transform animatorAvatarRoot { get; private set; }
        public GameObject[] bones { get; private set; }
        public Dictionary<GameObject, int> boneDic { get; private set; }
        public GameObject[] humanoidBones { get; private set; }
        public int[] parentBoneIndexes { get; private set; }
        public int[] mirrorBoneIndexes { get; private set; }
        public int[] mirrorBoneRootIndexes { get; private set; }
        public Dictionary<SkinnedMeshRenderer, Dictionary<string, string>> mirrorBlendShape { get; private set; }
        public HumanBodyBones[] boneIndex2humanoidIndex { get; private set; }
        public int[] humanoidIndex2boneIndex { get; private set; }
        public bool[] humanoidConflict { get; private set; }
        public string[] bonePaths { get; private set; }
        public Dictionary<string, int> bonePathDic { get; private set; }
        public TransformPoseSave.SaveData[] boneSaveTransforms { get; private set; }
        public HumanPose saveHumanPose { get; private set; }
        public DummyObject calcObject { get; private set; }
        public UAvatar.MuscleLimit[] humanoidMuscleLimit { get; private set; }
        public bool humanoidHasLeftHand { get; private set; }
        public bool humanoidHasRightHand { get; private set; }
        public bool humanoidHasTDoF { get; private set; }
        public Quaternion humanoidPreHipRotationInverse { get; private set; }
        public Quaternion humanoidPoseHipRotation { get; private set; }
        public bool[] humanoidMuscleContains { get; private set; }
        public HumanPoseHandler humanPoseHandler { get; private set; }
        public int rootMotionBoneIndex { get; private set; }
        public Vector3 humanWorldRootPositionCache { get; set; }
        public Quaternion humanWorldRootRotationCache { get; set; }
        public Bounds gameObjectBounds { get; set; }
        #endregion

        #region DummyObject
        public DummyObject dummyObject { get; private set; }
        public GameObject editGameObject { get { return dummyObject != null ? dummyObject.gameObject : vaw.gameObject; } }
        public Animator editAnimator { get { return dummyObject != null ? dummyObject.animator : vaw.animator; } }
        public GameObject[] editBones { get { return dummyObject != null ? dummyObject.bones : bones; } }
        public Dictionary<GameObject, int> editBoneDic { get { return dummyObject != null ? dummyObject.boneDic : boneDic; } }
        public GameObject[] editHumanoidBones { get { return dummyObject != null ? dummyObject.humanoidBones : humanoidBones; } }
        public HumanPoseHandler editHumanPoseHandler { get { return dummyObject != null ? dummyObject.humanPoseHandler : humanPoseHandler; } }
        public int EditBonesIndexOf(GameObject go)
        {
            var tmpBoneDic = editBoneDic;
            if (tmpBoneDic != null && go != null)
            {
                int boneIndex;
                if (tmpBoneDic.TryGetValue(go, out boneIndex))
                {
                    return boneIndex;
                }
            }
            return -1;
        }
        #endregion

        #region Current
        public AnimationClip currentClip { get; private set; }
        public float currentTime { get; private set; }
        #endregion
        #region Before
        private AnimationClip beforeClip;
        private float beforeTime;
        private float beforeLength;
        private Tool beforeCurrentTool;
        private EditorWindow beforeMouseOverWindow;
        private EditorWindow beforeFocusedWindow;
        #endregion

        #region Refresh
        private enum AnimationWindowStateRefreshType
        {
            None,
            CurvesOnly,
            Everything,
        }
        private AnimationWindowStateRefreshType animationWindowRefresh;
        private void SetAnimationWindowRefresh(AnimationWindowStateRefreshType type)
        {
            if (type > animationWindowRefresh)
                animationWindowRefresh = type;
        }
        private bool updateResampleAnimation;
        private AnimationClip needSyncEditorCurveClip;
        private bool animationWindowSynchroSelection;
        private Dictionary<int, EditorCurveBinding> animationWindowSynchroSelectionBindings;
        private bool updateGenericRootMotion;
        private bool updatePoseFixAnimation;
        #endregion

        #region EditorWindow
        public bool clampMuscle;
        public bool autoFootIK;
        public bool mirrorEnable;
        public enum RootCorrectionMode
        {
            Disable,
            Single,
            Full,
            Total
        }
        public RootCorrectionMode rootCorrectionMode;

        private PoseTemplate copyPaste;
        #endregion

        #region ControlWindow
        public List<VeryAnimationSaveSettings.SelectionData> selectionSetList;
        #endregion

        #region AnimationWindow
        private EditorWindow autoLockedAnimationWindow;
        #endregion

        public void OnEnable()
        {
            instance = this;

            edit = false;
#if UNITY_2018_1_OR_NEWER
            uAw = uAw_2017_1 = new UAnimationWindow_2018_1();
            uAnimationUtility = uAnimationUtility_2018_1 = new UAnimationUtility_2018_1();
#elif UNITY_2017_1_OR_NEWER
            uAw = uAw_2017_1 = new UAnimationWindow_2017_1();
            uAnimationUtility = new UAnimationUtility();
#else
            uAw = new UAnimationWindow();
            uAnimationUtility = new UAnimationUtility();
#endif
            uAnimationWindowUtility = new UAnimationWindowUtility();
            uAvatar = new UAvatar();
            uAnimator = new UAnimator();
            uAnimatorControllerTool = new UAnimatorControllerTool();
            uParameterControllerEditor = new UParameterControllerEditor();
            uCurveUtility = new UCurveUtility();
            uRotationCurveInterpolation = new URotationCurveInterpolation();
            uSceneView = new USceneView();

            musclePropertyName = new MusclePropertyName();
            animatorIK = new AnimatorIKCore();
            originalIK = new OriginalIKCore();

            CreateEditorCurveBindingPropertyNames();

            OnBoneShowFlagsUpdated += UpdateSkeletonShowBoneList;

            InternalEditorUtility.RepaintAllViews();
        }
        public void OnDisable()
        {
            OnBoneShowFlagsUpdated -= UpdateSkeletonShowBoneList;
        }
        public void OnDestroy()
        {
            instance = null;
        }
        public void OnFocus()
        {
            instance = this;    //Measures against the problem that OnEnable may not come when repeating Shift + Space.
        }

        public void Initialize()
        {
            Assert.IsFalse(edit);

            currentClip = uAw.GetSelectionAnimationClip();
            currentTime = uAw.GetCurrentTime();
#if UNITY_2017_1_OR_NEWER
            if (uAw_2017_1.GetLinkedWithTimeline())
            {
                var director = uAw_2017_1.GetTimelineCurrentDirector();
                currentTime = (float)director.time;
            }
#endif

            uAw.RecordingDisable();
#if UNITY_2017_1_OR_NEWER
            uAw_2017_1.SetTimelineRecording(false);
            uAw_2017_1.SetTimelinePreviewMode(false);
            linkedWithTimeline = uAw_2017_1.GetLinkedWithTimeline();
#endif

            edit = true;

            #region AutoLock
            {
                autoLockedAnimationWindow = null;
#if UNITY_2017_1_OR_NEWER
                if (!uAw_2017_1.GetLinkedWithTimeline())
#endif
                {
                    if (!uAw.GetLock(uAw.instance))
                    {
                        uAw.SetLock(uAw.instance, true);
                        autoLockedAnimationWindow = uAw.instance;
                    }
                }
            }
            #endregion

            beforeCurrentTool = lastTool = Tools.current;
            beforeMouseOverWindow = null;
            beforeFocusedWindow = null;

            transformPoseSave = new TransformPoseSave(vaw.gameObject);

            #region Animator
            if (vaw.animator != null)
            {
                if (!vaw.animator.isInitialized)
                    vaw.animator.Rebind();
                var ac = GetAnimatorController();
                #region AvatarpreviewShowIK
#if UNITY_2017_1_OR_NEWER
                if (uAw_2017_1.GetLinkedWithTimeline())
                    EditorPrefs.SetBool("AvatarpreviewShowIK", true);
                else
#endif
                if (ac != null && ac.layers.Length > 0)
                {
                    bool enable = false;
                    if (EditorApplication.isPlaying)
                    {
                        var state = vaw.animator.GetCurrentAnimatorStateInfo(0);
                        var index = ArrayUtility.FindIndex(ac.layers[0].stateMachine.states, (x) => x.state.nameHash == state.shortNameHash);
                        if (index >= 0)
                            enable = ac.layers[0].stateMachine.states[index].state.iKOnFeet;
                    }
                    else
                    {
                        foreach (var layer in ac.layers)
                        {
                            Func<Motion, bool> FindMotion = null;
                            FindMotion = (motion) =>
                            {
                                if (motion != null)
                                {
                                    if (motion is UnityEditor.Animations.BlendTree)
                                    {
                                        var blendTree = motion as UnityEditor.Animations.BlendTree;
                                        foreach (var c in blendTree.children)
                                        {
                                            if (FindMotion(c.motion))
                                                return true;
                                        }
                                    }
                                    else
                                    {
                                        if (motion == currentClip)
                                        {
                                            return true;
                                        }
                                    }
                                }
                                return false;
                            };
                            foreach (var state in layer.stateMachine.states)
                            {
                                if (FindMotion(state.state.motion))
                                {
                                    enable = state.state.iKOnFeet;
                                    break;
                                }
                            }
                        }
                    }
                    EditorPrefs.SetBool("AvatarpreviewShowIK", enable);
                }
                else
                {
                    EditorPrefs.SetBool("AvatarpreviewShowIK", false);
                }
                #endregion
#if UNITY_2017_1_OR_NEWER
                EditorPrefs.SetBool(UAvatarPreview.EditorPrefsApplyRootMotion, uAw_2017_1.GetLinkedWithTimeline() || vaw.animator.applyRootMotion);
#else
                EditorPrefs.SetBool(UAvatarPreview.EditorPrefsApplyRootMotion, vaw.animator.applyRootMotion);
#endif
                uAnimatorControllerTool.SetAnimatorController(ac);
                uParameterControllerEditor.SetAnimatorController(ac);
            }
            #endregion
            #region AnimatorIK
            {
                if (animatorIK == null)
                    animatorIK = new AnimatorIKCore();
                animatorIK.Initialize();
            }
            #endregion

            UpdateBones();

            #region OriginalIK
            {
                if (originalIK == null)
                    originalIK = new OriginalIKCore();
                originalIK.Initialize();
            }
            #endregion

#if UNITY_2017_1_OR_NEWER
            if (uAw_2017_1.GetLinkedWithTimeline())
            {
                dummyObject = new DummyObject();
                dummyObject.Initialize(vaw.gameObject);
                dummyObject.SetColor(vaw.editorSettings.settingDummyObjectColor);
                transformPoseSave.SetSyncTransforms(dummyObject.gameObject);
            }
#endif
            blendShapeWeightSave = new BlendShapeWeightSave(editGameObject);
            UpdateBlendShapeMirror();

            #region RecordingChange
            if (!uAw.GetRecording())
            {
                if (uAw.GetCanRecord())
                {
                    uAw.RecordingChange();
                }
#if UNITY_2017_1_OR_NEWER
                else if (uAw_2017_1.GetCanPreview())
                {
                    uAw_2017_1.PreviewingChange();
                }
#endif
            }
            #endregion

            if (EditorApplication.isPlaying && vaw.playingAnimationClip != null)
            {
                #region SetCurrentClipAndTime
                uAw.SetSelectionAnimationClip(vaw.playingAnimationClip, "playingAnimationClip");
                uAw.SetCurrentTime(vaw.playingAnimationTime);
                currentClip = uAw.GetSelectionAnimationClip();
                currentTime = uAw.GetCurrentTime();
                if (vaw.animator != null)
                {
                    ResampleAnimation();
                    int boneIndex = -1;
                    if (isHuman)
                        boneIndex = humanoidIndex2boneIndex[(int)HumanBodyBones.Hips];
                    else if (rootMotionBoneIndex >= 0)
                        boneIndex = rootMotionBoneIndex;
                    if (boneIndex >= 0)
                    {
                        var t = vaw.gameObject.transform;
                        var originalTransform = transformPoseSave.GetOriginalTransform(bones[boneIndex].transform);
                        {
                            var rot = originalTransform.rotation * Quaternion.Inverse(bones[boneIndex].transform.rotation);
                            t.localRotation *= rot;
                            var pos = originalTransform.position - bones[boneIndex].transform.position;
                            t.localPosition += pos;
                        }
                        boneSaveTransforms[0].Save(t);
                    }
                }
                #endregion
            }
            else
            {
                #region ResetCurrentTime
#if UNITY_2017_1_OR_NEWER
                if (uAw_2017_1.GetLinkedWithTimeline())
                {
                    var director = uAw_2017_1.GetTimelineCurrentDirector();
                    director.time = currentTime;
                    director.Evaluate();
                }
                else
#endif
                {
                    uAw.SetCurrentTime(currentTime);
                }
                #endregion
            }

            ResampleAnimation();

            humanWorldRootPositionCache = GetHumanWorldRootPosition();
            humanWorldRootRotationCache = GetHumanWorldRootRotation();

            SelectGameObjectEvent();

            #region gameObjectBounds
            {
                var bounds = new Bounds();
                var renderer = vaw.gameObject.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < renderer.Length; i++)
                {
                    if (i == 0)
                        bounds = renderer[i].bounds;
                    else
                        bounds.Encapsulate(renderer[i].bounds);
                }
                gameObjectBounds = bounds;
            }
            #endregion

            InitializeAnimatorRootCorrection();
            InitializeHumanoidFootIK();

            Undo.undoRedoPerformed += UndoRedoPerformed;
            AnimationUtility.onCurveWasModified += OnCurveWasModified;
#if UNITY_2018_1_OR_NEWER
            EditorApplication.hierarchyChanged += OnHierarchyWindowChanged;
#else
            EditorApplication.hierarchyWindowChanged += OnHierarchyWindowChanged;
#endif
        }
        public void Release()
        {
            Undo.undoRedoPerformed -= UndoRedoPerformed;
            AnimationUtility.onCurveWasModified -= OnCurveWasModified;
#if UNITY_2018_1_OR_NEWER
            EditorApplication.hierarchyChanged -= OnHierarchyWindowChanged;
#else
            EditorApplication.hierarchyWindowChanged -= OnHierarchyWindowChanged;
#endif

#if UNITY_2017_1_OR_NEWER
            if (uAw_2017_1 != null && uAw_2017_1.GetLinkedWithTimeline())
            {
                uAw_2017_1.SetTimelineRecording(false);
            }
            else
#endif
            {
                if (uAw != null)
                    uAw.RecordingDisable();
            }

            edit = false;
#if UNITY_2017_1_OR_NEWER
            linkedWithTimeline = false;
#endif
            selectionGameObjects = null;
            selectionBones = null;
            selectionHumanVirtualBones = null;
            renderers = null;
            isHuman = false;
            animatorApplyRootMotion = false;
            animatorAvatar = null;
            animatorAvatarRoot = null;
            bones = null;
            boneDic = null;
            humanoidBones = null;
            parentBoneIndexes = null;
            mirrorBoneIndexes = null;
            mirrorBoneRootIndexes = null;
            mirrorBlendShape = null;
            boneIndex2humanoidIndex = null;
            humanoidIndex2boneIndex = null;
            humanoidConflict = null;
            bonePaths = null;
            bonePathDic = null;
            boneSaveTransforms = null;
            if (calcObject != null)
            {
                calcObject.Release();
                calcObject = null;
            }
            humanoidMuscleLimit = null;
            humanoidMuscleContains = null;
            humanPoseHandler = null;

            beforeClip = null;
            beforeTime = 0f;
            beforeLength = 0f;

            animationWindowRefresh = AnimationWindowStateRefreshType.None;
            updateResampleAnimation = false;
            UpdateSyncEditorCurveClip();
            animationWindowSynchroSelection = false;
            animationWindowSynchroSelectionBindings = null;
            updateGenericRootMotion = false;
            updatePoseFixAnimation = false;

            boneShowFlags = null;

            editorCurveCacheClip = null;
            editorCurveCacheDic = null;

            if (uAnimationClipEditor != null)
            {
                uAnimationClipEditor.Release();
                uAnimationClipEditor = null;
            }
            if (uAvatarPreview != null)
            {
                uAvatarPreview.Release();
                uAvatarPreview = null;
            }

            if (animatorIK != null)
                animatorIK.Release();   //Not to be null
            if (originalIK != null)
                originalIK.Release();   //Not to be null
            if (dummyObject != null)
            {
                dummyObject.Release();
                dummyObject = null;
            }

            selectionSetList = null;

            curvesWasModified.Clear();

            if (transformPoseSave != null)
            {
#if UNITY_2017_1_OR_NEWER
                if (uAw_2017_1 == null || !uAw_2017_1.GetLinkedWithTimeline())
#endif
                {
                    transformPoseSave.ResetOriginalTransform();
                    transformPoseSave.ResetRootStartTransform();
                }
                transformPoseSave = null;
            }
            if (blendShapeWeightSave != null)
            {
#if UNITY_2017_1_OR_NEWER
                if (uAw_2017_1 == null || !uAw_2017_1.GetLinkedWithTimeline())
#endif
                {
                    blendShapeWeightSave.ResetOriginalWeight();
                }
                blendShapeWeightSave = null;
            }

            ReleaseAnimatorRootCorrection();
            ReleaseHumanoidFootIK();

            DisableCustomTools();

            #region AutoLock
            if (autoLockedAnimationWindow != null)
            {
                uAw.SetLock(autoLockedAnimationWindow, false);
                autoLockedAnimationWindow = null;
            }
            #endregion
        }

        public bool isEditError
        {
            get
            {
                return !edit || isError;
            }
        }
        public bool isError
        {
            get
            {
                return getErrorCode < 0;
            }
        }
        public int getErrorCode
        {
            get
            {
                if (uAw == null || uAw.instance == null || !uAw.HasFocus() || uAw.GetSelectionAnimationClip() == null)
                    return -1;
                if (vaw == null || vaw.gameObject == null || (vaw.animator == null && vaw.animation == null))
                    return -2;
                if (vaw.animator != null && !vaw.animator.hasTransformHierarchy)
                    return -3;
                if (vaw.animation != null && vaw.animation.GetClipCount() == 0)
                    return -4;
                if (vaw.animation != null && Application.isPlaying)
                    return -5;
                if (edit && vaw.gameObject != uAw.GetActiveRootGameObject())
                    return -6;
                if (edit && vaw.animator != null && animatorApplyRootMotion != vaw.animator.applyRootMotion)
                    return -7;
                if (edit && vaw.animator != null && animatorAvatar != vaw.animator.avatar)
                    return -8;
                if (edit && currentClip == null)
                    return -9;
#if UNITY_2017_1_OR_NEWER
                if (!uAw_2017_1.GetLinkedWithTimeline())
#endif
                {
                    if (!vaw.gameObject.activeInHierarchy)
                        return -10;
                    if (vaw.animator != null && vaw.animator.runtimeAnimatorController == null)
                        return -11;
                    if (!edit && vaw.animator != null && vaw.animator.runtimeAnimatorController != null && (vaw.animator.runtimeAnimatorController.hideFlags & (HideFlags.DontSave | HideFlags.NotEditable)) != 0)
                        return -12;
                }
#if UNITY_2017_1_OR_NEWER
                else
                {
                    if (!edit && !vaw.gameObject.activeInHierarchy)
                        return -20;
                    if (!uAw_2017_1.GetLinkedWithTimelineEditable())
                        return -21;
                    if (Application.isPlaying)
                        return -22;
                    var currentDirector = uAw_2017_1.GetTimelineCurrentDirector();
                    if (currentDirector != null)
                    {
                        if (!currentDirector.gameObject.activeInHierarchy)
                            return -23;
                        if (!currentDirector.enabled)
                            return -24;
                    }
                }
                if (edit && linkedWithTimeline != uAw_2017_1.GetLinkedWithTimeline())
                    return -30;
#endif
                if (edit && VeryAnimationEditorWindow.instance == null)
                    return -100;
                if (edit && VeryAnimationControlWindow.instance == null)
                    return -101;
                return 0;
            }
        }

        #region Update
        public void OnInspectorUpdate()
        {
            if (isEditError) return;

            #region AnimationWindowRefresh
            if (animationWindowRefresh != AnimationWindowStateRefreshType.None)
            {
                if (animationWindowRefresh == AnimationWindowStateRefreshType.CurvesOnly)
                    uAw.Repaint();
                else if (animationWindowRefresh == AnimationWindowStateRefreshType.Everything)
                    uAw.ForceRefresh();
                animationWindowRefresh = AnimationWindowStateRefreshType.None;
            }
            #endregion
        }
        public void Update()
        {
            currentClip = uAw.GetSelectionAnimationClip();
            currentTime = uAw.GetCurrentTime();

            if (isEditError) return;

#if Enable_Profiler
            Profiler.BeginSample("****VeryAnimation.Update");
#endif

            bool awForceRefresh = false;

            UpdateSyncEditorCurveClip();

            #region AnimationWindowSynchro
            if (animationWindowSynchroSelection && uAw.IsDoneRefresh())
            {
                if (EditorWindow.focusedWindow != uAw.instance)
                {
                    List<EditorCurveBinding> bindings = null;
                    if (animationWindowSynchroSelectionBindings != null)
                    {
                        bindings = new List<EditorCurveBinding>(animationWindowSynchroSelectionBindings.Values);
                    }
                    else
                    {
                        SelectGameObjectEvent();    //UpdateSelection
                        bindings = GetSelectionEditorCurveBindings();
                    }
                    uAw.SynchroCurveSelection(bindings);
                }
                animationWindowSynchroSelection = false;
                animationWindowSynchroSelectionBindings = null;
            }
            #endregion

            #region RecordingChange
#if Enable_Profiler
            Profiler.BeginSample("RecordingChange");
#endif
            if (!uAw.GetRecording())
            {
                if (uAw.GetCanRecord())
                {
                    uAw.RecordingChange();
                }
#if UNITY_2017_1_OR_NEWER
                else if (uAw_2017_1.GetCanPreview())
                {
                    if (!uAw_2017_1.GetPreviewing())
                    {
                        uAw_2017_1.PreviewingChange();
                    }
                }
#endif
            }

#if UNITY_2017_1_OR_NEWER
            if (uAw_2017_1.GetLinkedWithTimeline() && !uAw_2017_1.IsTimelineArmedForRecord())
            {
                uAw_2017_1.SetTimelineRecording(false);
                if (!uAw.GetRecording())
                    uAw.RecordingChange();
            }
#endif
#if Enable_Profiler
            Profiler.EndSample();
#endif
            #endregion

            #region ClipChange
#if Enable_Profiler
            Profiler.BeginSample("ClipChange");
#endif
            {
                if (currentClip != null && (currentClip != beforeClip || uAvatarPreview == null || uAnimationClipEditor == null))
                {
                    if (transformPoseSave != null)
                        transformPoseSave.ResetOriginalTransform();
                    if (blendShapeWeightSave != null)
                        blendShapeWeightSave.ResetOriginalWeight();
                    {
                        if (uAnimationClipEditor != null)
                        {
                            uAnimationClipEditor.Release();
                            uAnimationClipEditor = null;
                        }
                        if (uAvatarPreview != null)
                        {
                            var previewDir = uAvatarPreview.PreviewDir;
                            var zoomFactor = uAvatarPreview.ZoomFactor;
                            var playing = uAvatarPreview.playing;
                            uAvatarPreview.Release();
                            uAvatarPreview = new UAvatarPreview(currentClip, vaw.gameObject);
                            uAvatarPreview.SetTime(currentTime);
                            uAvatarPreview.PreviewDir = previewDir;
                            uAvatarPreview.ZoomFactor = zoomFactor;
                            uAvatarPreview.playing = playing;
                        }
                        else
                        {
                            uAvatarPreview = new UAvatarPreview(currentClip, vaw.gameObject);
                            uAvatarPreview.SetTime(currentTime);
                        }
                        uAnimationClipEditor = new UAnimationClipEditor(currentClip, uAvatarPreview);
                    }
                    ClearEditorCurveCache();
                    SetUpdateResampleAnimation();
                    SetSynchroIKtargetAll(true);
                    beforeClip = currentClip;
                    beforeTime = currentTime;
                    beforeLength = currentClip.length;
                    ToolsReset();
                }
            }
#if Enable_Profiler
            Profiler.EndSample();
#endif
            #endregion

            #region TimeChange
#if Enable_Profiler
            Profiler.BeginSample("TimeChange");
#endif
            {
                if (currentTime != beforeTime)
                {
                    if (!uAw.GetPlaying())
                    {
                        SetUpdateResampleAnimation();
                        SetSynchroIKtargetAll(true);
                        if (!uAvatarPreview.playing)
                        {
                            uAvatarPreview.SetTime(currentTime);
                        }
                    }
                    beforeTime = currentTime;
                }
            }
#if Enable_Profiler
            Profiler.EndSample();
#endif
            #endregion

            #region LengthChange
#if Enable_Profiler
            Profiler.BeginSample("LengthChange");
#endif
            {
                if (currentClip != null && beforeLength != currentClip.length)
                {
                    if (uAnimationClipEditor != null)
                    {
                        uAnimationClipEditor.Release();
                        uAnimationClipEditor = null;
                    }
                    beforeLength = currentClip.length;
                    if (uAvatarPreview != null)
                        uAnimationClipEditor = new UAnimationClipEditor(currentClip, uAvatarPreview);
                }
            }
#if Enable_Profiler
            Profiler.EndSample();
#endif
            #endregion

            #region ToolChange
#if Enable_Profiler
            Profiler.BeginSample("ToolChange");
#endif
            if (Tools.current == Tool.None)
            {
                if (beforeCurrentTool != lastTool)
                {
                    animationWindowSynchroSelection = true;
                    beforeCurrentTool = lastTool;
                }
            }
            else
            {
                if (beforeCurrentTool != Tools.current)
                {
                    animationWindowSynchroSelection = true;
                    beforeCurrentTool = Tools.current;
                }
            }
#if Enable_Profiler
            Profiler.EndSample();
#endif
            #endregion

            #region WindowChange
            {
                var mouseOverWindow = EditorWindow.mouseOverWindow;
                var focusedWindow = EditorWindow.focusedWindow;
                if (mouseOverWindow != beforeMouseOverWindow || focusedWindow != beforeFocusedWindow)
                {
                    #region AnimationWindow
                    if (mouseOverWindow == uAw.instance || focusedWindow == uAw.instance)
                    {
                        editorCurveCacheDirty = true;
                    }
                    #endregion
                    beforeMouseOverWindow = mouseOverWindow;
                    beforeFocusedWindow = focusedWindow;
                }
            }
            #endregion

            #region CurveChange Step1
#if Enable_Profiler
            Profiler.BeginSample("CurveChange Step1");
#endif
            bool rootUpdated = false;
            if (curvesWasModified.Count > 0)
            {
                SetOnCurveWasModifiedStop(true);
                foreach (var pair in curvesWasModified)
                {
                    #region CheckConflictCurve
                    if (pair.Value.deleted == AnimationUtility.CurveModifiedType.CurveModified)
                    {
                        if (pair.Value.binding.type == typeof(Transform))
                        {
                            var boneIndex = GetBoneIndexFromCurveBinding(pair.Value.binding);
                            if (boneIndex >= 0)
                            {
                                if (isHuman && humanoidConflict[boneIndex])
                                {
                                    EditorCommon.ShowNotification("Conflict");
                                    Debug.LogErrorFormat(Language.GetText(Language.Help.LogGenericCurveHumanoidConflictError), editBones[boneIndex].name);
                                    SetEditorCurveCache(pair.Value.binding, null);
                                    continue;
                                }
                                else if (IsTransformPositionCurveBinding(pair.Value.binding) || IsTransformRotationCurveBinding(pair.Value.binding))
                                {
                                    if (rootMotionBoneIndex >= 0 && boneIndex == 0)
                                    {
                                        EditorCommon.ShowNotification("Conflict");
                                        Debug.LogErrorFormat(Language.GetText(Language.Help.LogGenericCurveRootConflictError), editBones[boneIndex].name);
                                        SetEditorCurveCache(pair.Value.binding, null);
                                        continue;
                                    }
                                    else if (!isHuman && rootMotionBoneIndex >= 0 && boneIndex == rootMotionBoneIndex)
                                    {
                                        EditorCommon.ShowNotification("Conflict");
                                        Debug.LogErrorFormat(Language.GetText(Language.Help.LogGenericCurveGenericRootConflictError), editBones[boneIndex].name);
                                        updateGenericRootMotion = true;
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    #region EditorOptions - rootCorrectionMode
                    if (isHuman && rootCorrectionMode != RootCorrectionMode.Disable)
                    {
                        #region DisableAnimatorRootCorrection
                        if (IsAnimatorRootCurveBinding(pair.Value.binding))
                        {
                            if (pair.Value.deleted == AnimationUtility.CurveModifiedType.CurveModified)
                            {
                                DisableAnimatorRootCorrection();
                            }
                            else if (pair.Value.deleted == AnimationUtility.CurveModifiedType.CurveDeleted)
                            {
                                DisableAnimatorRootCorrection();
                            }
                        }
                        #endregion
                    }
                    #endregion
                    
                    #region GenericRootMotion
                    if (!isHuman && rootMotionBoneIndex >= 0 && !updateGenericRootMotion)
                    {
                        if (IsAnimatorRootCurveBinding(pair.Value.binding))
                        {
                            updateGenericRootMotion = true;
                        }
                        else if (GetBoneIndexFromCurveBinding(pair.Value.binding) >= 0)
                        {
                            if (pair.Value.binding.path == bonePaths[rootMotionBoneIndex] &&
                                (IsTransformPositionCurveBinding(pair.Value.binding) || IsTransformRotationCurveBinding(pair.Value.binding)))
                            {
                                updateGenericRootMotion = true;
                            }
                        }
                    }
                    #endregion

                    #region UpdateIK
                    {
                        if(!rootUpdated)
                        {
                            if (IsAnimatorRootCurveBinding(pair.Value.binding))
                            {
                                rootUpdated = true;
                            }
                        }
                    }
                    #endregion
                }
                SetOnCurveWasModifiedStop(false);
            }
#if Enable_Profiler
            Profiler.EndSample();
#endif
            #endregion

            #region GenericRootMotion
#if Enable_Profiler
            Profiler.BeginSample("GenericRootMotion");
#endif
            if (!isHuman && updateGenericRootMotion)
            {
                if (rootMotionBoneIndex >= 0)
                {
                    if ((currentClip.hideFlags & HideFlags.NotEditable) == HideFlags.None)
                    {
                        #region Position
                        {
                            var have = IsHaveAnimationCurveTransformPosition(rootMotionBoneIndex);
                            if (IsHaveAnimationCurveAnimatorRootT())
                            {
                                #region RootT -> Position
                                for (int i = 0; i < 3; i++)
                                {
                                    var curve = GetEditorCurveCache(AnimationCurveBindingAnimatorRootT[i]);
                                    SetEditorCurveCache(AnimationCurveBindingTransformPosition(rootMotionBoneIndex, i), curve);
                                }
                                if (!have)
                                    awForceRefresh = true;
                                #endregion
                            }
                            else if (have)
                            {
                                #region Position -> RootT
                                for (int i = 0; i < 3; i++)
                                {
                                    var curve = GetEditorCurveCache(AnimationCurveBindingTransformPosition(rootMotionBoneIndex, i));
                                    SetEditorCurveCache(AnimationCurveBindingAnimatorRootT[i], curve);
                                }
                                awForceRefresh = true;
                                #endregion
                            }
                            else
                            {
                                #region New
                                SetAnimationValueAnimatorRootT(GetAnimationValueAnimatorRootT(currentTime), currentTime);
                                awForceRefresh = true;
                                #endregion
                            }
                        }
                        #endregion
                        #region Rotation
                        {
                            var have = IsHaveAnimationCurveTransformRotation(rootMotionBoneIndex);
                            if (IsHaveAnimationCurveAnimatorRootQ())
                            {
                                #region RootQ -> Rotation
                                {
                                    var mode = IsHaveAnimationCurveTransformRotation(rootMotionBoneIndex);
                                    if (mode != URotationCurveInterpolation.Mode.RawQuaternions && mode != URotationCurveInterpolation.Mode.Undefined)
                                    {
                                        for (int i = 0; i < 3; i++)
                                        {
                                            SetEditorCurveCache(AnimationCurveBindingTransformRotation(rootMotionBoneIndex, i, mode), null);
                                        }
                                    }
                                }
                                for (int i = 0; i < 4; i++)
                                {
                                    var curve = GetEditorCurveCache(AnimationCurveBindingAnimatorRootQ[i]);
                                    SetEditorCurveCache(AnimationCurveBindingTransformRotation(rootMotionBoneIndex, i, URotationCurveInterpolation.Mode.RawQuaternions), curve);
                                }
                                if (have != URotationCurveInterpolation.Mode.RawQuaternions)
                                    awForceRefresh = true;
                                #endregion
                            }
                            else if (have == URotationCurveInterpolation.Mode.RawQuaternions)
                            {
                                #region Rotation -> RootQ
                                for (int i = 0; i < 4; i++)
                                {
                                    var curve = GetEditorCurveCache(AnimationCurveBindingTransformRotation(rootMotionBoneIndex, i, URotationCurveInterpolation.Mode.RawQuaternions));
                                    SetEditorCurveCache(AnimationCurveBindingAnimatorRootQ[i], curve);
                                }
                                awForceRefresh = true;
                                #endregion
                            }
                            else if (have == URotationCurveInterpolation.Mode.RawEuler)
                            {
                                Debug.LogWarning(Language.GetText(Language.Help.LogUpdateGenericRootMotionRawEulerError));
                            }
                            else
                            {
                                #region New
                                SetAnimationValueAnimatorRootQ(GetAnimationValueAnimatorRootQ(currentTime), currentTime);
                                awForceRefresh = true;
                                #endregion
                            }
                        }
                        #endregion
                        SetUpdateResampleAnimation();
                    }
                }
                updateGenericRootMotion = false;
            }
#if Enable_Profiler
            Profiler.EndSample();
#endif
            #endregion

            #region IK
#if Enable_Profiler
            Profiler.BeginSample("IK");
#endif
            if (GetUpdateIKtargetAll())
            {
                if (isHuman)
                {
                    #region Humanoid
                    if (animatorIK.GetUpdateIKtargetAll())
                    {
                        EnableAnimatorRootCorrection(currentTime, currentTime, currentTime);
                        UpdateAnimatorRootCorrection();
                        animatorIK.UpdateIK(currentTime, rootUpdated);
                    }
                    originalIK.UpdateIK(currentTime);
                    #endregion
                }
                else
                {
                    #region Generic and Legacy
                    originalIK.UpdateIK(currentTime);
                    #endregion
                }
                SetUpdateResampleAnimation();
            }
            else if(GetSynchroIKtargetAll())
            {
                SetUpdateResampleAnimation();
            }
#if Enable_Profiler
            Profiler.EndSample();
#endif
            #endregion

            #region CurveChange Step2
#if Enable_Profiler
            Profiler.BeginSample("CurveChange Step2");
#endif
            if (curvesWasModified.Count > 0 && ((isHuman && clampMuscle) || mirrorEnable) && !updatePoseFixAnimation)
            {
                SetOnCurveWasModifiedStop(true);
                foreach (var pair in curvesWasModified)
                {
                    if (pair.Value.deleted == AnimationUtility.CurveModifiedType.CurveModified)
                    {
                        #region EditorOptions - clampMuscle
                        if ((isHuman && clampMuscle))
                        {
                            if (GetMuscleIndexFromCurveBinding(pair.Value.binding) >= 0)
                            {
                                var curve = GetEditorCurveCache(pair.Value.binding);
                                if (curve != null)
                                {
                                    if (pair.Value.beforeCurve != null)
                                    {
                                        bool updated = false;
                                        for (int i = 0; i < curve.length; i++)
                                        {
                                            if (FindKeyframeIndex(pair.Value.beforeCurve, curve, i) < 0)
                                            {
                                                var key = curve[i];
                                                key.value = Mathf.Clamp(key.value, -1f, 1f);
                                                curve.MoveKey(i, key);
                                                updated = true;
                                            }

                                        }
                                        if (updated)
                                        {
                                            SetEditorCurveCache(pair.Value.binding, curve);
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogWarningFormat("<color=blue>[Very Animation]</color>Lost before cache '{0}'", pair.Value.binding.path, pair.Value.binding.propertyName);
                                    }
                                }
                            }
                        }
                        #endregion

                        #region EditorOptions - mirrorEnable
                        if (mirrorEnable)
                        {
                            var mbinding = GetMirrorAnimationCurveBinding(pair.Value.binding);
                            if (mbinding.HasValue)
                            {
                                var hash = GetEditorCurveBindingHashCode(mbinding.Value);
                                if (!curvesWasModified.ContainsKey(hash))
                                {
                                    var curve = GetEditorCurveCache(pair.Value.binding);
                                    if (curve != null)
                                    {
                                        if (pair.Value.beforeCurve != null)
                                        {
                                            bool updated = false;
                                            var mcurve = GetEditorCurveCache(mbinding.Value);
                                            if (mcurve == null)
                                            {
                                                mcurve = new AnimationCurve(curve.keys);
                                                updated = true;
                                            }
                                            for (int i = 0; i < pair.Value.beforeCurve.length; i++)
                                            {
                                                if (FindKeyframeAtTime(curve, pair.Value.beforeCurve[i].time) < 0)
                                                {
                                                    var index = FindKeyframeAtTime(mcurve, pair.Value.beforeCurve[i].time);
                                                    if (index >= 0)
                                                    {
                                                        mcurve.RemoveKey(index);
                                                        updated = true;
                                                    }
                                                }
                                            }
                                            AnimatorTDOFIndex tdofIndex;
                                            if (GetIkTIndexFromCurveBinding(pair.Value.binding) >= 0 ||
                                                GetIkQIndexFromCurveBinding(pair.Value.binding) >= 0)
                                            {
                                                #region IK
                                                for (int i = 0; i < curve.length; i++)
                                                {
                                                    if (FindKeyframeIndex(pair.Value.beforeCurve, curve, i) < 0)
                                                    {
                                                        AddHumanoidFootIK(curve[i].time);
                                                    }
                                                }
                                                #endregion
                                            }
                                            else if ((tdofIndex = GetTDOFIndexFromCurveBinding(pair.Value.binding)) >= 0)
                                            {
                                                #region TDOF
                                                var mtdofIndex = AnimatorTDOFMirrorIndexes[(int)tdofIndex];
                                                if (mtdofIndex != AnimatorTDOFIndex.None)
                                                {
                                                    var mirrorScale = HumanBonesAnimatorTDOFIndex[(int)AnimatorTDOFIndex2HumanBodyBones[(int)mtdofIndex]].mirror;
                                                    for (int i = 0; i < curve.length; i++)
                                                    {
                                                        if (FindKeyframeIndex(pair.Value.beforeCurve, curve, i) < 0)
                                                        {
                                                            var dof = GetDOFIndexFromCurveBinding(pair.Value.binding);
                                                            var key = curve[i];
                                                            key.value *= mirrorScale[dof];
                                                            key.inTangent *= mirrorScale[dof];
                                                            key.outTangent *= mirrorScale[dof];
                                                            SetKeyframe(mcurve, key);
                                                            updated = true;
                                                        }
                                                    }
                                                }
                                                #endregion
                                            }
                                            else if (IsTransformPositionCurveBinding(pair.Value.binding))
                                            {
                                                #region Position
                                                var boneIndex = GetBoneIndexFromCurveBinding(pair.Value.binding);
                                                for (int i = 0; i < curve.length; i++)
                                                {
                                                    if (FindKeyframeIndex(pair.Value.beforeCurve, curve, i) < 0)
                                                    {
                                                        var dof = GetDOFIndexFromCurveBinding(pair.Value.binding);
                                                        var key = curve[i];
                                                        {
                                                            Vector3 localPosition = Vector3.zero;
                                                            localPosition[dof] = key.value;
                                                            var mlocalPositionZero = GetMirrorBoneLocalPosition(boneIndex, Vector3.zero);
                                                            var mlocalPosition = GetMirrorBoneLocalPosition(boneIndex, localPosition);
                                                            if (Math.Sign(key.value * (mlocalPosition[dof] - mlocalPositionZero[dof])) < 0)
                                                            {
                                                                key.inTangent *= -1f;
                                                                key.outTangent *= -1f;
                                                            }
                                                            key.value = mlocalPosition[dof];
                                                        }
                                                        SetKeyframe(mcurve, key);
                                                        updated = true;
                                                    }
                                                }
                                                #endregion
                                            }
                                            else if (IsTransformRotationCurveBinding(pair.Value.binding))
                                            {
                                                #region Rotation
                                                var boneIndex = GetBoneIndexFromCurveBinding(pair.Value.binding);
                                                for (int i = 0; i < curve.length; i++)
                                                {
                                                    if (FindKeyframeIndex(pair.Value.beforeCurve, curve, i) < 0)
                                                    {
                                                        var dof = GetDOFIndexFromCurveBinding(pair.Value.binding);
                                                        var key = curve[i];
                                                        {
                                                            Quaternion localRotation = Quaternion.identity;
                                                            localRotation[dof] = key.value;
                                                            var mlocalRotationZero = GetMirrorBoneLocalRotation(boneIndex, Quaternion.identity);
                                                            var mlocalRotation = GetMirrorBoneLocalRotation(boneIndex, localRotation);
                                                            if (Math.Sign(key.value * (mlocalRotation[dof] - mlocalRotationZero[dof])) < 0)
                                                            {
                                                                key.inTangent *= -1f;
                                                                key.outTangent *= -1f;
                                                            }
                                                            key.value = mlocalRotation[dof];
                                                        }
                                                        SetKeyframe(mcurve, key);
                                                        updated = true;
                                                    }
                                                }
                                                #endregion
                                            }
                                            else if (IsTransformScaleCurveBinding(pair.Value.binding))
                                            {
                                                #region Scale
                                                var boneIndex = GetBoneIndexFromCurveBinding(pair.Value.binding);
                                                for (int i = 0; i < curve.length; i++)
                                                {
                                                    if (FindKeyframeIndex(pair.Value.beforeCurve, curve, i) < 0)
                                                    {
                                                        var dof = GetDOFIndexFromCurveBinding(pair.Value.binding);
                                                        var key = curve[i];
                                                        {
                                                            Vector3 localScale = Vector3.zero;
                                                            localScale[dof] = key.value;
                                                            var mlocalScaleZero = GetMirrorBoneLocalScale(boneIndex, Vector3.zero);
                                                            var mlocalScale = GetMirrorBoneLocalScale(boneIndex, localScale);
                                                            if (Math.Sign(key.value * (mlocalScale[dof] - mlocalScaleZero[dof])) < 0)
                                                            {
                                                                key.inTangent *= -1f;
                                                                key.outTangent *= -1f;
                                                            }
                                                            key.value = mlocalScale[dof];
                                                        }
                                                        SetKeyframe(mcurve, key);
                                                        updated = true;
                                                    }
                                                }
                                                #endregion
                                            }
                                            else
                                            {
                                                #region Other (As it is)
                                                for (int i = 0; i < curve.length; i++)
                                                {
                                                    if (FindKeyframeIndex(pair.Value.beforeCurve, curve, i) < 0)
                                                    {
                                                        var key = curve[i];
                                                        SetKeyframe(mcurve, key);
                                                        updated = true;
                                                    }
                                                }
                                                #endregion
                                            }
                                            if (updated)
                                            {
                                                SetEditorCurveCache(mbinding.Value, mcurve);
                                            }
                                        }
                                        else
                                        {
                                            Debug.LogWarningFormat("<color=blue>[Very Animation]</color>Lost before cache '{0}'", pair.Value.binding.path, pair.Value.binding.propertyName);
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    else if (pair.Value.deleted == AnimationUtility.CurveModifiedType.CurveDeleted)
                    {
                        #region EditorOptions - mirrorEnable
                        if (mirrorEnable)
                        {
                            var mbinding = GetMirrorAnimationCurveBinding(pair.Value.binding);
                            if (mbinding.HasValue)
                            {
                                var hash = GetEditorCurveBindingHashCode(mbinding.Value);
                                if (!curvesWasModified.ContainsKey(hash))
                                {
                                    SetEditorCurveCache(mbinding.Value, null);
                                }
                            }
                        }
                        #endregion
                    }
                }
                SetOnCurveWasModifiedStop(false);
            }
#if Enable_Profiler
            Profiler.EndSample();
#endif
            #endregion

            #region CurveChange Step3
#if Enable_Profiler
            Profiler.BeginSample("CurveChange Step3");
#endif
            if (curvesWasModified.Count > 0)
            {
                foreach (var pair in curvesWasModified)
                {
                    #region EditorOptions - rootCorrectionMode
                    if (isHuman && rootCorrectionMode != RootCorrectionMode.Disable)
                    {
                        #region EnableAnimatorRootCorrection
                        {
                            bool updatedMuscle = false;
                            {
                                var muscleIndex = GetMuscleIndexFromCurveBinding(pair.Value.binding);
                                if (muscleIndex >= 0)
                                {
                                    var humanoidIndex = (HumanBodyBones)HumanTrait.BoneFromMuscle(muscleIndex);
                                    if ((humanoidIndex >= HumanBodyBones.Hips && humanoidIndex <= HumanBodyBones.Jaw) ||
                                        humanoidIndex == HumanBodyBones.UpperChest)
                                    {
                                        updatedMuscle = true;
                                    }
                                }
                            }
                            if (updatedMuscle ||
                                GetTDOFIndexFromCurveBinding(pair.Value.binding) >= 0)
                            {
                                if (pair.Value.deleted == AnimationUtility.CurveModifiedType.CurveModified)
                                {
                                    var curve = GetEditorCurveCache(pair.Value.binding);
                                    if (curve != null)
                                    {
                                        if (pair.Value.beforeCurve != null)
                                        {
                                            for (int i = 0; i < curve.length; i++)
                                            {
                                                if (FindKeyframeIndex(pair.Value.beforeCurve, curve, i) < 0)
                                                {
                                                    EnableAnimatorRootCorrection(curve, i);
                                                }
                                            }
                                            for (int i = 0; i < pair.Value.beforeCurve.length; i++)
                                            {
                                                if (FindKeyframeIndex(curve, pair.Value.beforeCurve, i) < 0)
                                                {
                                                    EnableAnimatorRootCorrection(pair.Value.beforeCurve, i);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Debug.LogWarningFormat("<color=blue>[Very Animation]</color>Lost before cache '{0}'", pair.Value.binding.path, pair.Value.binding.propertyName);
                                        }
                                    }
                                }
                                else if (pair.Value.deleted == AnimationUtility.CurveModifiedType.CurveDeleted)
                                {
                                    EnableAnimatorRootCorrection(currentTime, 0f, currentClip.length);
                                }
                            }
                        }
                        #endregion
                    }
                    #endregion
                    
                    #region HumanoidFootIK
                    if (IsEnableUpdateHumanoidFootIK())
                    {
                        if (IsAnimatorRootCurveBinding(pair.Value.binding) ||
                            GetMuscleIndexFromCurveBinding(pair.Value.binding) >= 0 ||
                            GetTDOFIndexFromCurveBinding(pair.Value.binding) >= 0)
                        {
                            var curve = GetEditorCurveCache(pair.Value.binding);
                            if (curve != null)
                            {
                                if (pair.Value.beforeCurve != null)
                                {
                                    for (int i = 0; i < curve.length; i++)
                                    {
                                        if (FindKeyframeIndex(pair.Value.beforeCurve, curve, i) < 0)
                                        {
                                            AddHumanoidFootIK(curve[i].time);
                                        }
                                    }
                                }
                                else
                                {
                                    Debug.LogWarningFormat("<color=blue>[Very Animation]</color>Lost before cache '{0}'", pair.Value.binding.path, pair.Value.binding.propertyName);
                                }
                            }
                        }
                    }
                    #endregion
                }

                SetUpdateResampleAnimation();
            }
#if Enable_Profiler
            Profiler.EndSample();
#endif
            #endregion

            #region UpdateAnimation
#if Enable_Profiler
            Profiler.BeginSample("UpdateAnimation");
#endif
            bool updateAnimation = false;
            {
                if (isHuman)
                {
                    #region Humanoid
                    if (updateResampleAnimation)
                    {
                        transformPoseSave.ResetOriginalTransform();
                        blendShapeWeightSave.ResetOriginalWeight();
                        UpdateAnimatorRootCorrection();
                        ResampleAnimation();
                        updateAnimation = true;
                    }
                    #region FootIK
                    if (UpdateHumanoidFootIK())
                    {
                        updateAnimation = true;
                    }
                    #endregion
                    #endregion
                }
                else if (vaw.animator != null)
                {
                    #region Generic
                    if (updateResampleAnimation)
                    {
                        transformPoseSave.ResetOriginalTransform();
                        blendShapeWeightSave.ResetOriginalWeight();
                        ResampleAnimation();
                        updateAnimation = true;
                    }
                    #endregion
                }
                else if (vaw.animation != null)
                {
                    #region Legacy
                    if (updateResampleAnimation)
                    {
                        transformPoseSave.ResetOriginalTransform();
                        blendShapeWeightSave.ResetOriginalWeight();
                        ResampleAnimation();
                        updateAnimation = true;
                    }
                    #endregion
                }

                if (updateAnimation)
                {
                    bool nextUpdateResampleAnimation = false;
#if UNITY_2017_1_OR_NEWER
                    if (uAw_2017_1.GetLinkedWithTimeline())
                    {
                        var director = uAw_2017_1.GetTimelineCurrentDirector();
                        var timellineTime = director.time;
                        var timelineWrap = director.extrapolationMode;
                        director.extrapolationMode = UnityEngine.Playables.DirectorWrapMode.None;
                        var befActive = vaw.gameObject.activeInHierarchy;
                        director.Evaluate();
                        if (!befActive && vaw.gameObject.activeInHierarchy)
                        {
                            nextUpdateResampleAnimation = true;
                        }
                        director.time = timellineTime;
                        director.extrapolationMode = timelineWrap;
                    }
#endif
                    UpdateDummyObjectPosition();
                    UpdateSynchroIKSet();

                    if (EditorApplication.isPlaying && EditorApplication.isPaused) //Is there a bug that will not be updated while pausing? Therefore, it forcibly updates it.
                        RendererForceUpdate();
                    humanWorldRootPositionCache = GetHumanWorldRootPosition();
                    humanWorldRootRotationCache = GetHumanWorldRootRotation();
                    ResetAnimatorRootCorrection();
                    SaveAnimatorRootCorrection();
                    vaw.SetRepaintGUI(VeryAnimationWindow.RepaintGUI.Edit);
                    updateResampleAnimation = nextUpdateResampleAnimation;
                    if (uAw.IsShowCurveEditor())
                        SetAnimationWindowRefresh(AnimationWindowStateRefreshType.CurvesOnly);
                }
                else
                {
                    if (EditorApplication.isPlaying && EditorApplication.isPaused && uAw.GetPlaying())  //Is there a bug that will not be updated while pausing? Therefore, it forcibly updates it.
                        RendererForceUpdate();
                }

                EndChangeAnimationCurve();

                #region DummyObject
                if (dummyObject != null)
                {
                    var showGizmo = vaw.IsShowSceneGizmo();
                    if (dummyObject.gameObject.activeSelf != showGizmo)
                    {
                        dummyObject.UpdateState();
                        dummyObject.gameObject.SetActive(showGizmo);
                    }
                }
                #endregion
            }
#if Enable_Profiler
            Profiler.EndSample();
#endif
            #endregion
            
            #region AllCurveCache
            if (editorCurveCacheDirty)
            {
                foreach (var binding in AnimationUtility.GetCurveBindings(currentClip))
                {
                    if (!IsVeryAnimationEditableCurveBinding(binding)) continue;
                    GetEditorCurveCache(binding);
                    if (binding.type == typeof(Transform) && 
                        binding.propertyName == EditorCurveBindingTransformRotationPropertyNames[(int)URotationCurveInterpolation.Mode.RawQuaternions][0])
                    {
                        var tmpBinding = binding;
                        foreach (var propertyName in EditorCurveBindingTransformRotationPropertyNames[(int)URotationCurveInterpolation.Mode.NonBaked])
                        {
                            tmpBinding.propertyName = propertyName;
                            GetEditorCurveCache(tmpBinding);
                        }
                    }
                }
                editorCurveCacheDirty = false;
            }
            #endregion

            if (awForceRefresh)
            {
                uAw.ForceRefresh();
            }

            curvesWasModified.Clear();  //Do it last
            updatePoseFixAnimation = false;
            SetUpdateIKtargetAll(false);
            SetSynchroIKtargetAll(false);

#if Enable_Profiler
            Profiler.EndSample();
#endif
        }

        private void UpdateBones()
        {
            renderers = vaw.gameObject.GetComponentsInChildren<Renderer>(true);
            isHuman = vaw.animator != null && vaw.animator.isHuman;
            animatorApplyRootMotion = vaw.animator != null && vaw.animator.applyRootMotion;
            animatorAvatar = vaw.animator != null ? vaw.animator.avatar : null;
            animatorAvatarRoot = vaw.animator != null ? uAnimator.GetAvatarRoot(vaw.animator) : null;
            #region Humanoid
            if (isHuman)
            {
                if (!vaw.animator.isInitialized)
                    vaw.animator.Rebind();

                humanoidBones = new GameObject[HumanTrait.BoneCount];
                humanoidMuscleLimit = new UAvatar.MuscleLimit[HumanTrait.BoneCount];
                humanoidMuscleContains = new bool[HumanTrait.MuscleCount];
                for (int bone = 0; bone < HumanTrait.BoneCount; bone++)
                {
                    var t = vaw.animator.GetBoneTransform((HumanBodyBones)bone);
                    if (t != null)
                    {
                        humanoidBones[bone] = t.gameObject;
                    }
                    humanoidMuscleLimit[bone] = uAvatar.GetMuscleLimitNonError(animatorAvatar, (HumanBodyBones)bone);
                }
                humanoidHasLeftHand = uAvatar.GetHasLeftHand(animatorAvatar);
                humanoidHasRightHand = uAvatar.GetHasRightHand(animatorAvatar);
                humanoidHasTDoF = uAvatar.GetHasTDoF(animatorAvatar);
                humanoidPreHipRotationInverse = Quaternion.Inverse(uAvatar.GetPreRotation(animatorAvatar, (int)HumanBodyBones.Hips));
                humanoidPoseHipRotation = uAvatar.GetPostRotation(animatorAvatar, (int)HumanBodyBones.Hips);
                for (int mi = 0; mi < HumanTrait.MuscleCount; mi++)
                {
                    bool flag = false;
                    var humanoidIndex = (HumanBodyBones)HumanTrait.BoneFromMuscle(mi);
                    if (humanoidIndex >= 0)
                    {
                        if (humanoidIndex >= HumanBodyBones.LeftThumbProximal && humanoidIndex <= HumanBodyBones.LeftLittleDistal && humanoidHasLeftHand)
                            flag = true;
                        else if (humanoidIndex >= HumanBodyBones.RightThumbProximal && humanoidIndex <= HumanBodyBones.RightLittleDistal && humanoidHasRightHand)
                            flag = true;
                        else
                            flag = humanoidBones[(int)humanoidIndex] != null || HumanVirtualBones[(int)humanoidIndex] != null;
                    }
                    humanoidMuscleContains[mi] = flag;
                }
                humanPoseHandler = new HumanPoseHandler(animatorAvatar, animatorAvatarRoot);
            }
            else
            {
                humanoidBones = null;
                humanoidMuscleLimit = null;
                humanoidHasLeftHand = false;
                humanoidHasRightHand = false;
                humanoidHasTDoF = false;
                humanoidPreHipRotationInverse = Quaternion.identity;
                humanoidPoseHipRotation = Quaternion.identity;
                humanoidMuscleContains = null;
                humanPoseHandler = null;
            }
            #endregion
            #region bones
            bones = EditorCommon.GetHierarchyGameObject(vaw.gameObject).ToArray();
            boneDic = new Dictionary<GameObject, int>(bones.Length);
            for (int i = 0; i < bones.Length; i++)
            {
                boneDic.Add(bones[i], i);
            }
            #endregion
            #region boneIndex2humanoidIndex, humanoidIndex2boneIndex
            if (isHuman)
            {
                boneIndex2humanoidIndex = new HumanBodyBones[bones.Length];
                for (int i = 0; i < bones.Length; i++)
                    boneIndex2humanoidIndex[i] = (HumanBodyBones)EditorCommon.ArrayIndexOf(humanoidBones, bones[i]);
                humanoidIndex2boneIndex = new int[HumanTrait.BoneCount];
                for (int i = 0; i < humanoidBones.Length; i++)
                    humanoidIndex2boneIndex[i] = EditorCommon.ArrayIndexOf(bones, humanoidBones[i]);
            }
            else
            {
                boneIndex2humanoidIndex = null;
                humanoidIndex2boneIndex = null;
            }
            #endregion
            #region bonePaths, bonePathDic, boneSaveTransforms
            bonePaths = new string[bones.Length];
            bonePathDic = new Dictionary<string, int>(bonePaths.Length);
            boneSaveTransforms = new TransformPoseSave.SaveData[bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                bonePaths[i] = AnimationUtility.CalculateTransformPath(bones[i].transform, vaw.gameObject.transform);
                if (!bonePathDic.ContainsKey(bonePaths[i]))
                {
                    bonePathDic.Add(bonePaths[i], i);
                }
                else
                {
                    Debug.LogWarningFormat(Language.GetText(Language.Help.LogMultipleGameObjectsWithSameName), bonePaths[i]);
                }
                boneSaveTransforms[i] = transformPoseSave.GetOriginalTransform(bones[i].transform);
                Assert.IsNotNull(boneSaveTransforms[i]);
            }
            if (isHuman)
            {
                HumanPose humanPose = new HumanPose();
                humanPoseHandler.GetHumanPose(ref humanPose);
                saveHumanPose = humanPose;
            }
            #endregion
            #region calcObject
            {
                calcObject = new DummyObject();
                calcObject.Initialize(vaw.gameObject);
                calcObject.SetOrigin();
                calcObject.SetRendererEnabled(false);
                calcObject.AddEditComponent();
                calcObject.gameObject.SetActive(false);
            }
            #endregion
            #region rootMotionBoneIndex
            rootMotionBoneIndex = -1;
            if (vaw.animator != null)
            {
                if (vaw.animator.isHuman)
                {
                    rootMotionBoneIndex = 0;
                }
#if UNITY_2017_1_OR_NEWER
                else if (uAw_2017_1.GetLinkedWithTimeline() || vaw.animator.applyRootMotion)
#else
                else if (vaw.animator.applyRootMotion)
#endif
                {
                    var genericRootMotionBonePath = uAvatar.GetGenericRootMotionBonePath(animatorAvatar);
                    if (!string.IsNullOrEmpty(genericRootMotionBonePath))
                    {
                        int boneIndex;
                        if (bonePathDic.TryGetValue(genericRootMotionBonePath, out boneIndex))
                        {
                            rootMotionBoneIndex = boneIndex;
                        }
                    }
                }
            }
            #endregion
            #region parentBone
            {
                parentBoneIndexes = new int[bones.Length];
                for (int i = 0; i < bones.Length; i++)
                {
                    if (bones[i].transform.parent != null)
                        parentBoneIndexes[i] = BonesIndexOf(bones[i].transform.parent.gameObject);
                    else
                        parentBoneIndexes[i] = -1;
                }
            }
            #endregion
            UpdateBonesMirror();
            #region humanoidConflict
            if (isHuman)
            {
                humanoidConflict = new bool[bones.Length];
                Action<int> SetHumanoidConflict = null;
                SetHumanoidConflict = (index) =>
                {
                    if (index < 0) return;
                    humanoidConflict[index] = true;
                    if (parentBoneIndexes[index] >= 0)
                        SetHumanoidConflict(parentBoneIndexes[index]);
                };
                SetHumanoidConflict(humanoidIndex2boneIndex[(int)HumanBodyBones.Head]);
                SetHumanoidConflict(humanoidIndex2boneIndex[(int)HumanBodyBones.Jaw]);
                SetHumanoidConflict(humanoidIndex2boneIndex[(int)HumanBodyBones.LeftHand]);
                SetHumanoidConflict(humanoidIndex2boneIndex[(int)HumanBodyBones.LeftThumbDistal]);
                SetHumanoidConflict(humanoidIndex2boneIndex[(int)HumanBodyBones.LeftIndexDistal]);
                SetHumanoidConflict(humanoidIndex2boneIndex[(int)HumanBodyBones.LeftMiddleDistal]);
                SetHumanoidConflict(humanoidIndex2boneIndex[(int)HumanBodyBones.LeftRingDistal]);
                SetHumanoidConflict(humanoidIndex2boneIndex[(int)HumanBodyBones.LeftLittleDistal]);
                SetHumanoidConflict(humanoidIndex2boneIndex[(int)HumanBodyBones.RightHand]);
                SetHumanoidConflict(humanoidIndex2boneIndex[(int)HumanBodyBones.RightThumbDistal]);
                SetHumanoidConflict(humanoidIndex2boneIndex[(int)HumanBodyBones.RightIndexDistal]);
                SetHumanoidConflict(humanoidIndex2boneIndex[(int)HumanBodyBones.RightMiddleDistal]);
                SetHumanoidConflict(humanoidIndex2boneIndex[(int)HumanBodyBones.RightRingDistal]);
                SetHumanoidConflict(humanoidIndex2boneIndex[(int)HumanBodyBones.RightLittleDistal]);
                SetHumanoidConflict(humanoidIndex2boneIndex[(int)HumanBodyBones.LeftFoot]);
                SetHumanoidConflict(humanoidIndex2boneIndex[(int)HumanBodyBones.RightFoot]);
                SetHumanoidConflict(humanoidIndex2boneIndex[(int)HumanBodyBones.LeftToes]);
                SetHumanoidConflict(humanoidIndex2boneIndex[(int)HumanBodyBones.RightToes]);
                foreach (var index in humanoidIndex2boneIndex)
                {
                    if (index >= 0)
                        humanoidConflict[index] = true;
                }
            }
            else
            {
                humanoidConflict = null;
            }
            #endregion
            #region boneShowFlags
            boneShowFlags = new bool[bones.Length];
            if (isHuman)
            {
                ActionBoneShowFlagsHumanoidBody((index) =>
                {
                    boneShowFlags[index] = true;
                });
            }
            else
            {
                bool done = false;
                ActionBoneShowFlagsHaveWeight((index) =>
                {
                    boneShowFlags[index] = true;
                    done = true;
                });
                if (!done)
                {
                    ActionBoneShowFlagsHaveRendererParent((index) =>
                    {
                        boneShowFlags[index] = true;
                        done = true;
                    });
                }
                if (!done)
                {
                    ActionBoneShowFlagsHaveRenderer((index) =>
                    {
                        boneShowFlags[index] = true;
                        done = true;
                    });
                }
                if (!done)
                {
                    ActionBoneShowFlagsAll((index) =>
                    {
                        boneShowFlags[index] = true;
                        done = true;
                    });
                }
                if (animatorApplyRootMotion)
                {
                    if (rootMotionBoneIndex >= 0)
                    {
                        boneShowFlags[rootMotionBoneIndex] = true;
                        boneShowFlags[0] = false;
                    }
                    else if (boneShowFlags.Length > 0)
                    {
                        boneShowFlags[0] = true;
                    }
                }
            }
            {
                var animators = vaw.gameObject.GetComponentsInChildren<Animator>(true);
                foreach (var animator in animators)
                {
                    if (animator == vaw.animator) continue;
                    Action<int> HideFlag = null;
                    HideFlag = (bi) =>
                    {
                        if (bi < 0) return;
                        boneShowFlags[bi] = false;
                        for (int i = 0; i < bones[bi].transform.childCount; i++)
                        {
                            HideFlag(BonesIndexOf(bones[bi].transform.GetChild(i).gameObject));
                        }
                    };
                    HideFlag(BonesIndexOf(animator.gameObject));
                }
            }
            OnBoneShowFlagsUpdated.Invoke();
            #endregion

            IKUpdateBones();
        }
        public void UpdateBonesMirror()
        {
            var boneLRIgnorePaths = new string[bones.Length];
            {
                var splits = !string.IsNullOrEmpty(vaw.editorSettings.settingGenericMirrorNameDifferentCharacters) ? vaw.editorSettings.settingGenericMirrorNameDifferentCharacters.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries) : new string[0];
                for (int i = 0; i < bones.Length; i++)
                {
                    boneLRIgnorePaths[i] = bonePaths[i];
                    foreach (var split in splits)
                    {
                        boneLRIgnorePaths[i] = Regex.Replace(boneLRIgnorePaths[i], split, "*", RegexOptions.IgnoreCase);
                    }
                }
            }
            if (vaw.editorSettings.settingGenericMirrorNameIgnoreCharacter && !string.IsNullOrEmpty(vaw.editorSettings.settingGenericMirrorNameIgnoreCharacterString))
            {
                for (int i = 0; i < bones.Length; i++)
                {
                    var splits = boneLRIgnorePaths[i].Split(new string[] { "/" }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (splits.Length <= 0) continue;
                    for (int j = 0; j < splits.Length; j++)
                    {
                        var index = splits[j].IndexOf(vaw.editorSettings.settingGenericMirrorNameIgnoreCharacterString);
                        if (index < 0) continue;
                        splits[j] = splits[j].Remove(0, (index + 1));
                    }
                    boneLRIgnorePaths[i] = string.Join("/", splits);
                }
            }
            mirrorBoneIndexes = new int[bones.Length];
            mirrorBoneRootIndexes = new int[bones.Length];
            bool[] doneFlag = new bool[bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                if (doneFlag[i])
                    continue;
                Func<int, int, int> GetRootNode = (b1, b2) =>
                {
                    if (b1 < 0 || b2 < 0)
                        return -1;
                    var p1 = bones[b1].transform.parent;
                    var p2 = bones[b2].transform.parent;
                    while (p1 != p2 && p1 != null && p2 != null)
                    {
                        p1 = p1.parent;
                        p2 = p2.parent;
                    }
                    if (p1 == p2)
                        return BonesIndexOf(p1.gameObject);
                    else
                        return -1;
                };

                mirrorBoneIndexes[i] = -1;
                mirrorBoneRootIndexes[i] = -1;
                #region Humanoid
                if (isHuman)
                {
                    var humanoidIndex = boneIndex2humanoidIndex[i];
                    if (humanoidIndex >= 0)
                    {
                        var mhi = HumanBodyMirrorBones[(int)humanoidIndex];
                        if (mhi >= 0)
                        {
                            mirrorBoneIndexes[i] = BonesIndexOf(humanoidBones[(int)mhi]);
                            mirrorBoneRootIndexes[i] = GetRootNode(i, mirrorBoneIndexes[i]);
                        }
                    }
                }
                #endregion
                #region Name
                if (vaw.editorSettings.settingGenericMirrorName && mirrorBoneIndexes[i] < 0)
                {
                    for (int j = 0; j < bones.Length; j++)
                    {
                        if (i == j || boneLRIgnorePaths[i] != boneLRIgnorePaths[j])
                            continue;
                        if (isHuman)
                        {
                            if (boneIndex2humanoidIndex[j] >= 0)
                                continue;
                        }
                        var rootIndex = GetRootNode(i, j);
                        if (rootIndex >= 0)
                        {
                            mirrorBoneIndexes[i] = j;
                            mirrorBoneRootIndexes[i] = rootIndex;
                            break;
                        }
                    }
                }
                #endregion
                #region Position
                if (vaw.editorSettings.settingGenericMirrorPosition && mirrorBoneIndexes[i] < 0)
                {
                    float Threshold = vaw.editorSettings.settingGenericMirrorPositionThreshold;
                    var rootInv = Quaternion.Inverse(boneSaveTransforms[0].rotation);
                    for (int j = 0; j < bones.Length; j++)
                    {
                        if (i == j)
                            continue;
                        if (isHuman)
                        {
                            if (boneIndex2humanoidIndex[j] >= 0)
                                continue;
                        }
                        //ZeroCheck
                        if (boneSaveTransforms[i].localPosition.magnitude < Threshold)
                            continue;
                        //RootCheck
                        var rootIndex = GetRootNode(i, j);
                        if (rootIndex < 0)
                            continue;
                        //ChildCheck
                        {
                            if (bones[i].transform.childCount != bones[j].transform.childCount)
                                continue;
                            bool grandson = true;
                            for (int k = 0; k < bones[i].transform.childCount; k++)
                            {
                                if (bones[i].transform.GetChild(k).childCount != bones[j].transform.GetChild(k).childCount)
                                {
                                    grandson = false;
                                    break;
                                }
                            }
                            if (!grandson)
                                continue;
                        }
                        //PositionCheck
                        {
                            bool result = true;
                            var p1 = bones[i].transform;
                            var p2 = bones[j].transform;
                            while (p1 != bones[rootIndex].transform)
                            {
                                {
                                    var parentRot = (rootInv * boneSaveTransforms[i].rotation) * Quaternion.Inverse(boneSaveTransforms[i].localRotation);
                                    var world = parentRot * boneSaveTransforms[i].localPosition;
                                    world.x = -world.x;
                                    var mparentRot = (rootInv * boneSaveTransforms[j].rotation) * Quaternion.Inverse(boneSaveTransforms[j].localRotation);
                                    var mworld = mparentRot * boneSaveTransforms[j].localPosition;
                                    if (Mathf.Abs(world.x - mworld.x) >= Threshold ||
                                        Mathf.Abs(world.y - mworld.y) >= Threshold ||
                                        Mathf.Abs(world.z - mworld.z) >= Threshold)
                                    {
                                        result = false;
                                        break;
                                    }
                                }
                                p1 = p1.parent;
                                p2 = p2.parent;
                            }
                            if (!result)
                                continue;
                        }
                        mirrorBoneIndexes[i] = j;
                        mirrorBoneRootIndexes[i] = rootIndex;
                    }
                }
                #endregion
                doneFlag[i] = true;
                if (mirrorBoneIndexes[i] >= 0)
                {
                    mirrorBoneIndexes[mirrorBoneIndexes[i]] = i;
                    mirrorBoneRootIndexes[mirrorBoneIndexes[i]] = mirrorBoneRootIndexes[i];
                    doneFlag[mirrorBoneIndexes[i]] = true;
                }
            }
        }
        public void UpdateBlendShapeMirror()
        {
            mirrorBlendShape = new Dictionary<SkinnedMeshRenderer, Dictionary<string, string>>();
            if (vaw.editorSettings.settingBlendShapeMirrorName)
            {
                foreach (var renderer in editGameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    if (renderer.sharedMesh == null) continue;
                    if (renderer.sharedMesh.blendShapeCount <= 0) continue;
                    Dictionary<string, string> nameTable = new Dictionary<string, string>();
                    {
                        var nameLRIgnorePaths = new string[renderer.sharedMesh.blendShapeCount];
                        {
                            var splits = !string.IsNullOrEmpty(vaw.editorSettings.settingBlendShapeMirrorNameDifferentCharacters) ? vaw.editorSettings.settingBlendShapeMirrorNameDifferentCharacters.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries) : new string[0];
                            for (int i = 0; i < renderer.sharedMesh.blendShapeCount; i++)
                            {
                                nameLRIgnorePaths[i] = renderer.sharedMesh.GetBlendShapeName(i);
                                foreach (var split in splits)
                                {
                                    nameLRIgnorePaths[i] = Regex.Replace(nameLRIgnorePaths[i], split, "*", RegexOptions.IgnoreCase);
                                }
                            }
                        }
                        bool[] doneFlag = new bool[renderer.sharedMesh.blendShapeCount];
                        for (int i = 0; i < renderer.sharedMesh.blendShapeCount; i++)
                        {
                            if (doneFlag[i]) continue;
                            doneFlag[i] = true;
                            var nameI = renderer.sharedMesh.GetBlendShapeName(i);
                            if (nameTable.ContainsKey(nameI))
                            {
                                Debug.LogWarningFormat(Language.GetText(Language.Help.LogMultipleBlendShapesWithSameName), renderer.sharedMesh.name, nameI);
                                continue;
                            }
                            for (int j = 0; j < renderer.sharedMesh.blendShapeCount; j++)
                            {
                                if (i == j || nameLRIgnorePaths[i] != nameLRIgnorePaths[j])
                                    continue;
                                var nameJ = renderer.sharedMesh.GetBlendShapeName(j);
                                if (nameTable.ContainsKey(nameJ))
                                {
                                    Debug.LogWarningFormat(Language.GetText(Language.Help.LogMultipleBlendShapesWithSameName), renderer.sharedMesh.name, nameJ);
                                }
                                else
                                {
                                    nameTable.Add(nameI, nameJ);
                                    nameTable.Add(nameJ, nameI);
                                    doneFlag[j] = true;
                                }
                                break;
                            }
                        }
                    }
                    mirrorBlendShape.Add(renderer, nameTable);
                }
            }
        }

        public void SetUpdateResampleAnimation()
        {
            updateResampleAnimation = true;
        }
        public void ResampleAnimation(float time = -1)
        {
            UpdateSyncEditorCurveClip();

            if (time < 0f)
                time = currentTime;
            if (vaw.animator != null)
            {
                if (!vaw.animator.isInitialized)
                    vaw.animator.Rebind();
            }
            currentClip.SampleAnimation(vaw.gameObject, time);

            #region DummyObject
            if (dummyObject != null)
            {
                if (dummyObject.animator != null)
                {
                    if (!dummyObject.animator.isInitialized)
                        dummyObject.animator.Rebind();
                }
                currentClip.SampleAnimation(dummyObject.gameObject, time);
                UpdateDummyObjectPosition();
            }
            #endregion
        }
        public void UpdateSyncEditorCurveClip()
        {
            if (needSyncEditorCurveClip != null)
            {
                uAnimationUtility.Internal_SyncEditorCurves(needSyncEditorCurveClip);
                needSyncEditorCurveClip = null;
            }
        }

        private void UpdateDummyObjectPosition()
        {
            if (dummyObject != null)
            {
                var dt = dummyObject.gameObject.transform;
                if (vaw.editorSettings.settingDummyPositionType == EditorSettings.DummyPositionType.ScenePosition)
                {
                    dt.position = transformPoseSave.startPosition + transformPoseSave.startRotation * vaw.editorSettings.settingDummyObjectPosition;
                }
                else if (vaw.editorSettings.settingDummyPositionType == EditorSettings.DummyPositionType.TimelinePosition)
                {
#if UNITY_2017_1_OR_NEWER
                    Vector3 position;
                    Quaternion rotation;
                    uAw_2017_1.GetRootMotionOffsets(out position, out rotation);
                    dt.position = position + rotation * vaw.editorSettings.settingDummyObjectPosition;
                    dt.rotation = rotation;
#endif
                }
            }
        }

        private void OnHierarchyWindowChanged()
        {
            if (isEditError) return;

            List<GameObject> list = EditorCommon.GetHierarchyGameObject(vaw.gameObject);
            if (bones.Length != list.Count)
            {
                vaw.Release();
                return;
            }
            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i] != list[i] ||
                    bonePaths[i] != AnimationUtility.CalculateTransformPath(bones[i].transform, vaw.gameObject.transform))
                {
                    vaw.Release();
                    return;
                }
            }
        }
        #endregion

        #region HotKey
        public void HotKeys()
        {
            Event e = Event.current;

            Action KeyCommmon = () =>
            {
                vaw.SetRepaintGUI(VeryAnimationWindow.RepaintGUI.All);
                e.Use();
            };
            if (!e.control && !e.alt && !e.shift)
            {
                if (e.keyCode == KeyCode.Escape)
                {
                    EditorApplication.delayCall += () =>
                    {
                        vaw.Release();
                    };
                    KeyCommmon();
                }
                if (e.keyCode == KeyCode.O)
                {
                    Undo.RecordObject(vaw, "Change Clamp");
                    clampMuscle = !clampMuscle;
                    SetUpdateSelectionIKtarget();
                    KeyCommmon();
                }
                if (e.keyCode == KeyCode.L)
                {
                    Undo.RecordObject(vaw, "Change Root Correction Mode");
                    rootCorrectionMode = (RootCorrectionMode)((int)(++rootCorrectionMode) % ((int)RootCorrectionMode.Full + 1));
                    KeyCommmon();
                }
                if (e.keyCode == KeyCode.J)
                {
                    Undo.RecordObject(vaw, "Change Foot IK");
                    autoFootIK = !autoFootIK;
                    KeyCommmon();
                }
                if (e.keyCode == KeyCode.M)
                {
                    Undo.RecordObject(vaw, "Change Mirror");
                    mirrorEnable = !mirrorEnable;
                    SetUpdateResampleAnimation();
                    KeyCommmon();
                }
                if (e.keyCode == KeyCode.I)
                {
                    IKChangeSelection();
                    KeyCommmon();
                }
                if (e.keyCode == KeyCode.PageDown)
                {
#if UNITY_2017_1_OR_NEWER
                    if (!uAw_2017_1.GetLinkedWithTimeline())
#endif
                    {
                        if (currentClip != null)
                        {
                            var clips = AnimationUtility.GetAnimationClips(vaw.gameObject).Distinct().ToArray();
                            for (int i = 0; i < clips.Length; i++)
                            {
                                if (clips[i] == currentClip)
                                {
                                    i = (i + 1) % clips.Length;
                                    uAw.RecordingDisable();
                                    uAw.SetSelectionAnimationClip(clips[i], "Animation next animationclip");
                                    break;
                                }
                            }
                        }
                        KeyCommmon();
                    }
                }
                if (e.keyCode == KeyCode.PageUp)
                {
#if UNITY_2017_1_OR_NEWER
                    if (!uAw_2017_1.GetLinkedWithTimeline())
#endif
                    {
                        if (currentClip != null)
                        {
                            var clips = AnimationUtility.GetAnimationClips(vaw.gameObject).Distinct().ToArray();
                            for (int i = 0; i < clips.Length; i++)
                            {
                                if (clips[i] == currentClip)
                                {
                                    i = ((i - 1) + clips.Length) % clips.Length;
                                    uAw.RecordingDisable();
                                    uAw.SetSelectionAnimationClip(clips[i], "Animation previous animationclip");
                                    break;
                                }
                            }
                        }
                        KeyCommmon();
                    }
                }
                if (e.keyCode == KeyCode.F5)
                {
                    SetUpdateResampleAnimation();
                    SetAnimationWindowRefresh(AnimationWindowStateRefreshType.Everything);
                    KeyCommmon();
                }
                if (e.keyCode == KeyCode.Space)
                {
                    uAw.PlayingChange();
                    KeyCommmon();
                }
                if (e.keyCode == KeyCode.C)
                {
                    uAw.SwitchBetweenCurvesAndDopesheet();
                    KeyCommmon();
                }
                if (e.keyCode == KeyCode.K)
                {
                    #region AddSelectionKeyframe
                    var tool = CurrentTool();
                    if (isHuman)
                    {
                        foreach (var humanoidIndex in SelectionGameObjectsHumanoidIndex())
                        {
                            switch (tool)
                            {
                            case Tool.Move:
                                if (HumanBonesAnimatorTDOFIndex[(int)humanoidIndex] != null)
                                {
                                    SetAnimationValueAnimatorTDOF(HumanBonesAnimatorTDOFIndex[(int)humanoidIndex].index, GetAnimationValueAnimatorTDOF(HumanBonesAnimatorTDOFIndex[(int)humanoidIndex].index));
                                }
                                break;
                            case Tool.Rotate:
                                for (int dof = 0; dof < 3; dof++)
                                {
                                    var muscleIndex = HumanTrait.MuscleFromBone((int)humanoidIndex, dof);
                                    if (muscleIndex >= 0)
                                    {
                                        SetAnimationValueAnimatorMuscle(muscleIndex, GetAnimationValueAnimatorMuscle(muscleIndex));
                                    }
                                }
                                break;
                            }
                        }
                        if (SelectionGameObjectsIndexOf(vaw.gameObject) >= 0)
                        {
                            switch (tool)
                            {
                            case Tool.Move:
                                SetAnimationValueAnimatorRootT(GetAnimationValueAnimatorRootT());
                                break;
                            case Tool.Rotate:
                                SetAnimationValueAnimatorRootQ(GetAnimationValueAnimatorRootQ());
                                break;
                            }
                        }
                    }
                    foreach (var boneIndex in SelectionGameObjectsOtherHumanoidBoneIndex())
                    {
                        switch (tool)
                        {
                        case Tool.Move:
                            SetAnimationValueTransformPosition(boneIndex, GetAnimationValueTransformPosition(boneIndex));
                            break;
                        case Tool.Rotate:
                            SetAnimationValueTransformRotation(boneIndex, GetAnimationValueTransformRotation(boneIndex));
                            break;
                        case Tool.Scale:
                            SetAnimationValueTransformScale(boneIndex, GetAnimationValueTransformScale(boneIndex));
                            break;
                        }
                    }
                    #endregion
                    KeyCommmon();
                }
                if (e.keyCode == KeyCode.Comma)
                {
                    uAw.MoveToPrevFrame();
                    KeyCommmon();
                }
                if (e.keyCode == KeyCode.Period)
                {
                    uAw.MoveToNextFrame();
                    KeyCommmon();
                }
                if (e.keyCode == KeyCode.H)
                {
                    Undo.RecordObject(vaw, "Change Show Flag");
                    if (selectionBones != null)
                    {
                        foreach (var boneIndex in selectionBones)
                        {
                            boneShowFlags[boneIndex] = false;
                        }
                    }
                    OnBoneShowFlagsUpdated.Invoke();
                    KeyCommmon();
                }
                if (e.keyCode == KeyCode.P)
                {
                    if (uAvatarPreview != null)
                    {
                        uAvatarPreview.playing = !uAvatarPreview.playing;
                        if (uAvatarPreview.playing)
                            uAvatarPreview.SetTime(0f);
                        else
                            uAvatarPreview.SetTime(uAw.GetCurrentTime());
                    }
                    KeyCommmon();
                }
            }
            else if (!e.control && !e.shift)
            {
                if (e.keyCode == KeyCode.Comma)
                {
                    uAw.MoveToPreviousKeyframe();
                    KeyCommmon();
                }
                if (e.keyCode == KeyCode.Period)
                {
                    uAw.MoveToNextKeyframe();
                    KeyCommmon();
                }
            }
            else if (!e.control && !e.alt)
            {
                if (e.keyCode == KeyCode.Comma)
                {
                    uAw.MoveToFirstKeyframe();
                    KeyCommmon();
                }
                if (e.keyCode == KeyCode.Period)
                {
                    uAw.MoveToLastKeyframe();
                    KeyCommmon();
                }
                if (e.keyCode == KeyCode.H)
                {
                    Undo.RecordObject(vaw, "Change Show Flag");
                    if (selectionBones != null)
                    {
                        foreach (var boneIndex in selectionBones)
                        {
                            boneShowFlags[boneIndex] = true;
                        }
                    }
                    OnBoneShowFlagsUpdated.Invoke();
                    KeyCommmon();
                }
            }
            else if (!e.alt && !e.shift)
            {
                if (e.keyCode == KeyCode.KeypadPlus)
                {
                    if (originalIK.ikActiveTarget >= 0)
                    {
                        Undo.RecordObject(vaw, "Change Original IK Data");
                        for (int i = 0; i < originalIK.ikTargetSelect.Length; i++)
                        {
                            originalIK.ChangeTypeSetting(originalIK.ikTargetSelect[i], 1);
                        }
                    }
                    KeyCommmon();
                }
                if (e.keyCode == KeyCode.KeypadMinus)
                {
                    if (originalIK.ikActiveTarget >= 0)
                    {
                        Undo.RecordObject(vaw, "Change Original IK Data");
                        for (int i = 0; i < originalIK.ikTargetSelect.Length; i++)
                        {
                            originalIK.ChangeTypeSetting(originalIK.ikTargetSelect[i], -1);
                        }
                    }
                    KeyCommmon();
                }
                if (e.keyCode == KeyCode.Space)
                {
                    uAw.PlayingChange();
                    KeyCommmon();
                }
            }
        }
        public void Commands()
        {
            Event e = Event.current;
            switch (e.type)
            {
            case EventType.ValidateCommand:
                {
                    if (e.commandName == "Cut" ||
                        e.commandName == "Copy" ||
                        e.commandName == "Paste" ||
                        e.commandName == "SelectAll" ||
                        e.commandName == "FrameSelected" ||
                        e.commandName == "FrameSelectedWithLock" ||
                        e.commandName == "Delete" ||
                        e.commandName == "SoftDelete" ||
                        e.commandName == "Duplicate")
                    {
                        e.Use();
                    }
                }
                break;
            case EventType.ExecuteCommand:
                {
                    if (e.commandName == "Cut" ||
                        e.commandName == "Delete" ||
                        e.commandName == "SoftDelete" ||
                        e.commandName == "Duplicate")
                    {
                        e.Use();
                    }
                    else if (e.commandName == "Copy")
                    {
                        if (CommandCopy())
                            e.Use();
                    }
                    else if (e.commandName == "Paste")
                    {
                        if (CommandPaste())
                            e.Use();
                    }
                    else if (e.commandName == "SelectAll")
                    {
                        if (CommandSelectAll())
                            e.Use();
                    }
                    else if (e.commandName == "FrameSelected")
                    {
                        if (CommandFrameSelected(false))
                            e.Use();
                    }
                    else if (e.commandName == "FrameSelectedWithLock")
                    {
                        if (CommandFrameSelected(true))
                            e.Use();
                    }
                }
                break;
            }
        }
        private bool CommandCopy()
        {
            if (copyPaste != null)
                GameObject.DestroyImmediate(copyPaste);
            copyPaste = ScriptableObject.CreateInstance<PoseTemplate>();
            SaveSelectionPoseTemplate(copyPaste);
            return true;
        }
        private bool CommandPaste()
        {
            if (copyPaste != null)
            {
                Undo.RegisterCompleteObjectUndo(currentClip, "Paste");
                LoadPoseTemplate(copyPaste);
            }
            return true;
        }
        private bool CommandSelectAll()
        {
            List<GameObject> selectObjects = new List<GameObject>(bones.Length);
            for (int i = 0; i < bones.Length; i++)
            {
                if (!IsShowBone(i)) continue;
                selectObjects.Add(bones[i]);
            }
            List<HumanBodyBones> selectVirtual = new List<HumanBodyBones>(HumanVirtualBones.Length);
            for (int i = 0; i < HumanVirtualBones.Length; i++)
            {
                if (!IsShowVirtualBone((HumanBodyBones)i)) continue;
                selectVirtual.Add((HumanBodyBones)i);
            }
            SelectGameObjects(selectObjects.ToArray(), selectVirtual.ToArray());
            return true;
        }
        private bool CommandFrameSelected(bool withLock)
        {
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null) return false;
            Bounds bounds;
            if (!GetSelectionBounds(out bounds, 0.333f))
                return false;
            if (!withLock)
            {
                uSceneView.SetViewIsLockedToObject(sceneView, EditorApplication.timeSinceStartup - sceneView.lastFramingTime < 0.5);
                sceneView.FixNegativeSize();
                uSceneView.Frame(sceneView, bounds, EditorApplication.isPlaying);
                sceneView.lastFramingTime = EditorApplication.timeSinceStartup;
            }
            else
            {
                uSceneView.SetViewIsLockedToObject(sceneView, withLock);
                sceneView.FixNegativeSize();
                uSceneView.Frame(sceneView, bounds, EditorApplication.isPlaying);
            }
            return true;
        }
        #endregion

        #region SelectionGameObject
        public void SelectGameObjectEvent()
        {
            #region selectionGameObjects
            {
                if (selectionGameObjects == null)
                    selectionGameObjects = new List<GameObject>();
                selectionGameObjects.Clear();
                foreach (var go in Selection.gameObjects)
                    selectionGameObjects.Add(go);
                if (Selection.activeGameObject != null)
                {
                    selectionGameObjects.Remove(Selection.activeGameObject);
                    selectionGameObjects.Insert(0, Selection.activeGameObject);
                }
            }
            #endregion
            #region selectionBones
            {
                if (selectionBones == null)
                    selectionBones = new List<int>();
                selectionBones.Clear();
                foreach (var go in selectionGameObjects)
                {
                    var boneIndex = BonesIndexOf(go);
                    if (boneIndex < 0) continue;
                    selectionBones.Add(boneIndex);
                }
            }
            #endregion
            if (EditorWindow.focusedWindow == uAw.instance)
            {
                selectionHumanVirtualBones = null;
                ClearIkTargetSelect();
            }
        }
        public void SelectGameObjectMouseDrag(GameObject[] go, HumanBodyBones[] virtualBones, AnimatorIKCore.IKTarget[] animatorIKTarget, int[] originalIKTarget)
        {
            Undo.RecordObject(vaw, "Change Selection");
            Selection.objects = go;
            selectionHumanVirtualBones = virtualBones != null ? new List<HumanBodyBones>(virtualBones) : null;
            animatorIK.ikTargetSelect = animatorIKTarget;
            animatorIK.OnSelectionChange();
            originalIK.ikTargetSelect = originalIKTarget;
            originalIK.OnSelectionChange();
            animationWindowSynchroSelection = true;
        }
        public void SelectGameObjectPlusKey(GameObject go)
        {
            var select = new List<GameObject>();
            if (go != null)
                select.Add(go);
            var selectVirtual = new List<HumanBodyBones>();
            var e = Event.current;
            if (e.alt)
            {
                if (go != null)
                {
                    var boneIndex = BonesIndexOf(go);
                    ActionAllBoneChildren(boneIndex, (ci) =>
                    {
                        select.Add(bones[ci]);
                    });
                    ActionAllVirtualBoneChildren(boneIndex, (cvhi) =>
                    {
                        selectVirtual.Add(cvhi);
                    });
                }
            }
            if (e.control || e.shift)
            {
                if (selectionHumanVirtualBones != null)
                    selectVirtual.AddRange(selectionHumanVirtualBones);
                if (selectionGameObjects != null)
                {
                    foreach (var o in selectionGameObjects)
                    {
                        if (select.Contains(o))
                            select.Remove(o);
                        else
                            select.Add(o);
                    }
                }
            }
            if (go != null && select.Contains(go))
                Selection.activeGameObject = go;
            SelectGameObjects(select.ToArray(), selectVirtual.ToArray());
        }
        public void SelectGameObject(GameObject go)
        {
            Undo.RecordObject(vaw, "Change Selection");
            Selection.objects = new UnityEngine.Object[] { go };
            selectionHumanVirtualBones = null;
            ClearIkTargetSelect();
            SetUpdateResampleAnimation();
            animationWindowSynchroSelection = true;
            vaw.SetRepaintGUI(VeryAnimationWindow.RepaintGUI.All);
        }
        public void SelectGameObjects(GameObject[] go, HumanBodyBones[] virtualBones = null)
        {
            Undo.RecordObject(vaw, "Change Selection");
            Selection.objects = go;
            selectionHumanVirtualBones = virtualBones != null ? new List<HumanBodyBones>(virtualBones) : null;
            ClearIkTargetSelect();
            SetUpdateResampleAnimation();
            animationWindowSynchroSelection = true;
            vaw.SetRepaintGUI(VeryAnimationWindow.RepaintGUI.All);
        }
        public void SelectVirtualBonePlusKey(HumanBodyBones humanoidIndex)
        {
            if (humanoidIndex2boneIndex[(int)humanoidIndex] >= 0)
                return;

            var select = new List<GameObject>();
            var selectVirtual = new List<HumanBodyBones>();
            selectVirtual.Add(humanoidIndex);
            var e = Event.current;
            if (e.alt)
            {
                Action VirtualNeck = () =>
                {
                    int boneIndex;
                    if (humanoidIndex2boneIndex[(int)HumanBodyBones.Neck] >= 0)
                        boneIndex = humanoidIndex2boneIndex[(int)HumanBodyBones.Neck];
                    else
                    {
                        selectVirtual.Add(HumanBodyBones.Neck);
                        boneIndex = humanoidIndex2boneIndex[(int)HumanBodyBones.Head];
                    }
                    select.Add(bones[boneIndex]);
                    ActionAllBoneChildren(boneIndex, (ci) =>
                    {
                        select.Add(bones[ci]);
                    });
                };
                Action VirtualLeftShoulder = () =>
                {
                    int boneIndex;
                    if (humanoidIndex2boneIndex[(int)HumanBodyBones.LeftShoulder] >= 0)
                        boneIndex = humanoidIndex2boneIndex[(int)HumanBodyBones.LeftShoulder];
                    else
                    {
                        selectVirtual.Add(HumanBodyBones.LeftShoulder);
                        boneIndex = humanoidIndex2boneIndex[(int)HumanBodyBones.LeftUpperArm];
                    }
                    select.Add(bones[boneIndex]);
                    ActionAllBoneChildren(boneIndex, (ci) =>
                    {
                        select.Add(bones[ci]);
                    });
                };
                Action VirtualRightShoulder = () =>
                {
                    int boneIndex;
                    if (humanoidIndex2boneIndex[(int)HumanBodyBones.RightShoulder] >= 0)
                        boneIndex = humanoidIndex2boneIndex[(int)HumanBodyBones.RightShoulder];
                    else
                    {
                        selectVirtual.Add(HumanBodyBones.RightShoulder);
                        boneIndex = humanoidIndex2boneIndex[(int)HumanBodyBones.RightUpperArm];
                    }
                    select.Add(bones[boneIndex]);
                    ActionAllBoneChildren(boneIndex, (ci) =>
                    {
                        select.Add(bones[ci]);
                    });
                };
                switch (humanoidIndex)
                {
                case HumanBodyBones.Chest:
                    selectVirtual.Add(HumanBodyBones.UpperChest);
                    VirtualNeck();
                    VirtualLeftShoulder();
                    VirtualRightShoulder();
                    break;
                case HumanBodyBones.Neck:
                    VirtualNeck();
                    break;
                case HumanBodyBones.LeftShoulder:
                    VirtualLeftShoulder();
                    break;
                case HumanBodyBones.RightShoulder:
                    VirtualRightShoulder();
                    break;
                case HumanBodyBones.UpperChest:
                    VirtualNeck();
                    VirtualLeftShoulder();
                    VirtualRightShoulder();
                    break;
                default:
                    Assert.IsTrue(false);
                    break;
                }
            }
            if (e.control || e.shift)
            {
                if (selectionGameObjects != null)
                    select.AddRange(selectionGameObjects);
                if (selectionHumanVirtualBones != null)
                {
                    foreach (var h in selectionHumanVirtualBones)
                    {
                        if (selectVirtual.Contains(h))
                            selectVirtual.Remove(h);
                        else
                            selectVirtual.Add(h);
                    }
                }
            }
            SelectGameObjects(select.ToArray(), selectVirtual.ToArray());
        }
        public void SelectHumanoidBones(HumanBodyBones[] bones)
        {
            List<GameObject> goList = new List<GameObject>();
            List<HumanBodyBones> virtualList = new List<HumanBodyBones>();
            foreach (var hi in bones)
            {
                if (hi < 0)
                    goList.Add(vaw.gameObject);
                else if (humanoidBones[(int)hi] != null)
                    goList.Add(humanoidBones[(int)hi]);
                else if (HumanVirtualBones[(int)hi] != null)
                    virtualList.Add(hi);
            }
            Undo.RecordObject(vaw, "Change Selection");
            Selection.objects = goList.ToArray();
            selectionHumanVirtualBones = virtualList;
            ClearIkTargetSelect();
            SetUpdateResampleAnimation();
            animationWindowSynchroSelection = true;
            vaw.SetRepaintGUI(VeryAnimationWindow.RepaintGUI.All);
        }
        public void SelectAnimatorIKTargetPlusKey(AnimatorIKCore.IKTarget ikTarget)
        {
            List<AnimatorIKCore.IKTarget> select = new List<AnimatorIKCore.IKTarget>();
            select.Add(ikTarget);
            var e = Event.current;
            if (e != null && (e.control || e.shift))
            {
                if (animatorIK.ikTargetSelect != null)
                {
                    select = new List<AnimatorIKCore.IKTarget>(animatorIK.ikTargetSelect);
                    if (EditorCommon.ArrayContains(animatorIK.ikTargetSelect, ikTarget))
                        select.Remove(ikTarget);
                    else
                        select.Add(ikTarget);
                }
            }
            SelectIKTargets(select.ToArray(), null);
        }
        public void SelectOriginalIKTargetPlusKey(int ikTarget)
        {
            List<int> select = new List<int>();
            select.Add(ikTarget);
            var e = Event.current;
            if (e != null && (e.control || e.shift))
            {
                if (originalIK.ikTargetSelect != null)
                {
                    select = new List<int>(originalIK.ikTargetSelect);
                    if (EditorCommon.ArrayContains(originalIK.ikTargetSelect, ikTarget))
                        select.Remove(ikTarget);
                    else
                        select.Add(ikTarget);
                }
            }
            SelectIKTargets(null, select.ToArray());
        }
        public void SelectIKTargets(AnimatorIKCore.IKTarget[] animatorIKTargets, int[] originalIKTargets)
        {
            Undo.RecordObject(vaw, "Change Selection");
            Selection.activeGameObject = null;
            selectionHumanVirtualBones = null;
            animatorIK.ikTargetSelect = animatorIKTargets;
            animatorIK.OnSelectionChange();
            originalIK.ikTargetSelect = originalIKTargets;
            originalIK.OnSelectionChange();
            SetUpdateResampleAnimation();
            animationWindowSynchroSelection = true;
            vaw.SetRepaintGUI(VeryAnimationWindow.RepaintGUI.All);
        }
        public void SetAnimationWindowSynchroSelection(EditorCurveBinding[] bindings)
        {
            animationWindowSynchroSelection = true;
            if (animationWindowSynchroSelectionBindings == null)
                animationWindowSynchroSelectionBindings = new Dictionary<int, EditorCurveBinding>();
            foreach (var binding in bindings)
            {
                animationWindowSynchroSelectionBindings[GetEditorCurveBindingHashCode(binding)] = binding;
            }
        }
        public void AddAnimationWindowSynchroSelection(EditorCurveBinding binding)
        {
            animationWindowSynchroSelection = true;
            if (animationWindowSynchroSelectionBindings == null)
                animationWindowSynchroSelectionBindings = new Dictionary<int, EditorCurveBinding>();
            animationWindowSynchroSelectionBindings[GetEditorCurveBindingHashCode(binding)] = binding;
        }
        public List<EditorCurveBinding> GetSelectionEditorCurveBindings()
        {
            var bindings = new List<EditorCurveBinding>();
            
            Tool tool = CurrentTool();

            #region Humanoid
            if (isHuman)
            {
                Action<HumanBodyBones> AddMuscle = (humanoidIndex) =>
                {
                    switch (tool)
                    {
                    case Tool.Move:
                        if (HumanBonesAnimatorTDOFIndex[(int)humanoidIndex] != null)
                        {
                            for (int dof = 0; dof < 3; dof++)
                                bindings.Add(AnimationCurveBindingAnimatorTDOF(HumanBonesAnimatorTDOFIndex[(int)humanoidIndex].index, dof));
                        }
                        break;
                    case Tool.Rotate:
                        for (int dof = 0; dof < 3; dof++)
                        {
                            var muscleIndex = HumanTrait.MuscleFromBone((int)humanoidIndex, dof);
                            if (muscleIndex < 0) continue;
                            bindings.Add(AnimationCurveBindingAnimatorMuscle(muscleIndex));
                        }
                        break;
                    }
                };
                {
                    foreach (var go in selectionGameObjects)
                    {
                        HumanBodyBones humanoidIndex;
                        if (vaw.gameObject == go)
                        {
                            switch (tool)
                            {
                            case Tool.Move:
                                foreach (var binding in AnimationCurveBindingAnimatorRootT)
                                    bindings.Add(binding);
                                break;
                            case Tool.Rotate:
                                foreach (var binding in AnimationCurveBindingAnimatorRootQ)
                                    bindings.Add(binding);
                                break;
                            }
                        }
                        else if ((humanoidIndex = HumanoidBonesIndexOf(go)) >= 0)
                        {
                            AddMuscle(humanoidIndex);
                        }
                    }
                }
                if (selectionHumanVirtualBones != null)
                {
                    foreach (var humanoidIndex in selectionHumanVirtualBones)
                    {
                        AddMuscle(humanoidIndex);
                    }
                }
                #region AnimatorIK
                if (animatorIK.ikTargetSelect != null)
                {
                    foreach (var ikTarget in animatorIK.ikTargetSelect)
                    {
                        if (!animatorIK.ikData[(int)ikTarget].enable) continue;
                        switch (ikTarget)
                        {
                        case AnimatorIKCore.IKTarget.Head:
                            AddMuscle(HumanBodyBones.Head);
                            AddMuscle(HumanBodyBones.Neck);
                            break;
                        case AnimatorIKCore.IKTarget.LeftHand:
                            AddMuscle(HumanBodyBones.LeftHand);
                            AddMuscle(HumanBodyBones.LeftLowerArm);
                            AddMuscle(HumanBodyBones.LeftUpperArm);
                            break;
                        case AnimatorIKCore.IKTarget.RightHand:
                            AddMuscle(HumanBodyBones.RightHand);
                            AddMuscle(HumanBodyBones.RightLowerArm);
                            AddMuscle(HumanBodyBones.RightUpperArm);
                            break;
                        case AnimatorIKCore.IKTarget.LeftFoot:
                            AddMuscle(HumanBodyBones.LeftFoot);
                            AddMuscle(HumanBodyBones.LeftLowerLeg);
                            AddMuscle(HumanBodyBones.LeftUpperLeg);
                            break;
                        case AnimatorIKCore.IKTarget.RightFoot:
                            AddMuscle(HumanBodyBones.RightFoot);
                            AddMuscle(HumanBodyBones.RightLowerLeg);
                            AddMuscle(HumanBodyBones.RightUpperLeg);
                            break;
                        }
                    }
                }
                #endregion
            }
            #endregion
            #region Generic
            {
                Action<int> AddGeneric = (boneIndex) =>
                {
                    if (boneIndex == rootMotionBoneIndex)
                    {
                        switch (tool)
                        {
                        case Tool.Move:
                            foreach (var binding in AnimationCurveBindingAnimatorRootT)
                                bindings.Add(binding);
                            break;
                        case Tool.Rotate:
                            foreach (var binding in AnimationCurveBindingAnimatorRootQ)
                                bindings.Add(binding);
                            break;
                        }
                    }
                    else
                    {
                        switch (tool)
                        {
                        case Tool.Move:
                            for (int dof = 0; dof < 3; dof++)
                                bindings.Add(AnimationCurveBindingTransformPosition(boneIndex, dof));
                            break;
                        case Tool.Rotate:
                            for (int dof = 0; dof < 3; dof++)
                            {
                                bindings.Add(AnimationCurveBindingTransformRotation(boneIndex, dof, URotationCurveInterpolation.Mode.Baked));
                                bindings.Add(AnimationCurveBindingTransformRotation(boneIndex, dof, URotationCurveInterpolation.Mode.NonBaked));
                                bindings.Add(AnimationCurveBindingTransformRotation(boneIndex, dof, URotationCurveInterpolation.Mode.RawEuler));
                            }
                            for (int dof = 0; dof < 4; dof++)
                                bindings.Add(AnimationCurveBindingTransformRotation(boneIndex, dof, URotationCurveInterpolation.Mode.RawQuaternions));
                            break;
                        case Tool.Scale:
                            for (int dof = 0; dof < 3; dof++)
                                bindings.Add(AnimationCurveBindingTransformScale(boneIndex, dof));
                            break;
                        }
                    }
                };
                if (selectionBones != null)
                {
                    foreach (var boneIndex in selectionBones)
                    {
                        AddGeneric(boneIndex);
                    }
                }
                #region OriginalIK
                if (originalIK.ikTargetSelect != null)
                {
                    foreach (var ikTarget in originalIK.ikTargetSelect)
                    {
                        if (ikTarget < 0 || ikTarget >= originalIK.ikData.Count) continue;
                        if (!originalIK.ikData[ikTarget].enable) continue;
                        for (int i = 0; i < originalIK.ikData[ikTarget].joints.Count; i++)
                        {
                            var boneIndex = BonesIndexOf(originalIK.ikData[ikTarget].joints[i].bone);
                            if (boneIndex < 0) continue;
                            AddGeneric(boneIndex);
                        }
                    }
                }
                #endregion
            }
            #endregion

            return bindings;
        }

        public int SelectionGameObjectsIndexOf(GameObject go)
        {
            if (selectionGameObjects != null)
            {
                for (int i = 0; i < selectionGameObjects.Count; i++)
                {
                    if (selectionGameObjects[i] == go)
                        return i;
                }
            }
            return -1;
        }
        public bool SelectionGameObjectsContains(HumanBodyBones humanIndex)
        {
            if (selectionBones != null)
            {
                foreach (var boneIndex in selectionBones)
                {
                    if (boneIndex2humanoidIndex[boneIndex] == humanIndex)
                        return true;
                }
            }
            if (selectionHumanVirtualBones != null)
            {
                foreach (var vb in selectionHumanVirtualBones)
                {
                    if (vb == humanIndex)
                        return true;
                }
            }
            return false;
        }
        public HumanBodyBones SelectionGameObjectHumanoidIndex()
        {
            var humanoidIndex = HumanoidBonesIndexOf(selectionActiveGameObject);
            if (humanoidIndex < 0 && selectionHumanVirtualBones != null && selectionHumanVirtualBones.Count > 0)
                humanoidIndex = selectionHumanVirtualBones[0];
            return humanoidIndex;
        }
        public List<HumanBodyBones> SelectionGameObjectsHumanoidIndex()
        {
            List<HumanBodyBones> list = new List<HumanBodyBones>();
            if (isHuman)
            {
                if (selectionBones != null)
                {
                    foreach (var boneIndex in selectionBones)
                    {
                        var humanoidIndex = boneIndex2humanoidIndex[boneIndex];
                        if (humanoidIndex < 0) continue;
                        list.Add(humanoidIndex);
                    }
                }
                if (selectionHumanVirtualBones != null)
                {
                    foreach (var humanoidIndex in selectionHumanVirtualBones)
                    {
                        if (humanoidIndex < 0) continue;
                        list.Add(humanoidIndex);
                    }
                }
            }
            return list;
        }
        public bool IsSelectionGameObjectsHumanoidIndexContains(HumanBodyBones humanoidIndex)
        {
            if (isHuman)
            {
                if (selectionBones != null)
                {
                    foreach (var boneIndex in selectionBones)
                    {
                        if (boneIndex2humanoidIndex[boneIndex] == humanoidIndex)
                            return true;
                    }
                }
                if (selectionHumanVirtualBones != null)
                {
                    foreach (var hi in selectionHumanVirtualBones)
                    {
                        if (hi == humanoidIndex)
                            return true;
                    }
                }
            }
            return false;
        }
        public List<int> SelectionGameObjectsOtherHumanoidBoneIndex()
        {
            List<int> list = new List<int>();
            if (isHuman)
            {
                if (selectionBones != null)
                {
                    foreach (var boneIndex in selectionBones)
                    {
                        if (boneIndex == rootMotionBoneIndex ||
                            boneIndex2humanoidIndex[boneIndex] >= 0) continue;
                        list.Add(boneIndex);
                    }
                }
            }
            else
            {
                if (selectionBones != null)
                {
                    list.AddRange(selectionBones);
                }
            }
            return list;
        }
        public List<int> SelectionGameObjectsMuscleIndex(int dofIndex = -1)
        {
            List<int> list = new List<int>();
            var humanoidIndexs = SelectionGameObjectsHumanoidIndex();
            if (dofIndex < 0)
            {
                foreach (var humanoidIndex in humanoidIndexs)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        var muscleIndex = HumanTrait.MuscleFromBone((int)humanoidIndex, j);
                        if (muscleIndex < 0) continue;
                        list.Add(muscleIndex);
                    }
                }
            }
            else if (dofIndex >= 0 && dofIndex <= 2)
            {
                foreach (var humanoidIndex in humanoidIndexs)
                {
                    int muscleIndex = HumanTrait.MuscleFromBone((int)humanoidIndex, dofIndex);
                    if (muscleIndex < 0) continue;
                    list.Add(muscleIndex);
                }
            }
            return list;
        }
        public int GetSelectionGameObjectsMaxLevel()
        {
            int maxLevel = 1;
            if (selectionBones != null)
            {
                foreach (var boneIndex in selectionBones)
                {
                    int level = 1;
                    var bi = boneIndex;
                    while (parentBoneIndexes[bi] >= 0)
                    {
                        if (selectionBones.Contains(parentBoneIndexes[bi]))
                            level++;
                        bi = parentBoneIndexes[bi];
                    }
                    maxLevel = Math.Max(maxLevel, level);
                }
            }
            if (isHuman)
            {
                foreach (var humanoidIndex in SelectionGameObjectsHumanoidIndex())
                {
                    int level = 1;
                    var hi = humanoidIndex;
                    var parentHi = (HumanBodyBones)HumanTrait.GetParentBone((int)hi);
                    while (parentHi >= 0)
                    {
                        if (IsSelectionGameObjectsHumanoidIndexContains(parentHi))
                            level++;
                        hi = parentHi;
                        parentHi = (HumanBodyBones)HumanTrait.GetParentBone((int)hi);
                    }
                    maxLevel = Math.Max(maxLevel, level);
                }
            }
            return maxLevel;
        }

        public int BonesIndexOf(GameObject go)
        {
            if (boneDic != null && go != null)
            {
                int boneIndex;
                if (boneDic.TryGetValue(go, out boneIndex))
                {
                    return boneIndex;
                }
            }
            return -1;
        }
        public HumanBodyBones HumanoidBonesIndexOf(GameObject go)
        {
            if (go == null || !isHuman) return (HumanBodyBones)(-1);
            if (humanoidBones != null)
            {
                var boneIndex = BonesIndexOf(go);
                if (boneIndex >= 0)
                {
                    return boneIndex2humanoidIndex[boneIndex];
                }
            }
            return (HumanBodyBones)(-1);
        }
        #endregion

        #region Bounds
        public bool GetSelectionBounds(out Bounds bounds, float sizeAdjustment = 0f)
        {
            bounds = new Bounds(Vector3.zero, gameObjectBounds.size * sizeAdjustment);
            bool done = false;
            #region Bone
            if (selectionBones != null)
            {
                foreach (var boneIndex in selectionBones)
                {
                    if (isHuman && boneIndex == 0) continue;
                    if (!done)
                    {
                        bounds.center = editBones[boneIndex].transform.position;
                        done = true;
                    }
                    else
                    {
                        bounds.Encapsulate(editBones[boneIndex].transform.position);
                    }
                }
            }
            #endregion
            if (isHuman)
            {
                #region Root
                if (SelectionGameObjectsIndexOf(vaw.gameObject) >= 0)
                {
                    var position = humanWorldRootPositionCache;
                    if (!done)
                    {
                        bounds.center = position;
                        done = true;
                    }
                    else
                    {
                        bounds.Encapsulate(position);
                    }
                }
                #endregion
                #region VirtualBone
                if (selectionHumanVirtualBones != null)
                {
                    foreach (var virtualBone in selectionHumanVirtualBones)
                    {
                        var position = GetHumanVirtualBonePosition(virtualBone);
                        if (!done)
                        {
                            bounds.center = position;
                            done = true;
                        }
                        else
                        {
                            bounds.Encapsulate(position);
                        }
                    }
                }
                #endregion
                #region AnimatorIK
                if (animatorIK.ikActiveTarget != AnimatorIKCore.IKTarget.None)
                {
                    foreach (var ikTarget in animatorIK.ikTargetSelect)
                    {
                        var position = animatorIK.ikData[(int)ikTarget].worldPosition;
                        if (!done)
                        {
                            bounds.center = position;
                            done = true;
                        }
                        else
                        {
                            bounds.Encapsulate(position);
                        }
                    }
                }
                #endregion
            }
            #region OriginalIK
            if (originalIK.ikActiveTarget >= 0)
            {
                foreach (var ikTarget in originalIK.ikTargetSelect)
                {
                    var position = originalIK.ikData[ikTarget].worldPosition;
                    if (!done)
                    {
                        bounds.center = position;
                        done = true;
                    }
                    else
                    {
                        bounds.Encapsulate(position);
                    }
                }
            }
            #endregion
            return done;
        }
        public Vector3 GetSelectionOriginalBoundsCenter()
        {
            Vector3 center;
            {
                Vector3 combinePosition = Vector3.zero;
                int combineCount = 0;
                #region Bone
                if (selectionBones != null)
                {
                    foreach (var boneIndex in selectionBones)
                    {
                        if (isHuman && boneIndex == 0) continue;
                        combinePosition += boneSaveTransforms[boneIndex].position;
                        combineCount++;
                    }
                }
                #endregion
                if (isHuman)
                {
                    #region Root
                    if (SelectionGameObjectsIndexOf(vaw.gameObject) >= 0)
                    {
                        Vector3 position;
                        {
                            var localToWorldMatrix = Matrix4x4.TRS(transformPoseSave.startPosition, transformPoseSave.startRotation, transformPoseSave.startScale);
                            var bodyPosition = saveHumanPose.bodyPosition * editAnimator.humanScale;
                            position = localToWorldMatrix.MultiplyPoint3x4(bodyPosition);
                        }
                        combinePosition += position;
                        combineCount++;
                    }
                    #endregion
                    #region VirtualBone
                    if (selectionHumanVirtualBones != null)
                    {
                        foreach (var virtualBone in selectionHumanVirtualBones)
                        {
                            Vector3 position;
                            {
                                var vbs = HumanVirtualBones[(int)virtualBone];
                                if (vbs != null)
                                {
                                    foreach (var vb in vbs)
                                    {
                                        if (editHumanoidBones[(int)vb.boneA] == null || editHumanoidBones[(int)vb.boneB] == null) continue;
                                        var posA = boneSaveTransforms[humanoidIndex2boneIndex[(int)vb.boneA]].position;
                                        var posB = boneSaveTransforms[humanoidIndex2boneIndex[(int)vb.boneB]].position;
                                        position = Vector3.Lerp(posA, posB, vb.leap);
                                        combinePosition += position;
                                        combineCount++;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                    #region AnimatorIK
                    if (animatorIK.ikActiveTarget != AnimatorIKCore.IKTarget.None)
                    {
                        foreach (var ikTarget in animatorIK.ikTargetSelect)
                        {
                            var boneIndex = BonesIndexOf(animatorIK.ikData[(int)ikTarget].root);
                            var position = boneSaveTransforms[boneIndex].position;
                            combinePosition += position;
                            combineCount++;
                        }
                    }
                    #endregion
                }
                #region OriginalIK
                if (originalIK.ikActiveTarget >= 0)
                {
                    foreach (var ikTarget in originalIK.ikTargetSelect)
                    {
                        var boneIndex = BonesIndexOf(originalIK.ikData[(int)ikTarget].root);
                        var position = boneSaveTransforms[boneIndex].position;
                        combinePosition += position;
                        combineCount++;
                    }
                }
                #endregion
                center = combineCount > 0 ? combinePosition / combineCount : Vector3.zero;
            }
            return center;
        }
        #endregion

        #region ShowBone
        public List<int> skeletonShowBoneList { get; private set; }
        public bool[] boneShowFlags;

        public void ActionBoneShowFlagsAll(Action<int> action)
        {
            if (boneShowFlags == null) return;
            for (int i = 0; i < boneShowFlags.Length; i++)
                action(i);
        }
        public void ActionBoneShowFlagsHumanoidBody(Action<int> action)
        {
            action(0);    //Root
            for (int i = 0; i <= (int)HumanBodyBones.RightToes; i++)
            {
                var boneIndex = humanoidIndex2boneIndex[i];
                if (boneIndex < 0) continue;
                action(boneIndex);
            }
            {
                var boneIndex = humanoidIndex2boneIndex[(int)HumanBodyBones.UpperChest];
                if (boneIndex >= 0)
                    action(boneIndex);
            }
        }
        public void ActionBoneShowFlagsHumanoidHead(Action<int> action)
        {
            for (int i = (int)HumanBodyBones.LeftEye; i <= (int)HumanBodyBones.Jaw; i++)
            {
                var boneIndex = humanoidIndex2boneIndex[i];
                if (boneIndex < 0) continue;
                action(boneIndex);
            }
            for (int i = (int)HumanBodyBones.Neck; i <= (int)HumanBodyBones.Head; i++)
            {
                var boneIndex = humanoidIndex2boneIndex[i];
                if (boneIndex < 0) continue;
                action(boneIndex);
            }
        }
        public void ActionBoneShowFlagsHumanoidLeftHand(Action<int> action)
        {
            for (int i = (int)HumanBodyBones.LeftThumbProximal; i <= (int)HumanBodyBones.LeftLittleDistal; i++)
            {
                var boneIndex = humanoidIndex2boneIndex[i];
                if (boneIndex < 0) continue;
                action(boneIndex);
            }
        }
        public void ActionBoneShowFlagsHumanoidRightHand(Action<int> action)
        {
            for (int i = (int)HumanBodyBones.RightThumbProximal; i <= (int)HumanBodyBones.RightLittleDistal; i++)
            {
                var boneIndex = humanoidIndex2boneIndex[i];
                if (boneIndex < 0) continue;
                action(boneIndex);
            }
        }
        public void ActionBoneShowFlagsHaveWeight(Action<int> action)
        {
            if (renderers == null) return;
            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;
                if (renderer is SkinnedMeshRenderer)
                {
                    var skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
                    var mesh = skinnedMeshRenderer.sharedMesh;
                    if (mesh != null)
                    {
                        var meshBones = skinnedMeshRenderer.bones;
                        Dictionary<int, int> list = new Dictionary<int, int>();
                        Action<int> SetBoneIndex = (index) =>
                        {
                            if (index < 0 || index >= meshBones.Length)
                                return;
                            if (list.ContainsKey(index))
                                return;
                            if (meshBones[index] != null)
                                list.Add(index, BonesIndexOf(meshBones[index].gameObject));
                            else
                                list.Add(index, -1);
                        };
                        foreach (var boneWeight in mesh.boneWeights)
                        {
                            if (boneWeight.weight0 > 0f)
                                SetBoneIndex(boneWeight.boneIndex0);
                            if (boneWeight.weight1 > 0f)
                                SetBoneIndex(boneWeight.boneIndex1);
                            if (boneWeight.weight2 > 0f)
                                SetBoneIndex(boneWeight.boneIndex2);
                            if (boneWeight.weight3 > 0f)
                                SetBoneIndex(boneWeight.boneIndex3);
                        }
                        foreach (var pair in list)
                        {
                            if (pair.Value >= 0)
                                action(pair.Value);
                        }
                    }
                }
            }
        }
        public void ActionBoneShowFlagsHaveRenderer(Action<int> action)
        {
            if (renderers == null) return;
            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;
                var boneIndex = BonesIndexOf(renderer.transform.gameObject);
                if (boneIndex >= 0)
                    action(boneIndex);
            }
        }
        public void ActionBoneShowFlagsHaveRendererParent(Action<int> action)
        {
            if (renderers == null) return;
            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;
                var parent = renderer.transform.parent;
                if (parent == null) continue;
                var boneIndex = BonesIndexOf(parent.gameObject);
                if (boneIndex >= 0)
                    action(boneIndex);
            }
        }

        public bool IsShowBone(int boneIndex)
        {
            if (boneIndex < 0 || boneIndex >= bones.Length || bones[boneIndex] == null || !boneShowFlags[boneIndex])
                return false;
            if (isHuman)
            {
                if (animatorIK.IsIKBone(boneIndex2humanoidIndex[boneIndex]) != AnimatorIKCore.IKTarget.None)
                    return false;
            }
            if (originalIK.IsIKBone(boneIndex) >= 0)
                return false;
            return true;
        }
        public bool IsShowVirtualBone(HumanBodyBones humanoidIndex)
        {
            if (!isHuman)
                return false;
            if (humanoidBones[(int)humanoidIndex] != null || HumanVirtualBones[(int)humanoidIndex] == null)
                return false;
            {
                var ikIndex = animatorIK.IsIKBone(humanoidIndex);
                if (ikIndex >= 0 && ikIndex < AnimatorIKCore.IKTarget.Total)
                {
                    if (animatorIK.ikData[(int)ikIndex].enable)
                        return false;
                }
            }
            {
                var phi = GetHumanVirtualBoneParentBone(humanoidIndex);
                if (phi < 0 || humanoidIndex2boneIndex[(int)phi] < 0) return false;
                if (!IsShowBone(humanoidIndex2boneIndex[(int)phi])) return false;
            }
            return true;
        }
        public Action OnBoneShowFlagsUpdated;
        public void UpdateSkeletonShowBoneList()
        {
            if (isEditError) return;

            var flags = new bool[bones.Length];
            Action<int, bool> SetParentFlags = null;
            SetParentFlags = (boneIndex, flag) =>
            {
                if (parentBoneIndexes[boneIndex] < 0 || parentBoneIndexes[parentBoneIndexes[boneIndex]] < 0) return;
                flags[parentBoneIndexes[boneIndex]] = flag;
                SetParentFlags(parentBoneIndexes[boneIndex], flag);
            };
            for (int i = 0; i < bones.Length; i++)
            {
                flags[i] = boneShowFlags[i] && parentBoneIndexes[i] >= 0;
                if (flags[i])
                    SetParentFlags(i, true);
            }
            if (skeletonShowBoneList == null)
                skeletonShowBoneList = new List<int>(flags.Length);
            else
                skeletonShowBoneList.Clear();
            for (int i = 0; i < flags.Length; i++)
            {
                if (flags[i])
                    skeletonShowBoneList.Add(i);
            }
        }
        #endregion

        #region UnityTool
        public Tool lastTool { get; set; }

        public void EnableCustomTools(Tool t)
        {
            if (Tools.current != Tool.None)
            {
                lastTool = Tools.current;
                Tools.current = t;
            }
        }
        public void DisableCustomTools()
        {
            if (lastTool != Tool.None)
            {
                Tools.current = lastTool;
                lastTool = Tool.None;
            }
        }
        public Tool CurrentTool()
        {
            Tool tool = lastTool;
            var humanoidIndex = SelectionGameObjectHumanoidIndex();
            if (animatorIK.ikActiveTarget != AnimatorIKCore.IKTarget.None)
            {
                tool = Tool.Rotate;
            }
            else if (originalIK.ikActiveTarget >= 0)
            {
                tool = Tool.Rotate;
            }
            else if (selectionActiveBone >= 0 && selectionActiveBone == rootMotionBoneIndex)
            {
                if (lastTool == Tool.Move) tool = Tool.Move;
                else tool = Tool.Rotate;
            }
            else if (humanoidIndex >= 0)
            {
                switch (lastTool)
                {
                case Tool.Move:
                    if (!humanoidHasTDoF || HumanBonesAnimatorTDOFIndex[(int)humanoidIndex] == null || editHumanoidBones[(int)humanoidIndex] == null)
                        tool = Tool.Rotate;
                    break;
                default:
                    tool = Tool.Rotate;
                    break;
                }
            }
            else
            {
                switch (lastTool)
                {
                case Tool.Move:
                case Tool.Scale:
                    break;
                default:
                    tool = Tool.Rotate;
                    break;
                }
            }
            return tool;
        }
        #endregion

        #region Preview
        public void PreviewGUI()
        {
            if (uAvatarPreview != null)
            {
                {
                    EditorGUILayout.BeginHorizontal("preToolbar", GUILayout.Height(17f));
                    GUILayout.FlexibleSpace();
                    Rect lastRect = GUILayoutUtility.GetLastRect();
                    if (currentClip != null)
                        GUI.Label(lastRect, currentClip.name, "preToolbar2");
                    uAvatarPreview.OnPreviewSettings();
                    EditorGUILayout.EndHorizontal();
                }
                if (uAvatarPreview.playing)
                {
                    vaw.Repaint();
                }
                else
                {
                    if (Event.current.type == EventType.Repaint)
                        uAvatarPreview.ForceUpdate();
                }

                //It is not PreviewCullingLayer, but there is a problem that it will be rendered in Preview, so take measures here. 
                bool dummyActive = false;
                if (dummyObject != null && dummyObject.gameObject.activeSelf)
                {
                    dummyObject.gameObject.SetActive(false);
                    dummyActive = true;
                }
                {
                    var rect = EditorGUILayout.GetControlRect(false, 0);
                    rect.height = Math.Max(vaw.position.height - rect.y, 0);
                    uAvatarPreview.OnGUI(rect, "preBackground");
                }
                if (dummyObject != null && dummyActive)
                {
                    dummyObject.gameObject.SetActive(true);
                }
            }
        }
        #endregion

        #region SaveSettings
        public void LoadSaveSettings(VeryAnimationSaveSettings saveSettings)
        {
            animatorIK.LoadIKSaveSettings(saveSettings);
            originalIK.LoadIKSaveSettings(saveSettings);
            #region SelectionSet
            {
                selectionSetList = new List<VeryAnimationSaveSettings.SelectionData>();
                if (saveSettings != null && saveSettings.selectionData != null)
                {
                    foreach (var data in saveSettings.selectionData)
                    {
                        var newData = new VeryAnimationSaveSettings.SelectionData()
                        {
                            name = data.name,
                        };
                        {
                            var bones = new List<GameObject>();
                            if (data.bones != null)
                            {
                                foreach (var bone in data.bones)
                                {
                                    if (bone == null) continue;
                                    bones.Add(bone);
                                }
                            }
                            newData.bones = bones.ToArray();
                        }
                        {
                            var vbones = new List<HumanBodyBones>();
                            if (data.virtualBones != null)
                            {
                                foreach (var vbone in data.virtualBones)
                                {
                                    if (vbone < 0 || vbone >= HumanBodyBones.LastBone || humanoidBones[(int)vbone] != null) continue;
                                    vbones.Add(vbone);
                                }
                            }
                            newData.virtualBones = vbones.ToArray();
                        }
                        selectionSetList.Add(newData);
                    }
                }
            }
            #endregion
        }
        public void SaveSaveSettings(VeryAnimationSaveSettings saveSettings)
        {
            animatorIK.SaveIKSaveSettings(saveSettings);
            originalIK.SaveIKSaveSettings(saveSettings);

            #region SelectionSet
            {
                saveSettings.selectionData = selectionSetList.ToArray();
            }
            #endregion
        }
        #endregion

        #region Etc
        public void ActionAllBoneChildren(int boneIndex, Action<int> action)
        {
            var t = bones[boneIndex].transform;
            for (int i = 0; i < t.childCount; i++)
            {
                var childIndex = BonesIndexOf(t.GetChild(i).gameObject);
                if (childIndex < 0) continue;
                action.Invoke(childIndex);
                ActionAllBoneChildren(childIndex, action);
            }
        }
        public void ActionAllVirtualBoneChildren(int boneIndex, Action<HumanBodyBones> action)
        {
            if (!isHuman) return;
            Func<HumanBodyBones, bool> Check = (hi) =>
            {
                Action<HumanBodyBones> Invoke = (hhi) =>
                {
                    if (humanoidBones[(int)hhi] == null)
                        action.Invoke(hhi);
                };
                switch (hi)
                {
                case HumanBodyBones.Hips:
                case HumanBodyBones.Spine:
                    Invoke(HumanBodyBones.Chest);
                    Invoke(HumanBodyBones.Neck);
                    Invoke(HumanBodyBones.LeftShoulder);
                    Invoke(HumanBodyBones.RightShoulder);
                    Invoke(HumanBodyBones.UpperChest);
                    return true;
                case HumanBodyBones.Chest:
                    Invoke(HumanBodyBones.Neck);
                    Invoke(HumanBodyBones.LeftShoulder);
                    Invoke(HumanBodyBones.RightShoulder);
                    Invoke(HumanBodyBones.UpperChest);
                    return true;
                case HumanBodyBones.UpperChest:
                    Invoke(HumanBodyBones.Neck);
                    Invoke(HumanBodyBones.LeftShoulder);
                    Invoke(HumanBodyBones.RightShoulder);
                    return true;
                }
                return false;
            };

            if (Check(boneIndex2humanoidIndex[boneIndex]))
                return;
            var t = bones[boneIndex].transform;
            for (int i = 0; i < t.childCount; i++)
            {
                var childIndex = BonesIndexOf(t.GetChild(i).gameObject);
                if (childIndex < 0) continue;
                if (Check(boneIndex2humanoidIndex[childIndex]))
                    return;
                ActionAllVirtualBoneChildren(childIndex, action);
            }
        }

        public Type GetBoneType(int boneIndex)
        {
            if (isHuman && (vaw.gameObject == bones[boneIndex] || boneIndex2humanoidIndex[boneIndex] >= 0))
            {
                return typeof(Animator);
            }
            else if (rootMotionBoneIndex >= 0 && rootMotionBoneIndex == boneIndex)
            {
                return typeof(Animator);
            }
            else
            {
                var renderer = bones[boneIndex].GetComponent<Renderer>();
                if (renderer != null)
                    return renderer.GetType();
                else
                    return typeof(Transform);
            }
        }

        private void RendererForceUpdate()
        {
            if (renderers == null) return;
            //It is necessary to avoid situations where only display is not updated.
            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;
                renderer.enabled = !renderer.enabled;
                renderer.enabled = !renderer.enabled;
            }
        }
        #endregion
    }
}
