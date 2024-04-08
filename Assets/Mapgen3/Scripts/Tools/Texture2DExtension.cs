using csDelaunay;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using UnityEngine;
using Color = UnityEngine.Color;

namespace Marisa.Maps.Tools
{
    public static class Texture2DExtension
    {
        public static void SaveToPng(string path,Texture2D texture)
        {
            var bytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
        }

        public static Texture2D HardLightMix(int width,int height,Texture2D baseTexture,Texture2D newTexture)
        {
            Texture2D resultTexture = new Texture2D(width,height);
            resultTexture.filterMode= FilterMode.Point;

            Color new_, base_, result_;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    new_ = newTexture.GetPixel(x, y);
                    base_ = baseTexture.GetPixel(x, y);
                    result_ = new Color(0, 0, 0, 1);
                    result_.r = new_.r > 0.5f ? (1f - 2f * (1f - new_.r) * (1f - base_.r)) :
                                                               (2f * new_.r * base_.r);
                    result_.g = new_.g > 0.5f ? (1f - 2f * (1f - new_.g) * (1f - base_.g)) :
                                                         (2f * new_.g * base_.g);
                    result_.b = new_.b > 0.5f ? (1f - 2f * (1f - new_.b) * (1f - base_.b)) :
                                                         (2f * new_.b * base_.b);
                    resultTexture.SetPixel(x,y, result_);
                }
            }
            resultTexture.Apply();
            return resultTexture;
        }

        public static Texture2D CreateTexture(int width,int height)
        {
            Texture2D texture = new Texture2D(width,height);
            texture.filterMode = FilterMode.Point;
            return texture;
        }

        public static Texture2D CreateNoisyMap(int width, int height,int seed,float min = 0.3f,float max = 0.7f)
        {
            Random.InitState(seed);
            Texture2D texture = new Texture2D(width, height);
            texture.filterMode = FilterMode.Point;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float g = Random.Range(min,max);
                    texture.SetPixel(x,y,new Color(g,g,g,g));   
                }
            }
            texture.Apply();
            return texture;
        }

        public static Texture2D DrawLine(this Texture2D texture, Vector2 start,Vector2 end,int size,Color color)
        {
            var points = new List<Vector2Int>();
            var N = Vector2.Distance(start, end);
            for (int step = 0; step <= N; step++)
            {
                var t = N == 0f ? 0f : step / N;
                var v = Vector2.Lerp(start, end, t);
                points.Add(new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y)));
            }

            for (int i = 0; i < points.Count; i++)
            {
                texture.DrawCircle(points[i], size, color);
            }

            return texture;
        }

        public static Texture2D DrawTriangle(this Texture2D texture, Vector2 A, float hA, Vector2 B, float hB, Vector2 C, float hC)
        {
            int minX = Mathf.RoundToInt(Mathf.Min(A.x, B.x, C.x));
            int maxX = Mathf.RoundToInt(Mathf.Max(A.x, B.x, C.x));
            int minY = Mathf.RoundToInt(Mathf.Min(A.y, B.y, C.y));
            int maxY = Mathf.RoundToInt(Mathf.Max(A.y, B.y, C.y));

            float alpha, beta, gamma, t;
            int x, y;
            for (x = minX; x <= maxX; x++)
            {
                for (y = minY; y <= maxY; y++)
                {
                    if (x < 0 || y < 0 || x >= texture.width || y >= texture.height)
                        continue;
                    if (MathfTool.IsPointInTriangle(new Vector2(x, y), A, B, C))
                    {
                        alpha = (-(x - B.x) * (C.y - B.y) + (y - B.y) * (C.x - B.x)) /
                                    (-(A.x - B.x) * (C.y - B.y) + (A.y - B.y) * (C.x - B.x));
                        beta = (-(x - C.x) * (A.y - C.y) + (y - C.y) * (A.x - C.x)) /
                                 (-(B.x - C.x) * (A.y - C.y) + (B.y - C.y) * (A.x - C.x));
                        gamma = 1f - alpha - beta;
                        t = alpha * hA + beta * hB + gamma * hC;
                        texture.SetPixel(x, y, new Color(t, t, t, 1f));
                    }
                }
            }
            return texture;
        }

        public static Texture2D DrawTriangle(this Texture2D texture, Vector2 A, Color cA, Vector2 B, Color cB, Vector2 C, Color cC) 
        {
            int minX = Mathf.RoundToInt(Mathf.Min(A.x, B.x, C.x));
            int maxX = Mathf.RoundToInt(Mathf.Max(A.x, B.x, C.x));
            int minY = Mathf.RoundToInt(Mathf.Min(A.y, B.y, C.y));
            int maxY = Mathf.RoundToInt(Mathf.Max(A.y, B.y, C.y));

            Color c;
            float alpha, beta, gamma;
            int x, y;
            for (x = minX; x <= maxX; x++)
            {
                for (y = minY; y <= maxY; y++)
                {
                    if (x < 0 || y < 0 || x >= texture.width || y >= texture.height)
                        continue;
                    if (MathfTool.IsPointInTriangle(new Vector2(x, y), A, B, C))
                    {
                        alpha = (-(x - B.x) * (C.y - B.y) + (y - B.y) * (C.x - B.x)) /
                                    (-(A.x - B.x) * (C.y - B.y) + (A.y - B.y) * (C.x - B.x));
                        beta = (-(x - C.x) * (A.y - C.y) + (y - C.y) * (A.x - C.x)) /
                                 (-(B.x - C.x) * (A.y - C.y) + (B.y - C.y) * (A.x - C.x));
                        gamma = 1f - alpha - beta;
                        c = alpha * cA + beta * cB + gamma * cC;
                        texture.SetPixel(x, y, c);
                    }
                }
            }
            return texture;
        }


        public static Texture2D DrawTriangle(this Texture2D texture ,List<Vector2> triangle, Color color)
        {
            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;
            foreach (var pos in triangle)
            {
                int x = Mathf.RoundToInt(pos.x);
                int y = Mathf.RoundToInt(pos.y);
                if (x > maxX)
                    maxX = x;
                if (x < minX)
                    minX = x;
                if (y > maxY)
                    maxY = y;
                if (y < minY)
                    minY = y;
            }
            for (int xx = minX; xx <= maxX; xx++)
            {
                for (int yy = minY; yy <= maxY; yy++)
                {
                    if (xx < 0 || yy < 0 || xx >= texture.width || yy >= texture.height)
                        continue;
                    if (MathfTool.IsPointInTriangle(new Vector2(xx, yy), triangle[0], triangle[1], triangle[2]))
                    {
                        texture.SetPixel(xx, yy, color);
                    }
                }
            }
            return texture;
        }

        public static Texture2D DrawPolygon(this Texture2D texture, List<Vector2> vertices, Color color)
        {
            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;
            foreach (var pos in vertices)
            {
                int x = Mathf.RoundToInt(pos.x);
                int y = Mathf.RoundToInt(pos.y);
                if (x > maxX)
                    maxX = x;
                if (x < minX)
                    minX = x;
                if (y > maxY)
                    maxY = y;
                if (y < minY)
                    minY = y;
            }

            for(int xx = minX; xx <= maxX; xx++)
            {
                for(int yy = minY;yy <= maxY; yy++)
                {
                    if (xx < 0 || yy < 0 || xx >= texture.width || yy >= texture.height)
                        continue;
                    if(MathfTool.IsPointInPolygon(new Vector2(xx, yy), vertices))
                    {
                        texture.SetPixel(xx,yy, color);
                    }
                }
            }
            return texture;
        }

     

        public static Texture2D DrawCircle(this Texture2D texture, Vector2 pos, int radius, Color color)
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
                    if (xx < 0 || yy < 0 || xx > texture.width || yy > texture.height)
                        continue;
                    if(i * i + j * j <= radius * radius)
                        texture.SetPixel(xx, yy, color);
                }
            }
            return texture;
        }

        public static Texture2D Clear(this Texture2D texture, Color color)
        {
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    texture.SetPixel(x, y, color);
                }
            }
            return texture;
        }
    }
}