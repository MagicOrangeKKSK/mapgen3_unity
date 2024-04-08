using Marisa.Maps.Graph;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Marisa.Maps.Extension
{
    public class Roads 
    {
        //等高线字典
        //edge index -> int contour level
        public Dictionary<int, int> road = new Dictionary<int, int>();
        
        //center index -> array of edges with road 
        public Dictionary<int, List<CellEdge>> roadConnections = new Dictionary<int, List<CellEdge>>();

        public void CreateRoads(Mapgen3 map)
        {
            //标记不同海拔区域 以便于制作环绕岛屿的道路 将区域分隔开
            //海洋和海岸是最低的等高线区域 
            //  （1）与等高线水平K连接的任何内容，
            //           如果它低于等高线阈值K，
            //           或者如果它是水，
            //           就得到等高线水平K。
            //  （2）任何未分配等高线水平的内容，
            //           与等高线水平K连接，
            //           得到等高线水平K+1

            Queue<CellCenter> queue = new Queue<CellCenter>(); ;
            
            //这里把海拔简单划分几个区域
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