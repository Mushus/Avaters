Shader "Mushus/CharaCreatorV0/ImageBlendNormal"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
        _DstTex ("Dst Texture", 2D) = "black" {}
        _Color ("Color", Color) = (1,1,1,1)
        _BlendMode ("Blend Mode", Int) = 0
        _Opacity ("Opacity", Range(0, 1)) = 1
        _Offset ("Offset", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        Cull Off ZWrite Off ZTest Off
        Blend SrcAlpha OneMinusSrcAlpha 

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _DstTex;
            float4 _Color;
            int _BlendMode;
            float _Opacity;
            float4 _Offset;

            inline float4 blend(float4 dstCol, float4 srcCol, float3 blend)
            {
                float4 col;
                float ao = dstCol.a * srcCol.a + dstCol.a * (1 - srcCol.a) + srcCol.a * (1 - dstCol.a);
                if (ao > 0) {
                    col = float4(dstCol.a * srcCol.a * blend + dstCol.a * (1 - srcCol.a) * dstCol.rgb + srcCol.a * (1 - dstCol.a) * srcCol.rgb / ao, ao);
                }
                return col;
            }

            inline float multiply(float cb, float cs)
            {
                return cb * cs;
            }

            inline float additive(float cb, float cs)
            {
                return min(1, cb + cs);
            }

            inline float screen(float cb, float cs)
            {
                return cb + cs - cb * cs;
            }

            inline float hardLight(float cb, float cs)
            {
                return cs <= 0.5 ? multiply(cb, cs * 2) : screen(cb, cs * 2 - 1);
            }

            inline float overlay(float cb, float cs)
            {
                return hardLight(cs, cb);
            }

            inline float darken(float cb, float cs)
            {
                return min(cb, cs);
            }

            inline float lighten(float cb, float cs)
            {
                return max(cb, cs);
            }

            inline float colorDodge(float cb, float cs)
            {
                return cb == 0 ? 0 : cs == 1 ? 1 : min(1, cb / (1 - cs));
            }

            inline float colorBurn(float cb, float cs)
            {
                return cb == 1 ? 1 : cs == 0 ? 0 : 1 - min(1, (1 - cb) / cs);
            }

            inline float softLight(float cb, float cs)
            {
                float d = cb <= 0.25 ? ((16 * cb - 12) * cb + 4) * cb : sqrt(cb);
                return cs <= 0.5 ? cb - (1 - 2 * cs) * cb * (1 - cb) : cb + (2 * cs - 1) * (d - cb);
            }

            inline float difference(float cb, float cs)
            {
                return abs(cb - cs);
            }

            inline float exclusion(float cb, float cs)
            {
                return cb + cs - 2 * cb * cs;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 sharderOffset = float4(_Offset.x, 1 - _Offset.y - _Offset.w, _Offset.z, _Offset.w);
                float2 uv = (i.uv - sharderOffset.xy) / sharderOffset.zw;
                float4 srcCol = tex2D(_MainTex, uv);
                float4 dstCol = tex2D(_DstTex, i.uv);

                if (uv.x < 0 || 1 < uv.x || uv.y < 0 || 1 < uv.y) {
                    srcCol.a = 0;
                }

                srcCol = float4(srcCol.rgb * _Color.rgb, srcCol.a * _Color.a * _Opacity);
                
                float4 col;
                if (_BlendMode == 1) {
                    // Multiply
                    col = blend(dstCol, srcCol, float3(multiply(dstCol.r, srcCol.r), multiply(dstCol.g, srcCol.g), multiply(dstCol.b, srcCol.b)));
                } else if (_BlendMode == 2) {
                    // Additive
                    col = blend(dstCol, srcCol, float3(additive(dstCol.r, srcCol.r), additive(dstCol.g, srcCol.g), additive(dstCol.b, srcCol.b)));
                }
                else if (_BlendMode == 3) {
                    // Screen
                    col = blend(dstCol, srcCol, float3(screen(dstCol.r, srcCol.r), screen(dstCol.g, srcCol.g), screen(dstCol.b, srcCol.b)));
                } else if (_BlendMode == 4) {
                    // Overlay
                    col = blend(dstCol, srcCol, float3(overlay(dstCol.r, srcCol.r), overlay(dstCol.g, srcCol.g), overlay(dstCol.b, srcCol.b)));
                } else if (_BlendMode == 5) {
                    // Darken
                    col = blend(dstCol, srcCol, float3(darken(dstCol.r, srcCol.r), darken(dstCol.g, srcCol.g), darken(dstCol.b, srcCol.b)));
                } else if (_BlendMode == 6) {
                    // Lighten
                    col = blend(dstCol, srcCol, float3(lighten(dstCol.r, srcCol.r), lighten(dstCol.g, srcCol.g), lighten(dstCol.b, srcCol.b)));
                } else if (_BlendMode == 7) {
                    // color-dodge
                    col = blend(dstCol, srcCol, float3(colorDodge(dstCol.r, srcCol.r), colorDodge(dstCol.g, srcCol.g), colorDodge(dstCol.b, srcCol.b)));
                } else if (_BlendMode == 8) {
                    // color-burn
                    col = blend(dstCol, srcCol, float3(colorBurn(dstCol.r, srcCol.r), colorBurn(dstCol.g, srcCol.g), colorBurn(dstCol.b, srcCol.b)));
                } else if (_BlendMode == 9) {
                    // hard-light
                    col = blend(dstCol, srcCol, float3(hardLight(dstCol.r, srcCol.r), hardLight(dstCol.g, srcCol.g), hardLight(dstCol.b, srcCol.b)));
                } else if (_BlendMode == 10) {
                    // soft-light
                    col = blend(dstCol, srcCol, float3(softLight(dstCol.r, srcCol.r), softLight(dstCol.g, srcCol.g), softLight(dstCol.b, srcCol.b)));
                } else if (_BlendMode == 11) {
                    // difference
                    col = blend(dstCol, srcCol, float3(difference(dstCol.r, srcCol.r), difference(dstCol.g, srcCol.g), difference(dstCol.b, srcCol.b)));
                } else if (_BlendMode == 12) {
                    // exclusion
                    col = blend(dstCol, srcCol, float3(exclusion(dstCol.r, srcCol.r), exclusion(dstCol.g, srcCol.g), exclusion(dstCol.b, srcCol.b)));
                } else {
                    // Normal
                    col = blend(dstCol, srcCol, srcCol);
                }

                return col;
            }
            ENDCG
        }
    }
}
