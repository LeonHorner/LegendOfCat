Shader "LegendOfCat/EntryStudyPaint"
{
    Properties {
        _MainTex ("Native painting", 2D) = "white" {}
        _Recessed ("Recessed wall", Float) = 0
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
            sampler2D _MainTex; float _Recessed;
            struct Input { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct Output { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float2 world : TEXCOORD1; };
            Output vert(Input v) {
                Output o; o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;
                o.world=mul(unity_ObjectToWorld,v.vertex).xy;return o;
            }
            fixed4 frag(Output i) : SV_Target {
                fixed4 c=tex2D(_MainTex,i.uv);
                if(_Recessed>.5) {
                    // Permanent world-space recess, never switched by player position.
                    c.rgb*=float3(.64,.68,.80);
                    c.a*=1-smoothstep(-8,-2.8,i.world.y);
                } else {
                    clip(c.a-.95);c.a=1;
                    float shade=1-smoothstep(-10,-1,i.world.y);
                    c.rgb*=lerp(float3(1,1,1),float3(.77,.81,.93),shade);
                }
                return c;
            }
            ENDCG
        }
    }
}
