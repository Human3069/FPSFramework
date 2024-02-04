using Demo.Scripts.Runtime;
using Kinemation.FPSFramework.Runtime.Attributes;
using System.Collections.Generic;
using System.Reflection;
using Unity.Services.Analytics.Platform;
using UnityEditor;
using UnityEngine;

namespace FPS_Framework
{
    [CustomEditor(typeof(FPSControllerEx))]
    public class FPSControllerExEditor : Demo.Scripts.Editor.TabAttribute
    {
        protected SerializedProperty m_Script;

        protected override void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");
            
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;

            EditorGUILayout.Space(10f);

            base.OnInspectorGUI();
        }
    }
}
