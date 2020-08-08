﻿Shader "Custom/Foliage"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        grassHeight ("Grass height", float) = 3
		windMove ("Wind Move Freq", Float) = 1
        windDensity ("Wind Grouping Weight", Float) = 1
        windStrength ("Wind Strength (max displace)", Float) = 1
        yDisplace ("idk how to do math so do vert displace manually lol", Float) = .3
        walkAura ("Walk Aura idk why no runtime ", Float) = 10
        stepForce ("Step force", Float) = .03

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

            struct vertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

            };

            struct vertexOutput
            {
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float dispWeight : TEXCOORD2;
                float4 clipPos : SV_POSITION;
                
            };

            sampler2D _MainTex;            
            float4 _MainTex_ST;
            uniform float windMove;
            uniform float windDensity;
            uniform float windStrength;
            uniform float walkAura;
            uniform float stepForce;
            uniform float grassHeight;
            uniform float yDisplace;
            uniform float2 characterPositions[10];
            uniform float characterCount;

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

            float homemadeSmoothStep(float x)
            {
                //return saturate((x - 1) * (x - 1) * (x - 1) + 1);
                //return saturate((.8*x-1) * (.8*x-1) * (.8*x-1) + 1);
                //return saturate(x/2);
            }

            float mid(float a, float b) {
                //return b + .428 * a * a/b;
                return 7/8 * a + b/2;
               // return (a+b)/2;
                //return sqrt(a * a + b * b);
            }

            float sqrMagnitude(float2 a, float2 b) {
                return dot(a - b, a - b);
            }

            vertexOutput vert (vertexInput v)
            {
                vertexOutput o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex); //via matrices, get the world pos given the local vertex and unity shit
                //float3 currWorldPos = mul(unity_ObjectToWorld, v.vertex); //via matrices, get the world pos given the local vertex and unity shit
                o.dispWeight = smoothstep(-.2, 1, v.vertex.y); //weight based on distance from bottom 
                //o.dispWeight = smoothstep(0, 1, v.vertex.y); // default alternative

                //"pseudo wind"
                //get an input of two dimensions based on time causes stetching
                //all this shit here is random so literally you can throw any 
                //math operation here and try to make it look more like grass
                float2 inputX = float2(o.worldPos.x, o.worldPos.z);
                float2 inputZ = float2(o.worldPos.z, o.worldPos.x);
                inputX *= (_SinTime[1] * windMove); //sintime vs time? one feels more uniform idk
                inputZ *= (_SinTime[1] * windMove);
                // //feed into noise in outputting a displacement
                //axis independence is a choice, not poor programming
                float xNoiseVal, zNoiseVal;
                Unity_GradientNoise_float(inputX, windDensity, xNoiseVal);
                Unity_GradientNoise_float(inputZ, windDensity, zNoiseVal);
                
                 //add to breeze displacement
                v.vertex.x += xNoiseVal * windStrength * o.dispWeight;
                v.vertex.z += zNoiseVal * windStrength * o.dispWeight;
                v.vertex.y -= mid(xNoiseVal, xNoiseVal) * yDisplace * o.dispWeight;

                //character interaction
                //for each character (up to 4 only)


                for(int i = 0; i < characterCount; ++i) {
                    //compare its (global) position to the global vertex positions
                    float2 sphereDisp = (o.worldPos.xz - characterPositions[i])  * (1 - saturate(sqrMagnitude(characterPositions[i], o.worldPos.xz) / walkAura));

                    v.vertex.x -= sphereDisp.y *  stepForce * o.dispWeight; //idk why scales be off
                    v.vertex.z += sphereDisp.x *  stepForce * .1 * o.dispWeight; 
                    v.vertex.y -= mid(sphereDisp.x, sphereDisp.y)  * clamp(o.dispWeight, .3, 1);

                }
                //set the actual world pos, set DEFAULT TEXTURE
               // o.vertex = v.vertex;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.clipPos = UnityObjectToClipPos(v.vertex); 

                return o;
            }

            float4 frag (vertexOutput o) : SV_Target {
               
             //   float aura = o.worldPos.x - characterPositions[0].x;
             //   return float4(o.worldPos.x - characterPositions[0].x, 1, o.worldPos.z - characterPositions[0].y, 1);
                //return float4(o.vertex.yyy, 1);
                float4 col = tex2D(_MainTex, o.uv);
                return col;                
            }

            ENDCG
        }

    }
}

