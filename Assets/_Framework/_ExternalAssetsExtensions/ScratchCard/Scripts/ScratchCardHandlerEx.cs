using System.Collections.Generic;
using ScratchCardAsset.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

namespace _KMH_Framework
{
	[System.Serializable]
	public class ScratchCardHandlerEx : MonoBehaviour
	{
		[SerializeField]
		protected EraseProgressEx _progress;
        public ScratchCardEx Card;

        [Space(10)]
        [SerializeField]
        protected Camera _camera;
        public Camera _Camera
        {
            get
            {
                return _camera;
            }
            set
            {
                _camera = value;
                if (Card != null && Card.ScratchData != null)
                {
                    Card.ScratchData.Camera = _camera;
                }
            }
        }

		public ScratchCardRenderType RenderType;

		[Space(10)]
		public MeshRenderer MeshRendererCard;
		public SpriteRenderer SpriteRendererCard;
		public Image CanvasRendererCard;

		[SerializeField]
		private bool _isScratchSurfaceSpriteHasAlpha = true;
		public bool IsScratchSurfaceSpriteHasAlpha
		{
			get
			{
				return _isScratchSurfaceSpriteHasAlpha;
            }
			set
			{
				_isScratchSurfaceSpriteHasAlpha = value;
				if (_progress != null)
				{
                    _progress.IsSampleSourceTexture = _isScratchSurfaceSpriteHasAlpha;
				}
			}
		}

		[SerializeField]
		private ScratchMode _mode;
		public ScratchMode Mode
		{
			get
			{
				return _mode;
            }
			set
			{
				_mode = value;
				if (Card != null)
				{
					Card.Mode = _mode;
				}
			}
		}

		[SerializeField]
		private Sprite _scratchSurfaceSprite;
		public Sprite ScratchSurfaceSprite
		{
            get
            {
                return _scratchSurfaceSprite;
            }
            set
			{
				_scratchSurfaceSprite = value;
				if (Card != null)
				{
					if (Application.isPlaying == true)
					{
						if (isInitialized == true)
						{
							UpdateCardSprite(_scratchSurfaceSprite);
							Card.SetRenderType(RenderType, _camera);

                            if (Card.ScratchData == null)
                            {
                                Debug.LogError("ScratchData is null!");
                                this.enabled = false;
                                return;
                            }
                            Card.Init(this);

                            if (_progress != null)
							{
                                _progress.ResetProgress();
                                _progress.UpdateProgress();
							}
						}
					}
					else
					{
						InitSurfaceMaterial();
					}
				}
			}
		}
		
		[SerializeField]
		private ProgressAccuracy _progressAccuracy;
		public ProgressAccuracy ProgressAccuracy
		{
			get
			{
				return _progressAccuracy;
            }
			set
			{
				_progressAccuracy = value;
				if (_progress != null && _progress.ProgressMaterial != null)
				{
                    _progress.ProgressAccuracy = _progressAccuracy;
				}
			}
		}

		[SerializeField]
		private Texture _brushTexture;
		public Texture BrushTexture
		{
            get
            {
                return _brushTexture;
            }
            set
			{
				_brushTexture = value;
				if (Card != null && Card.BrushMaterial != null)
				{
					Card.BrushMaterial.mainTexture = _brushTexture;
				}
			}
		}
		
		[SerializeField]
		private float _brushSize = 1f;
		public float BrushSize
		{
            get
            {
                return _brushSize;
            }
            set
			{
				_brushSize = value;
				if (Card != null && Card.IsInitialized)
				{
					Card.BrushSize = _brushSize;
				}
			}
		}
		
		[SerializeField]
		private float _brushOpacity = 1f;
		public float BrushOpacity
		{
            get
            {
                return _brushOpacity;
            }
            set
			{
				_brushOpacity = value;
				if (Card != null && Card.BrushMaterial != null)
				{
					Card.BrushMaterial.color = new Color(Card.BrushMaterial.color.r, Card.BrushMaterial.color.g, Card.BrushMaterial.color.b, _brushOpacity);
				}
			}
		}

