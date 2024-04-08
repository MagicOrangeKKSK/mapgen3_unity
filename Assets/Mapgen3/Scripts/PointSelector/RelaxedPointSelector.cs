using csDelaunay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Marisa.Maps.PointSelectors
{
    [CreateAssetMenu(menuName = "Marisa/Point Selector/Relaxed")]
    public class RelaxedPointSelector : PointSelector
    {
        [Range(1,100)]
        public int NUM_LLOYD_RELAXATIONS = 2;

        //这里使用Lloyd Relaxation算法
        public override List<Vector2> Generator(int numPoints, Vector2 mapSize, int seed)
        {
            Random.InitState(seed);

            List<Vector2> points = base.Generator(numPoints, mapSize, seed);
            Rectf bounds = new Rectf(0, 0, mapSize.x, mapSize.y);
            Voronoi voronoi;
            for (int i = 0; i < NUM_LLOYD_RELAXATIONS; i++)
            {
                voronoi = new Voronoi(points,bounds);
                for (int j = 0; j < points.Count; j++)
                {
                    var p = points[j];
                    var region = voronoi.Region(p);
                    p.x = 0;
                    p.y = 0;
                    foreach (var q in region)
                    {
                        p.x += q.x;
                        p.y += q.y;
                    }
                    p.x /= region.Count;
                    p.y /= region.Count;

                    points[j] = p;
                }
            }
            return points;
        }
    }
}