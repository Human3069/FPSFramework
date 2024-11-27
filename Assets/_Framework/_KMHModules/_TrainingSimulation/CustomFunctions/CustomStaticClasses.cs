using UnityEditor;
using UnityEngine;

namespace VTSFramework.TSModule
{
    public static class CustomStaticClasses
    {
        public static string ToInMissionText(this string originalText)
        {
            if (string.IsNullOrEmpty(originalText) == true)
            {
                return string.Empty;
            }
            else
            {
                return originalText.ToLower().Replace(" ", "").Replace("\n", "").Trim();
            }
        }
    }

#if UNITY_EDITOR
    public static class EditorGUILayoutEx
    {
        public static void PropertyFieldWithDisabled(SerializedProperty property)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(property);
            EditorGUI.EndDisabledGroup();
        }
    }
#endif

    public static class CameraEx
    {
        private static bool IsInitialized = false;

        private static Camera _display_0_Camera;
        public static Camera Display_0_Camera
        {
            get
            {
                if (IsInitialized == false)
                {
                    Initialize();
                }

                return _display_0_Camera;
            }
            private set
            {
                _display_0_Camera = value;
            }
        }

        private static Camera _display_1_Camera;
        public static Camera Display_1_Camera
        {
            get
            {
                if (IsInitialized == false)
                {
                    Initialize();
                }

                return _display_1_Camera;
            }
            private set
            {
                _display_1_Camera = value;
            }
        }

        public static void Initialize()
        {
            Camera[] allCameras = Camera.allCameras;
            foreach (Camera camera in allCameras)
            {
                if (camera.targetDisplay == 0)
                {
                    Display_0_Camera = camera;
                }
                else if (camera.targetDisplay == 1)
                {
                    Display_1_Camera = camera;
                }
            }

            IsInitialized = true;
        }
    }
}