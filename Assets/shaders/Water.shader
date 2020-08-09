Shader "Custom/Water"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
     //   _Distance ("Distance", Float) = 1
      //  _DeepColor ("Deep Color", Color) = (0, 1, 1)
        // _AmbientLight ("Ambient light col", Color) = (0,0,0,1) 
        // _Gloss ("Gloss", float) = 1
        _WaterColor ("Color", Color) = (0, 0, .2)
        _RefreactNoiseScale ("refraction Noise Scale",Float) = 1
        _RefractNoiseDisp ("refraction Noise displacement", Float) = 1
        _WaterAmp ("water max amplitude", Float) = 5
        _Choppiness ("chop penis", Float) = .01
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent"}

        GrabPass { "_BackgroundTexture" }

        //Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

            struct vertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct vertexOutput
            {
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float4 grabPos : TEXCOORD2;
                float4 clipPos : SV_POSITION;
                float depth : DEPTH;
            };

            float2 unity_gradientNoise_dir(float2 p) {
                p = p % 289;
                float x = (34 * p.x + 1) * p.x % 289 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;
                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }

            float unity_gradientNoise(float2 p) {
                float2 ip = floor(p);
                float2 fp = frac(p);
                float d00 = dot(unity_gradientNoise_dir(ip), fp);
                float d01 = dot(unity_gradientNoise_dir(ip + float2(0, 1)), fp - float2(0, 1));
                float d10 = dot(unity_gradientNoise_dir(ip + float2(1, 0)), fp - float2(1, 0));
                float d11 = dot(unity_gradientNoise_dir(ip + float2(1, 1)), fp - float2(1, 1));
                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
                return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x);
            }

            void Unity_GradientNoise_float(float2 UV, float Scale, out float Out) {
                Out = unity_gradientNoise(UV * Scale) + 0.5;
            }

            sampler2D _MainTex;
            sampler2D _BackgroundTexture;
            uniform sampler2D_float _CameraDepthTexture;
            //uniform float _Distance;
            float _Gloss;
            float4 _AmbientLight;
            float4 _WaterColor;
            float _RefreactNoiseScale;
            float _RefractNoiseDisp;
            float _Choppiness;
            float _WaterAmp;

            vertexOutput vert (vertexInput v)
            {
                vertexOutput o;
                o.uv = v.uv;

                v.vertex.y += sin( (o.uv.x + o.uv.y + _Time[1]) * _Choppiness ) * _WaterAmp;
                //v.vertex.x += 10;


                o.clipPos = UnityObjectToClipPos(v.vertex);
                //object depth
                o.depth = -UnityObjectToViewPos(v.vertex).z * _ProjectionParams.w;
                o.grabPos = ComputeGrabScreenPos(o.clipPos);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex); //via matrices, get the world pos given the local vertex and unity shit
                
               
                return o;
            }
       

            float4 frag (vertexOutput o) : SV_Target
            {
                //scene depth
               // o.uv = 1 - o.uv; //shits upside down idk
                // float sceneDepth = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, o.uv));
                // sceneDepth = Linear01Depth(sceneDepth);

                // //water depth = scene depth - object depth
                // float waterDepth = sceneDepth - o.depth;
                // float waterWeight = saturate(waterDepth / _Distance);

                // //color dependent on distance from camera
                // float4 waterColor = lerp(_ShallowColor, _DeepColor, waterWeight);

                


                float noise;
                Unity_GradientNoise_float(o.uv * _Time[1], _RefreactNoiseScale, noise);
                o.grabPos += noise * _RefractNoiseDisp; //refraction
                float4 screenColorDistort = tex2Dproj(_BackgroundTexture, o.grabPos);
                
                //combination
                float4 finCol = lerp(screenColorDistort, _WaterColor, _WaterColor.w);

               // return float4(o.grabPos.zzz,1);

               // return waterWeight;
               return finCol;
             //  return finCol * allLighting(o);
              // return float4(allLighting(o).xyz, 0);
              // return (screenColorDistort/1.2);
               // return finCol;

            }
            ENDCG
        }
    }
}
