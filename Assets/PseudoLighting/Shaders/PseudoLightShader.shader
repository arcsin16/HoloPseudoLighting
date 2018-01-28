Shader "Unlit/PseudoLightShader"
{
	CGINCLUDE
	#include "UnityCG.cginc"

	//影の数
	int _Count;

	//影の位置、大きさ、色を複数指定するための配列
	fixed4 _PositionArray[32];
	fixed4 _ColorArray[32];

	struct v2f
	{
		float4 vertex : SV_POSITION;
		float3 worldPos : TEXCOORD0;
		UNITY_VERTEX_OUTPUT_STEREO
	};

	v2f vert(appdata_base v)
	{
		v2f o;
		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.worldPos = mul(unity_ObjectToWorld, v.vertex);
		return o;
	}

	fixed4 frag(v2f input) : SV_Target
	{
		fixed4 c = fixed4(0,0,0,0);
		for (int i = 0; i < _Count; i++) {
			//光源からの距離に応じて壁や床の色を設定する。
			fixed dist = distance(input.worldPos, _PositionArray[i].xyz);
			fixed alpha = 1 - dist/_PositionArray[i].w;
			if (alpha > 0) {
				c += _ColorArray[i] * alpha * 0.8;
			}
		}

		return c;
	}

	ENDCG
	SubShader
	{
		Tags {
			"RenderType"="Opaque"
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#pragma only_renderers d3d11
			ENDCG
		}
	}
	Fallback "Diffuse"
}
