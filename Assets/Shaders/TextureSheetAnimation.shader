Shader "Custom/ParticlesAdditiveTextureSheet"
{
    Properties
    {
        _MainTex ("Texture Sheet", 2D) = "white" {}
        _Columns ("Columns (Колонки)", Float) = 4.0
        _Rows ("Rows (Строки)", Float) = 4.0
        _Speed ("Speed (Скорость/FPS)", Float) = 12.0
    }
    SubShader
    {
        // Настройки для Additive (аддитивного) смешивания
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        
        Blend SrcAlpha One          // Режим Additive (добавление цветов, черный = прозрачный)
        Cull Off                    // Видно с обеих сторон
        Lighting Off                // Отключаем свет для производительности
        ZWrite Off                  // Отключаем запись в Z-буфер, чтобы не было черных квадратов

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color  : COLOR;     // Принимает цвет и альфу из Particle System
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Columns;
            float _Rows;
            float _Speed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color; // Передаем цвет частиц дальше
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Расчет сетки кадров
                float totalCells = _Columns * _Rows;
                
                // Вычисляем текущий кадр на основе времени
                float index = floor(_Time.y * _Speed);
                index = fmod(index, totalCells); // Зацикливаем анимацию

                // Рассчитываем координаты текущего кадра на текстуре
                float currentColumn = fmod(index, _Columns);
                // Инвертируем Y, так как UV в Unity идут снизу вверх, а анимация в листах обычно сверху вниз
                float currentRow = _Rows - 1.0 - floor(index / _Columns);

                // Размер одного кадра
                float2 size = float2(1.0 / _Columns, 1.0 / _Rows);
                
                // Сдвиг и масштабирование UV
                float2 offset = float2(currentColumn * size.x, currentRow * size.y);
                float2 animatedUV = i.uv * size + offset;

                // Сэмплируем текстуру по новым анимированным UV
                fixed4 col = tex2D(_MainTex, animatedUV);
                
                // Умножаем на цвет частицы (это активирует Color over Lifetime и прозрачность)
                col *= i.color;
                
                return col;
            }
            ENDCG
        }
    }
}
