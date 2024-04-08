using UnityEngine;

namespace Marisa.Maps.Shapes
{
    [CreateAssetMenu(menuName = "Marisa/Map Shape/Square Shape")]
    public class IslandShape : ScriptableObject
    {
        public virtual bool IsPointInsideShape(Vector2 point, Vector2 mapSize, int seed = 0)
        {
            Random.InitState(seed);
            return true;
        }
    }

}

 