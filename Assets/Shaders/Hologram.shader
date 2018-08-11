// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UtopiaWorx/Hologram" {
    Properties {
        _EmmisionColor ("Emmision Color", Color) = (0.3853807,0.5294118,0.4876924,0.5450981)
        _EmmisionPower ("Emmision Power", Range(0, 2)) = 1.641026
        _Frensel ("Frensel", Range(0, 20)) = 19.19611
        _ScanLines ("ScanLines", 2D) = "white" {}
        _Normal ("Normal", 2D) = "bump" {}
        _Noise ("Noise", Range(-20, 2)) = -5.976075
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "ForwardBase"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
            #pragma target 2.0
            uniform float4 _TimeEditor;
            uniform sampler2D _Normal; uniform float4 _Normal_ST;
            uniform float _Frensel;
            uniform float4 _EmmisionColor;
            uniform sampler2D _ScanLines; uniform float4 _ScanLines_ST;
            uniform float _EmmisionPower;
            uniform float _Noise;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 binormalDir : TEXCOORD4;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o;
                o.uv0 = v.texcoord0;
                o.normalDir = mul(float4(v.normal,0), unity_WorldToObject).xyz;
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.binormalDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.binormalDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
/////// Normals:
                float2 node_350 = i.uv0;
                float3 normalLocal = UnpackNormal(tex2D(_Normal,TRANSFORM_TEX(node_350.rg, _Normal))).rgb;
                float3 normalDirection =  normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                
                float nSign = sign( dot( viewDirection, i.normalDir ) ); // Reverse normal if this is a backface
                i.normalDir *= nSign;
                normalDirection *= nSign;
                
////// Lighting:
////// Emissive:
                float4 node_263 = _Time + _TimeEditor;
                float node_245_ang = node_263.r;
                float node_245_spd = 1.0;
                float node_245_cos = cos(node_245_spd*node_245_ang);
                float node_245_sin = sin(node_245_spd*node_245_ang);
                float2 node_245_piv = float2(0.5,0.5);
                float2 node_245 = (mul(i.uv0.rg-node_245_piv,float2x2( node_245_cos, -node_245_sin, node_245_sin, node_245_cos))+node_245_piv);
                float3 emissive = ((_EmmisionColor.rgb > 0.5 ?  (1.0-(1.0-2.0*(_EmmisionColor.rgb-0.5))*(1.0-tex2D(_ScanLines,TRANSFORM_TEX(node_245, _ScanLines)).a)) : (2.0*_EmmisionColor.rgb*tex2D(_ScanLines,TRANSFORM_TEX(node_245, _ScanLines)).a)) *_EmmisionPower);
                float3 finalColor = emissive;
                float3 node_92 = i.normalDir;
                float3 node_309 = (node_92/_Noise);
                float2 node_295_skew = node_309.rg + 0.2127+node_309.rg.x*0.3713*node_309.rg.y;
                float2 node_295_rnd = 4.789*sin(489.123*(node_295_skew));
                float node_295 = frac(node_295_rnd.x*node_295_rnd.y*(1+node_295_skew.x));
/// Final Color:
                return fixed4(finalColor,pow(1.0-max(0,dot(node_92, viewDirection)),(1.0-(1.0-_Frensel)*(1.0-node_295))));
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
