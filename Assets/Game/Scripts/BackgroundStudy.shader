Shader "LegendOfCat/BackgroundStudy"
{
    Properties { _MainTex ("Painted layer", 2D) = "white" {} _Submerged ("Underwater fish", Float) = 0 _AlphaFloor ("Cutout alpha floor", Float) = 0.5 _Cutout ("Use authored alpha", Float) = 0 _Brightness ("Paint brightness", Float) = 1 _Fog ("Atmospheric tint", Float) = 0.23 _FogTint ("Atmospheric color", Color) = (0.80,0.79,0.73,1) _MillMotion ("Mill wheel motion", Float) = 0 _WillowMotion ("Willow branch motion", Float) = 0 _Saturation ("Paint saturation", Float) = 1 _Corner ("Foreground tint", Float) = 0 _CornerSide ("Corner half", Float) = 0 _AtlasCount ("Atlas columns", Float) = 1 _AtlasIndex ("Atlas column", Float) = 0 }
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
            float4 _FogTint;
            float _MillMotion, _WillowMotion;
            struct Input { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct Output { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float2 world : TEXCOORD1; };
            Output vert(Input v) { Output o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv; o.world = mul(unity_ObjectToWorld, v.vertex).xy; return o; }
            fixed4 frag(Output i) : SV_Target {
                // Half-texel inset prevents adjacent atlas strips bleeding at their borders.
                float v = _Cutout > 0.5 ? i.uv.y : lerp(2.0 / 3 + 0.002, 1 - 0.002, i.uv.y);
                float u = _Corner > 0.5 ? i.uv.x * 0.5 + _CornerSide * 0.5 : i.uv.x;
                if (_AtlasCount > 1.5) u = (_AtlasIndex + lerp(0.006, 0.994, i.uv.x)) / _AtlasCount;
                float2 uv = float2(u, v);
                if (_WillowMotion > 0.5) {
                    // Sway the outer curtains of leaves and leave the trunk and hillside still.
                    float canopy = smoothstep(0.48, 0.55, v) * (1 - smoothstep(0.76, 0.81, v));
                    float tips = smoothstep(0.045, 0.115, abs(u - 0.50));
                    uv.x += sin(_Time.y * 1.3 + u * 24 + v * 12) * 0.004 * canopy * tips;
                }
                fixed4 c = tex2D(_MainTex, uv);
                if (_MillMotion > 0.5) {
                    // Animate the front spokes inside the painted wheel; the hub, rim and pier stay fixed.
                    float2 center = float2(0.433, 0.571);
                    float2 radius = float2(0.041, 0.083);
                    float2 q = (uv - center) / radius;
                    float distance = length(q);
                    float spokes = smoothstep(0.20, 0.33, distance) * (1 - smoothstep(0.78, 0.94, distance));
                    float pier = (1 - smoothstep(-0.13, -0.04, q.y))
                        * smoothstep(-0.72, -0.58, q.x) * (1 - smoothstep(0.00, 0.12, q.x));
                    float sine, cosine;
                    sincos(_Time.y * 0.38, sine, cosine);
                    float2 rotated = float2(cosine * q.x - sine * q.y, sine * q.x + cosine * q.y);
                    // Use the opposing spokes where the source angle is hidden by the support.
                    float covered = (1 - smoothstep(-0.13, -0.04, rotated.y))
                        * smoothstep(-0.72, -0.58, rotated.x) * (1 - smoothstep(0.00, 0.12, rotated.x));
                    fixed3 exposed = tex2D(_MainTex, center + rotated * radius).rgb;
                    fixed3 opposite = tex2D(_MainTex, center - rotated * radius).rgb;
                    c.rgb = lerp(c.rgb, lerp(exposed, opposite, covered), spokes * (1 - pier));
                }
                c.rgb = lerp(c.rgb, _FogTint.rgb, _Fog);
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
