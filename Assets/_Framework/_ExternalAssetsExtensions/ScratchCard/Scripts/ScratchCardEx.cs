using System;
using ScratchCardAsset.Core;
using ScratchCardAsset.Core.ScratchData;
using UnityEngine;
using UnityEngine.Rendering;

namespace _KMH_Framework
{
	[System.Serializable]
	public class ScratchCardEx 
	{
		[HideInInspector]
		public ScratchCardHandlerEx _Handler;
        public ScratchCardInputEx _Input;
        public ScratchCardRendererEx CardRenderer;

        public BaseData ScratchData;

        public event Action<ScratchCardEx> OnInitialized;
		public event Action<RenderTexture> OnRenderTextureInitialized;
		public event Action<Vector2, float> OnScratchHole;
		public event Action<Vector2, float> OnScratchHoleSucceed;
		public event Action<Vector2, float, Vector2, float> OnScratchLine;
		public event Action<Vector2, float, Vector2, float> OnScratchLineSucceed;

		public Transform SurfaceTransform;
		public Material SurfaceMaterial;
		public Material BrushMaterial;

		[Min(0.001f)]
		public float BrushSize = 1f;
		public Quality RenderTextureQuality = Quality.High;

		protected RenderTexture _renderTexture;
		public RenderTexture RenderTexture
		{
			get
			{
				return _renderTexture;
			}
			protected set
			{
				_renderTexture = value;
			}
		}

		protected RenderTargetIdentifier _renderTarget;
        public RenderTargetIdentifier RenderTarget
		{
			get
			{
				return _renderTarget;
            }
			protected set
			{
				_renderTarget = value;
            }
		}

		[SerializeField]
		private ScratchMode _mode = ScratchMode.Erase;
		public ScratchMode Mode
		{
			get
			{
				return _mode;
            }
			set
			{
				_mode = value;
				if (BrushMaterial != null)
				{
					BlendOp blendOp;
					if (_mode == ScratchMode.Erase)
					{
						blendOp = BlendOp.Add;
                    }
					else
					{
						blendOp = BlendOp.ReverseSubtract;
                    }

					BrushMaterial.SetInt(Constants.BrushShader.BlendOpShaderParam, (int)blendOp);
				}
			}
		}

		public bool IsScratched
		{
			get
			{
				if (CardRenderer != null)
				{
					return CardRenderer.IsScratched;
				}
				return false;
			}
			protected set
			{
                CardRenderer.IsScratched = value;
            }
		}

		public bool IsScratching
		{
			get
			{
				return _Input.IsScratching;
            }
		}

		public bool IsInitialized = false;

		public void Init(ScratchCardHandlerEx _handler)
		{
			this._Handler = _handler;

            UnsubscribeFromEvents();
			_Input = new ScratchCardInputEx(delegate { return IsScratched; });

			SubscribeToEvents();
			CardRenderer?.Release();
			CardRenderer = new ScratchCardRendererEx(this);
			RenderTexture destroyTarget = ReleaseRenderTexture();
			_handler.DestroyRenderTexture(destroyTarget);

			CreateRenderTexture();
			CardRenderer.FillRenderTextureWithColor(Color.clear);
			IsInitialized = true;
			OnInitialized?.Invoke(this);
        }

		public void SetRenderType(ScratchCardRenderType renderType, Camera mainCamera)
		{
			if (renderType == ScratchCardRenderType.MeshRenderer)
			{
				ScratchData = new MeshRendererData(SurfaceTransform, mainCamera);
			}
			else if (renderType == ScratchCardRenderType.SpriteRenderer)
			{
				ScratchData = new SpriteRendererData(SurfaceTransform, mainCamera);
			}
			else
			{
				ScratchData = new ImageData(SurfaceTransform, mainCamera);
			}
		}

		private void SubscribeToEvents()
		{
			UnsubscribeFromEvents();
			_Input.OnScratch += ScratchData.GetScratchPosition;
			_Input.OnScratchHole += TryScratchHole;
			_Input.OnScratchLine += TryScratchLine;
		}
		
		public void UnsubscribeFromEvents()
		{
			if (_Input == null)
			{
                return;
            }

			if (ScratchData == null)
			{
				SetRenderType(_Handler.RenderType, _Handler._Camera);
            }

			_Input.OnScratch -= ScratchData.GetScratchPosition;
			_Input.OnScratchHole -= TryScratchHole;
			_Input.OnScratchLine -= TryScratchLine;
		}

