// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Projector" {
    Properties {
        
       
          _Distance ("Distance", Float) = 0.0
        _Transparency("Transparency",Range (0,1.0)) = 0.5
        _Color ("Color", Color) = (1,1,1,1)
        _ShadowTex ("Projected Image", 2D) = "white" {}
    }
    SubShader {
        Pass {  
        Tags {"Queue"="Transparent"}
         ZWrite Off
			Fog { Color (1, 1, 1) }
			AlphaTest Greater 0
			ColorMask RGB
			Blend DstColor Zero
			Offset -1, -1
            CGPROGRAM
 
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
       
          
            fixed _Distance;
            fixed _Transparency;
            fixed4 _Color;
            uniform sampler2D _ShadowTex;
 
            uniform fixed4x4 unity_Projector; // transformation matrix
 
            struct vertexInput {
                fixed4 vertex : POSITION;
            };
       
            struct vertexOutput {
                fixed4 pos : SV_POSITION;
                fixed4 posProj : TEXCOORD0;
            };
 
            vertexOutput vert(vertexInput input) {
                vertexOutput output;
                output.posProj = mul(unity_Projector, input.vertex);
                output.pos = UnityObjectToClipPos(input.vertex);
                return output;
            }
 
            fixed4 frag(vertexOutput input) : COLOR{
               // if (input.posProj.w > 0.0)  { // in front of projector?
              //      fixed2 anim;
           
            
                    
                    return  _Color*lerp(fixed4(1,1,1,0),fixed4(tex2D(_ShadowTex ,(fixed2(input.posProj.xy) / input.posProj.w) - fixed2(0.0, _Distance))),_Transparency);
              //  }
               // else { // behind projector
                    //return fixed4(0.0,0.0,0.0,0.0);
                //}
            }
 
            ENDCG
        }
    }
   // Fallback "Projector/Multiply"
}