using System;
using System.Collections.Generic;
using AgroSim.Data;
using AgroSim.Operations;
using UnityEngine;

namespace AgroSim.UI
{
    // Presentation only. Machine, operation and persistence remain with the existing slice.
    public sealed class FarmMenus
    {
        private readonly FarmNavigation navigation=new FarmNavigation();
        private readonly Dictionary<FarmScreen,GameObject> screens=new Dictionary<FarmScreen,GameObject>();
        private readonly List<UnityEngine.UI.Text> summaries=new List<UnityEngine.UI.Text>();
        private readonly List<UnityEngine.UI.Image> fieldRows=new List<UnityEngine.UI.Image>();
        private readonly List<UnityEngine.UI.Image> cropMarkers=new List<UnityEngine.UI.Image>();
        private readonly PlaceholderSlice slice;
        private readonly Font font;
        private readonly GameObject garage;
        private readonly Transform parent;
        private readonly FarmArtwork art=new FarmArtwork();
        private GameObject quit;
        private UnityEngine.UI.Text lastSave,workshopInfo,fuelInfo,fieldInfo,workshopMessage,fuelMessage;
        private UnityEngine.UI.Button repair,refuel;
        private RectTransform fuelFill,wearFill;
        private static readonly Color Background=FarmTheme.Background;
        private static readonly Color Card=FarmTheme.Card;
        private static readonly Color Green=FarmTheme.Green;
        private static readonly Color Muted=FarmTheme.Muted;
        public FarmScreen Current=>navigation.Current;
        public FarmMenus(Transform canvas,GameObject garage,Font font,PlaceholderSlice slice)
        {
            parent=canvas; this.garage=garage; this.font=font; this.slice=slice;
            BuildMain(); BuildMap(); BuildService(false); BuildService(true); BuildField(); BuildQuit();
            ActionButton(garage.transform,"Voltar ao mapa",new Vector2(.57f,.895f),new Vector2(.76f,.96f),()=>Open(FarmScreen.FarmMap),Card,17);
            Refresh(); Apply();
        }
        public bool Open(FarmScreen screen)
        {
            if(!slice.AtHome || !navigation.Open(screen)) return false;
            Refresh(); Apply(); return true;
        }
        public void BeginOperation() { navigation.BeginOperation(); Apply(); }
        public void ReturnToGarage() { navigation.ReturnToGarage(); Refresh(); Apply(); }
        public void NewGameStarted() { navigation.ReturnToGarage(); navigation.Open(FarmScreen.FarmMap); Refresh(); Apply(); }
        public void Dispose() { art.Dispose(); }
        private void Apply()
        {
            foreach(var pair in screens) pair.Value.SetActive(pair.Key==Current);
            garage.SetActive(Current==FarmScreen.Garage);
            if(slice.AtHome) slice.ShowFieldPreview(Current==FarmScreen.Field);
            quit.SetActive(false);
        }
        public void Refresh()
        {
            string next=slice.NextOperationIndex>=5 ? "Ciclo concluído" : $"Etapa {slice.NextOperationIndex+1}/5\n{OperationCatalog.Get(slice.NextOperationIndex).Name}";
            string summary=$"{next}\n\nTR-01\nCombustível: {slice.Fuel:F1} / 100 L\nDesgaste: {slice.Wear:F1}%\nProdutividade: {slice.Productivity:F0}%\n\nSaldo\n{GarageEconomy.Money(slice.BalanceCents)}\n\nÚltimo save\n{slice.LastSaveDisplay}";
            foreach(var text in summaries) text.text=summary;
            lastSave.text="Último save: "+slice.LastSaveDisplay;
            workshopInfo.text=$"TR-01 na oficina\n\nDesgaste: {slice.Wear:F1}%\nReparo completo: {GarageEconomy.Money(GarageEconomy.RepairCost(slice.Wear))}\n\nO reparo restaura a condição do trator.\nO serviço é confirmado no save.";
            fuelInfo.text=$"TR-01 no abastecimento\n\nTanque: {slice.Fuel:F1} / 100 L\nAbastecer: {GarageEconomy.Money(GarageEconomy.RefuelCost(slice.Fuel))}\n\nO abastecimento completa o tanque.\nO serviço é confirmado no save.";
            workshopMessage.text=slice.HomeMessage; fuelMessage.text=slice.HomeMessage;
            repair.interactable=slice.CanUseServices && slice.Wear>.001f;
            refuel.interactable=slice.CanUseServices && slice.Fuel<99.99f;
            SetRect(fuelFill,Vector2.zero,new Vector2(Mathf.Clamp01(slice.Fuel/100),1));
            SetRect(wearFill,Vector2.zero,new Vector2(Mathf.Clamp01(slice.Wear/100),1));
            string[] states={"Solo intacto","Solo arado","Linhas de sementes","Brotos e adubo","Lavoura madura","Talhão colhido"};
            fieldInfo.text=$"{states[Mathf.Clamp(slice.NextOperationIndex,0,5)]} · Produtividade: {slice.Productivity:F0}%\n{next.Replace("\n"," · ")}\n{(slice.NextOperationIndex>=5 ? "Ciclo concluído." : "Prepare os equipamentos no galpão.")}";
            int state=Mathf.Clamp(slice.NextOperationIndex,0,5);
            foreach(var row in fieldRows) row.color=state==0 ? Color.clear : state==5 ? new Color(.65f,.49f,.2f,.5f) : new Color(.16f,.095f,.045f);
            Color[] colors={Color.clear,Color.clear,new Color(.85f,.64f,.28f),new Color(.36f,.63f,.22f),new Color(.85f,.69f,.22f),Color.clear};
            foreach(var marker in cropMarkers) marker.color=colors[state];
        }
        private GameObject Screen(FarmScreen kind,string title,string subtitle)
        {
            var go=Box(parent,kind+"Screen",Vector2.zero,Vector2.one,Background,true);
            screens.Add(kind,go);
            Text(go.transform,title,32,new Vector2(.045f,.88f),new Vector2(.77f,.965f));
            Text(go.transform,subtitle,18,new Vector2(.045f,.825f),new Vector2(.85f,.885f),Muted);
            Box(go.transform,"HeaderAccent",new Vector2(.045f,.815f),new Vector2(.115f,.819f),FarmTheme.Accent);
            var brand=Text(go.transform,"AGROSIM / FAZENDA",14,new Vector2(.77f,.91f),new Vector2(.955f,.965f),FarmTheme.Accent);
            brand.alignment=TextAnchor.MiddleRight; Fit(brand,10,14);
            return go;
        }
        private void BuildMain()
        {
            var screen=Screen(FarmScreen.MainMenu,"AGROSIM","Sua fazenda, suas decisões.");
            var choices=Box(screen.transform,"MainActions",new Vector2(.045f,.23f),new Vector2(.43f,.77f),Card);
            FarmTheme.Border(choices,FarmTheme.Line);
            Text(choices.transform,"COMEÇAR A JOGAR",22,new Vector2(.07f,.79f),new Vector2(.93f,.94f),Muted);
            ActionButton(choices.transform,"Continuar",new Vector2(.07f,.56f),new Vector2(.93f,.74f),()=>Open(FarmScreen.FarmMap),Green);
            ActionButton(choices.transform,"Novo jogo",new Vector2(.07f,.32f),new Vector2(.93f,.5f),slice.ShowNewGameConfirmation,Card);
            ActionButton(choices.transform,"Sair",new Vector2(.07f,.08f),new Vector2(.93f,.26f),()=>quit.SetActive(true),Card);
            var landscape=Box(screen.transform,"FarmIllustration",new Vector2(.47f,.23f),new Vector2(.955f,.77f),new Color(.31f,.44f,.26f));
            DrawFarm(landscape.transform,false);
            lastSave=Text(screen.transform,"",18,new Vector2(.045f,.12f),new Vector2(.94f,.19f),Muted);
            Text(screen.transform,"Planeje no mapa. Prepare no galpão. Trabalhe no talhão.",18,new Vector2(.045f,.04f),new Vector2(.95f,.11f),Muted);
        }
        private void BuildMap()
        {
            var screen=Screen(FarmScreen.FarmMap,"VISÃO GERAL DA FAZENDA","Escolha uma área no mapa para entrar.");
            var map=Box(screen.transform,"FarmMap",new Vector2(.045f,.18f),new Vector2(.695f,.8f),new Color(.31f,.44f,.26f));
            DrawFarm(map.transform,true);
            var details=Box(screen.transform,"FarmSummary",new Vector2(.72f,.18f),new Vector2(.955f,.8f),Card);
            FarmTheme.Border(details,FarmTheme.Line);
            var info=Text(details.transform,"",21,new Vector2(.07f,.2f),new Vector2(.93f,.94f)); Fit(info,16,21); summaries.Add(info);
            ActionButton(details.transform,"Ir ao galpão",new Vector2(.07f,.04f),new Vector2(.93f,.16f),()=>Open(FarmScreen.Garage),Green,18);
            ActionButton(screen.transform,"Menu inicial",new Vector2(.045f,.045f),new Vector2(.23f,.135f),()=>Open(FarmScreen.MainMenu),Card,18);
            ActionButton(screen.transform,"Tentar salvar",new Vector2(.75f,.045f),new Vector2(.955f,.135f),slice.RetryHomeSave,Card,18);
            Text(screen.transform,"Galpão · Oficina · Abastecimento · Talhão",16,new Vector2(.25f,.045f),new Vector2(.73f,.135f),Muted);
        }
        private void DrawFarm(Transform map,bool clickable)
        {
            art.Tile(map.gameObject,art.Grass);
            FarmTheme.Border(map.gameObject,FarmTheme.Line);
            MapRoad(map,"MainRoad",new Vector2(.39f,0),new Vector2(.45f,1));
            MapRoad(map,"AccessRoad",new Vector2(.2f,.35f),new Vector2(.62f,.42f));
            MapRoad(map,"WorkshopRoad",new Vector2(.2f,.76f),new Vector2(.45f,.82f));
            MapRoad(map,"FuelRoad",new Vector2(.2f,.1f),new Vector2(.45f,.16f));
            for(int i=0;i<6;i++)
            {
                float x=.52f+i*.075f;
                var tree=Box(map,"CozyMapTree"+i,new Vector2(x,.83f),new Vector2(x+.07f,.975f),new Color(.17f,.32f,.19f));
                FarmArtwork.Picture(tree,art.Tree(i));
            }
            var areaColor=new Color(.07f,.16f,.1f,.28f);
            var workshop=Area(map,"Oficina / Mecânico",new Vector2(.055f,.65f),new Vector2(.31f,.93f),areaColor,FarmScreen.Workshop,clickable);
            Building(workshop.transform,new Color(.68f,.49f,.26f),"OFICINA",1);
            var storage=Area(map,"Galpão",new Vector2(.055f,.31f),new Vector2(.31f,.6f),areaColor,FarmScreen.Garage,clickable);
            Building(storage.transform,new Color(.63f,.67f,.6f),"TR-01",0);
            var fuel=Area(map,"Abastecimento",new Vector2(.055f,.04f),new Vector2(.32f,.24f),areaColor,FarmScreen.Fuel,clickable);
            Box(fuel.transform,"PumpBase",new Vector2(.25f,.3f),new Vector2(.75f,.35f),new Color(.22f,.27f,.23f));
            Box(fuel.transform,"Pump",new Vector2(.38f,.34f),new Vector2(.61f,.91f),new Color(.85f,.48f,.24f));
            Box(fuel.transform,"Meter",new Vector2(.41f,.65f),new Vector2(.58f,.82f),new Color(.75f,.85f,.8f));
            Box(fuel.transform,"Hose",new Vector2(.62f,.4f),new Vector2(.66f,.79f),FarmTheme.Background);
            var field=Area(map,"Talhão 01",new Vector2(.54f,.17f),new Vector2(.95f,.77f),new Color(.29f,.21f,.13f),FarmScreen.Field,clickable);
            DrawRows(field.transform,new Vector2(.05f,.27f),new Vector2(.95f,.94f));
            var pond=Box(map,"Pond",new Vector2(.78f,.025f),new Vector2(.97f,.105f),new Color(.34f,.58f,.62f));
            FarmTheme.Border(pond,new Color(.43f,.58f,.34f));
            Box(pond.transform,"WaterReflection",new Vector2(.15f,.62f),new Vector2(.7f,.69f),new Color(.64f,.81f,.78f,.55f));
            Box(pond.transform,"WaterReflection2",new Vector2(.4f,.34f),new Vector2(.86f,.39f),new Color(.64f,.81f,.78f,.35f));
            field.transform.Find("AreaCaption").SetAsLastSibling();
        }
        private void MapRoad(Transform map,string name,Vector2 min,Vector2 max)
        {
            var road=Box(map,name,min,max,new Color(.68f,.59f,.42f)); art.Tile(road,art.Road);
        }
        private GameObject Area(Transform map,string title,Vector2 min,Vector2 max,Color color,FarmScreen area,bool clickable)
        {
            var go=Box(map,area+"Area",min,max,color,clickable);
            FarmTheme.Border(go,FarmTheme.Line);
            if(clickable) { var button=go.AddComponent<UnityEngine.UI.Button>(); button.onClick.AddListener(()=>Open(area)); FarmTheme.StyleButton(button); }
            var caption=Box(go.transform,"AreaCaption",new Vector2(.015f,.015f),new Vector2(.985f,.265f),Card);
            var label=Text(caption.transform,title,19,new Vector2(.025f,.02f),new Vector2(.975f,.98f)); label.alignment=TextAnchor.MiddleCenter; Fit(label,12,19);
            return go;
        }
        private void Building(Transform parent,Color color,string name,int variant)
        {
            var sprite=art.House(variant);
            if(sprite!=null)
            {
                var picture=Box(parent,"CozyBuilding",new Vector2(.1f,.29f),new Vector2(.9f,.97f),Color.white);
                FarmArtwork.Picture(picture,sprite); return;
            }
            Box(parent,"Building",new Vector2(.14f,.29f),new Vector2(.86f,.84f),color);
            Box(parent,"Roof",new Vector2(.09f,.72f),new Vector2(.91f,.89f),new Color(.28f,.19f,.13f));
            Box(parent,"Door",new Vector2(.36f,.29f),new Vector2(.64f,.6f),new Color(.16f,.21f,.18f));
            var label=Text(parent,name,17,new Vector2(.1f,.6f),new Vector2(.9f,.73f)); label.alignment=TextAnchor.MiddleCenter; Fit(label,12,17);
        }
        private void DrawRows(Transform parent,Vector2 min,Vector2 max)
        {
            for(int i=0;i<9;i++)
            {
                float y=Mathf.Lerp(min.y,max.y-.03f,i/8f);
                var row=Box(parent,"CropRow"+i,new Vector2(min.x,y),new Vector2(max.x,y+.018f),Green);
                fieldRows.Add(row.GetComponent<UnityEngine.UI.Image>());
                for(int column=0;column<12;column++)
                {
                    float x=Mathf.Lerp(min.x,max.x-.018f,column/11f);
                    var marker=Box(parent,"CropMarker"+i+"_"+column,new Vector2(x,y+.023f),new Vector2(x+.015f,y+.043f),Color.clear);
                    cropMarkers.Add(marker.GetComponent<UnityEngine.UI.Image>());
                }
            }
        }
        private void BuildService(bool fuel)
        {
            var kind=fuel ? FarmScreen.Fuel : FarmScreen.Workshop;
            var screen=Screen(kind,fuel ? "ABASTECIMENTO" : "OFICINA MECÂNICA",fuel ? "Complete o tanque antes de ir ao talhão." : "Cuide da condição do TR-01 para a próxima operação.");
            var bay=Box(screen.transform,"ServiceBay",new Vector2(.045f,.22f),new Vector2(.48f,.8f),Card);
            FarmTheme.Border(bay,FarmTheme.Line);
            var serviceFloor=Box(bay.transform,"ServiceFloor",new Vector2(.04f,.22f),new Vector2(.96f,.77f),new Color(.32f,.34f,.28f));
            art.Tile(serviceFloor,art.Floor,1.8f);
            Box(serviceFloor.transform,"FloorShade",Vector2.zero,Vector2.one,new Color(.07f,.13f,.1f,.48f));
            for(int i=0;i<2;i++) Box(bay.transform,"BayMark"+i,new Vector2(.27f+i*.5f,.28f),new Vector2(.278f+i*.5f,.72f),FarmTheme.Accent);
            Text(bay.transform,fuel ? "PONTO DE COMBUSTÍVEL" : "ÁREA DO MECÂNICO",22,new Vector2(.06f,.8f),new Vector2(.94f,.96f),Muted);
            // Simple equipment silhouettes, intentionally kept as local placeholders.
            Box(bay.transform,"Tractor",new Vector2(.3f,.36f),new Vector2(.74f,.58f),new Color(.9f,.72f,.14f));
            Box(bay.transform,"Cabin",new Vector2(.49f,.49f),new Vector2(.65f,.68f),new Color(.35f,.65f,.7f));
            for(int i=0;i<2;i++) Box(bay.transform,"Wheel"+i,new Vector2(.34f+i*.25f,.29f),new Vector2(.45f+i*.25f,.43f),new Color(.025f,.035f,.03f));
            if(fuel)
            {
                Box(bay.transform,"Pump",new Vector2(.09f,.29f),new Vector2(.23f,.66f),new Color(.8f,.36f,.22f));
                Box(bay.transform,"Meter",new Vector2(.11f,.5f),new Vector2(.21f,.61f),new Color(.7f,.83f,.78f));
                Box(bay.transform,"Hose",new Vector2(.23f,.35f),new Vector2(.3f,.38f),Muted);
            }
            else
            {
                Box(bay.transform,"Lift",new Vector2(.25f,.22f),new Vector2(.8f,.27f),new Color(.65f,.37f,.22f));
                Box(bay.transform,"ToolBench",new Vector2(.06f,.4f),new Vector2(.23f,.56f),new Color(.55f,.3f,.18f));
                Text(bay.transform,"TR-01 / manutenção",18,new Vector2(.06f,.065f),new Vector2(.94f,.18f),Muted).alignment=TextAnchor.MiddleCenter;
            }
            var detail=Box(screen.transform,"ServiceDetails",new Vector2(.52f,.22f),new Vector2(.955f,.8f),Card);
            FarmTheme.Border(detail,FarmTheme.Line);
            var info=Text(detail.transform,"",23,new Vector2(.07f,.33f),new Vector2(.93f,.94f)); Fit(info,18,23);
            var bar=Box(detail.transform,"ConditionBar",new Vector2(.07f,.25f),new Vector2(.93f,.29f),new Color(.2f,.25f,.22f));
            var fill=Box(bar.transform,"Fill",Vector2.zero,Vector2.one,fuel ? Green : new Color(.7f,.42f,.2f)).GetComponent<RectTransform>();
            var button=ActionButton(detail.transform,fuel ? "Abastecer TR-01" : "Reparar TR-01",new Vector2(.07f,.06f),new Vector2(.93f,.2f),()=>{ slice.UseHomeService(fuel); Refresh(); },Green,21);
            var message=Text(screen.transform,"",16,new Vector2(.28f,.045f),new Vector2(.955f,.16f),Muted); Fit(message,13,16);
            if(fuel) { fuelInfo=info; refuel=button; fuelFill=fill; fuelMessage=message; }
            else { workshopInfo=info; repair=button; wearFill=fill; workshopMessage=message; }
            BackToMap(screen.transform);
        }
        private void BuildField()
        {
            var screen=Screen(FarmScreen.Field,"TALHÃO 01","Acompanhe o estado do terreno e prepare a próxima operação.");
            screen.GetComponent<UnityEngine.UI.Image>().color=Color.clear;
            Box(screen.transform,"FieldHeading",new Vector2(0,.825f),Vector2.one,Card).transform.SetAsFirstSibling();
            var details=Box(screen.transform,"FieldDetails",Vector2.zero,new Vector2(1,.2f),Card);
            fieldInfo=Text(details.transform,"",19,new Vector2(.25f,.06f),new Vector2(.72f,.94f)); Fit(fieldInfo,12,19);
            ActionButton(details.transform,"Ir ao galpão",new Vector2(.76f,.2f),new Vector2(.955f,.8f),()=>Open(FarmScreen.Garage),Green,18);
            ActionButton(details.transform,"Voltar ao mapa",new Vector2(.045f,.2f),new Vector2(.225f,.8f),()=>Open(FarmScreen.FarmMap),Card,18);
        }
        private void BuildQuit()
        {
            quit=Box(parent,"QuitConfirmation",Vector2.zero,Vector2.one,new Color(.025f,.04f,.035f,.96f),true);
            var dialog=Box(quit.transform,"QuitDialog",new Vector2(.29f,.29f),new Vector2(.71f,.71f),Card);
            Text(dialog.transform,"Sair do AgroSim?\nSeu último save confirmado será mantido.",24,new Vector2(.07f,.56f),new Vector2(.93f,.93f));
            ActionButton(dialog.transform,"Cancelar",new Vector2(.07f,.3f),new Vector2(.93f,.49f),()=>quit.SetActive(false),Green,20);
            ActionButton(dialog.transform,"Sair",new Vector2(.07f,.06f),new Vector2(.93f,.25f),Exit,Card,20);
            quit.SetActive(false);
        }
        private static void Exit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#else
            Application.Quit();
#endif
        }
        private void BackToMap(Transform screen) => ActionButton(screen,"Voltar ao mapa",new Vector2(.045f,.045f),new Vector2(.25f,.15f),()=>Open(FarmScreen.FarmMap),Card,18);
        private GameObject Box(Transform parent,string name,Vector2 min,Vector2 max,Color color,bool raycast=false)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image)); go.transform.SetParent(parent,false);
            SetRect(go.GetComponent<RectTransform>(),min,max);
            var image=go.GetComponent<UnityEngine.UI.Image>(); image.color=color; image.raycastTarget=raycast;
            return go;
        }
        private UnityEngine.UI.Text Text(Transform parent,string value,int size,Vector2 min,Vector2 max,Color? color=null)
        {
            var go=new GameObject("Label",typeof(RectTransform),typeof(UnityEngine.UI.Text)); go.transform.SetParent(parent,false); SetRect(go.GetComponent<RectTransform>(),min,max);
            var text=go.GetComponent<UnityEngine.UI.Text>(); text.font=font; text.text=value; text.fontSize=size; text.color=color??FarmTheme.Ink; text.raycastTarget=false; text.alignment=TextAnchor.MiddleLeft;
            return text;
        }
        private UnityEngine.UI.Button ActionButton(Transform parent,string label,Vector2 min,Vector2 max,Action action,Color color,int size=24)
        {
            var go=Box(parent,label+"Button",min,max,color,true); var button=go.AddComponent<UnityEngine.UI.Button>(); button.onClick.AddListener(()=>action());
            FarmTheme.StyleButton(button);
            var text=Text(go.transform,label,size,new Vector2(.045f,.03f),new Vector2(.955f,.97f)); text.alignment=TextAnchor.MiddleCenter; Fit(text,14,size);
            return button;
        }
        private static void Fit(UnityEngine.UI.Text text,int min,int max) { text.resizeTextForBestFit=true; text.resizeTextMinSize=min; text.resizeTextMaxSize=max; }
        private static void SetRect(RectTransform rect,Vector2 min,Vector2 max) { rect.anchorMin=min; rect.anchorMax=max; rect.offsetMin=rect.offsetMax=Vector2.zero; }
    }
}
