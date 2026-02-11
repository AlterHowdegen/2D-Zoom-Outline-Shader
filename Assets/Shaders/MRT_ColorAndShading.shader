Shader "Custom/MRT_ColorAndShading"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "LightMode"="UniversalForward" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // URP Core and Lighting includes
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BaseColor;
                float _Smoothness;
            CBUFFER_END

            // The magic struct: This defines our two targets
            struct FragmentOutput
            {
                float4 color   : SV_Target0; // Maps to _TexColor (Color Buffer)
                float4 shading : SV_Target1; // Maps to _TexShading (Shading Buffer)
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            // FragmentOutput frag(Varyings input)
            // {
            //     FragmentOutput outData;

            //     // 1. Calculate Base Texture Color
            //     float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _BaseColor;

            //     // TARGET 0: Flat Color
            //     // We send pure color here. No shadows, no light. 
            //     // This makes edge detection extremely easy for the compositor.
            //     outData.color = texColor;

            //     // TARGET 1: Shading
            //     // Here we calculate standard URP lighting (Lambert/Specular)
            //     Light mainLight = GetMainLight();
            //     float3 lightColor = mainLight.color * saturate(dot(input.normalWS, mainLight.direction));
                
            //     // Add ambient/indirect light so it's not pitch black in shadows
            //     float3 ambient = SampleSH(input.normalWS);
                
            //     outData.shading = float4(texColor.rgb * (lightColor + ambient), texColor.a);

            //     return outData;
            // }

            FragmentOutput frag(Varyings input)
            {
                FragmentOutput outData;

                // 1. Calculate color (Target 0)
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _BaseColor;
                outData.color = texColor;

                // 2. Calculate shading (Target 1)
                Light mainLight = GetMainLight();
                float3 diffuse = saturate(dot(input.normalWS, mainLight.direction)) * mainLight.color;
                float3 ambient = SampleSH(input.normalWS);
                
                outData.shading = float4(texColor.rgb * (diffuse + ambient), texColor.a);

                return outData;
            }

            
            ENDHLSL
        }
    }
}