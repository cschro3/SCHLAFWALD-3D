// SCHLAFWALD – weiches, kindgerechtes Licht: Wrap-Diffuse ohne harte Schatten, dezenter Fell-/Materialeffekt
// (prozedurales Rauschen), Rim-Licht für kuschelige Silhouetten, Wind-Schwingen über Vertex-Alpha.
Shader "Schlafwald/StylizedLit"
{
    Properties
    {
        _BaseColor ("Farbe", Color) = (1,1,1,1)
        _ShadeColor ("Schattenfarbe (weich)", Color) = (0.45,0.40,0.58,1)
        _RimColor ("Rim-Farbe", Color) = (1,0.92,0.78,1)
        _RimPower ("Rim-Schärfe", Range(0.5,8)) = 3
        _RimStrength ("Rim-Stärke", Range(0,1)) = 0.3
        _Wrap ("Weiche Beleuchtung", Range(0,1)) = 0.55
        _FurAmount ("Fell/Material-Rauschen", Range(0,1)) = 0
        _FurScale ("Rauschen-Skalierung", Float) = 45
        _SwayAmount ("Wind-Stärke", Float) = 0
        _Emission ("Leuchten", Color) = (0,0,0,1)
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            Cull [_Cull]
            ZWrite On
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _ShadeColor;
                half4 _RimColor;
                half4 _Emission;
                half _RimPower;
                half _RimStrength;
                half _Wrap;
                half _FurAmount;
                half _FurScale;
                half _SwayAmount;
                half _Cull;
            CBUFFER_END
            float _SleepMotion; // global: 1 = lebhafter Abend, klein = ruhige Nacht (SleepForestController)

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; half4 color : COLOR; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                half4 color : COLOR;
                half fogFactor : TEXCOORD2;
            };

            float hash13(float3 p)
            {
                p = frac(p * 0.1031);
                p += dot(p, p.zyx + 31.32);
                return frac((p.x + p.y) * p.z);
            }
            float vnoise(float3 p)
            {
                float3 i = floor(p); float3 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float n000 = hash13(i);                 float n100 = hash13(i + float3(1,0,0));
                float n010 = hash13(i + float3(0,1,0)); float n110 = hash13(i + float3(1,1,0));
                float n001 = hash13(i + float3(0,0,1)); float n101 = hash13(i + float3(1,0,1));
                float n011 = hash13(i + float3(0,1,1)); float n111 = hash13(i + float3(1,1,1));
                return lerp(lerp(lerp(n000,n100,f.x), lerp(n010,n110,f.x), f.y),
                            lerp(lerp(n001,n101,f.x), lerp(n011,n111,f.x), f.y), f.z);
            }

            Varyings vert(Attributes v)
            {
                Varyings o;
                float3 pWS = TransformObjectToWorld(v.positionOS.xyz);
                float sway = v.color.a * _SwayAmount * (0.2 + 0.8 * _SleepMotion);
                pWS.x += sin(_Time.y * 1.3 + pWS.x * 0.8 + pWS.z * 0.6) * sway;
                pWS.z += cos(_Time.y * 1.1 + pWS.z * 0.7 + pWS.x * 0.5) * sway * 0.7;
                o.positionWS = pWS;
                o.positionCS = TransformWorldToHClip(pWS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.color = v.color;
                o.fogFactor = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            half4 frag(Varyings i, bool facing : SV_IsFrontFace) : SV_Target
            {
                half3 N = normalize(i.normalWS);
                if (!facing) N = -N;
                Light mainLight = GetMainLight();
                half ndl = saturate((dot(N, mainLight.direction) + _Wrap) / (1.0 + _Wrap));
                half3 vcol = i.color.rgb;
                #ifndef UNITY_COLORSPACE_GAMMA
                vcol = SRGBToLinear(vcol); // Vertexfarben sind in sRGB gemalt
                #endif
                half3 albedo = _BaseColor.rgb * vcol;
                half fur = (half)vnoise(i.positionWS * _FurScale);
                albedo *= 1.0 + (fur - 0.5) * _FurAmount * 0.7;
                half3 lightTint = lerp(_ShadeColor.rgb, half3(1,1,1), ndl);
                half3 diffuse = albedo * (lightTint * mainLight.color + SampleSH(N) * 0.9);
                half3 V = normalize(GetWorldSpaceViewDir(i.positionWS));
                half rim = pow(1.0 - saturate(dot(N, V)), _RimPower);
                half3 rimCol = _RimColor.rgb * rim * _RimStrength * (0.35 + 0.65 * ndl) * (1.0 + fur * _FurAmount);
                half3 col = diffuse + rimCol * mainLight.color + _Emission.rgb;
                col = MixFog(col, i.fogFactor);
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
