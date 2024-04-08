using Marisa.Maps.Graph;
using System.Collections.Generic;

namespace Marisa.Maps.Extension
{
    public class WaterSheds 
    {
        Dictionary<int, int> lowestCorner = new Dictionary<int, int>(); //polygon index -> corner index
        Dictionary<int,int> watersheds = new Dictionary<int, int>(); //polygon index -> corner index

       public void CreateWatersheds(Mapgen3 map)
        {
            foreach (var p in map.corners)
            {
                CellCorner  s = null;
                foreach (var q in p.neighborCorners)
                {
                     if(s == null || q.elevation <= s.elevation)
                        s = q;
                }
                lowestCorner[p.index] = (s == null) ? -1 : s.index;
                watersheds[p.index] = (s == null) ? -1 : (s.watershed == null) ? -1 : s.watershed.index;
            }
        }
    }
}