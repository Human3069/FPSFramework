using EPOOutline;
using DG.Tweening;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace VTSFramework.TSModule
{
    public class _3DHighlight : Outlinable, IHighlight
    {
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

        [Space(10)]
        [SerializeField]
        protected float _flickeringDuration;
        public float FlickeringDuration
        {
            get
            {
                return _flickeringDuration;
            }
            set
            {
                _flickeringDuration = value;
            }
        }

        [SerializeField]
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

        #region Shortcut Properties...
        protected Color FrontColor
        {
            get
            {
                return HighlightInMission._3DHighlightData.FrontParameter._Color;
            }
        }

        protected Color FrontParameterFillPassColor
        {
            get
            {
                return HighlightInMission._3DHighlightData.FrontParameter.FillPass.GetColor("_PublicColor");
            }
        }

        protected Color BackColor
        {
            get
            {
                return HighlightInMission._3DHighlightData.BackParameter._Color;
            }
        }

        protected Color BackParameterFillPassColor
        {
            get
            {
                return HighlightInMission._3DHighlightData.BackParameter.FillPass.GetColor("_PublicColor");
            }
        }
        #endregion

        protected virtual void Awake()
        {
            string uidValue = this.GetComponent<UIDObject>().UidValue.ToInMissionText();

            this.AddAllChildRenderersToRenderingList(RenderersAddingMode.MeshRenderer);
            HighlightInMission._3DHighlightData.InitializeProperties(this);
        }

        protected override void OnEnable()
        {
            UniTaskEx.Cancel(this, 0);

            this.FrontParameters.DOKill(true);
            this.FrontParameters.FillPass.DOKill(true);
            this.BackParameters.DOKill(true);
            this.BackParameters.FillPass.DOKill(true);

            Color frontOutColor = FrontColor;
            frontOutColor.a = 0f;
            this.FrontParameters.Color = frontOutColor;

            Color frontFillOutColor = FrontParameterFillPassColor;
            frontFillOutColor.a = 0f;
            this.FrontParameters.FillPass.SetColor("_PublicColor", frontFillOutColor);

            Color backOutColor = BackColor;
            backOutColor.a = 0f;
            this.BackParameters.Color = backOutColor;

            Color backFillOutColor = BackParameterFillPassColor;
            backFillOutColor.a = 0f;
            this.BackParameters.FillPass.SetColor("_PublicColor", backFillOutColor);

            UpdateVisibility();

            FlickeringRoutine().Forget();
            CheckHighlightDuration().Forget();
        }

        protected async UniTaskVoid CheckHighlightDuration()
        {
            if (HighlightDuration == 0f)
            {
                return;
            }

            await UniTask.WaitForSeconds(HighlightDuration);
            DisableAsync().Forget();
        }

        protected async UniTaskVoid FlickeringRoutine()
        {
            float halfOfDuration = FlickeringDuration / 2f;
            while (true)
            {
                this.FrontParameters.DOFade(FrontColor.a, halfOfDuration);
                this.FrontParameters.FillPass.DOFade("_PublicColor", FrontParameterFillPassColor.a, halfOfDuration);
                this.BackParameters.DOFade(BackColor.a, halfOfDuration);
                this.BackParameters.FillPass.DOFade("_PublicColor", BackParameterFillPassColor.a, halfOfDuration);

                await UniTaskEx.WaitForSeconds(this, 0, halfOfDuration);

                this.FrontParameters.DOFade(0f, halfOfDuration);
                this.FrontParameters.FillPass.DOFade("_PublicColor", 0f, halfOfDuration);
                this.BackParameters.DOFade(0f, halfOfDuration);
                this.BackParameters.FillPass.DOFade("_PublicColor", 0f, halfOfDuration);

                await UniTaskEx.WaitForSeconds(this, 0, halfOfDuration);
            }
        }

        public virtual async UniTaskVoid DisableAsync()
        {
            UniTaskEx.Cancel(this, 0);

            float halfOfDuration = FlickeringDuration / 2f;

            this.FrontParameters.DOFade(0f, halfOfDuration);
            this.FrontParameters.FillPass.DOFade("_PublicColor", 0f, halfOfDuration);
            this.BackParameters.DOFade(0f, halfOfDuration);
            this.BackParameters.FillPass.DOFade("_PublicColor", 0f, halfOfDuration);

            await UniTask.WaitForSeconds(halfOfDuration);

            this.enabled = false;
        }

        protected override void OnDisable()
        {
            outlinables.Remove(this);
            UniTaskEx.Cancel(this, 0);
        }
    }
}