using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VeryAnimation
{
    public class Language
    {
        public enum LanguageType
        {
            English,
            Japanese,
            Total,
        }
        public static readonly string[] LanguageTypeString =
        {
            LanguageType.English.ToString(),
            LanguageType.Japanese.ToString(),
        };
        private static LanguageType languageType;

        public static void SetLanguage(LanguageType type)
        {
            languageType = type;
            if (OnLanguageChanged != null)
                OnLanguageChanged.Invoke();
        }

        public enum Help
        {
            #region MainGUI
            AnimationWindowisnotopen,
            AnimationWindowisnotfocus,
            TheSequenceEditortowhichAnimationislinkedisnotenabled,
            Noanimatableobjectselected,
            AnimationClipisnotselectedinAnimationWindow,
            GameObjectisnotActive,
            Editingonoptimizedtransformhierarchyisnotsupported,
            ThereisnoAnimationClipinAnimationComponent,
            EditingLegacywhileplayingisnotsupported,
            AnimatorControllerisnotfound,
            ThereisnoAnimationClipinAnimatorController,
            AnimatorControllerisnoteditable,
            TheAnimationTracktowhichAnimationislinkedisnotenabled,
            EditingTimelinewhileplayingisnotsupported,
            TimelineGameObjectisnotActive,
            TimelinePlayableDirectorisnotEnable,
            TranslationDoFisdisabled,
            Roottransformscaleratiodoesnotmatch,
            //Animation
            HelpAnimation,
            AnimationclipisReadOnly,
            AnimationClipSettingsLoopPoseisenabled,
            AnimationClipSettingsRootTransformPositionYisFeet,
            AnimationClipSettingsBasedUponisnotOriginal,
            AnimationClipSettingsCycleOffsetisnot0,
            AnimationClipSettingsMirrorisenabled,
            AnimationClipSettingsFramerateofTimelineandAnimationClipdonotmatch,
            //Tools
            HelpTools,
            HelpToolsCopy,
            HelpToolsTrim,
            HelpToolsCreateNewClip,
            HelpToolsHumanoidIK,
            HelpToolsParameterRelatedCurves,
            HelpToolsRotationCurveInterpolation,
            HelpToolsEnsureQuaternionContinuity,
            HelpToolsCleanup,
            HelpToolsFixErrors,
            HelpToolsExport,
            HelpToolsHumanoidWarning,
            HelpToolsRotationCurveInterpolationEulerAnglesWarning,
            //Settings
            HelpSettings,
            //Help
            HelpHelp,
            //Preview
            HelpPreview,
            #endregion
            #region ControlGUI
            //AnimatorIK
            HelpAnimatorIK,
            AnimatorIKAll,
            //OriginalIK
            HelpOriginalIK,
            OriginalIKAll,
            OriginalIKTypeCcdIK,
            OriginalIKTypeLimbIK,
            OriginalIKTypeLockAt,
            OriginalIKResetRotations,
            OriginalIKDirection,
            OriginalIKPleasespecifyGameObject,
            //Humanoid
            HelpHumanoid,
            //SelectionSet
            HelpSelectionSet,
            //Hierarchy
            HelpHierarchy,
            HierarchyHumanoidName,
            HierarchyToolbarAll,
            HierarchyToolbarWeight,
            HierarchyToolbarBody,
            HierarchyToolbarHead,
            HierarchyToolbarLeftHand,
            HierarchyToolbarRightHand,
            HierarchyToolbarRenderer,
            HierarchyToolbarRendererParent,
            #endregion
            #region EditorGUI
            //Options
            EditorOptionsClamp,
            EditorOptionsFootIK,
            EditorOptionsMirror,
            //RootCorrection
            EditorRootCorrection,
            EditorRootCorrectionDisable,
            EditorRootCorrectionSingle,
            EditorRootCorrectionFull,
            //Pose
            HelpPose,
            EditorPoseHumanoidReset,
            EditorPoseStart,
            EditorPoseBind,
            EditorPosePrefab,
            EditorPoseMirror,
            EditorPoseTemplate,
            EditorPoseSaveAs,
            //BlendPose
            HelpBlendPose,
            BlendPoseNotPoseReady,
            BlendPoseModeTree,
            BlendPoseModeSelection,
            //MuscleGroup
            HelpMuscleGroup,
            //BlendShape
            HelpBlendShape,
            //Selection
            HelpSelection,
            SelectionHip,
            SelectionHumanoidConflict,
            SelectionNothingisselected,
            SelectionMirror,
            SelectionUpdateIK,
            SelectionResetIK,
            SelectionAnimatorIKSpaceTypeGlobal,
            SelectionAnimatorIKSpaceTypeLocal,
            SelectionAnimatorIKSpaceTypeParent,
            SelectionOriginalIKSpaceTypeGlobal,
            SelectionOriginalIKSpaceTypeLocal,
            SelectionOriginalIKSpaceTypeParent,
            SelectionIKFixed,
            #endregion
            #region ToolsGUI
            ToolsWindowVAEditing,
            ToolsWindowHelpResetPose,
            ToolsWindowHelpTemplatePose,
            ToolsWindowHelpRemoveSaveSettings,
            ToolsWindowHelpReplaceReference,
            #endregion
            #region Dialog
            DisplayDialogSaveInsideAssetsFolder,
            DisplayDialogAnimationCompressionOff,
            DisplayDialogAnimationRemoveSaveSettings,
            DisplayDialogAnimationReplaceReference,
            #endregion
            #region Log
            LogUpdateGenericRootMotionRawEulerError,
            LogAnimationClipReadOnlyError,
            LogGenericCurveHumanoidConflictError,
            LogGenericCurveRootConflictError,
            LogGenericCurveGenericRootConflictError,
            LogParameterRelatedCurveNameChanged,
            LogFixOverRotationCurve,
            LogFixErrors,
            LogSampleBindPoseBoneLengthError,
            LogSampleBindPoseBoneNullError,
            LogSampleBindPoseUnknownError,
            LogFailedLoadPoseError,
            LogMultipleGameObjectsWithSameName,
            LogMultipleBlendShapesWithSameName,
            #endregion
        }
        private static readonly GUIContent[][] LanguageHelps = new GUIContent[(int)LanguageType.Total][]
        {
            #region English
            new GUIContent[]
            {
                #region MainGUI
                new GUIContent("Animation Window is not open."),
                new GUIContent("Animation Window is not focus."),
                new GUIContent("The Sequence Editor to which Animation is linked is not enabled.\nPlease press the Unlink button on Animation Window."),
                new GUIContent("No animatable object selected.\n(Animator or Animation Component)"),
                new GUIContent("Animation Clip is not selected in Animation Window."),
                new GUIContent("GameObject is not Active."),
                new GUIContent("Editing on optimized transform hierarchy is not supported.\nPlease deoptimize transform hierarchy."),
                new GUIContent("There is no Animation Clip in Animation Component."),
                new GUIContent("Editing Legacy while playing is not supported."),
                new GUIContent("Animator Controller is not found."),
                new GUIContent("There is no Animation Clip in Animator Controller."),
                new GUIContent("Animator Controller is not editable."),
                new GUIContent("The Animation Track to which Animation is linked is not editable.\nPlease press the Unlink button on Animation Window."),
                new GUIContent("Editing Timeline while playing is not supported."),
                new GUIContent("Timeline GameObject is not Active."),
                new GUIContent("Timeline Playable Director is not Enable."),
                new GUIContent("{0} is not found.\n{1} Translation DoF is disabled."),
                new GUIContent("Root transform scale ratio does not match.\nIn this case Animator IK will not result in normal results.\nPlease set X, Y and Z to the same value.\n\t\'{0}\'\tScale{1}"),
                //Animation
                new GUIContent("This is animation information being edited."),
                new GUIContent("Animation clip is Read-Only.\nPlease select editable animation clip in Animation window.\nAlternatively, duplicate the selected animation clip from the button."),
                new GUIContent("Loop Pose (Blend) of Animation Clip setting is enabled.\nIn this state, the closer to the end of the animation, the larger the difference due to the correction when playing back."),
                new GUIContent("Animation Clip Settings Root Transform Position (Y) is Feet.\nHowever, Foot IK Keyframe does not exist.\nIt must be created."),
                new GUIContent("Animation Clip Settings Based Upon is not Original.\nIn this state, the animation behaves differently than during editing.\nIf you want it and have not changed it please change it to Original."),
                new GUIContent("Animation Clip Settings Cycle Offset is not 0.\nIn this case Animator IK will not result in normal results.\nPlease set 0."),
                new GUIContent("Animation Clip Settings Mirror is enabled.\nSince it does not correspond, it does not operate normally.\nPlease disable it."),
                new GUIContent("The frame rate of Timeline and Animation Clip do not match.\nBecause it causes problems, please set same.\n Animation Clip {0} != Timeline {1}"),
                //Tools
                new GUIContent("This is a variety of animation related processing."),
                new GUIContent("Copy a range of keyframes and events."),
                new GUIContent("Trim the animation clip."),
                new GUIContent("Create a new animation clip.\nThe settings of AnimationClip and AnimatorState are copied."),
                new GUIContent("Edit animation curve used when Foot IK of Animator State is valid.\nIt is used only for Humanoid Motion."),
                new GUIContent("Edit animation curve that relays the value to Animator Controller Parameter.\nThe only valid parameter type is 'Float'.\nIt is used only for Humanoid and Generic Motion."),
                new GUIContent("Change the interpolation mode of all Transform Rotation Curves.\nIt is recommended to standardize to Quaternion."),
                new GUIContent("Execute 'AnimationClip.EnsureQuaternionContinuity'.\nThe quaternion curve is modified."),
                new GUIContent("Removes the selected contents from the animation clip."),
                new GUIContent("It fix the condition where error or assert occurs.\nNormally you do not need to run it."),
                new GUIContent("Export the Collada files.\nCollada files can be converted to FBX files with other tools."),
                new GUIContent("Only Animation Type Humanoid will be effective."),
                new GUIContent("This conversion may cause problems such as reverse rotation."),
                //Settings
                new GUIContent("This will change the setting.\nSettings are saved in 'EditorPrefs'."),
                //Help
                new GUIContent("This is other information.\nHotkeys works only when focused on Scene View or Editor Window."),
                //Preview
                new GUIContent("This will display a preview.\nIt can be displayed with other models by drag and drop."),
                #endregion
                #region ControlGUI
                //AnimatorIK
                new GUIContent("Configure the IK using the function of Animator."),
                new GUIContent("All", "Change all targets"),
                //OriginalIK
                new GUIContent("Configure the IK that is specific to Very Animation.\nPlease select the bone which becomes the tip of IK and press the add button."),
                new GUIContent("All", "Change all targets"),
                new GUIContent("Basic", "This is the basic CCD-IK."),
                new GUIContent("Limb", "Use it for limbs."),
                new GUIContent("LockAt", "It is used to direct it toward the target direction."),
                new GUIContent("Reset Rotations", "When it is enabled, it will be reset to the initial state before updating.\nAs a result, twisting will not occur, but the accuracy falls off as it deviates from the initial position."),
                new GUIContent("Direction", "Direction of bending"),
                new GUIContent("Please specify GameObject of the correct hierarchy."),
                //Humanoid
                new GUIContent("This is the whole structure of Humanoid.\nHold down the Alt key and select, all children will also be selected."),
                //Selection
                new GUIContent("You can save and load the bones selected state with a name.\nPlease select the bones and press the plus button to add."),
                //Hierarchy
                new GUIContent("Hold down the Alt key and change Toggle to change all children 's flags as well.\nHold down the Alt key and select, all children will also be selected."),
                new GUIContent("Humanoid Name", "It displays in Hierarchy under the name of Mecanim Humanoid."),
                new GUIContent("All", "Change all bones"),
                new GUIContent("Weight", "Change only bones with vertex weights"),
                new GUIContent("Body", "Change only Mecanim Humanoid Body"),
                new GUIContent("Head", "Change only Mecanim Humanoid Head"),
                new GUIContent("Left Hand", "Change only Mecanim Humanoid Left Hand"),
                new GUIContent("Right Hand", "Change only Mecanim Humanoid Right Hand"),
                new GUIContent("Renderer", "Change only the bone with the Renderer Component"),
                new GUIContent("Renderer Parent", "Change only the parent of the bone that has the Renderer Component"),
                #endregion
                #region EditorGUI
                //Options
                new GUIContent("Clamp", "Limit Muscle to -1 to 1"),
                new GUIContent("Foot IK", "Automatically update Foot IK.\nFor operation in Timeline it is forcibly effective and works."),
                new GUIContent("Mirror", "Mirror left and right"),
                //RootCorrection
                new GUIContent("Root Correction", "This is a function to automatically modify Root so that the whole positional relationship does not fluctuate when Muscle etc. are changed."),
                new GUIContent(VeryAnimation.RootCorrectionMode.Disable.ToString(), "No correction is made."),
                new GUIContent(VeryAnimation.RootCorrectionMode.Single.ToString(), "It corrects only the changed frame."),
                new GUIContent(VeryAnimation.RootCorrectionMode.Full.ToString(), "Corrects all frames that are affected before and after the change."),
                //Pose
                new GUIContent("Perform operations that affect the whole."),
                new GUIContent("Reset", "Mecanim Humanoid default pose"),
                new GUIContent("Start", "Edit start pose"),
                new GUIContent("Bind", "Mesh Bind Pose.\nInitial state poses of models such as T and A."),
                new GUIContent("Prefab", "Prefab Pose"),
                new GUIContent("Mirror", "Mirror left and right."),
                new GUIContent("Template", "Load from template file"),
                new GUIContent("Save as", "Save as template file"),
                //BlendPose
                new GUIContent("Blend the two poses.\nBy applying only a part, you can reuse facial expressions created and saved once, pose of fingers, etc."),
                new GUIContent("Please set the pose to the left and right."),
                new GUIContent(BlendPoseTree.EditMode.Tree.ToString(), "Applies only to the operated Node or below."),
                new GUIContent(BlendPoseTree.EditMode.Selection.ToString(), "Applies only to selected bones."),
                //MuscleGroup
                new GUIContent("This will manipulate the Muscle of Mecanim Humanoid collectively to a certain extent.\nApplies only to the operated Node or below."),
                //BlendShape
                new GUIContent("Manage all Mesh Blend Shapes."),
                //Selection
                new GUIContent("Information on the currently selected object is displayed."),
                new GUIContent("No editing target"),
                new GUIContent("Can not edit because it conflicts with Humanoid"),
                new GUIContent("Nothing is selected"),
                new GUIContent("Mirror", "Mirror left and right.\n{0}"),
                new GUIContent("Update", "Calculate IK."),
                new GUIContent("Reset", "Correct IK according to the part."),
                new GUIContent(AnimatorIKCore.AnimatorIKData.SpaceType.Global.ToString(), "Global space"),
                new GUIContent(AnimatorIKCore.AnimatorIKData.SpaceType.Local.ToString(), "Local space of IK root parent transform"),
                new GUIContent(AnimatorIKCore.AnimatorIKData.SpaceType.Parent.ToString(), "Local space of parent transform"),
                new GUIContent(OriginalIKCore.OriginalIKData.SpaceType.Global.ToString(), "Global space"),
                new GUIContent(OriginalIKCore.OriginalIKData.SpaceType.Local.ToString(), "Local space of IK root parent transform"),
                new GUIContent(OriginalIKCore.OriginalIKData.SpaceType.Parent.ToString(), "Local space of parent transform"),
                new GUIContent("Fixed", "When enabled, it does not reset the target transform."),
                #endregion
                #region ToolsGUI
                new GUIContent("It does not work while editing VeryAnimation."),
                new GUIContent("Resets selected objects and children to a prefab transform."),
                new GUIContent("Set it to the pose of Template."),
                new GUIContent("Remove all 'VeryAnimationSaveSettings' component.\nPlease make a backup of the project."),
#if UNITY_2017_1_OR_NEWER
                new GUIContent("Replace animation clip reference in all AnimatorController, AnimatorOverrideController, Animation and Timeline.\nPlease make a backup of the project."),
#else
                new GUIContent("Replace animation clip reference in all AnimatorController, AnimatorOverrideController and Animation.\nPlease make a backup of the project."),
#endif
	            #endregion
                #region Dialog
                new GUIContent("Need to save in the Assets folder", "You need to save the file inside of the project's assets floder"),
                new GUIContent("Warning", "Animation compression of the source clip is invalid.\nIt is recommended to change compression before duplicating to reduce keyframes.\nThe setting is 'Anim. Compression' of Model Importer settings."),
                new GUIContent("Remove Save Settings", "Remove all 'VeryAnimationSaveSettings' component.\nPlease make backup and then execute.\nAre you sure?"),
                new GUIContent("Replace Reference", "Replace all assets in the project.\nPlease make backup and then execute.\nAre you sure?"),
                #endregion
                #region Log
                new GUIContent("<color=blue>[Very Animation]</color>It can not update RootQ because of Euler Angles.\nPlease change to Quaternion with 'Tools/Rotation Curve Interpolation'."),
                new GUIContent("<color=blue>[Very Animation]</color>The animation clip is read-only. '{0}'"),
                new GUIContent("<color=blue>[Very Animation]</color>This bone transform operation conflicts with the humanoid. Therefore, it can not operate. '{0}'"),
                new GUIContent("<color=blue>[Very Animation]</color>This bone transform operation conflicts with the root motion. Therefore, it can not operate. '{0}'"),
                new GUIContent("<color=blue>[Very Animation]</color>This bone transform operation conflicts with the root motion. Please edit RootT. '{0}'"),
                new GUIContent("<color=blue>[Very Animation]</color>It is a name that cannot be used. The parameter name was changed. '{0}' -> '{1}'"),
                new GUIContent("<color=blue>[Very Animation]</color>There was a difference of more than 180 degrees in the key frames before and after, so the key was added for correct movement.\nThere may be a minute change in the animation curve. '{0} - {1}'"),
                new GUIContent("<color=blue>[Very Animation]</color>There must be at least two keyframes. If not, an Assert will occur.\nKeyframes were added for correction. '{0} - {1}'"),
                new GUIContent("<color=blue>[Very Animation]</color>The count of bones in SkinnedMeshRenderer and bindposes count of Mesh do not match. '{0}' != '{1}'"),
                new GUIContent("<color=blue>[Very Animation]</color>There is null in SkinnedMeshRenderer.bones.\nYou can not get BindPose with this incorrect setting.\nname = {0}, index = {1}"),
                new GUIContent("<color=blue>[Very Animation]</color>An attempt to acquire BindPose failed due to an unknown error.\nI guess that the state of SkinnedMeshRenderer is not normal."),
                new GUIContent("<color=blue>[Very Animation]</color>Failed to load Pose. '{0}'"),
                new GUIContent("<color=blue>[Very Animation]</color>Target for curve is ambiguous since there are multiple GameObjects with same name.\nPlease change the name of GameObject. ({0})"),
                new GUIContent("<color=blue>[Very Animation]</color>The target is ambiguous because there are multiple same names in Mesh 's BlendShape.\nPlease change Mesh so that BlendShape does not overlap. (Mesh - {0}, Name - {1})"),
	            #endregion
            },
            #endregion
            #region Japanese
            new GUIContent[]
            {
                #region MainGUI
                new GUIContent("AnimationWindowが見つかりません。"),
                new GUIContent("AnimationWindowが前面に表示されていません。"),
                new GUIContent("AnimationWindowがリンクしているTimelineが有効ではありません。\nAnimationWindowのUnlinkボタンを押してリンクを解除して下さい。"),
                new GUIContent("アニメーション可能なオブジェクトが選択されていません。\n(Animator or Animation Component)"),
                new GUIContent("AnimationWindowでAnimationClipが選択されていません。"),
                new GUIContent("GameObjectをActiveにする必要があります。"),
                new GUIContent("最適化されたTransform hierarchyの編集はサポートされていません。\nTransform hierarchyの最適化を解除して下さい。"),
                new GUIContent("AnimationComponentにAnimationClipがありません。"),
                new GUIContent("Legacy(AnimationComponent)の実行中の編集はサポートされていません。"),
                new GUIContent("AnimatorControllerがセットされていません。"),
                new GUIContent("AnimatorControllerにAnimationClipがありません。"),
                new GUIContent("AnimatorControllerが編集できません。"),
                new GUIContent("AnimationWindowがリンクしているAnimationTrackが編集できません。\nAnimationWindowのUnlinkボタンを押してリンクを解除して下さい。"),
                new GUIContent("Timelineの実行中の編集はサポートされていません。"),
                new GUIContent("TimelineのGameObjectをActiveにする必要があります。"),
                new GUIContent("Timeline Playable DirectorをEnableにする必要があります。"),
                new GUIContent("{0}が見つかりません。\n{1}のTranslation DoFは無効です。"),
                new GUIContent("Root transform scaleが統一されていません。.\nこの状態ではAnimator IKが正しい結果を取得できません。\nScaleのXとYとZを同じ値にして下さい。\n\t\'{0}\'\tScale{1}"),
                //Animation
                new GUIContent("編集対象のアニメーション情報です。"),
                new GUIContent("AnimationClipは読み取り専用です。\n編集可能なAnimationClipをAnimationWindowで選択して下さい。\nまたは右のボタンからこのAnimationClipの複製と参照の置換を行って下さい。"),
                new GUIContent("AnimationClip設定のLoop Pose (Blend)が有効になっています。\nこの状態ではアニメーションの終端に近いほど再生した場合の補正による差異が大きくなります。"),
                new GUIContent("AnimationClip設定のRoot Transform Position (Y)がFeetになっています。\nしかし、FootIKのキーフレームが作成されていません。\n作成する必要があります。"),
                new GUIContent("AnimationClip設定のBased UponがOriginalではない設定になっています。\nこの状態では再生した場合に編集時と異なる挙動をする動作になります。\n意図的に設定したものでない場合にはOriginalに変更して下さい。"),
                new GUIContent("AnimationClip設定のCycle Offsetが0ではありません。\nこの状態ではAnimator IKが正常な結果を取得できません。\n0にセットして下さい。"),
                new GUIContent("AnimationClip設定のMirrorが有効になっています。\nこの状態では正しく編集することが出来ません。\nMirrorを無効にして下さい。"),
                new GUIContent("TimelineとAnimationClipのフレームレートが一致していません。\n問題が発生するため同一に設定してください。\n Animation Clip {0} != Timeline {1}"),
                //Tools
                new GUIContent("様々なアニメーション関係の処理ツールです。"),
                new GUIContent("指定範囲のキーフレームとイベントをコピーします。"),
                new GUIContent("AnimationClipをトリミングします。"),
                new GUIContent("新しいAnimationClipを作成します。\n現在のAnimationClip設定やAnimatorStateの状態もコピーされます。"),
                new GUIContent("AnimatorStateのFoot IKが有効な場合に使用される情報を編集します。\nこれはHumanoidのみで有効です。"),
                new GUIContent("AnimationCurveの値からAnimatorControllerParameterの値へリレーする情報を編集します。\nFloatパラメータタイプのみが使用できます。\nHumanoidとGeneric Motionで有効です。"),
                new GUIContent("全てのTransform Rotation Curvesの補間モードを変更します。\nQuaternionモードを推奨します。"),
                new GUIContent("AnimationClip.EnsureQuaternionContinuityを実行します。\nQuaternionカーブが編集されます。"),
                new GUIContent("AnimationClipから選択した情報のカーブを削除します。"),
                new GUIContent("エラーやアサートが発生する状態を修正します。\n通常は実行する必要はありません。"),
                new GUIContent("Colladaファイルを出力します。\nColladaファイルは外部ツールでFBXファイルに変換することが可能です。"),
                new GUIContent("Humanoidタイプのみで有効です。"),
                new GUIContent("この変換は逆回転などの問題を引き起こす可能性があります。"),
                //Settings
                new GUIContent("設定を変更します。\n設定はEditorPrefsに保存されます。"),
                //Help
                new GUIContent("その他の情報です。\nショートカットキーはSceneViewかEditorWindowにフォーカスしている場合にのみ機能します。"),
                //Preivew
                new GUIContent("プレビューを表示します。\nドラッグアンドドロップで他のモデルの表示に切り替えることが出来ます。"),
                #endregion
                #region ControlGUI
                //AnimatorIK
                new GUIContent("Animatorの機能を使用したIKの機能を設定します。"),
                new GUIContent("All", "すべての対象を変更します。"),
                //OriginalIK
                new GUIContent("Very Animation独自のIKを設定します。\n追加するにはIKの先端となるボーンを選択し+ボタンを押してください。"),
                new GUIContent("All", "すべての対象を変更します。"),
                new GUIContent("Basic", "基本的なCCD-IKタイプです。"),
                new GUIContent("Limb", "手足に使用するタイプです。"),
                new GUIContent("LockAt", "ターゲット方向に向かせるために使用します。"),
                new GUIContent("Reset Rotations", "有効にすると計算前に初期状態にリセットします。\nこれによりねじれは発生しませんが精度が低下します。"),
                new GUIContent("Direction", "曲がる方向の設定です。"),
                new GUIContent("GameObjectの指定にエラーがあります。\n正しいGameObjectと階層で指定して下さい。"),
                //Humanoid
                new GUIContent("Humanoidの全体構造です。\nAltキーを押しながら選択すると全ての子孫を選択することが出来ます。"),
                //Selection
                new GUIContent("ボーンの選択状態を名前をつけて保存して読み込むことができます。\n追加するにはボーンを選択し+ボタンを押してください。"),
                //Hierarchy
                new GUIContent("Altキーを押しながら変更することで全ての子孫のチェックを変更することが出来ます。\nAltキーを押しながら選択すると全ての子孫を選択することが出来ます。"),
                new GUIContent("Humanoid Name", "HierarchyでMecanim Humanoid制御のボーンをHumanoid名で表示します。"),
                new GUIContent("All", "全てのボーンを切り替えます。"),
                new GUIContent("Weight", "頂点にウェイトを持つボーンのみを切り替えます。"),
                new GUIContent("Body", "Mecanim HumanoidのBody部分のみを切り替えます。"),
                new GUIContent("Head", "Mecanim HumanoidのHead部分のみを切り替えます。"),
                new GUIContent("Left Hand", "Mecanim HumanoidのLeft Hand部分のみを切り替えます。"),
                new GUIContent("Right Hand", "Mecanim HumanoidのRight Hand部分のみを切り替えます。"),
                new GUIContent("Renderer", "Renderer Componentを持つボーンのみを切り替えます。"),
                new GUIContent("Renderer Parent", "Renderer Componentを持つボーンの親のみを切り替えます。"),
                #endregion
                #region EditorGUI
                //Options
                new GUIContent("Clamp", "Muscleを-1から1の範囲に制限します。"),
                new GUIContent("Foot IK", "Foot IK情報を自動的に更新します。\nTimelineリンク状態では強制的に有効になります。"),
                new GUIContent("Mirror", "左右のミラーでの編集を行います。"),
                //RootCorrection
                new GUIContent("Root Correction", "Muscleなどを変更したときに全体の位置関係が変動しないように自動的にRootを補正する機能です。"),
                new GUIContent(VeryAnimation.RootCorrectionMode.Disable.ToString(), "補正を行いません。"),
                new GUIContent(VeryAnimation.RootCorrectionMode.Single.ToString(), "変更されたフレームのみ補正を行います。"),
                new GUIContent(VeryAnimation.RootCorrectionMode.Full.ToString(), "変更されたフレーム前後の影響があるすべてのフレームを補正します。"),
                //Pose
                new GUIContent("全体に影響する操作を行います。"),
                new GUIContent("Reset", "Mecanim Humanoidのデフォルトポーズにリセットします。"),
                new GUIContent("Start", "編集を開始した初期ポーズにリセットします。"),
                new GUIContent("Bind", "Meshのバインドポーズ情報からポーズを変更します。\nこれは主にTポーズのような初期状態のポーズです。"),
                new GUIContent("Prefab", "Prefabのポーズに変更します。"),
                new GUIContent("Mirror", "現在ポーズを左右反転したポーズに変更します。"),
                new GUIContent("Template", "ロードしたポーズテンプレートファイルのポーズに変更します。"),
                new GUIContent("Save as", "現在のポーズをポーズテンプレートファイルに名前をつけて保存します。"),
                //BlendPose
                new GUIContent("2つのポーズをブレンドします。\n全体ではなく表情や指など部分的に適用することで一度作成して保存したものを再利用できます。"),
                new GUIContent("左右にポーズをセットして下さい。"),
                new GUIContent(BlendPoseTree.EditMode.Tree.ToString(), "操作されたノード以下のみに適用されます。"),
                new GUIContent(BlendPoseTree.EditMode.Selection.ToString(), "選択中のボーンのみに適用されます。"),
                //MuscleGroup
                new GUIContent("Mecanim HumanoidのMuscleをまとめて操作します。\n操作されたノード以下のみに適用されます。"),
                //BlendShape
                new GUIContent("すべてのMeshのBlendShapeを操作します。"),
                //Selection
                new GUIContent("選択中オブジェクトの情報が表示されます。"),
                new GUIContent("編集対象がありません"),
                new GUIContent("Humanoidと競合するため編集できません。"),
                new GUIContent("何も選択されていません。"),
                new GUIContent("Mirror", "左右ミラー\n{0}"),
                new GUIContent("Update", "IKを計算し更新します。"),
                new GUIContent("Reset", "オブジェクトに合わせてIKを修正します。"),
                new GUIContent(AnimatorIKCore.AnimatorIKData.SpaceType.Global.ToString(), "グローバル空間です。"),
                new GUIContent(AnimatorIKCore.AnimatorIKData.SpaceType.Local.ToString(), "IKルートの親オブジェクトのローカル空間です。"),
                new GUIContent(AnimatorIKCore.AnimatorIKData.SpaceType.Parent.ToString(), "指定した親オブジェクトのローカル空間です。"),
                new GUIContent(OriginalIKCore.OriginalIKData.SpaceType.Global.ToString(), "グローバル空間です。"),
                new GUIContent(OriginalIKCore.OriginalIKData.SpaceType.Local.ToString(), "IKルートの親オブジェクトのローカル空間です。"),
                new GUIContent(OriginalIKCore.OriginalIKData.SpaceType.Parent.ToString(), "指定した親オブジェクトのローカル空間です。"),
                new GUIContent("Fixed", "有効にするとIKの情報は更新されなくなります。"),
                #endregion
                #region ToolsGUI
                new GUIContent("VeryAnimation編集中には動作しません。"),
                new GUIContent("選択したオブジェクトの子孫すべてをPrefabのTransformにリセットします。"),
                new GUIContent("ポーズテンプレートファイルからTransformをセットします。"),
                new GUIContent("プロジェクトのすべてのVeryAnimationSaveSettings Componentを削除します。\nプロジェクトのバックアップを作成してから実行して下さい。"),
#if UNITY_2017_1_OR_NEWER
                new GUIContent("プロジェクトの全てのAnimatorController, AnimatorOverrideController, Animation, TimelineのAnimationClip参照を置き換えます。\nプロジェクトのバックアップを作成してから実行して下さい。"),
#else
                new GUIContent("プロジェクトの全てのAnimatorController, AnimatorOverrideController, AnimationのAnimationClip参照を置き換えます。"),
#endif
	            #endregion
                #region Dialog
                new GUIContent("Assetsフォルダ内に保存する必要があります", "プロジェクトのAssetsフォルダ内を指定してください。"),
                new GUIContent("警告", "ソースAnimationClipのキーフレーム圧縮が無効になっています。\n複製する前に圧縮設定を変更して、キーフレームを減らすことをお勧めします。\nこの設定はModelImporterの'Anim. Compression'設定です。"),
                new GUIContent("設定のセーブの削除", "プロジェクト内のすべての'VeryAnimationSaveSettings'コンポーネントを削除します。\nバックアップを作成してから実行してください。\nよろしいですか？"),
                new GUIContent("参照の置換", "プロジェクト内のすべての参照を置き換えます。\nバックアップを作成してから実行してください。\nよろしいですか？"),
                #endregion
                #region Log
                new GUIContent("<color=blue>[Very Animation]</color>RootQをRotation Curve InterpolationがEuler Anglesであるため更新ができません。\n'Tools/Rotation Curve Interpolation'を使用してQuaternionに変更してください。"),
                new GUIContent("<color=blue>[Very Animation]</color>このAnimationClipは読み取り専用です。 '{0}'"),
                new GUIContent("<color=blue>[Very Animation]</color>このボーンのTransformの操作はHumanoidの管理と競合するため操作できません。 '{0}'"),
                new GUIContent("<color=blue>[Very Animation]</color>このボーンのTransformの操作はルートモーションと競合するため操作できません。 '{0}'"),
                new GUIContent("<color=blue>[Very Animation]</color>このボーンのTransformの操作はルートモーションと競合するため操作できません。 RootTを操作してください。 '{0}'"),
                new GUIContent("<color=blue>[Very Animation]</color>指定した名前は使用できないためパラメータ名が変更されました。 '{0}' -> '{1}'"),
                new GUIContent("<color=blue>[Very Animation]</color>前後のキーフレームには180度以上の違いがあったため、正しい動きのためにキーが追加されました。\nAnimationCurveにはわずかな変更があります。 '{0} - {1}'"),
                new GUIContent("<color=blue>[Very Animation]</color>少なくとも2つのキーフレームが必要です。 そうでない場合、Assertが発生します。\n修正のためにキーフレームが追加されました。 '{0} - {1}'"),
                new GUIContent("<color=blue>[Very Animation]</color>SkinnedMeshRendererのbonesとMeshのbindposesの数が一致しません。 '{0}' != '{1}'"),
                new GUIContent("<color=blue>[Very Animation]</color>SkinnedMeshRenderer.bonesの中にnullがあります。\nこの状態ではBindPoseを取得できません。\nname = {0}, index = {1}"),
                new GUIContent("<color=blue>[Very Animation]</color>不明なエラーによってBindPoseの取得に失敗しました。\nSkinnedMeshRendererの状態が正常ではないと推測します。"),
                new GUIContent("<color=blue>[Very Animation]</color>Poseのロードに失敗しました。 '{0}'"),
                new GUIContent("<color=blue>[Very Animation]</color>同じ階層に同じ名前のGameObjectが複数存在するため、AnimationCurveのターゲットが曖昧です。\nGameObjectの名前を変更してください。 ({0})"),
                new GUIContent("<color=blue>[Very Animation]</color>MeshのBlendShapeに同じ名前が複数存在するため対象が曖昧です。\nBlendShapeは重複しない状態になるようにMeshを変更してください。 (Mesh - {0}, Name - {1})"),
	            #endregion
            },
            #endregion
        };

        public static string GetText(Help help)
        {
            return LanguageHelps[(int)languageType][(int)help].text;
        }
        public static string GetTooltip(Help help)
        {
            return LanguageHelps[(int)languageType][(int)help].tooltip;
        }
        public static GUIContent GetContent(Help help)
        {
            return LanguageHelps[(int)languageType][(int)help];
        }
        public static GUIContent GetContentFormat(Help help, params object[] tooltipArgs)
        {
            var content = new GUIContent(LanguageHelps[(int)languageType][(int)help]);
            content.tooltip = String.Format(content.tooltip, tooltipArgs);
            return content;
        }

        public static Action OnLanguageChanged;
    }
}
