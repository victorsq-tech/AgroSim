using UnityEngine;

namespace AgroSim.Operations
{
    public sealed class OperationDefinition
    {
        public readonly string Id, Name, Implement, Before, After, Event;
        public readonly Color Color;
        public OperationDefinition(string id,string name,string implement,string before,string after,string incident,Color color)
        { Id=id; Name=name; Implement=implement; Before=before; After=after; Event=incident; Color=color; }
    }
    // Illustrative sequence, not an agronomic prescription or a real crop calendar.
    public static class OperationCatalog
    {
        public const int Count=5;
        private static readonly OperationDefinition[] Definitions={
            new OperationDefinition("soil_preparation","Preparo do solo","Grade aradora","Solo intacto","Solo revirado e sulcos","SOLO COMPACTADO\nA resistência aumentou. Como proceder?",new Color(.65f,.27f,.2f)),
            new OperationDefinition("planting","Plantio","Semeadora","Solo arado","Linhas de sementes","DISTRIBUIÇÃO IRREGULAR\nAs sementes exigem ajuste. Como proceder?",new Color(.25f,.6f,.85f)),
            new OperationDefinition("fertilization","Fertilização","Distribuidor","Linhas plantadas","Brotos e adubo","DOSAGEM IRREGULAR\nO distribuidor exige ajuste. Como proceder?",new Color(.85f,.6f,.18f)),
            new OperationDefinition("spraying","Pulverização","Pulverizador","Plantas em crescimento","Plantas tratadas","VENTO NO TALHÃO\nA aplicação exige cuidado. Como proceder?",new Color(.25f,.7f,.65f)),
            new OperationDefinition("harvest","Colheita","Colhedora rebocada","Lavoura madura","Palhada e restolho","ACÚMULO NA COLHEDORA\nO equipamento exige ajuste. Como proceder?",new Color(.9f,.72f,.18f))
        };
        public static OperationDefinition Get(int index) => Definitions[Mathf.Clamp(index,0,Count-1)];
    }
}
