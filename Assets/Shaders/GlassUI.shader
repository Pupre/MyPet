Shader "Custom/GlassUI"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Main Tint", Color) = (1,1,1,0.2)
        _FrostedTint ("Frosted Tint", Color) = (1,1,1,1)
        _GrainAmount ("Grain Intensity", Range(0, 0.2)) = 0.05
        _EdgeHighlight ("Edge Intensity", Range(0, 5)) = 1.5
        _EdgeWidth ("Edge Width", Range(0, 0.5)) = 0.02
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _Color;
            float4 _FrostedTint;
            float _GrainAmount;
            float _EdgeHighlight;
            float _EdgeWidth;

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            float random (float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
            }

            half4 frag (Varyings input) : SV_Target
            {
                float2 centeredUV = input.uv - 0.5;
                float dist = length(centeredUV) * 2.0;
                
                // Base frosted color
                half4 col = _Color * input.color;
                
                // Add Grain (Noise)
                float noise = (random(input.uv + _Time.y * 0.01) - 0.5) * _GrainAmount;
                col.rgb += noise;
                col.rgb *= _FrostedTint.rgb;

                // Sharp Edge Highlight (Glass Rim)
                float edge = smoothstep(1.0, 1.0 - _EdgeWidth, dist) - smoothstep(1.0 - _EdgeWidth, 1.0 - (_EdgeWidth * 2.0), dist);
                col.rgb += edge * _EdgeHighlight;
                
                // Overall Alpha from texture (for circles)
                half4 tex = tex2D(_MainTex, input.uv);
                col.a *= tex.a;

                return col;
            }
            ENDHLSL
        }
    }
}
