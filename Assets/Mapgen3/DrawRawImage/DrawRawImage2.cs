using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawRawImage2 : MonoBehaviour
{
    public RawImage rawImage;

    public int width = 800;
    public int height = 800;

    private Texture2D texture;

    private void Start()
    {
        texture = new Texture2D(width,height);
        texture.filterMode = FilterMode.Point;
        rawImage.texture = texture;

        Clear(Color.blue);
        DrawCircle(new Vector2Int(400, 400), 30, Color.red);
        Vector2[] polygonVertices = new Vector2[]
       {
            new Vector2(50, 50),
            new Vector2(100, 100),
            new Vector2(150, 50),
            new Vector2(100, 10),
            new Vector2(600, 10),
       };
        DrawPolygon(polygonVertices, Color.green);
        texture.Apply();
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

    private bool IsPointInPolygon(Vector2 point,Vector2[] polygonVertices)
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

    public void DrawCircle(Vector2Int pos,int radius ,Color color)
    {
        int xx, yy;
        for(int i = -radius; i <= radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                xx = pos.x + i;
                yy = pos.y + j;
                if (xx < 0 || yy < 0 || xx >= width || yy >= height)
                    continue;
                if( i * i + j * j <= radius * radius)
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



}
