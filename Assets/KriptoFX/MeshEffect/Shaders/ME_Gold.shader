Shader "KriptoFX/ME/Gold" {
	Properties {
		_MainColor("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Cutoff("Cutoff", Range(0,.9)) = .5 
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_SpeedDistort("Speed(XY) Distort(ZW)", Vector) = (1,0,0,0)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		//AlphaTest Greater [_Cutoff]
		Cull Off
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows 

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Cutoff;
		half _Glossiness;
		half _Metallic;
		fixed4 _MainColor;
		half4 _SpeedDistort;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 mask = tex2D (_MainTex, IN.uv_MainTex + _Time.x * _SpeedDistort.xy);
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex + mask * _SpeedDistort.zw -  _Time.x * _SpeedDistort.xy  * 1.4);
			if(c.r > _MainColor.a - (1-_Cutoff)) discard;
			o.Albedo = _MainColor;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	Fallback "Transparent/Cutout/Diffuse"
}