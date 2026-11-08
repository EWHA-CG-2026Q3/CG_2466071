Shader "CG/Mid_TwilightSky"
{
    Properties
    {
        _TopColor("Upper night sky", Color)=(0.025,0.055,0.13,1)
        _HorizonColor("Blue twilight horizon", Color)=(0.24,0.34,0.46,1)
        _CloudColor("Thin clouds", Color)=(0.19,0.25,0.34,1)
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            half4 _TopColor, _HorizonColor, _CloudColor;
            CBUFFER_END
            struct Varyings { float4 positionCS:SV_POSITION; float3 direction:TEXCOORD0; };
            Varyings vert(float3 positionOS:POSITION)
            { Varyings o; o.positionCS=TransformObjectToHClip(positionOS); o.direction=positionOS; return o; }
            float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float noise(float2 p)
            {
                float2 i=floor(p), f=frac(p); f=f*f*(3-2*f);
                return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);
            }
            half4 frag(Varyings i):SV_Target
            {
                float3 d=normalize(i.direction);
                half3 c=lerp(_HorizonColor.rgb,_TopColor.rgb,pow(saturate(d.y+.08),.45));
                float2 p=d.xz/max(.15,d.y+.32)*2.1;
                float clouds=noise(p)*.55+noise(p*2.1)*.3+noise(p*4.3)*.15;
                float mask=smoothstep(.49,.78,clouds)*smoothstep(-.04,.15,d.y)*.28;
                c=lerp(c,_CloudColor.rgb,mask);
                return half4(c,1);
            }
            ENDHLSL
        }
    }
}
