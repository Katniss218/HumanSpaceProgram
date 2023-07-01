
Shader "Hidden/Atmosphere"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Center("Center", Vector) = (0, 0, 0)
		_SunPosition("SunPosition", Vector) = (0, 0, 0)
		_MinRadius("MinRadius", Float) = 0
		_MaxRadius("MaxRadius", Float) = 125
		_InScatteringPointCount("InScatteringPointCount", Int) = 5
		_OpticalDepthPointCount("OpticalDepthPointCount", Int) = 5
		_DensityFalloffPower("DensityFalloffPower", Float) = 1.77

	}

		SubShader
		{
			// No culling or depth
			Cull Off ZWrite Off ZTest Always

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"
				#include "../AtmosphereRes/Math.cginc"
				#include "../AtmosphereRes/Triplanar.cginc"

				sampler2D _MainTex;
				sampler2D _CameraDepthTexture;
				float3 _Center;
				float3 _SunPosition;
				float _MaxRadius; // atmosphere radius (outer)
				float _MinRadius; // planet radius (atm inner)
				int _InScatteringPointCount;
				int _OpticalDepthPointCount;
				float _DensityFalloffPower;

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					float4 vertex : SV_POSITION;
					float3 viewVector : TEXCOORD1;
				};

				float rayleighPhaseFunction(float angle)
				{
					const float Pi16 = 50.2654824574;
					float cosAngle = cos(angle);
					return (3 * (1 + (cosAngle * cosAngle))) / Pi16;
				}

				// optimizations: possibly change the scattering point count based on the length between enter and exit points.

				float densityAtPoint(float3 samplePoint)
				{
					float heightAboveSurface = length(samplePoint - _Center) - _MinRadius;

					float height01 = heightAboveSurface / (_MaxRadius - _MinRadius);

					float localDensity = exp(-height01 * _DensityFalloffPower) * (1 - height01);
					return localDensity;
				}

				float opticalDepth(float3 samplePoint, float3 rayDir, float rayLength)
				{
					float stepSize = rayLength / (_OpticalDepthPointCount - 1);
					float3 rayStep = rayDir * stepSize;

					float opticalDepth = 0;
					for (int i = 0; i < _OpticalDepthPointCount; i++) // is there an analytic formula for the part of the exponential falloff?
					{
						float localDensity = densityAtPoint(samplePoint);
						opticalDepth += localDensity * stepSize;
						samplePoint += rayStep;
					}
					return opticalDepth;
				}

				float3 calculateLight(float3 rayOrigin, float3 rayDir, float rayLength)
				{
					float3 inScatterPoint = rayOrigin;
					float stepSize = rayLength / (_InScatteringPointCount - 1);
					float inScatteredLight = 0;
					float dirToSun = normalize(_SunPosition - _Center);
					float3 rayStep = rayDir * stepSize;

					for (int i = 0; i < _InScatteringPointCount; i++)
					{
						// calculate the length of the ray from the point to the edge of the atmosphere, in the direction of the sun.
						float2 toSun = raySphere(inScatterPoint, dirToSun, _Center, _MaxRadius);
						float lengthToSun = toSun.y;
						// TODO - there's some weird stuff going on.

						float sunRayOpticalDepth = opticalDepth(inScatterPoint, dirToSun, lengthToSun); // average density of the ray from the point to the edge in the direction towards the sun.
						
						//if (sunRayOpticalDepth <= 0)
						//	continue;
						
						float viewRayOpticalDepth = opticalDepth(inScatterPoint, -rayDir, stepSize * i); // I feel like this could be optimized.

						// how much light reaches the point.
						float transmittance = exp(-(sunRayOpticalDepth + viewRayOpticalDepth));
						float localDensity = densityAtPoint(inScatterPoint);

						inScatteredLight += localDensity * transmittance * stepSize;
						inScatterPoint += rayStep;
					}

					return inScatteredLight;
				}

				fixed4 calculateSphere(v2f i)
				{
					float sceneDepthNonLinear = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
					float sceneDepth = LinearEyeDepth(sceneDepthNonLinear) * length(i.viewVector);

					float3 rayOrigin = _WorldSpaceCameraPos;
					float3 rayDir = normalize(i.viewVector);

					float2 hitInfo = raySphere(rayOrigin, rayDir, _Center, _MaxRadius);
					float distToSurface = sceneDepth;

					float2 minSphere = raySphere(rayOrigin, rayDir, _Center, _MinRadius);
					distToSurface = min(minSphere.x, distToSurface);

					float toAtmosphere = hitInfo.x;
					float inAtmosphere = min(hitInfo.y, distToSurface - toAtmosphere);

					if (toAtmosphere == maxFloat)
					{
						fixed4 col = tex2D(_MainTex, i.uv);
						return col;
					}
					return inAtmosphere / (_MaxRadius * 2);
				}

				fixed4 calculateSphere2(v2f i)
				{
					float sceneDepthNonLinear = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
					float sceneDepth = LinearEyeDepth(sceneDepthNonLinear) * length(i.viewVector);

					float3 rayOrigin = _WorldSpaceCameraPos;
					float3 rayDir = normalize(i.viewVector);

					float2 hitTop = raySphere(rayOrigin, rayDir, _Center, _MaxRadius);
					float distToSurface = sceneDepth;

					float2 hitSurface = raySphere(rayOrigin, rayDir, _Center, _MinRadius);
					distToSurface = min(hitSurface.x, distToSurface);

					float toAtmosphere = hitTop.x;
					float inAtmosphere = min(hitTop.y, distToSurface - toAtmosphere);

					fixed4 col = tex2D(_MainTex, i.uv);

					// if ray in atmosphere
					if (inAtmosphere > 0) // important.
					{
						float3 pointInAtmosphere = rayOrigin + (rayDir * toAtmosphere);
						float light = calculateLight(pointInAtmosphere, rayDir, inAtmosphere);
						return (col * (1 - light)) + light;
					}
					return col;
				}

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;

					// Camera space matches OpenGL convention where cam forward is Z-, in Unity, forward is Z+ - https://docs.unity3d.com/ScriptReference/Camera-cameraToWorldMatrix.html
					float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv.xy * 2 - 1, 0, -1));
					o.viewVector = mul(unity_CameraToWorld, float4(viewVector, 0));

					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					return calculateSphere2(i);
				}

				ENDCG
			}
		}
}
