Shader "LegendOfCat/WaterRecovery/Submerge"
{
    Properties {
        _MainTex ("Reaction drawing", 2D) = "white" {}
        _Surface ("Water surface", Float) = -3.25
        _SurfaceScale ("Water contact scale", Float) = 1
    }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; float _Surface, _SurfaceScale;
            struct Input { float4 vertex : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct Output { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float2 world : TEXCOORD1; float4 color : COLOR; };
            Output vert(Input v) {
                Output o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv;
                o.world = mul(unity_ObjectToWorld, v.vertex).xy; o.color = v.color; return o;
            }
            fixed4 frag(Output i) : SV_Target {
                float surface = _Surface + sin(i.world.x * 14 + _Time.y * 3) * 0.015 * _SurfaceScale;
                float height = (i.world.y - surface) / max(_SurfaceScale, 0.001);
                clip(height + 0.045);
                fixed4 color = tex2D(_MainTex, i.uv) * i.color;
                // A shallow translucent contact band suggests the neck continuing
                // beneath the river instead of ending at a sharp horizontal crop.
                color.rgb = lerp(float3(0.45, 0.73, 0.73), color.rgb, smoothstep(-0.02, 0.14, height));
                color.a *= smoothstep(-0.045, 0.11, height);
                return color;
            }
            ENDCG
        }
    }
}
