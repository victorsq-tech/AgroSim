using UnityEngine;
namespace AgroSim.UI
{
    // Serialized references keep the imported art available in player builds too.
    public sealed class CozyFarmEnvironmentSettings : ScriptableObject
    {
        public Sprite grass;
        public GameObject[] trees;
    }
}
