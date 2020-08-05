Shader "Outlined/Uniform"
{
	Properties
	{
		_Color("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex ("Texture", 2D) = "white" {}
		_OutlineColor ("Outline color", Color) = (0,0,0,1)
		_OutlineWidth ("Outlines width", Range (0.0, 2.0)) = 1.1
	}

	CGINCLUDE
	#include "UnityCG.cginc"

	struct vertexInput
	{
		float4 vertex : POSITION;
	};

	struct vertexOutput
	{
		float4 pos : POSITION;
	};

	uniform float _OutlineWidth;
	uniform float4 _OutlineColor;
	uniform sampler2D _MainTex;
	uniform float4 _Color;

	ENDCG

	SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" }

		Pass //Outline
		{
			Cull Back
        	ZWrite Off
            ZTest Always
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			vertexOutput vert(vertexInput v)
			{
				vertexInput original = v;
				v.vertex.xyz += _OutlineWidth * normalize(v.vertex.xyz);

				vertexOutput o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;

			}

			half4 frag(vertexOutput i) : COLOR
			{
				return _OutlineColor;
			}

			ENDCG
		}

		Tags{ "Queue" = "Geometry"}

		CGPROGRAM
		#pragma surface surf Lambert
		 
		struct Input {
			float2 uv_MainTex;
		};
		 
		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	Fallback "Diffuse"
}