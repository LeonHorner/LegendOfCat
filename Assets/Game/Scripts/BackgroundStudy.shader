Shader "LegendOfCat/BackgroundStudy"
{
    Properties { _MainTex ("Painted layer", 2D) = "white" {} _Cutout ("Use authored alpha", Float) = 0 }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off Cull Off
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; float _Cutout;
            struct Input { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct Output { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; };
            Output vert(Input v) { Output o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }
            fixed4 frag(Output i) : SV_Target {
                // Half-texel inset prevents adjacent atlas strips bleeding at their borders.
                float v = _Cutout > 0.5 ? i.uv.y : lerp(2.0 / 3 + 0.002, 1 - 0.002, i.uv.y);
                fixed4 c = tex2D(_MainTex, float2(i.uv.x, v));
                c.rgb = lerp(c.rgb, float3(0.80, 0.79, 0.73), 0.23);
                // Reject the generator's low-alpha halo, retaining actual painted contours.
                // No screen-height blend: transparent sky reveals the independent far layer.
                c.a = _Cutout > 0.5 ? smoothstep(0.5, 0.95, c.a) : 1;
                return c;
            }
            ENDCG
        }
    }
}
