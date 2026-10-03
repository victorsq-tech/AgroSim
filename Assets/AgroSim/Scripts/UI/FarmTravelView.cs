using UnityEngine;
namespace AgroSim.UI
{
    // Runtime presentation: preserve the original scene and its serialized assets.
    public sealed class FarmTravelView
    {
        private readonly Camera camera;
        private readonly GameObject roadRoot;
        private readonly SpriteRenderer ground,field;
        private bool onRoad,preview;
        private int width,height;
        public static readonly Vector3 RoadStart=new Vector3(25,-.4f,0), RoadEnd=new Vector3(35,-.4f,0);
        public FarmTravelView(Transform parent)
        {
            camera=Camera.main;
            ground=GameObject.Find("FarmGround")?.GetComponent<SpriteRenderer>();
            field=GameObject.Find("Field01")?.GetComponent<SpriteRenderer>();
            foreach(string name in new[]{"Road","Garage","GarageRoof"}) { var r=GameObject.Find(name)?.GetComponent<SpriteRenderer>(); if(r!=null) r.enabled=false; }
            if(ground!=null) { ground.transform.position=new Vector3(3,0,0); ground.size=new Vector2(24,10); }
            if(field!=null) { field.transform.position=new Vector3(3,0,0); field.size=new Vector2(16.4f,5.9f); }
            roadRoot=new GameObject("GarageToFieldRoad"); roadRoot.transform.SetParent(parent,false);
            var sprite=ground!=null ? ground.sprite : field?.sprite;
            Block(sprite,"RoadVerge",new Vector3(30,0),new Vector2(14,8),new Color(.3f,.43f,.23f),-10);
            Block(sprite,"AccessRoad",new Vector3(30,-.4f),new Vector2(12,1.8f),new Color(.64f,.53f,.36f),-5);
            Block(sprite,"GarageDestination",new Vector3(24,1),new Vector2(1.5f,2.4f),new Color(.4f,.43f,.39f),-4);
            Block(sprite,"FieldEntrance",new Vector3(36,1),new Vector2(1.5f,2.4f),new Color(.37f,.25f,.13f),-4);
            for(int i=0;i<9;i++) Block(sprite,"Tree"+i,new Vector3(25+i*1.25f,2.5f),new Vector2(.45f,.6f),new Color(.12f,.29f,.15f),-3);
            ShowField();
        }
        void Block(Sprite sprite,string name,Vector3 position,Vector2 size,Color color,int order)
        {
            var go=new GameObject(name); go.transform.SetParent(roadRoot.transform,false); go.transform.position=position;
            var r=go.AddComponent<SpriteRenderer>(); r.sprite=sprite; r.drawMode=SpriteDrawMode.Sliced; r.size=size; r.color=color; r.sortingOrder=order;
        }
        public void ShowRoad() { onRoad=true; preview=false; roadRoot.SetActive(true); if(ground!=null) ground.enabled=false; if(field!=null) field.enabled=false; Frame(new Vector3(30,0,-10),6.4f,3f,1); }
        public void ShowField(bool preview=false) { onRoad=false; this.preview=preview; roadRoot.SetActive(false); if(ground!=null) ground.enabled=true; if(field!=null) field.enabled=true; Frame(new Vector3(3,0,-10),8.8f,3.25f,preview ? .825f : 1); }
        public void RefreshFrame() { if(width==Screen.width && height==Screen.height) return; if(onRoad) ShowRoad(); else ShowField(preview); }
        public static Vector2 CalculateFrame(float aspect,float horizontal,float vertical,float bottom,float top)
        {
            float usable=top-bottom;
            float size=Mathf.Max(vertical/usable,horizontal/Mathf.Max(.1f,aspect));
            return new Vector2(size,(.5f-(top+bottom)/2)*2*size);
        }
        void Frame(Vector3 position,float horizontal,float vertical,float top)
        {
            if(camera==null) return;
            width=Screen.width; height=Screen.height;
            // Render the complete frame, including underneath the HUD, to clear old UI pixels.
            camera.rect=new Rect(0,0,1,1);
            var frame=CalculateFrame((float)width/Mathf.Max(1,height),horizontal,vertical,.2f,top);
            position.y+=frame.y; camera.transform.position=position; camera.orthographicSize=frame.x;
        }
    }
}
