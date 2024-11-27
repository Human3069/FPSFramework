using ScratchCardAsset.Animation;
using ScratchCardAsset.Core;
using UnityEngine;
using UnityEngine.Events;

namespace _KMH_Framework
{
    public class ScratchAnimatorEx : MonoBehaviour
    {
        public ScratchCardHandlerEx Handler;
        // public ScratchCardEx ScratchCard;
        public ScratchAnimation ScratchAnimation;

        [SerializeField]
        protected bool isPlayOnStart = true;

        protected bool _isPlaying;
        public bool IsPlaying
        {
            get
            {
                return _isPlaying;
            }
            protected set
            {
                _isPlaying = value;
            }
        }

        protected int currentScratchIndex;
        protected float progress;
        protected float totalTime;
        protected Vector2? previousPosition;
        protected Vector2 scale = Vector2.one;

        public UnityEvent _OnAnimationPlayEvent;
        public UnityEvent _OnAnimationPauseEvent;
        public UnityEvent _OnAnimationStopEvent;
        public UnityEvent _OnAnimationDoneEvent;

        protected void Start()
        {
            if (ScratchAnimation != null && ScratchAnimation.ScratchSpace == ScratchAnimationSpace.UV)
            {
                scale = Handler.Card.ScratchData.TextureSize;
            }

            if (isPlayOnStart == true)
            {
                Play();
            }
        }

        protected void Update()
        {
            if (IsPlaying == true)
            {
                UpdateScratches();
                totalTime += Time.deltaTime;
            }
        }

        private void UpdateScratches()
        {
            if (ScratchAnimation == null || ScratchAnimation.Scratches.Count == 0)
            {
                return;
            }
                
            BaseScratch _baseScratch = ScratchAnimation.Scratches[currentScratchIndex];
            if (totalTime < _baseScratch.Time)
            {
                return;
            }
            
            if (_baseScratch is LineScratch line)
            {
                float duration = line.TimeEnd - line.Time;
                if (duration == 0f)
                {
                    progress = 1f;
                }
                else
                {
                    progress = totalTime / duration;
                }

                Vector3 position = Vector3.Lerp(line.Position, line.PositionEnd, progress) * scale;
                float pressure = Mathf.Lerp(line.BrushScale, line.BrushScaleEnd, progress);
                if (previousPosition == null)
                {
                    previousPosition = line.Position * scale;
                }
                Handler.Card.ScratchLine(previousPosition.Value, position, pressure, pressure);
                previousPosition = position;
            }
            else
            {
                if (_baseScratch.Time == 0f)
                {
                    progress = 1f;
                }
                else
                {
                    progress = totalTime / _baseScratch.Time;
                }

                if (progress >= 1f)
                {
                    Vector3 position = _baseScratch.Position * scale;
                    float pressure = _baseScratch.BrushScale;
                    Handler.Card.ScratchHole(position, pressure);
                    previousPosition = null;
                }
            }
            
            if (progress >= 1f)
            {
                currentScratchIndex++;
                progress = 0f;
                previousPosition = null;
                if (currentScratchIndex == ScratchAnimation.Scratches.Count)
                {
                    Stop();
                    _OnAnimationDoneEvent.Invoke();
                }
                else
                {
                    UpdateScratches();
                }
            }
        }

        [ContextMenu("Play")]
        public void Play()
        {
            IsPlaying = true;
            _OnAnimationPlayEvent.Invoke();
        }

        [ContextMenu("Pause")]
        public void Pause()
        {
            IsPlaying = false;
            _OnAnimationPauseEvent.Invoke();
        }

        [ContextMenu("Stop")]
        public void Stop()
        {
            IsPlaying = false;
            totalTime = 0f;
            currentScratchIndex = 0;
            progress = 0f;
            previousPosition = null;

            _OnAnimationStopEvent.Invoke();
        }
    }
}