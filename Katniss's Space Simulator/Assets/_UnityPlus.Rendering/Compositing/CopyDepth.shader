Shader "Hidden/CopyDepth"
{
	Properties
	{
		_InputColor("Source Texture", 2D) = "white" {}
		_InputDepth("Source Texture", 2D) = "white" {}
		_SrcNear("Source Near Plane", Float) = 0.03
		_SrcFar("Source Far Plane", Float) = 0.03
		_DstNear("Destination Near Plane", Float) = 0.03
		_DstFar("Destination Far Plane", Float) = 0.03
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

			inline float4 GetZBufferParams(float near, float far, bool reversedZ)
			{
				float4 o;

				if (reversedZ)
				{
					o.x = (far / near) - 1;
					o.y = 1;
					o.z = o.x / far;
					o.w = 1 / far;
				}
				else
				{
					o.x = 1 - (far / near);
					o.y = far / near;
					o.z = o.x / far;
					o.w = o.y / far;
				}

				return o;
			}

			inline float RawDepthToLinearDepth(float z, float4 zBufferParams) // 2019's LinearEyeDepth
			{
				return 1.0 / (zBufferParams.z * z + zBufferParams.w);
			}

			inline float LinearDepthToRawDepth(float linearDepth, float4 zBufferParams)
			{
				// according to https://www.mathway.com/Precalculus, for `1.0 / (_ZBufferParams.z * depth + _ZBufferParams.w)`
				return (1 / (linearDepth * zBufferParams.z)) - (zBufferParams.w / zBufferParams.z);
			}

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

			struct fragOutput
			{
				fixed4 color : SV_Target;
				float depth : SV_Depth;
			};

			sampler2D _InputColor;
			sampler2D _InputDepth;
			float _SrcNear;
			float _SrcFar;
			float _DstNear;
			float _DstFar;

			fragOutput frag(v2f i)
			{
				fragOutput o;

				float4 _SrcZBufferParams = GetZBufferParams(_SrcNear, _SrcFar, UNITY_REVERSED_Z);
				float4 _DstZBufferParams = GetZBufferParams(_DstNear, _DstFar, UNITY_REVERSED_Z);

				float rawDepthIn = tex2D(_InputDepth, i.uv).r;
				float linearDepth = RawDepthToLinearDepth(rawDepthIn, _SrcZBufferParams);
				float rawDepthOut = LinearDepthToRawDepth(linearDepth, _DstZBufferParams);

				o.depth = rawDepthOut;
				o.color = tex2D(_InputColor, i.uv);
				//o.color = fixed4(rawDepthIn, 1, 0, 0); // Use for debugging, displays the depth as yellow in the background of the front.

				return o;
			}
			ENDCG
		}
	}
}
