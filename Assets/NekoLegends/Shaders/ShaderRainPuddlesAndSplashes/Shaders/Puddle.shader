Shader "Neko Legends/Rain/Puddle"
{
    Properties
    {
        [Header(Ripple Control)]
        _RippleStrength("Ripple Strength", Range(0, 1)) = 0.5
        _Frequency("Ripple Frequency", Range(0, 50)) = 20
        _Speed("Ripple Speed", Range(0, 10)) = 1
        _Decay("Ripple Decay", Range(0, 10)) = 2
        _DropSpacing("Drop Spacing", Range(0.1, 10)) = 1
        _TimeScale("Time Scale", Range(0, 5)) = 1

        
        _WaterColor("Water Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { 
            "Queue"="Transparent" 
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }
        LOD 200

        Pass
        {
            Name "ForwardPass"
            HLSLPROGRAM
            // Use vertex and fragment functions with instancing support.
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            // Target OpenGL ES 3.0 for WebGL 2.0 compatibility.
            #pragma target 3.0

            // Include the URP core functions.
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // Grab the opaque camera texture (make sure “Opaque Texture” is enabled in your URP asset).
            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            CBUFFER_START(UnityPerMaterial)
                float _RippleStrength;
                float _Frequency;
                float _Speed;
                float _Decay;
                float _DropSpacing;
                float _TimeScale;
                float4 _WaterColor;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                // Transform vertex positions and compute screen-space UVs.
                OUT.position = TransformObjectToHClip(IN.vertex.xyz);
                OUT.screenPos = ComputeScreenPos(OUT.position);
                OUT.uv = IN.uv;
                return OUT;
            }

            // A simple pseudo-random generator for 2D vectors.
            float2 random2d(float2 p)
            {
                float n = dot(p, float2(12.9898, 78.233));
                return frac(sin(float2(n, n + 1.0)) * 43758.5453);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Compute screen-space UV coordinates.
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                // Use the plane's UVs to generate the ripple pattern.
                float2 uv = IN.uv;
                float2 rippleOffset = float2(0.0, 0.0);

                // Determine the current cell based on drop spacing.
                float2 cell = floor(uv * _DropSpacing);

                // Loop over a 3x3 grid to simulate multiple droplet impacts.
                for (int j = -1; j <= 1; j++)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        float2 cellPos = cell + float2(i, j);
                        // Generate a random droplet center within the cell.
                        float2 rand = random2d(cellPos);
                        float2 dropCenter = (cellPos + rand) / _DropSpacing;

                        // Offset the droplet’s timing using a random factor.
                        float timeOffset = rand.x * 5.0;
                        float2 d = uv - dropCenter;
                        float dist = length(d);

                        // Compute the ripple effect: a sine wave modulated by distance,
                        // animated over time, and decaying with distance.
                        float ripple = sin(dist * _Frequency - ((_Time.y * _TimeScale) + timeOffset) * _Speed)
                                       * exp(-dist * _Decay);

                        if (dist > 0.001)
                        {
                            rippleOffset += (d / dist) * ripple;
                        }
                    }
                }

                // Convert the ripple displacement from UV to screen-space offset.
                float2 displacedUV = screenUV + rippleOffset * _RippleStrength * 0.01;
                // Sample the grabbed background texture using the displaced UVs.
                half4 col = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, displacedUV);
                // Tint the final result with the water color.
                col.rgb *= _WaterColor.rgb;
                col.a *= _WaterColor.a;
                return col;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}
