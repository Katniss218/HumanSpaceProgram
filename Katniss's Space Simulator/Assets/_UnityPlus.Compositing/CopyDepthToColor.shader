Shader "Hidden/CopyDepthToColor"
{
	Properties
	{
	}

	SubShader
	{
		Cull Off ZWrite On ZTest Less // Works with ZTest Less and Always (and possibly other)

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

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _CameraDepthTexture;

			float RawDepthToLinear01Depth(float depth) // 2019's Linear01Depth
			{
				// https://www.vertexfragment.com/ramblings/unity-custom-depth/
				return 1.0 / (_ZBufferParams.x * depth + _ZBufferParams.y);
			}

			float Linear01DepthToRawDepth(float linearDepth)
			{
				// according to https://www.mathway.com/Precalculus, for `1.0 / (_ZBufferParams.x * depth + _ZBufferParams.y)`
				return (1 / (linearDepth * _ZBufferParams.x)) - (_ZBufferParams.y / _ZBufferParams.x);
			}

			inline float RawDepthToLinearDepth(float z) // 2019's LinearEyeDepth
			{
				return 1.0 / (_ZBufferParams.z * z + _ZBufferParams.w);
			}

			inline float LinearDepthToRawDepth(float linearDepth)
			{
				// according to https://www.mathway.com/Precalculus, for `1.0 / (_ZBufferParams.z * depth + _ZBufferParams.w)`
				return (1 / (linearDepth * _ZBufferParams.z)) - (_ZBufferParams.w / _ZBufferParams.z);
			}

			float frag(v2f i) : SV_Target
			{
				float col = RawDepthToLinearDepth(tex2D(_CameraDepthTexture, i.uv).r);

				return col;
			}
			ENDCG
		}
	}
}
