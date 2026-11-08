Shader "CG/Mid_Moon"
{
    Properties { [HDR] _BaseColor("Moonlight",Color)=(1.8,1.95,2.2,1) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            CBUFFER_END
            struct Varyings { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; float3 normalWS:TEXCOORD1; float3 positionWS:TEXCOORD2; };
            Varyings vert(float3 p:POSITION,float3 n:NORMAL,float2 uv:TEXCOORD0)
            {Varyings o;o.pos=TransformObjectToHClip(p);o.uv=uv;o.normalWS=TransformObjectToWorldNormal(n);o.positionWS=TransformObjectToWorld(p);return o;}
            float hash(float2 p){return frac(sin(dot(p,float2(17.13,91.7)))*43758.5453);}
            float noise(float2 p)
            {float2 q=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(q),hash(q+float2(1,0)),f.x),lerp(hash(q+float2(0,1)),hash(q+1),f.x),f.y);}
            half4 frag(Varyings i):SV_Target
            {
                float2 uv=i.uv*float2(2,1);
                float terrain=.48+.3*noise(uv*9)+.14*noise(uv*25)+.08*noise(uv*63);
                float2 cell=frac(uv*18)-.5;
                float crater=length(cell);
                terrain-=.13*(1-smoothstep(.12,.28,crater));
                terrain+=.08*exp(-pow((crater-.29)*26,2));
                float facing=saturate(dot(normalize(i.normalWS),normalize(GetWorldSpaceViewDir(i.positionWS))));
                return half4(_BaseColor.rgb*terrain*(.72+.28*sqrt(facing)),1);
            }
            ENDHLSL
        }
    }
}
