
using UnityEngine;

/// <summary>
/// Serializeable Vector3 class
/// </summary>
/// 

namespace MSPlayground.Core
{
    public class SerializableV3
    {
        public float x, y, z;

        public SerializableV3(Vector3 v3)
        {
            x = v3.x;
            y = v3.y;
            z = v3.z;
        }

        public Vector3 ToV3()
        {
            return new Vector3(x, y, z);
        }
    }
}
