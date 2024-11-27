using UnityEngine;
using UnityEditor.UI;
using UnityEditor;
using geniikw.DataRenderer2D.Editors;

namespace _KMH_Framework
{
    [CustomEditor(typeof(UI_Line))]
    public class UI_LineEditor : ImageEditor
    {
        internal PointHandler _pointHandler;
        private MonoBehaviour _owner;

        // private SerializedProperty m_Script;
        private SerializedProperty _SplineData;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            _owner = target as MonoBehaviour;
            _SplineData = serializedObject.FindProperty("line");

            _pointHandler = new PointHandler(_owner, serializedObject);
        }

        private void OnSceneGUI()
        {
            _pointHandler.OnSceneGUI();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(m_Script);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(_SplineData,true);

            base.OnInspectorGUI();
            serializedObject.ApplyModifiedProperties();
        }
    }
}