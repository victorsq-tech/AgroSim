using UnityEngine;

namespace AgroSim.Farm
{
    [CreateAssetMenu(fileName = "FarmData", menuName = "AgroSim/Farm Data")]
    public sealed class FarmData : ScriptableObject
    {
        public string farmId = "FARM-01";
        public string displayName = "Fazenda AgroSim";
        [Min(0f)] public float fieldAreaHectares = 1f;
    }
}
