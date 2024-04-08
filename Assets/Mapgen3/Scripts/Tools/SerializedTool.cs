using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Marisa.Maps.Tools
{
    public static class SerializedTool 
    {
        public static string Vector2ToStr(Vector2 v)
        {
            return v.x + "," + v.y;
        }

        public static Vector2 StrToVector2(string line)
        {
            if(line.Contains(","))
            {
                var p = line.Split(',');
                return new Vector2(float.Parse(p[0]), float.Parse(p[1]));
            }
            return Vector2.zero;
        }

    }
}