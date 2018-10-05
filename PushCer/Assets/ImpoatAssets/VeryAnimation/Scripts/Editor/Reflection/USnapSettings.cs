using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Reflection;

namespace VeryAnimation
{
    public class USnapSettings
    {
        private Func<Vector3> dg_get_move;
        private Func<float> dg_get_scale;
        private Func<float> dg_get_rotation;

        public USnapSettings()
        {
            var asmUnityEditor = Assembly.LoadFrom(InternalEditorUtility.GetEditorAssemblyPath());
            var snapSettingsType = asmUnityEditor.GetType("UnityEditor.SnapSettings");
            Assert.IsNotNull(dg_get_move = (Func<Vector3>)Delegate.CreateDelegate(typeof(Func<Vector3>), null, snapSettingsType.GetProperty("move", BindingFlags.Public | BindingFlags.Static).GetGetMethod()));
            Assert.IsNotNull(dg_get_scale = (Func<float>)Delegate.CreateDelegate(typeof(Func<float>), null, snapSettingsType.GetProperty("scale", BindingFlags.Public | BindingFlags.Static).GetGetMethod()));
            Assert.IsNotNull(dg_get_rotation = (Func<float>)Delegate.CreateDelegate(typeof(Func<float>), null, snapSettingsType.GetProperty("rotation", BindingFlags.Public | BindingFlags.Static).GetGetMethod()));
        }

        public Vector3 move { get { return dg_get_move(); } }
        public float scale { get { return dg_get_scale(); } }
        public float rotation { get { return dg_get_rotation(); }  }
    }
}
