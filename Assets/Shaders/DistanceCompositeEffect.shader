Shader "Hidden/DistanceCompositeEffect" {
	Properties{
		_MainTex("Texture", 2D) = "white" {}
		_OutlineWidth("Outline width", Float) = 0.0025
		_OutlineColor("Outline color", Color) = (1, 0, 0, 1)
	}
		SubShader{
		  Cull Off ZWrite Off ZTest Always
		  Blend SrcAlpha OneMinusSrcAlpha
		  Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata {
			  float4 vertex : POSITION;
			  float2 uv : TEXCOORD0;
			};

			struct v2f {
			  float2 uv : TEXCOORD0;
			  float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v) {
			  v2f o;
			  o.vertex = UnityObjectToClipPos(v.vertex);
			  o.uv = v.uv;
			  return o;
			}

			sampler2D _MainTex;
			sampler2D _TargetTexture;
			float _OutlineWidth;
			fixed4 _OutlineColor;

			fixed4 frag(v2f i) : SV_Target {
			  float4 distanceTransform = tex2D(_MainTex, i.uv);
			  float mask = tex2D(_TargetTexture, i.uv);

			  float distance = sqrt(distanceTransform.z) - _OutlineWidth + 0.001;
			  float alpha = saturate(1 - saturate(distance / fwidth(distance)) - mask);

			  return _OutlineColor * alpha;
			}
			ENDCG
		  }
	}
}