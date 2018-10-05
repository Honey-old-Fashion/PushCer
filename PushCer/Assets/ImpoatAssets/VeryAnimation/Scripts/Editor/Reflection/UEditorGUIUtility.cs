using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Reflection;

namespace VeryAnimation
{
    public class UEditorGUIUtility
    {
        private Func<object, int> dg_get_s_LastControlID;
        private Func<string, Texture2D> dg_LoadIcon;

        public UEditorGUIUtility()
        {
            var asmUnityEditor = Assembly.LoadFrom(InternalEditorUtility.GetEditorAssemblyPath());
            var editorGUIUtilityType = asmUnityEditor.GetType("UnityEditor.EditorGUIUtility");
            Assert.IsNotNull(dg_get_s_LastControlID = EditorCommon.CreateGetFieldDelegate<int>(editorGUIUtilityType.GetField("s_LastControlID", BindingFlags.NonPublic | BindingFlags.Static)));
            Assert.IsNotNull(dg_LoadIcon = (Func<string, Texture2D>)Delegate.CreateDelegate(typeof(Func<string, Texture2D>), null, editorGUIUtilityType.GetMethod("LoadIcon", BindingFlags.NonPublic | BindingFlags.Static)));
        }

        public int GetLastControlID()
        {
            return dg_get_s_LastControlID(null);
        }

        public Texture2D LoadIcon(string name)
        {
            return dg_LoadIcon(name);
        }
    }
}
