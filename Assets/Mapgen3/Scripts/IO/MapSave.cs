using Marisa.Maps.Tools;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Marisa.Maps.IO
{
    public class MapSave : MonoBehaviour
    {
        public Mapgen3 map;

        public void Start()
        {
                Debug.Log("!");
            map.onMapGenerated += () =>
            {
                string path = Path.Combine(Application.streamingAssetsPath, "Save", "map_" + map.seed + ".dat");
                Save(path);
                Debug.Log("!!");
            };
            map.Generate();

        
        }

        public void Save(string path)
        {
            string directoryName = Path.GetDirectoryName(path);
            if(!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            if(!File.Exists(path)) 
                File.Create(path);

            StreamWriter writer = new StreamWriter(path,true,Encoding.UTF8);
            writer.WriteLine(map.seed);
            writer.WriteLine(map.size.x);
            writer.WriteLine(map.size.y);
            writer.WriteLine(map.relaxation);
            writer.WriteLine(map.pointNumber);
   



            writer.Close();
        }

    }
}