Shader "LegendOfCat/CaveRock"
{
    Properties {
        _MainTex("Rock painting",2D)="white"{}
        _BaseTex("Continuous underlying rock",2D)="white"{}
        _EdgeMask("Authored rock boundary detail",2D)="white"{}
        _Frame("World frame",Vector)=(0,0,8,5.33333)
        _WorldMapped("Shared world grain",Float)=1
        _Cutout("Native cutout detail",Float)=0
        _AuthoredFace("Continuous authored cliff face",Float)=0
        _Shade("Recess tint",Float)=1
        _Origin("Cave origin and scale",Vector)=(0,0,1,1)
        _DryMask("Permanent river coverage",2D)="white"{}
        _DryRect("Coverage world rectangle",Vector)=(0,0,1,1)
        _UseDryMask("Use river contact",Float)=0
    }
    SubShader {
        Tags {"Queue"="Geometry" "RenderType"="Opaque"}
        ZWrite On Cull Off
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex,_BaseTex,_EdgeMask,_DryMask;float4 _Frame,_Origin,_DryRect;float _WorldMapped,_Cutout,_Shade,_UseDryMask,_AuthoredFace;
            struct Input{float4 vertex:POSITION;float2 uv:TEXCOORD0;};
            struct Output{float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float2 world:TEXCOORD1;};
            Output vert(Input v){Output o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.world=(mul(unity_ObjectToWorld,v.vertex).xy-_Origin.xy)/_Origin.z;return o;}
            fixed4 RockSample(float2 uv){return tex2D(_BaseTex,float2(.025,.04)+frac(uv)*float2(.92,.90));}
            fixed4 frag(Output i):SV_Target{
                if(_WorldMapped<.5&&_Cutout>.5){clip(i.uv);clip(1-i.uv);}
                fixed4 c;
                float2 baseUV=frac((i.world-_Frame.xy)/_Frame.zw);
                float2 weight=sin(baseUV*3.14159265);weight*=weight;
                fixed4 baseRock=lerp(lerp(RockSample(baseUV+.5),RockSample(baseUV+float2(0,.5)),weight.x),
                    lerp(RockSample(baseUV+float2(.5,0)),RockSample(baseUV),weight.x),weight.y);
                if(_WorldMapped>.5){
                    c=baseRock;
                }else {if(_Cutout>.5){clip(i.uv);clip(1-i.uv);}c=tex2D(_MainTex,i.uv);
                    if(_Cutout>.5){
                        float edge=min(min(i.uv.x,1-i.uv.x),min(i.uv.y,1-i.uv.y));
                        float grain=sin(i.world.x*17+i.world.y*11)*sin(i.world.y*23-i.world.x*7)*.018;
                        float detail=_AuthoredFace>.5?1:tex2D(_EdgeMask,i.uv).r;
                        float feather=_AuthoredFace>.5?smoothstep(0,.035,edge):smoothstep(.01,.10,edge+grain);
                        c.rgb=lerp(baseRock.rgb,c.rgb,feather*detail);
                    }
                }
                if(_Cutout>.5)clip(c.a-.95);
                float shade=1-smoothstep(-10,-1,i.world.y);
                c.rgb*=lerp(float3(1,1,1),float3(.77,.81,.93),shade)*_Shade;
                float2 world=i.world*_Origin.z+_Origin.xy;
                float2 maskUV=(world-_DryRect.xy)/_DryRect.zw;
                float wet=(1-tex2D(_DryMask,saturate(maskUV)).r)*_UseDryMask;
                wet*=smoothstep(-5.4,-4.9,world.y)*(1-smoothstep(-3.15,-2.85,world.y));
                c.rgb*=lerp(float3(1,1,1),float3(.68,.74,.82),wet);
                return fixed4(c.rgb,1);
            }
            ENDCG
        }
    }
}
