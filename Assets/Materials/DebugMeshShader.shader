Shader "Custom/DebugMeshShader"
{
    Properties
    {
        _Color ("Color", Color) = (1, 0, 0, 0.5)
        
        // For each material that has a color, this must be a different number
        _StencilID ("Stencil ID", Int) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent+100" "RenderType"="Transparent" "IgnoreProjector"="True" }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        ZTest Always 

        Stencil 
        {
            Ref [_StencilID]
            Comp NotEqual   // Only draw if this pixel DOES NOT have our ID
            Pass Replace    // If we draw, stamp our ID onto this pixel
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            float4 _Color;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}