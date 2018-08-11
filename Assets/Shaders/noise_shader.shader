// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "noise_shader" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex("Main Texture", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_NoiseTex("Noise Texture", 2D) = "white" {}
	}
	
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		//Physically based Standard lighting model, and enable shadows on all light types
		//- Standard means standard lightning
		//- vertex:vert to be able to modify the vertices
		//- addshadow to make the shadows look correct after modifying the vertices
		#pragma surface surf Standard vertex:vert addshadow
		// #pragma surface surf Standard fullforwardshadows
		//Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		#pragma glsl

		sampler2D _MainTex;
		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		sampler2D _NoiseTex;

		//parameters
		float _Scale;
		float _Speed;
		float _Distance;
		float _WaterTime;
		float _NoiseStrength;
		float _NoiseWalk;
		float _FlipAxis;
		float _CenterOfMass;

		float accum=0.0;
		int num = 0;
		struct Input 
		{
			float2 uv_MainTex;
		};

		//The wave function
		float3 getWavePos(float3 pos)
		{			
			// pos.y = 0.0;

			

			// pos.y += sin((_WaterTime * _Speed) / _Distance);// * _Scale;

			//Add noise
			// pos.x += tex2Dlod(_NoiseTex, float4(pos.y, pos.z + sin(_WaterTime* 0.1), 0.0, 0.0) * _NoiseWalk).a * _NoiseStrength;
			
			// tex2dlod ((object)The sampler state, (4vector)The texture coordinate) 
			// returns sampled texture 4 vec
			pos.y += (tex2Dlod(_NoiseTex, 
								float4(pos.x + cos(_WaterTime*_Distance), 
									   pos.z + sin(_WaterTime*_Distance), //+ 
									   0.0, 
									   0.0) 
								* _NoiseWalk).z

							  * _NoiseStrength * (_Scale*40));

			accum += (pos.y/2);
			num += 1;
			return pos;
		}

		void vert(inout appdata_full IN) 
		{
			// glPushMatrix();
			//Get the global position of the vertice
			float4 worldPos = mul(unity_ObjectToWorld, IN.vertex);

			//Manipulate the position
			float3 withWave = getWavePos(worldPos.xyz);

			//Convert the position back to local
			float4 localPos = mul(unity_WorldToObject, float4(withWave, worldPos.w));

			//Assign the modified vertice
			// IN.vertex = localPos;
			// - ((accum/)/(float)num)

			IN.vertex = float4(localPos.x,localPos.y ,localPos.z,localPos.w);
			// glPopMatrix();
		}

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			//Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			//Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		
		ENDCG
	}
	FallBack "Diffuse"
}