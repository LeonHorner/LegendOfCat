Shader "LegendOfCat/EntryStudyPaint"
{
    Properties {
        _MainTex ("Native painting", 2D) = "white" {}
        _Recessed ("Recessed wall", Float) = 0
        _RoofJoin ("Roof overlap", Float) = 0
        _CaveOrigin ("Cave frame origin", Vector) = (0,0,0,0)
        _CaveScale ("Cave frame scale", Float) = 1
        _ExteriorRoof ("Exterior roof coverage", Float) = 0
        _EntryBank ("Entry bank", Float) = 0
        _Interior ("Interior lining", Float) = 0
        _ReturnDoor ("Return opening", Float) = 0
        _Band ("Native return band", Float) = 0
        _BandProfile ("Band contact profile", Vector) = (0,0,1,0)
        _BandRect ("Native band frame", Vector) = (0,0,36,26)
        _SecondTex ("Second native stone", 2D) = "white" {}
        _BandFloorBlend ("Stone variation", Float) = 0
    }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On Cull Off
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; float _Recessed,_RoofJoin;
            float4 _CaveOrigin;float _CaveScale;
            float _ExteriorRoof,_EntryBank,_Interior,_ReturnDoor;
            sampler2D _SecondTex;float _Band,_BandFloorBlend;float4 _BandProfile,_BandRect;
            struct Input { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct Output { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float2 world : TEXCOORD1; };
            Output vert(Input v) {
                Output o; o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;
                o.world=(mul(unity_ObjectToWorld,v.vertex).xy-_CaveOrigin.xy)/_CaveScale;return o;
            }
            fixed4 frag(Output i) : SV_Target {
                fixed4 c=tex2D(_MainTex,i.uv);
                if(_Band>.5) {
                    float contact=lerp(_BandProfile.y,_BandProfile.w,saturate((i.world.x-_BandProfile.x)/max(.001,_BandProfile.z-_BandProfile.x)));
                    float u=frac(i.world.x/_BandRect.z),v=(i.world.y-contact-_BandRect.y)/_BandRect.w;
                    float weight=sin(u*3.14159265);weight*=weight;
                    c=lerp(tex2D(_MainTex,float2(frac(u+.5),v)),tex2D(_MainTex,float2(u,v)),weight);
                    if(_BandFloorBlend>.5) {
                        float b=v-.073/26;
                        fixed4 second=lerp(tex2D(_SecondTex,float2(frac(u+.5),b)),tex2D(_SecondTex,float2(u,b)),weight);
                        c=lerp(c,second,.5+.5*sin(i.world.x*.043));
                        // Adjacent steps share the deep stone grain rather than restarting it at each tread.
                        float cycle=frac(i.world.y/12),other=frac(cycle+.5);
                        float verticalWeight=sin(cycle*3.14159265);verticalWeight*=verticalWeight;
                        fixed4 firstBody=lerp(tex2D(_MainTex,float2(frac(u+.5),.12+.48*cycle)),tex2D(_MainTex,float2(u,.12+.48*cycle)),weight);
                        fixed4 secondBody=lerp(tex2D(_MainTex,float2(frac(u+.5),.12+.48*other)),tex2D(_MainTex,float2(u,.12+.48*other)),weight);
                        fixed4 body=lerp(secondBody,firstBody,verticalWeight);
                        c=lerp(c,body,smoothstep(.5,1.7,contact-i.world.y));
                    }
                }
                if(_ReturnDoor>.5&&i.world.x>66&&i.world.y< -4.2&&i.world.y> -13)clip(-1);
                if(_ExteriorRoof>.5&&_Interior<.5&&i.world.y> -4.145) {
                    if(_EntryBank<.5)clip(-1);
                    float edge=11+.22*sin(i.world.y*2.3)+.13*sin(i.world.y*5.1);
                    clip(edge-i.world.x);
                    c.rgb*=lerp(.76,1,smoothstep(0,.55,edge-i.world.x));
                }
                if(_Recessed>2.5) {
                    // All rear-wall meshes sample the same native world-space grain at their joins.
                    float2 uv=frac(i.world/float2(40,44));
                    float2 weight=sin(uv*3.14159265);weight*=weight;
                    fixed4 a=tex2D(_MainTex,uv),b=tex2D(_MainTex,frac(uv+float2(.5,0)));
                    fixed4 d=tex2D(_MainTex,frac(uv+.5)),e=tex2D(_MainTex,frac(uv+float2(0,.5)));
                    c=lerp(lerp(d,e,weight.x),lerp(b,a,weight.x),weight.y);
                    c.rgb*=float3(.64,.68,.80);c.a*=smoothstep(65,72,i.world.x);
                    if(_Interior<.5)c.a*=1-smoothstep(-8,-2.8,i.world.y);
                    else c.a*=1-smoothstep(204,216,i.world.x);
                } else if(_Recessed>1.5) {
                    c.rgb*=float3(.85,.88,.96);
                    c.a*=smoothstep(31,37,i.world.x);
                } else if(_Recessed>.5) {
                    // Permanent world-space recess, never switched by player position.
                    c.rgb*=float3(.64,.68,.80);
                    if(_Interior<.5)c.a*=1-smoothstep(-8,-2.8,i.world.y);
                    else c.a*=1-smoothstep(204,216,i.world.x);
                } else {
                    clip(c.a-.95);c.a=_RoofJoin>.5?smoothstep(23,26,i.world.x):1;
                    if(_EntryBank>.5&&_Interior>.5)c.a*=1-smoothstep(31,36,i.world.x);
                    float shade=1-smoothstep(-10,-1,i.world.y);
                    c.rgb*=lerp(float3(1,1,1),float3(.77,.81,.93),shade);
                }
                return c;
            }
            ENDCG
        }
    }
}
