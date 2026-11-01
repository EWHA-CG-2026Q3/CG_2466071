Shader "CG/S17_MyTextureAbs"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            sampler2D _BaseMap;

            void vert(float3 positionOS : POSITION,
                      float2 uv : TEXCOORD0,
                      out float2 uvOut : TEXCOORD0,
                      out float4 positionCS : SV_POSITION)
            {
                uvOut = uv;
                positionCS = TransformObjectToHClip(positionOS);
            }

            float4 frag(float2 uv : TEXCOORD0) : SV_Target
            {
                // Quad의 왼쪽 위 1/4 밖은 회색으로 표시한다.
                if (uv.x < 0 || uv.x > 0.5 || uv.y < 0.5 || uv.y > 1)
                    return float4(0.5, 0.5, 0.5, 1);

                // 왼쪽 위 (0, 0.5)~(0.5, 1)을 오른쪽 아래 (0.5, 0)~(1, 0.5)로 옮긴다.
                float2 p = uv + float2(0.5, -0.5);
                return tex2D(_BaseMap, p);
            }
            ENDHLSL
        }
    }
}