using UnityEngine;
namespace AgroSim.UI
{
    // Runtime presentation: preserve the original scene and its serialized assets.
    public sealed class FarmTravelView
    {
        private readonly Camera camera;
        private readonly GameObject roadRoot;
        private readonly GameObject fieldTrees;
        private readonly CozyFarmEnvironmentSettings art;
        private readonly FarmArtwork artwork=new FarmArtwork();
        private Sprite tiledGrass;
        private readonly SpriteRenderer ground,field;
        private bool onRoad,preview;
        private int width,height;
        public static readonly Vector3 RoadStart=new Vector3(25,-.4f,0), RoadEnd=new Vector3(35,-.4f,0);
        public FarmTravelView(Transform parent)
        {
            camera=Camera.main;
            art=Resources.Load<CozyFarmEnvironmentSettings>("CozyFarmEnvironment");
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
            Block(sprite,"FieldEntrance",new Vector3(36,1.4f),new Vector2(1.5f,1.2f),new Color(.37f,.25f,.13f),-4);
            for(int i=0;i<4;i++) Block(sprite,"EntranceFurrow"+i,new Vector3(36,1+i*.25f),new Vector2(1.3f,.045f),new Color(.22f,.13f,.075f),-3);
            if(art?.road!=null)
            {
                var road=roadRoot.transform.Find("AccessRoad").GetComponent<SpriteRenderer>();
                road.sprite=artwork.TileSprite(art.road); road.color=Color.white;
                road.drawMode=SpriteDrawMode.Tiled; road.size=new Vector2(12,1.8f);
            }
            var house=artwork.House(0);
            if(house!=null)
            {
                var building=roadRoot.transform.Find("GarageDestination").GetComponent<SpriteRenderer>();
                building.sprite=house; building.color=Color.white; building.drawMode=SpriteDrawMode.Simple;
                building.transform.position=new Vector3(24,1.3f,0);
                var bounds=house.bounds.size;
                building.transform.localScale=Vector3.one*Mathf.Min(2f/bounds.x,1.65f/bounds.y);
            }
            for(int i=0;i<4;i++) Block(sprite,"RoadEdgeMarker"+i,new Vector3(25.5f+i*3,.65f),new Vector2(.055f,.22f),FarmTheme.Ink,-1);
            Block(sprite,"FieldGatePost",new Vector3(35.75f,.7f),new Vector2(.12f,.4f),new Color(.4f,.28f,.16f),-1);
            Block(sprite,"FieldGatePostLower",new Vector3(35.75f,-1.5f),new Vector2(.12f,.4f),new Color(.4f,.28f,.16f),-1);
            fieldTrees=new GameObject("CozyFarmFieldTrees"); fieldTrees.transform.SetParent(parent,false);
            if(art!=null && art.grass!=null && art.trees!=null && art.trees.Length>0)
            {
                // A full-rect runtime sprite allows tiling without changing the imported sprite mesh.
                var source=art.grass;
                tiledGrass=Sprite.Create(source.texture,source.rect,new Vector2(.5f,.5f),source.pixelsPerUnit,0,SpriteMeshType.FullRect);
                tiledGrass.name="Cozy Farm tiled grass";
                ApplyGrass(ground);
                ApplyGrass(roadRoot.transform.Find("RoadVerge").GetComponent<SpriteRenderer>());
                for(int i=0;i<6;i++) Tree(fieldTrees.transform,new Vector3(i<3 ? -6 : 12,-1.9f+(i%3)*1.7f),i);
                for(int i=0;i<5;i++) Tree(roadRoot.transform,new Vector3(26+i*2,1.8f),i);
            }
            else
            {
                Debug.LogWarning("AgroSim: Cozy Farm settings missing; using environment placeholders.");
                for(int i=0;i<9;i++) Block(sprite,"Tree"+i,new Vector3(25+i*1.25f,2.5f),new Vector2(.45f,.6f),new Color(.12f,.29f,.15f),-3);
            }
            ShowField();
        }
        void ApplyGrass(SpriteRenderer renderer)
        {
            if(renderer==null) return;
            var size=renderer.size; renderer.sprite=tiledGrass; renderer.color=Color.white;
            renderer.drawMode=SpriteDrawMode.Tiled; renderer.tileMode=SpriteTileMode.Continuous; renderer.size=size;
        }
        void Tree(Transform parent,Vector3 position,int variant)
        {
            // Tree_4 is a wide stump; use the four standing-tree variants for the borders.
            var prefab=art.trees[variant%Mathf.Min(4,art.trees.Length)]; if(prefab==null) return;
            var tree=Object.Instantiate(prefab,parent); tree.name="CozyFarmTree_"+variant;
            tree.transform.position=position;
            var renderer=tree.GetComponent<SpriteRenderer>();
            var size=renderer!=null && renderer.sprite!=null ? renderer.sprite.bounds.size : Vector3.one;
            tree.transform.localScale=Vector3.one*Mathf.Min(1.3f/Mathf.Max(.01f,size.y),1.1f/Mathf.Max(.01f,size.x));
            if(renderer!=null) { renderer.color=Color.white; renderer.sortingOrder=-2; }
        }
        public void Dispose() { if(tiledGrass!=null) Object.Destroy(tiledGrass); artwork.Dispose(); }
        void Block(Sprite sprite,string name,Vector3 position,Vector2 size,Color color,int order)
        {
            var go=new GameObject(name); go.transform.SetParent(roadRoot.transform,false); go.transform.position=position;
            var r=go.AddComponent<SpriteRenderer>(); r.sprite=sprite; r.drawMode=SpriteDrawMode.Sliced; r.size=size; r.color=color; r.sortingOrder=order;
        }
        public void ShowRoad() { onRoad=true; preview=false; roadRoot.SetActive(true); fieldTrees.SetActive(false); if(ground!=null) ground.enabled=false; if(field!=null) field.enabled=false; Frame(new Vector3(30,0,-10),7.2f,3f,1); }
        public void ShowField(bool preview=false) { onRoad=false; this.preview=preview; roadRoot.SetActive(false); fieldTrees.SetActive(true); if(ground!=null) ground.enabled=true; if(field!=null) field.enabled=true; Frame(new Vector3(3,0,-10),10f,3.25f,preview ? .825f : 1); }
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
