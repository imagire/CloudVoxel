Shader "Unlit/Testofcloud"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NumStep ("NumStep", int) = 0
        _StepSize ("StepSize", float) = 0.0
        _DensityScale ("DensityScale", float) = 0.0
        _Sphere ("Sphere", Vector) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType"="TransparentCutout" "IgnoreProjector" = "True"}
        LOD 100

        Pass
        {
            Tags { "LightMode" = "ForwardBase" }

            ZWrite on
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos:TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            int _NumStep;
            float _StepSize;
            float _DensityScale;
            float4 _Sphere;

            v2f vert (appdata v)
            {
                v2f o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float distanceField(float3 ro, float3 pos)
            {
                float sphere = length(ro - pos);
                return sphere;
            }

            float raymarching(float3 ro, float3 rd, int numStep, float stepSize, float densityScale, float4 sphere)
            {
                float result = 0;

                float density = 0;

                for (int i = 0; i < stepSize; i++)
                {
                    ro += rd * stepSize;

                    float sphereDist = distanceField(ro, sphere.xyz);

                    if (sphereDist < sphere.w)
                    {
                        density += 0.1;
                    }
                }

                result = density * densityScale;

                return result;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed3 ro = i.worldPos;
                fixed3 camPos = _WorldSpaceCameraPos;
                fixed3 rd = normalize(mul(ro - camPos,(float3x3)unity_WorldToObject));
                float result = raymarching(ro, rd, _NumStep, _StepSize, _DensityScale, _Sphere);
                fixed4 col = fixed4(1.0, 1.0, 1.0, result);
                
                return col;
            }
            ENDCG
        }
    }
}
