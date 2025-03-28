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
			
			float3 ViewSpaceToClipSpace(float3 viewPos)
			{
				float4 clipPos = mul(unity_CameraProjection, float4(viewPos, 1));
				return clipPos.xyz;
			}

			fixed4 CalculateShadows(float2 uv)
			{
				float mask = tex2D(_ShadowMask, uv).r;
				//if (mask < 0.01)
				//	return mask; // if already in shadow, return that.

				float rawDepth = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, uv );
				float linearDepth = LinearEyeDepth( rawDepth );

				if (linearDepth > _ShadowDistance)
					return mask; // Background.

				float2 uvChangeRayTotal = ViewSpaceToClipSpace( _LightDir * _RayLength ).xy; // _LightDir is in view space (local space of camera).
				
				float2 uvStep = uvChangeRayTotal / _SampleCount;

	//	THE UV STEP APPEARS TO BE WRONG, IT'S NOT IN CORRECT METERS AT GIVEN DEPTH FOR SURE. THE AXES AND DIRECTIONS SEEM ALRIGHT THOUGH.

			// Needs to project 2 points, one at origin, one at (origin + ray), and subtract one from the other to account for perspective.



				// Linear depth increases further from camera (i.e. it's a distance from camera pixel).
				// `_LightDir.z * _RayLength` decreases when the light is behind the camera (the distance at the tip of the ray is smaller than the base)

				float depthChangeRayTotal = _LightDir.z * _RayLength; // _LightDir is in view space (local space of camera).
				float depthStep = depthChangeRayTotal / _SampleCount;
				
			// THE HORIZONTAL UV STEP SOMEHOW DEPENDS ON THE VERTICAL ANGLE OF THE CAMERA. EVEN THOUGH uvChangeRayTotal DOESN'T CHANGE, 
			//	 AND ONLY WHEN THE LIGHT IS NOT AT RIGHT ANGLE TO THE CAMERA

				//return float4(uvChangeRayTotal.x, uvChangeRayTotal.y, 0, 0);
				//return float4(0, uvChangeRayTotal.y, 0, 0);
				//return float4(-uvChangeRayTotal.x, 0, 0, 0);

				//return float4(depthChangeRayTotal, 0, 0, 0);

				UNITY_LOOP for (int i = 1; i < _SampleCount + 1; i++) // loop starts at 1 instead of 0 since it makes no sense to sample our own pixel.
				{
					//float rawDepthAtRaySample = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, uv + (uvStep * i) );
					float sampleU = clamp( uv.x + uvStep.x * i, 0, 1 );
					float sampleV = clamp( uv.y + uvStep.y * i, 0, 1 );

					float rawDepthAtRaySample = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, float2(sampleU, sampleV) );
					float linearDepthAtRaySample = LinearEyeDepth( rawDepthAtRaySample );

					float distanceToRaySample = linearDepth + (depthStep * i);

					//float diff = (distanceToRaySample - _Bias) - linearDepthAtRaySample;
					float diff = (distanceToRaySample - 0) - linearDepthAtRaySample;

					return float4( diff, diff, 0.0, 0.0);

		// PIXELS NEAR THE EDGES WILL OVERFLOW AND SAMPLE WRONG DEPTH UVs.

					// there's still something wrong here, it makes too much stuff dark.

					float biasedSampleDepth = distanceToRaySample - _Bias;

					if( biasedSampleDepth > linearDepthAtRaySample )
					{
						return 1 - _ShadowStrength;
					}
				}

				return float4(mask, 0, 0, 0);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return CalculateShadows(i.uv);
			}

			ENDHLSL
		}


		//

		// Below is not used.
		// Below is not used.
		// Below is not used.
		// Below is not used.
		//
		//

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
				// the idea here is to calculate the texture UV march space, and the distance is marched in real meter units for accuracy.

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
