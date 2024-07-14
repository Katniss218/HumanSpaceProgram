Shader "Hidden/CopyDepth"
{
	Properties
	{
		_Input1Depth("First Depth Texture", 2D) = "white" {}
		_Input1Near("First Near Plane", Float) = 0.03
		_Input1Far("First Far Plane", Float) = 1000.0

		_Input2Depth("Second Depth Texture", 2D) = "white" {}
		_Input2Near("Second Near Plane", Float) = 0.03
		_Input2Far("Second Far Plane", Float) = 1000.0

		_DstNear("Destination Near Plane", Float) = 0.03
		_DstFar("Destination Far Plane", Float) = 1000.0
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
				//return 1.0 / (_ZBufferParams.z * linearDepth + _ZBufferParams.w);
				
				// but this works better when rendering the atmosphere.
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

			sampler2D _Input1Depth;
			float _Input1Near;
			float _Input1Far;

			sampler2D _Input2Depth;
			float _Input2Near;
			float _Input2Far;

			float _DstNear;
			float _DstFar;

			//
			//	PASS #1

			//	Pass 1 uses both depth textures. It's used when both cameras are enabled.
			//

			fragOutput frag(v2f i)
			{
				fragOutput o;

				float4 _Input1ZBufferParams = GetZBufferParams(_Input1Near, _Input1Far, UNITY_REVERSED_Z);
				float4 _Input2ZBufferParams = GetZBufferParams(_Input2Near, _Input2Far, UNITY_REVERSED_Z);
				float4 _DstZBufferParams = GetZBufferParams(_DstNear, _DstFar, UNITY_REVERSED_Z);

				float input1RawDepth = SAMPLE_DEPTH_TEXTURE(_Input1Depth, i.uv);
				float input2RawDepth = SAMPLE_DEPTH_TEXTURE(_Input2Depth, i.uv);
				
				// camera depth buffer in linear distance is always in range [near..far]

				float input1LinearDepth = RawDepthToLinearDepth(input1RawDepth, _Input1ZBufferParams);
				float input2LinearDepth = RawDepthToLinearDepth(input2RawDepth, _Input2ZBufferParams);

				// Assumes that '2's far plane is closer than '1's far plane
				if( input2LinearDepth > (_Input2Far - 0.1) )
					input2LinearDepth = _Input1Far;
				
				float minLinearDepth = min(input1LinearDepth, input2LinearDepth);
				
				float outputRawDepth = LinearDepthToRawDepth(minLinearDepth, _DstZBufferParams);

				o.depth = outputRawDepth;
				o.color = fixed4(0, 0, 0, 0); // Use for debugging, displays the depth as yellow in the background of the front.
				
				return o;
			}
			ENDCG
		}

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
				//return 1.0 / (_ZBufferParams.z * linearDepth + _ZBufferParams.w);
				
				// but this works better when rendering the atmosphere.
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

			sampler2D _Input1Depth;
			float _Input1Near;
			float _Input1Far;

			sampler2D _Input2Depth;
			float _Input2Near;
			float _Input2Far;

			float _DstNear;
			float _DstFar;
			
			//
			//	PASS #2

			//	Pass 2 uses only the 'far' depth texture input. It's used when the near camera is disabled.
			//

			fragOutput frag(v2f i)
			{
				fragOutput o;

				float4 _Input1ZBufferParams = GetZBufferParams(_Input1Near, _Input1Far, UNITY_REVERSED_Z);
				float4 _DstZBufferParams = GetZBufferParams(_DstNear, _DstFar, UNITY_REVERSED_Z);

				//float input1RawDepth = tex2D(_Input1Depth, i.uv).r;
				float input1RawDepth = SAMPLE_DEPTH_TEXTURE(_Input1Depth, i.uv);
				
				// camera depth buffer in linear distance is always in range [near..far]

				float input1LinearDepth = RawDepthToLinearDepth(input1RawDepth, _Input1ZBufferParams);

				float outputRawDepth = LinearDepthToRawDepth(input1LinearDepth, _DstZBufferParams);

				o.depth = outputRawDepth;
				o.color = fixed4(0, 0, 0, 0); // Use for debugging, displays the depth as yellow in the background of the front.
				
				return o;
			}
			ENDCG
		}
	}
}