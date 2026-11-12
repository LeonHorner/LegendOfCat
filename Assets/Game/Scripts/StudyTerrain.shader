Shader "LegendOfCat/StudyTerrain"
{
    Properties {
        _MainTex ("Painted terrain module", 2D) = "white" {}
        [HideInInspector] _CourseOriginY ("Course origin", Float) = 0
        [HideInInspector] _CourseScale ("Course scale", Float) = 1
        _DryMask ("Dry passage coverage", 2D) = "black" {}
        _DryRect ("Coverage world rectangle", Vector) = (0,0,1,1)
        _UseDryMask ("Use passage coverage", Float) = 0
        _Backdrop ("Quiet interior rock", Float) = 0
        _WorldMapped ("World stone mapping", Float) = 0
        _CaveBoundary ("Permanent cave distances", 2D) = "black" {}
        _UseCaveBoundary ("Use cave edge shading", Float) = 0
        _CaveLayer ("Solid cave layer", Float) = 0
    }
    SubShader {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
        Cull Off
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            sampler2D _DryMask; float4 _DryRect; float _UseDryMask;
            float _CourseOriginY, _CourseScale;
            float _Backdrop;
            float _WorldMapped;
            sampler2D _CaveBoundary;float4 _CaveBoundary_TexelSize;
            float _UseCaveBoundary,_CaveLayer;
            struct Input { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct Output { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float height : TEXCOORD1; float2 world : TEXCOORD2; };
            Output vert(Input v) {
                Output o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv; o.world=mul(unity_ObjectToWorld,v.vertex).xy;
                o.height = (mul(unity_ObjectToWorld, v.vertex).y - _CourseOriginY) / _CourseScale; return o;
            }
            fixed4 frag(Output i) : SV_Target {
                fixed4 color;
                if (_WorldMapped > .5) {
                    float2 uv=frac(i.world/max(_CourseScale,.001)/8);
                    float2 weight=sin(uv*3.14159265);weight*=weight;
                    fixed4 a=tex2D(_MainTex,uv),b=tex2D(_MainTex,frac(uv+float2(.5,0)));
                    fixed4 c=tex2D(_MainTex,frac(uv+float2(0,.5))),d=tex2D(_MainTex,frac(uv+.5));
                    color=lerp(lerp(d,c,weight.x),lerp(b,a,weight.x),weight.y);
                } else color=tex2D(_MainTex,i.uv);
                // Reject generated haze outside the authored rock and leaf contours.
                clip(color.a - 0.95);
                // Share the river's warm sage atmosphere without losing landing contrast.
                float luminance = dot(color.rgb, float3(0.2126, 0.7152, 0.0722));
                color.rgb = lerp(luminance.xxx, color.rgb, 0.80);
                color.rgb *= float3(0.95, 0.98, 1.02);
                float foot = 1 - smoothstep(-3.4, -1.3, i.height);
                float2 dryUV=(i.world-_DryRect.xy)/_DryRect.zw;
                float dry=tex2D(_DryMask,saturate(dryUV)).g*step(0,dryUV.x)*step(dryUV.x,1)*step(0,dryUV.y)*step(dryUV.y,1)*_UseDryMask;
                foot *= 1-dry;
                color.rgb = lerp(color.rgb, float3(0.57, 0.64, 0.57), 0.06 + foot * 0.11);
                // A restrained cool damp zone connects the submerged rocks to the teal river.
                color.rgb *= lerp(float3(1, 1, 1), float3(0.79, 0.91, 0.93),
                    (1 - smoothstep(-3.45, -2.85, i.height))*(1-dry));
                color.a = 1;
                color.rgb *= lerp(1, .64, _Backdrop);
                if(_UseCaveBoundary>.5) {
                    float2 uv=saturate(dryUV);
                    float4 boundary=tex2D(_CaveBoundary,uv);
                    if(_CaveLayer>.5) {
                        float dx=tex2D(_CaveBoundary,uv+float2(_CaveBoundary_TexelSize.x,0)).g
                            -tex2D(_CaveBoundary,uv-float2(_CaveBoundary_TexelSize.x,0)).g;
                        float dy=tex2D(_CaveBoundary,uv+float2(0,_CaveBoundary_TexelSize.y)).g
                            -tex2D(_CaveBoundary,uv-float2(0,_CaveBoundary_TexelSize.y)).g;
                        float2 normal=-float2(dx,dy)/max(length(float2(dx,dy)),.001);
                        float lighting=saturate(.5+dot(normal,normalize(float2(.55,.85)))*.5);
                        float edge=1-smoothstep(.025,.24,boundary.g*1.5);
                        color.rgb*=lerp(1,lerp(.62,1.15,lighting),edge);
                    } else color.rgb*=lerp(.40,1,smoothstep(.025,1.15,boundary.b*1.5));
                }
                return color;
            }
            ENDCG
        }
    }
}
