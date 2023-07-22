Shader "Hidden/CopyDepthFromColor"
{
	Properties
	{
		_garbage("_garbage", 2D) = "white" {}
	}

		SubShader
	{
		Cull Off ZWrite On ZTest Always Blend One Zero // ZWrite On is needed to write to SV_Depth, idk why. Works with ZTest Less and Always (and possibly other)

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

			// Values used to linearize the Z buffer (http://www.humus.name/temp/Linearize%20depth.txt)
			// x = 1-far/near
			// y = far/near
			// z = x/far
			// w = y/far
			// or in case of a reversed depth buffer (UNITY_REVERSED_Z is 1)
			// x = -1+far/near
			// y = 1
			// z = x/far
			// w = 1/far

			float RawDepthToLinearDepth(float depth) // Unity 2019's Linear01Depth
			{
				return 1.0 / (_ZBufferParams.x * depth + _ZBufferParams.y);
			}

			float InverseRawToLinear(float linearDepth)
			{
				// according to https://www.mathway.com/Precalculus, for `1.0 / (_ZBufferParams.x * depth + _ZBufferParams.y)`
				return (1 / (linearDepth * _ZBufferParams.x)) - (_ZBufferParams.y / _ZBufferParams.x);
			}

			float LinearDepthToRawDepth(float linearDepth)
			{
				// https://www.vertexfragment.com/ramblings/unity-custom-depth/
				return (1.0f - (linearDepth * _ZBufferParams.y)) / (linearDepth * _ZBufferParams.x);
			}

			sampler2D _garbage;

			struct fragOutput
			{
				fixed4 color : SV_Target;
				float depth : SV_Depth;
			};

			fragOutput frag(v2f i)
			{
				fragOutput o;

				float depthTex = tex2D(_garbage, i.uv).r; // Texture values are wrong in commandbuffer, but correct in blit inside onprerender.

				o.color = fixed4(depthTex,1,0,0);
				depthTex = InverseRawToLinear(depthTex);
				//o.depth = depthTex;
				o.depth = 0.00005f; // works in commandbuffer, doesn't work in blit inside onprerender.

				return o;
			}
			ENDCG
		}
	}
}
