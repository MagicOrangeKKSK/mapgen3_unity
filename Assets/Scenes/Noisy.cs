using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noisy : MonoBehaviour
{
    public Transform v0;
    public Transform v1;
    public Transform d0;
    public Transform d1;

    [Range(1,6)]
    public int num = 2;

    public int seed = 10000;

    public void Update()
    {
        Debug.DrawLine(v0.position,d0.position);
        Debug.DrawLine(v1.position,d0.position);
        Debug.DrawLine(v0.position,d1.position);
        Debug.DrawLine(v1.position,d1.position);

        Random.InitState(seed);

        var point = NoisyEdge(v0.position, d0.position, v1.position, d1.position,num);
        for (int i = 0; i < point.Count - 1; i++)
        {
            Debug.DrawLine(point[i], point[i + 1],Color.red);
        }
    }


    public List<Vector3> NoisyEdge(Vector3 a, Vector3 b, Vector3 c, Vector3 d, int num)
    {
        List<Vector3> points = new List<Vector3>();

        void Subdivide(Vector3 a, Vector3 b, Vector3 c, Vector3 d, int num)
        {
            if (num < 0)
                return;

            var n = Random.Range(0.2f, 0.8f);
            var m = Vector3.Lerp(b, d, n);

            Subdivide(a, Vector3.Lerp(a, b, 0.5f), m, Vector3.Lerp(a, d, 0.5f), num - 1);
            points.Add(m);
            Subdivide(m, Vector3.Lerp(b, c, 0.5f), c, Vector3.Lerp(c, d, 0.5f), num - 1);
        }

        points.Add(a);
        Subdivide(a, b, c, d, num - 1);
        points.Add(c);
        return points;
    }

}
