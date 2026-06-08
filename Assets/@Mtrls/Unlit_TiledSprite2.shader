Shader "Custom/WaveTile2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Tiling ("Tiling", Vector) = (1,1,0,0)
        _Alpha ("Alpha", Range(0,1)) = 1
        
        //  물결 효과 파라미터
        _WaveSpeed ("Wave Speed", Range(0, 2)) = 0.5
        _WaveAmplitude ("Wave Amplitude", Range(0, 0.1)) = 0.02
        _WaveFrequency ("Wave Frequency", Range(0, 10)) = 3
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            sampler2D _MainTex;
            float4 _Tiling;
            float _Alpha;
            float _WaveSpeed;
            float _WaveAmplitude;
            float _WaveFrequency;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };
            
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * _Tiling.xy;
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                //  물결 효과 계산
                float time = _Time.y * _WaveSpeed;
                
                // X와 Y 방향으로 다른 주파수의 사인파 (자연스러운 물결)
                float waveX = sin(i.uv.y * _WaveFrequency + time) * _WaveAmplitude;
                float waveY = sin(i.uv.x * _WaveFrequency + time * 0.7) * _WaveAmplitude;
                
                // UV에 물결 효과 적용
                float2 wavedUV = i.uv + float2(waveX, waveY);
                
                float4 col = tex2D(_MainTex, wavedUV);
                col.a *= _Alpha;
                return col;
            }
            ENDHLSL
        }
    }
}