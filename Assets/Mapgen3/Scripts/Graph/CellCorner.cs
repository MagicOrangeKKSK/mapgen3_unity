using System.Collections.Generic;

namespace Marisa.Maps.Graph
{
    public class CellCorner : MapPoint
    {
        public int river;
        public CellCorner watershed;
        public int watershedSize;

        public CellCorner downslopeCorner; //基于海拔的相邻最低角落
        public CellEdge downslopeEdge; //连接此角落到下坡的的边缘


        public List<CellCorner> neighborCorners = new List<CellCorner>(); //相邻的角
        public List<CellEdge> connectedEdges = new List<CellEdge>(); //与该角连接的边
        public List<CellCenter> touchingCells = new List<CellCenter>(); //
    }
}
