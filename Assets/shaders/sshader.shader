Shader "Custom/grass"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		windMove ("Wind Move Freq", Float) = 1
        windDensity ("Wind Grouping Weight", Float) = 1
        windStrength ("Wind Strength (max displace)", Float) = 1

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
                float2 uv : TEXCOORD0;

            };

            struct vertexOutput
            {
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float dispWeight : TEXCOORD2;
                float dispDebug : TEXCOORD3;
                float4 clipPos : SV_POSITION;
                
            };

            sampler2D _MainTex;            
            float4 _MainTex_ST;
            uniform float windMove;
            uniform float windDensity;
            uniform float windStrength;
            uniform float3 globalPlayerPos;


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

            vertexOutput vert (vertexInput v)
            {
                vertexOutput o;

                float3 currWorldPos = mul(unity_ObjectToWorld, v.vertex); //via matrices, get the world pos given the local vertex and unity shit
                //weight based on distance from bottom 
                o.dispWeight = smoothstep(0, 1, v.vertex.y);

                //for "pseudo wind"
                //get an input of two dimensions based on time causes stetching
                //tempoerarily just shove the y value in instead
                float2 input2 = float2(currWorldPos.y, currWorldPos.y);
                input2 *= (_Time[1] * windMove);
                // //feed into noise in outputting a displacement
                float noiseVal;
                Unity_GradientNoise_float(input2, windDensity, noiseVal);
                float windDisp = (noiseVal - .5) * windStrength;

                //for player interaction
                //displace the vertex AWAY from the position of the player
                // float xDisp = v.vertex.x - (currWorldPos.x - globalPlayerPos.x);
                // float zDisp = v.vertex.z - (currWorldPos.z - globalPlayerPos.z);
                // float playerDisp = saturate(v.vertex)

                //by combining both calculations, displace the vertex respectively
                o.dispDebug = windDisp;
                v.vertex.x += o.dispDebug * o.dispWeight;
                // //v.vertex.y = smoothstep(0, 1, 1 - v.vertex.x * v.vertex);
                v.vertex.z += o.dispDebug * o.dispWeight;
               // v.vertex.y = smoothstep(-v.vertex.y, v.vertex.y, -v.vertex.y);


                //set the actual world pos, set DEFAULT TEXTURE
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex); //via matrices, get the world pos given the local vertex and unity shit
                o.clipPos = UnityObjectToClipPos(v.vertex); 

                return o;
            }

            float4 frag (vertexOutput o) : SV_Target {
                float4 col = tex2D(_MainTex, o.uv);
                return col;
                
               // return float4(o.dispDebug, o.dispDebug, o.dispDebug, 1);
                
            }

            ENDCG
        }

    }
}
