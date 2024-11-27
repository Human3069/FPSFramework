using EPOOutline;
using UnityEngine;

namespace VTSFramework.TSModule
{
    [System.Serializable]
    public class _3DHighlightableData
    {
        [System.Serializable]
        public class PairParameter
        {
            public bool isEnabled;
            public Color _Color;
            public float DilateShift;
            public float BlurShift;

            public SerializedPass FillPass;
        }

        [Header("Initialize Datas")]
        public float FlickeringDuration = 0.5f;

        [Header("Component Datas")]
        public ComplexMaskingMode _ComplexMaskingMode;
        public OutlinableDrawingMode _OutlinableDrawingMode;
        public int OutlineLayer = 0;
        public RenderStyle _RenderStyle;

        [Space(10)]
        public PairParameter BackParameter;
        public PairParameter FrontParameter;

        public void InitializeProperties(_3DHighlight _outlinable)
        {
            _outlinable.FlickeringDuration = this.FlickeringDuration;

            _outlinable.ComplexMaskingMode = this._ComplexMaskingMode;
            _outlinable.DrawingMode = this._OutlinableDrawingMode;
            _outlinable.OutlineLayer = this.OutlineLayer;
            _outlinable.RenderStyle = this._RenderStyle;

            _outlinable.BackParameters.Enabled = this.BackParameter.isEnabled;

            Color backColor = this.BackParameter._Color;
            backColor.a = 0f;
            _outlinable.BackParameters.Color = backColor;

            _outlinable.BackParameters.DilateShift = this.BackParameter.DilateShift;
            _outlinable.BackParameters.BlurShift = this.BackParameter.BlurShift;
            _outlinable.BackParameters.FillPass.Shader = this.BackParameter.FillPass.Shader;

            Color backPublicColor = this.BackParameter.FillPass.GetColor("_PublicColor");
            backPublicColor.a = 0f;
            _outlinable.BackParameters.FillPass.SetColor("_PublicColor", backPublicColor);

            _outlinable.FrontParameters.Enabled = this.FrontParameter.isEnabled;

            Color frontColor = this.FrontParameter._Color;
            frontColor.a = 0f;
            _outlinable.FrontParameters.Color = frontColor;

            _outlinable.FrontParameters.DilateShift = this.FrontParameter.DilateShift;
            _outlinable.FrontParameters.BlurShift = this.FrontParameter.BlurShift;
            _outlinable.FrontParameters.FillPass.Shader = this.FrontParameter.FillPass.Shader;

            Color frontPublicColor = this.BackParameter.FillPass.GetColor("_PublicColor");
            frontPublicColor.a = 0f;
            _outlinable.FrontParameters.FillPass.SetColor("_PublicColor", frontPublicColor);
        }
    }
}