using Marisa.Maps.Graph;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Vector2;

namespace Marisa.Maps.Tools
{
    public static class MathfTool
    {
        public static float Cross(Vector2 a , Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        public static bool IsPointInTriangle(Vector2 P,Vector2 A,Vector2 B,Vector2 C)
        {
            Vector2 AP = P - A;
            Vector2 AB = B - A;
            float z1 = Cross(AB, AP);

            Vector2 BP = P - B;
            Vector2 BC = C - B;
            float z2 = Cross(BC, BP);

            Vector2 CP = P - C;
            Vector2 CA = A - C;
            float z3 = Cross(CA, CP);

            if (z1 > 0 && z2 > 0 && z3 > 0)
                return true;
            if (z1 < 0 && z2 < 0 && z3 < 0)
                return true;
            return false;
        }

        public static bool IsPointInPolygon(Vector2 point, List<Vector2> vertices)
        {
            bool isInside = false;

            Vector2 last = vertices[vertices.Count - 1];
            foreach (var vertice in vertices)
            {
                if (((vertice.y < point.y && last.y >= point.y) ||
                    (last.y < point.y && vertice.y >= point.y)) &&
                    (vertice.x <= point.x || last.x <= point.x))
                {
                    if (vertice.x + (point.y - vertice.y) / (last.y - vertice.y) * (last.x - vertice.x) < point.x)
                    {
                        isInside = !isInside;
                    }
                }
                last = vertice;
            }
            return isInside;
        }

        public static Vector2 GetCenter(List<Vector2> list)
        {
            Vector2 center = new Vector2();
            for (int j = 0; j < list.Count; j++)
                center += list[j];
            center /= list.Count;
            return center;
        }
    


    }

}