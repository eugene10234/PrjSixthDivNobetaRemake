// Barrier.shader — Unity 6 URP
//
// A reusable energy barrier for spheres, quads, or any mesh.
//
// Key features:
//   Color + Alpha     — fully tunable base tint with master alpha
//   Fresnel edge fade — rim brightens/fades at silhouette edges, direction invertible
//   Fade axis         — world-space directional fade (top-to-bottom, side, etc.)
//   Scanline pattern  — scrolling line grid for that classic barrier look
//   Pulse             — animated brightness wave driven by _Time
//   Dissolve          — noise-clipped with emissive edge glow, same as GhostLit
//                       driven by BarrierController via MaterialPropertyBlock
//   Visibility        — master opacity multiplier, separate from dissolve clip
//
// C# usage:
//   _mpb.SetFloat("_Dissolve",    0→1); // noise-clips the barrier away with edge glow
//   _mpb.SetFloat("_Visibility",  0→1); // fades master alpha (no clip, no glow)

Shader "Nobeta/Barrier"
{
    Properties
    {
        // ── Surface ───────────────────────────────────────────────────────────
        [HDR] _Color            ("Barrier Color",        Color)             = (0.2, 0.6, 1.0, 1.0)
        _Alpha                  ("Master Alpha",         Range(0.0, 1.0))   = 0.5

        // ── Fresnel edge ──────────────────────────────────────────────────────
        _FresnelPower           ("Fresnel Power",        Range(0.1, 8.0))   = 2.0
        _FresnelIntensity       ("Fresnel Intensity",    Range(0.0, 4.0))   = 1.5
        // 0 = bright at edges (classic shield), 1 = bright at center
        [Toggle] _InvertFresnel ("Invert Fresnel",       Float)             = 0

        // ── Fade axis ─────────────────────────────────────────────────────────
        // World-space direction along which the barrier fades out.
        // (0,1,0) = fades toward the top.   (0,-1,0) = fades toward the bottom.
        // (1,0,0) = fades toward +X side.   Set to (0,0,0) to disable axis fade.
        _FadeAxis               ("Fade Axis (World)",    Vector)            = (0, 1, 0, 0)
        _FadeStrength           ("Fade Strength",        Range(0.0, 1.0))   = 0.6
        // Bias shifts where the fade starts along the axis (0.5 = midpoint of mesh)
        _FadeBias               ("Fade Bias",            Range(-1.0, 1.0))  = 0.0

        // ── Scanlines ─────────────────────────────────────────────────────────
        _ScanlineTex            ("Scanline Texture",     2D)                = "white" {}
        _ScanlineScale          ("Scanline Scale",       Range(1.0, 50.0))  = 10.0
        _ScanlineSpeed          ("Scanline Scroll Speed",Range(0.0, 5.0))   = 0.3
        _ScanlineIntensity      ("Scanline Intensity",   Range(0.0, 1.0))   = 0.3

        // ── Pulse ─────────────────────────────────────────────────────────────
        _PulseSpeed             ("Pulse Speed",          Range(0.0, 10.0))  = 1.5
        _PulseIntensity         ("Pulse Intensity",      Range(0.0, 1.0))   = 0.2

        // ── Intersection / contact highlight ─────────────────────────────────
        // Brightens where the barrier intersects scene geometry (requires depth texture).
        [HDR] _IntersectColor   ("Intersect Color",      Color)             = (0.4, 0.8, 1.0, 1.0)
        _IntersectWidth         ("Intersect Width",      Range(0.01, 2.0))  = 0.3

        // ── Dissolve (C# driven) ──────────────────────────────────────────────
        // Driven by BarrierController via MaterialPropertyBlock.
        // 0 = fully visible, 1 = fully clipped away with edge glow at boundary.
        // Use this for the gate-open animation — it clips geometry so the barrier
        // physically disappears rather than just fading in alpha.
        _Dissolve               ("Dissolve",             Range(0.0, 1.0))   = 0.0
        _DissolveTex            ("Dissolve Noise",       2D)                = "white" {}
        [HDR] _DissolveEdgeColor("Dissolve Edge Color",  Color)             = (0.3, 0.8, 1.0, 1.0)
        _DissolveEdgeWidth      ("Dissolve Edge Width",  Range(0.001, 0.3)) = 0.05
        _DissolveEdgeBrightness ("Edge Brightness",      Range(1.0, 10.0))  = 4.0

        // ── Visibility (C# driven) ────────────────────────────────────────────
        // Multiplies master alpha — useful for instant hide without dissolve.
        // 0 = fully invisible, 1 = fully visible.
        _Visibility             ("Visibility",           Range(0.0, 1.0))   = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Transparent"
            "Queue"          = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        // No shadow caster — transparent barriers cast no shadows.

        Pass
        {
            Name "BarrierForward"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off          // transparent — don't write depth
            Cull Off            // visible from both sides (inside a sphere barrier)
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #pragma multi_compile _ _FORWARD_PLUS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4  _Color;
                half   _Alpha;

                half   _FresnelPower;
                half   _FresnelIntensity;
                half   _InvertFresnel;

                float4 _FadeAxis;
                half   _FadeStrength;
                half   _FadeBias;

                float4 _ScanlineTex_ST;
                half   _ScanlineScale;
                half   _ScanlineSpeed;
                half   _ScanlineIntensity;

                half   _PulseSpeed;
                half   _PulseIntensity;

                half4  _IntersectColor;
                half   _IntersectWidth;

                half   _Dissolve;
                float4 _DissolveTex_ST;
                half4  _DissolveEdgeColor;
                half   _DissolveEdgeWidth;
                half   _DissolveEdgeBrightness;

                half   _Visibility;
            CBUFFER_END

            TEXTURE2D(_ScanlineTex);  SAMPLER(sampler_ScanlineTex);
            TEXTURE2D(_DissolveTex);  SAMPLER(sampler_DissolveTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float2 dissolveUV  : TEXCOORD1;
                float3 normalWS    : TEXCOORD2;
                float3 viewDirWS   : TEXCOORD3;
                float3 positionWS  : TEXCOORD4;
                float4 screenPos   : TEXCOORD5;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   nrmInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS = posInputs.positionCS;
                OUT.positionWS = posInputs.positionWS;
                OUT.normalWS   = nrmInputs.normalWS;
                OUT.viewDirWS  = GetWorldSpaceViewDir(posInputs.positionWS);
                OUT.uv         = TRANSFORM_TEX(IN.uv, _ScanlineTex);
                OUT.dissolveUV = TRANSFORM_TEX(IN.uv, _DissolveTex);
                OUT.screenPos  = ComputeScreenPos(posInputs.positionCS);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // ── Early outs ────────────────────────────────────────────────
                // Skip all math when completely invisible
                clip(_Visibility - 0.001h);

                // ── Dissolve clip (same logic as GhostLit) ────────────────────
                // noise [0..1]. Pixels where noise < _Dissolve are clipped away.
                // dissolveAlpha → 0 at the clip boundary → that thin band gets
                // the edge glow colour.
                half noise        = SAMPLE_TEXTURE2D(_DissolveTex, sampler_DissolveTex,
                                                      IN.dissolveUV).r;
                half dissolveAlpha = noise - _Dissolve;
                clip(dissolveAlpha);

                // Edge glow — lerp from edge colour toward zero over _DissolveEdgeWidth.
                // Masked so glow is invisible when _Dissolve is 0 (not dissolving).
                half  edgeFactor   = saturate(dissolveAlpha / max(_DissolveEdgeWidth, 0.001h));
                half3 dissolveGlow = _DissolveEdgeColor.rgb
                                   * (1.0h - edgeFactor)
                                   * _DissolveEdgeBrightness
                                   * saturate(_Dissolve * 10.0h); // suppress glow at rest

                // ── Fresnel ───────────────────────────────────────────────────
                half3 normalWS  = normalize(IN.normalWS);
                half3 viewDirWS = normalize(IN.viewDirWS);

                half NdotV    = saturate(dot(normalWS, viewDirWS));
                half fresnel  = pow(1.0h - NdotV, _FresnelPower);
                // Invert: 0 = bright at rim (default shield), 1 = bright at center
                fresnel = lerp(fresnel, 1.0h - fresnel, _InvertFresnel);
                fresnel = saturate(fresnel * _FresnelIntensity);

                // ── Axis fade ─────────────────────────────────────────────────
                float3 fadeDir       = normalize(_FadeAxis.xyz + float3(0.0001, 0, 0));
                float  axisProj      = dot(IN.positionWS, fadeDir);
                float  objCentreProj = dot(UNITY_MATRIX_M._m03_m13_m23, fadeDir);
                float  localProj     = axisProj - objCentreProj;
                float  fadeMask      = saturate(localProj + _FadeBias + 0.5h);
                fadeMask = lerp(1.0h, fadeMask, _FadeStrength);

                // ── Scanlines ─────────────────────────────────────────────────
                float2 scanUV    = IN.uv * _ScanlineScale;
                scanUV.y        += _Time.y * _ScanlineSpeed;
                half scanline    = SAMPLE_TEXTURE2D(_ScanlineTex, sampler_ScanlineTex, scanUV).r;
                half scanContrib = lerp(1.0h, scanline, _ScanlineIntensity);

                // ── Pulse ─────────────────────────────────────────────────────
                half pulse = sin(_Time.y * _PulseSpeed) * 0.5h + 0.5h;
                pulse      = 1.0h + pulse * _PulseIntensity;

                // ── Intersection highlight ────────────────────────────────────
                float2 screenUV     = IN.screenPos.xy / IN.screenPos.w;
                float  sceneDepth   = LinearEyeDepth(SampleSceneDepth(screenUV), _ZBufferParams);
                float  barrierDepth = IN.screenPos.w;
                float  depthDiff    = sceneDepth - barrierDepth;
                half   intersect    = 1.0h - saturate(depthDiff / max(_IntersectWidth, 0.001h));
                half3  intersectRGB = _IntersectColor.rgb * intersect;

                // ── Combine ───────────────────────────────────────────────────
                half3 baseRGB  = _Color.rgb * pulse * scanContrib;

                // dissolveGlow is purely additive — same as GhostLit rim treatment
                half3 finalRGB = baseRGB + intersectRGB + dissolveGlow;

                // Alpha: master alpha × fresnel × axis fade × visibility
                // Dissolve is handled by clip() above — no alpha involvement needed
                half finalAlpha = _Alpha
                                * fresnel
                                * fadeMask
                                * _Visibility;

                // Contact highlight boosts alpha so the intersection line always reads
                finalAlpha = saturate(finalAlpha + intersect * _IntersectColor.a * _Visibility);

                return half4(finalRGB, finalAlpha);
            }

            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
