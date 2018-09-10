Shader "Custom/BlendSubject"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_SecondTex ("Object Render Texture", 2D) = "white" {}
		_Contrast("Contrast", float) = 1.0
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.0
			#pragma glsl
			
			#include "UnityCG.cginc"

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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _SecondTex;
			float _Contrast;	

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				//col.rgb = pow(abs(col.rgb * 2 - 1), 1/max(_Contrast, 0.0001)) * sign(col.rgb - 0.5) + 0.5;
				fixed4 subCol = tex2D(_SecondTex, i.uv);
                col.rgb = subCol.rgb * subCol.a + col.rgb * (1 - subCol.a);
				return col;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
