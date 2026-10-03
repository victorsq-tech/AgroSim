using UnityEngine;
namespace AgroSim.Core
{
    public sealed class GarageManeuver
    {
        public static readonly Vector2 Parking=new Vector2(.12f,.76f), Hitch=new Vector2(.64f,.42f), Door=new Vector2(.5f,.22f);
        public Vector2 Position { get; private set; }=Parking;
        public bool Attached { get; private set; }
        public bool Ready { get; private set; }
        public bool Busy=>phase!=0;
        public bool IsStoring=>Busy && !attaching;
        private int phase;
        private bool attaching;
        private float elapsed;
        private Vector2 origin;
        private Vector2 hitchPoint=Hitch;
        public void SelectTool(int index) { if(!Busy && !Attached) hitchPoint=new Vector2(.6825f+(index%3)*.115f,index<3 ? .77f : .47f); }
        public bool Request(bool attach)
        {
            if(Busy || attach==Attached) return false;
            attaching=attach; origin=Position; elapsed=0; phase=1; Ready=false; return true;
        }
        public void Tick(float dt)
        {
            float remaining=Mathf.Max(0,dt);
            while(Busy && remaining>0)
            {
                float step=Mathf.Min(remaining,2-elapsed); elapsed+=step; remaining-=step;
                var target=phase==1 ? hitchPoint : attaching ? Door : Parking;
                Position=Vector2.Lerp(origin,target,elapsed/2);
                if(elapsed<2) break;
                if(phase==1) { Attached=attaching; phase=2; origin=Position; elapsed=0; }
                else { phase=0; Ready=Attached; }
            }
        }
        public void Reset() { phase=0; elapsed=0; Attached=Ready=false; Position=Parking; }
    }
}
