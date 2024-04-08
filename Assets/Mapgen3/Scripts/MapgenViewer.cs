using Marisa.Maps.Enums;
using Marisa.Maps.Graph;
using Marisa.Maps.Tools;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Marisa.Maps
{
    public class MapgenViewer : MonoBehaviour
    {
        public RawImage rawImage;
        [Space]
        public Mapgen3 map;
        public int width = 800;
        public int height = 800;

        [Header("Light")]
        public Color lightColor;
        public Color darkColor;
        public float offset = 500;
        public Vector3 lightVector; 


        private Texture2D base_texture;
        private Texture2D noise_texture;
        private Texture2D result_texture;


        private void Start()
        {
            map.onMapGenerated += Map_onMapGenerated;
            map.Generate();
        }




        private void Map_onMapGenerated()
        {
            CreateNoisymap();
            
            base_texture = Texture2DExtension.CreateTexture(width, height);
            base_texture.Clear(ColorTool.GetColor(Biomes.Ocean));
            base_texture =  RenderBaseMap();
            RenderEdges();
            //RenderRivers();
            //RenderElevation();
            base_texture.Apply();

            result_texture = Texture2DExtension.HardLightMix(width, height, noise_texture, base_texture);
            rawImage.texture = result_texture;
            //rawImage.texture = base_texture;

            Texture2DExtension.SaveToPng(Application.streamingAssetsPath + $"/base_{map.seed}.png", base_texture);
            Texture2DExtension.SaveToPng(Application.streamingAssetsPath + $"/result_{map.seed}.png", result_texture);

            base_texture.Apply();


            //Save Voronoi Texture
            RenderVoronoi();
            RenderDelaunay();
            RenderElevation();
            RenderMoisture();
            RenderTriangles();
            RenderVoronoiAndDelaunay();
        }

        private Texture2D RenderBaseMap()
        {
            Texture2D base_texture = Texture2DExtension.CreateTexture(width,height);
            base_texture.Clear(ColorTool.GetColor(Biomes.Ocean));
            CellEdge edge;
            Color color;
            List<Vector2> path = new List<Vector2>();
            void DrawPath(CellCenter center)
            {
                path.Clear();
                path.AddRange(map.noisyEdges.path[edge.index]);
                path.Add(center.position);
                base_texture.DrawPolygon(path, color);
            }

            foreach (var center in map.cells)
            {
                foreach (var neighbor in center.neighborCells)
                {
                    edge = map.LookupEdgeFromCenter(center, neighbor);
                    if (map.noisyEdges.path[edge.index] == null)
                        continue;
                    color = ColorTool.GetColor(center.biome);
                    color = ColorWithSlope(color, center, neighbor, edge);
                    DrawPath(center);
                }
            }
            base_texture.Apply();
            return base_texture;
        }

        private Texture2D RenderVoronoiAndDelaunay()
        {
            Texture2D voronoi_delaunay_texture = Texture2DExtension.CreateTexture(width, height);
            voronoi_delaunay_texture.Clear(Color.black);
            for (int i = 0; i < map.edges.Count; i++)
            {
                var edge = map.edges[i];
                voronoi_delaunay_texture.DrawLine(edge.v0.position, edge.v1.position, 0, Color.white);
                voronoi_delaunay_texture.DrawLine(edge.d0.position, edge.d1.position, 0, Color.red);
            }
            Texture2DExtension.SaveToPng(Application.streamingAssetsPath+$"/test_{map.seed}.png",voronoi_delaunay_texture);
            voronoi_delaunay_texture.Apply();
            return voronoi_delaunay_texture;
        }

        public Texture2D RenderDelaunay()
        {
            Texture2D delaunay_texture = Texture2DExtension.CreateTexture(width, height);
            delaunay_texture.Clear(Color.black);
            for (int i = 0; i < map.edges.Count; i++)
            {
                var edge = map.edges[i];
                delaunay_texture.DrawLine(edge.d0.position,edge.d1.position,0,Color.white);
            }
            Texture2DExtension.SaveToPng(Application.streamingAssetsPath+$"/delaunay_{map.seed}.png",delaunay_texture);
            delaunay_texture.Apply();
            return delaunay_texture; 
        }

        public Texture2D RenderVoronoi()
        {
            Texture2D voronoi_texture = Texture2DExtension.CreateTexture(width, height);
            voronoi_texture.Clear(Color.black);
            for (int i = 0; i < map.edges.Count; i++)
            {
                var edge = map.edges[i];
                voronoi_texture.DrawLine(edge.v0.position, edge.v1.position, 0, Color.white);
            }

            Texture2DExtension.SaveToPng(Application.streamingAssetsPath+$"/voronoi_{map.seed}.png",voronoi_texture);
            voronoi_texture.Apply();
            return voronoi_texture;
        }


        public void RenderRivers()
        {
            var rivers = map.rivers;
            Color color = ColorTool.GetColor(Biomes.River);
            float size = 1f;
            for (int i = 0; i < rivers.riverSources.Count - 1; i++)
            {
                int high = rivers.riverSources[i];
                while (rivers.lowestCells.TryGetValue(high,out int low))
                {
                    if (!map.cells[high].isWater && !map.cells[high].isOcean)
                    {
                        size = Mathf.Sqrt(rivers.waterVolumes[high]) / 2f; 
                        base_texture.DrawLine(map.cells[high].position, map.cells[low].position, Mathf.RoundToInt(size), color);
                    }

                    high = low;
                }
            }
        }

        public void RenderElevation()
        {
            Texture2D elevation_texture = Texture2DExtension.CreateTexture(width, height);
            elevation_texture.Clear(Color.black);
            float minElevation = map.cells.Min(x => x.elevation);
            float maxElevation = map.cells.Max(x => x.elevation);

            for (int i = 0; i < map.cells.Count; i++)
            {
                var list = GetPolygon(map.cells[i]);
                var elevation = map.cells[i].elevation;
                var t = (elevation - minElevation) / (maxElevation - minElevation);
                Color color = Color.Lerp(Color.black, Color.white, t);

                elevation_texture.DrawPolygon(list, color);
            }
            Texture2DExtension.SaveToPng(Application.streamingAssetsPath + $"/elevation_{map.seed}.png", elevation_texture);
        }

        public void RenderMoisture()
        {
            Texture2D moisture_texture = Texture2DExtension.CreateTexture(width, height);
            moisture_texture.Clear(Color.black);
            float minMoisture = map.cells.Min(x => x.moisture);
            float maxMoisture = map.cells.Max(x => x.moisture);

            for (int i = 0; i < map.cells.Count; i++)
            {
                var list = GetPolygon(map.cells[i]);
                var moisture = map.cells[i].moisture;
                var t = (moisture - minMoisture) / (maxMoisture - minMoisture);
                Color color = Color.Lerp(Color.white, Color.black, t);

                moisture_texture.DrawPolygon(list, color);
            }
            Texture2DExtension.SaveToPng(Application.streamingAssetsPath + $"/moisture_{map.seed}.png", moisture_texture);
        }

        public void RenderTriangles()
        {
            Texture2D triangle_texture = Texture2DExtension.CreateTexture(width, height);

            CellCenter cell;
            List<CellCenter> neighbors;
            List<Vector2> temp;
            float g;
            for (int i = 0; i < map.cells.Count; i++)
            {
                cell = map.cells[i];
                neighbors = GetPolygonByNeighbor(cell);
                for (int j = 0; j < neighbors.Count - 1; j++)
                {
                    //g = (cell.elevation + neighbors[j].elevation + neighbors[j + 1].elevation) / 3f;
                    //temp = GetTriangle(cell, neighbors[j], neighbors[j + 1]);
                    //triangle_texture.DrawPolygon(temp, new Color(g, g, g, 1));

                    triangle_texture.DrawTriangle(
                        cell.position,ColorTool.GetColor(cell.biome),
                        neighbors[j].position, ColorTool.GetColor(neighbors[j].biome),
                        neighbors[j+1].position, ColorTool.GetColor(neighbors[j+1].biome));
                }
                //g = (cell.elevation + neighbors[0].elevation + neighbors[neighbors.Count - 1].elevation) / 3f;
                //temp = GetTriangle(cell, neighbors[0], neighbors[neighbors.Count - 1]);
                //triangle_texture.DrawPolygon(temp, new Color(g, g, g, 1));

                triangle_texture.DrawTriangle(
                         cell.position, ColorTool.GetColor(cell.biome),
                         neighbors[0].position,ColorTool.GetColor(neighbors[0].biome),
                         neighbors[neighbors.Count- 1].position, ColorTool.GetColor(neighbors[neighbors.Count -  1].biome));
            }

            Texture2DExtension.SaveToPng(Application.streamingAssetsPath + $"/triangle_{map.seed}.png", triangle_texture);
        }


        private List<Vector2> GetTriangle(CellCenter a,CellCenter b, CellCenter c)
        {
            List<Vector2> list = new List<Vector2>()
            {
                a.position,
                b.position,
                c.position,
            };
            Vector2 center = (a.position + b.position + c.position) / 3f;
            list = list.OrderBy(p => Mathf.Atan2(p.y - center.y, p.x - center.x)).ToList();
            return list;
        }


        private List<CellCenter> GetPolygonByNeighbor(CellCenter cell)
        {
            List<CellCenter> list = new List<CellCenter>();
            for (int i = 0; i < cell.neighborCells.Count; i++)
            {
                list.Add(cell.neighborCells[i]);
            }

            Vector2 center = Vector2.zero;
            for (int i = 0; i < list.Count; i++)
                center += list[i].position;
            center /= list.Count;

            list = list.OrderBy(p => Mathf.Atan2(p.position.y - center.y, p.position.x - center.x)).ToList();
            return list;
        }

        private List<Vector2> GetPolygon(CellCenter cell)
        {
            List<Vector2> list = new List<Vector2>();
            for (int j = 0; j < cell.cellCorners.Count; j++)
            {
                var point = cell.cellCorners[j].position;
                int x = Mathf.RoundToInt((point.x / map.size.x) * width);
                int y = Mathf.RoundToInt((point.y / map.size.y) * height);
                list.Add(new Vector2(x, y));
            }

            var center = MathfTool.GetCenter(list);
            list = list.OrderBy(p => Mathf.Atan2(p.y - center.y, p.x - center.x)).ToList();
            return list;
        }


        public void RenderEdges()
        {
            CellEdge edge;
            Color color;
            int size;
            foreach (var p in map.cells)
            {
                foreach (var r in p.neighborCells)
                {
                    edge = map.LookupEdgeFromCenter(p, r);
                    if (map.noisyEdges.path[edge.index] == null)
                        continue;
                    if (p.isOcean != r.isOcean)
                    {
                        //»­º£°¶Ïß
                        size = 2;
                        color = ColorTool.GetColor(Biomes.Coast);
                    }
                    else if (p.isWater != r.isWater && p.biome != Biomes.Ice && r.biome != Biomes.Ice)
                    {
                        //»­ºþ²´
                        size = 1;
                        color = ColorTool.GetColor(Biomes.Lake);
                    }
                    else if (p.isWater || r.isWater)
                    {
                        continue;
                    }
                    else if (edge.waterVolume > 0)
                    {
                        //»­River
                        float sizef = Mathf.Sqrt(edge.waterVolume);
                        if (sizef < 0) sizef = 0;
                        size = Mathf.RoundToInt(sizef);
                        color = ColorTool.GetColor(Biomes.River);
                        //continue;
                    }
                    else
                    {
                        //no edge
                        continue;
                    }

                    var path = map.noisyEdges.path[edge.index];
                    for (int i = 0; i < path.Count - 1; i++)
                    {
                        base_texture.DrawLine(path[i], path[i + 1], size, color);
                    }
                }
            }
        }




         

        private Color ColorWithSlope(Color color,CellCenter p,CellCenter q,CellEdge edge)
        {
            CellCorner r = edge.v0;
            CellCorner s = edge.v1;
            if (r == null || s == null)
                return ColorTool.GetColor(Biomes.Ocean);
            else if (p.isWater)
                return color;

            if (q != null && p.isCoast == q.isWater)
                color = Color.Lerp(color, ColorTool.GetColor(q.biome), 0.4f);
            Color colorLow = Color.Lerp(color, darkColor, 0.7f);
            Color colorHigh = Color.Lerp(color, lightColor, 0.3f);
            float light = CalculateLighting(p, r, s);
            if (light < 0.5f)
                return Color.Lerp(colorLow, color, light * 2f);
            return Color.Lerp(color, colorHigh, light * 2f - 1f);
        }

        private float CalculateLighting(CellCenter p, CellCorner r, CellCorner s)
        {
            var a = new Vector3(p.position.x, p.position.y, p.elevation * offset);
            var b = new Vector3(r.position.x, r.position.y, r.elevation * offset);
            var c = new Vector3(s.position.x, s.position.y, s.elevation * offset);
            var normal = Vector3.Cross(b - a, c - a);
            if (normal.z < 0)
                normal *= -1;
            normal.Normalize();
            var light = 0.5f + 0.35f * Vector3.Dot(normal, lightVector);
            light = Mathf.Clamp01(light);
            return light;
        }

        private void CreateNoisymap()
        {
            noise_texture = Texture2DExtension.CreateNoisyMap(width, height,map.seed);
        }

    }
}