using UnityEngine;

namespace Marisa.Maps.Graph
{
    public class MapPoint  //地图站点
    {
        public int index;
        public Vector2 position;
        public bool isBorder;
        public bool isWater;  //水
        public bool isOcean;  //海洋
        public bool isCoast; //海岸
        public float elevation; // Normalized
        public float moisture; //Normalized
        public int islandID;
    }
}
