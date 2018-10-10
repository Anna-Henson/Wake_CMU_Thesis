Shader "KriptoFX/ME/GlowCutoutGradient" {
	Properties{
	[HDR]_TintColor("Tint Color", Color) = (0.5,0.5,0.5,1)
	_GradientStrength("Gradient Strength", Float) = 0.5
	_TimeScale("Time Scale", Vector) = (1,1,1,1)
	_MainTex("Noise Texture", 2D) = "white" {}
	_BorderScale("Border Scale (XY) Offset (Z)", Vector) = (0.5,0.05,1,1)
	}
		Category{

		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
						
		SubShader{
			Pass {
				Blend DstColor Zero
				Cull Off
				Lighting Off
				ZWrite Off
				Offset -1, -1

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				float4 _TintColor;
				float _GradientStrength;

				struct appdata_t {
					float4 vertex : POSITION;
				};

				struct v2f {
					float4 vertex : POSITION;
				};

				v2f vert(appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					return o;
				}
		

				half4 frag(v2f i) : COLOR
				{
					return lerp(1, _GradientStrength, _TintColor.a);
				}
				ENDCG
			}

			Pass {
				Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
				Blend SrcAlpha One
				Cull Off
				Lighting Off
				ZWrite Off
				Offset -1, -1

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				sampler2D _MainTex;
				float4 _TintColor;
				float4 _TimeScale;
				float4 _BorderScale;

				struct appdata_t {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
					float3 normal : NORMAL;
				};

				struct v2f {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
					float4 worldPosScaled : TEXCOORD1;
					float3 normal : NORMAL;
					
				};

				float4 _MainTex_ST;

				v2f vert(appdata_t v)
				{
					v2f o;
					//v.vertex.xyz += v.normal / 100 * _BorderScale.z;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.color = v.color;
					o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
					float3 worldPos = v.vertex * float3(length(unity_ObjectToWorld[0].xyz), length(unity_ObjectToWorld[1].xyz), length(unity_ObjectToWorld[2].xyz));
					o.worldPosScaled.x = worldPos.x *  _MainTex_ST.x;
					o.worldPosScaled.y = worldPos.y *  _MainTex_ST.y;
					o.worldPosScaled.z = worldPos.z *  _MainTex_ST.x;
					o.worldPosScaled.w = worldPos.z *  _MainTex_ST.y;
					o.normal = abs (v.normal);
					return o;
				}

				sampler2D _CameraDepthTexture;

				half tex2DTriplanar(sampler2D tex, float2 offset, float4 worldPos, float3 normal)
				{
					half3 texColor;
					texColor.x = tex2D(tex, worldPos.zy + offset);
					texColor.y = tex2D(tex, worldPos.xw + offset);
					texColor.z = tex2D(tex, worldPos.xy + offset);
					normal = normal / (normal.x + normal.y + normal.z);
					return dot(texColor, normal);
				}

				half4 frag(v2f i) : COLOR
				{
					half mask = tex2DTriplanar(_MainTex, _Time.x * _TimeScale.xy, i.worldPosScaled, i.normal);
					half tex = tex2DTriplanar(_MainTex, _Time.x * _TimeScale.zw + mask * _BorderScale.x, i.worldPosScaled, i.normal);
					half alphaMask = tex2DTriplanar(_MainTex, 0.3 + mask * _BorderScale.y, i.worldPosScaled, i.normal);
					float4 res;
					res = tex * mask * i.color * _TintColor;
					res = lerp(0, res, alphaMask);
					res.rgb = pow(res.rgb, _BorderScale.w);
					res.a = saturate(res.a * 4);
					return  res;
				}
				ENDCG
			}
		}

		}
}