		[SerializeField]
		private bool _inputEnabled = true;
		public bool InputEnabled
		{
            get
            {
                return _inputEnabled;
            }
            set
			{
				_inputEnabled = value;
				this.enabled = _inputEnabled;
			}
		}

		[SerializeField]
		private bool usePressure;
		public bool UsePressure
		{
            get
            {
                return usePressure;
            }
            set
			{
				usePressure = value;
				if (Card != null && Card.IsInitialized)
				{
					Card._Input.UsePressure = usePressure;
				}
			}
		}
		
		[SerializeField]
		private bool _isCanvasRaycasts = true;
		public bool IsCanvasRaycasts
		{
			get
			{
				return _isCanvasRaycasts;
            }
			set
			{
				_isCanvasRaycasts = value;
				if (Card != null && Card.IsInitialized)
				{
					Card._Input.CheckCanvasRaycasts = _isCanvasRaycasts;
					if (_isCanvasRaycasts == true)
					{
						Card._Input.InitRaycastsController(Card.SurfaceTransform.gameObject, canvasesForRaycastsBlocking);
					}
				}
			}
		}

		[SerializeField]
		private Canvas[] canvasesForRaycastsBlocking;
		public Canvas[] CanvasesForRaycastsBlocking
		{
			get
			{
				return canvasesForRaycastsBlocking;
            }
			set
			{
				canvasesForRaycastsBlocking = value;
				if (Card != null && Card.IsInitialized == true)
				{
					Card._Input.InitRaycastsController(Card.SurfaceTransform.gameObject, canvasesForRaycastsBlocking);
				}
			}
		}
		
		[SerializeField]
		private Shader maskShader;
		[SerializeField]
		private Shader brushShader;
		[SerializeField]
		private Shader maskProgressShader;
		
		private Material surfaceMaterial;
		private Texture2D scratchTexture;
		private Color[] spritePixels;
		private Sprite scratchSprite;
		private MigrationHelperEx migrationHelper;
		private bool isInitialized;

		private void Awake()
		{
            if (Application.isPlaying == false)
			{
				migrationHelper = new MigrationHelperEx();
				migrationHelper.StartMigrate(this);
				InitSurfaceMaterial();
			}
		}

		private void Start()
		{
			// Progresser Initialize
            if (Application.isPlaying == true)
			{
                _progress.Initialize(this, Card);
            }
            else
			{
				migrationHelper.FinishMigrate();
				migrationHelper = null;

				return;
			}

			// Card Initialize
            if (Card.IsInitialized == true)
            {
                return;
            }
            Card.Init(this);

			// Self Initialize
            if (isInitialized == true)
			{
                return;
            }
			
			Init();
		}

		private void OnDestroy()
		{
            Card.UnsubscribeFromEvents();
            Destroy(Card.ReleaseRenderTexture());
            Card.CardRenderer.Release();

            if (_progress.PercentRenderTexture != null && _progress.PercentRenderTexture.IsCreated() == true)
            {
                _progress.PercentRenderTexture.Release();
                Destroy(_progress.PercentRenderTexture);
                _progress.PercentRenderTexture = null;
            }

            if (_progress.PercentRenderTexture != null)
            {
                Destroy(_progress.PercentRenderTexture);
                _progress.PercentRenderTexture = null;
            }

            if (_progress._Mesh != null)
            {
                Destroy(_progress._Mesh);
                _progress._Mesh = null;
            }
            _progress.DeInitialize();


			if (surfaceMaterial != null)
			{
				if (Application.isPlaying == true)
				{
					Destroy(surfaceMaterial);
				}
				else
				{
					DestroyImmediate(surfaceMaterial);
				}
				surfaceMaterial = null;
			}

			if (Application.isPlaying == false)
			{
				//
			}
			else
			{
                if (Card != null)
                {
                    Card.OnInitialized -= OnCardInitialized;
                    Card.OnRenderTextureInitialized -= OnCardRenderTextureInitialized;
                }
                ReleaseTexture();
            }
		}

