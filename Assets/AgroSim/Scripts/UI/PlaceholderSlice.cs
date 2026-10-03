using AgroSim.Core;
using AgroSim.Operations;
using AgroSim.Data;
using AgroSim.Tractors;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace AgroSim.UI
{
    public sealed class PlaceholderSlice : MonoBehaviour
    {
        public Transform tractor;
        public GameManager manager;
        public TractorData tractorData;
        private PlaceholderOperation operation;
        private Font font;
        private UnityEngine.UI.Text status;
        private GameObject garage, eventPanel, resultPanel;
        private UnityEngine.UI.Text result;
        private Vector3[] route;
        private int routeIndex;
        private float startingFuel;
        private readonly GaragePreparation preparation = new GaragePreparation();
        private Transform implement;
        private UnityEngine.UI.Button startButton;
        private UnityEngine.UI.Button implementButton;
        private UnityEngine.UI.Text equipmentDetails;
        private UnityEngine.UI.Text maintenanceDetails, garageMessage;
        private UnityEngine.UI.Button refuelButton, repairButton;
        private UnityEngine.UI.Text refuelActionLabel,repairActionLabel;
        private string serviceMessage;
        private GameObject tractorPreview, implementPreview;
        private UnityEngine.UI.Text tractorLabel, implementLabel;
        private readonly LocalOperationLog log = new LocalOperationLog();
        private float sampleTimer;
        private SaveGameStore saves;
        private UnityEngine.UI.Text saveMessage;
        private UnityEngine.UI.Text saveDate;
        private GameObject newGamePanel;
        private bool saveFailed, recovering;
        private readonly Vector3 home = new Vector3(-5f, -1f, 0f);

        private void Awake()
        {
            operation = gameObject.AddComponent<PlaceholderOperation>();
            implement = tractor.Find("GradeAradora");
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvas = new GameObject("PlaceholderHUD", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = .5f;
            if (FindAnyObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            var top = Panel(canvas.transform, "Telemetry", new Vector2(.02f,.8f), new Vector2(.98f,.98f));
            Label(top.transform, "AGROSIM  /  DIA 1 — PREPARO DO SOLO", 24, new Vector2(.03f,.55f), new Vector2(.97f,.95f));
            status = Label(top.transform, "", 19, new Vector2(.03f,.08f), new Vector2(.97f,.55f));
            garage = Panel(canvas.transform, "GarageScreen", Vector2.zero, Vector2.one);
            garage.GetComponent<UnityEngine.UI.Image>().color = new Color(.075f,.1f,.12f,1f);
            Label(garage.transform,"AGROSIM  /  GALPÃO",32,new Vector2(.05f,.87f),new Vector2(.8f,.96f));
            Label(garage.transform,"Prepare o equipamento e cuide do TR-01 antes de sair.",20,new Vector2(.05f,.79f),new Vector2(.95f,.87f));
            var equipment=Panel(garage.transform,"EquipmentBay",new Vector2(.04f,.18f),new Vector2(.51f,.76f));
            Label(equipment.transform,"01  EQUIPAMENTOS",24,new Vector2(.06f,.87f),new Vector2(.94f,.97f));
            tractorPreview=Panel(equipment.transform,"TractorPreview",new Vector2(.08f,.69f),new Vector2(.31f,.81f));
            tractorPreview.GetComponent<UnityEngine.UI.Image>().color=new Color(.85f,.67f,.13f);
            Label(tractorPreview.transform,"TR-01",20,Vector2.zero,Vector2.one).alignment=TextAnchor.MiddleCenter;
            implementPreview=Panel(equipment.transform,"ImplementPreview",new Vector2(.36f,.69f),new Vector2(.64f,.81f));
            implementPreview.GetComponent<UnityEngine.UI.Image>().color=new Color(.63f,.24f,.2f);
            Label(implementPreview.transform,"GRADE",18,Vector2.zero,Vector2.one).alignment=TextAnchor.MiddleCenter;
            equipmentDetails = Label(equipment.transform, "", 20, new Vector2(.06f,.22f), new Vector2(.94f,.46f));
            equipmentDetails.resizeTextForBestFit = true;
            equipmentDetails.resizeTextMinSize = 15;
            equipmentDetails.resizeTextMaxSize = 19;
            tractorLabel = Button(equipment.transform, "Selecionar TR-01", .48f, () => { preparation.ToggleTractor(); UpdateSelection(); });
            implementLabel = Button(equipment.transform, "Acoplar grade aradora", .03f, () => { preparation.ToggleImplement(); UpdateSelection(); });
            implementButton = implementLabel.transform.parent.GetComponent<UnityEngine.UI.Button>();
            var maintenance=Panel(garage.transform,"MaintenanceBay",new Vector2(.54f,.18f),new Vector2(.96f,.76f));
            Label(maintenance.transform,"02  MANUTENÇÃO",24,new Vector2(.06f,.87f),new Vector2(.94f,.97f));
            maintenanceDetails=Label(maintenance.transform,"",22,new Vector2(.06f,.51f),new Vector2(.94f,.82f));
            maintenanceDetails.resizeTextForBestFit=true; maintenanceDetails.resizeTextMinSize=16; maintenanceDetails.resizeTextMaxSize=22;
            var refuelLabel=Button(maintenance.transform,"Abastecer até 100 L",.29f,Refuel);
            refuelActionLabel=refuelLabel;
            refuelButton=refuelLabel.transform.parent.GetComponent<UnityEngine.UI.Button>();
            var repairLabel=Button(maintenance.transform,"Reparar desgaste",.07f,Repair);
            repairActionLabel=repairLabel;
            repairButton=repairLabel.transform.parent.GetComponent<UnityEngine.UI.Button>();
            var footer=Panel(garage.transform,"GarageFooter",new Vector2(.04f,.025f),new Vector2(.96f,.15f));
            garageMessage=Label(footer.transform,"",19,new Vector2(.025f,.1f),new Vector2(.6f,.9f));
            var startLabel = Button(footer.transform, "Sair para o talhão", .13f, StartOperation);
            Rect(startLabel.transform.parent.GetComponent<RectTransform>(),new Vector2(.63f,.15f),new Vector2(.97f,.85f));
            startButton = startLabel.transform.parent.GetComponent<UnityEngine.UI.Button>();
            eventPanel = Panel(canvas.transform, "AgriculturalEvent", new Vector2(.32f,.24f), new Vector2(.78f,.67f));
            Label(eventPanel.transform, "SOLO COMPACTADO\nA resistência aumentou. Como proceder?", 23, new Vector2(.06f,.59f), new Vector2(.94f,.94f));
            Button(eventPanel.transform, "Ajustar velocidade · +3 L", .34f, () => Decide(true));
            Button(eventPanel.transform, "Continuar · -15% produtividade", .11f, () => Decide(false));
            eventPanel.SetActive(false);
            resultPanel = Panel(canvas.transform, "Result", new Vector2(.3f,.2f), new Vector2(.8f,.7f));
            result = Label(resultPanel.transform, "", 23, new Vector2(.07f,.35f), new Vector2(.93f,.94f));
            result.resizeTextForBestFit = true;
            result.resizeTextMinSize = 14;
            result.resizeTextMaxSize = 23;
            Button(resultPanel.transform, "Voltar ao galpão", .1f, BeginReturn);
            resultPanel.SetActive(false);
            saveMessage=Label(garage.transform,"",15,new Vector2(.05f,.755f),new Vector2(.76f,.79f));
            saveDate=Label(garage.transform,"",18,new Vector2(.05f,.835f),new Vector2(.76f,.875f));
            var saveTools=Panel(garage.transform,"SaveTools",new Vector2(.79f,.8f),new Vector2(.96f,.96f));
            var newGameLabel=Button(saveTools.transform,"Novo jogo",.55f,()=>newGamePanel.SetActive(true));
            Rect(newGameLabel.transform.parent.GetComponent<RectTransform>(),new Vector2(.06f,.52f),new Vector2(.94f,.94f));
            newGameLabel.fontSize=18;
            var retryLabel=Button(saveTools.transform,"Tentar salvar",.15f,RetrySave);
            Rect(retryLabel.transform.parent.GetComponent<RectTransform>(),new Vector2(.06f,.06f),new Vector2(.94f,.48f));
            retryLabel.fontSize=18;
            newGamePanel=Panel(canvas.transform,"NewGameConfirmation",Vector2.zero,Vector2.one);
            var confirmation=Panel(newGamePanel.transform,"ConfirmationDialog",new Vector2(.27f,.29f),new Vector2(.73f,.71f));
            Label(confirmation.transform,"Iniciar um novo jogo?\nO progresso salvo será reiniciado.\nOs registros de operações serão mantidos.",22,new Vector2(.06f,.58f),new Vector2(.94f,.95f));
            Button(confirmation.transform,"Cancelar",.32f,()=>newGamePanel.SetActive(false));
            Button(confirmation.transform,"Confirmar novo jogo",.09f,NewGame);
            newGamePanel.SetActive(false);
            saves=new SaveGameStore(System.IO.Path.Combine(Application.persistentDataPath,"AgroSim"));
            recovering=true;
            saveFailed=!saves.RecoverAttempt();
            if(!saveFailed && saves.NeedsInitialSave) saveFailed=!saves.EnsureSaved();
            operation.RestoreMachine(saves.Fuel,saves.Wear);
            ReturnHome();
            recovering=false;
        }

        private void StartOperation()
        {
            if (saveFailed || !preparation.CanStart || !operation.CanOperate || manager.CurrentState != GameState.Garage) return;
            string id=saves.BeginAttempt();
            if(id==null) { saveFailed=true; UpdateSelection(); return; }
            serviceMessage=null;
            startingFuel=operation.Fuel;
            operation.Begin();
            route=new[]{home,new Vector3(-2.7f,-1f,0f),new Vector3(-2.7f,-2f,0f),new Vector3(0f,-2f,0f)}; routeIndex=1;
            log.Begin(id,saves.RunId,saves.CheckpointId,saves.DirectoryPath); sampleTimer = 0f; Record("started");
            manager.ChangeState(GameState.Traveling);
            garage.SetActive(false);
        }

        private void Update()
        {
            if (manager.CurrentState == GameState.Traveling)
            {
                if(FollowRoute()) manager.ChangeState(GameState.Operating);
            }
            else if (manager.CurrentState == GameState.Operating)
            {
                operation.Tick(Time.deltaTime);
                float p = operation.Progress * 4f;
                int lane = Mathf.Min(3, Mathf.FloorToInt(p));
                float x = Mathf.Lerp(0f,6f,p-lane);
                if (lane % 2 == 1) x = 6f-x;
                MoveTractor(new Vector3(x,-2f+lane*1.05f,0f));
                eventPanel.SetActive(operation.EventPending);
                sampleTimer += Time.deltaTime;
                if (sampleTimer >= 1f) { sampleTimer = 0f; Record("telemetry"); }
                if (!operation.Running)
                {
                    manager.ChangeState(GameState.Result);
                    Record(operation.Progress>=1f ? "completed" : "interrupted");
                    if(operation.Progress>=1f) saveFailed=!saves.CompleteAttempt(log.OperationId);
                    result.text = $"{(operation.Progress>=1f ? "OPERAÇÃO CONCLUÍDA" : "OPERAÇÃO INTERROMPIDA")}\n\nTalhão 01 · preparo do solo\nCombustível utilizado: {startingFuel-operation.Fuel:F1} L\nDesgaste acumulado: {operation.Wear:F1}%\nProdutividade: {operation.Productivity:F0}%\n\n{((saveFailed ? saves.LastError : null) ?? log.LastError ?? "Telemetria e resultado salvos neste PC.")}\nRetorne ao galpão para manutenção.";
                    Debug.Log("AgroSim operation log: " + log.FilePath);
                    resultPanel.SetActive(true);
                }
            }
            else if (manager.CurrentState == GameState.Returning)
            {
                if(FollowRoute()) ReturnHome();
            }
            UpdateStatus();
        }

        private void Decide(bool adjust)
        {
            operation.ResolveEvent(adjust); eventPanel.SetActive(false);
            Record("decision", adjust ? "adjust_speed" : "continue");
        }
        private void Record(string kind, string decision = null)
        { log.Write(kind, operation.Progress, operation.Fuel, operation.Wear, operation.Productivity, decision); }
        private void BeginReturn()
        {
            resultPanel.SetActive(false);
            var from=tractor.position;
            route=new[]{from,new Vector3(-.6f,from.y,0f),new Vector3(-2.7f,from.y,0f),new Vector3(-2.7f,-1f,0f),home}; routeIndex=1;
            manager.ChangeState(GameState.Returning);
        }
        private void ReturnHome()
        {
            if(!recovering && saves.PendingAttemptId!=null)
            {
                saveFailed=!saves.ConfirmAttempt(log.OperationId,operation.Fuel,operation.Wear);
                if(!saveFailed) Record("confirmed");
            }
            manager.ChangeState(GameState.Garage);
            tractor.position = home;
            tractor.rotation=Quaternion.identity;
            garage.SetActive(true); resultPanel.SetActive(false); eventPanel.SetActive(false);
            preparation.Reset();
            operation.Reset(); UpdateSelection();
        }
        private void UpdateSelection()
        {
            tractorLabel.text = preparation.TractorSelected ? "Desmarcar TR-01" : "Selecionar TR-01";
            implementLabel.text = preparation.ImplementSelected ? "Desacoplar grade aradora" : "Acoplar grade aradora";
            implementButton.interactable = preparation.TractorSelected;
            if (implement != null) implement.gameObject.SetActive(preparation.ImplementSelected);
            startButton.interactable = !saveFailed && preparation.CanStart && operation.CanOperate;
            long refuelCost=GarageEconomy.RefuelCost(operation.Fuel), repairCost=GarageEconomy.RepairCost(operation.Wear);
            refuelButton.interactable=!saveFailed && preparation.TractorSelected && operation.Fuel<99.99f && saves.BalanceCents>=refuelCost;
            repairButton.interactable=!saveFailed && preparation.TractorSelected && operation.Wear>.001f && saves.BalanceCents>=repairCost;
            refuelActionLabel.text="Abastecer · "+GarageEconomy.Money(refuelCost);
            repairActionLabel.text="Reparar · "+GarageEconomy.Money(repairCost);
            bool insufficient=(operation.Fuel<99.99f && saves.BalanceCents<refuelCost) || (operation.Wear>.001f && saves.BalanceCents<repairCost);
            saveMessage.text=saveFailed ? (saves.LastError ?? "Save pendente. Use Tentar salvar antes de sair.") : (insufficient ? "Saldo insuficiente para os serviços bloqueados." : serviceMessage ?? saves.LoadMessage ?? "Progresso do galpão salvo neste PC.");
            saveDate.text="Último save: "+saves.LastSaveDisplay;
            tractorPreview.GetComponent<UnityEngine.UI.Image>().color=preparation.TractorSelected ? new Color(.95f,.75f,.15f) : new Color(.4f,.38f,.25f);
            implementPreview.SetActive(preparation.ImplementSelected);
            maintenanceDetails.text=$"Saldo: {GarageEconomy.Money(saves.BalanceCents)}\nCombustível: {operation.Fuel:F1} / 100 L\nDesgaste: {operation.Wear:F1}%\nPreços: {GarageEconomy.Money(GarageEconomy.FuelPriceCents)}/L\nReparo: {GarageEconomy.Money(GarageEconomy.WearPointPriceCents)}/ponto de desgaste";
            garageMessage.text=!preparation.TractorSelected ? "Selecione o TR-01 para preparar ou fazer manutenção." : !operation.CanOperate ? "Abasteça ou repare antes de sair. Reserva mínima: 22,5 L." : !preparation.ImplementSelected ? "Acople a grade aradora para liberar a saída." : "Equipamento pronto · preparo do solo no talhão 01.";
            equipmentDetails.text = preparation.TractorSelected
                ? $"{(tractorData != null ? tractorData.displayName : "Trator TR-01")}\nTanque: {(tractorData != null ? tractorData.fuelCapacityLiters : 100f):F0} L · disponível: {operation.Fuel:F0} L\nDesgaste: {operation.Wear:F1}%\nGrade aradora · preparo do solo\nDados ilustrativos do protótipo"
                : "Selecione o trator para ver os dados.\nDepois, acople a grade aradora.\nO início requer os dois equipamentos.";
            UpdateStatus();
        }
        private void Refuel()
        {
            if(manager.CurrentState!=GameState.Garage || !preparation.TractorSelected) return;
            if(saveFailed) return;
            PurchaseService(true);
        }
        private void Repair()
        {
            if(manager.CurrentState!=GameState.Garage || !preparation.TractorSelected) return;
            if(saveFailed) return;
            PurchaseService(false);
        }
        private void PurchaseService(bool refuel)
        {
            long cost=refuel ? saves.RefuelCostCents : saves.RepairCostCents;
            string id=System.Guid.NewGuid().ToString("N");
            bool bought=refuel ? saves.BuyRefuel(id) : saves.BuyRepair(id);
            if(bought)
            {
                operation.RestoreMachine(saves.Fuel,saves.Wear);
                serviceMessage=(refuel ? "Abastecimento" : "Reparo")+" confirmado · "+GarageEconomy.Money(cost)+".";
            }
            else { serviceMessage=saves.LastError; saveFailed=saves.BalanceCents>=cost; }
            UpdateSelection();
        }
        private void RetrySave()
        {
            if(manager.CurrentState!=GameState.Garage) return;
            if(saves.PendingAttemptId!=null && log.OperationId==saves.PendingAttemptId)
            {
                saveFailed=!saves.ConfirmAttempt(log.OperationId,operation.Fuel,operation.Wear);
                if(!saveFailed) Record("confirmed");
            }
            else
            {
                saveFailed=!saves.RecoverAttempt();
                if(!saveFailed) saveFailed=!saves.SaveCheckpoint(operation.Fuel,operation.Wear);
            }
            if(!saveFailed) serviceMessage=null;
            UpdateSelection();
        }
        private void NewGame()
        {
            if(manager.CurrentState!=GameState.Garage) return;
            saveFailed=!saves.NewGame();
            if(!saveFailed) { serviceMessage=null; operation.RestoreMachine(saves.Fuel,saves.Wear); newGamePanel.SetActive(false); ReturnHome(); }
            UpdateSelection();
        }
        private bool FollowRoute()
        {
            if(routeIndex>=route.Length) return true;
            var next=Vector3.MoveTowards(tractor.position,route[routeIndex],2f*Time.deltaTime);
            MoveTractor(next);
            if(Vector3.Distance(tractor.position,route[routeIndex])<.001f) routeIndex++;
            return routeIndex>=route.Length;
        }
        private void MoveTractor(Vector3 next)
        {
            var direction=next-tractor.position;
            if(direction.sqrMagnitude>.000001f) tractor.rotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg);
            tractor.position=next;
        }
        private void UpdateStatus()
        {
            if (manager.CurrentState == GameState.Garage)
                status.text = $"TR-01: {(preparation.TractorSelected ? "selecionado" : "aguardando seleção")}   |   Grade: {(preparation.ImplementSelected ? "acoplada" : "aguardando seleção")}   |   {(preparation.CanStart ? "Pronto para iniciar" : "Preparação incompleta")}";
            else
            {
                string phase = manager.CurrentState == GameState.Traveling ? "Indo ao talhão" : manager.CurrentState == GameState.Returning ? "Voltando ao galpão" : manager.CurrentState == GameState.Result ? "Concluído" : operation.EventPending ? "Aguardando decisão" : "Preparando o solo";
                status.text = $"{phase}   |   {operation.Progress:P0}   |   Combustível {operation.Fuel:F1} L   |   Desgaste {operation.Wear:F1}%   |   Produtividade {operation.Productivity:F0}%";
            }
        }
        private GameObject Panel(Transform parent, string name, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            go.transform.SetParent(parent,false);
            Rect(go.GetComponent<RectTransform>(),min,max);
            go.GetComponent<UnityEngine.UI.Image>().color = new Color(.055f,.09f,.075f,.96f);
            return go;
        }
        private UnityEngine.UI.Text Label(Transform parent,string text,int size,Vector2 min,Vector2 max)
        {
            var go = new GameObject("Label",typeof(RectTransform),typeof(UnityEngine.UI.Text));
            go.transform.SetParent(parent,false); Rect(go.GetComponent<RectTransform>(),min,max);
            var label = go.GetComponent<UnityEngine.UI.Text>();
            label.font = font; label.text = text; label.fontSize = size; label.color = Color.white;
            label.raycastTarget = false; label.alignment = TextAnchor.MiddleLeft;
            return label;
        }
        private UnityEngine.UI.Text Button(Transform parent,string text,float y,UnityEngine.Events.UnityAction action)
        {
            var go = Panel(parent,"Button",new Vector2(.06f,y),new Vector2(.94f,y+.17f));
            go.GetComponent<UnityEngine.UI.Image>().color = new Color(.25f,.43f,.22f);
            go.AddComponent<UnityEngine.UI.Button>().onClick.AddListener(action);
            var label = Label(go.transform,text,20,new Vector2(.04f,0),new Vector2(.96f,1));
            label.alignment = TextAnchor.MiddleCenter;
            return label;
        }
        private static void Rect(RectTransform rect,Vector2 min,Vector2 max)
        { rect.anchorMin=min; rect.anchorMax=max; rect.offsetMin=rect.offsetMax=Vector2.zero; }
    }
}

