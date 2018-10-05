-----------------------------------------------------
	Very Animation
	Copyright (c) 2017 AloneSoft
	http://alonesoft.sakura.ne.jp/
	mail: support@alonesoft.sakura.ne.jp
	twitter: @AlSoSupport
-----------------------------------------------------

Thank you for purchasing "Very Animation".


[How to Update] 
Remove old VeryAnimation folder
Import VeryAnimation


[Documentation]
Assets/VeryAnimation/Documents


[Demo]
Assets/VeryAnimation/Demo


[Support]
mail: support@alonesoft.sakura.ne.jp
twitter: @AlSoSupport


[Update History]
Version 1.1.1p4
- FIX : Fixed an issue that may not work due to an error when multiple identical names exist in BlendShape

Version 1.1.1p3
- FIX : Fixed an issue that does not work with errors when there are multiple GameObjects with the same name in the same hierarchy

Version 1.1.1p2
- FIX : Responding to problems that do not work due to BindPose acquisition processing error
- FIX : IK : Correspond so that Bone is selected when list is selected in IK invalid state
- FIX : Correction of animation curve update processing
- FIX : Changed so that Animation Window keyframe selection is not canceled by animation curve update

Version 1.1.1p1
- ADD : Tools/Clearnup : Add BlendShape
- FIX : Tools/Clearnup : Change Eye, Jaw and Toe individually
- FIX : Mirror : Fixed an issue that sometimes does not work due to operation with Animation Window
- FIX : MuscleGroup/Part : Independence from Head with Eye and Jaw as Face
- FIX : Unity2018.2 : Fixed problem that "Open Animation Window" button does not work

Version 1.1.1
- ADD : Unity2018.2 support
- ADD : Muscle Group : Change Foldout to Bone selection button of target Node
- ADD : Blend Shape : Change Foldout to Bone selection button of target Node
- ADD : Multiple selection : Added correction referring to maximum parent-child relationship hierarchy number for handle operation
- ADD : Animation Window : Correspondence between Clamp, Mirror and Root Correction operation when operated on Animation Window side
- ADD : Animation Window : Correspondence of action to select Keyframe at current time when Animation Curve selection is synchronized
- FIX : Blend Shape : Copy and Paste problem
- FIX : Animator IK : Correction of calculation
- FIX : Original IK : Correction of calculation
- FIX : Root Correction : Correction for interpolation after the last frame
- FIX : speedup

Version 1.1.0p1
- FIX : MenuItem : Delete priority specification
- FIX : Fixed problem by UnityEditor language change
- FIX : RootCorrection : Update method update
- FIX : FootIK : Changes to force enabling in Timeline link state
- FIX : FootIK : Update method update
- FIX : Profiler relationship was left, so delete
- FIX : Unity2018.2 : Timeline

Version 1.1.0
- ADD : Unity2018.1 support
- ADD : Language select : Japanese
- ADD : Humanoid Root correction : Change from Lock button to Disable, Single and Full button. Full function addition
- FIX : Action correction when edited by AnimationWindow
- FIX : OriginalIK : Basic : Change the initial Weight of the tip to 0.5
- FIX : Selection Set : Set default name from active object
- FIX : Hierarchy : Change of Icon acquisition method
- FIX : Correction of reverse rotation correction processing
- FIX : FootIK : Update processing change
- FIX : BlendShape : Correspondence of the part where mirror correspondence was insufficient. Changed to be the value at the start of editing instead of 0 by reset
- FIX : Unity2018.2 : Error

Version 1.0.9
- ADD : Multiple selection behavior in PivotMode.Center
- ADD : Added warning when 'Based Upon' setting is not Original
- FIX : Improvement of multiple selection operation
- FIX : Action correction when edited by AnimationWindow
- FIX : speedup

Version 1.0.8
- ADD : Blend Pose
- FIX : Editing while paused : Fixed a bug that did not get correct animation and time on shortcut launch
- FIX : Editing while paused : Fixed a bug that sometimes did not return to the original position after editing
- FIX : Exporter : Texture output which is not Texture2D also supports texture output, error check added
- FIX : AnimatorIK : Head : Fixed bug where unintended initialization occurred
- FIX : Select Bone : Frontmost polygon may not be selected Bug fix
- FIX : speedup

Version 1.0.7p1
- FIX : AnimationWindow automatic lock at editing : Changed to invalid in Timeline
- FIX : Selection Set : Null error

Version 1.0.7
- ADD : Selection Set
- ADD : AnimationWindow automatic lock at editing
- ADD : Error display for forced termination of edit mode is added
- FIX : Tools : Clearnup : Correspondence of BlendShape information
- FIX : ControlWindow : Correction of Humanoid selection process

Version 1.0.6
- ADD : Edit BlendShape
- FIX : AnimatorIK : Head Swivel correspondence
- FIX : Fixed bug where Free Rotate Handle was not running
- FIX : EditorWindow : Add ToolBar, move previous element to Options
- FIX : MuscleGroup : Fixed a bug that was added in a situation where it is not necessary to add a curve with Reset
- FIX : IKTarget : Correction of range selection defect when space is not Global
- FIX : Humanoid : Fixed bug where Global rotation of Head got wrong when Neck does not exist
- FIX : Other bug fixes, speedup

Version 1.0.5
- ADD : Tools : Create New Clip
- ADD : Startup shortcut key correspondence
- FIX : Action correction when Animator is moved to a different hierarchy from Avatar creation time
- FIX : Correction of behavior that Glocal rotation operation does not reflect correctly
- FIX : Fixed a bug when IKTarget's Mirror reflected and each other's space was different
- FIX : DaeExporter : Correcting errors in the material without the '_Color' property

Version 1.0.4
- ADD : IK : Global, Local and Parent space switching supported
- ADD : IK : Automatic reflection switching of Rotation
- FIX : Fixed a bug where Mirror's curve change is not reflected in AnimationWindow
- FIX : Other bug fixes, speedup
- FIX : Documentation : Add a description
- FIX : Unity 2018.1 : Fix error

Version 1.0.3
- ADD : Original IK : Limb IK
- FIX : Original IK : GUI
- FIX : Hotkeys : Editor Window focus state
- FIX : Hotkeys : Change Keypad Plus and Minus

Version 1.0.2p2
- FIX : Timeline : Fixed problem that Dummy Object disappeared
- FIX : Timeline : Fix to Active change

Version 1.0.2p1
- ADD : Timeline : Dummy Timeline Position Type
- FIX : Timeline : Root : Reset All

Version 1.0.2
- ADD : Original IK
- ADD : Save Toolbar Valid State
- ADD : IK range selection
- ADD : Hierarchy : Selected Object Auto Expand Setting
- FIX : Hotkeys : Scene View focus state only
- FIX : Animator IK
- FIX : Settings : IK Default
- FIX : Reverse rotation correction processing

Version 1.0.1
- ADD : Legacy (Animation Component) support
- FIX : VA Tools : "Remove Save Settings" and "Replace Reference"

Version 1.0.0p1
- ADD : Generic Mirror condition setting, Ignore setting
- FIX : Save Settings

Version 1.0.0
- first release

