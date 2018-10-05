using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Reflection;

namespace VeryAnimation
{
    public class UEditorWindow
    {
        private Func<bool> dg_get_hasFocus;
        private Func<int> dg_GetNumTabs;

        private FieldInfo fi_m_Parent;

        public class UDockArea
        {
            private Type dockAreaType;

            private Func<int> dg_get_selected;
            private Action<int> dg_set_selected;

            public UDockArea()
            {
                var asmUnityEditor = Assembly.LoadFrom(InternalEditorUtility.GetEditorAssemblyPath());
                dockAreaType = asmUnityEditor.GetType("UnityEditor.DockArea");
            }

            public int GetSelected(UnityEngine.Object dockArea)
            {
                if (dockArea == null) return -1;
                if (dg_get_selected == null || dg_get_selected.Target != (object)dockArea)
                    dg_get_selected = (Func<int>)Delegate.CreateDelegate(typeof(Func<int>), dockArea, dockAreaType.GetProperty("selected", BindingFlags.Public | BindingFlags.Instance).GetGetMethod());
                return dg_get_selected();
            }
            public void SetSelected(UnityEngine.Object dockArea, int selected)
            {
                if (dockArea == null) return;
                if (dg_set_selected == null || dg_set_selected.Target != (object)dockArea)
                    dg_set_selected = (Action<int>)Delegate.CreateDelegate(typeof(Action<int>), dockArea, dockAreaType.GetProperty("selected", BindingFlags.Public | BindingFlags.Instance).GetSetMethod());
                dg_set_selected(selected);
            }
        }

        private UDockArea uDockArea;

        public UEditorWindow()
        {
            var asmUnityEditor = Assembly.LoadFrom(InternalEditorUtility.GetEditorAssemblyPath());
            var editorWindowType = asmUnityEditor.GetType("UnityEditor.EditorWindow");

            Assert.IsNotNull(fi_m_Parent = editorWindowType.GetField("m_Parent", BindingFlags.NonPublic | BindingFlags.Instance));

            uDockArea = new UDockArea();
        }

        public bool HasFocus(EditorWindow w)
        {
            if (w == null) return false;
            if (dg_get_hasFocus == null || dg_get_hasFocus.Target != (object)w)
                dg_get_hasFocus = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), w, w.GetType().GetProperty("hasFocus", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true));
            return dg_get_hasFocus();
        }

        public int GetNumTabs(EditorWindow w)
        {
            if (w == null) return 0;
            if (dg_GetNumTabs == null || dg_GetNumTabs.Target != (object)w)
                dg_GetNumTabs = (Func<int>)Delegate.CreateDelegate(typeof(Func<int>), w, w.GetType().GetMethod("GetNumTabs", BindingFlags.NonPublic | BindingFlags.Instance));
            return dg_GetNumTabs();
        }

        public int GetSelectedTab(EditorWindow w)
        {
            return uDockArea.GetSelected(fi_m_Parent.GetValue(w) as UnityEngine.Object);
        }
        public void SetSelectedTab(EditorWindow w, int selected)
        {
            uDockArea.SetSelected(fi_m_Parent.GetValue(w) as UnityEngine.Object, selected);
        }
    }
}
