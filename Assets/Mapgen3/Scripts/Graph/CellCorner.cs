using System.Collections.Generic;

namespace Marisa.Maps.Graph
{
    public class CellCorner : MapPoint
    {
        public int river;
        public CellCorner watershed;
        public int watershedSize;

        public CellCorner downslopeCorner; //���ں��ε�������ͽ���
        public CellEdge downslopeEdge; //���Ӵ˽��䵽���µĵı�Ե


        public List<CellCorner> neighborCorners = new List<CellCorner>(); //���ڵĽ�
        public List<CellEdge> connectedEdges = new List<CellEdge>(); //��ý����ӵı�
        public List<CellCenter> touchingCells = new List<CellCenter>(); //
    }
}
