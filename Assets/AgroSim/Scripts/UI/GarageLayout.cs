using System;
using AgroSim.Core;
using AgroSim.Operations;
using UnityEngine;
namespace AgroSim.UI
{
    public sealed class GarageLayout
    {
        private readonly Font font;
        private readonly GameObject[] tools=new GameObject[5];
        private readonly UnityEngine.UI.Button[] toolButtons=new UnityEngine.UI.Button[5];
        private readonly RectTransform tractor,attached;
        private readonly FarmArtwork art=new FarmArtwork();
        private readonly UnityEngine.UI.Outline selectedBay;
        public UnityEngine.UI.Text TractorLabel { get; }
        public GarageLayout(Transform root,Font font,Action select,Action<int> choose)
        {
            this.font=font;
            var floor=Box(root,"GarageFloor",new Vector2(.04f,.27f),new Vector2(.96f,.74f),new Color(.26f,.29f,.27f));
            art.Tile(floor,art.Floor,2.4f);
            Box(floor.transform,"FloorShade",Vector2.zero,Vector2.one,new Color(.06f,.11f,.09f,.72f));
            FarmTheme.Border(floor,FarmTheme.Line);
            // Clear maneuver space between parking, the depot and the exit.
            Box(floor.transform,"ManeuverLane",new Vector2(.025f,.18f),new Vector2(.61f,.43f),new Color(.16f,.22f,.19f,.76f));
            for(int i=0;i<8;i++)
                Box(floor.transform,"LaneDash"+i,new Vector2(.03f+i*.071f,.2f),new Vector2(.07f+i*.071f,.206f),new Color(.74f,.67f,.44f,.7f));
            Box(floor.transform,"DepotFloor",new Vector2(.62f,.08f),new Vector2(.985f,.99f),new Color(.075f,.12f,.1f,.9f));
            Box(floor.transform,"DepotDivider",new Vector2(.615f,.08f),new Vector2(.618f,.99f),FarmTheme.Accent);
            for(int i=0;i<4;i++)
            {
                float x=.025f+i*.145f;
                var bay=Box(floor.transform,"TractorBay"+(i+1),new Vector2(x,.6f),new Vector2(x+.13f,.95f),new Color(.14f,.2f,.18f));
                FarmTheme.Border(bay,i==0 ? FarmTheme.Accent : FarmTheme.Line);
                if(i==0) selectedBay=bay.GetComponent<UnityEngine.UI.Outline>();
                Box(bay.transform,"BayNumberStrip",new Vector2(0,.81f),Vector2.one,new Color(.08f,.14f,.11f,.85f));
                for(int edge=0;edge<2;edge++) Box(bay.transform,"ParkingMark"+edge,new Vector2(.05f+edge*.88f,.04f),new Vector2(.075f+edge*.88f,.76f),new Color(.71f,.65f,.44f,.6f));
                Text(bay.transform,"VAGA "+(i+1)+(i==0 ? "" : "\nVAZIA"),17,i==0 ? new Vector2(0,.8f) : Vector2.zero,Vector2.one);
                var button=Button(floor.transform,i==0 ? "Selecionar TR-01" : "Vaga vazia",new Vector2(x,.46f),new Vector2(x+.13f,.57f),select);
                button.interactable=i==0;
                if(i==0) TractorLabel=button.GetComponentInChildren<UnityEngine.UI.Text>();
            }
            Text(floor.transform,"DEPÓSITO DE IMPLEMENTOS",20,new Vector2(.63f,.88f),new Vector2(.98f,.98f));
            for(int i=0;i<5;i++)
            {
                float x=.635f+(i%3)*.115f, y=i<3 ? .48f : .18f;
                var def=OperationCatalog.Get(i);
                tools[i]=Box(floor.transform,"StoredTool"+i,new Vector2(x,y+.1f),new Vector2(x+.095f,y+.23f),def.Color);
                FarmTheme.Border(tools[i],new Color(def.Color.r,def.Color.g,def.Color.b,.75f));
                Box(tools[i].transform,"ToolLabelBackground",Vector2.zero,Vector2.one,new Color(.055f,.09f,.075f,.68f));
                Box(tools[i].transform,"OperationColor",new Vector2(0,.9f),Vector2.one,def.Color);
                Text(tools[i].transform,def.Implement,15,Vector2.zero,Vector2.one);
                int index=i;
                toolButtons[i]=Button(floor.transform,"Selecionar",new Vector2(x,y-.035f),new Vector2(x+.095f,y+.075f),()=>choose(index));
            }
            Box(floor.transform,"ExitDoor",new Vector2(.41f,.05f),new Vector2(.59f,.18f),new Color(.08f,.12f,.1f));
            for(int i=0;i<7;i++) Box(floor.transform,"DoorStripe"+i,new Vector2(.405f+i*.027f,.043f),new Vector2(.42f+i*.027f,.057f),FarmTheme.Accent);
            Text(floor.transform,"PORTA / SAÍDA",18,new Vector2(.32f,0),new Vector2(.68f,.085f));
            tractor=Box(floor.transform,"MovingTractor",Vector2.zero,Vector2.one,new Color(.93f,.74f,.17f)).GetComponent<RectTransform>();
            Box(tractor,"Cabin",new Vector2(.33f,.32f),new Vector2(.65f,.85f),new Color(.35f,.64f,.67f));
            for(int i=0;i<4;i++) { float x=i%2==0 ? -.1f : .8f, y=i<2 ? .05f : .7f; Box(tractor,"Wheel"+i,new Vector2(x,y),new Vector2(x+.3f,y+.25f),new Color(.02f,.03f,.02f)); }
            Text(tractor,"TR-01",15,new Vector2(0,.05f),new Vector2(1,.35f));
            Box(tractor,"Hood",new Vector2(.18f,.86f),new Vector2(.82f,.98f),new Color(.99f,.84f,.37f));
            attached=Box(floor.transform,"AttachedTool",Vector2.zero,Vector2.one,Color.white).GetComponent<RectTransform>();
        }
        public void Refresh(GarageManeuver move,int index,bool selected,bool allowed)
        {
            selectedBay.effectColor=selected ? FarmTheme.Accent : FarmTheme.Line;
            var p=move.Position;
            Rect(tractor,p-new Vector2(.038f,.075f),p+new Vector2(.038f,.075f));
            tractor.GetComponent<UnityEngine.UI.Image>().color=selected ? new Color(.93f,.74f,.17f) : new Color(.65f,.55f,.28f);
            Rect(attached,p-new Vector2(.03f,.17f),p-new Vector2(-.03f,.08f));
            attached.GetComponent<UnityEngine.UI.Image>().color=OperationCatalog.Get(index).Color;
            attached.gameObject.SetActive(move.Attached);
            TractorLabel.text=selected ? "Desmarcar TR-01" : "Selecionar TR-01";
            TractorLabel.transform.parent.GetComponent<UnityEngine.UI.Button>().interactable=!move.Busy;
            for(int i=0;i<5;i++)
            {
                tools[i].SetActive(!(move.Attached && i==index));
                toolButtons[i].interactable=allowed && selected && !move.Busy && i==index;
                toolButtons[i].GetComponentInChildren<UnityEngine.UI.Text>().text=i==index ? move.Attached ? "Guardar" : "Acoplar" : "Outra etapa";
            }
        }
        public void Dispose() { art.Dispose(); }
        GameObject Box(Transform parent,string name,Vector2 min,Vector2 max,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image)); go.transform.SetParent(parent,false); Rect(go.GetComponent<RectTransform>(),min,max);
            var image=go.GetComponent<UnityEngine.UI.Image>(); image.color=color; image.raycastTarget=false; return go;
        }
        UnityEngine.UI.Text Text(Transform parent,string value,int size,Vector2 min,Vector2 max)
        {
            var go=new GameObject("Label",typeof(RectTransform),typeof(UnityEngine.UI.Text)); go.transform.SetParent(parent,false); Rect(go.GetComponent<RectTransform>(),min,max);
            var t=go.GetComponent<UnityEngine.UI.Text>(); t.text=value; t.font=font; t.fontSize=size; t.color=FarmTheme.Ink; t.alignment=TextAnchor.MiddleCenter; t.raycastTarget=false;
            t.resizeTextForBestFit=true; t.resizeTextMinSize=10; t.resizeTextMaxSize=size; return t;
        }
        UnityEngine.UI.Button Button(Transform parent,string label,Vector2 min,Vector2 max,Action action)
        {
            var go=Box(parent,label,min,max,new Color(.22f,.38f,.25f)); go.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            var b=go.AddComponent<UnityEngine.UI.Button>(); b.onClick.AddListener(()=>action()); FarmTheme.StyleButton(b); Text(go.transform,label,17,new Vector2(.04f,.03f),new Vector2(.96f,.97f)); return b;
        }
        static void Rect(RectTransform r,Vector2 min,Vector2 max) { r.anchorMin=min; r.anchorMax=max; r.offsetMin=r.offsetMax=Vector2.zero; }
    }
}
