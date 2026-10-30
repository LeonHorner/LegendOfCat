Shader "LegendOfCat/StudyWater"
{
    Properties {
        _MainTex ("Painted water and stones", 2D) = "white" {}
        _WaterTime ("Water animation time", Float) = 0
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
            float _WaterTime, _Fog, _Saturation;
            struct Input { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct Output { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float2 world : TEXCOORD1; };
            Output vert(Input v) {
                Output o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv;
                o.world = mul(unity_ObjectToWorld, v.vertex).xy; return o;
            }
            fixed4 frag(Output i) : SV_Target {
                fixed4 painted = tex2D(_MainTex, i.uv);
                // Refract the bright cyan water, preserving dark/warm stones and shore plants.
                float light = dot(painted.rgb, float3(0.2126, 0.7152, 0.0722));
                float water = smoothstep(0.035, 0.16, painted.g - painted.r)
                    * smoothstep(0.025, 0.12, painted.b - painted.r) * smoothstep(0.24, 0.46, light);
                float phase = i.world.x * 3.1 + i.world.y * 12.0 - _WaterTime * 1.5;
                float secondary = i.world.x * 5.7 - i.world.y * 8.0 + _WaterTime * 1.1;
                // Fade UV displacement at tile edges; world-space shimmer has one shared phase.
                float edge = smoothstep(0, 0.025, i.uv.x) * smoothstep(0, 0.025, 1 - i.uv.x);
                float2 ripple = float2(sin(phase) * 0.0009, sin(secondary) * 0.0012) * water * edge;
                fixed3 refracted = tex2D(_MainTex, i.uv + ripple).rgb;
                fixed3 color = lerp(painted.rgb, refracted, water);
                color *= 1 + water * (sin(phase) * 0.045 + sin(secondary) * 0.025);
                color = lerp(color, float3(0.80, 0.79, 0.73), _Fog);
                color = lerp(dot(color, float3(0.2126, 0.7152, 0.0722)).xxx, color, _Saturation);
                // Keep the authored wave silhouette fixed at cliff feet; only the water interior flows.
                return fixed4(color, smoothstep(0.5, 0.95, painted.a));
            }
            ENDCG
        }
    }
}
