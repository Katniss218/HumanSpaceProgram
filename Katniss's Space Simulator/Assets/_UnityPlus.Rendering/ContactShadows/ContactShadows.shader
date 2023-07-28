Shader "Hidden/ContactShadows"
{
	Properties
	{
	}

	SubShader
	{
		Cull Off ZWrite Off ZTest Always
		
		Pass
		{
			// adapted from https://github.com/keijiro/ContactShadows

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _ShadowMask;
			sampler2D _CameraDepthTexture;
			float3 _LightDir;
			float _ShadowStrength;
			float _ShadowDistance;
			float _RayLength;
			float _Thickness;
			float _Bias;
			int _SampleCount;

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

			float3 ViewSpaceToClipSpace(float3 vp)
			{
				float4 clipPos = mul(unity_CameraProjection, float4(vp.xy, -vp.z, 1));
				return clipPos.xyz;
			}

			float CalculateShadows(float2 uv)
			{
				float mask = tex2D(_ShadowMask, uv).r;
				if (mask < 0.01)
					return mask; // if already in shadow, return that.

				float3 lightDirUV = ViewSpaceToClipSpace(_LightDir * _RayLength); // original _LightDir is in view space (aligned with camera, meter units).

				float depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv)); // depth value is in view space.

				float2 uvStep = lightDirUV.xy / _SampleCount;
				float depthChangeRayTotal = _LightDir.z * _RayLength; // total depth change from the start to the end of the ray (ray is in view space already, so we take the component pointing towards the screen).
				float depthStep = depthChangeRayTotal / _SampleCount; // So this is also in view space.
				UNITY_LOOP for (int i = 0; i < _SampleCount; i++)
				{
					float depth2 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv + (uvStep * i))) - (depthStep * i);

					// there's still something wrong here, it makes too much stuff dark.

					float diff = depth - depth2;
					if (diff > _Bias && diff < _Thickness) // depth2 can't be too close to, or too much in front of depth.
						return 0;
				}
				return 1;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return CalculateShadows(i.uv);
			}

			ENDHLSL
		}
		Pass
		{
			// adapted from https://github.com/keijiro/ContactShadows

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _ShadowMask;
			sampler2D _CameraDepthTexture;
			float3 _LightDir;
			float _ShadowStrength;
			float _ShadowDistance;
			float _RayLength;
			float _Thickness;
			float _Bias;
			int _SampleCount;

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

			// Get a raw depth from the depth buffer.
			float SampleRawDepth(float2 uv)
			{
				float z = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv); // equivalent to sampling the red channel.
#if defined(UNITY_REVERSED_Z) // needed.
				z = 1 - z;
#endif
				return z;
			}

			// Project view space into clip space.
			float3 ViewSpaceToClipSpace(float3 vp)
			{
				float4 clipPos = mul(unity_CameraProjection, float4(vp.xy, -vp.z, 1)); // replacing with UNITY_MATRIX_P breaks things.
				return (clipPos.xyz / clipPos.w + 1) * 0.5;
			}

			// Project clip space and depth into view space. Inverse of ViewSpaceToClipSpace
			float3 ClipSpaceToViewSpace(float2 uv, float z)
			{
				float4 cp = float4(float3(uv, z) * 2 - 1, 1);
				float4 vp = mul(unity_CameraInvProjection, cp);
				return float3(vp.xy, -vp.z) / vp.w;
			}

			float CalculateShadows(float2 uv)
			{
				float mask = tex2D(_ShadowMask, uv).r;
				if (mask < 0.01)
					return mask; // if already in shadow, return that.

				float depth = SampleRawDepth(uv);
				if (depth > 0.999999)
					return mask; // Background.

				float3 originalPosViewSpace = ClipSpaceToViewSpace(uv, depth);

				float3 step = (_LightDir * _RayLength) / _SampleCount; // in local object space.

				// Raymarch from the pixel to the light.
				UNITY_LOOP for (int i = 0; i < _SampleCount; i++)
				{
					float3 positionSampleVS = originalPosViewSpace + (step * i); // we could do exponential steps too.

					// clip (uv) space of the sample.
					float2 uvSample = ViewSpaceToClipSpace(positionSampleVS);

					float depthSampleVS = ClipSpaceToViewSpace(uvSample, SampleRawDepth(uvSample)).z;

					// todo - this could be marched in clip space and in linear depth units, and reduce the number of samples, and increase accuracy.

					// Negative: Ray sample is closer to the camera (not occluded)
					// Positive: Ray sample is beyond the depth sample (possibly occluded)
					float diff = positionSampleVS.z - depthSampleVS;

					if (diff > _Bias && diff < _Thickness)
						return 0;
				}

				return mask;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return CalculateShadows(i.uv);
			}
				 
			ENDHLSL
		}
	}
}
