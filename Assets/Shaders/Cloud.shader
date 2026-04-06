Shader "Custom/CloudCubeSoftModular"
{
    Properties
    {
        _MainTex ("Noise Texture", 2D) = "white" {}
        _CloudColor ("Cloud Color", Color) = (1,1,1,1)
        _Density ("Density", Range(0,3)) = 1
        _Softness ("Edge Softness", Range(0,1)) = 0.5
        _Opacity ("Cloud Opacity", Range(0,1)) = 1
        _ScrollSpeed ("Scroll Speed", Vector) = (0.01,0.01,0,0)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _CloudColor;
            float _Density;
            float _Softness;
            float _Opacity;
            float4 _ScrollSpeed;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 localPos : TEXCOORD1;
                float3 bounds : TEXCOORD2; // cube bounds for auto-scaling
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.localPos = IN.positionOS.xyz;

                // Pass cube bounds (assuming object is centered at origin)
                OUT.bounds = float3(0.5, 0.5, 0.5) * 2.0; // approximate half-size *2 = full size

                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // Scroll noise
                float2 uv = IN.uv + _ScrollSpeed.xy * _Time.y;

                // Sample noise
                float noise = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).r;

                // Cloud shape using edge softness
                float cloud = smoothstep(_Softness, 1.0, noise * _Density);

                // Automatic height fade based on cube size
                float cubeHeight = IN.bounds.y;  // height of cube
                float fadeMin = 0.05;            // minimum alpha at top
                float heightFade = smoothstep(cubeHeight, 0.0, IN.localPos.y); 
                heightFade = lerp(fadeMin, 1.0, heightFade);

                // Darken cloud color near bottom (gradient)
                float colorFade = pow(heightFade, 1.5); // sharper dark at bottom
                float3 gradientColor = _CloudColor.rgb * colorFade;

                // Final alpha
                float alpha = cloud * _Opacity * heightFade;

                return float4(gradientColor, alpha);
            }

            ENDHLSL
        }
    }
}