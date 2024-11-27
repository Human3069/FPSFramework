using System.Collections;
using _KMH_Framework._TS_Module;
using geniikw.DataRenderer2D;
using ScratchCardAsset.Core;
using ScratchCardAsset.Tools;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace _KMH_Framework
{
	[System.Serializable]
	public class EraseProgressEx
	{
		private ScratchCardHandlerEx _handler;
		private ScratchCardEx _card;

		[HideInInspector]
		public Material ProgressMaterial;
        [HideInInspector]
        public bool IsSampleSourceTexture;

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
				UpdateAccuracy();
				if (_progressAccuracy == ProgressAccuracy.Default)
				{
					IsUpdateProgress = false;
					if (pixelsBuffer.IsCreated)
					{
						if (isCalculating == true)
						{
							AsyncGPUReadback.WaitAllRequests();
						}
						pixelsBuffer.Dispose();
					}
				}
			}
		}

		public UnityEvent<float> _OnValueChangedScratchProgressEvent;
		public UnityEvent<float> _OnDoneScratchProgressEvent;

        private float _progressValue;

        [HideInInspector]
        public ScratchMode ScratchMode;

		private NativeArray<byte> pixelsBuffer;
		private int asyncGPUReadbackFrame;
		private int updateProgressFrame;
		private Color[] sourceSpritePixels;
		private CommandBuffer commandBuffer;

        [HideInInspector]
        public Mesh _Mesh;
        [HideInInspector]
        public RenderTexture PercentRenderTexture;

		private RenderTargetIdentifier percentTargetIdentifier;
		private Rect percentTextureRect;

		private Texture2D progressTexture;

		private int bitsPerPixel = 1;

		[HideInInspector]
		public bool IsUpdateProgress;
		private bool isCalculating;

		[ReadOnly]
		public bool IsCompleted;

        public void Initialize(ScratchCardHandlerEx handler, ScratchCardEx card)
        {
			this._handler = handler;
            this._card = card;

            if (_card.IsInitialized == true)
            {
                OnCardRenderTextureInitialized(_card.RenderTexture);
            }

            _card.OnRenderTextureInitialized += OnCardRenderTextureInitialized;
            UpdateAccuracy();

            ScratchMode = _card.Mode;
            commandBuffer = new CommandBuffer { name = "EraseProgress" };

            _Mesh = MeshGenerator.GenerateQuad(Vector3.one, Vector3.zero);

            RenderTextureFormat renderTextureFormat;
            if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8) == true)
            {
                renderTextureFormat = RenderTextureFormat.R8;
            }
            else
            {
                renderTextureFormat = RenderTextureFormat.ARGB32;
            }
            PercentRenderTexture = new RenderTexture(1, 1, 0, renderTextureFormat);

            percentTargetIdentifier = new RenderTargetIdentifier(PercentRenderTexture);
            percentTextureRect = new Rect(0, 0, PercentRenderTexture.width, PercentRenderTexture.height);

            TextureFormat textureFormat = SystemInfo.SupportsTextureFormat(TextureFormat.R8) ? TextureFormat.R8 : TextureFormat.ARGB32;
            if (SystemInfo.SupportsTextureFormat(TextureFormat.R8) == true)
            {
                textureFormat = TextureFormat.R8;
            }
            else
            {
                textureFormat = TextureFormat.ARGB32;
            }
            progressTexture = new Texture2D(PercentRenderTexture.width, PercentRenderTexture.height, textureFormat, false, true);
        }

        public void DeInitialize()
		{
			if (_progressAccuracy == ProgressAccuracy.High && isCalculating)
			{
				AsyncGPUReadback.WaitAllRequests();
			}
			
			if (pixelsBuffer.IsCreated == true)
			{
				pixelsBuffer.Dispose();
			}

			if (commandBuffer != null)
			{
				commandBuffer.Release();
				commandBuffer = null;
			}

			if (_card != null)
			{
				_card.OnRenderTextureInitialized -= OnCardRenderTextureInitialized;
			}
		}

		private void OnCardRenderTextureInitialized(RenderTexture renderTexture)
		{
			if (renderTexture.format == RenderTextureFormat.R8)
			{
				bitsPerPixel = 1;
            }
			else
			{
				bitsPerPixel = 4;
            }
		}

		private void UpdateAccuracy()
		{
			if (_progressAccuracy == ProgressAccuracy.High && !SystemInfo.supportsAsyncGPUReadback)
			{
				Debug.LogWarning("AsyncGPUReadback is not supported! Switching to ProgressAccuracy.Default.");

				_progressAccuracy = ProgressAccuracy.Default;
			}
		}

		private IEnumerator CalculateProgressValueRoutine()
		{
			if (IsCompleted == false && isCalculating == false)
			{
				isCalculating = true;
				if (_progressAccuracy == ProgressAccuracy.High)
				{
					if (!pixelsBuffer.IsCreated)
					{
						var length = _card.RenderTexture.width * _card.RenderTexture.height * bitsPerPixel;
						pixelsBuffer = new NativeArray<byte>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
					}

					asyncGPUReadbackFrame = Time.frameCount;

					AsyncGPUReadbackRequest _gpuRequest = AsyncGPUReadback.RequestIntoNativeArray(ref pixelsBuffer, _card.RenderTexture);
					while (_gpuRequest.done == false)
					{
						yield return null;
					}

					if (_gpuRequest.hasError == true)
					{
						isCalculating = false;
						IsUpdateProgress = false;
						Debug.LogError("GPU readback error detected.");
						yield break;
					}
					
					_progressValue = 0f;
					if (IsSampleSourceTexture)
					{
						float totalAlpha = 0f;
						for (int i = 0; i < pixelsBuffer.Length; i += bitsPerPixel)
						{
                            float sourceAlpha = sourceSpritePixels[i / bitsPerPixel].a;
							totalAlpha += sourceAlpha;
							_progressValue += pixelsBuffer[i] / 255f * sourceAlpha;
						}

                        float div = pixelsBuffer.Length / (float)bitsPerPixel;
						totalAlpha /= div;

						_progressValue /= div;
						_progressValue /= totalAlpha;
					}
					else
					{
						for (var i = 0; i < pixelsBuffer.Length; i += bitsPerPixel)
						{
							_progressValue += pixelsBuffer[i] / 255f;
						}
						_progressValue /= pixelsBuffer.Length / (float)bitsPerPixel;
					}

					if (asyncGPUReadbackFrame > updateProgressFrame)
					{
						IsUpdateProgress = false;
					}
				}
				else if (_progressAccuracy == ProgressAccuracy.Default)
				{
					RenderTexture prevRenderTexture = RenderTexture.active;
					RenderTexture.active = PercentRenderTexture;

					progressTexture.ReadPixels(percentTextureRect, 0, 0);
					progressTexture.Apply();

					RenderTexture.active = prevRenderTexture;
					Color pixelColor = progressTexture.GetPixel(0, 0);
					_progressValue = pixelColor.r;
				}
				
				_OnValueChangedScratchProgressEvent.Invoke(_progressValue);
				if (_OnDoneScratchProgressEvent != null)
                {
					float completeValue;
                    if (_card.Mode == ScratchMode.Erase)
					{
						completeValue = 1f;
                    }
					else
					{
						completeValue = 0f;
                    }

					if (Mathf.Abs(_progressValue - completeValue) < float.Epsilon)
					{
                        _OnDoneScratchProgressEvent.Invoke(_progressValue);
						IsCompleted = true;
					}
				}
				isCalculating = false;
			}
			else if (_progressAccuracy == ProgressAccuracy.High && isCalculating == true && _card.IsScratched == true)
			{
				IsUpdateProgress = true;
				updateProgressFrame = Time.frameCount;
			}
		}
		
		public float GetProgress()
		{
			return _progressValue;
		}

		public void UpdateProgress()
		{
			if (commandBuffer == null)
			{
				Debug.LogError("Can't update progress cause commandBuffer is null!");
				return;
			}
			
			GL.LoadOrtho();
			commandBuffer.Clear();
			commandBuffer.SetRenderTarget(percentTargetIdentifier);
			commandBuffer.ClearRenderTarget(false, true, Color.clear);

			int pass;
			if (IsSampleSourceTexture == true)
			{
				pass = 1;
			}
			else
			{
				pass = 0;
			}

			commandBuffer.DrawMesh(_Mesh, Matrix4x4.identity, ProgressMaterial, 0, pass);
			Graphics.ExecuteCommandBuffer(commandBuffer);

            if (_handler.gameObject.activeInHierarchy == true)
            {
                _handler.StartCoroutine(CalculateProgressValueRoutine());
            }
        }

		public void ResetProgress()
		{
			IsCompleted = false;
		}

		public void SetSpritePixels(Color[] pixels)
		{
			sourceSpritePixels = pixels;
		}
	}
}