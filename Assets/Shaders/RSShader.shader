Shader "Custom/RSShader" 
{
	Properties {
		_MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
		_DepthTex ("Vertex Modify", 2D) = "white" {}
		_PrevDepthTex ("Prev Vertex Modify", 2D) = "black" {}
		_WindowSize ("Window size", float) = 2.5
		_BackgroundSub("Background Point", float) = 0.5
		_DepthScale("Depth Scale", float) = 1000
		_Clip("toggle window mask 1 for on 0 for off", float) = 1
		
	}
	SubShader {
		Tags
        {
             "Queue"="Transparent" "RenderType"="TransparentCutout" "IgnoreProjector"="True" 
		}
		
		
		ZTest LEqual
        ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		Cull back
		AlphaToMask On
		
		//Fog { Mode Off }

		CGPROGRAM
		#include "UnityCG.cginc"

		#pragma vertex vert
		#pragma surface surf BlinnPhong  alphatest:_Cutoff alpha
		#pragma target 4.0
		#pragma glsl
		#pragma debug

		sampler2D _MainTex;
		sampler2D _DepthTex;
		sampler2D _PrevDepthTex;
		sampler2D _BackgroundTex;
		float _WindowDistance;
		float _WindowSize;
		float _BackgroundSub;
		float _DepthScale;
		float _Clip;
		float _Magnitude;

		struct Input {
			float2 uv_MainTex;
			float3 modelPos;
			float getRidOfThisPoint;
			float3 modelNormal;
			INTERNAL_DATA
		};
		
		void vert(inout appdata_full v, out Input o) {
			float4 tex = tex2Dlod(_DepthTex, float4(v.texcoord.xy, 0, 0));
			UNITY_INITIALIZE_OUTPUT(Input,o);
			//v.vertex.y -= smoothstep(0,1,tex.r * _ModAmount);
			float d = tex.r;
			o.getRidOfThisPoint = 0;

			float rs_planeZDist = 3.5;

			float3 projectionVec = normalize(v.vertex.xyz - float3(0,rs_planeZDist,0));

			if (d == 0){
				//o.getRidOfThisPoint = 1;
				tex = tex2Dlod(_PrevDepthTex, float4(v.texcoord.xy, 0, 0));
				d = tex.r;
				if (d == 0){
					
					float xOffset = 0;
					float yOffset = 0;
					float2 dirVector = normalize(float2(0.5- v.texcoord.x, 0.5 - v.texcoord.y));
					for(int i = 1 ; i < 11; i ++){
						
						xOffset = dirVector.x * (i/30.);
						yOffset = dirVector.y * (i/30.);
						d = ((tex2Dlod(_DepthTex, float4(v.texcoord.x + xOffset, v.texcoord.y + yOffset, 0, 0))).r);
						if (d != 0 && d < _BackgroundSub){
							break;
						}
					}
					float dd = 0;
					for(int j = 1 ; j < 11; j ++){
						xOffset = dirVector.x * (j/30.);
						yOffset = dirVector.y * (j/30.);
						dd = ((tex2Dlod(_DepthTex, float4(v.texcoord.x - xOffset, v.texcoord.y - yOffset, 0, 0))).r);
						if (dd != 0 && dd < _BackgroundSub){
							break;
						}
					}

					if(dd != 0){
						if (d != 0){
							d = min(d,dd);
						}
						else{ // d == 0;
							d = dd;
						}
					}
					if (d == 0){
						o.getRidOfThisPoint = 1;
					}		
				}

			}

			d *= _DepthScale;
			v.vertex.xyz += d * projectionVec;	
			o.modelPos = v.vertex.xyz;
			o.modelNormal = v.normal;
		}
		
		void surf (Input IN, inout SurfaceOutput o) {
			float2 uv = IN.uv_MainTex;
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			half4 b = tex2D (_DepthTex, IN.uv_MainTex);
			
			o.Albedo = c.rgb;
			o.Alpha = 1;

		
			if (_Clip == 0){
				return;
			}

			float d = b.r * _DepthScale;
			float3 correctedPos = float3(IN.modelPos.x,IN.modelPos.y,IN.modelPos.z);
			float distFromCenter = distance(correctedPos, float3(0,IN.modelPos.y,0));

			
			if (distFromCenter > 2.5){
				o.Alpha = 1;
				o.Alpha = clamp(1-(distFromCenter - _WindowSize)*2,0,1.);
			}

			if(IN.getRidOfThisPoint == 1){
				o.Alpha = 0;
				discard;
			}
			else if (d == 0){
				o.Alpha = 0;
				discard;
			}
			else{
				if (d > _BackgroundSub ){
					//discard;
					o.Alpha = 0;
				}
	
			}

			//Cutoff the rim of the UV
			if (IN.uv_MainTex.x <= 0.005 || IN.uv_MainTex.y <= 0.005 || IN.uv_MainTex.x >= 0.995 || IN.uv_MainTex.y >= 0.995){
				o.Alpha = 0;
			}

			o.Albedo = IN.modelNormal;
			
		}

		ENDCG
	} 
	FallBack "Diffuse"
}