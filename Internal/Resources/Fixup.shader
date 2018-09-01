// KlakSpout - Spout realtime video sharing plugin for Unity
// https://github.com/keijiro/KlakSpout
Shader "Hidden/Spout/Fixup" {
    Properties {
        _MainTex("", 2D) = "white" {}
    }

	CGINCLUDE
	#include "UnityCG.cginc"

	sampler2D _MainTex;
	float _ClearAlpha;

	v2f_img vert(appdata_img v) {
		v2f_img o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = float2(v.texcoord.x, 1 - v.texcoord.y);
		return o;
	}

	fixed4 frag(v2f_img i) : SV_Target {
		fixed4 col = tex2D(_MainTex, i.uv);
		col.a = saturate(col.a + _ClearAlpha);
		return col;
	}
	ENDCG

    SubShader {
        Cull Off ZWrite Off ZTest Always
        Pass {
            CGPROGRAM
			#pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}
