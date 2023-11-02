Shader "Unlit/CloudTesstShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Tags {"LightMode" = "ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

            #define MAX_STEP 30
            #define MIN_DIS 0.001

            struct appdata
            {
                //float4 vertex : POSITION;
                float4 objPos : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                //float4 vertex : SV_POSITION;
                float4 objPos : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            uniform float4 _sphere1;

            //uniform float4 _r;

            v2f vert (appdata v)
            {
                v2f o;
                //o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.objPos = UnityObjectToClipPos(v.objPos);
                return o;
            }

            float sphereSDF(float3 pos)
            {
                return length(pos) - 1;
            }

            /*float opSmoothUnion(float d1, float d2, float k)
            {
                float h = max(k - abs(d1 - d2), 0.0);
                return min(d1, d2) - h * h * 0.25 / k;
            }*/

            float3 getNormal(float3 p)
            {
                float3 x = float3 (0.001, 0.00, 0.00);
                float3 y = float3 (0.00, 0.001, 0.00);
                float3 z = float3 (0.00, 0.00, 0.001);
                return normalize(float3(sphereSDF(p + x) - sphereSDF(p - x), sphereSDF(p + y) - sphereSDF(p - y), sphereSDF(p + z) - sphereSDF(p - z)));
            }

            float ambient_occlusion(float3 pos, float3 nor)
            {
                float occ = 0.0;
                float sca = 1.0;
                for (int i = 0; i < 5; i++)
                {
                    float hr = 0.01 + 0.12 * float(i) / 4.0;
                    float3 aopos = nor * hr + pos;
                    float dd = sphereSDF(aopos);
                    occ += -(dd - hr) * sca;
                    sca *= 0.95;
                }
                return clamp(1.0 - 3.0 * occ, 0.0, 1.0);
            }

            float3 lighting(float3 ro, float3 ld, float3 rd, float3 pos)
            {
                float3 AmbientLight = float3 (0.1, 0.1, 0.1);
                float3 LightDirection = normalize(ld);
                float3 NormalDirection = getNormal(pos);
                float3 LightColor = _LightColor0.rgb;
                return (max(0.0, dot(ld, NormalDirection) * 0.6 + 0.15) * LightColor + AmbientLight) * ambient_occlusion(pos, NormalDirection);
            }

            float raymarch(in float3 ro, in float3 rd,in float3 ld, in float3 pos)
            {
                for (int i = 0; i < MAX_STEP; i++) 
                {
                    float ray = sphereSDF(ro);
                    if (distance(ro, ray * rd) > 250)
                        break;

                    if (ray < 100)
                        return float4 (lighting(ro, ld, rd, pos), 1.0);
                    else
                        ro += ray * rd;
                }
                return float4 (0.0, 0.0, 0.0, 0.0);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 射向目标
                float3 pos = i.objPos;
                // 出发点
                float3 ro = _WorldSpaceCameraPos;
                // 射向方向（归一化）
                float3 rd = normalize(pos - ro);
                float3 ld = normalize(UnityWorldSpaceLightDir(pos));

                //half4 col = CalBackColor(ro, rd);

                return raymarch(ro, rd, ld, pos);
            }
            ENDCG
        }
    }
}
