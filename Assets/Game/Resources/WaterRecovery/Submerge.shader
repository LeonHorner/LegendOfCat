Shader "LegendOfCat/WaterRecovery/Submerge"
{
    Properties { _MainTex ("Reaction drawing", 2D) = "white" {} _Surface ("Water surface", Float) = -3.25 }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; float _Surface;
            struct Input { float4 vertex : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct Output { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float2 world : TEXCOORD1; float4 color : COLOR; };
            Output vert(Input v) {
                Output o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv;
                o.world = mul(unity_ObjectToWorld, v.vertex).xy; o.color = v.color; return o;
            }
            fixed4 frag(Output i) : SV_Target {
                float surface = _Surface + sin(i.world.x * 14 + _Time.y * 3) * 0.015;
                clip(i.world.y - surface);
                fixed4 color = tex2D(_MainTex, i.uv) * i.color;
                color.rgb = lerp(float3(0.56, 0.76, 0.75), color.rgb, smoothstep(0, 0.10, i.world.y - surface));
                return color;
            }
            ENDCG
        }
    }
}
