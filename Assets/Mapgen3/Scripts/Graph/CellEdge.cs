using UnityEngine;

namespace Marisa.Maps.Graph
{
    public class CellEdge
    {
        public int index;
        public CellCenter d0 = new CellCenter();
        public CellCenter d1 = new CellCenter();

        public CellCorner v0 = new CellCorner();
        public CellCorner v1 = new CellCorner();

        public int waterVolume;
        public Vector2 midPosition; //v0 和 v1的中间点
    }
}
