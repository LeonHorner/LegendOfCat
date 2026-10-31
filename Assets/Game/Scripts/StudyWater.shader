Shader "LegendOfCat/StudyWater"
{
    Properties {
        _MainTex ("Painted water and stones", 2D) = "white" {}
        _WaterTime ("Capture time", Float) = 0
        [HideInInspector] _UseCaptureTime ("Use explicit capture clock", Float) = 0
        [HideInInspector] _RunAmount ("Running wave intensity", Float) = 0
        [HideInInspector] _RunTimeOffset ("Continuous extra running time", Float) = 0
        _Fog ("Atmospheric tint", Float) = 0.20
        _Saturation ("Paint saturation", Float) = 0.70
    }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off Cull Off
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float _WaterTime, _UseCaptureTime, _RunAmount, _RunTimeOffset, _Fog, _Saturation;
            struct Input { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct Output { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float2 world : TEXCOORD1; };
            Output vert(Input v) {
                Output o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv;
                o.world = mul(unity_ObjectToWorld, v.vertex).xy; return o;
            }
            float WaterMask(float3 color) {
                float light = dot(color, float3(0.2126, 0.7152, 0.0722));
                float cyan = smoothstep(0.035, 0.16, color.g - color.r)
                    * smoothstep(0.025, 0.12, color.b - color.r) * smoothstep(0.24, 0.46, light);
                float foam = smoothstep(0.68, 0.9, light) * smoothstep(-0.025, 0.015, color.g - color.r)
                    * smoothstep(-0.055, 0.02, color.b - color.r);
                return max(cyan, foam);
            }
            fixed4 frag(Output i) : SV_Target {
                fixed4 painted = tex2D(_MainTex, i.uv);
                // Normal Play advances in the shader; capture/tests may supply an explicit time.
                float time = (_UseCaptureTime > 0.5 ? _WaterTime : _Time.y) + _RunTimeOffset;
                float water = WaterMask(painted.rgb);
                float phase = i.world.x * 2.0 + i.world.y * 10.0 - time * 2.2;
                float secondary = i.world.x * 3.4 - i.world.y * 7.0 + time * 1.3;
                // Fade UV displacement at tile edges; world-space shimmer has one shared phase.
                float edge = smoothstep(0, 0.025, i.uv.x) * smoothstep(0, 0.025, 1 - i.uv.x);
                float2 ripple = float2(sin(phase) * 0.006 + sin(secondary) * 0.002,
                    sin(secondary) * 0.0035) * water * edge * (1 + _RunAmount * 0.35);
                fixed3 refracted = tex2D(_MainTex, i.uv + ripple).rgb;
                // Do not pull the nearby rock colors into the flowing water.
                fixed3 color = lerp(painted.rgb, refracted, water * WaterMask(refracted));
                color *= 1 + water * (sin(phase) * 0.06 + sin(secondary) * 0.035);
                float crest = pow(saturate(0.5 + 0.5 * sin(i.world.y * 18 + i.world.x * 0.5 - time * 2.1)), 12);
                float broken = smoothstep(-0.3, 0.8, sin(i.world.x * 3.2 + i.world.y * 2.1 - time * 0.8));
                color = lerp(color, float3(0.88, 0.94, 0.85), water * crest * broken * 0.28);
                float runCrest = pow(saturate(0.5 + 0.5 * sin(i.world.y * 29 - i.world.x * 0.8 - time * 2.8)), 10);
                float runBreak = smoothstep(-0.4, 0.65, sin(i.world.x * 4.7 + i.world.y * 3.0 - time * 1.2));
                color = lerp(color, float3(0.88, 0.94, 0.85), water * runCrest * runBreak * _RunAmount * 0.24);
                color = lerp(color, float3(0.80, 0.79, 0.73), _Fog);
                color = lerp(dot(color, float3(0.2126, 0.7152, 0.0722)).xxx, color, _Saturation);
                // Keep the authored wave silhouette fixed at cliff feet; only the water interior flows.
                return fixed4(color, smoothstep(0.5, 0.95, painted.a));
            }
            ENDCG
        }
    }
}
