
Shader "Hidden/Atmosphere"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Center("Center", Vector) = (0, 0, 0)
		_MinRadius("MinRadius", Float) = 0
		_MaxRadius("MaxRadius", Float) = 125

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
				float _MaxRadius;
				float _MinRadius;

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

				fixed4 calculateSphere(v2f i)
				{
					float sceneDepthNonLinear = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
					float sceneDepth = LinearEyeDepth(sceneDepthNonLinear) * length(i.viewVector);

					float3 rayOrigin = _WorldSpaceCameraPos;
					float3 rayDir = normalize(i.viewVector);

					float2 hitInfo = raySphere(_Center, _MaxRadius, rayOrigin, rayDir);
					float distToSurface = sceneDepth;

					if (_MinRadius > 0)
					{
						float2 minSphere = raySphere(_Center, _MinRadius, rayOrigin, rayDir);
						distToSurface = min(minSphere.x, distToSurface);
					}

					float toAtmosphere = hitInfo.x;
					float inAtmosphere = min(hitInfo.y, distToSurface - toAtmosphere);

					if (toAtmosphere == maxFloat)
					{
						fixed4 col = tex2D(_MainTex, i.uv);
						return col;
					}
					return inAtmosphere / (_MaxRadius * 2);
				}

				float3 calculateLight(float3 rayOrigin, float3 rayDir, float rayLength)
				{
					return float3(0, 0, 0);
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
					return calculateSphere(i);
				}

				ENDCG
			}
		}
}
