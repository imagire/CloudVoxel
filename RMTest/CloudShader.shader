Shader "Unlit/CloudShader"
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
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                //float3 worldNormal : TEXCOORD1;
                float4 vertex : SV_POSITION;
                float4 pos_world : TEXCOORD1;
                float3 normal:TEXCOORD2;
                SHADOW_COORDS(3)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _StepCount;
            float _StepSize;

            float3 _SpherePos;
            float3 _SpherePos1;

            float _D2;
            float _K;

            sampler2D _CameraDepthTexture;

            #define MIN_DIST_TO_SDF 0.001
            #define MAX_DIST_TO_SDF 64
            #define EPSILON 0.0001

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = v.normal;
                //o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.pos_world = mul(UNITY_MATRIX_M, v.vertex);
                TRANSFER_SHADOW(o);
                return o;
            }

            float sdSphere(float3 position, float3 center, float radius)
            {
                return distance(position, center) - radius;
            }

            float SmoothUnion(float d1, float d2, float k)
            {
                float h = max(k - abs(d1 - d2), 0.0);
                return min(d1, d2) - h * h * 0.25 / k;
            }

            float4 DistanceField(float3 p) {
                return sdSphere(p,(0, 0, 0), 3);
            }

            float sdScene(float3 p1,float3 p2) {
                p1 /= 0.8;// 全场景uniform scale为0.8倍
                p2 /= 0.8;
                //p = mul(rotateY(_Time[1] * 0.5), p);// 全场景绕Y轴旋转
                float d1 = sdSphere(p1, float3(0.0, 0.0, 0.0), 0.5);
                float d2 = sdSphere(p2, float3(0.0, 0.0, 0.0), 0.5);
                return SmoothUnion(d1, d2, _K) * 0.8;// 取平滑交集,*0.8表示第一步uniform scale的补偿
            }

            fixed3 estimateNormal(float3 p1, float3 p2) {
                return normalize(float3(
                    sdScene(float3(p1.x + EPSILON, p1.y, p1.z), float3(p2.x + EPSILON, p2.y, p2.z)) - sdScene(float3(p1.x - EPSILON, p1.y, p1.z), float3(p2.x - EPSILON, p2.y, p2.z)),
                    sdScene(float3(p1.x, p1.y + EPSILON, p1.z), float3(p2.x, p2.y + EPSILON, p2.z)) - sdScene(float3(p1.x, p1.y - EPSILON, p1.z),float3(p2.x, p2.y - EPSILON, p2.z)),
                    sdScene(float3(p1.x, p1.y, p1.z + EPSILON), float3(p2.x, p2.y, p2.z + EPSILON)) - sdScene(float3(p1.x, p1.y, p1.z - EPSILON), float3(p2.x, p2.y, p2.z - EPSILON))
                    ));
            }

            float Map(float3 position)
            {
                float radius = 0.5;
                float3 center = float3(0.0, 0.0, 0.0);

                float sphere = sdSphere(position, center, radius);

                return sphere;
            }

            float3 GetNormal(float3 position)
            {
                float2 d = float2(0.01, 0);
                float gx = Map(position + d.xyy) - Map(position - d.xyy);
                float gy = Map(position + d.yxy) - Map(position - d.yxy);
                float gz = Map(position + d.yyx) - Map(position - d.yyx);
                float3 normal = float3(gx, gy, gz);
                return normalize(normal);
            }

            float raymarching(float3 position, float3 direction, float MaxDist)
            {
                float3 startPoint = position;
                float dist = 0.02;
                for (int i = 0; i < _StepCount; i++)
                {
                    if (dist < MIN_DIST_TO_SDF)
                    {
                        break;
                    }
                    else if (dist > MaxDist)
                    {
                        break;
                    }
                    else
                    {
                        startPoint += direction * _StepSize;
                        if (_SpherePos.x - startPoint.x > -1 && _SpherePos.x - startPoint.x < 1 
                            && _SpherePos.y - startPoint.y > -1 && _SpherePos.y - startPoint.y < 1 
                            && _SpherePos.z - startPoint.z > -1 && _SpherePos.z - startPoint.z < 1)
                        {
                            dist += 0.02;
                        }
                    }
                }
                return dist;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                //float depth = tex2D(_CameraDepthTexture, i.uv);

                // 相机为射线起点
                float3 rayStart = _WorldSpaceCameraPos;
                // 物体的世界坐标 - 相机的坐标为射线的方向
                float3 direction = normalize(_SpherePos - rayStart);
                // 遍历场景
                float dist = raymarching(rayStart, direction, MAX_DIST_TO_SDF);

                float4 color = float4(1.0, 1.0, 1.0, 1.0);

                /*if (dist < MAX_DIST_TO_SDF)
                {
                    float3 position = rayStart + direction * dist;
                    float3 normal = GetNormal(position);
                    //color = float4(normal, 1.0);
                    //float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
                    
                }*/

                //color += depth;

                float4 lightColor = _LightColor0;
                float3 lightDir = WorldSpaceLightDir(i.pos_world);
                UNITY_LIGHT_ATTENUATION(atten, i, i.pos_world.xyz);

                float p1 = rayStart + dist * direction;
                float p2 = rayStart + dist * direction;

                float3 normal = i.normal + estimateNormal(p1,p2);

                //color = SmoothUnion(sphere, _D2, 0.5);

                return color * lightColor * saturate(dot(lightDir, normal)) * atten + dist;
            }
            ENDCG
        }
        pass
        {
            Tags{ "LightMode" = "ForwardAdd" }
                Blend One One
                CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fwdadd_fullshadows

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
                struct v2f
            {
                float4 pos : POSITION;
                float4 vertex : TEXCOORD0;
                float3 normal : NORMAL;
                SHADOW_COORDS(2)
            };

            v2f vert(appdata_full data)
            {
                v2f v;
                v.pos = UnityObjectToClipPos(data.vertex);
                v.vertex = mul(UNITY_MATRIX_M, data.vertex);
                v.normal = data.normal;
                TRANSFER_SHADOW(v);
                return v;
            }

            float4 frag(v2f v) :SV_Target
            {
                float3 lightColor = _LightColor0;
                #ifdef USING_DIRECTIONAL_LIGHT
                float3 lightDir = _WorldSpaceLightPos0;
                #else
                float3 lightDir = _WorldSpaceLightPos0 - v.vertex;
                #endif
                UNITY_LIGHT_ATTENUATION(atten, v, v.vertex.xyz);
                float3 color = lightColor * saturate(dot(lightDir, v.normal) * atten);
                return float4(color, 1);
            }
            ENDCG
        }

    }
    Fallback "Specular"
}
