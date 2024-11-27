using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VTSFramework.TSModule
{
    public class _2DHighlight : BaseObject, IHighlight
    {
        [SerializeField]
        protected _2DHighlightableData _data; // 안씀, 그러나 추후 사용할 예정

        protected float _highlightDuration;
        public float HighlightDuration
        {
            get
            {
                return _highlightDuration;
            }
            set
            {
                _highlightDuration = value;
            }
        }

        [ReadOnly]
        [SerializeField]
        protected InMissionType _inMissionType;
        public InMissionType _InMissionType
        {
            get
            {
                return _inMissionType;
            }
            set
            {
                _inMissionType = value;
            }
        }

        [SerializeField]
        protected List<Image> highlightImageList = new List<Image>();
        protected Dictionary<Image, float> originAlphaList = new Dictionary<Image, float>();

        protected bool isAwaken = false;

        protected virtual void Awake()
        {
            this.enabled = false;

            if (highlightImageList.Count == 0)
            {
                RectTransform rectT = this.transform as RectTransform;

                GameObject resourcePrefab = Instantiate(Resources.Load("_2DHighlight") as GameObject);
                Image highlightImage = resourcePrefab.GetComponent<Image>();
                highlightImage.transform.SetParent(this.transform);

                highlightImage.rectTransform.anchoredPosition = new Vector2(0f, 0f);
                highlightImage.rectTransform.sizeDelta = rectT.sizeDelta + new Vector2(22f, 22f);

                highlightImageList.Add(highlightImage);
            }

            foreach (Image highlightImage in highlightImageList)
            {
                originAlphaList.Add(highlightImage, highlightImage.color.a);
                highlightImage.gameObject.SetActive(false);
            }

            PostAwake().Forget();
        }

        protected virtual async UniTaskVoid PostAwake()
        {
            await UniTask.WaitWhile(delegate { return HighlightInMission._2DHighlightData == null; });

            this._data = HighlightInMission._2DHighlightData;
            this.isAwaken = true;
        }

        protected virtual void OnEnable()
        {
            PostOnEnable().Forget();
        }

        public virtual async UniTaskVoid PostOnEnable()
        {
            await UniTaskEx.WaitWhile(this, 0, PredicateFunc);
            bool PredicateFunc()
            {
                return isAwaken == false;
            }

            foreach (Image highlightImage in highlightImageList)
            {
                highlightImage.gameObject.SetActive(true);

                Color outColor = _data.FromColor;
                outColor.a = 0f;

                highlightImage.color = outColor;
            }

            float halfOfDuration = _data.FlickeringDuration / 2f;
            foreach (Image highlightImage in highlightImageList)
            {
                highlightImage.DOFade(originAlphaList[highlightImage], halfOfDuration);
            }

            await UniTaskEx.WaitForSeconds(this, 0, halfOfDuration);

            while (true)
            {
                foreach (Image highlightImage in highlightImageList)
                {
                    highlightImage.DOColor(_data.ToColor, halfOfDuration);
                }

                await UniTaskEx.WaitForSeconds(this, 0, halfOfDuration);

                foreach (Image highlightImage in highlightImageList)
                {
                    highlightImage.DOColor(_data.FromColor, halfOfDuration);
                }

                await UniTaskEx.WaitForSeconds(this, 0, halfOfDuration);
            }
        }

        public virtual async UniTaskVoid DisableAsync()
        {
            UniTaskEx.Cancel(this, 0);

            if (_data == null)
            {
                this._data = HighlightInMission._2DHighlightData;
            }

            float halfOfDuration = _data.FlickeringDuration / 2f;
            foreach (Image highlightImage in highlightImageList)
            {
                highlightImage.DOFade(0f, halfOfDuration);
            }

            await UniTaskEx.WaitForSeconds(this, 0, halfOfDuration);

            this.enabled = false;
        }

        protected virtual void OnDisable()
        {
            foreach (Image highlightImage in highlightImageList)
            {
                highlightImage.gameObject.SetActive(false);
            }
        }
    }
}