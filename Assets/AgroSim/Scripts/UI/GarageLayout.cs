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
        public UnityEngine.UI.Text TractorLabel { get; }
        public GarageLayout(Transform root,Font font,Action select,Action<int> choose)
        {
            this.font=font;
            var floor=Box(root,"GarageFloor",new Vector2(.04f,.27f),new Vector2(.96f,.78f),new Color(.26f,.29f,.27f));
            for(int i=0;i<4;i++)
            {
                float x=.025f+i*.145f;
                var bay=Box(floor.transform,"TractorBay"+(i+1),new Vector2(x,.6f),new Vector2(x+.13f,.95f),new Color(.14f,.2f,.18f));
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
                Text(tools[i].transform,def.Implement,15,Vector2.zero,Vector2.one);
                int index=i;
                toolButtons[i]=Button(floor.transform,"Selecionar",new Vector2(x,y-.035f),new Vector2(x+.095f,y+.075f),()=>choose(index));
            }
            Box(floor.transform,"ExitDoor",new Vector2(.41f,.05f),new Vector2(.59f,.18f),new Color(.08f,.12f,.1f));
            Text(floor.transform,"PORTA / SAÍDA",18,new Vector2(.32f,0),new Vector2(.68f,.085f));
            tractor=Box(floor.transform,"MovingTractor",Vector2.zero,Vector2.one,new Color(.93f,.74f,.17f)).GetComponent<RectTransform>();
            Box(tractor,"Cabin",new Vector2(.33f,.32f),new Vector2(.65f,.85f),new Color(.35f,.64f,.67f));
            for(int i=0;i<4;i++) { float x=i%2==0 ? -.1f : .8f, y=i<2 ? .05f : .7f; Box(tractor,"Wheel"+i,new Vector2(x,y),new Vector2(x+.3f,y+.25f),new Color(.02f,.03f,.02f)); }
            Text(tractor,"TR-01",15,new Vector2(0,.05f),new Vector2(1,.35f));
            attached=Box(floor.transform,"AttachedTool",Vector2.zero,Vector2.one,Color.white).GetComponent<RectTransform>();
        }
        public void Refresh(GarageManeuver move,int index,bool selected,bool allowed)
        {
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
        GameObject Box(Transform parent,string name,Vector2 min,Vector2 max,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image)); go.transform.SetParent(parent,false); Rect(go.GetComponent<RectTransform>(),min,max);
            var image=go.GetComponent<UnityEngine.UI.Image>(); image.color=color; image.raycastTarget=false; return go;
        }
        UnityEngine.UI.Text Text(Transform parent,string value,int size,Vector2 min,Vector2 max)
        {
            var go=new GameObject("Label",typeof(RectTransform),typeof(UnityEngine.UI.Text)); go.transform.SetParent(parent,false); Rect(go.GetComponent<RectTransform>(),min,max);
            var t=go.GetComponent<UnityEngine.UI.Text>(); t.text=value; t.font=font; t.fontSize=size; t.color=Color.white; t.alignment=TextAnchor.MiddleCenter; t.raycastTarget=false;
            t.resizeTextForBestFit=true; t.resizeTextMinSize=10; t.resizeTextMaxSize=size; return t;
        }
        UnityEngine.UI.Button Button(Transform parent,string label,Vector2 min,Vector2 max,Action action)
        {
            var go=Box(parent,label,min,max,new Color(.22f,.38f,.25f)); go.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            var b=go.AddComponent<UnityEngine.UI.Button>(); b.onClick.AddListener(()=>action()); Text(go.transform,label,17,Vector2.zero,Vector2.one); return b;
        }
        static void Rect(RectTransform r,Vector2 min,Vector2 max) { r.anchorMin=min; r.anchorMax=max; r.offsetMin=r.offsetMax=Vector2.zero; }
    }
}
