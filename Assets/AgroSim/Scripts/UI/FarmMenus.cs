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
        private readonly PlaceholderSlice slice;
        private readonly Font font;
        private readonly GameObject garage;
        private readonly Transform parent;
        private GameObject quit;
        private UnityEngine.UI.Text lastSave,workshopInfo,fuelInfo,fieldInfo,workshopMessage,fuelMessage;
        private UnityEngine.UI.Button repair,refuel;
        private RectTransform fuelFill,wearFill;
        private static readonly Color Background=new Color(.055f,.09f,.085f,1);
        private static readonly Color Card=new Color(.095f,.15f,.14f,1);
        private static readonly Color Green=new Color(.27f,.47f,.29f,1);
        private static readonly Color Muted=new Color(.69f,.78f,.71f,1);
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
            Color[] colors={new Color(.47f,.36f,.22f),new Color(.3f,.17f,.09f),new Color(.8f,.63f,.28f),new Color(.36f,.63f,.22f),new Color(.85f,.69f,.22f),new Color(.65f,.49f,.2f)};
            foreach(var row in fieldRows) row.color=colors[Mathf.Clamp(slice.NextOperationIndex,0,5)];
        }
        private GameObject Screen(FarmScreen kind,string title,string subtitle)
        {
            var go=Box(parent,kind+"Screen",Vector2.zero,Vector2.one,Background,true);
            screens.Add(kind,go);
            Text(go.transform,title,32,new Vector2(.045f,.88f),new Vector2(.77f,.965f));
            Text(go.transform,subtitle,18,new Vector2(.045f,.825f),new Vector2(.85f,.885f),Muted);
            return go;
        }
        private void BuildMain()
        {
            var screen=Screen(FarmScreen.MainMenu,"AGROSIM","Sua fazenda, suas decisões.");
            var choices=Box(screen.transform,"MainActions",new Vector2(.045f,.23f),new Vector2(.43f,.77f),Card);
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
            var info=Text(details.transform,"",21,new Vector2(.07f,.2f),new Vector2(.93f,.94f)); Fit(info,16,21); summaries.Add(info);
            ActionButton(details.transform,"Ir ao galpão",new Vector2(.07f,.04f),new Vector2(.93f,.16f),()=>Open(FarmScreen.Garage),Green,18);
            ActionButton(screen.transform,"Menu inicial",new Vector2(.045f,.045f),new Vector2(.23f,.135f),()=>Open(FarmScreen.MainMenu),Card,18);
            ActionButton(screen.transform,"Tentar salvar",new Vector2(.75f,.045f),new Vector2(.955f,.135f),slice.RetryHomeSave,Card,18);
            Text(screen.transform,"Galpão · Oficina · Abastecimento · Talhão",16,new Vector2(.25f,.045f),new Vector2(.73f,.135f),Muted);
        }
        private void DrawFarm(Transform map,bool clickable)
        {
            Box(map,"MainRoad",new Vector2(.39f,0),new Vector2(.45f,1),new Color(.68f,.59f,.42f));
            Box(map,"AccessRoad",new Vector2(.2f,.35f),new Vector2(.62f,.42f),new Color(.68f,.59f,.42f));
            Box(map,"WorkshopRoad",new Vector2(.2f,.76f),new Vector2(.45f,.82f),new Color(.68f,.59f,.42f));
            for(int i=0;i<8;i++) Box(map,"Tree"+i,new Vector2(.52f+i*.05f,.88f),new Vector2(.55f+i*.05f,.95f),new Color(.17f,.32f,.19f));
            var workshop=Area(map,"Oficina / Mecânico",new Vector2(.055f,.65f),new Vector2(.31f,.93f),new Color(.57f,.38f,.23f),FarmScreen.Workshop,clickable);
            Building(workshop.transform,new Color(.68f,.49f,.26f),"OFICINA");
            var storage=Area(map,"Galpão",new Vector2(.055f,.31f),new Vector2(.31f,.6f),new Color(.4f,.44f,.39f),FarmScreen.Garage,clickable);
            Building(storage.transform,new Color(.63f,.67f,.6f),"TR-01");
            var fuel=Area(map,"Abastecimento",new Vector2(.055f,.04f),new Vector2(.32f,.24f),new Color(.45f,.26f,.23f),FarmScreen.Fuel,clickable);
            Box(fuel.transform,"Pump",new Vector2(.37f,.39f),new Vector2(.63f,.88f),new Color(.85f,.48f,.24f));
            var field=Area(map,"Talhão 01",new Vector2(.54f,.17f),new Vector2(.95f,.77f),new Color(.29f,.21f,.13f),FarmScreen.Field,clickable);
            DrawRows(field.transform,new Vector2(.05f,.27f),new Vector2(.95f,.94f));
            Box(map,"Pond",new Vector2(.78f,.025f),new Vector2(.97f,.105f),new Color(.34f,.58f,.62f));
        }
        private GameObject Area(Transform map,string title,Vector2 min,Vector2 max,Color color,FarmScreen area,bool clickable)
        {
            var go=Box(map,area+"Area",min,max,color,clickable);
            if(clickable) go.AddComponent<UnityEngine.UI.Button>().onClick.AddListener(()=>Open(area));
            var label=Text(go.transform,title,19,new Vector2(.025f,.015f),new Vector2(.975f,.26f)); label.alignment=TextAnchor.MiddleCenter; Fit(label,12,19);
            return go;
        }
        private void Building(Transform parent,Color color,string name)
        {
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
            }
        }
        private void BuildService(bool fuel)
        {
            var kind=fuel ? FarmScreen.Fuel : FarmScreen.Workshop;
            var screen=Screen(kind,fuel ? "ABASTECIMENTO" : "OFICINA MECÂNICA",fuel ? "Complete o tanque antes de ir ao talhão." : "Cuide da condição do TR-01 para a próxima operação.");
            var bay=Box(screen.transform,"ServiceBay",new Vector2(.045f,.22f),new Vector2(.48f,.8f),Card);
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
            var text=go.GetComponent<UnityEngine.UI.Text>(); text.font=font; text.text=value; text.fontSize=size; text.color=color??Color.white; text.raycastTarget=false; text.alignment=TextAnchor.MiddleLeft;
            return text;
        }
        private UnityEngine.UI.Button ActionButton(Transform parent,string label,Vector2 min,Vector2 max,Action action,Color color,int size=24)
        {
            var go=Box(parent,label+"Button",min,max,color,true); var button=go.AddComponent<UnityEngine.UI.Button>(); button.onClick.AddListener(()=>action());
            var text=Text(go.transform,label,size,new Vector2(.045f,.03f),new Vector2(.955f,.97f)); text.alignment=TextAnchor.MiddleCenter; Fit(text,14,size);
            return button;
        }
        private static void Fit(UnityEngine.UI.Text text,int min,int max) { text.resizeTextForBestFit=true; text.resizeTextMinSize=min; text.resizeTextMaxSize=max; }
        private static void SetRect(RectTransform rect,Vector2 min,Vector2 max) { rect.anchorMin=min; rect.anchorMax=max; rect.offsetMin=rect.offsetMax=Vector2.zero; }
    }
}
