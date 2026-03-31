Shader "Custom/MagicRune_Rotating"
{
    Properties
    {
        // Base
        _BaseMap("Rune Texture", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1,1,1,1)

            // HDR Emission
            [HDR]_EmissionColor("Emission Color", Color) = (2,2,2,1)
            _EmissionIntensity("Emission Intensity", Range(0,10)) = 2

            // Dissolve
            _DissolveTex("Dissolve Noise", 2D) = "white" {}
            _Dissolve("Dissolve Amount", Range(0,1)) = 0
            _EdgeWidth("Edge Width", Range(0,0.2)) = 0.05
            [HDR]_EdgeColor("Edge Color", Color) = (5,2,10,1)

                // Rotation
                _RotationSpeed("Rotation Speed", Float) = 1
                _RotationOffset("Rotation Offset", Float) = 0

                // Transparency
                _Alpha("Alpha", Range(0,1)) = 1
    }

        SubShader
            {
                Tags
                {
                    "RenderType" = "Transparent"
                    "Queue" = "Transparent"
                    "RenderPipeline" = "UniversalPipeline"
                }

                Blend SrcAlpha OneMinusSrcAlpha
                ZWrite Off
                Cull Off   // 🔥 Render both faces (important for quads)

                Pass
                {
                    HLSLPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag

                    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

                    struct Attributes
                    {
                        float4 positionOS : POSITION;
                        float2 uv         : TEXCOORD0;
                    };

                    struct Varyings
                    {
                        float4 positionHCS : SV_POSITION;
                        float2 uv          : TEXCOORD0;
                    };

                    sampler2D _BaseMap;
                    sampler2D _DissolveTex;

                    float4 _BaseColor;
                    float4 _EmissionColor;
                    float _EmissionIntensity;

                    float _Dissolve;
                    float _EdgeWidth;
                    float4 _EdgeColor;

                    float _RotationSpeed;
                    float _RotationOffset;

                    float _Alpha;

                    // 🔁 UV Rotation
                    float2 RotateUV(float2 uv, float angle)
                    {
                        float2 center = float2(0.5, 0.5);
                        uv -= center;

                        float s = sin(angle);
                        float c = cos(angle);

                        float2x2 rot = float2x2(c, -s, s, c);
                        uv = mul(rot, uv);

                        uv += center;
                        return uv;
                    }

                    Varyings vert(Attributes v)
                    {
                        Varyings o;
                        o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                        o.uv = v.uv;
                        return o;
                    }

                    half4 frag(Varyings i) : SV_Target
                    {
                        // 🔁 Rotating UV
                        float angle = (_Time.y * _RotationSpeed) + _RotationOffset;
                        float2 uv = RotateUV(i.uv, angle);

                        // Base texture
                        float4 baseTex = tex2D(_BaseMap, uv);
                        float4 col = baseTex * _BaseColor;

                        // Dissolve noise (non-rotating for better effect)
                        float noise = tex2D(_DissolveTex, i.uv).r;

                        // Clip dissolve
                        float dissolveEdge = _Dissolve;
                        clip(noise - dissolveEdge);

                        // Edge glow
                        float edge = smoothstep(dissolveEdge, dissolveEdge + _EdgeWidth, noise);
                        float edgeMask = 1 - edge;

                        float3 edgeEmission = edgeMask * _EdgeColor.rgb;

                        // Emission
                        float3 emission = col.rgb * _EmissionColor.rgb * _EmissionIntensity;

                        float3 finalColor = emission + edgeEmission;

                        return float4(finalColor, col.a * _Alpha);
                    }

                    ENDHLSL
                }
            }
}