Shader "LegendOfCat/CaveRock"
{
    Properties {
        _NativeShelf("Registered river shelf",Float)=0
        _BankFace("Conforming bank paint",Float)=0
        _BankEnds("Riverbank limits",Vector)=(0,0,0,0)
        _Bedrock("Sheltered river stone",2D)="gray"{}
        _RockMargin("Bed or vault edge",Float)=0
        _Falls("Fern grotto",2D)="gray"{}
        _Cliff("Native ascent cliff",2D)="gray"{}
        _StreamClock("Flow time",Float)=0
        _MainTex("Rock painting",2D)="white"{}
        _BaseTex("Continuous underlying rock",2D)="white"{}
        _ChannelStone("River-cut bedding",2D)="gray"{}
        _RootWall("Rooted channel alcove",2D)="gray"{}
        _Fissure("Painted chamber and light fissure",2D)="gray"{}
        _ChamberWall("Rear chamber",Float)=0
        _Alcove("Recessed mouth",Float)=0
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
        Blend SrcAlpha OneMinusSrcAlpha
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex,_BaseTex,_EdgeMask,_DryMask;float4 _Frame,_Origin,_DryRect;float _WorldMapped,_Cutout,_Shade,_UseDryMask,_AuthoredFace;
            struct Input{float4 vertex:POSITION;float2 uv:TEXCOORD0;float2 limits:TEXCOORD1;};
            struct Output{float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float2 world:TEXCOORD1;float limit:TEXCOORD2;};
            Output vert(Input v){Output o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.limit=v.limits.x;o.world=(mul(unity_ObjectToWorld,v.vertex).xy-_Origin.xy)/_Origin.z;return o;}
            fixed4 RockSample(float2 uv){return tex2D(_BaseTex,float2(.025,.04)+frac(uv)*float2(.92,.90));}
            sampler2D _ChannelStone, _Fissure, _RootWall;
            sampler2D _Falls, _Cliff, _Bedrock;
            float4 _BankEnds; float _RockMargin;
            float _ChamberWall, _Alcove, _StreamClock, _NativeShelf, _BankFace;
            float3 channelStone(float2 p) {
                // Reflect a continuous sheet at its borders and bend its beds across formations.
                float2 uv = p / float2(24,12);
                uv.y += .19*sin(p.x*.13) + .11*cos(p.x*.073);
                uv.x += .09*sin(p.y*.33 + p.x*.04);
                return tex2D(_ChannelStone, 1-abs(frac(uv*.5)*2-1)).rgb;
            }
            float3 cliffPatch(float2 p) {
                return tex2D(_Cliff, .20+.61*frac(p)).rgb;
            }
            fixed4 frag(Output i):SV_Target{
                if(_WorldMapped<.5&&_Cutout>.5){clip(i.uv);clip(1-i.uv);}
                if(_NativeShelf>.5) {
                    fixed4 shelf=tex2D(_MainTex,i.uv);
                    clip(shelf.a-.55);
                    float daylight=smoothstep(-9,1,i.world.y);
                    return fixed4(shelf.rgb*lerp(float3(.67,.77,.81),1,daylight),1);
                }
                fixed4 c;
                float2 baseUV=frac((i.world-_Frame.xy)/_Frame.zw);
                float2 weight=sin(baseUV*3.14159265);weight*=weight;
                fixed4 baseRock=lerp(lerp(RockSample(baseUV+.5),RockSample(baseUV+float2(0,.5)),weight.x),
                    lerp(RockSample(baseUV+float2(.5,0)),RockSample(baseUV),weight.x),weight.y);
                float interior = 1-smoothstep(-8,1,i.world.y);
                baseRock.rgb=lerp(baseRock.rgb,channelStone(i.world),.90*interior);
                // Keep the brush scale of the valley's tall cliffs on exposed faces at both mouths.
                float mouth=max(1-smoothstep(12,35,i.world.x),smoothstep(185,215,i.world.x));
                if (mouth>0) {
                    float2 p=i.world/float2(8.2,11.5)+float2(.12*sin(i.world.y*.28),.09*sin(i.world.x*.24));
                    float2 blend=smoothstep(.02,.17,min(frac(p),1-frac(p)));
                    float3 cliff=lerp(lerp(cliffPatch(p+.5),cliffPatch(p+float2(0,.5)),blend.x),
                        lerp(cliffPatch(p+float2(.5,0)),cliffPatch(p),blend.x),blend.y);
                    baseRock.rgb=lerp(baseRock.rgb,cliff,mouth*.84);
                }
                if(_RockMargin>.5) {
                    bool roof=_RockMargin>1.5;
                    float inward=i.uv.y;
                    float v=roof ? .535+inward/4.2 : .72-inward/4.2;
                    float phase=frac(i.uv.x/14.1+.06*sin(i.uv.x*.19));
                    float mixTile=smoothstep(.015,.14,min(phase,1-phase));
                    fixed4 edge=lerp(tex2D(_MainTex,float2(frac(phase+.5),saturate(v))),
                                     tex2D(_MainTex,float2(phase,saturate(v))),mixTile);
                    clip(edge.a-.42);
                    float fade=1-smoothstep(i.limit*.26,i.limit*.96,inward);
                    c=fixed4(edge.rgb,edge.a*fade);
                    clip(c.a-.025);
                }
                else if(_BankFace>.5) {
                    float phase=frac(i.uv.x/12.8+.43);
                    float other=frac(phase+.5);
                    float v=saturate(.587+i.uv.y/4.1);
                    float blend=smoothstep(.04,.22,min(phase,1-phase));
                    fixed4 bank=lerp(tex2D(_MainTex,float2(.11+.78*other,v)),
                                     tex2D(_MainTex,float2(.11+.78*phase,v)),blend);
                    // Give the exposed ends their own painted nose and trailing vegetation.
                    float left=i.world.x-_BankEnds.x, right=_BankEnds.y-i.world.x;
                    float endDist=min(left,right);
                    float endU=left<right ? endDist/12.8 : 1-endDist/12.8;
                    bank=lerp(tex2D(_MainTex,float2(saturate(endU),v)),bank,smoothstep(1.45,3.2,endDist));
                    float stoneV=saturate(.72+i.uv.y/4.2);
                    fixed4 bed=lerp(tex2D(_Bedrock,float2(other,stoneV)),tex2D(_Bedrock,float2(phase,stoneV)),blend);
                    float sheltered=(1-smoothstep(-10,-1.8,i.world.y-i.uv.y))*smoothstep(.15,1.45,endDist);
                    bank=lerp(bank,bed,sheltered);
                    clip(bank.a-.40);
                    float depth=i.uv.y+.17*sin(i.world.x*1.2);
                    c=fixed4(bank.rgb,bank.a*smoothstep(-2.36,-.95,depth));
                    clip(c.a-.025);
                }
                else if(_WorldMapped>.5){
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
                if(_Alcove>.5)
                    c.rgb=lerp(c.rgb,baseRock.rgb,smoothstep(18,34,i.world.x));
                if (_ChamberWall>.5) {
                    // A quiet, wide-angle recessed wall runs between the two unique chambers.
                    float2 fold = float2(i.world.x/46+.04*sin(i.world.y*.19),i.world.y/23+.09*cos(i.world.x*.10));
                    float2 rockUV=float2(.04,.09)+float2(.50,.83)*(1-abs(frac(fold*.5)*2-1));
                    c.rgb=tex2D(_RootWall,rockUV).rgb*1.15;
                    float2 rootUV=(i.world-float2(106,-26))/float2(46,23);
                    float roots=smoothstep(0,.16,rootUV.x)*(1-smoothstep(.86,1,rootUV.x));
                    c.rgb=lerp(c.rgb,tex2D(_RootWall,saturate(rootUV)).rgb*1.43,roots);
                    float2 uv=(i.world-float2(33,-21))/float2(31,15.5);
                    float seam=sin(i.world.y*.73)*.028;
                    float blend=smoothstep(0,.17,uv.x+seam)*(1-smoothstep(.82,1,uv.x+seam));
                    float3 painting=tex2D(_Fissure,saturate(uv)).rgb;
                    c.rgb=lerp(c.rgb,painting*1.56,blend);
                    float2 fallsUV=(i.world-float2(80,-25.3))/float2(32,16);
                    float stream=(1-smoothstep(.011,.034,abs(fallsUV.x-.508)))*smoothstep(.13,.18,fallsUV.y)*(1-smoothstep(.89,.94,fallsUV.y));
                    float ripples=sin(fallsUV.y*170+_StreamClock*10);
                    fallsUV.y+=stream*ripples*.006;
                    float3 falls=tex2D(_Falls,saturate(fallsUV)).rgb*(1.52+stream*ripples*.16);
                    float grotto=smoothstep(0,.17,fallsUV.x+seam)*(1-smoothstep(.82,1,fallsUV.x+seam));
                    grotto*=smoothstep(-.045,.08,fallsUV.y)*(1-smoothstep(.93,1.05,fallsUV.y));
                    c.rgb=lerp(c.rgb,falls,grotto);
                    c.rgb=lerp(baseRock.rgb,c.rgb,smoothstep(33,42,i.world.x));
                }
                if(_Cutout>.5)clip(c.a-.95);
                float shade=1-smoothstep(-10,-1,i.world.y);
                c.rgb*=lerp(float3(1,1,1),float3(.77,.81,.93),shade)*_Shade;
                float2 world=i.world*_Origin.z+_Origin.xy;
                float2 maskUV=(world-_DryRect.xy)/_DryRect.zw;
                float wet=(1-tex2D(_DryMask,saturate(maskUV)).r)*_UseDryMask;
                wet*=smoothstep(-5.4,-4.9,world.y)*(1-smoothstep(-3.15,-2.85,world.y));
                c.rgb*=lerp(float3(1,1,1),float3(.68,.74,.82),wet);
                return fixed4(c.rgb,c.a);
            }
            ENDCG
        }
    }
}
