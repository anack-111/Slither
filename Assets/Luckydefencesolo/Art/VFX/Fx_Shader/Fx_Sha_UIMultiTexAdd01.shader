Shader "RolingHero/VFX/UIMultiTexAdd01"
{
    Properties
    {
		_Color ("Color" , Vector) = (1,1,1,1)
		_ColorPow("ColorPow", Float) = 1
        _MainTex ("MainTex", 2D) = "white" {}
		_MainUV ("_MainUV" , Vector) = (1,1,0,0)
		_SubTex01 ("SubTex01", 2D) = "white" {}
		_SubUV01 ("_SubUV01" , Vector) = (1,1,0,0)
		_MainPanX ("MainPanX", Float) = 0
		_MainPanY ("MainPanY", Float) = 0
		_SubPanX ("SubPanX", Float) = 0
		_SubPanY ("SubPanY", Float) = 0
		_Stencil ("Stencil Ref", Float) = 0
		_StencilReadMask ("ReadMask [0;255]", Int) = 255
		_StencilWriteMask ("WriteMask [0;255]", Int) = 255
		_ColorMask ("Color Mask", Float) = 15
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Int) = 8
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Stencil Operation", Int) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilFail ("Stencil Fail", Int) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilZFail ("Stencil ZFail", Int) = 0
    }
    SubShader
    {
        Tags 
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"PreviewType" = "Plane"
		}
        LOD 100
		
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Fog {Mode Off}
		Blend One One
		ColorMask [_ColorMask]
		Cull Off

        Pass
        {
			Stencil {

					Ref [_Stencil]
					ReadMask [_StencilReadMask]
					WriteMask [_StencilWriteMask]
					Comp [_StencilComp]
					Pass [_StencilOp]
					Fail [_StencilFail]
					ZFail [_StencilZFail]
				}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
				float4 color : COLOR;
                float2 uv : TEXCOORD0;
				//float2 uv1 : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
				//float2 uv1 : TEXCOORD1;
				float4 color : COLOR;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

			CBUFFER_START(UnityPerMaterial)
            sampler2D _MainTex;
            //float4 _MainTex_ST;
			sampler2D _SubTex01;
			//float4 _SubTex01_ST;
			float4 _Color;
			float4 _MainUV;
			float4 _SubUV01;
			float _ColorPow;
			float _MainPanX;
			float _MainPanY;
			float _SubPanX;
			float _SubPanY;
			CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
				o.color = v.color * _Color;
                o.vertex = UnityObjectToClipPos(v.vertex);
				//o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv = v.uv;
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex) + float2(_MainPanX, _MainPanY)*_Time.y;
				//o.uv1 = TRANSFORM_TEX(v.uv1, _SubTex01) + float2(_SubPanX, _SubPanY)*_Time.y;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
				fixed4 tex01 = tex2D(_MainTex, (i.uv + _MainUV.ba) * _MainUV.rg + float2(_MainPanX, _MainPanY)*_Time.y);
				fixed4 tex02 = tex2D(_SubTex01, (i.uv + _SubUV01.ba) * _SubUV01.rg + float2(_SubPanX, _SubPanY)*_Time.y);
				fixed4 tex03 = tex01 * tex02 * _ColorPow;
				fixed4 tex00 = clamp(tex03 ,0,1);
				fixed4 col = tex00 * i.color * i.color.a;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
