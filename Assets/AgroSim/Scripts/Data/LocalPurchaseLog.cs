using System;
using System.IO;
using UnityEngine;

namespace AgroSim.Data
{
    public static class LocalPurchaseLog
    {
        [Serializable] private sealed class Purchase
        { public string purchaseId,runId,checkpointId,kind,timestampUtc; public long costCents,balanceCents; }
        public static void Write(string directory,string id,string run,string checkpoint,string kind,long cost,long balance,string timestamp)
        {
            try
            {
                var purchase=new Purchase {purchaseId=id,runId=run,checkpointId=checkpoint,kind=kind,timestampUtc=timestamp,costCents=cost,balanceCents=balance};
                File.AppendAllText(Path.Combine(directory,"purchases.jsonl"),JsonUtility.ToJson(purchase)+Environment.NewLine);
            }
            catch(Exception error) when(error is IOException || error is UnauthorizedAccessException)
            { Debug.LogWarning("Compra confirmada no save; histórico de compras indisponível: "+error.Message); }
        }
    }
}