		private void CreateRenderTexture()
		{
			float qualityRatio = (float)RenderTextureQuality;
			Vector2 renderTextureSize = new Vector2(ScratchData.TextureSize.x / qualityRatio, ScratchData.TextureSize.y / qualityRatio);

			RenderTextureFormat renderTextureFormat;
			if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8) == true)
			{
				renderTextureFormat = RenderTextureFormat.R8;
            }
			else
			{
				renderTextureFormat = RenderTextureFormat.ARGB32;
            }
            RenderTexture = new RenderTexture((int)renderTextureSize.x, (int)renderTextureSize.y, 0, renderTextureFormat);

			SurfaceMaterial.SetTexture(Constants.MaskShader.MaskTexture, RenderTexture);
			RenderTarget = new RenderTargetIdentifier(RenderTexture);

			OnRenderTextureInitialized.Invoke(RenderTexture);
		}
		
		public RenderTexture ReleaseRenderTexture()
		{
			if (RenderTexture != null && RenderTexture.IsCreated() == true)
			{
				RenderTexture.Release();
				// Destroy(RenderTexture);
				RenderTexture destroyTarget = RenderTexture;
				RenderTexture = null;

				return destroyTarget;
            }

			return null;
		}

		private void OnScratchStart()
		{
			CardRenderer.IsScratched = false;
		}

		private void TryScratchHole(Vector2 position, float pressure)
		{
			CardRenderer.ScratchHole(position, pressure);
			Vector2 localPosition = ScratchData.GetLocalPosition(position);
			OnScratchHole?.Invoke(localPosition, pressure);
			if (IsScratched == true)
			{
				OnScratchHoleSucceed?.Invoke(localPosition, pressure);
			}
		}

		private void TryScratchLine(Vector2 startPosition, float startPressure, Vector2 endPosition, float endPressure)
		{
			CardRenderer.ScratchLine(startPosition, endPosition, startPressure, endPressure);

			Vector2 startLocalPosition = ScratchData.GetLocalPosition(startPosition);
            Vector2 endLocalPosition = ScratchData.GetLocalPosition(endPosition);

			OnScratchLine?.Invoke(startLocalPosition, startPressure, endLocalPosition, endPressure);
			if (IsScratched == true)
			{
				OnScratchLineSucceed?.Invoke(startLocalPosition, startPressure, endLocalPosition, endPressure);
			}
		}

		public void Fill(bool isScratched = true)
		{
			CardRenderer.FillRenderTextureWithColor(Color.white);
			if (isScratched == true)
			{
				IsScratched = true;
			}
		}

		public void Clear(bool isScratched = true)
		{
			CardRenderer.FillRenderTextureWithColor(Color.clear);
			if (isScratched == true)
			{
				IsScratched = true;
			}
		}

		public RenderTexture ResetRenderTexture()
		{
            RenderTexture destroyTarget = ReleaseRenderTexture();
            CreateRenderTexture();

			CardRenderer.FillRenderTextureWithColor(Color.clear);
			IsScratched = true;

			return destroyTarget;
        }

		public void ScratchHole(Vector2 position, float pressure = 1f)
		{
			CardRenderer.ScratchHole(position, pressure);
			Vector2 localPosition = ScratchData.GetLocalPosition(position);

			OnScratchHole?.Invoke(localPosition, pressure);
			if (IsScratched == true)
			{
				OnScratchHoleSucceed?.Invoke(localPosition, pressure);
			}
		}

		public void ScratchLine(Vector2 startPosition, Vector2 endPosition, float startPressure = 1f, float endPressure = 1f)
		{
			CardRenderer.ScratchLine(startPosition, endPosition, startPressure, endPressure);

			Vector2 startLocalPosition = ScratchData.GetLocalPosition(startPosition);
            Vector2 endLocalPosition = ScratchData.GetLocalPosition(endPosition);

			OnScratchLine?.Invoke(startLocalPosition, startPressure, endLocalPosition, endPressure);
			if (IsScratched == true)
			{
				OnScratchLineSucceed?.Invoke(startLocalPosition, startPressure, endLocalPosition, endPressure);	
			}
		}

		public Texture2D GetScratchTexture()
		{
            RenderTexture previousRenderTexture = RenderTexture.active;
            Texture2D texture2D = new Texture2D(RenderTexture.width, RenderTexture.height, TextureFormat.ARGB32, false);

			RenderTexture.active = RenderTexture;
			texture2D.ReadPixels(new Rect(0, 0, texture2D.width, texture2D.height), 0, 0, false);
			texture2D.Apply();

			RenderTexture.active = previousRenderTexture;

			return texture2D;
		}
	}
}