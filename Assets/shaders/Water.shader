Shader "Custom/Water"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Distance ("Distance", Float) = 1
    }
    SubShader
    {

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct vertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct vertexOutput
            {
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float4 clipPos : SV_POSITION;
                float depth : DEPTH;
            };

            vertexOutput vert (vertexInput v)
            {
                vertexOutput o;
                o.clipPos = UnityObjectToClipPos(v.vertex);
                //object depth
                o.depth = -UnityObjectToViewPos(v.vertex).z * _ProjectionParams.w;
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex); //via matrices, get the world pos given the local vertex and unity shit
                return o;
            }

            sampler2D _MainTex;
            uniform sampler2D_float _CameraDepthTexture;
            uniform float _Distance;
    

            float4 frag (vertexOutput o) : SV_Target
            {
                //scene depth
                o.uv = 1 - o.uv; //shits upside down idk
                float sceneDepth = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, o.uv));
                sceneDepth = Linear01Depth(sceneDepth);

                //water depth = scene depth - object depth
                float waterDepth = sceneDepth - o.depth;
                
                float waterWeight = saturate(waterDepth / _Distance);

                return float4(waterWeight, waterWeight, waterWeight, 1);
                float3 colorA = float3(.1, 1, 1);  
                float3 colorB = float3(0, .2, 1);

           //     return float4(depthWeight, 1);
                return float4(o.depth, 1-o.depth, 1-o.depth, 1);

            }
            ENDCG
        }
    }
}
