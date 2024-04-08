using Marisa.Maps.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Marisa.Maps.Enums
{
    public enum Biomes
    {
        //Features
        Ocean,                          // 海洋
        Coast,   //海岸
        LakeShore,
        Lake,  // 湖泊
        River, //河流
        Marsh,   // 沼泽
        Ice,   // 冰川
        Beach,   // 沙滩
        Road1,
        Road2,
        Road3,
        Bridge,
        Lava, //熔岩


        Snow,                           // 雪地
        Tundra,                         // 冻原
        Bare,                           // 裸地
        Scorched,                       // 焦土
        Taiga,                          // 针叶林
        Shrubland,                      // 灌木地
        Temperate_Desert,               // 温带沙漠
        Temperate_Rain_Forest,          // 温带雨林
        Temperate_Deciduous_Forest,     // 温带落叶林
        Grassland,                      // 草原
        Tropical_Rain_Forest,           // 热带雨林
        Tropical_Seasonal_Forest,       // 热带季节性林
        Subtropical_Desert              // 亚热带沙漠
    }

    public static class ColorTool
    {
        public static Dictionary<string, int> BiomesHtmlString = new Dictionary<string, int>
    {
        // Features
        {"OCEAN", 0x44447a},
        {"COAST", 0x33335a},
        {"LAKESHORE", 0x225588},
        {"LAKE", 0x336699},
        {"RIVER", 0x225588},
        {"MARSH", 0x2f6666},
        {"ICE", 0x99ffff},
        {"BEACH", 0xa09077},
        {"ROAD1", 0x442211},
        {"ROAD2", 0x553322},
        {"ROAD3", 0x664433},
        {"BRIDGE", 0x686860},
        {"LAVA", 0xcc3333},

        // Terrain
        {"SNOW", 0xffffff},
        {"TUNDRA", 0xbbbbaa},
        {"BARE", 0x888888},
        {"SCORCHED", 0x555555},
        {"TAIGA", 0x99aa77},
        {"SHRUBLAND", 0x889977},
        {"TEMPERATE_DESERT", 0xc9d29b},
        {"TEMPERATE_RAIN_FOREST", 0x448855},
        {"TEMPERATE_DECIDUOUS_FOREST", 0x679459},
        {"GRASSLAND", 0x88aa55},
        {"SUBTROPICAL_DESERT", 0xd2b98b},
        {"TROPICAL_RAIN_FOREST", 0x337755},
        {"TROPICAL_SEASONAL_FOREST", 0x559944},
        };

        public static Color GetColor(Biomes biome)
        {
            string key = biome.ToString().ToUpper();
            Color color = Color.black;
            string str = "#" + BiomesHtmlString[key].ToString("X");
            ColorUtility.TryParseHtmlString(str, out color);
            return color;
        }

        public static Color GetColor(string key)
        {
            Color color = Color.black;
            string str = "#" + BiomesHtmlString[key].ToString("X");
            ColorUtility.TryParseHtmlString(str, out color);
            return color;
        }

    }


}