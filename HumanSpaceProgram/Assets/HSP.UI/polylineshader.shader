Shader "Hidden/HSP/PolyLine"
{
    Properties
    {
        _GlobalColor ("Global Color", Color) = (1,1,1,1)
        _DepthBias ("Depth Bias", Float) = 0.0005
        [Toggle] _WorldSpaceWidth ("Use World Space Width", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Cull Off
        ZWrite On
        ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            float4 _GlobalColor;
            float _DepthBias;
            float _WorldSpaceWidth;

            struct appdata
            {
                float3 vertex : POSITION;   
                fixed4 color : COLOR;
                float4 uv1 : TEXCOORD1;     // next point xyz, thickness in w
                float4 uv2 : TEXCOORD2;     // prev point xyz, side sign in w
            };

            struct v2f
            {
                float4 clipPos : SV_POSITION;
                float4 screenPos : TEXCOORD0;
                fixed4 color : COLOR;
                float depth : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                const float EPSILON = 1e-3;
                v2f o;

                float4 clipCurrent = mul(UNITY_MATRIX_VP, float4(v.vertex, 1.0));
                float4 clipNext = mul(UNITY_MATRIX_VP, float4(v.uv1.xyz, 1.0));
                float4 clipPrev = mul(UNITY_MATRIX_VP, float4(v.uv2.xyz, 1.0));
                
                float thicknessInput = v.uv1.w;
                float side = v.uv2.w;

                // --- World Space Width Logic ---
                float thicknessPx = thicknessInput;

                if (_WorldSpaceWidth > 0.5)
                {
                    // Linear depth to camera plane
                    float dist = clipCurrent.w; 
                    dist = max(dist, 0.001); // Prevent division by zero

                    // unity_CameraProjection[1][1] is cot(FOV/2)
                    // Project world size to pixel size
                    float projectionScale = unity_CameraProjection[1][1]; 
                    thicknessPx = (thicknessInput * _ScreenParams.y * projectionScale) / (dist * 2.0);
                }
                // -------------------------------

                o.depth = clipCurrent.z / clipCurrent.w;

                // Handle points behind camera (Clipping logic)
                bool currentBehind = clipCurrent.w <= 0.0;
                bool nextBehind = clipNext.w <= 0.0;
                bool prevBehind = clipPrev.w <= 0.0;

                if (currentBehind) { clipCurrent.xy *= (EPSILON / clipCurrent.w); clipCurrent.z = -1.0; clipCurrent.w = EPSILON; }
                if (nextBehind) { clipNext.xy *= (EPSILON / clipNext.w); clipNext.z = -1.0; clipNext.w = EPSILON; }
                if (prevBehind) { clipPrev.xy *= (EPSILON / clipPrev.w); clipPrev.z = -1.0; clipPrev.w = EPSILON; }

                float2 ndcCurrent = clipCurrent.xy / clipCurrent.w;
                float2 ndcNext = clipNext.xy / clipNext.w;
                float2 ndcPrev = clipPrev.xy / clipPrev.w;

                float2 screenSize = _ScreenParams.xy;
                float2 sCurrent = (ndcCurrent * 0.5 + 0.5) * screenSize;
                float2 sNext = (ndcNext * 0.5 + 0.5) * screenSize;
                float2 sPrev = (ndcPrev * 0.5 + 0.5) * screenSize;

                // Miter Corner Logic
                float2 dirToNext = sNext - sCurrent;
                float2 dirFromPrev = sCurrent - sPrev;
                float2 perp;

                float lenToNext = length(dirToNext);
                float lenFromPrev = length(dirFromPrev);
                
                if (lenToNext < EPSILON && lenFromPrev < EPSILON) {
                    perp = float2(0.0, 1.0);
                }
                else if (lenFromPrev < EPSILON) {
                    float2 dirB = normalize(dirToNext);
                    perp = float2(-dirB.y, dirB.x);
                }
                else if (lenToNext < EPSILON) {
                    float2 dirA = normalize(dirFromPrev);
                    perp = float2(-dirA.y, dirA.x);
                }
                else {
                    float2 dirA = normalize(dirFromPrev);
                    float2 dirB = normalize(dirToNext);
                    float2 tangent = dirA + dirB;
                    float tlen = length(tangent);

                    if (tlen < EPSILON) {
                        perp = float2(-dirA.y, dirA.x);
                    } else {
                        tangent /= tlen;
                        float2 miter = float2(-tangent.y, tangent.x);
                        float2 normalA = float2(-dirA.y, dirA.x);
                        
                        float dotSign = dot(miter, normalA);
                        if (dotSign < 0.0) { miter = -miter; dotSign = -dotSign; }
                        
                        float scale = 1.0 / max(dotSign, EPSILON);
                        scale = min(scale, 10.0); // Miter limit
                        perp = miter * scale;
                    }
                }

                float2 offset = perp * (thicknessPx * 0.5 * side);
                float2 sNew = sCurrent + offset;
                float2 ndcNew = (sNew / screenSize - 0.5) * 2.0;

                o.clipPos = float4(ndcNew * clipCurrent.w, clipCurrent.z, clipCurrent.w);
                o.screenPos = ComputeScreenPos(o.clipPos);
                o.color = v.color * _GlobalColor;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Simple clip to prevent artifacts at extreme angles
                float2 uv = i.screenPos.xy / i.screenPos.w;
                if (any(uv < 0) || any(uv > 1)) discard;

                return i.color;
            }
            ENDCG
        }
    }
    FallBack Off
}