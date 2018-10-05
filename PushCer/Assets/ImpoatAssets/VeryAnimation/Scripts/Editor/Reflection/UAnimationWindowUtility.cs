using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace VeryAnimation
{
    public class UAnimationWindowUtility
    {
        private MethodInfo mi_IsNodeLeftOverCurve;
        private MethodInfo mi_CreateNewClipAtPath;

        public UAnimationWindowUtility()
        {
            var asmUnityEditor = Assembly.LoadFrom(InternalEditorUtility.GetEditorAssemblyPath());
            var animationWindowUtilityType = asmUnityEditor.GetType("UnityEditorInternal.AnimationWindowUtility");
            Assert.IsNotNull(mi_IsNodeLeftOverCurve = animationWindowUtilityType.GetMethod("IsNodeLeftOverCurve", BindingFlags.Public | BindingFlags.Static));
            Assert.IsNotNull(mi_CreateNewClipAtPath = animationWindowUtilityType.GetMethod("CreateNewClipAtPath", BindingFlags.NonPublic | BindingFlags.Static));
        }

        public bool IsNodeLeftOverCurve(object node)
        {
            return (bool)mi_IsNodeLeftOverCurve.Invoke(null, new object[] { node });
        }

        public AnimationClip CreateNewClipAtPath(string clipPath)
        {
            return mi_CreateNewClipAtPath.Invoke(null, new object[] { clipPath }) as AnimationClip;
        }
    }
}
