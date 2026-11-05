Shader "LegendOfCat/Checkpoint"
{
    Properties {
        [PerRendererData] _MainTex ("Atlas", 2D) = "white" {}
        _State ("State", Float) = 0
        _Pulse ("Pulse", Float) = 0
    }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float _State, _Pulse;
            struct Input { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct Output { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; };
            Output vert(Input v) { Output o; o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; return o; }
            fixed4 frag(Output i) : SV_Target {
                float2 cell = i.uv;
                fixed4 color = tex2D(_MainTex, float2((cell.x + _State) / 3, cell.y));
                // Only the warm engraving breathes; the stone outline and surrounding moss stay still.
                float paw = smoothstep(.25,.32,cell.x) * (1-smoothstep(.67,.73,cell.x))
                    * smoothstep(.24,.30,cell.y) * (1-smoothstep(.66,.73,cell.y));
                float warmth = saturate((color.r-color.b)*4);
                if (_State > .5 && _State < 1.5) color.rgb *= 1 + paw * warmth * (_Pulse-.5) * .12;
                return color;
            }
            ENDCG
        }
    }
}
