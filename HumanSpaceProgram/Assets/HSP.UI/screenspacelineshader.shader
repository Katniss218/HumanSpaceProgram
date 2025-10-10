Shader "Hidden/ScreenSpaceLine/CPUGenerated"
{
    Properties
    {
        _GlobalColor ("Global Color", Color) = (1,1,1,1)
        _DepthBias ("Depth Bias", Float) = 0.0005
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

            // vertex input - we use UV1 and UV2:
            struct appdata
            {
                float3 vertex : POSITION;    // world-space current point
                fixed4 color : COLOR;
                float4 uv1 : TEXCOORD1;     // next point xyz, thicknessPx in w
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

                // transform all points to clip space
                float4 clipCurrent = mul(UNITY_MATRIX_VP, float4(v.vertex, 1.0));
                float4 clipNext = mul(UNITY_MATRIX_VP, float4(v.uv1.xyz, 1.0));
                float4 clipPrev = mul(UNITY_MATRIX_VP, float4(v.uv2.xyz, 1.0));
                float thicknessPx = v.uv1.w;
                float side = v.uv2.w;

                // Depth output for bias
                o.depth = clipCurrent.z / clipCurrent.w;

                bool currentBehind = clipCurrent.w <= 0.0;
                bool nextBehind = clipNext.w <= 0.0;
                bool prevBehind = clipPrev.w <= 0.0;

                if (currentBehind)
                {
                    // Project to near plane
                    float nearW = EPSILON;
                    clipCurrent.xy = clipCurrent.xy * (nearW / clipCurrent.w);
                    clipCurrent.z = -1.0; // Behind everything
                    clipCurrent.w = nearW;
                }

                if (nextBehind)
                {
                    float nearW = EPSILON;
                    clipNext.xy = clipNext.xy * (nearW / clipNext.w);
                    clipNext.z = -1.0;
                    clipNext.w = nearW;
                }

                if (prevBehind)
                {
                    float nearW = EPSILON;
                    clipPrev.xy = clipPrev.xy * (nearW / clipPrev.w);
                    clipPrev.z = -1.0;
                    clipPrev.w = nearW;
                }

                float2 ndcCurrent = clipCurrent.xy / clipCurrent.w;
                float2 ndcNext = clipNext.xy / clipNext.w;
                float2 ndcPrev = clipPrev.xy / clipPrev.w;

                // pixel coordinates
                float2 screenSize = float2(_ScreenParams.x, _ScreenParams.y);
                float2 sCurrent = (ndcCurrent * 0.5 + 0.5) * screenSize;
                float2 sNext = (ndcNext * 0.5 + 0.5) * screenSize;
                float2 sPrev = (ndcPrev * 0.5 + 0.5) * screenSize;

                // dirs between points.
                float2 dirToNext = sNext - sCurrent;
                float2 dirFromPrev = sCurrent - sPrev;

                float2 perp; // normal direction of the corner (angle in = angle out).

                float lenToNext = length(dirToNext);
                float lenFromPrev = length(dirFromPrev);
                
                // Both directions degenerate -> just use vertical
                if (lenToNext < EPSILON && lenFromPrev < EPSILON)
                {
                    perp = float2(0.0, 1.0);
                }
                // Only prev degenerate -> use perpendicular of next
                else if (lenFromPrev < EPSILON)
                {
                    float2 dirB = normalize(dirToNext);
                    perp = float2(-dirB.y, dirB.x);
                }
                // Only next degenerate -> use perpendicular of prev
                else if (lenToNext < EPSILON)
                {
                    float2 dirA = normalize(dirFromPrev);
                    perp = float2(-dirA.y, dirA.x);
                }
                // Both directions valid -> compute miter
                else
                {
                    float2 dirA = normalize(dirFromPrev);
                    float2 dirB = normalize(dirToNext);

                    float2 tangent = dirA + dirB;
                    float tlen = length(tangent);

                    if (tlen < EPSILON) // Sharp (near 180 deg) corner
                    {
                        perp = float2(-dirA.y, dirA.x);
                    }
                    else
                    {
                        tangent = tangent / tlen; // normalize
                        // miter is perpendicular to the tangent
                        float2 miter = float2(-tangent.y, tangent.x);

                        // normal of the "reference" segment (use incoming segment's normal)
                        float2 normalA = float2(-dirA.y, dirA.x);

                        // ensure miter points to the same side as normalA (for consistent sign)
                        float dotSign = dot(miter, normalA);
                        if (dotSign < 0.0)
                        {
                            miter = -miter;
                            dotSign = -dotSign;
                        }

                        // scale factor so that projected thickness along normalA equals requested half-thickness
                        // scale = 1 / dot(miter, normalA)
                        float denom = max(dotSign, EPSILON);
                        float scale = 1.0 / denom;

                        const float MITER_LIMIT = 10.0;
                        scale = min(scale, MITER_LIMIT);

                        perp = miter * scale;
                    }
                }

                // Thickness
                float2 offset = perp * (thicknessPx * 0.5 * side);

                float2 sNew = sCurrent + offset;

                // Convert pixels back to NDC
                float2 ndcNew = (sNew / screenSize - 0.5) * 2.0;

                float4 newClipPos = float4(ndcNew * clipCurrent.w, clipCurrent.z, clipCurrent.w);

                o.clipPos = newClipPos;
                o.screenPos = ComputeScreenPos(o.clipPos);
                o.color = v.color * _GlobalColor;
                return o;
            }

            struct fragOutput
            {
                fixed4 color : SV_Target;
                float depth : SV_Depth;
            };

            fragOutput frag(v2f i) : SV_Target
            {
                fragOutput o;
                
                float2 uv = i.screenPos.xy / i.screenPos.w;
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                    discard;

                o.color = i.color;
                o.depth = i.depth + _DepthBias; // Apply depth bias to prevent z-fighting
                
                return o;
            }
            ENDCG
        }
    }
    FallBack Off
}