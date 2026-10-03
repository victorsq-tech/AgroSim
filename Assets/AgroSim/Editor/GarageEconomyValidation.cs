using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AgroSim.Editor
{
    [InitializeOnLoad]
    public static class GarageEconomyValidation
    {
        static GarageEconomyValidation() { EditorApplication.delayCall+=Validate; }
        [MenuItem("AgroSim/Validar economia do galpão")]
        public static void Validate()
        {
            if(EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall+=Validate; return; }
            string root=Path.Combine(Path.GetTempPath(),"AgroSim-economy-tests-"+Guid.NewGuid().ToString("N"));
            try
            {
                var type=Type.GetType("AgroSim.Data.SaveGameStore, Assembly-CSharp");
                Require(type.GetMethod("BuyRefuel")!=null,"BuyRefuel is missing.");
                var rules=Type.GetType("AgroSim.Data.GarageEconomy, Assembly-CSharp");
                Require((long)rules.GetMethod("RefuelCost").Invoke(null,new object[]{0f})==0 && (long)rules.GetMethod("RepairCost").Invoke(null,new object[]{100f})==0,"Prototype services must be free.");
                Require((long)rules.GetMethod("CalculateCost").Invoke(null,new object[]{.0005f,1000})==1,"Half-cent must round away from zero.");
                Require((long)rules.GetMethod("CalculateCost").Invoke(null,new object[]{.005f,600})==3,"Fractional liters must round once to cents.");
                Func<object> open=()=>Activator.CreateInstance(type,new object[]{root,600,1000});
                Func<object,string,object[],object> call=(target,name,args)=>type.GetMethod(name).Invoke(target,args);
                Func<object,string,object> read=(target,name)=>type.GetProperty(name).GetValue(target);
                var free=Activator.CreateInstance(type,new object[]{Path.Combine(root,"free")});
                call(free,"SaveCheckpoint",new object[]{70f,20f});
                Require((bool)call(free,"BuyRefuel",new object[]{Guid.NewGuid().ToString("N")}) && (bool)call(free,"BuyRepair",new object[]{Guid.NewGuid().ToString("N")}) && (long)read(free,"BalanceCents")==500000,"Free services must preserve balance.");
                var store=open(); Require((long)read(store,"BalanceCents")==500000,"Initial balance must be R$5000.");
                call(store,"SaveCheckpoint",new object[]{77.5f,3.6f});
                Require((long)read(store,"RefuelCostCents")==13500 && (long)read(store,"RepairCostCents")==3600,"Service estimates must match approved prices.");
                string purchase=Guid.NewGuid().ToString("N");
                Require((bool)call(store,"BuyRefuel",new object[]{purchase}),"Refuel purchase must save.");
                Require((long)read(store,"BalanceCents")==486500 && (float)read(store,"Fuel")==100f && (float)read(store,"Wear")==3.6f,"Refuel must debit once and preserve wear.");
                Require((bool)call(store,"BuyRefuel",new object[]{purchase}) && (long)read(store,"BalanceCents")==486500,"Duplicate purchase must not debit twice.");
                Require(!(bool)call(store,"BuyRefuel",new object[]{Guid.NewGuid().ToString("N")}),"Full tank must not charge.");
                Require((bool)call(store,"BuyRepair",new object[]{Guid.NewGuid().ToString("N")}),"Repair must save.");
                Require((long)read(open(),"BalanceCents")==482900 && (float)read(open(),"Wear")==0,"Repair must persist balance and condition.");
                Require((bool)call(store,"BuyRefuel",new object[]{purchase}) && (long)read(store,"BalanceCents")==482900,"Old purchase must remain idempotent after another service.");
                string path=(string)read(store,"FilePath"); string json=File.ReadAllText(path);
                string cheap=System.Text.RegularExpressions.Regex.Replace(json,"\"balanceCents\"\\s*:\\s*[0-9]+","\"balanceCents\": 13500");
                File.WriteAllText(path,cheap); var exact=open(); call(exact,"SaveCheckpoint",new object[]{77.5f,3.6f});
                Require((bool)call(exact,"BuyRefuel",new object[]{Guid.NewGuid().ToString("N")}) && (long)read(exact,"BalanceCents")==0,"Exact balance must be accepted.");
                string date=(string)read(exact,"SavedAtUtc");
                Require(!(bool)call(exact,"BuyRepair",new object[]{Guid.NewGuid().ToString("N")}) && (float)read(exact,"Wear")==3.6f && (string)read(exact,"SavedAtUtc")==date,"Insufficient balance must preserve condition and date.");
                File.WriteAllText(path,json); var failing=open(); call(failing,"SaveCheckpoint",new object[]{80f,4f});
                string before=File.ReadAllText(path); if(File.Exists(path+".bak")) File.Delete(path+".bak"); Directory.CreateDirectory(path+".bak");
                Require(!(bool)call(failing,"BuyRefuel",new object[]{Guid.NewGuid().ToString("N")}),"Write failure must reject purchase.");
                Require(File.ReadAllText(path)==before && (float)read(failing,"Fuel")==80f && (long)read(failing,"BalanceCents")==482900,"Failed purchase must not apply service or debit.");
                Directory.Delete(path+".bak");
                string legacy=json.Replace("\"version\": 2","\"version\": 1");
                File.WriteAllText(path,legacy); var migrated=open();
                Require((long)read(migrated,"BalanceCents")==500000 && (float)read(migrated,"Fuel")==100f && (string)read(migrated,"SavedAtUtc")== (string)read(store,"SavedAtUtc"),"Migration must preserve machine/date and set initial credit.");
                Require((bool)call(migrated,"EnsureSaved",Array.Empty<object>()),"Migration must persist.");
                call(migrated,"SaveCheckpoint",new object[]{90f,0f}); call(migrated,"BuyRefuel",new object[]{Guid.NewGuid().ToString("N")});
                Require((long)read(open(),"BalanceCents")==494000,"Restart must not credit initial balance again.");
                string operation=(string)call(migrated,"BeginAttempt",Array.Empty<object>());
                call(migrated,"CompleteAttempt",new object[]{operation});
                Require((bool)call(migrated,"ConfirmAttempt",new object[]{operation,75f,2f}) && (long)read(open(),"BalanceCents")==494000,"Operation confirmation must not charge fuel again.");
                call(migrated,"BeginAttempt",Array.Empty<object>());
                var interrupted=open(); call(interrupted,"RecoverAttempt",Array.Empty<object>());
                Require((long)read(open(),"BalanceCents")==494000 && (float)read(open(),"Fuel")==75f,"Interruption must preserve purchased services and balance.");
                Require((bool)call(migrated,"NewGame",Array.Empty<object>()) && (long)read(open(),"BalanceCents")==500000,"New game must reset balance.");
                Debug.Log("AgroSim economy validation PASSED: prices, purchases, exact/insufficient balance, idempotency, persistence, failure and migration.");
            }
            catch(Exception error) { Debug.LogError("AgroSim economy validation FAILED: "+error.GetBaseException().Message); }
            finally { if(Directory.Exists(root)) Directory.Delete(root,true); }
        }
        private static void Require(bool value,string message) { if(!value) throw new InvalidOperationException(message); }
    }
}
