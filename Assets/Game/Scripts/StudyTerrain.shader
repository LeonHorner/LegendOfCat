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
            struct Output { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; };
            Output vert(Input v) { Output o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }
            fixed4 frag(Output i) : SV_Target {
                fixed4 color = tex2D(_MainTex, i.uv);
                // Reject generated haze outside the authored rock and leaf contours.
                clip(color.a - 0.95);
                color.a = 1;
                return color;
            }
            ENDCG
        }
    }
}
