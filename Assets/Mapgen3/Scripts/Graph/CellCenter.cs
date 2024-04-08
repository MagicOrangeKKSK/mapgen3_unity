using Marisa.Maps.Enums;
using System.Collections.Generic;
namespace Marisa.Maps.Graph
{
    public class CellCenter : MapPoint
    {
        public Biomes biome;

        public List<CellCenter> neighborCells = new List<CellCenter>(); //���ڵĵ�Ԫ
        public List<CellEdge> borderEdges = new List<CellEdge>();    //��
        public List<CellCorner> cellCorners = new List<CellCorner>(); //��
    }
}