using csDelaunay;
using Marisa.Maps.Enums;
using Marisa.Maps.Extension;
using Marisa.Maps.Graph;
using Marisa.Maps.PointSelectors;
using Marisa.Maps.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Color = UnityEngine.Color;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace Marisa.Maps
{

    public class Mapgen2 : MonoBehaviour
    {
        public bool useCustomSeed;

        [Header("Map")]
        public int seed;
        public int polygonCount = 200;
        public Vector2 size;
        public int relaxation = 0;
        public Shape islandShape;
        public AnimationCurve elevationCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public IslandShape shape;
        public PointSelector pointSelector;
        public bool singleIsland;
        public int minIslandSize;

        [Header("Rivers")]
        public int springsSeed;
        public int numberOfSprings = 5;
        [Range(0, 1)] public float minSpringElevation = 0.3f;
        [Range(0, 1)] public float maxSpringElevation = 0.9f;

        public List<CellCenter> cells = new List<CellCenter>();
        public List<CellCorner> corners = new List<CellCorner>();
        public List<CellEdge> edges = new List<CellEdge>();
        public List<List<CellCenter>> islands = new List<List<CellCenter>>();

        public event Action onMapGenerated;

        private const float LAKE_THRESHOLD = 0.3f;

        public List<Vector2> points;
        public NoisyEdges noisyEdges;

        [ContextMenu("Generate")]
        public void Generate()
        {
            ShapingMap();
            PlacePoints();
            BuildingGraph3();
            Features();
            //DrawTriangle();
           // Edges();

            onMapGenerated?.Invoke();
        }

        private void Features()
        {
            Debug.Log("*----Features...----*");
            AssignElevations();
            //AssignMoisture();
            //AssignBiomes();
        }

        private void Edges()
        {
            Debug.Log("*----Edges...----*");
            //Roads 
            //watershaeds
            noisyEdges = new NoisyEdges();
            //noisyEdges.BuildNoisyEdges(this, seed);
        }

 

        private void ShapingMap()
        {
            Debug.Log("*---Shaping Map... ---*");
            ResetMapInfo();
        }

        private List<Vector2> PlacePoints()
        {
            Debug.Log("*---Place points...---*");
            points = pointSelector.Generator(polygonCount, size, seed);
            return points;
        }


         

        private void BuildingGraph3()
        {
            Rectf bounds = new Rectf(0, 0, size.x, size.y);
            Voronoi voronoi = new Voronoi(points, bounds, relaxation);

            Dictionary<string, CellCenter> lookupCenter = new Dictionary<string, CellCenter>();
           
            string ToKey(Vector2 point)
            {
                return (int)(point.x * 10000) + "," + (int)(point.y * 10000);
            }

            foreach (var site in voronoi.SitesIndexedByLocation)
            {
                string key = ToKey(site.Key);
                if (!lookupCenter.ContainsKey(key))
                {
                    CellCenter c = new CellCenter();
                    c.index = cells.Count;
                    c.position = site.Key;
                    cells.Add(c);
                    lookupCenter.Add(key, c);
                }
            }
            Dictionary<string, CellCorner> lookupCorner = new Dictionary<string, CellCorner>();
            void makeCorner( Vector2 point)
            {
                string key = ToKey(point);
                if (!lookupCorner.ContainsKey(key))
                {
                    CellCorner c = new CellCorner();
                    c.index = corners.Count;
                    c.position = point;
                    c.isBorder = c.position.x == 0 ||
                             c.position.x == size.x ||
                             c.position.y == 0 ||
                             c.position.y == size.y;
                    corners.Add(c);
                    lookupCorner.Add(key, c);
                }
            }

            void addPointToPointList<T>(List<T> list, T point) where T : MapPoint
            {
                if (!list.Contains(point))
                    list.Add(point);
            }

            foreach (var edge in voronoi.Edges)
            {
                if (edge.ClippedEnds == null)
                    continue;

                makeCorner(edge.ClippedEnds[LR.LEFT]);
                makeCorner(edge.ClippedEnds[LR.RIGHT]);
            }


            foreach (var voronoiEdge in voronoi.Edges)
            {
                if (voronoiEdge.ClippedEnds == null)
                    continue;

                CellEdge edge = new CellEdge();
                edge.index = edges.Count;
                edges.Add(edge);

                edge.v0 = lookupCorner[ToKey(voronoiEdge.ClippedEnds[LR.LEFT])];
                edge.v1 = lookupCorner[ToKey(voronoiEdge.ClippedEnds[LR.RIGHT])];

                edge.d0 = lookupCenter[ToKey( voronoiEdge.LeftSite.Coord)];
                edge.d1 = lookupCenter[ToKey(voronoiEdge.RightSite.Coord)];

                edge.d0.borderEdges.Add(edge);
                edge.d1.borderEdges.Add(edge);
                edge.v0.connectedEdges.Add(edge);
                edge.v1.connectedEdges.Add(edge);

                addPointToPointList(edge.d0.neighborCells, edge.d1);
                addPointToPointList(edge.d1.neighborCells, edge.d0);

                addPointToPointList(edge.v0.neighborCorners, edge.v1);
                addPointToPointList(edge.v1.neighborCorners, edge.v0);

                addPointToPointList(edge.d0.cellCorners, edge.v0);
                addPointToPointList(edge.d0.cellCorners, edge.v1);

                addPointToPointList(edge.d1.cellCorners, edge.v0);
                addPointToPointList(edge.d1.cellCorners, edge.v1);

                addPointToPointList(edge.v0.touchingCells, edge.d0);
                addPointToPointList(edge.v0.touchingCells, edge.d1);
                addPointToPointList(edge.v1.touchingCells, edge.d0);
                addPointToPointList(edge.v1.touchingCells, edge.d1);

                if (edge.v0 != null && edge.v1 != null)
                    edge.midPosition = (edge.v0.position + edge.v1.position) / 2f;
            }
        }


        public void AssignElevations()
        {
            Debug.Log("*---Assign elevations...---*");
            AssignWater();
            AssignCornerElevations();
            AssignOceanCoastAndLand();
            RedistributeElevations();
            foreach (var q in corners)
                if (q.isOcean || q.isCoast)
                    q.elevation = 0;
            AssignPolygonElevations();  
        }


        private void AssignWater()
        {
            foreach (var corner in corners)
            {
                corner.isWater = !shape.IsPointInsideShape(corner.position, size, seed);
            }
        }

        private void AssignCornerElevations()
        {
            Debug.Log("*---assign corner elevations...---*");

            Queue<CellCorner> queue = new Queue<CellCorner>();

            foreach (var corner in corners)
            {
                if (corner.isBorder)
                {
                    corner.elevation = 0f;
                    queue.Enqueue(corner);
                }
                else
                {
                    corner.elevation = 10000f;
                    //corner.elevation = Mathf.Infinity;
                }
            }
          

            while (queue.Count > 0)
            {
                var q = queue.Dequeue();
                foreach (var s in q.neighborCorners)
                {
                    float newElevation = 0.01f + q.elevation;
                    if (!q.isWater && !s.isWater)
                        newElevation += 1;

                    if (newElevation < s.elevation)
                    {
                        s.elevation = newElevation;
                        queue.Enqueue(s);
                    }
                }
            }

            Dictionary<string, int> time = new Dictionary<string, int>();

            foreach (var corner in corners)
            {
                string str = corner.position.ToString("F6");
                if (!time.ContainsKey(str))
                    time[str] = 0;
                    time[str]++;
            }
            foreach (var t in time)
            {
                if (t.Value > 1)
                    Debug.LogError(t.Key);
            }

        }

        public void AssignOceanCoastAndLand()
        {
            Debug.Log("*---assign ocean、coast and land...---*");

            //遍历所有单元，如果一个单元位于边缘则意味着它是海洋
            //在一次遍历中将海洋板块入队
            //之后则从海洋板块开始进行广度搜索
            Queue<CellCenter> queue = new Queue<CellCenter>();
            int numWater = 0;

            foreach (var center in cells)
            {
                numWater = 0;
                foreach (var corner in center.cellCorners)
                {
                    if (corner.isBorder)
                    {
                        center.isBorder = true;
                        center.isOcean = true;
                        corner.isWater = true;
                        queue.Enqueue(center);
                    }
                    if (corner.isWater)
                        numWater += 1;
                }
                center.isWater = center.isOcean ||
                      numWater >= center.cellCorners.Count * LAKE_THRESHOLD;
            }

            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                foreach (var n in c.neighborCells)
                {
                    if (n.isWater && !n.isOcean)
                    {
                        n.isOcean = true;
                        queue.Enqueue(n);
                    }
                }
            }

            //遍历每个板块的每个邻居
            //若它同时挨着海洋和陆地则标记为沙滩板块
            foreach (var cell in cells)
            {
                int numOcean = 0;
                int numLand = 0;
                foreach (var n in cell.neighborCells)
                {
                    numOcean += (n.isOcean ? 1 : 0);
                    numLand += (!n.isWater ? 1 : 0);
                    if (numOcean > 0 && numLand > 0)
                    {
                        cell.isCoast = true;
                        break;
                    }
                }
            }

            foreach (var corner in corners)
            {
                int numOcean = 0;
                int numLand = 0;
                foreach (var cell in corner.touchingCells)
                {
                    numOcean += cell.isOcean ? 1 : 0;
                    numLand += !cell.isWater ? 1 : 0;
                }

                corner.isOcean = numOcean == corner.touchingCells.Count;
                corner.isCoast = numOcean > 0 && numLand > 0;
                corner.isWater = corner.isBorder ||
                    numLand != corner.touchingCells.Count &&
                    !corner.isCoast;
            }
        }

        public void RedistributeElevations()
        {
            var SCALE_FACTOR = 1.1f;
            var locations = LandCorners();

            float min = float.MaxValue, max = float.MinValue;
            for (int i = 0; i < locations.Count; i++)
            {
                var elevation = locations[i].elevation;
                if (elevation > max) max = elevation;
                if (elevation < min) min = elevation;
            }

            //locations.Sort((a, b) => a.elevation.CompareTo(b.elevation));
            for (int i = 0; i < locations.Count; i++)
            {
                //float y = (float)i / (locations.Count - 1);
                //float x = Mathf.Sqrt(SCALE_FACTOR) - Mathf.Sqrt(SCALE_FACTOR * (1.0f - y));
                //if (x > 1f)
                //    x = 1f;
                //locations[i].elevation = x;
                locations[i].elevation = (locations[i].elevation - min) / (max - min);
                if (locations[i].elevation > 1)
                    locations[i].elevation = 1;
            }
        }


        //获取所有为陆地板块的角落
        private List<CellCorner> LandCorners()
        {
            List<CellCorner> locations = new List<CellCorner>();
            foreach (var corner in corners)
            {
                if (!corner.isOcean && !corner.isCoast)
                    locations.Add(corner);
            }
            return locations;
        }

        public void AssignPolygonElevations()
        {
         
            foreach (var cell in cells)
            {
                float elevation = 0;
                foreach (var corner in cell.cellCorners)
                {
                    elevation += corner.elevation;
                }
                //Debug.LogError(cell.index  + "  "+ elevation +"  "+cell.cellCorners.Count);
                cell.elevation = elevation / cell.cellCorners.Count;
            }
        }




        private void DetectIslands()
        {
            List<CellCenter> land = new List<CellCenter>();  //多个三角形构成一个Cell
            foreach (var cell in cells)
            {
                if (cell.isOcean)
                    cell.islandID = -1;
                else
                    land.Add(cell);
            }

            Dictionary<int, CellCenter> lookupCell = new Dictionary<int, CellCenter>();

            islands = new List<List<CellCenter>>();
            for (int i = 0; i < land.Count; i++)
            {
                CellCenter currentCell = land[i];
                //if (!islands.Any(x => x.Contains(currentCell)))
                if(!lookupCell.ContainsKey(currentCell.index))
                {
                    lookupCell[currentCell.index] = currentCell;

                    List<CellCenter> currentIsland = new List<CellCenter>();
                    islands.Add(currentIsland);

                    currentIsland.Add(currentCell);
                    currentCell.islandID = islands.Count - 1;

                    Queue<CellCenter> islandQueue = new Queue<CellCenter>();
                    islandQueue.Enqueue(currentCell);

                    while (islandQueue.Count > 0)
                    {
                        currentCell = islandQueue.Dequeue();
                        foreach (var neighbor in currentCell.neighborCells)
                        {
                            if (!neighbor.isOcean && !currentIsland.Contains(neighbor))
                            {
                                islandQueue.Enqueue(neighbor);
                                currentIsland.Add(neighbor);
                                neighbor.islandID = islands.Count - 1;
                            }
                        }
                    }
                }
            }

            if (singleIsland)
                minIslandSize = islands.Max(x => x.Count);

            for (int i = 0; i < islands.Count; i++)
            {
                List<CellCenter> currentIsland = islands[i];
                //如果不足以形成岛屿则移除
                if (currentIsland.Count < minIslandSize)
                {
                    foreach (var cell in currentIsland)
                    {
                        cell.isWater = true;
                        cell.isOcean = true;
                        cell.islandID = -1;
                    }
                    islands.RemoveAt(i);
                    i--;
                }
            }
        }



        public void AssignMoisture()
        {
            Debug.Log("*---Assign moisture...---*");
            CalculateDownslopes();
            CalculateWatersheds();
            CreateRivers();
            AssignCornerMoisture();
            RedistributeMoisture(LandCorners());
            AssignPolygonMoisture();
        }

        private void CalculateDownslopes()
        {
            foreach (var q in corners)
            {
                CellCorner corner = q;
                foreach (var s in q.neighborCorners)
                    if (s.elevation <= corner.elevation)
                        corner = s;
                q.downslopeCorner = corner;
            }
        }

        private void CalculateWatersheds()
        {
            foreach (var q in corners)
            {
                q.watershed = q;
                if(!q.isOcean && !q.isCoast)
                {
                    q.watershed = q.downslopeCorner;
                }
            }

            for (int i = 0; i < 100; i++)
            {
                bool changed = false;
                foreach (var q in corners)
                {
                    if(!q.isOcean && !q.isCoast && !q.watershed.isCoast)
                    {
                        CellCorner r = q.downslopeCorner.watershed;
                        if (!r.isOcean)
                        {
                            q.watershed = r;
                            changed = true;
                        }
                    }
                }
                if (!changed)
                    break;
            }

            foreach (var q in corners)
            {
                q.watershed.watershedSize++;
            }
        }

        private void  CreateRivers()
        {
            for (int i = 0; i < size.x / 2f; i++)
            {
                var q = corners[Random.Range(0, corners.Count - 1)];
                if (q.isOcean || q.elevation < 0.3f || q.elevation > 0.9f)
                    continue;
                while (!q.isCoast)
                {
                    if (q == q.downslopeCorner)
                        break;

                    var edge = LookupEdgeFromCorner(q, q.downslopeCorner);
                    edge.waterVolume = edge.waterVolume + 1;
                    q.river++;
                    q.downslopeCorner.river++;
                    q = q.downslopeCorner;
                }
            }
        }

        private void AssignCornerMoisture()
        {
            //TODO
            Queue<CellCorner> queue = new Queue<CellCorner>();

            foreach (var q in corners)
            {
                if((q.isWater || q.river > 0) && !q.isOcean)
                {
                    q.moisture = q.river > 0 ? Mathf.Min(3.0f, 0.2f * q.river) : 1.0f;
                    queue.Enqueue(q);
                }
                else
                {
                    q.moisture = 0.0f;
                }
            }

            while(queue.Count > 0)
            {
                var q = queue.Dequeue();
                var newMoisture = 0.0f;
                foreach (var r in q.neighborCorners)
                {
                    newMoisture = q.moisture * 0.9f;
                    if(newMoisture > r.moisture)
                    {
                        r.moisture = newMoisture;
                        queue.Enqueue(r);
                    }
                }
            }

            foreach (var q in corners)
            {
                if (q.isOcean || q.isCoast)
                    q.moisture = 1f;
            }
        }



        private void RedistributeMoisture(List<CellCorner> list)
        {
            list.Sort((a, b) => a.moisture.CompareTo(b.moisture));
            for (int i = 0; i < list.Count; i++)
            {
                list[i].moisture = (float)i / (list.Count - 1);
            }
        }



        private void AssignPolygonMoisture()
        {
            float sumMoisture = 0;
            foreach (var p in cells)
            {
                sumMoisture = 0;
                foreach (var q in p.cellCorners)
                {
                    if (q.moisture > 1.0f)
                        q.moisture = 1.0f;
                    sumMoisture += q.moisture;
                }
                p.moisture = sumMoisture / p.cellCorners.Count;
            }
        }

        private void AssignBiomes()
        {
            foreach (var p in cells)
            {
                p.biome = GetBiome(p);
            }
        }

        private Biomes GetBiome(CellCenter cell)
        {
            if (cell.isOcean)
                return Biomes.Ocean;
            else if (cell.isWater)
            {
                if (cell.elevation < 0.1f) return Biomes.Marsh;
                else if (cell.elevation > 0.8f) return Biomes.Ice;
                else return Biomes.Lake;
            }
            else if (cell.isCoast)
                return Biomes.Beach;
            else if(cell.elevation > 0.8f)
            {
                if (cell.moisture > 0.5f) return Biomes.Snow;
                else if (cell.moisture > 0.33f) return Biomes.Tundra;
                else if (cell.moisture > 0.16f) return Biomes.Bare;
                else return Biomes.Scorched;
            }
            else if(cell.elevation > 0.6f)
            {
                if (cell.moisture > 0.66f) return Biomes.Taiga;
                else if (cell.moisture > 0.33f) return Biomes.Shrubland;
                else return Biomes.Temperate_Desert;
            }
            else if(cell.elevation > 0.3f)
            {
                if (cell.moisture > 0.83f) return Biomes.Temperate_Rain_Forest;
                else if (cell.moisture > 0.5f) return Biomes.Temperate_Deciduous_Forest;
                else if (cell.moisture > 0.16f) return Biomes.Grassland;
                else return Biomes.Temperate_Desert;
            }
            else
            {
                if (cell.moisture > 0.66f) return Biomes.Tropical_Rain_Forest;
                else if (cell.moisture > 0.33f) return Biomes.Tropical_Seasonal_Forest;
                else if (cell.moisture > 0.16f) return Biomes.Grassland;
                else return Biomes.Subtropical_Desert;
            }
        }

        public CellEdge LookupEdgeFromCenter(CellCenter p,CellCenter r)
        {
            foreach (var edge in p.borderEdges)
                if (edge.d0 == r || edge.d1 == r)
                    return edge;
            return null;
        }

        private CellEdge LookupEdgeFromCorner(CellCorner q,CellCorner s)
        {
            foreach (var edge in q.connectedEdges)
            {
                if (edge.v0 == s || edge.v1 == s)
                    return edge;
            }
            return null;
        }


        private void ResetMapInfo()
        {
            Debug.Log("---Reset Map Info---");
            corners.Clear();
            cells.Clear();
            edges.Clear();
        }
    }
}