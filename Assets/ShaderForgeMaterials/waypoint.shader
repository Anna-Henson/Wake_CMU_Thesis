// Shader created with Shader Forge v1.38 
// Shader Forge (c) Freya Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:4013,x:33218,y:32422,varname:node_4013,prsc:2|emission-1665-OUT,alpha-8765-OUT;n:type:ShaderForge.SFN_Color,id:1304,x:32488,y:32136,ptovrint:False,ptlb:Color,ptin:_Color,varname:node_1304,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:0.9433962,c2:0.3604486,c3:0.3771043,c4:1;n:type:ShaderForge.SFN_Fresnel,id:5117,x:32559,y:32569,varname:node_5117,prsc:2|EXP-338-OUT;n:type:ShaderForge.SFN_Add,id:1515,x:32754,y:32468,varname:node_1515,prsc:2|A-3413-OUT,B-5117-OUT;n:type:ShaderForge.SFN_Multiply,id:1665,x:33004,y:32242,varname:node_1665,prsc:2|A-6759-OUT,B-1515-OUT;n:type:ShaderForge.SFN_ValueProperty,id:338,x:32232,y:32592,ptovrint:False,ptlb:_FresnelFac,ptin:__FresnelFac,varname:node_338,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Tex2dAsset,id:1280,x:32205,y:32333,ptovrint:False,ptlb:NoiseTex,ptin:_NoiseTex,varname:node_1280,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:bcba9da6cfeab4bc8a1b11323ea31db9,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:227,x:32488,y:32292,varname:node_227,prsc:2,tex:bcba9da6cfeab4bc8a1b11323ea31db9,ntxv:0,isnm:False|UVIN-6921-OUT,TEX-1280-TEX;n:type:ShaderForge.SFN_Blend,id:6759,x:32800,y:32242,varname:node_6759,prsc:2,blmd:14,clmp:True|SRC-1304-RGB,DST-227-RGB;n:type:ShaderForge.SFN_TexCoord,id:6979,x:31191,y:31440,varname:node_6979,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Tex2dAsset,id:790,x:31191,y:31738,ptovrint:False,ptlb:_DispTex,ptin:__DispTex,varname:node_790,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:418d3eca8588b4d12ae021739d45bb8b,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:9058,x:31498,y:31758,varname:node_9058,prsc:2,tex:418d3eca8588b4d12ae021739d45bb8b,ntxv:0,isnm:False|UVIN-7076-OUT,TEX-790-TEX;n:type:ShaderForge.SFN_Sin,id:5356,x:32028,y:31757,varname:node_5356,prsc:2|IN-4299-OUT;n:type:ShaderForge.SFN_Add,id:7076,x:31371,y:31515,varname:node_7076,prsc:2|A-6979-UVOUT,B-1216-OUT;n:type:ShaderForge.SFN_Time,id:9370,x:30969,y:31582,varname:node_9370,prsc:2;n:type:ShaderForge.SFN_Code,id:4299,x:31682,y:31713,varname:node_4299,prsc:2,code:cgBlAHQAdQByAG4AIABBAC4AcgBnADsA,output:1,fname:Function_node_4299,width:247,height:132,input:2,input_1_label:A|A-9058-RGB;n:type:ShaderForge.SFN_Multiply,id:345,x:32224,y:31757,varname:node_345,prsc:2|A-406-OUT,B-5356-OUT;n:type:ShaderForge.SFN_Vector1,id:406,x:32016,y:31669,varname:node_406,prsc:2,v1:2;n:type:ShaderForge.SFN_Subtract,id:4597,x:32448,y:31757,varname:node_4597,prsc:2|A-345-OUT,B-2029-OUT;n:type:ShaderForge.SFN_Vector1,id:2029,x:32224,y:31905,varname:node_2029,prsc:2,v1:1;n:type:ShaderForge.SFN_TexCoord,id:4606,x:32068,y:32082,varname:node_4606,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Add,id:6921,x:32296,y:32082,varname:node_6921,prsc:2|A-6026-OUT,B-4606-UVOUT;n:type:ShaderForge.SFN_Multiply,id:6026,x:32665,y:31610,varname:node_6026,prsc:2|A-9079-OUT,B-4597-OUT;n:type:ShaderForge.SFN_Vector1,id:9079,x:32392,y:31486,varname:node_9079,prsc:2,v1:0.1;n:type:ShaderForge.SFN_Divide,id:1216,x:31191,y:31582,varname:node_1216,prsc:2|A-9370-T,B-9204-OUT;n:type:ShaderForge.SFN_Vector1,id:9204,x:30949,y:31715,varname:node_9204,prsc:2,v1:10;n:type:ShaderForge.SFN_Add,id:8765,x:32957,y:32535,varname:node_8765,prsc:2|A-1515-OUT,B-7235-OUT;n:type:ShaderForge.SFN_Vector1,id:7235,x:32770,y:32712,varname:node_7235,prsc:2,v1:0.1;n:type:ShaderForge.SFN_ValueProperty,id:3413,x:32392,y:32468,ptovrint:False,ptlb:_Rim,ptin:__Rim,varname:node_3413,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.2;proporder:1304-338-1280-790-3413;pass:END;sub:END;*/

Shader "Shader Forge/waypoint" {
    Properties {
        [HDR]_Color ("Color", Color) = (0.9433962,0.3604486,0.3771043,1)
        __FresnelFac ("_FresnelFac", Float ) = 1
        _NoiseTex ("NoiseTex", 2D) = "white" {}
        __DispTex ("_DispTex", 2D) = "white" {}
        __Rim ("_Rim", Float ) = 0.2
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend One OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x xboxone ps4 psp2 n3ds wiiu 
            #pragma target 3.0
            uniform float4 _Color;
            uniform float __FresnelFac;
            uniform sampler2D _NoiseTex; uniform float4 _NoiseTex_ST;
            uniform sampler2D __DispTex; uniform float4 __DispTex_ST;
            float2 Function_node_4299( float3 A ){
            return A.rg;
            }
            
            uniform float __Rim;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                UNITY_FOG_COORDS(3)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
////// Lighting:
////// Emissive:
                float4 node_9370 = _Time;
                float2 node_7076 = (i.uv0+(node_9370.g/10.0));
                float4 node_9058 = tex2D(__DispTex,TRANSFORM_TEX(node_7076, __DispTex));
                float2 node_6921 = ((0.1*((2.0*sin(Function_node_4299( node_9058.rgb )))-1.0))+i.uv0);
                float4 node_227 = tex2D(_NoiseTex,TRANSFORM_TEX(node_6921, _NoiseTex));
                float node_1515 = (__Rim+pow(1.0-max(0,dot(normalDirection, viewDirection)),__FresnelFac));
                float3 emissive = (saturate(( _Color.rgb > 0.5 ? (node_227.rgb + 2.0*_Color.rgb -1.0) : (node_227.rgb + 2.0*(_Color.rgb-0.5))))*node_1515);
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,(node_1515+0.1));
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
