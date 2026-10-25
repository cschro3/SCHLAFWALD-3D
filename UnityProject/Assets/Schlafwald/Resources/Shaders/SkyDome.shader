// SCHLAFWALD – Himmelskuppel: Farbverlauf (Sonnenuntergang → Nacht), weiche Sonne, Mond mit Hof, prozedurale Sterne.
// Sterne/Mond/Sonne liegen im Shader, damit kein Texturspeicher nötig ist.
Shader "Schlafwald/SkyDome"
{
    Properties
    {
        _Horizon ("Horizont", Color) = (1,0.62,0.42,1)
        _Zenith ("Zenit", Color) = (0.28,0.30,0.55,1)
        _SunColor ("Sonnenfarbe", Color) = (1,0.75,0.5,1)
        _SunDir ("Sonnenrichtung", Vector) = (0,0.1,1,0)
        _SunAmount ("Sonne sichtbar", Range(0,1)) = 1
        _MoonDir ("Mondrichtung", Vector) = (0.3,0.5,0.8,0)
        _MoonAmount ("Mond sichtbar", Range(0,1)) = 0
        _Stars ("Sterne sichtbar", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Background" "RenderPipeline"="UniversalPipeline" "Queue"="Background" "PreviewType"="Skybox" }
        Pass
        {
            Name "Sky"
            Tags { "LightMode"="UniversalForward" }
            ZWrite Off
            Cull Front
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _Horizon; half4 _Zenith; half4 _SunColor;
                float4 _SunDir; half _SunAmount;
                float4 _MoonDir; half _MoonAmount; half _Stars;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 dir : TEXCOORD0; };
            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.dir = v.positionOS.xyz;
                return o;
            }
            float hash31(float3 p)
            {
                p = frac(p * 0.1031);
                p += dot(p, p.zyx + 31.32);
                return frac((p.x + p.y) * p.z);
            }
            float3 hash33(float3 p)
            {
                return float3(hash31(p), hash31(p + 17.13), hash31(p + 41.7));
            }
            float vnoise(float3 p)
            {
                float3 i = floor(p); float3 f = frac(p); f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(lerp(hash31(i), hash31(i+float3(1,0,0)), f.x), lerp(hash31(i+float3(0,1,0)), hash31(i+float3(1,1,0)), f.x), f.y),
                            lerp(lerp(hash31(i+float3(0,0,1)), hash31(i+float3(1,0,1)), f.x), lerp(hash31(i+float3(0,1,1)), hash31(i+float3(1,1,1)), f.x), f.y), f.z);
            }
            half4 frag(Varyings i) : SV_Target
            {
                float3 d = normalize(i.dir);
                float up = saturate(d.y);
                half3 col = lerp(_Horizon.rgb, _Zenith.rgb, pow(up, 0.55));
                if (d.y < 0) col = _Horizon.rgb * (1.0 + d.y * 0.8);

                // Sonne: weicher Hof + Scheibe
                float sd = saturate(dot(d, normalize(_SunDir.xyz)));
                half sun = pow(sd, 6.0) * 0.45 + pow(sd, 90.0) * 0.6 + smoothstep(0.9985, 0.9995, sd);
                col += _SunColor.rgb * sun * _SunAmount;

                // Sterne
                float3 p = d * 60.0;
                float3 id = floor(p);
                float3 h = hash33(id);
                float3 f = frac(p) - 0.5 - (h - 0.5) * 0.55;
                float star = smoothstep(0.09, 0.0, length(f)) * step(0.92, h.x);
                star *= 0.6 + 0.4 * sin(_Time.y * (0.6 + h.y * 1.6) + h.z * 20.0);
                col += star * _Stars * saturate(d.y * 3.0) * half3(1.0, 0.97, 0.88);

                // Mond: weiche Scheibe mit leichter Maserung + Hof
                float md = saturate(dot(d, normalize(_MoonDir.xyz)));
                float disc = smoothstep(0.9965, 0.9972, md);
                float mottle = 0.88 + 0.12 * vnoise(d * 22.0);
                half3 moonCol = half3(1.0, 0.96, 0.82) * mottle;
                col = lerp(col, moonCol, disc * _MoonAmount);
                col += half3(0.75, 0.82, 1.0) * (pow(md, 60.0) * 0.30 + pow(md, 7.0) * 0.07) * _MoonAmount;
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
