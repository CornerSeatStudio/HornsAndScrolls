Shader "Custom/testLight"
{
    Properties
    {
        _TempPos ("Pos", Vector) = (0,0,0)

    }
    SubShader
    {
        Tags { "RenderType" = "Opaque"}

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct vertexInput
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct vertexOutput
            {
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            uniform float3 _TempPos;

            vertexOutput vert (vertexInput v)
            {
                vertexOutput o;
                o.uv = v.uv;
                o.normal = v.normal;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex); //via matrices, get the world pos given the local vertex and unity shit
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (vertexOutput o) : SV_Target
            {
                float dist = distance(_TempPos, o.worldPos);
                float3 temp = saturate(1 - dist/100);
                return float4(temp, .00001);
                
            }
            ENDCG
        }
    }
}
