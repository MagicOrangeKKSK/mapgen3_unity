using Marisa.Maps.Graph;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Marisa.Maps.Extension
{
    public class Rivers
    {
        public Dictionary<int, int> lowestCells = new Dictionary<int, int>(); //cell index -> cell index
        public Dictionary<int,int> waterVolumes = new Dictionary<int, int>(); //cell index -> int water volume
        
        public List<int> riverSources = new List<int>();

        public void CreateRivers(Mapgen3 map,int riverCount)
        {
            riverSources.Clear();

            foreach (var p in map.cells)
            {
                CellCenter s = null;
                AddWaterVolumes(p.index);

                if (p.isOcean)
                    continue;

                foreach (var q in p.neighborCells)
                {
                    if (s == null || q.elevation <= s.elevation)
                    {
                        s = q;
                    }

                    if (s != null)
                    {
                        AddWaterVolumes(s.index);
                        lowestCells[p.index] = s.index;
                    }
                }
            }

            while(riverSources.Count < riverCount)
            {
                var p = map.cells[Random.Range(0, map.cells.Count)];
                if (p.isOcean || p.elevation < 0.3f || p.elevation > 0.9f)
                    continue;
                riverSources.Add(p.index);
            }
        }


        private void AddWaterVolumes(int index)
        {
            if(!waterVolumes.ContainsKey(index))
                waterVolumes[index] = 0;
            waterVolumes[index]++;
        }

    }
}