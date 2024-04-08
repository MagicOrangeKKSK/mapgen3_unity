using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Marisa.Maps.Shapes
{
    [CreateAssetMenu(menuName = "Marisa/Map Shape/Noise Shape")]
    public class NoiseShape : IslandShape
    {
        public int variation = 0;
        public float noiseSize = 1;
        [Range(1, 4)] public int octaves = 4;

        public override bool IsPointInsideShape(Vector2 point, Vector2 mapSize, int seed = 0)
        {
            base.IsPointInsideShape(point, mapSize, seed);

            float noiseSeed = Random.Range(0, 10000f) + variation * 10;

            Vector2 normalizedPosition = new Vector2()
            {
                x = ((point.x / mapSize.x) - 0.5f) * 2,
                y = ((point.y / mapSize.y) - 0.5f) * 2
            };

            float value = SamplePoint(normalizedPosition.x * noiseSize + noiseSeed,
                                      normalizedPosition.y * noiseSize + noiseSeed,
                                      octaves);

            // Perlin噪声函数不是随机的 因此我们需要添加一个“种子”来偏移值
            return value > (0.3f + 0.3f * normalizedPosition.magnitude * normalizedPosition.magnitude);

        }

        private static float SamplePoint(float x, float y, int octaves = 1, float persistence = 0.5f, float lacunarity = 2)
        {
            persistence = Mathf.Clamp01(persistence);
            float result = 0;
            float frequency = 1;
            float amplitude = 1;
            float sumOfAmplitudes = 0;

            for (int i = 0; i < octaves; i++)
            {
                result += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;

                sumOfAmplitudes += amplitude;
                frequency *= lacunarity;
                amplitude *= persistence;
            }
            return result / sumOfAmplitudes;
        }

    }
}