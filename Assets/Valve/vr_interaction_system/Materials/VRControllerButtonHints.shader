Shader "VRControllerButtonHints"
{
	Properties
	{
		_MainTex ( "Texture", 2D ) = "white" {}
		_Color( "Color", Color ) = ( 1, 1, 1, 1 )
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent+1" "RenderType" = "Transparent" }
		LOD 100

		Pass
		{
			// Render State ---------------------------------------------------------------------------------------------------------------------------------------------
			Blend SrcAlpha OneMinusSrcAlpha // Alpha blending
			Cull Off
			ZWrite Off
			ZTest Always

			CGPROGRAM

			#pragma vertex MainVS
			#pragma fragment MainPS
			
			// Includes -------------------------------------------------------------------------------------------------------------------------------------------------
			#include "UnityCG.cginc"

			// Structs --------------------------------------------------------------------------------------------------------------------------------------------------
			struct VertexInput
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct VertexOutput
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			// Globals --------------------------------------------------------------------------------------------------------------------------------------------------
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			
			// MainVs ---------------------------------------------------------------------------------------------------------------------------------------------------
			VertexOutput MainVS( VertexInput i )
			{
				VertexOutput o;

				o.vertex = mul(UNITY_MATRIX_MVP, i.vertex);
				
				o.uv = TRANSFORM_TEX( i.uv, _MainTex );
				
				return o;
			}
			
			// MainPs ---------------------------------------------------------------------------------------------------------------------------------------------------
			float4 MainPS( VertexOutput i ) : SV_Target
			{
				float4 vColor = tex2D(_MainTex, i.uv).rgba * _Color.rgba;

				return vColor.rgba;
			}

			ENDCG
		}
	}
}
