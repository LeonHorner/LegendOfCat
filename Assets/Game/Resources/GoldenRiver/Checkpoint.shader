Shader "LegendOfCat/Checkpoint"
{
    Properties {
        [PerRendererData] _MainTex ("Atlas", 2D) = "white" {}
        _State ("State", Float) = 0
        _Pulse ("Pulse", Float) = 0
        _Active ("Active light", Float) = 0
        _Visited ("Visited engraving", Float) = 0
        _Spark ("Spark mode", Float) = 0
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
            float _Pulse, _Active, _Visited, _Spark;
            struct Input { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 tint : COLOR; };
            struct Output { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; fixed4 tint : COLOR; };
            Output vert(Input v) { Output o; o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.tint=v.tint; return o; }
            fixed4 frag(Output i) : SV_Target {
                float2 cell = i.uv;
                if (_Spark > .5) {
                    float glow = saturate(1-length(cell*2-1));
                    return fixed4(lerp(i.tint.rgb, float3(1,.94,.65), glow*glow), pow(glow,1.4)*i.tint.a);
                }
                fixed4 color = tex2D(_MainTex, float2(cell.x / 3, cell.y));
                fixed3 active = tex2D(_MainTex, float2((cell.x+1)/3,cell.y)).rgb;
                fixed3 visited = tex2D(_MainTex, float2((cell.x+2)/3,cell.y)).rgb;
                // Only the warm engraving breathes; the stone outline and surrounding moss stay still.
                float paw = smoothstep(.25,.32,cell.x) * (1-smoothstep(.67,.73,cell.x))
                    * smoothstep(.24,.30,cell.y) * (1-smoothstep(.66,.73,cell.y));
                float light = smoothstep(0,1,_Active);
                fixed3 engraving = lerp(color.rgb, visited, smoothstep(0,1,_Visited));
                engraving = lerp(engraving,active,light);
                color.rgb = lerp(color.rgb,engraving,paw);
                float warmth = saturate((color.r-color.b)*4);
                color.rgb *= 1 + paw * warmth * (_Pulse-.5) * .12 * light;
                return color;
            }
            ENDCG
        }
    }
}
