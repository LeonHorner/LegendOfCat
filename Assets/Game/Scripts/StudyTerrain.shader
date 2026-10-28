Shader "LegendOfCat/StudyTerrain"
{
    Properties { _MainTex ("Painted terrain module", 2D) = "white" {} }
    SubShader {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
        Cull Off
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            struct Input { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct Output { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float height : TEXCOORD1; };
            Output vert(Input v) {
                Output o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv;
                o.height = mul(unity_ObjectToWorld, v.vertex).y; return o;
            }
            fixed4 frag(Output i) : SV_Target {
                fixed4 color = tex2D(_MainTex, i.uv);
                // Reject generated haze outside the authored rock and leaf contours.
                clip(color.a - 0.95);
                // Share the river's warm sage atmosphere without losing landing contrast.
                float luminance = dot(color.rgb, float3(0.2126, 0.7152, 0.0722));
                color.rgb = lerp(luminance.xxx, color.rgb, 0.80);
                color.rgb *= float3(0.95, 0.98, 1.02);
                float foot = 1 - smoothstep(-3.4, -1.3, i.height);
                color.rgb = lerp(color.rgb, float3(0.57, 0.64, 0.57), 0.06 + foot * 0.11);
                // A restrained cool damp zone connects the submerged rocks to the teal river.
                color.rgb *= lerp(float3(1, 1, 1), float3(0.79, 0.91, 0.93),
                    1 - smoothstep(-3.45, -2.85, i.height));
                color.a = 1;
                return color;
            }
            ENDCG
        }
    }
}
