// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/RSShader_justProjection" 
{
	Properties {
		_MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
		_DepthTex ("Vertex Modify", 2D) = "white" {}
		_PrevDepthTex ("Prev Vertex Modify", 2D) = "black" {}
		_ModAmount ("Modulation Amount", float) = 1.0
		_BackgroundSub("Background Point", float) = 0.5
		_DepthScale("Depth Scale", float) = 100
		
	}
	SubShader {
		Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
		}
		// TO DO: Change all params to be sliders
		ZWrite on
		Cull off
        Blend SrcAlpha OneMinusSrcAlpha
		//Fog { Mode Off }

		CGPROGRAM
		#pragma vertex vert
		#pragma surface surf BlinnPhong //alpha:transparent
		#pragma target 3.0
		#pragma glsl

		sampler2D _MainTex;
		sampler2D _DepthTex;
		sampler2D _PrevDepthTex;
		sampler2D _BackgroundTex;
		float _ModAmount;
		float _BackgroundSub;
		float _DepthScale;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
			float getRidOfThisPoint;
		};
		
		void vert(inout appdata_full v, out Input o) {
			float4 tex = tex2Dlod(_DepthTex, float4(v.texcoord.xy, 0, 0));
			UNITY_INITIALIZE_OUTPUT(Input,o);
			float3 projectionVec = normalize(v.vertex.xyz - float3(0,3,0));
			//v.vertex.y -= smoothstep(0,1,tex.r * _ModAmount);
			float d = tex.r;
			o.getRidOfThisPoint = 0;
			if (d == 0){
				//v.vertex.z = _DepthScale;
				tex = tex2Dlod(_PrevDepthTex, float4(v.texcoord.xy, 0, 0));
				d = tex.r;
				if (d == 0){
					d = 0;
					o.getRidOfThisPoint = 1;
					float j = 0;
					float k = 0;
					for(int i = 0 ; i < 5; i ++){
						j = sin(i * 0.6) * (i/20.);
						k = cos(i*0.6) * (i/20.);
						d = ((tex2Dlod(_DepthTex, float4(v.texcoord.x + k, v.texcoord.y + j, 0, 0))).r);
						if (d != 0){
							break;
						}
					}
					if (d == 0 ){
						d = 100;
					}
				}

			}
			d *= _DepthScale;
			v.vertex.xyz += (((d)) * projectionVec);
			
			
		}
		
		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			half4 b = tex2D (_DepthTex, IN.uv_MainTex);
			
			o.Albedo = c.rgb;

			float d = clamp((b.r) * _DepthScale, 0, 1.);
			float dd = b.r * _DepthScale;
			o.Alpha = 1;

			if(IN.getRidOfThisPoint == 1 ){
				o.Alpha = 0;
			}
			else if (dd == 0){
				o.Alpha = 0;
				//discard;
			
			}
			else{
				if (d> _BackgroundSub ){
					//discard;
					o.Alpha = (1-d)/_BackgroundSub;
				}
				else{
					o.Alpha = 1;
				}
				
			}
		}
		ENDCG
	} 
	FallBack "Diffuse"
}