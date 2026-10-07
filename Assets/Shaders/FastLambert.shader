Shader "Custom/FastLambert"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            Name "FORWARD"
            Tags { "LightMode"="ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // Включаем поддержку GPU Instancing
            #pragma multi_compile_instancing
            // Игнорируем ненужные варианты тумана/света для ускорения компиляции и выполнения
            #pragma skip_variants FOG_EXP FOG_EXP2

            #include "UnityCG.cginc"
            #include "Lighting.cginc"  // <-- Добавьте эту строку (содержит _LightColor0)
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                // Запекаем расчет освещения прямо в вершины (Per-Vertex Lighting)
                fixed3 vLight : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                // Нормаль в мировых координатах
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);

                // Расчет Lambert (Dot-product) на вершинном этапе
                fixed NdotL = max(0.0, dot(worldNormal, _WorldSpaceLightPos0.xyz));

                // Основной Directional Light + Ambient (окружающий свет)
                fixed3 ambient = ShadeSH9(half4(worldNormal, 1.0));
                fixed3 diffLight = _LightColor0.rgb * NdotL;

                o.vLight = ambient + diffLight;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                fixed4 texColor = tex2D(_MainTex, i.uv);
                fixed4 colorRes = texColor * UNITY_ACCESS_INSTANCED_PROP(Props, _Color);

                // Итоговый цвет: Текстура * Цвет * Предрассчитанный свет
                colorRes.rgb *= i.vLight;

                return colorRes;
            }
            ENDCG
        }
    }

    FallBack "VertexLit"
}