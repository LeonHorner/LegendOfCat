Shader "LegendOfCat/BackgroundStudy"
{
    Properties { _MainTex ("Painted atlas", 2D) = "white" {} _Strip ("Strip", Float) = 0 _Fade ("Atmospheric fade", Float) = 0 }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off Cull Off
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; float _Strip, _Fade;
            struct Input { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct Output { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; };
            Output vert(Input v) { Output o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }
            fixed4 frag(Output i) : SV_Target {
                // Half-texel inset prevents adjacent atlas strips bleeding at their borders.
                float v = lerp((2 - _Strip) / 3 + 0.002, (3 - _Strip) / 3 - 0.002, i.uv.y);
                fixed4 c = tex2D(_MainTex, float2(i.uv.x, v));
                c.rgb = lerp(c.rgb, float3(0.80, 0.79, 0.73), 0.23);
                c.a = lerp(1, 1 - smoothstep(0.35, 0.73, i.uv.y), _Fade);
                return c;
            }
            ENDCG
        }
    }
}
