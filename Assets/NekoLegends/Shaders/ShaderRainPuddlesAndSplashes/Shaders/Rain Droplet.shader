Shader "Neko Legends/Rain/Rain Droplet"
{
    Properties
    {
        [Header(Main Colors)]
        _TintColor ("Tint Color", Color) = (1,1,1,1)
        _ColorStrength ("Color Strength", Range(0, 5)) = 1
        
        [Header(Textures)]
        _MainTex ("Color", 2D) = "white" {}
        _Rotation ("Rotation (Degrees)", Range(0, 360)) = 0
        _Size ("Size", Range(0.1, 5)) = 1
        _TeardropFactor ("Teardrop Factor", Range(0, 1)) = 0.5 // New property
        
        [Header(Distortion Controls)]
        _DistortionScale ("Distortion Scale", Range(0, 1)) = 0.2
    }

    SubShader
    {
        Tags { 
            "Queue"="Transparent" 
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }
        LOD 100

        Pass
        {
            Name "ForwardPass"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 position : SV_POSITION;
                float2 uvMain : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _TintColor;
                float _ColorStrength;
                float _DistortionScale;
                float _Rotation;
                float _Size;
                float _TeardropFactor; 
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                
                OUT.position = TransformObjectToHClip(IN.vertex.xyz);
                OUT.screenPos = ComputeScreenPos(OUT.position);
                
                // Apply tiling and offset from _MainTex_ST
                float2 uv = TRANSFORM_TEX(IN.uv, _MainTex);
                
                // Apply size scaling (smaller value = larger texture, larger value = smaller texture)
                uv = (uv - 0.5) / _Size + 0.5; // Scale UVs around center
                
                // Rotate UVs around pivot (0.5, 0.5)
                float rotRad = radians(_Rotation);
                float cosR = cos(rotRad);
                float sinR = sin(rotRad);
                float2x2 rotationMatrix = float2x2(cosR, -sinR, sinR, cosR);
                uv -= 0.5; // Pivot at center
                OUT.uvMain = mul(rotationMatrix, uv) + 0.5;
                
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                float2 offset = IN.color.rg * _DistortionScale;
                float2 distortedUV = saturate(screenUV + offset * 0.1);
                half4 distortedGrab = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, distortedUV);
            
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uvMain) * IN.color;
            
                float v = IN.uvMain.y;
                float bottom_width = 0.5;
                float top_width = bottom_width * (1 - _TeardropFactor);
                float width = lerp(top_width, bottom_width, v);
                float center_u = 0.5;
                float blur = 0.01;
                float alpha_proc = smoothstep(center_u - width/2 - blur, center_u - width/2, IN.uvMain.x) *
                                  smoothstep(center_u + width/2 + blur, center_u + width/2 - blur, IN.uvMain.x);
            
                half4 emission = distortedGrab + tex * _ColorStrength * _TintColor * alpha_proc;
                emission.a = alpha_proc * _TintColor.a * IN.color.a;
                return saturate(emission);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}