        protected void OnDisable()
        {
            if (Card.IsInitialized == false)
            {
                return;
            }

            Card._Input.ResetData();
        }

        protected void Update()
        {
            if (Card._Input.TryUpdate() == false)
            {
				if (Card.CardRenderer == null)
				{
                    Card.Init(this);
                }

                Card.CardRenderer.IsScratched = false;
            }
        }

        protected void LateUpdate()
        {
			if (Application.isPlaying == true)
			{
                if (Card.Mode != _progress.ScratchMode)
                {
                    _progress.ScratchMode = Card.Mode;
                    _progress.ResetProgress();
                }

                if ((Card.IsScratched == true || _progress.IsUpdateProgress == true) && _progress.IsCompleted == false)
                {
                    _progress.UpdateProgress();
                }
            }
        }

        public void Init()
		{
			if (Card == null)
			{
				Debug.LogError("ScratchCard field is not assigned!");
				return;
			}
			
			if (_camera == null)
			{
				_camera = _camera != null ? _camera : Camera.main;
			}

			InitSurfaceMaterial();
			InitBrushMaterial();
			InitProgressMaterial();
			if (TrySelectCard(RenderType) == true)
			{
				Card.BrushSize = BrushSize;
				Card.Mode = _mode;
				Card.SetRenderType(RenderType, _camera);
				Card.OnInitialized -= OnCardInitialized;
				Card.OnInitialized += OnCardInitialized;
				Card.OnRenderTextureInitialized -= OnCardRenderTextureInitialized;
				Card.OnRenderTextureInitialized += OnCardRenderTextureInitialized;

                if (Card.ScratchData == null)
                {
                    Debug.LogError("ScratchData is null!");
                    this.enabled = false;
                    return;
                }
                Card.Init(this);
			}
			else
			{
				this.enabled = false;
			}
			
			if (Card.Mode == ScratchMode.Restore)
			{
				Card.Fill(false);
			}
			
			isInitialized = true;
		}

		public void DestroyRenderTexture(RenderTexture _renderTexture)
		{
			Destroy(_renderTexture);
		}


        private void OnCardInitialized(ScratchCardEx scratchCard)
		{
			scratchCard._Input.UsePressure = usePressure;
			scratchCard._Input.CheckCanvasRaycasts = _isCanvasRaycasts;
			if (_isCanvasRaycasts == true)
			{
				scratchCard._Input.InitRaycastsController(scratchCard.SurfaceTransform.gameObject, canvasesForRaycastsBlocking);
			}
		}

		private void OnCardRenderTextureInitialized(RenderTexture renderTexture)
		{
			if (_progress != null && _progress.ProgressMaterial != null)
			{
                _progress.ProgressMaterial.mainTexture = renderTexture;
			}
		}
		
		private void ReleaseTexture()
		{
			if (scratchTexture != null)
			{
				Destroy(scratchTexture);
				scratchTexture = null;
			}

			if (scratchSprite != null)
			{
				Destroy(scratchSprite);
				scratchSprite = null;
			}
		}

		public void InitSurfaceMaterial()
		{
			if (Card != null && Card.SurfaceMaterial == null)
			{
                Material scratchSurfaceMaterial = new Material(maskShader);
				Card.SurfaceMaterial = scratchSurfaceMaterial;
				surfaceMaterial = scratchSurfaceMaterial;
			}
			UpdateCardSprite(_scratchSurfaceSprite);
		}

