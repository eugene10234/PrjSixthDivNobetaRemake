// GhostLit.shader — Unity 6 URP
//
// One shader covers both living and death states:
//   Living material  → _Dissolve = 0 (set in Inspector, never touched at runtime)
//   Death material   → _Dissolve driven 0→1 by BGAnimationActor MaterialPropertyBlock
//
// BGAnimationActor requires zero changes — it already sets _Dissolve by property name.
// Swap to the death material in PlayDeathSequence() exactly as you do now, then
// let the MaterialPropertyBlock animate _Dissolve as before.
//
// Key features:
//   Rim light      — Fresnel silhouette glow, fully tunable per material
//   Ambient boost  — flat additive floor so ghost never blacks out in dark scenes
//   Dissolve       — noise-clipped alpha with emissive edge glow
//   Shadow caster  — dissolve-aware, clips shadow geometry to match visible surface

Shader "Nobeta/GhostLit"
{
    Properties
    {
        // ── Surface ───────────────────────────────────────────────────────────
        _BaseColor          ("Base Color",           Color)            = (0.4, 0.6, 0.9, 1.0)
        _BaseMap            ("Base Texture",         2D)               = "white" {}

        // ── Rim light ─────────────────────────────────────────────────────────
        _RimColor           ("Rim Color",            Color)            = (0.4, 0.8, 1.0, 1.0)
        _RimPower           ("Rim Power",            Range(0.1, 8.0))  = 2.0
        _RimIntensity       ("Rim Intensity",        Range(0.0, 5.0))  = 1.5

        // ── Ambient boost ─────────────────────────────────────────────────────
        // Additive floor — keeps ghost readable in near-black environments
        _AmbientBoost       ("Ambient Boost",        Range(0.0, 1.0))  = 0.15

        // ── Dissolve ──────────────────────────────────────────────────────────
        // _Dissolve is set by BGAnimationActor via MaterialPropertyBlock.
        // Keep [HideInInspector] — artists should not edit this directly.
        _Dissolve           ("Dissolve",             Range(0.0, 1.0))  = 0.0
        _DissolveTex        ("Dissolve Noise",       2D)               = "white" {}
        _DissolveEdgeColor  ("Dissolve Edge Color",  Color)            = (0.3, 0.7, 1.0, 1.0)
        _DissolveEdgeWidth  ("Dissolve Edge Width",  Range(0.001, 0.3))= 0.05

        // -- Emission ---------------------------------------------------------
        [HDR] _EmissionColor ("Emission Color", Color) = (0,0,0,1)
        _EmissionMap   ("Emission Map", 2D)    = "white" {}
        _EmissionScale ("Emission Scale", Range(0, 10)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline"
        }

        // ══════════════════════════════════════════════════════════════════════
        // PASS 1 — Forward lit
        // ══════════════════════════════════════════════════════════════════════
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            // URP shadow keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            // Unity 6 requires this for forward+ rendering path
            #pragma multi_compile _ _FORWARD_PLUS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4  _BaseColor;
                half4  _RimColor;
                half   _RimPower;
                half   _RimIntensity;
                half   _AmbientBoost;
                half   _Dissolve;
                float4 _DissolveTex_ST;
                half4  _DissolveEdgeColor;
                half   _DissolveEdgeWidth;
                half4  _EmissionColor;
                half   _EmissionScale;
                float4 _EmissionMap_ST;
            CBUFFER_END

            TEXTURE2D(_BaseMap);     SAMPLER(sampler_BaseMap);
            TEXTURE2D(_DissolveTex); SAMPLER(sampler_DissolveTex);
            TEXTURE2D(_EmissionMap); SAMPLER(sampler_EmissionMap);

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
                float3 positionWS  : TEXCOORD3;
                float3 viewDirWS   : TEXCOORD4;
                float4 shadowCoord : TEXCOORD5;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   nrmInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS  = posInputs.positionCS;
                OUT.positionWS  = posInputs.positionWS;
                OUT.normalWS    = nrmInputs.normalWS;
                OUT.viewDirWS   = GetWorldSpaceViewDir(posInputs.positionWS);
                OUT.uv          = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.dissolveUV  = TRANSFORM_TEX(IN.uv, _DissolveTex);

                // Unity 6 — use GetShadowCoord which handles all cascade modes
                OUT.shadowCoord = GetShadowCoord(posInputs);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // ── Base colour ───────────────────────────────────────────────
                half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv)
                                * _BaseColor;

                // ── Dissolve clip ─────────────────────────────────────────────
                // noise [0..1] from texture. Pixels where noise < _Dissolve are clipped.
                // dissolveAlpha approaches 0 at the clip boundary — that thin band
                // gets the edge glow colour.
                half noise        = SAMPLE_TEXTURE2D(_DissolveTex, sampler_DissolveTex,
                                                      IN.dissolveUV).r;
                half dissolveAlpha = noise - _Dissolve;
                clip(dissolveAlpha);

                // Edge glow — lerp from edge colour to black over _DissolveEdgeWidth
                half  edgeFactor   = saturate(dissolveAlpha / max(_DissolveEdgeWidth, 0.001));
                half3 dissolveGlow = _DissolveEdgeColor.rgb
                                   * (1.0h - edgeFactor)
                                   * 4.0h; // brightness multiplier — tune in Inspector

                // ── Lighting ──────────────────────────────────────────────────
                half3 normalWS  = normalize(IN.normalWS);
                half3 viewDirWS = normalize(IN.viewDirWS);

                // -- Emission calculation ---------------------------------------------
                // We use TRANSFORM_TEX logic if you want the emission to tile with the base map
                float2 emissionUV = TRANSFORM_TEX(IN.uv, _EmissionMap);
                half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, emissionUV).rgb 
                                 * _EmissionColor.rgb 
                                 * _EmissionScale;

                // Unity 6 URP 17+ — GetMainLight requires a fully populated InputData.
                // Build the minimum struct needed for shadow sampling.
                InputData inputData = (InputData)0;
                inputData.positionWS              = IN.positionWS;
                inputData.positionCS              = IN.positionCS;
                inputData.normalWS                = normalWS;
                inputData.viewDirectionWS         = viewDirWS;
                inputData.shadowCoord             = IN.shadowCoord;
                inputData.fogCoord                = 0;
                inputData.vertexLighting          = half3(0, 0, 0);
                inputData.bakedGI                 = half3(0, 0, 0);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionCS);
                inputData.shadowMask              = half4(1, 1, 1, 1);

                AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(inputData.normalizedScreenSpaceUV);
                Light mainLight = GetMainLight(inputData, IN.shadowCoord, aoFactor);

                half  NdotL   = saturate(dot(normalWS, mainLight.direction));
                half3 diffuse = mainLight.color
                              * mainLight.shadowAttenuation
                              * NdotL;

                // Additional lights — Unity 6 Forward+ uses LIGHT_LOOP macros
                #if defined(_ADDITIONAL_LIGHTS)
                {
                    uint lightCount = GetAdditionalLightsCount();
                    LIGHT_LOOP_BEGIN(lightCount)
                        Light light   = GetAdditionalLight(lightIndex, IN.positionWS,
                                                           half4(1,1,1,1));
                        half addNdotL = saturate(dot(normalWS, light.direction));
                        diffuse      += light.color
                                      * light.distanceAttenuation
                                      * light.shadowAttenuation
                                      * addNdotL;
                    LIGHT_LOOP_END
                }
                #endif

                // Spherical harmonics ambient + flat boost
                half3 ambient = SampleSH(normalWS) + _AmbientBoost;

                // ── Rim light (Fresnel) ───────────────────────────────────────
                // Silhouette edges have NdotV near 0 → (1 - NdotV) near 1 → bright rim.
                // Additive on top of lighting — not multiplied by albedo — so it glows
                // visibly even when the surface is very dark.
                half  NdotV   = saturate(dot(normalWS, viewDirWS));
                half  rimMask = pow(1.0h - NdotV, _RimPower);
                half3 rim     = _RimColor.rgb * rimMask * _RimIntensity;

                // FIX: Mask the glow so it only shows up when we are actually dissolving
                // This forces the glow to 0 when _Dissolve is 0
                dissolveGlow *= saturate(_Dissolve * 10.0);

                // ── Combine ───────────────────────────────────────────────────
                half3 lit      = diffuse + ambient;
                half3 finalRGB = baseColor.rgb * lit   // albedo × lighting
                               + rim                   // rim is purely additive
                               + dissolveGlow
                               + emission;         // dissolve edge is purely additive

                return half4(finalRGB, baseColor.a);
            }

            ENDHLSL
        }

        // ══════════════════════════════════════════════════════════════════════
        // PASS 2 — Shadow caster
        // Dissolve-aware: clipped pixels cast no shadow, so a half-dissolved
        // ghost casts a matching partial shadow.
        // ══════════════════════════════════════════════════════════════════════
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest  LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex   shadowVert
            #pragma fragment shadowFrag
            #pragma multi_compile_shadowcaster
            // Unity 6 — required to avoid shadow acne on skinned meshes
            #pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4  _BaseColor;
                half4  _RimColor;
                half   _RimPower;
                half   _RimIntensity;
                half   _AmbientBoost;
                half   _Dissolve;
                float4 _DissolveTex_ST;
                half4  _DissolveEdgeColor;
                half   _DissolveEdgeWidth;
            CBUFFER_END

            TEXTURE2D(_DissolveTex); SAMPLER(sampler_DissolveTex);

            struct ShadowAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 dissolveUV : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            ShadowVaryings shadowVert(ShadowAttributes IN)
            {
                ShadowVaryings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                // Unity 6 URP shadow caster — use the helper struct and macro
                // instead of manually calling ApplyShadowBias (API changed in URP 17)
                float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 nrmWS = TransformObjectToWorldNormal(IN.normalOS);

                #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
                    float3 lightDir = normalize(_AdditionalLightsPosition[0].xyz - posWS);
                #else
                    float3 lightDir = _MainLightPosition.xyz;
                #endif

                // GetShadowPositionHClip handles bias correctly in Unity 6
                OUT.positionCS = TransformWorldToHClip(
                    ApplyShadowBias(posWS, nrmWS, lightDir));

                #if UNITY_REVERSED_Z
                    OUT.positionCS.z = min(OUT.positionCS.z,
                                          UNITY_NEAR_CLIP_VALUE * OUT.positionCS.w);
                #else
                    OUT.positionCS.z = max(OUT.positionCS.z,
                                          UNITY_NEAR_CLIP_VALUE * OUT.positionCS.w);
                #endif

                OUT.dissolveUV = TRANSFORM_TEX(IN.uv, _DissolveTex);
                return OUT;
            }

            half4 shadowFrag(ShadowVaryings IN) : SV_Target
            {
                // Mirror the same clip as the forward pass
                half noise = SAMPLE_TEXTURE2D(_DissolveTex, sampler_DissolveTex,
                                               IN.dissolveUV).r;
                clip(noise - _Dissolve);
                return 0;
            }

            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
