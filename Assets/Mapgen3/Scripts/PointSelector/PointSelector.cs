using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Marisa.Maps.PointSelectors
{
    [CreateAssetMenu(menuName = "Marisa/Point Selector/Random")]
    public class PointSelector  :ScriptableObject 
    {
       public virtual List<Vector2> Generator(int numPoints ,Vector2 mapSize,int seed)
        {
            Random.InitState(seed);
            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i < numPoints; i++)
            {
                var p = new Vector2(Random.Range(0f, mapSize.x ),
                                    Random.Range(0f, mapSize.y ));
                points.Add(p);
            }
            return points;
        }
    }
}