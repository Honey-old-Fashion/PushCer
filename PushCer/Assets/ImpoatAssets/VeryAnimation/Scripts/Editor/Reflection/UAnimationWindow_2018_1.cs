using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Reflection;
#if UNITY_2018_1_OR_NEWER
using UnityEngine.Playables;
using UnityEngine.Timeline;
#endif

namespace VeryAnimation
{
#if UNITY_2018_1_OR_NEWER
    public class UAnimationWindow_2018_1 : UAnimationWindow_2017_1    //2018.1 or later
    {
        protected class UAnimEditor_2018_1 : UAnimEditor
        {
            public UAnimEditor_2018_1(Assembly asmUnityEditor) : base(asmUnityEditor)
            {
                var animEditorType = asmUnityEditor.GetType("UnityEditor.AnimEditor");
                Assert.IsNotNull(pi_selectedItem = animEditorType.GetProperty("selection"));
            }
        }

        protected UAnimEditor_2018_1 uAnimEditor_2018_1;
        protected UEditorGUIUtility_2018_1 uEditorGUIUtility_2018_1;

        protected Func<object, object> dg_get_m_LockTracker;

        public UAnimationWindow_2018_1()
        {
            var asmUnityEditor = Assembly.LoadFrom(InternalEditorUtility.GetEditorAssemblyPath());
            var animationWindowType = asmUnityEditor.GetType("UnityEditor.AnimationWindow");
            uAnimEditor = uAnimEditor_2018_1 = new UAnimEditor_2018_1(asmUnityEditor);
            uEditorGUIUtility_2018_1 = new UEditorGUIUtility_2018_1();
            Assert.IsNotNull(dg_get_m_LockTracker = EditorCommon.CreateGetFieldDelegate<object>(animationWindowType.GetField("m_LockTracker", BindingFlags.NonPublic | BindingFlags.Instance)));
        }

        public override bool GetLock(EditorWindow aw)
        {
            if (aw == null) return false;
            return uEditorGUIUtility_2018_1.uEditorLockTracker.GetLock(dg_get_m_LockTracker(aw));
        }
        public override void SetLock(EditorWindow aw, bool flag)
        {
            if (aw == null) return ;
            uEditorGUIUtility_2018_1.uEditorLockTracker.SetLock(dg_get_m_LockTracker(aw), flag);
        }
    }
#endif
}
