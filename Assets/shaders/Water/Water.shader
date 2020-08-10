Shader "Custom/Water"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BumpMap ("Normal Map (don't touch for now", 2D) = "bump" {}
     //   _Distance ("Distance", Float) = 1
      //  _DeepColor ("Deep Color", Color) = (0, 1, 1)
        // _AmbientLight ("Ambient light col", Color) = (0,0,0,1) 
        // _Gloss ("Gloss", float) = 1
        _WaterColor ("Color", Color) = (0, 0, .2)
        _RefractNoiseScale ("refraction Noise Scale",Float) = 1
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
            Cull Back
            ZWrite Off
            Blend Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

            struct vertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
               // float4 tangent : TANGENT;
            };

            struct vertexOutput
            {
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float4 grabPos : TEXCOORD2;
                float3 normal :TEXCOORD3;
                float3 worldNormal : TEXCOORD4;

                float4 clipPos : SV_POSITION;
          //      float depth : DEPTH;
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
            sampler2D _BumpMap;
            uniform sampler2D_float _CameraDepthTexture;
            //uniform float _Distance;
            float _Gloss;
            float4 _AmbientLight;
            float4 _WaterColor;
            float _RefractNoiseScale;
            float _RefractNoiseDisp;
            float _Choppiness;
            float _WaterAmp;

            vertexOutput vert (vertexInput v)
            {
                vertexOutput o;
                o.uv = v.uv;
                o.normal = v.normal;
                v.vertex.y += sin( (o.uv.x + o.uv.y + _Time[1]) * _Choppiness ) * _WaterAmp;
                //v.vertex.x += 10;

                o.clipPos = UnityObjectToClipPos(v.vertex);
                //object depth
               // o.depth = -UnityObjectToViewPos(v.vertex).z * _ProjectionParams.w;
                o.grabPos = ComputeGrabScreenPos(o.clipPos);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex); //via matrices, get the world pos given the local vertex and unity shit
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                // float3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                // float3 wBitangent = cross(worldNormal, worldTangent) * (v.tangent.w * unity_WorldTransformParams.w);
                // o.tspace0 = float3(worldTangent.x, wBitangent.x, worldNormal.x);
                // o.tspace1 = float3(worldTangent.y, wBitangent.y, worldNormal.y);
                // o.tspace2 = float3(worldTangent.z, wBitangent.z, worldNormal.z);
                //o.worldRefl = reflect(-normalize(UnityWorldSpaceViewDir(o.worldPos)), o.worldNormal);
               
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

                //reflection - based on angle of incidence (being the camera angle)
               // float4 tnormal = normalize(float4(UnpackNormal(tex2D(_BumpMap, o.uv)), 0));


        
                
                
               // float4 screenColorDistort = tex2Dproj(_BackgroundTexture, o.grabPos);
                
                
//                float3 worldNormal;
                // worldNormal.x = dot(o.tspace0, tnormal);
                // worldNormal.y = dot(o.tspace1, tnormal);
                // worldNormal.z = dot(o.tspace2, tnormal);

                // //return float4(worldNormal, 1);
                // float3 worldViewDir = normalize(UnityWorldSpaceViewDir(o.worldPos));
                // float3 worldRefl = reflect(-worldViewDir, worldNormal);
                    //float4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, worldRefl);
                // float3 skyColor = DecodeHDR(skyData, unity_SpecCube0_HDR);
                // float3 worldViewDir = normalize(UnityWorldSpaceViewDir(o.worldPos)); //Direction of ray from the camera towards the object surface
                // float3 reflection = reflect(-worldViewDir, o.worldNormal); // Direction of ray after hitting the surface of object
                // /*If Roughness feature is not needed : UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, reflection) can be used instead.
                // It chooses the correct LOD value based on camera distance*/
                // float4 skyData = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflection, 0); //UNITY_SAMPLE_TEXCUBE_LOD('cubemap', 'sample coordinate', 'map-map level')
                // float3 skyColor = DecodeHDR (skyData, unity_SpecCube0_HDR); // This is done because the cubemap is stored HDR
                // return float4(skyData.xyz, 1.0);
               //return float4(unity_SpecCube0);

                //refraction - only below surface
                float noise; Unity_GradientNoise_float(o.uv * _Time[1], _RefractNoiseScale, noise);
                float distortion = noise * _RefractNoiseDisp;

                float4 refraction = tex2Dproj(_BackgroundTexture, o.grabPos + distortion);
                float4 noRefraction = tex2Dproj(_BackgroundTexture, o.grabPos);

                if(LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, o.grabPos + distortion))) < o.grabPos.w)  refraction = noRefraction;

                return lerp(refraction, _WaterColor, _WaterColor.w);


            }
            ENDCG
        }
    }
}
