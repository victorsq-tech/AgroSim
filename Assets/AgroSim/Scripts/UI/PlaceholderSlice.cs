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
        private float travel;
        private readonly GaragePreparation preparation = new GaragePreparation();
        private Transform implement;
        private UnityEngine.UI.Button startButton;
        private UnityEngine.UI.Button implementButton;
        private UnityEngine.UI.Text equipmentDetails;
        private UnityEngine.UI.Text tractorLabel, implementLabel;
        private readonly LocalOperationLog log = new LocalOperationLog();
        private float sampleTimer;
        private Vector3 returnStart;
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
            garage = Panel(canvas.transform, "Garage", new Vector2(.025f,.04f), new Vector2(.43f,.75f));
            Label(garage.transform, "GALPÃO · preparar operação", 23, new Vector2(.06f,.88f), new Vector2(.94f,.98f));
            equipmentDetails = Label(garage.transform, "", 19, new Vector2(.06f,.47f), new Vector2(.94f,.68f));
            equipmentDetails.resizeTextForBestFit = true;
            equipmentDetails.resizeTextMinSize = 15;
            equipmentDetails.resizeTextMaxSize = 19;
            tractorLabel = Button(garage.transform, "Selecionar TR-01", .7f, () => { preparation.ToggleTractor(); UpdateSelection(); });
            implementLabel = Button(garage.transform, "Acoplar grade aradora", .27f, () => { preparation.ToggleImplement(); UpdateSelection(); });
            implementButton = implementLabel.transform.parent.GetComponent<UnityEngine.UI.Button>();
            var startLabel = Button(garage.transform, "Iniciar preparo do solo", .06f, StartOperation);
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
            ReturnHome();
        }

        private void StartOperation()
        {
            if (!preparation.CanStart || manager.CurrentState != GameState.Garage) return;
            operation.Begin(); travel = 0f;
            log.Begin(); sampleTimer = 0f; Record("started");
            manager.ChangeState(GameState.Traveling);
            garage.SetActive(false);
        }

        private void Update()
        {
            if (manager.CurrentState == GameState.Traveling)
            {
                travel = Mathf.Min(1f, travel + Time.deltaTime / 4f);
                tractor.position = Vector3.Lerp(home, new Vector3(0f,-2f,0f), travel);
                if (travel >= 1f) manager.ChangeState(GameState.Operating);
            }
            else if (manager.CurrentState == GameState.Operating)
            {
                operation.Tick(Time.deltaTime);
                float p = operation.Progress * 4f;
                int lane = Mathf.Min(3, Mathf.FloorToInt(p));
                float x = Mathf.Lerp(0f,6f,p-lane);
                if (lane % 2 == 1) x = 6f-x;
                tractor.position = new Vector3(x,-2f+lane*1.05f,0f);
                eventPanel.SetActive(operation.EventPending);
                sampleTimer += Time.deltaTime;
                if (sampleTimer >= 1f) { sampleTimer = 0f; Record("telemetry"); }
                if (!operation.Running)
                {
                    manager.ChangeState(GameState.Result);
                    Record("completed");
                    result.text = $"OPERAÇÃO CONCLUÍDA\n\nTalhão 01 · preparo do solo\nCombustível utilizado: {100f-operation.Fuel:F1} L\nDesgaste: {operation.Wear:F1}%\nProdutividade: {operation.Productivity:F0}%\n\n{(log.LastError ?? "Telemetria e resultado salvos neste PC.")}\nValores ilustrativos do protótipo.";
                    Debug.Log("AgroSim operation log: " + log.FilePath);
                    resultPanel.SetActive(true);
                }
            }
            else if (manager.CurrentState == GameState.Returning)
            {
                travel = Mathf.Min(1f, travel + Time.deltaTime / 4f);
                tractor.position = Vector3.Lerp(returnStart, home, travel);
                if (travel >= 1f) ReturnHome();
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
            resultPanel.SetActive(false); returnStart = tractor.position; travel = 0f;
            manager.ChangeState(GameState.Returning);
        }
        private void ReturnHome()
        {
            manager.ChangeState(GameState.Garage);
            tractor.position = home;
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
            startButton.interactable = preparation.CanStart;
            equipmentDetails.text = preparation.TractorSelected
                ? $"{(tractorData != null ? tractorData.displayName : "Trator TR-01")}\nTanque: {(tractorData != null ? tractorData.fuelCapacityLiters : 100f):F0} L · disponível: {operation.Fuel:F0} L\nDesgaste: {operation.Wear:F1}%\nGrade aradora · preparo do solo\nDados ilustrativos do protótipo"
                : "Selecione o trator para ver os dados.\nDepois, acople a grade aradora.\nO início requer os dois equipamentos.";
            UpdateStatus();
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

