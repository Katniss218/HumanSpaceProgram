Shader "Hidden/CopyDepth"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            struct fragOutput 
            {
                fixed4 color : SV_Target;
                float depth : SV_Depth;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            float _InputMin;
            float _InputMax;
            float _OutputMin;
            float _OutputMax;
            float4 _CameraParams;

            float Remap(float value, float minInput, float maxInput, float minOutput, float maxOutput) 
            {
                return minOutput + (value - minInput) * (maxOutput - minOutput) / (maxInput - minInput);
            }

            float LinearToNonlinearDepth(float linearDepth) // idfk
            {
                float nearClipPlane = _CameraParams.x;
                float farClipPlane = _CameraParams.y;

                float depthRange = farClipPlane - nearClipPlane;
                float nonlinearDepth = (2.0 / depthRange) * (nearClipPlane * farClipPlane) /
                    (farClipPlane + nearClipPlane - linearDepth * depthRange);

                return nonlinearDepth;
            }

            fragOutput frag(v2f i)
            {
                fragOutput o;

                o.depth = LinearToNonlinearDepth(tex2D(_MainTex, i.uv).r);

                return o;
            }
            ENDCG
        }
    }
}
