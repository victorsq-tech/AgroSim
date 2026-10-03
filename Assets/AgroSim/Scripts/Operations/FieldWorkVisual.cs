using UnityEngine;

namespace AgroSim.Operations
{
    // Small reusable cell grid. Coverage follows the same alternating passes as the tractor.
    public sealed class FieldWorkVisual : MonoBehaviour
    {
        public const int Lanes=4, Columns=24;
        private SpriteRenderer[] cells;
        private Sprite[] textures;
        private Texture2D[] generated;
        private int lastIndex=-1,lastCount=-1;
        public static int WorkedCellCount(float progress) => Mathf.Clamp(Mathf.FloorToInt(progress*Lanes*Columns+.0001f),0,Lanes*Columns);
        public static bool IsCellWorked(int lane,int column,float progress)
        { return lane*Columns+(lane%2==0 ? column : Columns-1-column)<WorkedCellCount(progress); }
        public static Vector3 PositionAt(float progress)
        {
            float p=Mathf.Clamp01(progress)*Lanes;
            int lane=Mathf.Min(Lanes-1,Mathf.FloorToInt(p));
            float x=6f*(p-lane); if(lane%2==1) x=6f-x;
            return new Vector3(x,-2f+lane*1.05f,0);
        }
        public void Initialize(SpriteRenderer field)
        {
            if(cells!=null) return;
            // Hide only the old always-plowed decoration; leave the saved scene intact.
            foreach(var child in field.transform.parent.GetComponentsInChildren<SpriteRenderer>())
                if(child!=field && child.name.StartsWith("SoilRow")) child.enabled=false;
            generated=new Texture2D[7]; textures=new Sprite[7];
            for(int state=0;state<7;state++) textures[state]=MakeTexture(state);
            cells=new SpriteRenderer[Lanes*Columns];
            for(int lane=0;lane<Lanes;lane++) for(int column=0;column<Columns;column++)
            {
                var go=new GameObject($"Soil_{lane}_{column}"); go.transform.SetParent(transform,false);
                go.transform.position=new Vector3(.125f+column*.25f,-2f+lane*1.05f,0);
                go.transform.localScale=new Vector3(.25f,1.05f/4f,1);
                var r=go.AddComponent<SpriteRenderer>(); r.sortingLayerID=field.sortingLayerID; r.sortingOrder=field.sortingOrder+1;
                cells[lane*Columns+column]=r;
            }
        }
        public void Render(int operationIndex,float progress)
        {
            if(cells==null) return;
            int count=WorkedCellCount(progress);
            if(lastIndex==operationIndex && lastCount==count) return;
            lastIndex=operationIndex; lastCount=count;
            int before=operationIndex>=5 ? 6 : operationIndex==4 ? 5 : operationIndex;
            int after=operationIndex>=4 ? 6 : operationIndex+1;
            for(int lane=0;lane<Lanes;lane++) for(int col=0;col<Columns;col++)
            {
                cells[lane*Columns+col].sprite=textures[IsCellWorked(lane,col,progress) ? after : before];
            }
        }
        private Sprite MakeTexture(int state)
        {
            const int w=16,h=64;
            var texture=new Texture2D(w,h,TextureFormat.RGBA32,false) {name="AgroSim field placeholder "+state,filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
            generated[state]=texture;
            var pixels=new Color[w*h];
            for(int y=0;y<h;y++) for(int x=0;x<w;x++)
            {
                float grain=((x*17+y*13)%11)/110f;
                Color soil=state==0 ? new Color(.47f+grain,.36f+grain,.22f+grain) : new Color(.3f+grain,.17f+grain,.09f+grain);
                int row=y%16;
                if(state==0 && (x*5+y*7)%29<4) soil=new Color(.35f,.29f,.19f); // compact clods
                if(state>0 && row<3) soil=new Color(.18f,.095f,.045f); // furrows after turning
                if(state==2 && row>6 && row<10 && x%8<3) soil=new Color(.85f,.64f,.28f); // seed markers
                if(state>=3 && state<=5 && row>=5 && row<=12 && x%8<=3)
                    soil=state==5 ? new Color(.85f,.69f,.22f) : state==3 ? new Color(.36f,.63f,.22f) : new Color(.18f,.5f,.17f);
                if(state==3 && (x*3+y*5)%37==0) soil=new Color(.82f,.81f,.55f); // fertilizer granules
                if(state==4 && row==14) soil=new Color(.4f,.63f,.58f); // treated row, illustrative spray
                if(state==6 && (x+y*3)%13<2) soil=new Color(.65f,.49f,.2f); // stubble
                pixels[y*w+x]=soil;
            }
            texture.SetPixels(pixels); texture.Apply(false,true);
            return Sprite.Create(texture,new Rect(0,0,w,h),new Vector2(.5f,.5f),w);
        }
        private void OnDestroy()
        {
            if(textures!=null) foreach(var sprite in textures) if(sprite!=null) Destroy(sprite);
            if(generated!=null) foreach(var texture in generated) if(texture!=null) Destroy(texture);
        }
    }
}
