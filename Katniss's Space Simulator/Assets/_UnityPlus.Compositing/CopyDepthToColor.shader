Shader "Hidden/CopyDepthToColor"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _InputMin("Input Min", Float) = 1
        _InputMax("Input Max", Float) = 2
        _OutputMin("Output Min", Float) = 1
        _OutputMax("Output Max", Float) = 2
    }

    SubShader
    {
        Cull Off ZWrite On ZTest Always

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

            float Remap(float value, float minInput, float maxInput, float minOutput, float maxOutput) 
            {
                return minOutput + (value - minInput) * (maxOutput - minOutput) / (maxInput - minInput);
            }

            float RawDepthToLinearDepth(float depth)
            {
                // https://www.vertexfragment.com/ramblings/unity-custom-depth/
                return 1.0 / (_ZBufferParams.x * depth + _ZBufferParams.y);
            }
            float LinearDepthToRawDepth(float linearDepth)
            {
                // https://www.vertexfragment.com/ramblings/unity-custom-depth/
                return (1.0f - (linearDepth * _ZBufferParams.y)) / (linearDepth * _ZBufferParams.x);
            }

            float frag(v2f i) : SV_Target
            {
                float col = Remap(
                    RawDepthToLinearDepth(tex2D(_CameraDepthTexture, i.uv).r),
                    0,
                    1,
                    _OutputMin,
                    _OutputMax
                );

                return col;
            }
            ENDCG
        }
    }
}
