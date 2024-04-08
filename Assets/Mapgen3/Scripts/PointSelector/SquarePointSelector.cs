using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Marisa.Maps.PointSelectors
{
    [CreateAssetMenu(menuName = "Marisa/Point Selector/Square")]
    public class SquarePointSelector : PointSelector
    {
        public override List<Vector2> Generator(int numPoints, Vector2 mapSize, int seed)
        {
            Random.InitState(seed);
            var points = new List<Vector2>();
            int n = (int)Mathf.Sqrt(numPoints);
            for (int x = 0; x < n; x++)
            {
                for (int y = 0; y < n; y++)
                {
                    points.Add(new Vector2((0.5f + x) / n * mapSize.x, (0.5f + y) / n * mapSize.y));
                }
            }
            return points;
        }
    }
}
