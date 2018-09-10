Shader "Custom/RSShaderEdge" 
{
	Properties {
		_MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
		_DepthTex ("Vertex Modify", 2D) = "white" {}
		_PrevDepthTex ("Prev Vertex Modify", 2D) = "black" {}
		_WindowSize ("Window size", float) = 2.5
		_BackgroundSub("Background Point", float) = 0.5
		_DepthScale("Depth Scale", float) = 1000
		_Clip("toggle window mask 1 for on 0 for off", float) = 1
		_ScanRange("Range of Visible Depth", float) = 0.005
		//Edge Detection Code with Roberts Operators
		_BackgroundColor ("Background Color", Color) = (1, 1, 1, 1)
		_SampleDistance("Sample Distance", float) = 1.0 //How far away should the samples be apart from each other
		_Sensitivity("Sensitivity", float) = 1.0 //Sensitivity to depth change
		_FadeOut("Fade Out Slider", range(0, 1)) = 0
		
	}

	SubShader {
		Pass {
			Tags
			{
				 "Queue"="AlphaTest" "RenderType"="Transparent" "IgnoreProjector"="True"  "DisableBatching"="True"
			}
		
			ZTest LEqual
			ZWrite off
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
		
			//Fog { Mode Off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#pragma target 4.0
			#pragma glsl
			#include "UnityCG.cginc"


			struct appdata {
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 uv_MainTex : TEXCOORD0;
				float4 modelPos : SV_POSITION;
				float4 getRidOfThisPoint: TEXCOORD3;
				float3 normal: NORMAL;
			};

			sampler2D _MainTex;
			half4 _MainTex_TexelSize;
			sampler2D _DepthTex;
			sampler2D _PrevDepthTex;
			sampler2D _BackgroundTex;
			float _WindowDistance;
			float _WindowSize;
			float _BackgroundSub;
			float _DepthScale;
			float _Clip;
			float _ScanRange;
			//Initiate Edge Detection Variables
			fixed4 _EdgeColor;
			float _EdgeOnly;
			fixed4 _BackgroundColor;
			float _Sensitivity;
			float _SampleDistance;
			float _FadeOut;
		
			v2f vert(appdata v) {
				v2f o;
				float2 uv = v.texcoord.xy;
				float4 tex = tex2Dlod(_DepthTex, float4(v.texcoord.xy, 0, 0));
				//UNITY_INITIALIZE_OUTPUT(Input,o);
				//v.vertex.y -= smoothstep(0,1,tex.r * _ModAmount);
				float d = tex.r;
				o.getRidOfThisPoint = float4(0,0,0,0);

				float rs_planeZDist = 3.5;

				float3 projectionVec = normalize(v.vertex.xyz - float3(0,rs_planeZDist,0));

				if (d == 0){
					o.getRidOfThisPoint.x = 1;
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
							d = 1;
						}		
					}

				}

				d *= _DepthScale;
				v.vertex.xyz = v.vertex.xyz + d * projectionVec;
				o.modelPos.xyz = v.vertex.xyz;
				o.getRidOfThisPoint.yzw = o.modelPos.xyz;
				o.modelPos = UnityObjectToClipPos(o.modelPos);
				o.uv_MainTex = v.texcoord;
				return o;
			}

			fixed4 frag (v2f IN) : SV_Target {
				fixed4 o;
				float2 uv = IN.uv_MainTex;
				half4 c = tex2D (_MainTex, IN.uv_MainTex);
				half4 b = tex2D (_DepthTex, IN.uv_MainTex);

				//-----------------Edge Detection-------------------------------------------------------------//
				half sample1 = tex2D(_DepthTex, uv + _MainTex_TexelSize.xy * half2(1, 1) * _SampleDistance).r;
				half sample2 = tex2D(_DepthTex, uv + _MainTex_TexelSize.xy * half2(-1, -1) * _SampleDistance).r;
				half sample3 = tex2D(_DepthTex, uv + _MainTex_TexelSize.xy * half2(-1, 1) * _SampleDistance).r;
				half sample4 = tex2D(_DepthTex, uv + _MainTex_TexelSize.xy * half2(1, -1) * _SampleDistance).r;

				half edge = 1.0;

				float diffDepth12 = abs(sample1 - sample2) * _Sensitivity;
				float diffDepth34 = abs(sample3 - sample4) * _Sensitivity;

				int isSameDepth12 = diffDepth12 < 0.1 * sample1;
				int isSameDepth34 = diffDepth34 < 0.1 * sample3;

				edge = edge * isSameDepth12 * isSameDepth34;
				//---------------End of Edge Detection---------------------------------------------------------//

				o.rgb = c.rgb;
				o.a = _FadeOut;

		
				if (_Clip == 0){
					return o;
				}

				float d = b.r * _DepthScale;
				float3 correctedPos = IN.getRidOfThisPoint.yzw;
				float distFromCenter = distance(correctedPos, float3(0,IN.getRidOfThisPoint.z,0));

			
				if (distFromCenter > 2.5){
					o.a = _FadeOut;
					o.a = clamp(_FadeOut-(distFromCenter - _WindowSize)*2,0,_FadeOut);
				}

				if(IN.getRidOfThisPoint.x == 1){
					o.a = 0;
					discard;
				}
				else if (d == 0){
					o.a = 0;
					discard;
				}
				else{
					if (d > _BackgroundSub || d < _BackgroundSub - _ScanRange * _DepthScale){
						//discard;
						o.a = 0;
					}
	
				}

				//Cutoff the rim of the UV
				if (IN.uv_MainTex.x <= 0.005 || IN.uv_MainTex.y <= 0.005 || IN.uv_MainTex.x >= 0.995 || IN.uv_MainTex.y >= 0.995){
					o.a = 0;
				}
			
				//Make Steep Depth Change Edge Transparent
				if (edge < 1.0) 			
				{	
					o.a = 0;
				}
				return o;
			}

			ENDCG
		}

	}
	
	FallBack "Transparent/Cutout/VertexLit"
	
}