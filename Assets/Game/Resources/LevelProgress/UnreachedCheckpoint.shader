Shader "LegendOfCat/HUD/UnreachedCheckpoint"
{
    Properties
    {
        _MainTex ("Checkpoint artwork", 2D) = "white" {}
        _Tint ("World tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            fixed4 _Tint;
            fixed4 frag(v2f_img input) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, input.uv) * _Tint;
                fixed gray = dot(color.rgb, fixed3(.299, .587, .114));
                gray = lerp(.32, .68, gray);
                return fixed4(gray, gray, gray, color.a * .82);
            }
            ENDCG
        }
    }
}
