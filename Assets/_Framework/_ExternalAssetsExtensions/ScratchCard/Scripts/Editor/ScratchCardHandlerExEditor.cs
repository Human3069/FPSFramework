using ScratchCardAsset.Core;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using ScratchCardAsset;

namespace _KMH_Framework
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(ScratchCardHandlerEx))]
	public class ScratchCardHandlerExEditor : Editor
	{
        protected SerializedProperty m_Script;

        private SerializedProperty _progress;
        private SerializedProperty Card;
        private SerializedProperty renderType;
        private SerializedProperty MeshRendererCard;
        private SerializedProperty SpriteRendererCard;
        private SerializedProperty imageCard;
        private SerializedProperty hasAlpha;
        private SerializedProperty _mode;
        private SerializedProperty _camera;
        private SerializedProperty _scratchSurfaceSprite;
        private SerializedProperty _progressAccuracy;
        private SerializedProperty _brushTexture;
        private SerializedProperty _brushSize;
        private SerializedProperty _brushOpacity;
        private SerializedProperty _inputEnabled;
        private SerializedProperty usePressure;
        private SerializedProperty _isCanvasRaycasts;
        private SerializedProperty canvasesForRaycastsBlocking;
        private SerializedProperty maskShader;
        private SerializedProperty brushShader;
        private SerializedProperty maskProgressShader;

        private ScratchCardHandlerEx cardHandler;

        private bool inputBlockShow;
        private bool cardParametersBlockShow;

        private const string InputBlockPrefsKey = "ScratchCardManager.InputBlock";
		private const string CardParametersBlockPrefsKey = "ScratchCardManager.CardParametersBlock";

		protected void OnEnable()
		{
			m_Script = serializedObject.FindProperty("m_Script");

			_progress = serializedObject.FindProperty("_progress");
			Card = serializedObject.FindProperty("Card");

			renderType = serializedObject.FindProperty("RenderType");
			MeshRendererCard = serializedObject.FindProperty("MeshRendererCard");
			SpriteRendererCard = serializedObject.FindProperty("SpriteRendererCard");
			imageCard = serializedObject.FindProperty("canvasRendererCard");
			hasAlpha = serializedObject.FindProperty("scratchSurfaceSpriteHasAlpha");

			_mode = serializedObject.FindProperty("_mode");
			_camera = serializedObject.FindProperty("_camera");
			_scratchSurfaceSprite = serializedObject.FindProperty("_scratchSurfaceSprite");
			_progressAccuracy = serializedObject.FindProperty("_progressAccuracy");
			_brushTexture = serializedObject.FindProperty("_brushTexture");
			_brushSize = serializedObject.FindProperty("_brushSize");
			_brushOpacity = serializedObject.FindProperty("_brushOpacity");
			
			_inputEnabled = serializedObject.FindProperty("_inputEnabled");
			usePressure = serializedObject.FindProperty("usePressure");
			_isCanvasRaycasts = serializedObject.FindProperty("_isCanvasRaycasts");
			canvasesForRaycastsBlocking = serializedObject.FindProperty("canvasesForRaycastsBlocking");

			maskShader = serializedObject.FindProperty("maskShader");
			brushShader = serializedObject.FindProperty("brushShader");
			maskProgressShader = serializedObject.FindProperty("maskProgressShader");

			inputBlockShow = EditorPrefs.GetBool(InputBlockPrefsKey, true);
			cardParametersBlockShow = EditorPrefs.GetBool(CardParametersBlockPrefsKey, true);
		}

		public override bool RequiresConstantRepaint()
		{
			return cardHandler != null && cardHandler.Card != null && cardHandler.Card.RenderTexture != null && cardHandler.Card.IsScratched;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.PropertyField(m_Script);
            EditorGUI.EndDisabledGroup();

			EditorGUILayout.PropertyField(_progress);
			EditorGUILayout.PropertyField(Card);

            EditorGUILayout.PropertyField(_camera, new GUIContent("Main Camera"));
            cardHandler = (ScratchCardHandlerEx)target;
			
			EditorGUI.BeginDisabledGroup(Application.isPlaying);
			EditorGUI.BeginChangeCheck();
			
			EditorGUILayout.PropertyField(renderType, new GUIContent("Render Type"));
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(cardHandler, "Change Render Type");
				if (cardHandler.TrySelectCard((ScratchCardRenderType)renderType.enumValueIndex))
				{
					cardHandler.InitSurfaceMaterial();
				}
			}
			
			var canSetNativeSize = true;
			var cardManagerChanged = false;
			var cardRenderType = (ScratchCardRenderType)renderType.enumValueIndex;
			if (cardRenderType == ScratchCardRenderType.MeshRenderer)
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(MeshRendererCard, new GUIContent("Mesh Card"));
				var changed = EditorGUI.EndChangeCheck();
				cardManagerChanged |= changed;
				if (changed && MeshRendererCard.objectReferenceValue != null)
				{
					serializedObject.ApplyModifiedProperties();
					cardHandler.InitSurfaceMaterial();
				}
				
				if (MeshRendererCard.objectReferenceValue == null)
				{
					EditorGUILayout.HelpBox("Mesh Card is null, please set reference to Mesh Card", MessageType.Warning);
					canSetNativeSize = false;
				}
			}
			else if (cardRenderType == ScratchCardRenderType.SpriteRenderer)
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(SpriteRendererCard, new GUIContent("Sprite Card"));
				var changed = EditorGUI.EndChangeCheck();
				cardManagerChanged |= changed;
				if (changed && SpriteRendererCard.objectReferenceValue != null)
				{
					serializedObject.ApplyModifiedProperties();
					cardHandler.InitSurfaceMaterial();
				}

				if (SpriteRendererCard.objectReferenceValue == null)
				{
					EditorGUILayout.HelpBox("Sprite Card is null, please set reference to Sprite Card", MessageType.Warning);
					canSetNativeSize = false;
				}
			}
			else if (cardRenderType == ScratchCardRenderType.CanvasRenderer)
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(imageCard, new GUIContent("Image Card"));
				var changed = EditorGUI.EndChangeCheck();
				cardManagerChanged |= changed;
				if (changed && imageCard.objectReferenceValue != null)
				{
					serializedObject.ApplyModifiedProperties();
					cardHandler.InitSurfaceMaterial();
				}

				if (imageCard.objectReferenceValue == null)
				{
					EditorGUILayout.HelpBox("Image Card is null, please set reference to Image Card", MessageType.Warning);
					canSetNativeSize = false;
				}
			}

			EditorGUI.EndDisabledGroup();

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(_scratchSurfaceSprite, new GUIContent("Sprite"));
			var spriteChanged = EditorGUI.EndChangeCheck();
			if (spriteChanged)
			{
				cardHandler.ScratchSurfaceSprite = (Sprite)_scratchSurfaceSprite.objectReferenceValue;
			}
			if (_scratchSurfaceSprite.objectReferenceValue == null)
			{
				EditorGUILayout.HelpBox("Sprite is null, please set reference to Sprite", MessageType.Warning);
			}
			
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(_progressAccuracy, new GUIContent("Progress Accuracy"));
			if (EditorGUI.EndChangeCheck())
			{
				cardManagerChanged = true;
				cardHandler.ProgressAccuracy = (ProgressAccuracy)_progressAccuracy.enumValueIndex;
			}

			EditorGUI.BeginDisabledGroup(Application.isPlaying);

			if (maskShader.objectReferenceValue == null)
			{
				EditorGUILayout.PropertyField(maskShader, new GUIContent("Mask Shader"));
			}

			if (brushShader.objectReferenceValue == null)
			{
				EditorGUILayout.PropertyField(brushShader, new GUIContent("Brush Shader"));
			}

			if (maskProgressShader.objectReferenceValue == null)
			{
				EditorGUILayout.PropertyField(maskProgressShader, new GUIContent("Mask Progress Shader"));
			}

			EditorGUI.EndDisabledGroup();
			
			DrawHorizontalLine();

			EditorGUI.BeginChangeCheck();
			inputBlockShow = EditorGUILayout.Foldout(inputBlockShow, "Input Parameters");
			if (EditorGUI.EndChangeCheck())
			{
				EditorPrefs.SetBool(InputBlockPrefsKey, inputBlockShow);
			}
			bool inputEnableChanged = false;
            bool usePressureChanged = false;
            bool checkCanvasRaycastsChanged = false;
            bool canvasesForRaycastsBlockingChanged = false;
			if (inputBlockShow == true)
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(_inputEnabled, new GUIContent("Input Enabled"));
				inputEnableChanged = EditorGUI.EndChangeCheck();
				
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(usePressure, new GUIContent("Use Pressure"));
				usePressureChanged = EditorGUI.EndChangeCheck();

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(_isCanvasRaycasts, new GUIContent("Check Canvas Raycasts"));
				checkCanvasRaycastsChanged = EditorGUI.EndChangeCheck();
				
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(canvasesForRaycastsBlocking, new GUIContent("Canvases For Raycasts Blocking"));
				canvasesForRaycastsBlockingChanged = EditorGUI.EndChangeCheck();
			}
			
			DrawHorizontalLine();
			
			EditorGUI.BeginDisabledGroup(true);
			EditorGUI.BeginChangeCheck();
			cardParametersBlockShow = EditorGUILayout.Foldout(cardParametersBlockShow, "Scratch Card Parameters");
			if (EditorGUI.EndChangeCheck())
			{
				EditorPrefs.SetBool(CardParametersBlockPrefsKey, cardParametersBlockShow);
			}
			var brushTextureChanged = false;
			var brushOpacityChanged = false;
			var brushSizeChanged = false;
			var scratchModeChanged = false;
			if (cardParametersBlockShow)
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(_brushTexture, new GUIContent("Brush Texture"));
				brushTextureChanged = EditorGUI.EndChangeCheck();
			
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.Slider(_brushOpacity, 0f, 1f, new GUIContent("Brush Opacity"));
				brushOpacityChanged = EditorGUI.EndChangeCheck();
			
				if (_brushTexture.objectReferenceValue != null)
				{
					if (cardHandler.Card != null)
					{
						_brushSize.floatValue = cardHandler.BrushSize;
					}
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.Slider(_brushSize, 0.01f, 4f, new GUIContent("Brush Size"));
					brushSizeChanged = EditorGUI.EndChangeCheck();
				}
			
				if (cardHandler.Card != null)
				{
					_mode.enumValueIndex = (int) cardHandler.Card.Mode;
				}

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(_mode, new GUIContent("Scratch Mode"));
				scratchModeChanged = EditorGUI.EndChangeCheck();
			
				EditorGUI.EndDisabledGroup();
			}

			DrawHorizontalLine();

			EditorGUI.BeginDisabledGroup(!canSetNativeSize);
			if (GUILayout.Button("Set Native Size"))
			{
				if (cardRenderType == ScratchCardRenderType.MeshRenderer)
				{
					var meshRendererComponent = MeshRendererCard.objectReferenceValue as MeshRenderer;
					if (meshRendererComponent != null)
					{
						Undo.RecordObject(meshRendererComponent.transform, "Set Native Size");
						cardHandler.SetNativeSize();
					}
				}
				else if (cardRenderType == ScratchCardRenderType.SpriteRenderer)
				{
					var spriteRendererComponent = SpriteRendererCard.objectReferenceValue as SpriteRenderer;
					if (spriteRendererComponent != null)
					{
						Undo.RecordObject(spriteRendererComponent.transform, "Set Native Size");
						cardHandler.SetNativeSize();
					}
				}
				else if (cardRenderType == ScratchCardRenderType.CanvasRenderer)
				{
					var imageComponent = imageCard.objectReferenceValue as Image;
					if (imageComponent != null)
					{
						Undo.RecordObject(imageComponent.rectTransform, "Set Native Size");
						cardHandler.SetNativeSize();
					}
				}
			}
			EditorGUI.EndDisabledGroup();
			
			if (_scratchSurfaceSprite.objectReferenceValue != null && spriteChanged)
			{
				var path = AssetDatabase.GetAssetPath(_scratchSurfaceSprite.objectReferenceValue);
				var importer = (TextureImporter) AssetImporter.GetAtPath(path);
				if (importer != null)
				{
					hasAlpha.boolValue = importer.DoesSourceTextureHaveAlpha();
					cardHandler.IsScratchSurfaceSpriteHasAlpha = hasAlpha.boolValue;
				}
			}

            // var scratchCardChanged = false;
            if (brushTextureChanged && cardHandler.Card.BrushMaterial != null)
            {
                cardHandler.Card.BrushMaterial.mainTexture = _brushTexture.objectReferenceValue as Texture2D;
                // scratchCardChanged = true;
            }

            if (brushOpacityChanged)
            {
                Undo.RecordObject(cardHandler, "Set Brush Opacity");
                cardHandler.BrushOpacity = _brushOpacity.floatValue;
                // scratchCardChanged = true;
            }

            if (brushSizeChanged)
            {
                // Undo.RecordObject(cardHandler.Card, "Set Brush Size");
                cardHandler.BrushSize = _brushSize.floatValue;
                // scratchCardChanged = true;
            }

            if (inputEnableChanged)
            {
                // scratchCardChanged = true;
                cardHandler.InputEnabled = _inputEnabled.boolValue;
            }

            if (usePressureChanged)
            {
                cardHandler.UsePressure = usePressure.boolValue;
            }

            if (checkCanvasRaycastsChanged)
            {
                cardHandler.IsCanvasRaycasts = _isCanvasRaycasts.boolValue;
            }

            if (canvasesForRaycastsBlockingChanged)
            {
                serializedObject.ApplyModifiedProperties();
                var data = cardHandler.CanvasesForRaycastsBlocking;
                cardHandler.CanvasesForRaycastsBlocking = data;
            }

            if (scratchModeChanged)
            {
                cardHandler.Mode = (ScratchMode)_mode.enumValueIndex;
                // scratchCardChanged = true;
            }

			if (cardHandler.Card == null)
			{
				cardHandler.Card = cardHandler.GetComponent<ScratchCardEx>();

            }

            if (cardHandler.Card.RenderTexture != null)
            {
                DrawHorizontalLine();
                var rect = GUILayoutUtility.GetRect(160, 120, GUILayout.ExpandWidth(true));
                GUI.DrawTexture(rect, cardHandler.Card.RenderTexture, ScaleMode.ScaleToFit);
                DrawHorizontalLine();

                if (Application.isPlaying)
                {
                    if (GUILayout.Button("Clear"))
                    {
                        cardHandler.ClearScratchCard();
                    }

                    if (GUILayout.Button("Fill"))
                    {
                        cardHandler.FillScratchCard();
                    }
                }
            }

            if (cardManagerChanged)
            {
                MarkAsDirty(target);
            }

            // if (scratchCardChanged)
            // {
            //     MarkAsDirty(cardHandler.Card);
            // }
			
            serializedObject.ApplyModifiedProperties();
		}

		private void MarkAsDirty(Object objectTarget, bool markScene = true)
		{
			if (Application.isPlaying)
				return;

			EditorUtility.SetDirty(objectTarget);
			if (markScene)
			{
				var component = objectTarget as Component;
				if (component != null)
				{
					EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
				}
			}
		}

		private void DrawHorizontalLine()
		{
			GUILayout.Space(5f);
			EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 2f), Color.gray);
			GUILayout.Space(5f);
		}
	}
}