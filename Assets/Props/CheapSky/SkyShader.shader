Shader "Aubergine/ObjectSpace/SkyShader" {
	Properties {
		_CloudTint ("Cloud Tint Color", Color) = (1, 1, 1, 1)
		_CloudTex ("Texture", 2D) = "white" {}
		_Level1 ("Low Level Range", Range (0, 1.0)) = 0.1
		_Level2 ("Medium Level Range", Range (0, 1.0)) = 0.2
		_Level3 ("High Level Range", Range (0, 1.0)) = 0.4
		_Col1 ("Low Color", Color) = (.1, .1, .5, 1)
		_Col2 ("Medium Color", Color) = (.1, .1, .5, 1)
		_Col3 ("High Color", Color) = (.2, .2, .8, 1)
		_Col4 ("Top Color", Color) = (.2, .2, .5, 1)
		_Mist1 ("Low Level Mist", Range (0, 1.0)) = 0.1
		_Mist2 ("Medium Level Mist", Range (0, 1.0)) = 0.2
		_Mist3 ("Top Level Mist", Range (0, 1.0)) = 1.0
		_Speed1 ("Clouds1 Speed", float) = 0.01
		_Speed2 ("Clouds2 Speed", float) = 0.02
		_Speed3 ("Clouds3 Speed", float) = 0.03
		_DirX ("Wind X Direction", Range (-1.0, 1.0)) = 0.0
		_DirZ ("Wind Z Direction", Range (-1.0, 1.0)) = 1.0
	}
	SubShader {
		Tags { "Queue"="Background" "RenderType"="Background" }
		Cull Off ZWrite Off Lighting Off Fog { Mode Off }
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"

				struct a2f {
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
					float3 normal : NORMAL;
				};

				struct v2f {
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
					float ang : TEXCOORD1;
				};
				
				sampler2D _CloudTex;
				float4 _CloudTex_ST;
				float4 _CloudTint, _Col1, _Col2, _Col3, _Col4;
				float _Level1, _Level2, _Level3;
				float _Mist1, _Mist2, _Mist3;
				float _Speed1, _Speed2, _Speed3;
				float _DirX, _DirZ;

				half3 grad(float3 ori, float3 dest, float start, float end, float step) {
					return lerp(ori, dest, smoothstep(start, end, step));
				}

				v2f vert(a2f v) {
					v2f o;
					o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
					o.uv = TRANSFORM_TEX(v.texcoord, _CloudTex);
					o.ang = v.normal.y;
					return o;
				}

				half4 frag (v2f i) : COLOR {
					float angle = i.ang;
					//Clouds
					float2 wind = float2(_DirX, _DirZ);
					half4 clouds = tex2D(_CloudTex, i.uv + _Time.y * wind * _Speed1) * _CloudTint;
					//Try removing below cloud layers if you want
					clouds.rgb += tex2D(_CloudTex, i.uv + _Time.y * wind * _Speed2).rgb * _CloudTint.rgb;
					clouds.rgb *= tex2D(_CloudTex, i.uv + _Time.y * wind * _Speed3).rgb * _CloudTint.rgb;
					//Define gradient with clouds mix
					half3 lev1 = lerp(_Col2, clouds.rgb, clouds.a * _Mist1);
					half3 lev2 = lerp(_Col3, clouds.rgb, clouds.a * _Mist2);
					half3 lev3 = lerp(_Col4, clouds.rgb, clouds.a * _Mist3);
					//Blend all
					half3 col = _Col1;
					if (angle > 0f)
						col = grad(_Col1, lev1, 0f, _Level1, angle);
					if (angle > _Level1)
						col = grad(lev1, lev2, _Level1, _Level2, angle);
					if (angle > _Level2)
						col = grad(lev2, lev3, _Level2, _Level3, angle);
					return half4(col, 1);
				}
			ENDCG
		}
	}
}