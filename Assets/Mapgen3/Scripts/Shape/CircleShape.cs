using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Marisa.Maps.Shapes
{
    [CreateAssetMenu(menuName = "Marisa/Map Shape/Circle Shape")]
    public class CircleShape : IslandShape
    {
        [Range(0, 1)] public float size = 1f;

        public override bool IsPointInsideShape(Vector2 point, Vector2 mapSize, int seed = 0)
        {
            base.IsPointInsideShape(point, mapSize, seed);
            Vector2 normalizedPosition = new Vector2()
            {
                x = ((point.x / mapSize.x) - 0.5f) * 2f,
                y = ((point.y / mapSize.y) - 0.5f) * 2f
            };

            float value = Vector2.Distance(Vector2.zero, normalizedPosition);
            return value < size;
        }
    }
}