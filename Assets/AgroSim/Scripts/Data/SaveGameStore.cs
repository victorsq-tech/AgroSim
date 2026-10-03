using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace AgroSim.Data
{
    // Machine state changes only at a garage checkpoint. Attempt metadata is journaled separately.
    public sealed class SaveGameStore
    {
        [Serializable] private sealed class Save
        {
            public int version;
            public string runId, checkpointId, savedAtUtc;
            public float fuelLiters, wearPercent;
            public string[] confirmedIds;
            public Attempt pending;
            public long balanceCents;
            public Purchase lastPurchase;
            public string[] confirmedPurchaseIds;
        }
        [Serializable] private sealed class Purchase
        { public string id,kind,timestampUtc; public long costCents; }
        [Serializable] private sealed class Attempt
        { public string id, checkpointId, status; }
        private Save current;
        private bool blocked;
        private readonly int fuelPriceCents, wearPointPriceCents;
        public string DirectoryPath { get; }
        public string FilePath => Path.Combine(DirectoryPath,"save.json");
        public string LastError { get; private set; }
        public string LoadMessage { get; private set; }
        public bool NeedsInitialSave { get; private set; } = true;
        public float Fuel => current.fuelLiters;
        public float Wear => current.wearPercent;
        public string RunId => current.runId;
        public string CheckpointId => current.checkpointId;
        public long BalanceCents => current.balanceCents;
        public long RefuelCostCents => GarageEconomy.CalculateCost(100f-Fuel,fuelPriceCents);
        public long RepairCostCents => GarageEconomy.CalculateCost(Wear,wearPointPriceCents);
        public string SavedAtUtc => current.savedAtUtc;
        public string LastSaveDisplay => DateTimeOffset.TryParse(current.savedAtUtc,System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None,out var saved)
            ? saved.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss") : "Data indisponível (save antigo)";
        public string PendingAttemptId => current.pending?.id;
        public string PendingAttemptStatus => current.pending?.status;
        public string PendingCheckpointId => current.pending?.checkpointId;
        public SaveGameStore(string directory) : this(directory,GarageEconomy.FuelPriceCents,GarageEconomy.WearPointPriceCents) {}
        public SaveGameStore(string directory,int fuelPriceCents,int wearPointPriceCents)
        {
            if(fuelPriceCents<0 || wearPointPriceCents<0) throw new ArgumentOutOfRangeException(nameof(fuelPriceCents));
            this.fuelPriceCents=fuelPriceCents; this.wearPointPriceCents=wearPointPriceCents;
            DirectoryPath=directory; current=Fresh();
            if(!File.Exists(FilePath)) return;
            try
            {
                string json=File.ReadAllText(FilePath);
                var loaded=JsonUtility.FromJson<Save>(json);
                if(loaded?.pending!=null && string.IsNullOrEmpty(loaded.pending.id) && string.IsNullOrEmpty(loaded.pending.checkpointId) && string.IsNullOrEmpty(loaded.pending.status)) loaded.pending=null;
                if(loaded?.lastPurchase!=null && string.IsNullOrEmpty(loaded.lastPurchase.id) && string.IsNullOrEmpty(loaded.lastPurchase.kind) && string.IsNullOrEmpty(loaded.lastPurchase.timestampUtc) && loaded.lastPurchase.costCents==0) loaded.lastPurchase=null;
                bool migrated=loaded!=null && loaded.version==1;
                if(migrated) { loaded.version=2; loaded.balanceCents=GarageEconomy.InitialBalanceCents; loaded.lastPurchase=null; loaded.confirmedPurchaseIds=Array.Empty<string>(); }
                if(loaded!=null && !migrated && !json.Contains("\"balanceCents\"")) throw new ArgumentException("Missing balance.");
                if(!json.Contains("\"fuelLiters\"") || !json.Contains("\"wearPercent\"") || !Valid(loaded))
                    throw new ArgumentException("Invalid or unsupported save.");
                current=loaded;
                NeedsInitialSave=migrated;
                LoadMessage="Último estado do galpão recuperado.";
            }
            catch(Exception error) when(error is IOException || error is UnauthorizedAccessException || error is ArgumentException)
            {
                try
                {
                    File.Copy(FilePath,FilePath+".invalid-"+Guid.NewGuid().ToString("N"));
                    LoadMessage="Save inválido preservado em uma cópia. Iniciado estado seguro: 100 L, desgaste zero.";
                }
                catch(Exception archiveError) when(archiveError is IOException || archiveError is UnauthorizedAccessException)
                { blocked=true; LastError="Não foi possível preservar o save anterior. Saída bloqueada."; }
            }
        }
        private static string Id() => Guid.NewGuid().ToString("N");
        private static bool ValidId(string id) => Guid.TryParseExact(id,"N",out _);
        private static bool Condition(float fuel,float wear) => !float.IsNaN(fuel) && !float.IsInfinity(fuel) && fuel>=0 && fuel<=100 && !float.IsNaN(wear) && !float.IsInfinity(wear) && wear>=0 && wear<=100;
        private static Save Fresh() => new Save {version=2,runId=Id(),checkpointId=Id(),fuelLiters=100,wearPercent=0,confirmedIds=Array.Empty<string>(),confirmedPurchaseIds=Array.Empty<string>(),balanceCents=GarageEconomy.InitialBalanceCents};
        private static bool Valid(Save save)
        {
            if(save==null || save.version!=2 || save.balanceCents<0 || save.balanceCents>GarageEconomy.InitialBalanceCents || !ValidId(save.runId) || !ValidId(save.checkpointId) || !Condition(save.fuelLiters,save.wearPercent) || save.confirmedIds==null) return false;
            if(save.lastPurchase!=null && (!ValidId(save.lastPurchase.id) || (save.lastPurchase.kind!="refuel" && save.lastPurchase.kind!="repair") || save.lastPurchase.costCents<0 || save.lastPurchase.costCents>100000 || !DateTimeOffset.TryParse(save.lastPurchase.timestampUtc,out _))) return false;
            foreach(var id in save.confirmedIds) if(!ValidId(id)) return false;
            if(save.confirmedPurchaseIds==null) return false;
            foreach(var id in save.confirmedPurchaseIds) if(!ValidId(id)) return false;
            return save.pending==null || (ValidId(save.pending.id) && save.pending.checkpointId==save.checkpointId && (save.pending.status=="in_progress" || save.pending.status=="completed") && Array.IndexOf(save.confirmedIds,save.pending.id)<0);
        }
        private Save Copy() => new Save {version=current.version,runId=current.runId,checkpointId=current.checkpointId,savedAtUtc=current.savedAtUtc,fuelLiters=current.fuelLiters,wearPercent=current.wearPercent,balanceCents=current.balanceCents,confirmedPurchaseIds=(string[])current.confirmedPurchaseIds.Clone(),lastPurchase=current.lastPurchase==null ? null : new Purchase {id=current.lastPurchase.id,kind=current.lastPurchase.kind,timestampUtc=current.lastPurchase.timestampUtc,costCents=current.lastPurchase.costCents},confirmedIds=(string[])current.confirmedIds.Clone(),pending=current.pending==null ? null : new Attempt {id=current.pending.id,checkpointId=current.pending.checkpointId,status=current.pending.status}};
        public bool EnsureSaved()
        {
            if(!NeedsInitialSave) return true;
            var next=Copy(); if(string.IsNullOrEmpty(next.savedAtUtc)) next.savedAtUtc=DateTime.UtcNow.ToString("o");
            return Persist(next);
        }
        public bool BuyRefuel(string purchaseId) => Buy(purchaseId,"refuel",RefuelCostCents);
        public bool BuyRepair(string purchaseId) => Buy(purchaseId,"repair",RepairCostCents);
        private bool Buy(string id,string kind,long cost)
        {
            if(Array.IndexOf(current.confirmedPurchaseIds,id)>=0) return true;
            if(!ValidId(id) || current.pending!=null) { LastError="Compra inválida ou operação pendente."; return false; }
            if((kind=="refuel" && Fuel>=99.99f) || (kind=="repair" && Wear<=.001f)) { LastError="Este serviço não é necessário."; return false; }
            if(BalanceCents<cost) { LastError="Saldo insuficiente para este serviço."; return false; }
            var next=Copy(); next.balanceCents-=cost;
            if(kind=="refuel") next.fuelLiters=100; else next.wearPercent=0;
            next.checkpointId=Id(); next.savedAtUtc=DateTime.UtcNow.ToString("o");
            next.lastPurchase=new Purchase {id=id,kind=kind,timestampUtc=next.savedAtUtc,costCents=cost};
            var ids=new string[next.confirmedPurchaseIds.Length+1]; Array.Copy(next.confirmedPurchaseIds,ids,next.confirmedPurchaseIds.Length); ids[ids.Length-1]=id; next.confirmedPurchaseIds=ids;
            if(!Persist(next)) return false;
            LocalPurchaseLog.Write(DirectoryPath,id,RunId,CheckpointId,kind,cost,BalanceCents,next.savedAtUtc);
            return true;
        }
        public bool SaveCheckpoint(float fuel,float wear)
        {
            if(current.pending!=null || !Condition(fuel,wear)) { LastError="Estado inválido ou tentativa ainda pendente."; return false; }
            var next=Copy(); next.fuelLiters=fuel; next.wearPercent=wear; next.checkpointId=Id(); next.savedAtUtc=DateTime.UtcNow.ToString("o"); return Persist(next);
        }
        public string BeginAttempt()
        {
            if(current.pending!=null) { LastError="Confirme a operação anterior antes de sair."; return null; }
            var next=Copy(); next.pending=new Attempt {id=Id(),checkpointId=next.checkpointId,status="in_progress"};
            return Persist(next) ? next.pending.id : null;
        }
        public bool CompleteAttempt(string id)
        {
            if(current.pending?.id!=id) return false;
            var next=Copy(); next.pending.status="completed"; return Persist(next);
        }
        public bool ConfirmAttempt(string id,float fuel,float wear)
        {
            if(Array.IndexOf(current.confirmedIds,id)>=0) return true;
            if(current.pending?.id!=id || !Condition(fuel,wear)) { LastError="Não foi possível confirmar a tentativa."; return false; }
            var next=Copy(); var ids=new string[next.confirmedIds.Length+1];
            Array.Copy(next.confirmedIds,ids,next.confirmedIds.Length); ids[ids.Length-1]=id;
            next.confirmedIds=ids; next.pending=null; next.fuelLiters=fuel; next.wearPercent=wear; next.checkpointId=Id();
            next.savedAtUtc=DateTime.UtcNow.ToString("o");
            return Persist(next);
        }
        public bool RecoverAttempt()
        {
            if(current.pending==null) return !blocked;
            var pending=current.pending;
            string kind=pending.status=="completed" ? "completed_unconfirmed" : "interrupted";
            if(!LocalOperationLog.WriteRecovery(DirectoryPath,pending.id,current.runId,pending.checkpointId,kind))
            { LastError="Não foi possível registrar a interrupção. Tente salvar novamente."; return false; }
            var next=Copy(); next.pending=null;
            if(!Persist(next)) return false;
            LoadMessage="Operação anterior "+(kind=="interrupted" ? "interrompida" : "concluída sem retorno ao galpão")+". Último estado do galpão recuperado.";
            return true;
        }
        public bool NewGame()
        {
            if(!RecoverAttempt()) return false;
            var next=Fresh(); next.savedAtUtc=DateTime.UtcNow.ToString("o");
            if(!Persist(next)) return false;
            LoadMessage="Novo jogo salvo neste PC."; return true;
        }
        private bool Persist(Save next)
        {
            if(blocked || !Valid(next)) return false;
            string temporary=FilePath+".tmp-"+Id();
            try
            {
                Directory.CreateDirectory(DirectoryPath);
                byte[] bytes=Encoding.UTF8.GetBytes(JsonUtility.ToJson(next,true));
                using(var stream=new FileStream(temporary,FileMode.CreateNew,FileAccess.Write,FileShare.None))
                { stream.Write(bytes,0,bytes.Length); stream.Flush(true); }
                if(File.Exists(FilePath)) File.Replace(temporary,FilePath,FilePath+".bak");
                else File.Move(temporary,FilePath);
                current=next; NeedsInitialSave=false; LastError=null; LoadMessage=null; return true;
            }
            catch(Exception error) when(error is IOException || error is UnauthorizedAccessException)
            { LastError="Não foi possível salvar neste PC. Último save preservado; tente novamente."; Debug.LogWarning(LastError+" "+error.Message); return false; }
            finally { try { if(File.Exists(temporary)) File.Delete(temporary); } catch(IOException) {} catch(UnauthorizedAccessException) {} }
        }
    }
}
