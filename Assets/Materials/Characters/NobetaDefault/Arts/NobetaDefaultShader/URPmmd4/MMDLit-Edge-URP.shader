// Converted to URP from MMD4Mecanim/MMDLit-Edge
Shader "URP/MMD/MMDLit-Edge"
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
        
        // URP required properties
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0
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
            
            Cull Back
            ZWrite On
            ZTest LEqual
            
            HLSLPROGRAM
            #pragma target 3.0
            
            // URP keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            
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
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Sample textures
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 color = mainTex * _Color;
                
                // Lighting
                float3 normalWS = normalize(input.normalWS);
                
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float NdotL = saturate(dot(normalWS, lightDir));
                
                // Toon shading
                float2 toonUV = float2(NdotL * 0.5 + 0.5, 0.5);
                half4 toonColor = SAMPLE_TEXTURE2D(_ToonTex, sampler_ToonTex, toonUV);
                
                // Sphere map
                float3 viewDirWS = normalize(_WorldSpaceCameraPos - input.positionWS);
                float3 reflectDir = reflect(-viewDirWS, normalWS);
                half4 sphereColor = SAMPLE_TEXTURECUBE(_SphereCube, sampler_SphereCube, reflectDir);
                
                // Combine lighting
                half3 ambient = _Ambient.rgb * _AmbientToDiffuse;
                half3 diffuse = mainLight.color * NdotL * toonColor.rgb * _ShadowLum;
                
                // Specular
                float3 halfDir = normalize(lightDir + viewDirWS);
                float NdotH = saturate(dot(normalWS, halfDir));
                half3 specular = _Specular.rgb * pow(NdotH, _Shininess) * mainLight.color;
                
                // Final color
                half3 finalColor = color.rgb * (ambient + diffuse) + specular + sphereColor.rgb + _Emissive.rgb;
                
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
                    
                    finalColor += color.rgb * light.color * toonValue * light.distanceAttenuation * light.shadowAttenuation;
                }
                #endif
                
                // Fog
                finalColor = MixFog(finalColor, input.fogFactor);
                
                return half4(finalColor, color.a);
            }
            ENDHLSL
        }
        
        // Shadow Caster Pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            Cull Off
            ZWrite On
            ZTest LEqual
            
            HLSLPROGRAM
            #pragma target 3.0
            
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };
            
            float3 _LightDirection;
            
            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = TransformWorldToHClip(ApplyShadowBias(vertexInput.positionWS, normalInput.normalWS, _LightDirection));
                
                #if UNITY_REVERSED_Z
                output.positionCS.z = min(output.positionCS.z, output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                output.positionCS.z = max(output.positionCS.z, output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif
                
                return output;
            }
            
            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
        
        // Depth Only Pass
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            
            Cull Back
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
        
        // Edge Pass (Outline) - Base
        Pass
        {
            Name "OutlineBase"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            
            Cull Front
            ZWrite On
            ZTest Less
            Blend SrcAlpha OneMinusSrcAlpha
            
            HLSLPROGRAM
            #pragma target 3.0
            
            #pragma multi_compile_fog
            
            #pragma vertex OutlineVertex
            #pragma fragment OutlineFragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            CBUFFER_START(UnityPerMaterial)
                float4 _EdgeColor;
                float _EdgeScale;
                float _EdgeSize;
            CBUFFER_END
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float fogFactor : TEXCOORD0;
            };
            
            Varyings OutlineVertex(Attributes input)
            {
                Varyings output;
                
                // Extrude vertices along normal
                float3 normalOS = normalize(input.normalOS);
                float3 extrudedPos = input.positionOS.xyz + normalOS * _EdgeSize * _EdgeScale * 0.01;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(extrudedPos);
                output.positionCS = vertexInput.positionCS;
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
                
                return output;
            }
            
            half4 OutlineFragment(Varyings input) : SV_Target
            {
                half4 color = _EdgeColor;
                color.rgb = MixFog(color.rgb, input.fogFactor);
                return color;
            }
            ENDHLSL
        }
        
        // Edge Pass (Outline) - Additive for additional lights
        Pass
        {
            Name "OutlineAdd"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            
            Cull Front
            ZWrite Off
            ZTest Less
            Blend One One
            
            HLSLPROGRAM
            #pragma target 3.0
            
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            
            #pragma vertex OutlineVertexAdd
            #pragma fragment OutlineFragmentAdd
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            CBUFFER_START(UnityPerMaterial)
                float4 _EdgeColor;
                float _EdgeScale;
                float _EdgeSize;
            CBUFFER_END
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };
            
            Varyings OutlineVertexAdd(Attributes input)
            {
                Varyings output;
                
                // Extrude vertices along normal
                float3 normalOS = normalize(input.normalOS);
                float3 extrudedPos = input.positionOS.xyz + normalOS * _EdgeSize * _EdgeScale * 0.01;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(extrudedPos);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                
                return output;
            }
            
            half4 OutlineFragmentAdd(Varyings input) : SV_Target
            {
                half3 color = 0;
                
                #ifdef _ADDITIONAL_LIGHTS
                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                {
                    Light light = GetAdditionalLight(lightIndex, input.positionWS);
                    color += light.color * light.distanceAttenuation * light.shadowAttenuation * _EdgeColor.rgb;
                }
                #endif
                
                return half4(color, 0);
            }
            ENDHLSL
        }
    }
    
    Fallback Off
}
