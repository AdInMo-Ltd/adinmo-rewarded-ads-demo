Shader "Custom/URP_TwoTone_Safe"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}

        [HDR] _LitColor("Lit Tint", Color) = (1, 1, 1, 1)
        [HDR] _ShadowColor("Shadow Tint", Color) = (0.3, 0.3, 0.3, 1)

        _FalloffCenter("Falloff Center", Range(0.0, 1.0)) = 0.5
        _FalloffSoftness("Falloff Softness", Range(0.01, 1.0)) = 0.1
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline" 
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 positionWS   : TEXCOORD1;
                float3 normalWS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _LitColor;
                float4 _ShadowColor;
                float _FalloffCenter;
                float _FalloffSoftness;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS, float4(1,1,1,1));

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. Sample Texture
                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);

                // 2. Get Light Info
                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                
                // 3. Calculate Intensity
                float lightIntensity = saturate(dot(input.normalWS, mainLight.direction)) * mainLight.shadowAttenuation;

                // 4. Calculate Mask
                float minRange = _FalloffCenter - (_FalloffSoftness * 0.5);
                float maxRange = _FalloffCenter + (_FalloffSoftness * 0.5);
                float mask = smoothstep(minRange, maxRange, lightIntensity);

                // 5. Mix Tints
                float3 tintColor = lerp(_ShadowColor.rgb, _LitColor.rgb, mask);

                // 6. Combine
                float3 finalColor = albedo.rgb * tintColor;

                return half4(finalColor, albedo.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
}