		private void UpdateCardSprite(Sprite sprite)
		{
			if (Card == null)
			{
				Card = this.GetComponent<ScratchCardEx>();
            }

			ReleaseTexture();
            Material scratchSurfaceMaterial = Card.SurfaceMaterial;
			bool isPartOfAtlas = sprite != null && (sprite.texture.width != sprite.rect.size.x || sprite.texture.height != sprite.rect.size.y);
			if (Application.isPlaying)
			{
				if (isPartOfAtlas == true || _isScratchSurfaceSpriteHasAlpha == true)
				{
					if (sprite.texture.isReadable == true)
					{
						if (sprite.packed == true)
						{
							spritePixels = sprite.texture.GetPixels((int)sprite.textureRect.x, (int)sprite.textureRect.y, (int)sprite.rect.width, (int)sprite.rect.height);
						}
						else
						{
							spritePixels = sprite.texture.GetPixels((int)sprite.rect.x, (int)sprite.rect.y, (int)sprite.rect.width, (int)sprite.rect.height);
						}
					}
					else
					{
						Debug.LogError("Texture is not readable, please set Read/Write flag in texture settings.");
					}
				}

				if (isPartOfAtlas == true)
				{
					scratchTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
					scratchTexture.SetPixels(spritePixels);
					scratchTexture.Apply();
					
					if (scratchSurfaceMaterial != null)
					{
						scratchSurfaceMaterial.mainTexture = scratchTexture;
					}

					if (RenderType == ScratchCardRenderType.SpriteRenderer || RenderType == ScratchCardRenderType.CanvasRenderer)
					{
						Rect croppedRect = new Rect(0, 0, scratchTexture.width, scratchTexture.height);
						Vector2 pivot = _scratchSurfaceSprite.pivot / croppedRect.size;
						scratchSprite = Sprite.Create(scratchTexture, croppedRect, pivot, Constants.General.PixelsPerUnit);
						sprite = scratchSprite;
					}
				}
				else if (scratchSurfaceMaterial != null && _scratchSurfaceSprite != null)
				{
					scratchSurfaceMaterial.mainTexture = _scratchSurfaceSprite.texture;
				}
				UpdateProgressMaterial();
			}
			else if (Card.SurfaceMaterial != null && _scratchSurfaceSprite != null)
			{
				Card.SurfaceMaterial.mainTexture = _scratchSurfaceSprite.texture;
			}

			UpdateCardOffset();

			if (RenderType == ScratchCardRenderType.MeshRenderer && MeshRendererCard != null)
			{
				if (Card.SurfaceMaterial != null)
				{
					MeshRendererCard.sharedMaterial = Card.SurfaceMaterial;
				}
				if (Card.SurfaceMaterial != null && _scratchSurfaceSprite != null)
				{
					Card.SurfaceMaterial.mainTexture = _scratchSurfaceSprite.texture;
				}
			}
			
			if (RenderType == ScratchCardRenderType.SpriteRenderer && SpriteRendererCard != null)
			{
				if (Card.SurfaceMaterial != null)
				{
					SpriteRendererCard.sharedMaterial = Card.SurfaceMaterial;
				}
				if (sprite != null)
				{
					SpriteRendererCard.sprite = sprite;
				}
			}
			
			if (RenderType == ScratchCardRenderType.CanvasRenderer && CanvasRendererCard != null)
			{
				if (Card.SurfaceMaterial != null)
				{
					CanvasRendererCard.material = Card.SurfaceMaterial;
				}
				if (sprite != null)
				{
					CanvasRendererCard.sprite = sprite;
				}
			}
		}

		private void UpdateCardOffset()
		{
			if (Card.SurfaceMaterial != null)
			{
				if (RenderType == ScratchCardRenderType.MeshRenderer && _scratchSurfaceSprite != null)
				{
					var offset = new Vector4(
						_scratchSurfaceSprite.textureRect.min.x / _scratchSurfaceSprite.texture.width,
						_scratchSurfaceSprite.textureRect.min.y / _scratchSurfaceSprite.texture.height,
						_scratchSurfaceSprite.textureRect.width / _scratchSurfaceSprite.texture.width,
						_scratchSurfaceSprite.textureRect.height / _scratchSurfaceSprite.texture.height);
					Card.SurfaceMaterial.SetVector(Constants.MaskShader.Offset, offset);
				}
				else
				{
					Card.SurfaceMaterial.SetVector(Constants.MaskShader.Offset, new Vector4(0, 0, 1, 1));
				}
			}
		}

