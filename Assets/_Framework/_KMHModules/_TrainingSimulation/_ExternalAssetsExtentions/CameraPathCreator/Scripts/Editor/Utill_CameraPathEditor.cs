using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace VTSFramework.TSModule
{
    public enum CPC_EManipulationModes
    {
        Free,
        SelectAndTransform
    }

    public enum CPC_ENewWaypointMode
    {
        SceneCamera,
        LastWaypoint,
        WaypointIndex,
        WorldCenter
    }

    [CustomEditor(typeof(Utill_CameraPath))]
    public class Utill_CameraPathEditor : Editor
    {
        private SerializedProperty m_Script;

        private Utill_CameraPath cameraPath;
        private ReorderableList pointReorderableList;

        //Editor variables
        private bool visualFoldout;
        private bool manipulationFoldout;
        private bool showRawValues;
        private float time;
        private CPC_EManipulationModes cameraTranslateMode;
        private CPC_EManipulationModes cameraRotationMode;
        private CPC_EManipulationModes handlePositionMode;
        private CPC_ENewWaypointMode waypointMode;
        private int waypointIndex = 1;
        private CPC_ECurveType allCurveType = CPC_ECurveType.Custom;
        private AnimationCurve allAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        //GUIContents
        private GUIContent addPointContent = new GUIContent("Add Point", "Adds a waypoint at the scene view camera's position/rotation");
        private GUIContent testButtonContent = new GUIContent("Test", "Only available in play mode");
        private GUIContent pauseButtonContent = new GUIContent("Pause", "Paused Camera at current Position");
        private GUIContent continueButtonContent = new GUIContent("Continue", "Continues Path at current position");
        private GUIContent stopButtonContent = new GUIContent("Stop", "Stops the playback");
        private GUIContent deletePointContent = new GUIContent("X", "Deletes this waypoint");
        private GUIContent gotoPointContent = new GUIContent("Goto", "Teleports the scene camera to the specified waypoint");
        private GUIContent relocateContent = new GUIContent("Relocate", "Relocates the specified camera to the current view camera's position/rotation");
        private GUIContent alwaysShowContent = new GUIContent("Always show", "When true, shows the curve even when the GameObject is not selected - \"Inactive cath color\" will be used as path color instead");
        private GUIContent chainedContent = new GUIContent("o───o", "Toggles if the handles of the specified waypoint should be chained (mirrored) or not");
        private GUIContent unchainedContent = new GUIContent("o─x─o", "Toggles if the handles of the specified waypoint should be chained (mirrored) or not");
        private GUIContent replaceAllPositionContent = new GUIContent("Replace all position lerps", "Replaces curve types (and curves when set to \"Custom\") of all the waypoint position lerp types with the specified values");
        private GUIContent replaceAllRotationContent = new GUIContent("Replace all rotation lerps", "Replaces curve types (and curves when set to \"Custom\") of all the waypoint rotation lerp types with the specified values");

        //Serialized Properties
        private SerializedObject serializedObjectTarget;
        private SerializedProperty targetTransformProperty;
        private SerializedProperty lookAtTargetProperty;
        private SerializedProperty lookAtTargetTransformProperty;
        private SerializedProperty visualPathProperty;
        private SerializedProperty visualInactivePathProperty;
        private SerializedProperty visualFrustumProperty;
        private SerializedProperty visualHandleProperty;
        private SerializedProperty alwaysShowProperty;

        private int selectedIndex = -1;

        private float currentTime;
        private float previousTime;

        private bool hasScrollBar = false;

        protected virtual void OnEnable()
        {
            EditorApplication.update += Update;

            m_Script = serializedObject.FindProperty("m_Script");
            cameraPath = (Utill_CameraPath)target;

            if (cameraPath == null)
            {
                return;
            }

            SetupEditorVariables();
            GetVariableProperties();
            SetupReorderableList();
        }

        protected virtual void OnDisable()
        {
            EditorApplication.update -= Update;
        }

        protected virtual void Update()
        {
            if (cameraPath == null)
            {
                return;
            }

            currentTime = cameraPath.GetCurrentWayPoint() + cameraPath.GetCurrentTimeInWaypoint();
            if (Math.Abs(currentTime - previousTime) > 0.0001f)
            {
                Repaint();
                previousTime = currentTime;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObjectTarget.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(m_Script);
            EditorGUI.EndDisabledGroup();

            DrawPlaybackWindow();
            Rect scale = GUILayoutUtility.GetLastRect();
            hasScrollBar = (Screen.width - scale.width <= 12);
            GUILayout.Space(5);
            GUILayout.Box("", GUILayout.Width(Screen.width - 20), GUILayout.Height(3));
            GUILayout.Space(5);
            DrawBasicSettings();
            GUILayout.Space(5);
            GUILayout.Box("", GUILayout.Width(Screen.width - 20), GUILayout.Height(3));
            DrawVisualDropdown();
            GUILayout.Box("", GUILayout.Width(Screen.width - 20), GUILayout.Height(3));
            DrawManipulationDropdown();
            GUILayout.Box("", GUILayout.Width(Screen.width - 20), GUILayout.Height(3));
            GUILayout.Space(10);
            DrawWaypointList();
            GUILayout.Space(10);
            DrawRawValues();

            serializedObjectTarget.ApplyModifiedProperties();
        }

        protected virtual void OnSceneGUI()
        {
            if (cameraPath.pointList.Count >= 2)
            {
                for (int i = 0; i < cameraPath.pointList.Count; i++)
                {
                    DrawHandles(i);
                    Handles.color = Color.white;
                }
            }
        }

        protected virtual void SelectIndex(int index)
        {
            selectedIndex = index;
            pointReorderableList.index = index;
            Repaint();
        }

        protected virtual void SetupEditorVariables()
        {
            cameraTranslateMode = (CPC_EManipulationModes)PlayerPrefs.GetInt("CPC_cameraTranslateMode", 1);
            cameraRotationMode = (CPC_EManipulationModes)PlayerPrefs.GetInt("CPC_cameraRotationMode", 1);
            handlePositionMode = (CPC_EManipulationModes)PlayerPrefs.GetInt("CPC_handlePositionMode", 0);
            waypointMode = (CPC_ENewWaypointMode)PlayerPrefs.GetInt("CPC_waypointMode", 0);
            time = PlayerPrefs.GetFloat("CPC_time", 10);
        }

        protected virtual void GetVariableProperties()
        {
            serializedObjectTarget = new SerializedObject(cameraPath);
            targetTransformProperty = serializedObjectTarget.FindProperty("targetTransform");
            lookAtTargetProperty = serializedObjectTarget.FindProperty("lookAtTarget");
            lookAtTargetTransformProperty = serializedObjectTarget.FindProperty("target");
            visualPathProperty = serializedObjectTarget.FindProperty("visual.pathColor");
            visualInactivePathProperty = serializedObjectTarget.FindProperty("visual.inactivePathColor");
            visualFrustumProperty = serializedObjectTarget.FindProperty("visual.frustrumColor");
            visualHandleProperty = serializedObjectTarget.FindProperty("visual.handleColor");
            alwaysShowProperty = serializedObjectTarget.FindProperty("alwaysShow");
        }

        protected virtual void SetupReorderableList()
        {
            pointReorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("pointList"), true, true, false, false);

            pointReorderableList.elementHeight *= 2;
            pointReorderableList.drawElementCallback = (rect, index, active, focused) =>
            {
                float startRectY = rect.y;
                if (index > cameraPath.pointList.Count - 1) return;
                rect.height -= 2;
                float fullWidth = rect.width - 16 * (hasScrollBar ? 1 : 0);
                rect.width = 40;
                fullWidth -= 40;
                rect.height /= 2;
                GUI.Label(rect, "#" + (index + 1));
                rect.y += rect.height - 3;
                rect.x -= 14;
                rect.width += 12;
                if (GUI.Button(rect, cameraPath.pointList[index].chained ? chainedContent : unchainedContent))
                {
                    Undo.RecordObject(cameraPath, "Changed chain type");
                    cameraPath.pointList[index].chained = !cameraPath.pointList[index].chained;
                }
                rect.x += rect.width + 2;
                rect.y = startRectY;
                //Position
                rect.width = (fullWidth - 22) / 3 - 1;
                EditorGUI.BeginChangeCheck();
                CPC_ECurveType tempP = (CPC_ECurveType)EditorGUI.EnumPopup(rect, cameraPath.pointList[index].curveTypePosition);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(cameraPath, "Changed enum value");
                    cameraPath.pointList[index].curveTypePosition = tempP;
                }
                rect.y += pointReorderableList.elementHeight / 2 - 4;
                //rect.x += rect.width + 2;
                EditorGUI.BeginChangeCheck();
                GUI.enabled = cameraPath.pointList[index].curveTypePosition == CPC_ECurveType.Custom;
                AnimationCurve tempACP = EditorGUI.CurveField(rect, cameraPath.pointList[index].positionCurve);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(cameraPath, "Changed curve");
                    cameraPath.pointList[index].positionCurve = tempACP;
                }
                GUI.enabled = true;
                rect.x += rect.width + 2;
                rect.y = startRectY;

                //Rotation

                rect.width = (fullWidth - 22) / 3 - 1;
                EditorGUI.BeginChangeCheck();
                CPC_ECurveType temp = (CPC_ECurveType)EditorGUI.EnumPopup(rect, cameraPath.pointList[index].curveTypeRotation);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(cameraPath, "Changed enum value");
                    cameraPath.pointList[index].curveTypeRotation = temp;
                }
                rect.y += pointReorderableList.elementHeight / 2 - 4;
                //rect.height /= 2;
                //rect.x += rect.width + 2;
                EditorGUI.BeginChangeCheck();
                GUI.enabled = cameraPath.pointList[index].curveTypeRotation == CPC_ECurveType.Custom;
                AnimationCurve tempAC = EditorGUI.CurveField(rect, cameraPath.pointList[index].rotationCurve);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(cameraPath, "Changed curve");
                    cameraPath.pointList[index].rotationCurve = tempAC;
                }
                GUI.enabled = true;

                rect.y = startRectY;
                rect.height *= 2;
                rect.x += rect.width + 2;
                rect.width = (fullWidth - 22) / 3;
                rect.height = rect.height / 2 - 1;
                if (GUI.Button(rect, gotoPointContent))
                {
                    pointReorderableList.index = index;
                    selectedIndex = index;
                    SceneView.lastActiveSceneView.pivot = cameraPath.pointList[pointReorderableList.index].position;
                    SceneView.lastActiveSceneView.size = 3;
                    SceneView.lastActiveSceneView.Repaint();
                }
                rect.y += rect.height + 2;
                if (GUI.Button(rect, relocateContent))
                {
                    Undo.RecordObject(cameraPath, "Relocated waypoint");
                    pointReorderableList.index = index;
                    selectedIndex = index;
                    cameraPath.pointList[pointReorderableList.index].position = SceneView.lastActiveSceneView.camera.transform.position;
                    cameraPath.pointList[pointReorderableList.index].rotation = SceneView.lastActiveSceneView.camera.transform.rotation;
                    SceneView.lastActiveSceneView.Repaint();
                }
                rect.height = (rect.height + 1) * 2;
                rect.y = startRectY;
                rect.x += rect.width + 2;
                rect.width = 20;

                if (GUI.Button(rect, deletePointContent))
                {
                    Undo.RecordObject(cameraPath, "Deleted a waypoint");
                    cameraPath.pointList.Remove(cameraPath.pointList[index]);
                    SceneView.RepaintAll();
                }
            };

            pointReorderableList.drawHeaderCallback = rect =>
            {
                float fullWidth = rect.width;
                rect.width = 56;
                GUI.Label(rect, "Sum: " + cameraPath.pointList.Count);
                rect.x += rect.width;
                rect.width = (fullWidth - 78) / 3;
                GUI.Label(rect, "Position Lerp");
                rect.x += rect.width;
                GUI.Label(rect, "Rotation Lerp");
                //rect.x += rect.width*2;
                //GUI.Label(rect, "Del.");
            };

            pointReorderableList.onSelectCallback = l =>
            {
                selectedIndex = l.index;
                SceneView.RepaintAll();
            };
        }

        protected virtual void DrawPlaybackWindow()
        {
            GUI.enabled = Application.isPlaying;
            GUILayout.BeginVertical("Box");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(testButtonContent))
            {
                cameraPath.PlayPathAsync();
            }

            if (!cameraPath.IsPaused())
            {
                if (Application.isPlaying && !cameraPath.IsPlaying()) GUI.enabled = false;
                if (GUILayout.Button(pauseButtonContent))
                {
                    cameraPath.PausePath();
                }
            }
            else if (GUILayout.Button(continueButtonContent))
            {
                cameraPath.ResumePath();
            }

            if (GUILayout.Button(stopButtonContent))
            {
                cameraPath.StopPath();
            }
            GUI.enabled = true;
            EditorGUI.BeginChangeCheck();
            GUILayout.Label("Time (seconds)");
            time = EditorGUILayout.FloatField("", time, GUILayout.MinWidth(5), GUILayout.MaxWidth(50));
            if (EditorGUI.EndChangeCheck())
            {
                time = Mathf.Clamp(time, 0.001f, Mathf.Infinity);
                cameraPath.UpdateTimeInSeconds(time);
                PlayerPrefs.SetFloat("CPC_time", time);
            }
            GUILayout.EndHorizontal();
            GUI.enabled = Application.isPlaying;
            EditorGUI.BeginChangeCheck();
            currentTime = EditorGUILayout.Slider(currentTime, 0, cameraPath.pointList.Count - 1.01f);
            if (EditorGUI.EndChangeCheck())
            {
                cameraPath.SetCurrentWayPoint(Mathf.FloorToInt(currentTime));
                cameraPath.SetCurrentTimeInWaypoint(currentTime % 1);
                cameraPath.RefreshTransform();
            }
            GUI.enabled = false;
            Rect rr = GUILayoutUtility.GetRect(4, 8);
            float endWidth = rr.width - 60;
            rr.y -= 4;
            rr.width = 4;
            int c = cameraPath.pointList.Count;
            for (int i = 0; i < c; ++i)
            {
                GUI.Box(rr, "");
                rr.x += endWidth / (c - 1);
            }
            GUILayout.EndVertical();
            GUI.enabled = true;
        }

        protected virtual void DrawBasicSettings()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(targetTransformProperty);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            lookAtTargetProperty.boolValue = GUILayout.Toggle(lookAtTargetProperty.boolValue, "Look at target", GUILayout.Width(Screen.width / 3f));
            GUI.enabled = lookAtTargetProperty.boolValue;
            lookAtTargetTransformProperty.objectReferenceValue = (Transform)EditorGUILayout.ObjectField(lookAtTargetTransformProperty.objectReferenceValue, typeof(Transform), true);
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }

        protected virtual void DrawVisualDropdown()
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            visualFoldout = EditorGUILayout.Foldout(visualFoldout, "Visual");
            alwaysShowProperty.boolValue = GUILayout.Toggle(alwaysShowProperty.boolValue, alwaysShowContent);
            GUILayout.EndHorizontal();
            if (visualFoldout)
            {
                GUILayout.BeginVertical("Box");
                visualPathProperty.colorValue = EditorGUILayout.ColorField("Path color", visualPathProperty.colorValue);
                visualInactivePathProperty.colorValue = EditorGUILayout.ColorField("Inactive path color", visualInactivePathProperty.colorValue);
                visualFrustumProperty.colorValue = EditorGUILayout.ColorField("Frustum color", visualFrustumProperty.colorValue);
                visualHandleProperty.colorValue = EditorGUILayout.ColorField("Handle color", visualHandleProperty.colorValue);
                if (GUILayout.Button("Default colors"))
                {
                    Undo.RecordObject(cameraPath, "Reset to default color values");
                    cameraPath.visual = new CPC_Visual();
                }
                GUILayout.EndVertical();
            }
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }
        }

        protected virtual void DrawManipulationDropdown()
        {
            manipulationFoldout = EditorGUILayout.Foldout(manipulationFoldout, "Transform manipulation modes");
            EditorGUI.BeginChangeCheck();
            if (manipulationFoldout)
            {
                GUILayout.BeginVertical("Box");
                cameraTranslateMode = (CPC_EManipulationModes)EditorGUILayout.EnumPopup("Waypoint Translation", cameraTranslateMode);
                cameraRotationMode = (CPC_EManipulationModes)EditorGUILayout.EnumPopup("Waypoint Rotation", cameraRotationMode);
                handlePositionMode = (CPC_EManipulationModes)EditorGUILayout.EnumPopup("Handle Translation", handlePositionMode);
                GUILayout.EndVertical();
            }
            if (EditorGUI.EndChangeCheck())
            {
                PlayerPrefs.SetInt("CPC_cameraTranslateMode", (int)cameraTranslateMode);
                PlayerPrefs.SetInt("CPC_cameraRotationMode", (int)cameraRotationMode);
                PlayerPrefs.SetInt("CPC_handlePositionMode", (int)handlePositionMode);
                SceneView.RepaintAll();
            }
        }

        protected virtual void DrawWaypointList()
        {
            GUILayout.Label("Replace all lerp types");
            GUILayout.BeginVertical("Box");
            GUILayout.BeginHorizontal();
            allCurveType = (CPC_ECurveType)EditorGUILayout.EnumPopup(allCurveType, GUILayout.Width(Screen.width / 3f));
            if (GUILayout.Button(replaceAllPositionContent))
            {
                Undo.RecordObject(cameraPath, "Applied new position");
                foreach (var index in cameraPath.pointList)
                {
                    index.curveTypePosition = allCurveType;
                    if (allCurveType == CPC_ECurveType.Custom)
                        index.positionCurve.keys = allAnimationCurve.keys;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUI.enabled = allCurveType == CPC_ECurveType.Custom;
            allAnimationCurve = EditorGUILayout.CurveField(allAnimationCurve, GUILayout.Width(Screen.width / 3f));
            GUI.enabled = true;
            if (GUILayout.Button(replaceAllRotationContent))
            {
                Undo.RecordObject(cameraPath, "Applied new rotation");
                foreach (var index in cameraPath.pointList)
                {
                    index.curveTypeRotation = allCurveType;
                    if (allCurveType == CPC_ECurveType.Custom)
                        index.rotationCurve = allAnimationCurve;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Space(Screen.width / 2f - 20);
            GUILayout.Label("↓");
            GUILayout.EndHorizontal();
            serializedObject.Update();
            pointReorderableList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
            Rect r = GUILayoutUtility.GetRect(Screen.width - 16, 18);
            //r.height = 18;
            r.y -= 10;
            GUILayout.Space(-30);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(addPointContent))
            {
                Undo.RecordObject(cameraPath, "Added camera path point");
                switch (waypointMode)
                {
                    case CPC_ENewWaypointMode.SceneCamera:
                        cameraPath.pointList.Add(new CPC_Point(SceneView.lastActiveSceneView.camera.transform.position, SceneView.lastActiveSceneView.camera.transform.rotation));
                        break;
                    case CPC_ENewWaypointMode.LastWaypoint:
                        if (cameraPath.pointList.Count > 0)
                            cameraPath.pointList.Add(new CPC_Point(cameraPath.pointList[cameraPath.pointList.Count - 1].position, cameraPath.pointList[cameraPath.pointList.Count - 1].rotation) { handlenext = cameraPath.pointList[cameraPath.pointList.Count - 1].handlenext, handleprev = cameraPath.pointList[cameraPath.pointList.Count - 1].handleprev });
                        else
                        {
                            cameraPath.pointList.Add(new CPC_Point(Vector3.zero, Quaternion.identity));
                            Debug.LogWarning("No previous waypoint found to place this waypoint, defaulting position to world center");
                        }
                        break;
                    case CPC_ENewWaypointMode.WaypointIndex:
                        if (cameraPath.pointList.Count > waypointIndex - 1 && waypointIndex > 0)
                            cameraPath.pointList.Add(new CPC_Point(cameraPath.pointList[waypointIndex - 1].position, cameraPath.pointList[waypointIndex - 1].rotation) { handlenext = cameraPath.pointList[waypointIndex - 1].handlenext, handleprev = cameraPath.pointList[waypointIndex - 1].handleprev });
                        else
                        {
                            cameraPath.pointList.Add(new CPC_Point(Vector3.zero, Quaternion.identity));
                            Debug.LogWarning("Waypoint index " + waypointIndex + " does not exist, defaulting position to world center");
                        }
                        break;
                    case CPC_ENewWaypointMode.WorldCenter:
                        cameraPath.pointList.Add(new CPC_Point(Vector3.zero, Quaternion.identity));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                selectedIndex = cameraPath.pointList.Count - 1;
                SceneView.RepaintAll();
            }
            GUILayout.Label("at", GUILayout.Width(20));
            EditorGUI.BeginChangeCheck();
            waypointMode = (CPC_ENewWaypointMode)EditorGUILayout.EnumPopup(waypointMode, waypointMode == CPC_ENewWaypointMode.WaypointIndex ? GUILayout.Width(Screen.width / 4) : GUILayout.Width(Screen.width / 2));
            if (waypointMode == CPC_ENewWaypointMode.WaypointIndex)
            {
                waypointIndex = EditorGUILayout.IntField(waypointIndex, GUILayout.Width(Screen.width / 4));
            }
            if (EditorGUI.EndChangeCheck())
            {
                PlayerPrefs.SetInt("CPC_waypointMode", (int)waypointMode);
            }
            GUILayout.EndHorizontal();
        }

        protected virtual void DrawHandles(int i)
        {
            DrawHandleLines(i);
            Handles.color = cameraPath.visual.handleColor;
            DrawNextHandle(i);
            DrawPrevHandle(i);
            DrawWaypointHandles(i);
            DrawSelectionHandles(i);
        }

        protected virtual void DrawHandleLines(int i)
        {
            Handles.color = cameraPath.visual.handleColor;
            if (i < cameraPath.pointList.Count - 1)
            {
                Handles.DrawLine(cameraPath.pointList[i].position, cameraPath.pointList[i].position + cameraPath.pointList[i].handlenext);
            }

            if (i > 0)
            {
                Handles.DrawLine(cameraPath.pointList[i].position, cameraPath.pointList[i].position + cameraPath.pointList[i].handleprev);
            }
            Handles.color = Color.white;
        }

        protected virtual void DrawNextHandle(int i)
        {
            if (i < cameraPath.pointList.Count - 1)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 posNext = Vector3.zero;
                float size = HandleUtility.GetHandleSize(cameraPath.pointList[i].position + cameraPath.pointList[i].handlenext) * 0.1f;
                if (handlePositionMode == CPC_EManipulationModes.Free)
                {
#if UNITY_5_5_OR_NEWER
                    var fmh_570_97_638613611676667022 = Quaternion.identity; posNext = Handles.FreeMoveHandle(cameraPath.pointList[i].position + cameraPath.pointList[i].handlenext, fmh_570_97_638613611676667022, size, Vector3.zero, Handles.SphereHandleCap);
#else
                posNext = Handles.FreeMoveHandle(t.pointList[i].position + t.pointList[i].handlenext, Quaternion.identity, size, Vector3.zero, Handles.SphereCap);
#endif
                }
                else
                {
                    if (selectedIndex == i)
                    {
#if UNITY_5_5_OR_NEWER
                        Handles.SphereHandleCap(0, cameraPath.pointList[i].position + cameraPath.pointList[i].handlenext, Quaternion.identity, size, EventType.Repaint);
#else
                    Handles.SphereCap(0, t.pointList[i].position + t.pointList[i].handlenext, Quaternion.identity, size);
#endif
                        posNext = Handles.PositionHandle(cameraPath.pointList[i].position + cameraPath.pointList[i].handlenext, Quaternion.identity);
                    }
                    else if (Event.current.button != 1)
                    {
#if UNITY_5_5_OR_NEWER
                        if (Handles.Button(cameraPath.pointList[i].position + cameraPath.pointList[i].handlenext, Quaternion.identity, size, size, Handles.CubeHandleCap))
                        {
                            SelectIndex(i);
                        }
#else
                    if (Handles.Button(t.pointList[i].position + t.pointList[i].handlenext, Quaternion.identity, size, size, Handles.CubeCap))
                    {
                        SelectIndex(i);
                    }
#endif
                    }
                }
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Changed Handle Position");
                    cameraPath.pointList[i].handlenext = posNext - cameraPath.pointList[i].position;
                    if (cameraPath.pointList[i].chained)
                        cameraPath.pointList[i].handleprev = cameraPath.pointList[i].handlenext * -1;
                }
            }
        }

        protected virtual void DrawPrevHandle(int i)
        {
            if (i > 0)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 posPrev = Vector3.zero;
                float size = HandleUtility.GetHandleSize(cameraPath.pointList[i].position + cameraPath.pointList[i].handleprev) * 0.1f;
                if (handlePositionMode == CPC_EManipulationModes.Free)
                {
#if UNITY_5_5_OR_NEWER
                    var fmh_622_97_638613611676693016 = Quaternion.identity; posPrev = Handles.FreeMoveHandle(cameraPath.pointList[i].position + cameraPath.pointList[i].handleprev, fmh_622_97_638613611676693016, 0.1f * HandleUtility.GetHandleSize(cameraPath.pointList[i].position + cameraPath.pointList[i].handleprev), Vector3.zero, Handles.SphereHandleCap);
#else
                posPrev = Handles.FreeMoveHandle(t.pointList[i].position + t.pointList[i].handleprev, Quaternion.identity, 0.1f * HandleUtility.GetHandleSize(t.pointList[i].position + t.pointList[i].handleprev), Vector3.zero, Handles.SphereCap);
#endif
                }
                else
                {
                    if (selectedIndex == i)
                    {
#if UNITY_5_5_OR_NEWER
                        Handles.SphereHandleCap(0, cameraPath.pointList[i].position + cameraPath.pointList[i].handleprev, Quaternion.identity, 0.1f * HandleUtility.GetHandleSize(cameraPath.pointList[i].position + cameraPath.pointList[i].handlenext), EventType.Repaint);
#else
                    Handles.SphereCap(0, t.pointList[i].position + t.pointList[i].handleprev, Quaternion.identity,
                        0.1f * HandleUtility.GetHandleSize(t.pointList[i].position + t.pointList[i].handlenext));
#endif
                        posPrev = Handles.PositionHandle(cameraPath.pointList[i].position + cameraPath.pointList[i].handleprev, Quaternion.identity);
                    }
                    else if (Event.current.button != 1)
                    {
#if UNITY_5_5_OR_NEWER
                        if (Handles.Button(cameraPath.pointList[i].position + cameraPath.pointList[i].handleprev, Quaternion.identity, size, size, Handles.CubeHandleCap))
                        {
                            SelectIndex(i);
                        }
#else
                    if (Handles.Button(t.pointList[i].position + t.pointList[i].handleprev, Quaternion.identity, size, size,
                        Handles.CubeCap))
                    {
                        SelectIndex(i);
                    }
#endif
                    }
                }
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Changed Handle Position");
                    cameraPath.pointList[i].handleprev = posPrev - cameraPath.pointList[i].position;
                    if (cameraPath.pointList[i].chained)
                        cameraPath.pointList[i].handlenext = cameraPath.pointList[i].handleprev * -1;
                }
            }
        }

        protected virtual void DrawWaypointHandles(int i)
        {
            if (Tools.current == Tool.Move)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 pos = Vector3.zero;
                if (cameraTranslateMode == CPC_EManipulationModes.SelectAndTransform)
                {
                    if (i == selectedIndex) pos = Handles.PositionHandle(cameraPath.pointList[i].position, (Tools.pivotRotation == PivotRotation.Local) ? cameraPath.pointList[i].rotation : Quaternion.identity);
                }
                else
                {
#if UNITY_5_5_OR_NEWER
                    var fmh_678_68_638613611676700785 = (Tools.pivotRotation == PivotRotation.Local) ? cameraPath.pointList[i].rotation : Quaternion.identity; pos = Handles.FreeMoveHandle(cameraPath.pointList[i].position, fmh_678_68_638613611676700785, HandleUtility.GetHandleSize(cameraPath.pointList[i].position) * 0.2f, Vector3.zero, Handles.RectangleHandleCap);
#else
                pos = Handles.FreeMoveHandle(t.pointList[i].position, (Tools.pivotRotation == PivotRotation.Local) ? t.pointList[i].rotation : Quaternion.identity, HandleUtility.GetHandleSize(t.pointList[i].position) * 0.2f, Vector3.zero, Handles.RectangleCap);
#endif
                }
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Moved Waypoint");
                    cameraPath.pointList[i].position = pos;
                }
            }
            else if (Tools.current == Tool.Rotate)
            {

                EditorGUI.BeginChangeCheck();
                Quaternion rot = Quaternion.identity;
                if (cameraRotationMode == CPC_EManipulationModes.SelectAndTransform)
                {
                    if (i == selectedIndex) rot = Handles.RotationHandle(cameraPath.pointList[i].rotation, cameraPath.pointList[i].position);
                }
                else
                {
                    rot = Handles.FreeRotateHandle(cameraPath.pointList[i].rotation, cameraPath.pointList[i].position, HandleUtility.GetHandleSize(cameraPath.pointList[i].position) * 0.2f);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Rotated Waypoint");
                    cameraPath.pointList[i].rotation = rot;
                }
            }
        }

        protected virtual void DrawSelectionHandles(int i)
        {
            if (Event.current.button != 1 && selectedIndex != i)
            {
                if (cameraTranslateMode == CPC_EManipulationModes.SelectAndTransform && Tools.current == Tool.Move
                    || cameraRotationMode == CPC_EManipulationModes.SelectAndTransform && Tools.current == Tool.Rotate)
                {
                    float size = HandleUtility.GetHandleSize(cameraPath.pointList[i].position) * 0.2f;
#if UNITY_5_5_OR_NEWER
                    if (Handles.Button(cameraPath.pointList[i].position, Quaternion.identity, size, size, Handles.CubeHandleCap))
                    {
                        SelectIndex(i);
                    }
#else
                if (Handles.Button(t.pointList[i].position, Quaternion.identity, size, size, Handles.CubeCap))
                {
                    SelectIndex(i);
                }
#endif
                }
            }
        }

        protected virtual void DrawRawValues()
        {
            if (GUILayout.Button(showRawValues ? "Hide raw values" : "Show raw values"))
                showRawValues = !showRawValues;

            if (showRawValues)
            {
                foreach (var i in cameraPath.pointList)
                {
                    EditorGUI.BeginChangeCheck();
                    GUILayout.BeginVertical("Box");
                    Vector3 pos = EditorGUILayout.Vector3Field("Waypoint Position", i.position);
                    Quaternion rot = Quaternion.Euler(EditorGUILayout.Vector3Field("Waypoint Rotation", i.rotation.eulerAngles));
                    Vector3 posp = EditorGUILayout.Vector3Field("Previous Handle Offset", i.handleprev);
                    Vector3 posn = EditorGUILayout.Vector3Field("Next Handle Offset", i.handlenext);
                    GUILayout.EndVertical();
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        Undo.RecordObject(cameraPath, "Changed waypoint transform");
                        i.position = pos;
                        i.rotation = rot;
                        i.handleprev = posp;
                        i.handlenext = posn;
                        SceneView.RepaintAll();
                    }
                }
            }
        }
    }
}