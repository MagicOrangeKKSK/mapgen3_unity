using Marisa.Maps.Enums;
using Marisa.Maps.Graph;
using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace Marisa.Maps
{
    public class MapViewer : MonoBehaviour
    {
        public Mapgen3 mapgen;

        public int width = 800;
        public int height = 800;
        public RawImage rawImage;
        private Texture2D texture;

        private Texture2D noise_texture;

        public ComputeShader cs;

        void Start()
        {
            CreateNoisymap();

            mapgen.onMapGenerated += Mapgen_onMapGenerated;
            mapgen.Generate();
        }

        private void CreateNoisymap()
        {
            Random.InitState(452959354);
            noise_texture = new Texture2D(width, height);
            noise_texture.filterMode = FilterMode.Point;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    //float nx = (float)x / width * 20f+ 35423;
                    //float ny = (float)y / height *20f + 12314;

                    //float g = 1f * Mathf.PerlinNoise(nx * 1f,ny * 1f) +
                    //           (1f/2f) * Mathf.PerlinNoise(nx * 2f,ny * 2f) +
                    //           (1f/4f) * Mathf.PerlinNoise(nx * 4f,ny * 4f) +
                    //           (1f/8f) * Mathf.PerlinNoise(nx * 8f ,ny * 8f)+
                    //           (1f / 16f) * Mathf.PerlinNoise(nx *16f,ny * 16f);
                    //g /= (1f + 1f / 2f + 1f / 4f + 1f / 8f + 1f / 16f);
                    float g = Random.Range(0.3f,0.7f);

                    noise_texture.SetPixel(x, y, new Color(g, g, g, g));
                }
            }
            noise_texture.Apply();
            var bytes = noise_texture.EncodeToPNG();
            File.WriteAllBytes(Application.streamingAssetsPath + "/noise.png", bytes);
        }

        private void Mapgen_onMapGenerated()
        {
            texture = new Texture2D(width, height);
            texture.filterMode = FilterMode.Point;
            rawImage.texture = texture;
            
            Clear(ColorTool.GetColor(Biomes.Ocean));

            //DrawBiome();
            List<Vector2> path;
            CellEdge edge;
            Color color;


            void DrawPath(CellCenter center)
            {
                path = new List<Vector2>();
                path.AddRange(mapgen.noisyEdges.path[edge.index]);
                path.Add(center.position);
                DrawPolygon(path.ToArray(), color);
            }

            foreach (var center in mapgen.cells)
            {
                foreach (var neighbor in center.neighborCells)
                {
                    edge = mapgen.LookupEdgeFromCenter(center, neighbor);
                    if (mapgen.noisyEdges.path[edge.index] == null)
                        continue;
                    color = ColorTool.GetColor(center.biome);
                    //if (center.biome == Biomes.Ocean)
                    //{
                    //    Color coast = ColorTool.GetColor(Biomes.Coast);
                    //    float nx = edge.midPosition.x / width * 4f;
                    //    float ny = edge.midPosition.y / height * 4f;

                    //    float e = Mathf.PerlinNoise( nx,ny) + 
                    //                    Mathf.PerlinNoise(nx*2 ,ny * 2) * 0.5f + 
                    //                    Mathf.PerlinNoise(nx*4 ,ny * 4) * 0.25f+
                    //                    Mathf.PerlinNoise(nx *8, ny * 8) * 0.125f;
                    //    e /= (1f + 0.5f + 0.25f + 0.125f);

                    //    color = Color.Lerp(color, coast, e);
                    //}
                    color = ColorWithSlope(color, center, neighbor, edge);

                    DrawPath(center);
                }
            }

            RenderEdges();
            // RenderRoads();
            // DrawElevation();

            #region Test

            //DrawCells();

            #endregion

            texture.Apply();
            var bytes = texture.EncodeToPNG();
            File.WriteAllBytes(Application.streamingAssetsPath + "/map.png", bytes);


            Texture2D result = new Texture2D(width,height);
            result.filterMode = FilterMode.Point;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color new_ = texture.GetPixel(x, y);
                    Color base_ = noise_texture.GetPixel(x, y);
                    Color result_ = new Color(0, 0, 0, 1);
                    if (new_.r > 0.5f)
                        result_.r = 1f - 2f * (1f - new_.r) * (1f - base_.r);
                    else
                        result_.r = 2f * new_.r * base_.r;

                    if (new_.b > 0.5f)
                        result_.b = 1f - 2f * (1f - new_.b) * (1f - base_.b);
                    else
                        result_.b = 2f * new_.b * base_.b;

                    if (new_.g > 0.5f)
                        result_.g = 1f - 2f * (1f - new_.g) * (1f - base_.g);
                    else
                        result_.g = 2f * new_.g * base_.g;

                    result.SetPixel(x,y,result_);
                }
            }

            var result_bytes = result.EncodeToPNG();
            File.WriteAllBytes(Application.streamingAssetsPath+"/result.png", result_bytes);

          //  cs.SetTexture(0, "inputTextureA", noise_texture);
          //  cs.SetTexture(0, "inputTextureB", texture);
          //  cs.SetTexture(0, "Result", output);
          //  cs.Dispatch(0, width / 8, height / 8, 1);

          //var output_bytes  =   output.EncodeToPNG();
          //  File.WriteAllBytes(Application.streamingAssetsPath + "/output.png", output_bytes);
        }



        public Vector3 LightVector = new Vector3(-1, -1, 0);
        public Color a;
        public Color b;

        private Color ColorWithSlope(Color color ,CellCenter p,CellCenter q, CellEdge edge) 
        {
            CellCorner r = edge.v0;
            CellCorner s = edge.v1;
            if (r == null || s == null)
                return ColorTool.GetColor(Biomes.Ocean);
            else if (p.isWater)
                return color;

            if (q != null && p.isWater == q.isWater)
                color = Color.Lerp(color, ColorTool.GetColor(q.biome), 0.4f);

            //Color a, b;
            //ColorUtility.TryParseHtmlString("#333333", out a);
            //ColorUtility.TryParseHtmlString("#FFFFFF", out b);
            var colorLow = Color.Lerp(color, a, 0.7f);
            var colorHigh = Color.Lerp(color, b, 0.3f);
            float light = CalculatLighting(p, r, s);
            if (light < 0.5f) return Color.Lerp(colorLow, color, light * 2f );
            else return Color.Lerp(color, colorHigh, light * 2f -1f );
        }

        private float CalculatLighting(CellCenter p, CellCorner r, CellCorner s)
        {
            float OFFSET = 500;
            var a = new Vector3(p.position.x, p.position.y, p.elevation* OFFSET);
            var b = new Vector3(r.position.x, r.position.y, r.elevation* OFFSET);
            var c = new Vector3(s.position.x, s.position.y, s.elevation* OFFSET);
            var normal = Vector3.Cross(b - a, c - a);
            if (normal.z < 0)
                normal *= -1;
          normal  =   normal.normalized;
            var light = 0.5f + 0.35f * Vector3.Dot(normal, LightVector);

            if(light < 0) light = 0;
            if (light > 1) light = 1;
            return light;
        }

        public void RenderEdges()
        {
            CellEdge edge;
            Color color;
            int size;
            foreach (var p in mapgen.cells)
            {
                foreach (var r in p.neighborCells)
                {
                    edge = mapgen.LookupEdgeFromCenter(p, r);
                    if (mapgen.noisyEdges.path[edge.index] == null)
                        continue;
                    if(p.isOcean != r.isOcean)
                    {
                        //»­º£°¶Ïß
                        size = 2;
                        color = ColorTool.GetColor(Biomes.Coast);
                    }
                    else if(p.isWater != r.isWater && p.biome != Biomes.Ice && r.biome != Biomes.Ice)
                    {
                        //»­ºþ²´
                        size = 1;
                        color = ColorTool.GetColor(Biomes.Lake);
                    }
                    else if(p.isWater || r.isWater)
                    {
                        continue;
                    }
                    else if(edge.waterVolume > 0)
                    {
                        //»­River
                        float sizef = Mathf.Sqrt(edge.waterVolume);
                        if (sizef < 0) sizef = 0;
                        size = Mathf.RoundToInt(sizef);
                        color = ColorTool.GetColor(Biomes.River);
                    }
                    else
                    {
                        //no edge
                        continue;
                    }

                    var path = mapgen.noisyEdges.path[edge.index];
                    for (int i = 0; i < path.Count-1; i++)
                    {
                        DrawLine(path[i], path[i + 1], size, color);
                    }

                }
            }
        }

        private Vector2 NormalTowards(CellEdge e,Vector2 c,float len)
        {
            Vector2 n = new Vector2(-(e.v1.position.y - e.v0.position.y),e.v1.position.x - e.v0.position.x );
            Vector2 d = c - e.midPosition;
            if(n.x * d.x + n.y * d.y < 0)
            {
                n.x = -n.x;
                n.y = -n.y;
            }
            n.Normalize();
            n *= len;
            return n;
        }

        public void RenderRoads()
        {
            var roads = mapgen.roads;
            foreach (var p in mapgen.cells)
            {
                if (roads.roadConnections.ContainsKey(p.index))
                {
                    var edges = p.borderEdges;
                    for (int i = 0; i < edges.Count; i++)
                    {
                        var edge1 = edges[i];
                        if (roads.road.ContainsKey(edge1.index) &&
                            roads.road[edge1.index] > 0)
                        {
                            for (int j = 0; j < edges.Count; j++)
                            {
                                var edge2 = edges[j];
                                if (roads.road.ContainsKey(edge2.index) && roads.road[edge2.index] > 0)
                                {
                                    float d = 0.5f * Mathf.Min(Vector2.Distance(edge1.midPosition, p.position),
                                                                             Vector2.Distance(edge2.midPosition, p.position));
                                    Vector2 A = NormalTowards(edge1, p.position, d) + (edge1.midPosition);
                                    Vector2 B = NormalTowards(edge2, p.position, d) + (edge2.midPosition);
                                    Vector2 C = Vector2.Lerp(A, B, 0.5f);

                                    int size = 1;
                                    
                                    Color color = ColorTool.GetColor("ROAD" + roads.road[edge1.index]);
                                    DrawLine(edge1.midPosition, A,size,color);
                                    DrawLine(A, C,size,color);
                                     color = ColorTool.GetColor("ROAD" + roads.road[edge2.index]);
                                    DrawLine(B, edge1.midPosition, size,color);
                                }
                            }
                        }
                    }
                }
            }
        }


        #region Draw
        
        public List<Vector2> SortAtan2(List<Vector2> list)
        {
            for(int j = 0; j < list.Count; j++)
            {
                var point = list[j];
                int x = (int)((point.x / mapgen.size.x) * width);
                int y = (int)((point.y / mapgen.size.y) * height);
                list[j] = new Vector2(x,y);
            }

            Vector2 center = new Vector2();
            for (int j = 0; j < list.Count; j++)
                center += list[j];
            center /= list.Count;

            list = list.OrderBy(p => Mathf.Atan2(p.y - center.y, p.x - center.x)).ToList();
            return list;
        }
        
        public void DrawBiome()
        {

            for(int i=0;i<mapgen.cells.Count;i++)
            {
                var cell = mapgen.cells[i];
                var list = GetPolygon(mapgen.cells[i]);
                DrawPolygon(list, ColorTool.GetColor(cell.biome));
            }
        }
        Vector2[] GetPolygon(CellCenter cell)
        {
            Vector2[] list = new Vector2[cell.cellCorners.Count];
            for (int j = 0; j < cell.cellCorners.Count; j++)
            {
                var point = cell.cellCorners[j].position;
                int x = (int)((point.x / mapgen.size.x) * width);
                int y = (int)((point.y / mapgen.size.y) * height);
                list[j] = new Vector2(x, y);
            }

            Vector2 center = new Vector2();
            for (int j = 0; j < list.Length; j++)
            {
                center += list[j];
            }
            center /= list.Length;

            list = list.OrderBy(p => Mathf.Atan2(p.y - center.y, p.x - center.x))
                .ToArray();
            ;
            return list;
        }

        public void DrawCells()
        {

            for (int i = 0; i < mapgen.cells.Count; i++)
            {
                var list = GetPolygon(mapgen.cells[i]);
                var t = Random.value;
                Color color = Color.Lerp(Color.black,Color.white, t);

                DrawPolygon(list, color);
            }
        }

        public void DrawMoisture()
        {
            float minMositure = mapgen.cells.Min(x => x.moisture);
            float maxMositure = mapgen.cells.Max(x => x.moisture);

            for (int i = 0; i < mapgen.cells.Count; i++)
            {
                var list = GetPolygon(mapgen.cells[i]);
                var mositure = mapgen.cells[i].moisture;
                var t = (mositure - minMositure) / (maxMositure - minMositure);
                Color color = Color.Lerp(Color.white, Color.blue, t);

                DrawPolygon(list, color);
            }
        }

        public void DrawElevation()
        {
            float minElevation = mapgen.cells.Min(x => x.elevation);
            float maxElevation = mapgen.cells.Max(x => x.elevation);

            for (int i = 0; i < mapgen.cells.Count; i++)
            {
                var list = GetPolygon(mapgen.cells[i]);
                var elevation = mapgen.cells[i].elevation;
                var t = (elevation - minElevation) / (maxElevation - minElevation);
                Color color = Color.Lerp(Color.black, Color.white, t);

                DrawPolygon(list, color);
            }
        }

        #endregion

        #region Graphics Tool


        public void DrawLine(Vector2 start, Vector2 end,int size, Color color)
        {
            var points = new List<Vector2Int>();
            var N = Vector2.Distance(start, end);
            for (int step = 0; step <=N ; step++)
            {
                var t = N == 0f ? 0f : step / N;
                var v = Vector2.Lerp(start,end, t);
                points.Add(new Vector2Int(Mathf.RoundToInt(v.x),Mathf.RoundToInt(v.y)));
            }

            for(int i=0;i<points.Count;i++)
            {
                DrawCircle(points[i], size, color);
            }
        }

        public void DrawPolygon(Vector2[] vertices, Color color)
        {
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].x > maxX)
                    maxX = (int)vertices[i].x;
                if (vertices[i].y > maxY)
                    maxY = (int)vertices[i].y;
                if (vertices[i].x < minX)
                    minX = (int)vertices[i].x;
                if (vertices[i].y < minY)
                    minY = (int)vertices[i].y;

            }

            for (int xx = minX; xx <= maxX; xx++)
            {
                for (int yy = minY; yy <= maxY; yy++)
                {
                    if (xx < 0 || yy < 0 || xx >= width || yy >= height)
                        continue;

                    if (IsPointInPolygon(new Vector2(xx, yy), vertices))
                    {
                        texture.SetPixel(xx, yy, color);
                    }
                }
            }
        }

        private bool IsPointInPolygon(Vector2 point, Vector2[] polygonVertices)
        {
            bool isInside = false;
            int j = polygonVertices.Length - 1;

            for (int i = 0; i < polygonVertices.Length; i++)
            {
                if ((polygonVertices[i].y < point.y && polygonVertices[j].y >= point.y
                    || polygonVertices[j].y < point.y && polygonVertices[i].y >= point.y)
                    && (polygonVertices[i].x <= point.x || polygonVertices[j].x <= point.x))
                {
                    if (polygonVertices[i].x + (point.y - polygonVertices[i].y) / (polygonVertices[j].y - polygonVertices[i].y) * (polygonVertices[j].x - polygonVertices[i].x) < point.x)
                    {
                        isInside = !isInside;
                    }
                }

                j = i;
            }

            return isInside;
        }

        public void DrawCircle(Vector2 pos, int radius, Color color)
        {
            int x = Mathf.RoundToInt(pos.x);
            int y = Mathf.RoundToInt(pos.y);
            int xx, yy;
            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    xx = x + i;
                    yy = y + j;
                    if (xx < 0 || yy < 0 || xx >= width || yy >= height)
                        continue;
                    if (i * i + j * j <= radius * radius)
                    {
                        texture.SetPixel(xx, yy, color);
                    }
                }
            }
        }

        private void Clear(Color color)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }

        #endregion
    }
}