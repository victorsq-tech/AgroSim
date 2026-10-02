using UnityEngine;

namespace AgroSim.Tractors
{
    [CreateAssetMenu(fileName = "TractorData", menuName = "AgroSim/Tractor Data")]
    public sealed class TractorData : ScriptableObject
    {
        public string tractorId = "TR-01";
        public string displayName = "Trator TR-01";
        [Min(0f)] public float fuelCapacityLiters = 100f;
        [Min(0f)] public float fuelConsumptionLitersPerHour = 10f;
    }
}
