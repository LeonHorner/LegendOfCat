Shader "LegendOfCat/StudyRiver"
{
    Properties {
        _MainTex ("Connected river and banks", 2D) = "white" {}
        _Cutout ("Use authored alpha", Float) = 1
        _Fog ("Atmospheric tint", Float) = 0.23
        _Saturation ("Paint saturation", Float) = 1
        _Waterline ("Platform base waterline", Float) = -3.4
        [HideInInspector] _Corner ("Corner mode", Float) = 0
        [HideInInspector] _CornerSide ("Corner half", Float) = 0
        [HideInInspector] _AtlasCount ("Atlas columns", Float) = 1
        [HideInInspector] _AtlasIndex ("Atlas column", Float) = 0
    }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off Cull Off
        // Render the complete painting behind terrain. Its lower water can also
        // cover cliff bases using the exact same mesh, UVs, texture and motion.
        UsePass "LegendOfCat/BackgroundStudy/PAINTED"
        Pass {
            Name "CONNECTED_WATER"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off ZTest Always Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; float _Fog, _Saturation, _Waterline;
            struct Input { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct Output { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float2 world : TEXCOORD1; };
            Output vert(Input v) {
                Output o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv;
                o.world = mul(unity_ObjectToWorld, v.vertex).xy; return o;
            }
            fixed4 frag(Output i) : SV_Target {
                fixed4 c = tex2D(_MainTex, i.uv);
                c.rgb = lerp(c.rgb, float3(0.80, 0.79, 0.73), _Fog);
                c.rgb = lerp(dot(c.rgb, float3(0.2126, 0.7152, 0.0722)).xxx, c.rgb, _Saturation);
                float shore = _Waterline + sin(i.world.x * 2.8) * 0.04;
                c.a = smoothstep(0.5, 0.95, c.a) * (1 - smoothstep(shore - 0.07, shore + 0.07, i.world.y));
                return c;
            }
            ENDCG
        }
    }
}
