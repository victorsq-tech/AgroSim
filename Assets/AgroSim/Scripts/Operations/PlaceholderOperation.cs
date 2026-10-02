using UnityEngine;

namespace AgroSim.Operations
{
    public sealed class PlaceholderOperation : MonoBehaviour
    {
        public float Progress { get; private set; }
        public float Fuel { get; private set; } = 100f;
        public float Wear { get; private set; }
        public float Productivity { get; private set; } = 100f;
        public bool EventPending { get; private set; }
        public bool Running { get; private set; }
        private bool eventResolved;

        public void Reset()
        {
            Progress = Wear = 0f;
            Fuel = Productivity = 100f;
            eventResolved = EventPending = Running = false;
        }

        public void Begin()
        {
            Progress = Wear = 0f;
            Fuel = Productivity = 100f;
            eventResolved = EventPending = false;
            Running = true;
        }

        public void Tick(float deltaTime)
        {
            if (!Running || EventPending || deltaTime <= 0f) return;
            deltaTime = Mathf.Min(deltaTime, 30f * (1f - Progress));
            if (!eventResolved) deltaTime = Mathf.Min(deltaTime, Mathf.Max(0f, 30f * (.45f - Progress)));
            Progress = Mathf.Min(1f, Progress + deltaTime / 30f);
            Fuel = Mathf.Max(0f, Fuel - deltaTime * .65f);
            Wear += deltaTime * .12f;
            if (Progress >= .45f - .000001f && !eventResolved) { Progress = .45f; EventPending = true; }
            if (Progress >= 1f - .000001f) { Progress = 1f; Running = false; }
        }

        public void ResolveEvent(bool adjust)
        {
            if (!EventPending) return;
            if (adjust) Fuel = Mathf.Max(0f, Fuel - 3f);
            else { Productivity -= 15f; Wear += 5f; }
            eventResolved = true;
            EventPending = false;
        }
    }
}
