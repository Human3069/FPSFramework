﻿Shader "Scope Pro/Red Dot Effect" {
    Properties {
        _Color ("Glass Color", Color) = (1,1,1,1)
        _RedDotColor ("Emission Color", Color) = (1,1,1,1)
        _EmissionTexture ("Emission Texture (A)", 2D) = "white" {}
        _EmissionStrength ("Emission Strength", Range(0,1000)) = 1.0
        _RedDotSize ("Size", Range(0,30)) = 0.0
        _RedDotDist ("Distance Offset", Range(0,50)) = 2.0
        _OffsetX ("Horizontal Offset", Float) = 0.0
        _OffsetY ("Vertical Offset", Float) = 0.0
    }
 
    SubShader {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent"}
        Blend One One
       
        Pass {
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                       
                #include "UnityCG.cginc"
 
                struct appdata_t {
                     float4 vertex : POSITION;
                     float2 uv : TEXCOORD0;
                };
 
                struct v2f {
                    float4 vertex : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    float2 RedDotUV : TEXCOORD1;
                };
 
                fixed4 _Color;
                sampler2D _EmissionTexture;
                fixed4 _RedDotColor;
                fixed _RedDotSize;
                fixed _RedDotDist;
                fixed _OffsetX;
                fixed _OffsetY;
                fixed _EmissionStrength; 
                       
                v2f vert (appdata_t v) {
                    v2f o = (v2f)0;
                    o.vertex = UnityObjectToClipPos(v.vertex);

                    // 여기서부터
                    fixed3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    fixed3 viewDir = _WorldSpaceCameraPos - worldPos;
            
                    fixed3 objectCenter = mul(unity_ObjectToWorld, fixed4(0,0,0,1));
                    fixed dist = length(objectCenter - _WorldSpaceCameraPos);

                    o.RedDotUV = v.vertex.xy - fixed2(_OffsetX, _OffsetY);
                    o.RedDotUV -= mul(unity_WorldToObject, viewDir).xy * _RedDotDist;
                    o.RedDotUV /= (_RedDotSize * dist * 100);
                    o.RedDotUV += fixed2(0.5, 0.5);
                    // 여기까지

                    return o;
                }
                       
                    fixed4 frag (v2f i): COLOR {
                    fixed4 col = _Color;
                    fixed redDot = tex2D (_EmissionTexture, i.RedDotUV).a;
                    col.rgb += redDot * _RedDotColor.rgb * _RedDotColor.a;
                    col.a += redDot * _RedDotColor.a;
                    col.rgb += redDot * _RedDotColor.rgb * _EmissionStrength; 

                    return col;
                }
            ENDCG
        }
    }
}