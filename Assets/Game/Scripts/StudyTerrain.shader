Shader "LegendOfCat/StudyTerrain"
{
    Properties { _MainTex ("Painted turf and stone", 2D) = "white" {} _TopTex ("Meadow surface", 2D) = "white" {} _Width ("Width", Float) = 4 _Depth ("Depth", Float) = 10 _Surface ("Top face", Float) = 0 _Seed ("Texture variation", Float) = 0 _Tint ("Stone variation", Color) = (1,1,1,1) }
    SubShader {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        Cull Off
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex, _TopTex; float _Width, _Depth, _Surface, _Seed; float4 _Tint;
            struct Input { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct Output { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; };
            Output vert(Input v) { Output o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }
            float mirror(float value) { return 1 - abs(frac(value * 0.5) * 2 - 1); }
            fixed4 frag(Output i) : SV_Target {
                if (_Surface > 0.5) {
                    fixed4 grass = tex2D(_TopTex, float2(mirror(i.uv.x * _Width / 4 + _Seed), i.uv.y * 0.6 + 0.2));
                    grass.rgb *= _Tint.rgb * lerp(1.02, 0.84, i.uv.y); grass.a = 1; return grass;
                }
                float belowTop = (1 - i.uv.y) * _Depth;
                float u = mirror(i.uv.x * _Width / 4 + _Seed);
                float v = belowTop < 0.32 ? lerp(0.975, 0.88, belowTop / 0.32)
                    : lerp(0.88, 0.04, mirror((belowTop - 0.32) / 3.5));
                fixed4 color = tex2D(_MainTex, float2(u, v));
                color.rgb *= _Tint.rgb * lerp(1, 0.72, smoothstep(0.82, 1, i.uv.x));
                color.a = 1; return color;
            }
            ENDCG
        }
    }
}
