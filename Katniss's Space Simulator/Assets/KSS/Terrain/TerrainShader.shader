Shader "Custom/TerrainShader"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_NormalTex("Normal (RGB)", 2D) = "white" {}
		_NormalStrength("Normal Strength", Range(-1,1)) = 0.5
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}

		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM

			#pragma surface surf Standard fullforwardshadows
			#pragma target 3.0

			sampler2D _MainTex;
			sampler2D _NormalTex;

			struct Input
			{
				float2 uv_MainTex;
				float3 worldNormal; INTERNAL_DATA
			};

			half _Glossiness;
			half _Metallic;
			half _NormalStrength;
			fixed4 _Color;

			UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_INSTANCING_BUFFER_END(Props)

			void surf(Input IN, inout SurfaceOutputStandard o)
			{
				// Albedo comes from a texture tinted by color
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

				o.Normal = UnpackScaleNormal(tex2D(_NormalTex, IN.uv_MainTex), _NormalStrength);

				o.Albedo = c.rgb;
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = c.a;
			}
			ENDCG
		}
		FallBack "Diffuse"
}