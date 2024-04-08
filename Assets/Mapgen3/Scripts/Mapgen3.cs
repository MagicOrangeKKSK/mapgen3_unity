using csDelaunay;
using Marisa.Maps.Enums;
using Marisa.Maps.Extension;
using Marisa.Maps.Graph;
using Marisa.Maps.PointSelectors;
using Marisa.Maps.Shapes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.U2D;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace Marisa.Maps
{
    public class Mapgen3 : MonoBehaviour
    {
        public float LAKE_THRESHOLD = 0.3f;
        public int seed;
        public int pointNumber = 8000;
        public Vector2 size = new Vector2(800, 800);
        public int relaxation = 0; //松弛系数
        public int riverCount = 2000;
        public PointSelector pointSelector;
        public IslandShape shape;

        public List<CellCenter> cells = new List<CellCenter>();
        public List<CellCorner> corners = new List<CellCorner>();
        public List<CellEdge> edges = new List<CellEdge>();

        public List<Vector2> points { private set; get; } = new List<Vector2>(); 

        public Roads roads { private set; get; }
        public WaterSheds waterSheds { private set; get; }
        public NoisyEdges noisyEdges { private set; get; }
        public Rivers rivers { private set; get; }
        public event Action onMapGenerated;



        public void Generate()
        {
            roads = new Roads();
            waterSheds = new WaterSheds();
            noisyEdges = new NoisyEdges();
            //rivers = new Rivers();


            Stopwatch sw = Stopwatch.StartNew();

            Debug.Log("*---- start generator ----*");
            ResetMapInfo();
            Debug.Log("*---- place points... ----*");
            PlacePoints();
            Debug.Log("*---- build graph... ----*");
            BuildGraph();
            Debug.Log("*---- features... ----*");
            Features();
            Debug.Log("*---- edges... ----");
            Edges();
            Debug.Log("*---- end ----*");
            onMapGenerated?.Invoke();

            sw.Stop();
            Debug.Log("time:" + sw.ElapsedMilliseconds);
        }

        private void Features()
        {
            AssignElevations();
            AssignMoistures();
            AssignBiomes();
        }

        private void Edges()
        {
            foreach (var edge in edges)
            {
                if (edge.v0 != null && edge.v1 != null)
                    edge.midPosition = Vector2.Lerp(edge.v0.position, edge.v1.position, 0.5f);
            }

            roads.CreateRoads(this);
            waterSheds.CreateWatersheds(this);
            noisyEdges.BuildNoisyEdges(this);
            //rivers.CreateRivers(this,riverCount);
        }

        #region AssignElevations

        private void AssignElevations()
        {
            AssignWater();
            AssignCornerElevations();
          //  Erode();

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
                    corner.elevation = Mathf.Infinity;
                }
            }

            while (queue.Count > 0)
            {
                var q = queue.Dequeue();
                float newElevation = 0.01f + q.elevation;
                foreach (var s in q.neighborCorners)
                {
                    if (!q.isWater && !s.isWater)
                        newElevation += 1;
                    if (newElevation < s.elevation)
                    {
                        s.elevation = newElevation; ;
                        queue.Enqueue(s);
                    }
                }
            }
        }


        public void Erode()
        {
            //float initialSpeed = 1f;
            //float initialWaterVolume = 1f;

            //float minElevation = float.MaxValue;
            //float maxElevation = float.MinValue;
            //for (int i = 0; i < corners.Count; i++)
            //{
            //    if (corners[i].elevation < minElevation)
            //        minElevation = corners[i].elevation;
            //    if (corners[i].elevation > maxElevation)
            //        maxElevation = corners[i].elevation;
            //}
            //for (int i = 0; i < corners.Count; i++)
            //{
            //    float elevation = corners[i].elevation;
            //    corners[i].elevation = (elevation - minElevation) / (maxElevation - minElevation);
            //}


            //for (int iteraction = 0; iteraction < 600000; iteraction++)
            //{
            //    float speed = initialSpeed;
            //    float water = initialWaterVolume;
            //    float sediment = 0;
            //    CellCorner corner = corners[Random.Range(0, corners.Count)];
            //    for (int lifetime = 0; lifetime < 30; lifetime++)
            //    {
            //        var low = corner;
            //        foreach (var neighbor in corner.neighborCorners)
            //            if (neighbor.elevation < low.elevation)
            //                low = neighbor;


            //        float deltaHeight = low.elevation - corner.elevation;
            //        float sedimentCapacity = Mathf.Max(-deltaHeight * speed * water * 4, 0.0001f);  //高度差 * 速度 * 水量 * 因子 
            //        //Debug.Log($"deltaHeight[{-deltaHeight}] *spped[{speed}] * water[{water}] * 4 = {-deltaHeight * speed * water * 4}");
            //        if (sediment > sedimentCapacity || deltaHeight > 0)
            //        {
            //            float amountToDeposit = deltaHeight > 0 ? Mathf.Min(deltaHeight, sediment) : (sediment - sedimentCapacity) * 0.01f;
            //            //Debug.Log($"增加 {corner.index}[{corner.elevation}]->{low.index}[{low.elevation}]  沉积物:{sediment}>容量:{sedimentCapacity} :: {amountToDeposit}");
            //            sediment -= amountToDeposit;
            //            low.elevation += amountToDeposit;
            //        }
            //        else
            //        {
            //            float amountToErode = Mathf.Min(-deltaHeight, (sedimentCapacity - sediment) * 0.01f); //沉积速度
            //            //Debug.Log($"减少 {corner.index}[{corner.elevation}]->{low.index}[{low.elevation}]  沉积物:{sediment}<=容量:{sedimentCapacity} :: {amountToErode}");
            //            low.elevation -= amountToErode;
            //            sediment += amountToErode;
            //        }

            //        //float delta = corner.elevation - low.elevation;
            //        //if (delta < 0.1f)
            //        //    low.elevation = corner.elevation - 0.1f;

            //        corner = low;
            //        //Debug.Log($"spped = speed[{speed}] * speed[{speed}] + deltaHeight[{deltaHeight}] * 4 = {speed * speed + deltaHeight *4f}");
            //        speed = Mathf.Sqrt(speed * speed + deltaHeight * 4f);
            //        water *= (1f - 0.01f);
            //    }
            //    //Debug.Log(sediment);
            //}

            //for (int i = 0; i < corners.Count; i++)
            //{
            //    corners[i].elevation = Mathf.Lerp(minElevation, maxElevation, corners[i].elevation);
            //}

            for (int i = 0; i < corners.Count; i++)
            {
                for (int j = 0; j < corners[i].neighborCorners.Count; j++)
                {
                    Debug.DrawLine(corners[i].position, corners[i].neighborCorners[j].position,Color.red, 1000);
                }
            }

        }



        public void AssignOceanCoastAndLand()
        {

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
        private List<CellCorner> LandCorners()
        {
            List<CellCorner> locations = new List<CellCorner>();
            foreach (var corner in corners)
                if (!corner.isOcean && !corner.isCoast)
                    locations.Add(corner);
            return locations;
        }


        public void RedistributeElevations()
        {
            var SCALE_FACTOR = 1.1f;
            var locations = LandCorners();
            locations.Sort((a, b) => a.elevation.CompareTo(b.elevation));
            for (int i = 0; i < locations.Count; i++)
            {
                float y = (float)i / (locations.Count - 1);
                float x = Mathf.Sqrt(SCALE_FACTOR) - Mathf.Sqrt(SCALE_FACTOR * (1.0f - y));
                if (x > 1f)
                    x = 1f;
                locations[i].elevation = x;
            }
        }

        public void AssignPolygonElevations()
        {
            foreach (var cell in cells)
            {
                float elevation = 0;
                foreach (var corner in cell.cellCorners)
                    elevation += corner.elevation;
                cell.elevation = elevation / cell.cellCorners.Count;
            }
        }

        #endregion

        #region AssignMoistures

        private void AssignMoistures()
        {
            CalculateDownslopes();
            CalculateWatersheds();
            CreateRivers();
            AssignCornerMoisture();
            RedistributeMoisture();
            AssignPolygonMositure();
        }

        private void CalculateDownslopes()
        {
            foreach (var corner in corners)
            {
                var low = corner;
                foreach (var neighbor in corner.neighborCorners)
                {
                    if(neighbor.elevation <= low.elevation)
                    {
                        low = neighbor;
                    }
                }
                corner.downslopeCorner = low;
            }
        }

        private void CalculateWatersheds()
        {
            foreach (var q in corners)
            {
                q.watershed = q;
                if (!q.isOcean && !q.isCoast)
                    q.watershed = q.downslopeCorner;
            }

            for (int i = 0; i < corners.Count; i++)
            {
                bool changed = false;
                foreach (var q in corners)
                {
                    if (!q.isOcean && !q.isCoast && !q.watershed.isCoast)
                    {
                        var r = q.downslopeCorner.watershed;
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

        private void CreateRivers()
        {
            //for (int i = 0; i < size.x / 2; i++)
            for (int i = 0; i < riverCount; i++)
                {
                var q = corners[Random.Range(0, corners.Count)];
                if (q.isOcean || q.elevation < 0.3f || q.elevation > 0.9f)
                    continue;
                while (!q.isCoast)
                {
                    if (q == q.downslopeCorner)
                        break;
                    var edge = LookupEdgeFromCorner(q, q.downslopeCorner);
                    edge.waterVolume++;
                    q.river++;
                    q.downslopeCorner.river++;
                    q = q.downslopeCorner;
                }
            }
        }

        private void AssignCornerMoisture()
        {
            Queue<CellCorner> queue = new Queue<CellCorner>();
            foreach (var q in corners)
            {
                if((q.isWater || q.river > 0) && !q.isOcean)
                {
                    q.moisture = q.river > 0 ? Mathf.Min(3.0f,(0.2f * q.river )) : 1.0f;
                    queue.Enqueue(q);
                }
                else
                {
                    q.moisture = 0f;
                }
            }
            while(queue.Count > 0)
            {
                var q = queue.Dequeue();
                foreach (var r in q.neighborCorners)
                {
                    var newMoisture = q.moisture * 0.9f;
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
                    q.moisture = 1.0f;
            }
        }

        private void RedistributeMoisture()
        {
            var locations = LandCorners();
            locations.Sort((a, b) => a.moisture.CompareTo(b.moisture));
            for (int i = 0; i < locations.Count; i++)
                locations[i].moisture = (float)i / (locations.Count - 1);
        }

        private void AssignPolygonMositure()
        {
            foreach (var cell in cells)
            {
                var sumMoisture = 0f;
                foreach (var corner in cell.cellCorners)
                {
                    if (corner.moisture > 1.0f)
                        corner.moisture = 1.0f;
                    sumMoisture += corner.moisture;
                }
                cell.moisture = sumMoisture / cell.cellCorners.Count;
            }
        }

        #endregion

        #region AssignBiomes

        private void AssignBiomes()
        {
            foreach (var cell in cells)
            {
               GetBiome(cell);
            }
        }

        private void GetBiome(CellCenter cell)
        {
            if (cell.isOcean)
                cell.biome = Biomes.Ocean;
            else if (cell.isWater)
            {
                if (cell.elevation < 0.1f)
                    cell.biome = Biomes.Marsh;
                else if (cell.elevation > 0.8f)
                    cell.biome = Biomes.Ice;
                else
                    cell.biome = Biomes.Lake;
            }
            else if (cell.isCoast)
            {
                cell.biome = Biomes.Beach;
            }
            else if (cell.elevation > 0.8f)
            {
                if (cell.moisture > 0.5f)
                    cell.biome = Biomes.Snow;
                else if (cell.moisture > 0.33f)
                    cell.biome = Biomes.Tundra;
                else if (cell.moisture > 0.16f)
                    cell.biome = Biomes.Bare;
                else
                    cell.biome = Biomes.Scorched;
            }
            else if (cell.elevation > 0.6f)
            {
                if (cell.moisture > 0.66f)
                    cell.biome = Biomes.Taiga;
                else if (cell.moisture > 0.33f)
                    cell.biome = Biomes.Shrubland;
                else
                    cell.biome = Biomes.Temperate_Desert;
            }
            else if (cell.elevation > 0.3f)
            {
                if (cell.moisture > 0.83f)
                    cell.biome = Biomes.Temperate_Rain_Forest;
                else if (cell.moisture > 0.5f)
                    cell.biome = Biomes.Temperate_Deciduous_Forest;
                else if (cell.moisture > 0.16f)
                    cell.biome = Biomes.Grassland;
                else
                    cell.biome = Biomes.Temperate_Desert;
            }
            else
            {
                if (cell.moisture > .66)
                    cell.biome = Biomes.Tropical_Rain_Forest;
                else if (cell.moisture > .33)
                    cell.biome = Biomes.Tropical_Seasonal_Forest;
                else if (cell.moisture > .16)
                    cell.biome = Biomes.Grassland;
                else
                    cell.biome = Biomes.Subtropical_Desert;
            }
        }

        #endregion

        private void BuildGraph()
        {
            Rectf bounds = new Rectf(0, 0, size.x, size.y);
            Voronoi voronoi = new Voronoi(points, bounds, relaxation);

            Dictionary<Vector2, CellCenter> centerLookup = new Dictionary<Vector2, CellCenter>();

            foreach (var point in voronoi.SitesIndexedByLocation)
            {
                var p = new CellCenter();
                p.index = cells.Count;
                p.position = point.Key;
                cells.Add(p);
                centerLookup[point.Key] = p;
            }

            Dictionary<(int,int), List<CellCorner>> bucket = new Dictionary<(int, int), List<CellCorner>>();
            CellCorner MakeCorner(Vector2 point)
            {
                if (point == null) return null;
                int x = (int)point.x;
                int y = (int)point.y;
                for (int xx = x - 1; xx <= x + 1; xx++)
                {
                    for (int yy = y - 1; yy <= y + 1; yy++)
                    {
                        if (!bucket.ContainsKey((xx,yy)))
                            bucket.Add((xx,yy), new List<CellCorner>());
                        var list = bucket[(xx,yy)];
                        for (int i = 0; i < list.Count; i++)
                        {
                            var dx = list[i].position.x - point.x;
                            var dy = list[i].position.y - point.y;
                            if (dx * dx + dy * dy < 1e-6)
                                return list[i];
                        }
                    }
                   
                }

                CellCorner q = new CellCorner();
                q.index = corners.Count;
                corners.Add(q);
                q.position = point;
                q.isBorder = (point.x == 0 || point.x == size.x || point.y == 0 || point.y == size.y);
                bucket[(x,y)].Add(q);
                return q;
            }

            void addPointToPointList<T>(List<T> list, T point) where T : MapPoint
            {
                if (!list.Contains(point))
                    list.Add(point);
            }

            foreach (var libEdge in voronoi.Edges)
            {
                if (libEdge.ClippedEnds == null) continue;

                var edge = new CellEdge();
                edge.index = edges.Count;
                edges.Add(edge);

                edge.v0 = MakeCorner(libEdge.ClippedEnds[LR.LEFT]);
                edge.v1 = MakeCorner(libEdge.ClippedEnds[LR.RIGHT]);
                edge.d0 = centerLookup[libEdge.LeftSite.Coord];
                edge.d1 = centerLookup[libEdge.RightSite.Coord];
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

        private void PlacePoints()
        {
            points = pointSelector.Generator(pointNumber, size, seed);
        }

        //查找两个Cell 共用的边
        public CellEdge LookupEdgeFromCenter(CellCenter center0,CellCenter center1)
        {
            foreach (var edge in center0.borderEdges)
                if (edge.d0 == center1 || edge.d1 == center1)
                    return edge;
            return null;
        }

        //查找两个Corner 共用的边
        public CellEdge LookupEdgeFromCorner(CellCorner corner0,CellCorner corner1)
        {
            foreach (var edge in corner0.connectedEdges)
                if(edge.v0 == corner1 || edge.v1 == corner1)
                    return edge;
            return null;
        }

        private void ResetMapInfo()
        {
            Debug.Log("---Reset Map Info---");
            points.Clear();
            corners.Clear();
            cells.Clear();
            edges.Clear();
        }
    }
}