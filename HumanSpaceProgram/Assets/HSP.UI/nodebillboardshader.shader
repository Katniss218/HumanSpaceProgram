Shader "Hidden/HSP/NodeBillboard"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _InnerRadius ("Inner Radius", Range(0,1)) = 0 // 0 = Solid circle, >0 = Ring
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+1" }
        // Draw on top of lines (Lines are ZWrite On). 
        // We use ZTest LEqual but usually render after opaque.
        ZWrite Off 
        ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
                UNITY_DEFINE_INSTANCED_PROP(float, _InnerRadius)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(o, v);

                // Billboard Logic:
                // 1. Get center of instance in View Space
                float3 centerView = UnityObjectToViewPos(float3(0,0,0));
                
                // 2. Extract scale from the Instance Matrix
                // (Assumption: Uniform scale)
                float scale = length(float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].x, unity_ObjectToWorld[2].x));

                // 3. Offset vertex in View Space (keeps quad facing camera)
                // v.vertex is usually (-0.5, -0.5) to (0.5, 0.5)
                float3 offset = v.vertex.xyz * scale;
                
                // 4. Final View Position
                float3 finalViewPos = centerView + offset;

                o.pos = mul(UNITY_MATRIX_P, float4(finalViewPos, 1));
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float4 col = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                float innerRad = UNITY_ACCESS_INSTANCED_PROP(Props, _InnerRadius);

                // Circular SDF
                // UVs are 0..1, center is 0.5
                float2 d = i.uv - 0.5;
                float dist = length(d) * 2.0; // Map to 0..1 range (0 at center, 1 at edge)

                // Anti-aliasing delta
                float delta = fwidth(dist);
                float alpha = smoothstep(1.0, 1.0 - delta, dist);

                // Ring: Cut out inner hole
                if (innerRad > 0)
                {
                    float alphaInner = smoothstep(innerRad, innerRad + delta, dist);
                    alpha *= alphaInner;
                }

                col.a *= alpha;
                return col;
            }
            ENDCG
        }
    }
}