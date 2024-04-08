using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Marisa.Maps.Extension
{
    public class NoisyEdges 
    {
        public Dictionary<int, List<Vector2>> path = new Dictionary<int, List<Vector2>>();

        public void BuildNoisyEdges(Mapgen3 map)
        {
            foreach (var p in map.cells)
            {
                foreach (var edge in p.borderEdges)
                {
                    if (edge.d0 != null && edge.d1 != null &&
                       edge.v0 != null && edge.v1 != null && !path.ContainsKey(edge.index))
                    {
                        int num = 0;
                        if (edge.d0.biome != edge.d1.biome)
                            num = 2;
                        if (edge.d0.isOcean && edge.d1.isOcean)
                            num = 0;
                        if (edge.d0.isCoast || edge.d1.isCoast)
                            num = 3;
                        if (edge.waterVolume > 0)
                            num = 3;

                        path[edge.index] = NoisyEdge(edge.v0.position,edge.d0.position,edge.v1.position,edge.d1.position, num);
                    }
                }
            }
        }

        public List<Vector2> NoisyEdge(Vector2 a, Vector2 b, Vector2 c, Vector2 d, int num)
        {
            List<Vector2> points = new List<Vector2>();

            void Subdivide(Vector2 a, Vector2 b, Vector2 c, Vector2 d, int num)
            {
                if (num < 0)
                    return;
                if (Vector2.Distance(a, c) < 0.1f || Vector2.Distance(b, d) < 0.1f)
                    return;

                var n = Random.Range(0.4f, 0.6f);
                var m = Vector2.Lerp(b, d, n);

                Subdivide(a, Vector2.Lerp(a, b, 0.5f), m, Vector2.Lerp(a, d, 0.5f), num - 1);
                points.Add(m);
                Subdivide(m, Vector2.Lerp(b, c, 0.5f), c, Vector2.Lerp(c, d, 0.5f), num - 1);
            }

            points.Add(a);
            Subdivide(a, b, c, d, num - 1);
            points.Add(c);
            return points;
        }


      
    }
}