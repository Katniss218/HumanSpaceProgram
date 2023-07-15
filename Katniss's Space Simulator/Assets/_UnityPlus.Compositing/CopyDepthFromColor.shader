Shader "Hidden/CopyDepthFromColor"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Never

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
                float depth : DEPTH;
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

            fragOutput frag(v2f i)
            {
                fragOutput o;

                o.depth = LinearDepthToRawDepth(tex2D(_MainTex, i.uv).r);
                o.depth = 1.0f;

                //o.color = fixed4(0, 0, 0, 0); // clear image (temp).

                return o;
            }
            ENDCG
        }
    }
}
