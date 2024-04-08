using Marisa.Maps.Enums;
using System.Collections.Generic;
namespace Marisa.Maps.Graph
{
    public class CellCenter : MapPoint
    {
        public Biomes biome;

        public List<CellCenter> neighborCells = new List<CellCenter>(); //相邻的单元
        public List<CellEdge> borderEdges = new List<CellEdge>();    //边
        public List<CellCorner> cellCorners = new List<CellCorner>(); //角
    }
}