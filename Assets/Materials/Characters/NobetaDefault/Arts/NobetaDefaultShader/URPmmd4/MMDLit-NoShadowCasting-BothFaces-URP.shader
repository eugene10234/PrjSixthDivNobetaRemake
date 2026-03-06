// Converted to URP from MMD4Mecanim/MMDLit-NoShadowCasting-BothFaces
Shader "URP/MMD/MMDLit-NoShadowCasting-BothFaces"
{
    Properties
    {
        _Color("Diffuse", Color) = (1,1,1,1)
        _Specular("Specular", Color) = (1,1,1)
        _Ambient("Ambient", Color) = (1,1,1)
        _Shininess("Shininess", Float) = 0
        _ShadowLum("ShadowLum", Range(0,10)) = 1.5
        _AmbientToDiffuse("AmbientToDiffuse", Float) = 5
        _EdgeColor("EdgeColor", Color) = (0,0,0,1)
        _EdgeScale("EdgeScale", Range(0,2)) = 0
        _EdgeSize("EdgeSize", float) = 0
        _MainTex("MainTex", 2D) = "white" {}
        _ToonTex("ToonTex", 2D) = "white" {}
        _SphereCube("SphereCube", Cube) = "white" {}
        _Emissive("Emissive", Color) = (0,0,0,0)
        _ALPower("ALPower", Float) = 0
        _AddLightToonCen("AddLightToonCen", Float) = -0.1
        _AddLightToonMin("AddLightToonMin", Float) = 0.5
        _ToonTone("ToonTone", Vector) = (1.0, 0.5, 0.5, 0.0)
        
        // Feature toggles
        [Toggle(SPECULAR_ON)] _SpecularEnable("Enable Specular", Float) = 1
        [Toggle(EMISSIVE_ON)] _EmissiveEnable("Enable Emissive", Float) = 0
        [KeywordEnum(None, Mul, Add)] _SphereMap("Sphere Map Mode", Float) = 0
        [Toggle(SELFSHADOW_ON)] _SelfShadow("Enable Self Shadow", Float) = 1
        
        // URP required properties
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 0.0 // Off for both faces
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry+1"
        }
        LOD 200
        
        // Main Forward Pass
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            Cull Off
            ZWrite On
            ZTest LEqual
            Blend Off
            
            HLSLPROGRAM
            #pragma target 3.0
            
            // URP keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            
            // MMD feature keywords
            #pragma shader_feature_local SPECULAR_ON
            #pragma shader_feature_local EMISSIVE_ON
            #pragma shader_feature_local SELFSHADOW_ON
            #pragma shader_feature_local _SPHEREMAP_NONE _SPHEREMAP_MUL _SPHEREMAP_ADD
            
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _Specular;
                float4 _Ambient;
                float4 _Emissive;
                float _Shininess;
                float _ShadowLum;
                float _AmbientToDiffuse;
                float _ALPower;
                float _AddLightToonCen;
                float _AddLightToonMin;
                float4 _ToonTone;
                float4 _MainTex_ST;
            CBUFFER_END
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_ToonTex);
            SAMPLER(sampler_ToonTex);
            TEXTURECUBE(_SphereCube);
            SAMPLER(sampler_SphereCube);
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float fogFactor : TEXCOORD3;
                #ifdef _MAIN_LIGHT_SHADOWS
                float4 shadowCoord : TEXCOORD4;
                #endif
            };
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
                
                #ifdef _MAIN_LIGHT_SHADOWS
                output.shadowCoord = GetShadowCoord(vertexInput);
                #endif
                
                return output;
            }
            
            half4 frag(Varyings input, half facing : VFACE) : SV_Target
            {
                // Sample textures
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 color = mainTex * _Color;
                
                // Flip normal for back faces
                float3 normalWS = normalize(input.normalWS);
                normalWS *= facing > 0 ? 1 : -1;
                
                // Get main light
                #ifdef _MAIN_LIGHT_SHADOWS
                Light mainLight = GetMainLight(input.shadowCoord);
                #else
                Light mainLight = GetMainLight();
                #endif
                
                float3 lightDir = normalize(mainLight.direction);
                float NdotL = saturate(dot(normalWS, lightDir));
                
                // Apply shadow
                half shadow = 1.0;
                #ifdef SELFSHADOW_ON
                shadow = mainLight.shadowAttenuation;
                #endif
                
                // Toon shading
                float2 toonUV = float2(NdotL * shadow * 0.5 + 0.5, 0.5);
                half4 toonColor = SAMPLE_TEXTURE2D(_ToonTex, sampler_ToonTex, toonUV);
                
                // Ambient lighting
                half3 ambient = _Ambient.rgb * _AmbientToDiffuse;
                
                // Diffuse lighting
                half3 diffuse = mainLight.color * NdotL * toonColor.rgb * _ShadowLum * shadow;
                
                // Specular lighting
                half3 specular = 0;
                #ifdef SPECULAR_ON
                float3 viewDirWS = normalize(_WorldSpaceCameraPos - input.positionWS);
                float3 halfDir = normalize(lightDir + viewDirWS);
                float NdotH = saturate(dot(normalWS, halfDir));
                specular = _Specular.rgb * pow(NdotH, _Shininess) * mainLight.color * shadow;
                #endif
                
                // Sphere mapping
                half3 sphereColor = 0;
                #if defined(_SPHEREMAP_MUL) || defined(_SPHEREMAP_ADD)
                float3 viewDirWS = normalize(_WorldSpaceCameraPos - input.positionWS);
                float3 reflectDir = reflect(-viewDirWS, normalWS);
                half4 sphereSample = SAMPLE_TEXTURECUBE(_SphereCube, sampler_SphereCube, reflectDir);
                
                #ifdef _SPHEREMAP_MUL
                sphereColor = sphereSample.rgb;
                color.rgb *= sphereColor;
                #elif _SPHEREMAP_ADD
                sphereColor = sphereSample.rgb;
                #endif
                #endif
                
                // Emissive
                half3 emissive = 0;
                #ifdef EMISSIVE_ON
                emissive = _Emissive.rgb;
                #endif
                
                // Combine lighting
                half3 finalColor = color.rgb * (ambient + diffuse) + specular;
                
                #ifdef _SPHEREMAP_ADD
                finalColor += sphereColor;
                #endif
                
                finalColor += emissive;
                
                // Additional lights
                #ifdef _ADDITIONAL_LIGHTS
                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                {
                    Light light = GetAdditionalLight(lightIndex, input.positionWS);
                    float addNdotL = saturate(dot(normalWS, light.direction));
                    
                    // Toon shading for additional lights
                    float toonValue = saturate((addNdotL - _AddLightToonCen) * _ALPower);
                    toonValue = lerp(_AddLightToonMin, 1.0, toonValue);
                    
                    half3 addDiffuse = color.rgb * light.color * toonValue * light.distanceAttenuation;
                    
                    #ifdef SELFSHADOW_ON
                    addDiffuse *= light.shadowAttenuation;
                    #endif
                    
                    #ifdef SPECULAR_ON
                    float3 addHalfDir = normalize(light.direction + viewDirWS);
                    float addNdotH = saturate(dot(normalWS, addHalfDir));
                    half3 addSpecular = _Specular.rgb * pow(addNdotH, _Shininess) * light.color * light.distanceAttenuation;
                    #ifdef SELFSHADOW_ON
                    addSpecular *= light.shadowAttenuation;
                    #endif
                    addDiffuse += addSpecular;
                    #endif
                    
                    finalColor += addDiffuse;
                }
                #endif
                
                // Fog
                finalColor = MixFog(finalColor, input.fogFactor);
                
                return half4(finalColor, color.a);
            }
            ENDHLSL
        }
        
        // NO Shadow Caster Pass - This shader does not cast shadows
        // ShadowCaster pass is intentionally omitted
        
        // Depth Only Pass
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            
            Cull Off
            ZWrite On
            ColorMask 0
            
            HLSLPROGRAM
            #pragma target 3.0
            
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };
            
            Varyings DepthOnlyVertex(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }
            
            half4 DepthOnlyFragment(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
        
        // Depth Normals Pass (for SSAO, etc.)
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            
            Cull Off
            ZWrite On
            
            HLSLPROGRAM
            #pragma target 3.0
            
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
            };
            
            Varyings DepthNormalsVertex(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = vertexInput.positionCS;
                output.normalWS = normalInput.normalWS;
                
                return output;
            }
            
            half4 DepthNormalsFragment(Varyings input, half facing : VFACE) : SV_Target
            {
                // Flip normal for back faces
                float3 normalWS = normalize(input.normalWS);
                normalWS *= facing > 0 ? 1 : -1;
                
                return half4(normalWS, 0);
            }
            ENDHLSL
        }
    }
    
    Fallback Off
}
