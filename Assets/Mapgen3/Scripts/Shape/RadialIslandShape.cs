using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Marisa.Maps.Shapes
{

    [CreateAssetMenu(menuName = "Marisa/Map Shape/Radial Shape")]
    public class RadialIslandShape : IslandShape
    {
        [Range(1,6)]
        public int bumps;
        [Range(0,2 * Mathf.PI)]
        public float startAngle;
        [Range(0,2*Mathf.PI)]
        public float dipAngle;

        [Range(0.2f,0.7f)]
        public float dipWidth;

        public float ISLAND_FACTOR = 1.07f;


        public override bool IsPointInsideShape(Vector2 point, Vector2 mapSize, int seed)
        {
            Random.InitState(seed);
            Vector2 q = new Vector2()
            {
                x = ((point.x / mapSize.x) - 0.5f) * 2f,
                y = ((point.y / mapSize.y) - 0.5f) * 2f
            };

            float angle = Mathf.Atan2(q.y, q.x);
            float length = 0.5f * (Mathf.Max(Mathf.Abs(q.x), Mathf.Abs(q.y)) + q.magnitude);

            float r1 = 0.5f + 0.4f * Mathf.Sin(startAngle + bumps * angle + Mathf.Cos((bumps + 3) * angle));
            float r2 = 0.7f - 0.2f * Mathf.Sin(startAngle + bumps * angle - Mathf.Sin((bumps + 2) * angle));
            if (Mathf.Abs(angle - dipAngle) < dipWidth ||
               Mathf.Abs(angle - dipAngle + 2 * Mathf.PI) < dipWidth ||
               Mathf.Abs(angle - dipAngle - 2 * Mathf.PI) < dipWidth)
            {
                r1 = 0.2f;
                r2 = 0.2f;
            }
            return (length < r1 || (length > r1 * ISLAND_FACTOR && length < r2));
        }
    }
}