		private void InitBrushMaterial()
		{
			if (Card.BrushMaterial == null)
			{
				Card.BrushMaterial = new Material(brushShader);
			}
			Card.BrushMaterial.mainTexture = _brushTexture;
			Card.BrushMaterial.color = new Color(1, 1, 1, _brushOpacity);
		}

		private void InitProgressMaterial()
		{
			if (_progress == null)
			{
                return;
            }
			
			if (_progress.ProgressMaterial == null)
			{
                Material progressMaterial = new Material(maskProgressShader);
                _progress.ProgressMaterial = progressMaterial;
                _progress.IsSampleSourceTexture = _isScratchSurfaceSpriteHasAlpha;
			}

            _progress.ProgressAccuracy = _progressAccuracy;
			SetProgressSourceTexture();
		}

		private void SetProgressSourceTexture()
		{
			if (_isScratchSurfaceSpriteHasAlpha == true)
			{
				if (scratchTexture != null)
				{
                    _progress.ProgressMaterial.SetTexture(Constants.ProgressShader.SourceTexture, scratchTexture);
				}
				else if (_scratchSurfaceSprite != null)
				{
                    _progress.ProgressMaterial.SetTexture(Constants.ProgressShader.SourceTexture, _scratchSurfaceSprite.texture);
				}
			}
		}

		private void UpdateProgressMaterial()
		{
			if (_progress != null)
			{
				if (_progress.ProgressMaterial != null)
				{
					SetProgressSourceTexture();
				}

				if (Application.isPlaying && spritePixels != null)
				{
                    _progress.SetSpritePixels(spritePixels);
					spritePixels = null;
				}
			}
		}
		
		public void FillScratchCard()
		{
			Card.Fill(false);
			if (_progress != null)
			{
                _progress.UpdateProgress();
			}
		}

		public void ClearScratchCard()
		{
			Card.Clear(false);
			if (_progress != null)
			{
                _progress.UpdateProgress();
			}
		}

		public bool TrySelectCard(ScratchCardRenderType renderType)
		{
			RenderType = renderType;
			var cards = new Dictionary<ScratchCardRenderType, Component>
			{
				{ ScratchCardRenderType.MeshRenderer, MeshRendererCard },
				{ ScratchCardRenderType.SpriteRenderer, SpriteRendererCard },
				{ ScratchCardRenderType.CanvasRenderer, CanvasRendererCard }
			};
			foreach (var card in cards)
			{
				var isActive = card.Key == RenderType;
				if (card.Value != null)
				{
					card.Value.gameObject.SetActive(isActive);
					if (isActive)
					{
						Card.SurfaceTransform = card.Value.transform;
					}
				}
				else if (isActive)
				{
					Debug.LogError($"{card.Key} Card is null! Assign reference to the component in Inspector");
					return false;
				}
			}
			return true;
		}

		public void SetNativeSize()
		{
			var cardRenderType = RenderType;
			if (cardRenderType == ScratchCardRenderType.MeshRenderer)
			{
				if (MeshRendererCard != null && MeshRendererCard.sharedMaterial != null && MeshRendererCard.sharedMaterial.mainTexture != null)
				{
					float width, height;
					if (_scratchSurfaceSprite != null)
					{
						width = _scratchSurfaceSprite.rect.width;
						height = _scratchSurfaceSprite.rect.height;
					}
					else
					{
						var texture = MeshRendererCard.sharedMaterial.mainTexture;
						width = texture.width;
						height = texture.height;
					}

					var meshCardTransform = MeshRendererCard.transform;
					meshCardTransform.localScale = new Vector3(width / Constants.General.PixelsPerUnit, 
						height / Constants.General.PixelsPerUnit, meshCardTransform.localScale.z);
				}
			}
			else if (cardRenderType == ScratchCardRenderType.SpriteRenderer)
			{
				if (SpriteRendererCard != null)
				{
					SpriteRendererCard.transform.localScale = Vector3.one;
				}
			}
			else if (cardRenderType == ScratchCardRenderType.CanvasRenderer)
			{
				if (CanvasRendererCard != null)
				{
					CanvasRendererCard.SetNativeSize();
				}
			}
		}
	}
}