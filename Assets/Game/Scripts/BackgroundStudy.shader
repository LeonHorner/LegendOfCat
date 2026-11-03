Shader "LegendOfCat/BackgroundStudy"
{
    Properties { _MainTex ("Painted layer", 2D) = "white" {} _Submerged ("Underwater fish", Float) = 0 _AlphaFloor ("Cutout alpha floor", Float) = 0.5 _Cutout ("Use authored alpha", Float) = 0 _Brightness ("Paint brightness", Float) = 1 _Fog ("Atmospheric tint", Float) = 0.23 _Saturation ("Paint saturation", Float) = 1 _Corner ("Foreground tint", Float) = 0 _CornerSide ("Corner half", Float) = 0 _AtlasCount ("Atlas columns", Float) = 1 _AtlasIndex ("Atlas column", Float) = 0 }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off Cull Off
        Pass {
            Name "PAINTED"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; float _AlphaFloor, _Submerged, _Cutout, _Brightness, _Fog, _Saturation, _Corner, _CornerSide, _AtlasCount, _AtlasIndex;
            struct Input { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct Output { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float2 world : TEXCOORD1; };
            Output vert(Input v) { Output o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv; o.world = mul(unity_ObjectToWorld, v.vertex).xy; return o; }
            fixed4 frag(Output i) : SV_Target {
                // Half-texel inset prevents adjacent atlas strips bleeding at their borders.
                float v = _Cutout > 0.5 ? i.uv.y : lerp(2.0 / 3 + 0.002, 1 - 0.002, i.uv.y);
                float u = _Corner > 0.5 ? i.uv.x * 0.5 + _CornerSide * 0.5 : i.uv.x;
                if (_AtlasCount > 1.5) u = (_AtlasIndex + lerp(0.006, 0.994, i.uv.x)) / _AtlasCount;
                fixed4 c = tex2D(_MainTex, float2(u, v));
                c.rgb = lerp(c.rgb, float3(0.80, 0.79, 0.73), _Fog);
                c.rgb = lerp(dot(c.rgb, float3(0.2126, 0.7152, 0.0722)).xxx, c.rgb, _Saturation);
                // Reject the generator's low-alpha halo, retaining actual painted contours.
                // No screen-height blend: transparent sky reveals the independent far layer.
                c.a = _Cutout > 0.5 ? smoothstep(_AlphaFloor, 0.95, c.a) : 1;
                c.rgb *= _Brightness;
                if (_Submerged > 0.5) {
                    // Let bright ribbons of river show through the fish rather than painting it blue.
                    float ripple = sin(i.world.x * 5 + i.world.y * 29 - _Time.y * 2.1);
                    float ribbon = smoothstep(0.65, 0.98, ripple);
                    c.a *= 0.88 - ribbon * 0.54;
                }
                if (_Corner > 0.5) {
                    c.rgb *= 0.72;
                }
                return c;
            }
            ENDCG
        }
    }
}
