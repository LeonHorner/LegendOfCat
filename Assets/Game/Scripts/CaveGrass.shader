Shader "LegendOfCat/CaveGrass"
{
    Properties{_ChannelStone("Exposed rock lip",2D)="gray"{} _MainTex("Registered vegetation",2D)="white"{} _Origin("Cave frame",Vector)=(0,0,1,1) _Contact("Contact height",Float)=0 _FollowContact("Mesh contact heights",Float)=0}
    SubShader{
        Tags{"Queue"="Transparent" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex,_ChannelStone;float4 _Origin;float _Contact,_FollowContact;
            struct Input{float4 vertex:POSITION;float2 contact:TEXCOORD1;};
            struct Output{float4 vertex:SV_POSITION;float2 world:TEXCOORD0;float contact:TEXCOORD1;};
            Output vert(Input v){Output o;o.vertex=UnityObjectToClipPos(v.vertex);o.world=(mul(unity_ObjectToWorld,v.vertex).xy-_Origin.xy)/_Origin.z;o.contact=lerp(_Contact,v.contact.x,_FollowContact);return o;}
            fixed4 frag(Output i):SV_Target{
                float u=frac(i.world.x/7.5),v=.66+(i.world.y-i.contact)*(.34/.6),w=sin(u*3.14159265);w*=w;
                fixed4 c=lerp(tex2D(_MainTex,float2(frac(u+.5),v)),tex2D(_MainTex,float2(u,v)),w);
                c.a*=smoothstep(i.contact-.12,i.contact-.02,i.world.y);
                float shade=1-smoothstep(-10,-1,i.world.y);c.rgb*=lerp(float3(1,1,1),float3(.66,.69,.79),shade);
                float daylight=smoothstep(-7.6,-1.4,i.contact+.6*sin(i.world.x*1.6));
                float2 stoneUV=1-abs(frac((i.world/float2(24,12))*.5)*2-1);
                float3 slate=tex2D(_ChannelStone,stoneUV).rgb;
                float rimWidth=.045+.045*slate.r;
                float rim=(1-smoothstep(-.012,.004,i.world.y-i.contact))*
                    smoothstep(-rimWidth,-rimWidth+.022,i.world.y-i.contact);
                fixed4 ledge=fixed4(slate*float3(1.18,1.12,1.02),rim*.90);
                return lerp(ledge,c,daylight);
            }
            ENDCG
        }
    }
}
