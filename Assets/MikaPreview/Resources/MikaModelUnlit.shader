Shader "LegendOfCat/Mika Model Unlit"
{
    Properties { _MainTex ("Base Color", 2D) = "white" {} }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            struct Varyings { float4 position : SV_POSITION; float2 uv : TEXCOORD0; };
            Varyings vert(appdata_base input)
            {
                Varyings output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.uv = input.texcoord.xy;
                return output;
            }
            fixed4 frag(Varyings input) : SV_Target { return tex2D(_MainTex, input.uv); }
            ENDCG
        }
    }
}
