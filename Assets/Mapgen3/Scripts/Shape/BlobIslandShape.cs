using UnityEngine;

namespace Marisa.Maps.Shapes
{
    [CreateAssetMenu(menuName = "Marisa/Map Shape/Blob Shape")]
    public class BlobIslandShape : IslandShape
    {
        public override bool IsPointInsideShape(Vector2 point, Vector2 mapSize, int seed)
        {
            base.IsPointInsideShape(point, mapSize, seed);
            Vector2 normalizedPosition = new Vector2()
            {
                x = ((point.x / mapSize.x) - 0.5f) * 2f,
                y = ((point.y / mapSize.y) - 0.5f) * 2f
            };

            bool eye1 = new Vector2(normalizedPosition.x - 0.2f, normalizedPosition.y / 2f + 0.2f).magnitude < 0.05f;
            bool eye2 = new Vector2(normalizedPosition.x + 0.2f, normalizedPosition.y / 2f + 0.2f).magnitude < 0.05f;
            bool body = point.magnitude < 0.8f - 0.18f * Mathf.Sin(5f * Mathf.Atan2(normalizedPosition.y, normalizedPosition.x));
            return body && !eye1 && !eye2;
        }
    }
}