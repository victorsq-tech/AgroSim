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
        private float startingProductivity=100f;
        public void Configure(float productivity)
        { if(!Running) { startingProductivity=Mathf.Clamp(productivity,0,100); Reset(); } }
        public const float FuelCapacity = 100f;
        public const float RequiredFuel = 22.5f;
        public bool CanOperate => Fuel >= RequiredFuel && Wear <= 91.4f;

        public void Refuel() { if(!Running) Fuel=FuelCapacity; }
        public void Repair() { if(!Running) Wear=0f; }
        public void RestoreMachine(float fuel,float wear)
        {
            if(Running) return;
            Fuel=Mathf.Clamp(fuel,0f,FuelCapacity); Wear=Mathf.Clamp(wear,0f,100f);
        }

        public void Reset()
        {
            Progress = 0f;
            Productivity = startingProductivity;
            eventResolved = EventPending = Running = false;
        }

        public void Begin()
        {
            Reset();
            Running = CanOperate;
        }

        public void Tick(float deltaTime)
        {
            if (!Running || EventPending || deltaTime <= 0f) return;
            deltaTime = Mathf.Min(deltaTime, 30f * (1f - Progress));
            if (!eventResolved) deltaTime = Mathf.Min(deltaTime, Mathf.Max(0f, 30f * (.45f - Progress)));
            deltaTime = Mathf.Min(deltaTime,Fuel/.65f,(100f-Wear)/.12f);
            Progress = Mathf.Min(1f, Progress + deltaTime / 30f);
            Fuel = Mathf.Max(0f, Fuel - deltaTime * .65f);
            Wear += deltaTime * .12f;
            if (Progress >= .45f - .000001f && !eventResolved) { Progress = .45f; EventPending = true; }
            if (Progress >= 1f - .000001f) { Progress = 1f; Running = false; }
            if(Fuel<=.000001f || Wear>=100f) { Running=false; EventPending=false; }
        }

        public void ResolveEvent(bool adjust)
        {
            if (!EventPending) return;
            if (adjust) Fuel = Mathf.Max(0f, Fuel - 3f);
            else { Productivity = Mathf.Max(0,Productivity-15f); Wear = Mathf.Min(100f,Wear+5f); }
            eventResolved = true;
            EventPending = false;
        }
    }
}
