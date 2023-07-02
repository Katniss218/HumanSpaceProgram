
Shader "Hidden/Atmosphere"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Center("Center", Vector) = (0, 0, 0)
		_SunDirection("SunDirection", Vector) = (0, 0, 0)
			//_ScatteringCoefficients("ScatteringCoefficients", Vector) = (0.213244481466, 0.648883128925, 1.36602691073) // 700, 530, 440, mul 2
			_ScatteringWavelengths("ScatteringWavelengths", Vector) = (700, 530, 400)
			_ScatteringStrength("ScatteringStrength", Float) = 4
			_TerminatorFalloff("TerminatorFalloff", Float) = 32
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
				float3 _SunDirection;
				float3 _ScatteringCoefficients;
				float3 _ScatteringWavelengths;
				float _ScatteringStrength;
				float _TerminatorFalloff;
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
					// A value in [0..1] representing the thickness of the atmosphere.

					//float heightAboveSurface = length(samplePoint - _Center) - _MinRadius;
					float heightAboveSurface = length(samplePoint - _Center);

					//float height01 = heightAboveSurface / (_MaxRadius - _MinRadius);
					float height01 = heightAboveSurface / (_MaxRadius);
					if (height01 <= 0 || height01 >= 1)
						return 0;

					float localDensity = exp(-height01 * _DensityFalloffPower) * (1 - height01);
					return localDensity;
				}

				float opticalDepth(float3 samplePoint, float3 rayDir, float rayLength)
				{
					// I think this could be optimized with a lookup.
					// The ratio between the amount of incident light, and the amount of light transmitted to the point.
					float3 densitySamplePoint = samplePoint; // HLSL passes parameters by reference.
					float stepSize = rayLength / (_OpticalDepthPointCount - 1);
					float3 rayStep = rayDir * stepSize;

					float opticalDepth = 0;
					for (int i = 0; i < _OpticalDepthPointCount; i++)
					{
						float localDensity = densityAtPoint(samplePoint);
						opticalDepth += localDensity * stepSize;
						densitySamplePoint += rayStep;
					}
					return opticalDepth;
				}

				float3 calculateLight(float3 rayOrigin, float3 rayDir, float rayLength, float3 originalColor)
				{
					float3 dirToSun = normalize(_SunDirection);
					float stepSize = rayLength / (_InScatteringPointCount - 1);
					float3 rayStep = rayDir * stepSize;

					float viewRayOpticalDepth; // use the last iteration later.

					float3 inScatterPoint = rayOrigin; // HLSL passes parameters by reference.
					float3 inScatteredLight = 0;
					for (int i = 0; i < _InScatteringPointCount; i++)
					{
						float localDensity = densityAtPoint(inScatterPoint);

						// calculate the length of the ray from the point to the edge of the atmosphere, in the direction of the sun.
						float2 toSun = raySphere(inScatterPoint, dirToSun, _Center, _MaxRadius);
						float lengthToSun = toSun.y;
						float2 hitSurface = raySphere(inScatterPoint, dirToSun, _Center, _MinRadius);
						lengthToSun = min(hitSurface.x + hitSurface.y, lengthToSun);

						if (hitSurface.x != maxFloat)
						{
							//continue;
						}

						float sunRayOpticalDepth = opticalDepth(inScatterPoint, dirToSun, lengthToSun); // average density of the ray from the point to the edge in the direction towards the sun.
						viewRayOpticalDepth = opticalDepth(inScatterPoint, -rayDir, stepSize * i);

						//if (i != _InScatteringPointCount - 1) // terminator and blacking out the back
						//{
						//	float2 hitSurface = raySphere(inScatterPoint, dirToSun, _Center, _MinRadius);
						//	if (hitSurface.x != maxFloat)
						//	{
						//		sunRayOpticalDepth *= (hitSurface.y / _MaxRadius) * ((float(i) / float(_InScatteringPointCount)) * _TerminatorFalloff);
						//	}
						//}

						// how much light reaches the point.
						float3 transmittance = exp(-(sunRayOpticalDepth)*_ScatteringCoefficients) * exp(-(viewRayOpticalDepth)); // apparently no scattering on the view ray is the way to go.

						inScatteredLight += localDensity * transmittance * _ScatteringCoefficients * stepSize;
						inScatterPoint += rayStep;

					}

					float originalColorTransmittance = exp(-viewRayOpticalDepth); // viewRayOpticalDepth should be the optical depth for the entire view ray here.

					return originalColor * originalColorTransmittance + inScatteredLight;
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

					// raycast against the outer edge of the atmosphere
					float2 hitTop = raySphere(rayOrigin, rayDir, _Center, _MaxRadius);
					float distToSurface = sceneDepth;

					// and the inner edge (surface of the planet)
					float2 hitSurface = raySphere(rayOrigin, rayDir, _Center, _MinRadius);
					distToSurface = min(hitSurface.x, distToSurface);

					// calculate the distance from camera to atmosphere, and run of the ray through (in) the atmosphere.
					float toAtmosphere = hitTop.x;
					float inAtmosphere = min(hitTop.y, distToSurface - toAtmosphere);

					// hacky way for now so I can change wavelengths directly in the editor.
					_ScatteringCoefficients = ((400 / _ScatteringWavelengths) * (400 / _ScatteringWavelengths) * (400 / _ScatteringWavelengths) * (400 / _ScatteringWavelengths)) * _ScatteringStrength;

					fixed4 col = tex2D(_MainTex, i.uv);

					// if ray in atmosphere
					if (inAtmosphere > 0) // important.
					{
						float3 pointInAtmosphere = rayOrigin + (rayDir * toAtmosphere);
						float3 light = calculateLight(pointInAtmosphere, rayDir, inAtmosphere, col.xyz);
						return fixed4(light, col.w);
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
