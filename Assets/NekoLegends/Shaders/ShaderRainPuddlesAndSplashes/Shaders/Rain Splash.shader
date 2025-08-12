Shader "Neko Legends/Rain/Rain Splash"
{
    Properties
    {
        [Header(Main Colors)]
        _TintColor ("Tint Color", Color) = (1,1,1,1)
        _ColorStrength ("Color Strength", Range(0, 5)) = 1
        
        [Header(Textures)]
        _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
        _BumpMap ("Normalmap", 2D) = "bump" {} // Renamed to just "Normalmap"
        _Radius ("Mask Radius", Range(0, 1)) = 1
        
        [Header(Distortion Controls)]
        _BumpAmt ("Distortion Strength", Range(0, 50)) = 10
        _DistortionScale ("Distortion Scale", Range(0, 1)) = 0.2
        
        [Header(Rotation)]
        _RotationAngle ("Rotation Angle", Range(0, 360)) = 0
        
        [Header(Scale)]
        _Scale ("Scale", Range(0.1, 10)) = 1
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
                float2 uvBump : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);
            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BumpMap_ST;
                half4 _TintColor;
                float _ColorStrength;
                float _BumpAmt;
                float _DistortionScale;
                float _Radius;
                float _RotationAngle;
                float _Scale;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                
                OUT.position = TransformObjectToHClip(IN.vertex.xyz);
                OUT.screenPos = ComputeScreenPos(OUT.position);
                
                // Compute transformed UVs
                float2 uvMain = TRANSFORM_TEX(IN.uv, _MainTex);
                float2 uvBump = TRANSFORM_TEX(IN.uv, _BumpMap);
                
                // Scale uvMain around (0.5, 0.5)
                float2 uvMainCenter = float2(0.5, 0.5);
                float2 uvMainOffset = uvMain - uvMainCenter;
                float2 scaledUvMainOffset = uvMainOffset * _Scale;
                float2 scaledUvMain = scaledUvMainOffset + uvMainCenter;
                
                // Scale uvBump around (0.5, 0.5)
                float2 uvBumpCenter = float2(0.5, 0.5);
                float2 uvBumpOffset = uvBump - uvBumpCenter;
                float2 scaledUvBumpOffset = uvBumpOffset * _Scale;
                float2 scaledUvBump = scaledUvBumpOffset + uvBumpCenter;
                
                // Apply rotation
                float angle = radians(_RotationAngle);
                float s = sin(angle);
                float c = cos(angle);
                float2x2 rotationMatrix = float2x2(c, -s, s, c);
                
                // Rotate scaled uvMain
                float2 rotatedUvMainOffset = mul(rotationMatrix, scaledUvMain - uvMainCenter);
                OUT.uvMain = rotatedUvMainOffset + uvMainCenter;
                
                // Rotate scaled uvBump
                float2 rotatedUvBumpOffset = mul(rotationMatrix, scaledUvBump - uvBumpCenter);
                OUT.uvBump = rotatedUvBumpOffset + uvBumpCenter;
                
                OUT.color = IN.color;
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                float2 centerUV = float2(0.5, 0.5);
                float distanceToCenter = length(IN.uvMain - centerUV);
                float mask = 1.0 - smoothstep(0.0, _Radius, distanceToCenter);
                
                // Sample only Normal Map 1
                float2 bump = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, IN.uvBump)).rg;
                
                float2 offset = bump * _BumpAmt * _DistortionScale * IN.color.r * mask;
                float2 distortedUV = saturate(screenUV + offset * 0.01);
                half4 distortedGrab = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, distortedUV);
                
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uvMain) * IN.color * mask;
                half4 emission = distortedGrab + tex * _ColorStrength * _TintColor;
                emission.a = _TintColor.a * IN.color.a;
                return saturate(emission);
            }
            
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}