using Marisa.Maps.Graph;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Marisa.Maps.Extension
{
    public class Roads 
    {
        //�ȸ����ֵ�
        //edge index -> int contour level
        public Dictionary<int, int> road = new Dictionary<int, int>();
        
        //center index -> array of edges with road 
        public Dictionary<int, List<CellEdge>> roadConnections = new Dictionary<int, List<CellEdge>>();

        public void CreateRoads(Mapgen3 map)
        {
            //��ǲ�ͬ�������� �Ա����������Ƶ���ĵ�· ������ָ���
            //����ͺ�������͵ĵȸ������� 
            //  ��1����ȸ���ˮƽK���ӵ��κ����ݣ�
            //           ��������ڵȸ�����ֵK��
            //           �����������ˮ��
            //           �͵õ��ȸ���ˮƽK��
            //  ��2���κ�δ����ȸ���ˮƽ�����ݣ�
            //           ��ȸ���ˮƽK���ӣ�
            //           �õ��ȸ���ˮƽK+1

            Queue<CellCenter> queue = new Queue<CellCenter>(); ;
            
            //����Ѻ��μ򵥻��ּ�������
            List<float> elevationThresholds = new List<float>()
            {
                0,0.05f,0.37f,0.64f
            };

            //corner index -> int contour level
            Dictionary<int,int> cornerContour = new Dictionary<int, int>();
            //center index -> int contour level
            Dictionary<int, int> centerContour = new Dictionary<int, int>();


            foreach (var p in map.cells)
            {
                if(p.isCoast || p.isOcean)
                {
                    centerContour[p.index] = 1;
                    queue.Enqueue(p);
                }
            }

            while(queue.Count > 0)
            {
                var p = queue.Dequeue();
                foreach (var r in p.neighborCells)
                {
                    var newLevel = GetDictionaryValue(centerContour, p.index, 0);
                    while(newLevel < elevationThresholds.Count &&
                        r.elevation > elevationThresholds[newLevel] && !r.isWater)
                    {
                        newLevel += 1;
                    }


                    if(newLevel < GetDictionaryValue(centerContour,r.index,999))
                    {
                        centerContour[r.index] = newLevel;
                        queue.Enqueue(r);
                    }
                }
            }

            foreach (var p in map.cells)
            {
                foreach (var q in p.cellCorners)
                {
                    cornerContour[q.index] = Mathf.Min(GetDictionaryValue(cornerContour, q.index, 999),
                                                                                GetDictionaryValue(centerContour, p.index, 999));
                }
            }

            foreach (var p in map.cells)
            {
                foreach (var edge in p.borderEdges)
                {
                    if (edge.v0 != null && edge.v1 != null &&
                        cornerContour[edge.v0.index] != cornerContour[edge.v1.index])
                    {
                        road[edge.index] = Mathf.Min(cornerContour[edge.v0.index],
                                                                         cornerContour[edge.v1.index]);
                        if (!roadConnections.ContainsKey(p.index))
                            roadConnections[p.index] = new List<CellEdge>();
                        roadConnections[p.index].Add(edge);
                    }
                }
            }
        }

        private T2 GetDictionaryValue<T1,T2>(Dictionary<T1,T2> dict,T1 index,T2 defaultValue)
        {
            if (dict.ContainsKey(index))
                return dict[index];
            return defaultValue;
        }
    }
}