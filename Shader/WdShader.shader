Shader "Unlit/WdShader"
{
    Properties
    {
         _MainTex("Texture", 2D) = "white" {}
        [HDR]_SunColor("SunColor",Color) = (1,1,1,1)
        [HDR]_GroundColor("GroundColor",Color) = (0,0.19,0.85,1)
        [HDR]_LineColor("LineColor",Color) = (5.3,4.4,1.3,1)
        _OutLineRange("OutLineRange",Range(1,3)) = 2
        _OutLineWhite("OutLineWhite",Range(0,1)) = 1
        [HDR]_AColor("AColor",Color) = (0,0.1,0.2,1)


    }
        SubShader
        {
            Tags { "RenderType" = "Transparent" }
            LOD 100
            Cull off
        pass
        {
            Tags{ "LightMode" = "ShadowCaster" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_shadowcaster

            #include "UnityCG.cginc"

                struct appdata
            {
                  float2 uv : TEXCOORD0;
                float4 vertex : POSITION;
                half3 normal:NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                V2F_SHADOW_CASTER;
            };

            sampler2D _MainTex;

            v2f vert(appdata v)
            {
                v2f o;
                o.uv = v.uv;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            fixed4 frag(v2f i) : SV_TARGET
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                clip(col.a - 0.5);

                SHADOW_CASTER_FRAGMENT(i)
            }
                ENDCG
        }




            Pass
            {
                 Tags { "LightMode" = "ForwardBase" }
                CGPROGRAM

                #pragma multi_compile_fwdbase

                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fog

                #include "UnityCG.cginc"

                #include "AutoLight.cginc"
                #include "Lighting.cginc"

                struct appdata
                {
                float2 uv : TEXCOORD0;
                    float4 vertex : POSITION;
                    half3 normal:NORMAL;
                    float4 col:COLOR;
                };



                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    half3 worldNormal:TEXCOORD1;
                    float3 WordPos : TEXCOORD2;
                    float4 pos : SV_POSITION;
                    SHADOW_COORDS(4)
                };
                sampler2D _MainTex;
                float4 _SunColor;
                float4 _LineColor;
                float4 _GroundColor;
                float _OutLineRange;
                float   _OutLineWhite;
                float4 _AColor;
                v2f vert(appdata v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);

                    o.worldNormal = UnityObjectToWorldNormal(v.normal);
                    o.WordPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                    o.uv = v.uv;
                    TRANSFER_SHADOW(o);
                    UNITY_TRANSFER_FOG(o, o.pos);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed shadow = SHADOW_ATTENUATION(i);

                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                half Ramp_Lighting = dot(i.worldNormal,lightDir);



                fixed3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.WordPos);


                fixed3 reflectDir = saturate(normalize(reflect(-lightDir, i.worldNormal)));

                fixed 	Ramp_Specular = saturate(dot(reflectDir, viewDir));


                half Ramp_F = saturate(1 - dot(i.worldNormal, viewDir));



                //线性过程转阶梯
               // Ramp_Lighting = ceil(Ramp_Lighting * 5) / 5;

                fixed4 col = tex2D(_MainTex, i.uv);

                clip(col.a - 0.5);

                saturate(col.a - 0.5);
                col.rgb *= lerp(
                    _GroundColor.rgb,
                    _SunColor.rgb,saturate(Ramp_Lighting * shadow))
                    + pow(Ramp_Specular, _OutLineRange) * _LineColor.rgb * _OutLineWhite * shadow
                     + _AColor;
                return col;
            }
            ENDCG
        }
        }
}
